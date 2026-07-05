using Pulsar.Server.DiscordRPC;
using Pulsar.Server.Forms;
using System;
using System.Windows.Forms;

namespace Pulsar.Server
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            using (FrmMain mainForm = new FrmMain())
            {
                DiscordRPCManager.Initialize(mainForm);

                var customMainForm = Plugins.UIExtensionManager.GetCustomMainForm();
                if (customMainForm != null)
                {
                    mainForm.Hide();
                    customMainForm.Show();
                    
                    customMainForm.FormClosed += (s, e) => 
                    {
                        mainForm.Close();
                    };
                    
                    customMainForm.Disposed += (s, e) => 
                    {
                        if (!mainForm.IsDisposed)
                        {
                            mainForm.Close();
                        }
                    };
                    
                    Application.Run(customMainForm);
                }
                else
                {
                    Application.Run(mainForm);
                }

                DiscordRPCManager.Shutdown();
            }
        }
    }
}