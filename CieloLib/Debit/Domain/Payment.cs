using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit.Domain
{
	public class Payment
	{
		[JsonProperty("Amount")]
		public int Amount
		{
			get;
			set;
		}

		[JsonProperty("Authenticate")]
		public bool Authenticate { get; set; } = true;

		[JsonProperty("AuthenticationUrl")]
		public string AuthenticationUrl
		{
			get;
			set;
		}

		[JsonProperty("Country")]
		public string Country
		{
			get;
			set;
		}

		[JsonProperty("Currency")]
		public string Currency
		{
			get;
			set;
		}

		[JsonProperty("CurrencyIsoCode")]
		public int CurrencyIsoCode
		{
			get;
			set;
		}

		[JsonProperty("DebitCard")]
		public CieloLib.Debit.Domain.DebitCard DebitCard
		{
			get;
			set;
		}

		[JsonProperty("ExtraDataCollection")]
		public object[] ExtraDataCollection
		{
			get;
			set;
		}

		[JsonProperty("Installments")]
		public long Installments
		{
			get;
			set;
		}

        [JsonProperty("SoftDescriptor")]
        public string SoftDescriptor
        {
            get;
            set;
        }

        [JsonProperty("Links")]
		public CieloLib.Domain.Link[] Links
		{
			get;
			set;
		}

		[JsonProperty("PaymentId")]
		public Guid PaymentId
		{
			get;
			set;
		}

		//[JsonProperty("ResponseCode")]
		//public CieloLib.Debit.Domain.ResponseCode ResponseCode
		//{
		//	get;
		//	set;
		//}

		[JsonProperty("ReturnCode")]
		public string ReturnCode
		{
			get;
			set;
		}

		[JsonProperty("ReturnMessage")]
		public string ReturnMessage
		{
			get;
			set;
		}

		[JsonProperty("ReturnUrl")]
		public string ReturnUrl
		{
			get;
			set;
		}

		[JsonProperty("Status")]
		public long Status
		{
			get;
			set;
		}

		[JsonProperty(PropertyName="amt_tax")]
		public decimal TaxAmount
		{
			get;
			set;
		}

		[JsonProperty("Tid")]
		public string Tid
		{
			get;
			set;
		}

		[JsonProperty("Type")]
		public string Type
		{
			get;
			set;
		}

		public Payment()
		{
			this.DebitCard = new CieloLib.Debit.Domain.DebitCard();
		}
	}
}