using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Boleto
{
	public class BoletoRequest: CieloRequest
	{
		public BoletoRequest(): base()
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