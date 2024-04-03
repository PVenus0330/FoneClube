using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit
{
	public class DebitResponse : CieloResponse
	{

		public DebitResponse(): base()
		{
		}

        [JsonProperty("Payment")]
        public Domain.Payment Payment
        {
            get;
            set;
        }

        public string PaymentUrl { get; set; }
    }
}