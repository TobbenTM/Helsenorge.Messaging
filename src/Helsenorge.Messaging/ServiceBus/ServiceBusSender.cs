using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Helsenorge.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Helsenorge.Messaging.ServiceBus
{
    [ExcludeFromCodeCoverage] // Azure service bus implementation
    internal class ServiceBusSender : IMessagingSender
    {
        private readonly MessageSender _implementation;
        public ServiceBusSender(MessageSender implementation)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            _implementation = implementation;
        }
        public async Task SendAsync(IMessagingMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var brokeredMessage = message.OriginalObject as Message;
            if (brokeredMessage == null) throw new InvalidOperationException("OriginalObject is not a Brokered message");

            await _implementation.SendAsync(brokeredMessage).ConfigureAwait(false);
        }
        bool ICachedMessagingEntity.IsClosed => _implementation.IsClosedOrClosing;
        void ICachedMessagingEntity.Close() => _implementation.CloseAsync().GetAwaiter().GetResult();
    }
}
