using System;
using System.Linq;
using System.Threading.Tasks;
using Helsenorge.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;

namespace Helsenorge.Messaging.Tests.Mocks
{
    internal class MockReceiver : IMessagingReceiver
    {
        private readonly MockFactory _factory;
        private readonly string _id;

        public MockReceiver(MockFactory factory, string id)
        {
            _factory = factory;
            _id = id;
        }

        public bool IsClosed => false;

        public void Close() {}

        public Task CompleteMessageAsync(Message messsage)
        {
            throw new NotImplementedException();
        }

        public Task AbandonMessageAsync(Message messsage)
        {
            throw new NotImplementedException();
        }

        public Task DeadletterMessageAsync(Message messsage)
        {
            throw new NotImplementedException();
        }

        public async Task<IMessagingMessage> ReceiveAsync(TimeSpan serverWaitTime)
        {
            if (_factory.Qeueues.ContainsKey(_id))
            {
                var queue = _factory.Qeueues[_id];
                if (queue.Count > 0)
                {
                    return await Task.FromResult(queue.First());
                }
            }
            //System.Threading.Thread.Sleep(serverWaitTime);
            return null;
        }

        public Task RenewMessageLockAsync(Message messsage)
        {
            throw new NotImplementedException();
        }
    }
}
