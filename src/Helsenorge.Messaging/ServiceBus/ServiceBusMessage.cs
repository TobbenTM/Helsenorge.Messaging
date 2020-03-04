using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Helsenorge.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;

namespace Helsenorge.Messaging.ServiceBus
{
    [ExcludeFromCodeCoverage] // Azure service bus implementation
    internal class ServiceBusMessage : IMessagingMessage
    {
        private readonly Message _implementation;
        private readonly IMessagingReceiver _receiver;

        public ServiceBusMessage(Message implementation, IMessagingReceiver receiver = null)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            _implementation = implementation;
            _receiver = receiver;
        }

        public int FromHerId
        {
            [DebuggerStepThrough] get { return GetValue(ServiceBusCore.FromHerIdHeaderKey, 0); }
            [DebuggerStepThrough] set { SetValue(ServiceBusCore.FromHerIdHeaderKey, value); }
        }
        public int ToHerId
        {
            [DebuggerStepThrough] get { return GetValue(ServiceBusCore.ToHerIdHeaderKey, 0); }
            [DebuggerStepThrough] set { SetValue(ServiceBusCore.ToHerIdHeaderKey, value); }
        }
        public DateTime ApplicationTimestamp
        {
            [DebuggerStepThrough] get { return GetValue(ServiceBusCore.ApplicationTimestampHeaderKey, DateTime.MinValue); }
            [DebuggerStepThrough] set { SetValue(ServiceBusCore.ApplicationTimestampHeaderKey, value); }
        }
        public string CpaId
        {
            [DebuggerStepThrough] get { return GetValue(ServiceBusCore.CpaIdHeaderKey, string.Empty); }
            [DebuggerStepThrough] set { SetValue(ServiceBusCore.CpaIdHeaderKey, value); }
        }
        public object OriginalObject => _implementation;
        public DateTime EnqueuedTimeUtc => _implementation.SystemProperties.EnqueuedTimeUtc;
        public DateTime ExpiresAtUtc => _implementation.ExpiresAtUtc;
        public IDictionary<string, object> Properties => _implementation.UserProperties;
        public long Size => _implementation.Size;
        public string ContentType
        {
            [DebuggerStepThrough] get { return _implementation.ContentType; }
            [DebuggerStepThrough] set { _implementation.ContentType = value; }
        }
        public string CorrelationId
        {
            [DebuggerStepThrough] get { return _implementation.CorrelationId; }
            [DebuggerStepThrough] set { _implementation.CorrelationId = value; }
        }
        public string MessageFunction
        {
            [DebuggerStepThrough] get { return _implementation.Label; }
            [DebuggerStepThrough] set { _implementation.Label = value; }
        }
        public string MessageId
        {
            [DebuggerStepThrough] get { return _implementation.MessageId; }
            [DebuggerStepThrough] set { _implementation.MessageId = value; }
        }
        public string ReplyTo
        {
            [DebuggerStepThrough] get { return _implementation.ReplyTo; }
            [DebuggerStepThrough] set { _implementation.ReplyTo = value; }
        }
        public DateTime ScheduledEnqueueTimeUtc
        {
            [DebuggerStepThrough] get { return _implementation.ScheduledEnqueueTimeUtc; }
            [DebuggerStepThrough] set { _implementation.ScheduledEnqueueTimeUtc = value; }
        }
        public TimeSpan TimeToLive
        {
            [DebuggerStepThrough] get { return _implementation.TimeToLive; }
            [DebuggerStepThrough] set { if (value > TimeSpan.Zero) _implementation.TimeToLive = value; }
        }
        public string To
        {
            [DebuggerStepThrough] get { return _implementation.To; }
            [DebuggerStepThrough] set { _implementation.To = value; }
        }

        public int DeliveryCount
        {
            [DebuggerStepThrough] get { return _implementation.SystemProperties.DeliveryCount; }
        }

        [DebuggerStepThrough]
        public IMessagingMessage Clone(bool includePayload = true)
        {
            if (includePayload) {
                return new ServiceBusMessage(_implementation.Clone(), _receiver);
            }
            else
            {
                var message = new ServiceBusMessage(new Message()
                {
                    ContentType = _implementation.ContentType,
                    CorrelationId = _implementation.CorrelationId,
                    Label = _implementation.Label,
                    MessageId = _implementation.MessageId,
                    PartitionKey = _implementation.PartitionKey,
                    ReplyTo = _implementation.ReplyTo,
                    ReplyToSessionId = _implementation.ReplyToSessionId,
                    ScheduledEnqueueTimeUtc = _implementation.ScheduledEnqueueTimeUtc,
                    SessionId = _implementation.SessionId,
                    TimeToLive = _implementation.TimeToLive,
                    To = _implementation.To,
                    ViaPartitionKey = _implementation.ViaPartitionKey
                }, _receiver);

                foreach (var p in _implementation.UserProperties)
                {
                    message.Properties.Add(p);
                }

                return message;
            }
        }

        [DebuggerStepThrough]
        public void Dispose()
        {
            // noop
        }
        [DebuggerStepThrough]
        public Stream GetBody() => new MemoryStream(_implementation.Body);
        [DebuggerStepThrough]
        public override string ToString() => _implementation.ToString();

        public void AddDetailsToException(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            var data = new Dictionary<string, object>
                {
                    { "BrokeredMessageId", _implementation.MessageId },
                    { "CorrelationId", _implementation.CorrelationId },
                    { "Label", _implementation.Label },
                    { "To", _implementation.To },
                    { "ReplyTo", _implementation.ReplyTo },
                };
            foreach (var key in data.Keys)
            {
                try
                {
                    ex.Data.Add(key, data[key]);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (ArgumentException) // ignore duplicate keys
                {
                }
            }
        }

        public Task CompleteAsync()
        {
            if (_receiver.IsClosed)
            {
                throw new ObjectDisposedException("The correlated MessageReceiver for this message has been disposed.");
            }
            return _receiver.CompleteMessageAsync(_implementation);
        }

        public Task RenewLockAsync()
        {
            if (_receiver.IsClosed)
            {
                throw new ObjectDisposedException("The correlated MessageReceiver for this message has been disposed.");
            }
            return _receiver.RenewMessageLockAsync(_implementation);
        }

        public Task DeadLetterAsync()
        {
            if (_receiver.IsClosed)
            {
                throw new ObjectDisposedException("The correlated MessageReceiver for this message has been disposed.");
            }
            return _receiver.DeadletterMessageAsync(_implementation);
        }

        private void SetValue(string key, string value) => _implementation.UserProperties[key] = value;
        private void SetValue(string key, DateTime value) => _implementation.UserProperties[key] = value.ToString(StringFormatConstants.IsoDateTime, DateTimeFormatInfo.InvariantInfo);
        private void SetValue(string key, int value) => _implementation.UserProperties[key] = value.ToString(CultureInfo.InvariantCulture);

        private string GetValue(string key, string value) => _implementation.UserProperties.ContainsKey(key) ? _implementation.UserProperties[key].ToString() : value;
        private int GetValue(string key, int value) => _implementation.UserProperties.ContainsKey(key) ? int.Parse(_implementation.UserProperties[key].ToString()) : value;
        private DateTime GetValue(string key, DateTime value) => _implementation.UserProperties.ContainsKey(key) ? DateTime.Parse(_implementation.UserProperties[key].ToString(), CultureInfo.InvariantCulture) : value;
    }
}
