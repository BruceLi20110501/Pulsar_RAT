using System;
using System.Collections.Generic;
using Pulsar.Common.Messages;
using Pulsar.Client.Networking;

namespace Pulsar.Client.Plugins
{
    public interface IClientPlugin
    {
        string Name { get; }
        Version Version { get; }
        void Register(List<IMessageProcessor> processors, PulsarClient client);
    }
}
