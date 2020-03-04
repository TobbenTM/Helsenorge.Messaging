using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Helsenorge.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Helsenorge.Messaging.ServiceBus
{
    [ExcludeFromCodeCoverage] // Azure service bus implementation
    internal class ServiceBusFactory : IMessagingFactory
    {
        private readonly ServiceBusConnectionStringBuilder _connectionStringBuilder;

        public ServiceBusFactory(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _connectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString);
        }

        public IMessagingReceiver CreateMessageReceiver(string id)
        {
            return new ServiceBusReceiver(new MessageReceiver(_connectionStringBuilder, ReceiveMode.PeekLock));
        }

        public IMessagingSender CreateMessageSender(string id)
        {
            return new ServiceBusSender(new MessageSender(_connectionStringBuilder));
        }

        bool ICachedMessagingEntity.IsClosed => true;

        void ICachedMessagingEntity.Close()
        {
            // noop
        }

        public IMessagingMessage CreateMessage(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return new ServiceBusMessage(new Message(memoryStream.ToArray()));
            }
        }
    }
}
