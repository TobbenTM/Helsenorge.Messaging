using System;
using System.Runtime.Serialization;

namespace Helsenorge.Registries.Abstractions
{
    [Serializable]
    public class InvalidChannelBindingException : Exception
    {
        public InvalidChannelBindingException()
        {
        }

        public InvalidChannelBindingException(string message) : base(message)
        {
        }

        public InvalidChannelBindingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidChannelBindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
