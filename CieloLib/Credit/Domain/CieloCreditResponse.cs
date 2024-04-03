using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit.Domain
{
	public class CieloCreditResponse
	{
		[JsonProperty("Customer")]
		public CieloLib.Domain.Customer Customer
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

		[JsonProperty("Payment")]
		public CieloLib.Credit.Domain.Payment Payment
		{
			get;
			set;
		}

		public CieloCreditResponse()
		{
		}
	}
}