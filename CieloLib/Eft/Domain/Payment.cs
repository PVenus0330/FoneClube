using CieloLib.Domain;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Eft.Domain
{
	public class Payment
	{
		[JsonProperty("Amount")]
		public decimal Amount
		{
			get;
			set;
		}

		[JsonProperty("Url")]
		public string Url
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

		[JsonProperty("Links")]
		public Link[] Links
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

        [JsonProperty("Provider")]
        public string Provider { get; set; }

        [JsonProperty("ReceivedDate")]
        public DateTime ReceivedDate { get; set; }

        public Payment()
		{
        }
	}
}