using Pulsar.Server.Forms.DarkMode;
using Pulsar.Server.Utilities;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    public partial class FrmAbout : Form
    {
        private readonly string _repositoryUrl = @"https://github.com/BruceLi20110501/Pulsar_RAT";
        private readonly string _telegramUrl = @"https://t.me/+h7gwb2c5NaM0M2M5";

        // 贡献者信息
        private const string ContributorsMessage = """

- [𝙎𝙐𝙍𝙂𝙀 𝙒𝙄𝙉] – 插件维护与开发
- [Bruce](https://github.com/BruceLi20110501) – 中文汉化与二次开发
- [KingKDot](https://github.com/KingKDot) – 主要开发者
- [TheChosenSkywalker](https://github.com/thechosenskywalker) – 主要开发者
- [MaxXor](https://github.com/MaxXor) – Quasar RAT 原始作者
- [Twobit](https://github.com/officialtwobit) – 多功能模块开发
- [Lucky](https://t.me/V_Lucky_V) – HVNC 专家
- [fedx](https://github.com/fedx-988) – README 设计 & Discord RPC
- [Ace](https://github.com/Knakiri) – HVNC 功能 & WinRE 存活
- [Java](https://github.com/JavaRenamed-dev) – 功能扩展
- [Body](https://body.sh) – 混淆
- [cpores](https://github.com/vahrervert) – VNC 绘制、收藏夹、叠加层
- [Rishie](https://github.com/rishieissocool) – 信息收集选项
- [jungsuxx](https://github.com/jungsuxx) – HVNC 输入 & 代码简化
- [MOOM aka my lebron](https://github.com/moom825) – 灵感来源 & 批量混淆
- [Poli](https://github.com/paulmaster59) – Discord 服务器 & Pulsar 定制加密器
- [Deadman](https://github.com/DeadmanLabs) – 内存转储 & Shellcode 构建器
- [User76](https://github.com/user76-real) – 网络性能优化
""";

        public FrmAbout()
        {
            InitializeComponent();

            DarkModeManager.ApplyDarkMode(this);
            DpiImageScaling.ApplyToIconPictureBox(picIcon, DpiImageScaling.GetScaleFactor(this), 64);
            ScreenCaptureHider.ScreenCaptureHider.Apply(this.Handle);

            lblVersion.Text = ServerVersion.Display;
            lblTitle.Text = ServerVersion.AppName;
            Text = $"{ServerVersion.AppName} - 关于";
            rtxtContent.Text = Properties.Resources.License;
            cntTxtContent.Text = ContributorsMessage;

            lnkGithubPage.Links.Add(new LinkLabel.Link { LinkData = _repositoryUrl });
            lnkTelegram.Links.Add(new LinkLabel.Link { LinkData = _telegramUrl });
            lnkCredits.Links.Add(new LinkLabel.Link
            {
                LinkData = "https://github.com/BruceLi20110501/Pulsar_RAT/tree/main/Licenses"
            });
        }

        private void lnkGithubPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink(lnkGithubPage, e);
        }

        private void lnkCredits_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink(lnkCredits, e);
        }

        private void lnkTelegram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink(lnkTelegram, e);
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static void OpenLink(LinkLabel label, LinkLabelLinkClickedEventArgs e)
        {
            if (label == null)
                return;

            label.LinkVisited = true;

            if (e.Link?.LinkData is string target)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"无法打开链接。\n\n{ex.Message}",
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        // 设置图片从上到下的透明度渐变
        private Image SetOpacityGradient(Image image)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            Bitmap src = new Bitmap(image);

            float startOpacity = 0.15f; // 顶部 15%
            float endOpacity = 0.01f;   // 底部 1%

            for (int y = 0; y < bmp.Height; y++)
            {
                float opacity = startOpacity +
                                (endOpacity - startOpacity) *
                                ((float)y / (bmp.Height - 1));

                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = src.GetPixel(x, y);
                    Color newPixel = Color.FromArgb(
                        (int)(opacity * 255),
                        pixel.R, pixel.G, pixel.B
                    );
                    bmp.SetPixel(x, y, newPixel);
                }
            }

            return bmp;
        }

        private void FrmAbout_Load(object sender, EventArgs e)
        {
            picIcon.Cursor = Cursors.Hand;

            // 自动识别 URL
            cntTxtContent.DetectUrls = true;
            cntTxtContent.ReadOnly = true;

            cntTxtContent.LinkClicked += (s, e2) =>
            {
                try
                {
                    Process.Start(
                        new ProcessStartInfo(e2.LinkText)
                        {
                            UseShellExecute = true
                        }
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"无法打开链接。\n\n{ex.Message}",
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string disclaimer =
                "本软件仅用于学习和研究目的。\n" +
                "未经授权在非本人拥有或未明确允许的计算机上使用属于违法行为。\n\n" +
                "使用本软件即表示你需对自己的行为承担全部责任。";

            MessageBox.Show(
                disclaimer,
                "法律免责声明",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private void picIcon_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/BruceLi20110501/Pulsar_RAT",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开链接：" + ex.Message);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/quasar/Quasar",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开链接：" + ex.Message);
            }
        }

        private void cntTxtContent_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
