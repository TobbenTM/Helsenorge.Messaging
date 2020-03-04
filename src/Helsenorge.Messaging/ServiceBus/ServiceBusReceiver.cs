using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Helsenorge.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Helsenorge.Messaging.ServiceBus
{
    [ExcludeFromCodeCoverage] // Azure service bus implementation
    internal class ServiceBusReceiver : IMessagingReceiver
    {
        private readonly MessageReceiver _implementation;

        public ServiceBusReceiver(MessageReceiver implementation)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            _implementation = implementation;
        }
        public async Task<IMessagingMessage> ReceiveAsync(TimeSpan serverWaitTime)
        {
            var message = await _implementation.ReceiveAsync(serverWaitTime).ConfigureAwait(false);
            return message != null ? new ServiceBusMessage(message, this) : null;
        }
        bool ICachedMessagingEntity.IsClosed => _implementation.IsClosedOrClosing;
        void ICachedMessagingEntity.Close() => _implementation.CloseAsync().GetAwaiter().GetResult();

        public Task CompleteMessageAsync(Message messsage)
        {
            return _implementation.CompleteAsync(messsage.SystemProperties.LockToken);
        }

        public Task RenewMessageLockAsync(Message messsage)
        {
            return _implementation.RenewLockAsync(messsage.SystemProperties.LockToken);
        }

        public Task AbandonMessageAsync(Message messsage)
        {
            return _implementation.AbandonAsync(messsage.SystemProperties.LockToken);
        }

        public Task DeadletterMessageAsync(Message messsage)
        {
            return _implementation.DeadLetterAsync(messsage.SystemProperties.LockToken);
        }
    }
}
