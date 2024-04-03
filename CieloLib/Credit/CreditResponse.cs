using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit
{
	public class CreditResponse : CieloResponse
	{

		public CreditResponse(): base()
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