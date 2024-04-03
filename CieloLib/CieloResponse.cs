using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib
{
	public class CieloResponse
	{
		[JsonProperty("Customer")]
		public Domain.Customer Customer
		{
			get;
			set;
		}

		[JsonProperty("MerchantOrderId")]
		public string MerchantOrderId
		{
			get;
			set;
		}

		public CieloResponse()
		{
		}

        public string Message { get; set; }

        public string Code { get; set; }
    }
}