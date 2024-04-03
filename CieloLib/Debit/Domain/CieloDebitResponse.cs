using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit.Domain
{
	public class CieloDebitResponse
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
		public CieloLib.Debit.Domain.Payment Payment
		{
			get;
			set;
		}

		public CieloDebitResponse()
		{
		}
	}
}