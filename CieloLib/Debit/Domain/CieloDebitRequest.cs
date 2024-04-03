using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit.Domain
{
	public class CieloDebitRequest
	{
		[JsonProperty("Customer")]
		public CieloLib.Domain.Customer Customer
		{
			get;
			set;
		}

		[JsonProperty("MerchantId")]
		public Guid MerchantId
		{
			get;
			set;
		}

		[JsonProperty("MerchantKey")]
		public string MerchantKey
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

		public CieloDebitRequest()
		{
			this.Customer = new CieloLib.Domain.Customer();
			this.Payment = new CieloLib.Debit.Domain.Payment();
		}
	}
}