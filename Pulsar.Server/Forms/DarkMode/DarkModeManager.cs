using DarkModeForms;
using Pulsar.Server.Models;
using Pulsar.Server.Utilities;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Pulsar.Server.Forms.DarkMode
{
    public class DarkModeManager
    {
        private static readonly DarkModeCS.DisplayMode lightMode = DarkModeCS.DisplayMode.ClearMode;
        private static readonly DarkModeCS.DisplayMode darkMode = DarkModeCS.DisplayMode.DarkMode;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private const int DWMWA_BORDER_COLOR = 34; // DWM attribute for border color

        public static void ApplyDarkMode(Form form)
        {
            bool isDarkModeChecked = Settings.DarkMode;

            DarkModeCS _ = new DarkModeCS(form)
            {
                ColorMode = isDarkModeChecked ? darkMode : lightMode,
                ColorizeIcons = false,
            };

            Color borderColor = isDarkModeChecked ? Color.DimGray : Color.DimGray;
            SetBorderColor(form, borderColor);

            // 统一 DPI 图标适配：覆盖该窗体控件树里的 ToolStrip / 右键菜单 / ListView 的 ImageList。
            // 幂等，可安全对所有窗体调用。
            DpiImageScaling.ApplyToForm(form);
        }

        private static void SetBorderColor(Form form, Color color)
        {
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref colorValue, sizeof(int));
        }
    }
}
