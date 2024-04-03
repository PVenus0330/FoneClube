using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Domain
{
	public class Customer
	{
		public string Name
		{
			get;
			set;
		}

        /// <summary>
        /// New / Existing
        /// </summary>
        public string Status
        {
            get;
            set;
        }

        /// <summary>
        /// CPF
        /// </summary>
		public string Identity
		{
			get;
			set;
		}

        [JsonProperty("Address")]
        public CustomerAddress Address { get; set; }

    }

    public class CustomerAddress
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }/***optional***/
        public string ZipCode { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}