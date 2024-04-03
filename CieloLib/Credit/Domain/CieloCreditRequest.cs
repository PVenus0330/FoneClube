using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit.Domain
{
	public class CieloCreditRequest: CieloBaseRequest
	{
		[JsonProperty("Customer")]
		public Customer Customer
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

		[JsonProperty("Payment")]
		public CieloLib.Credit.Domain.Payment Payment
		{
			get;
			set;
		}

		public CieloCreditRequest()
		{
			this.Customer = new Domain.Customer();
			this.Payment = new CieloLib.Credit.Domain.Payment();
		}
	}
}