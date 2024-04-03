using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit.Domain
{
	public class DebitCard
	{
		[JsonProperty("Brand")]
		public string Brand
		{
			get;
			set;
		}

		[JsonProperty("CardNumber")]
		public string CardNumber
		{
			get;
			set;
		}

		[JsonProperty("ExpirationDate")]
		public string ExpirationDate
		{
			get;
			set;
		}

		[JsonProperty("Holder")]
		public string Holder
		{
			get;
			set;
		}

		[JsonProperty("IsTokenize")]
		public bool IsTokenize
		{
			get;
			set;
		}

		[JsonProperty("SaveCard")]
		public bool SaveCard
		{
			get;
			set;
		}

		[JsonProperty("SecurityCode")]
		public string SecurityCode
		{
			get;
			set;
		}

		public DebitCard()
		{
		}
	}
}