using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Boleto
{
	public class BoletoResponse: CieloResponse
	{

		public BoletoResponse(): base()
		{
		}

        [JsonProperty("Payment")]
        public Domain.Payment Payment
        {
            get;
            set;
        }
    }
}