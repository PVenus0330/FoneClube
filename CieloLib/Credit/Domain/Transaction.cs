using System;

namespace CieloLib.Credit.Domain
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public partial class Transaction
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the billplz identifier
        /// </summary>
        ///
        public string Tid { get; set; }
        public Guid PaymentId { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string AuthenticationUrl { get; set; }
        public string QueryUrl { get; set; }
    }
}