using System;

namespace Helsenorge.Registries
{
    /// <summary>
    /// Information required when communicating with the address registry
    /// </summary>
    public class AddressRegistrySettings
    {
        /// <summary>
        /// SOAP configuration
        /// </summary>
        public SoapConfiguration SoapConfiguration { get; set; }
        /// <summary>
        /// The amount of time values should be cached
        /// </summary>
        public TimeSpan CachingInterval { get; set; }
    }
}
