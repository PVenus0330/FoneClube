using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit
{
	public class DebitRequest : CieloRequest
	{
		public DebitRequest(): base()
		{
            this.Payment = new Domain.Payment();
        }

        [JsonProperty("Payment")]
        public Domain.Payment Payment
        {
            get;
            set;
        }
    }
}