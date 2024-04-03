using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Eft
{
	public class EftRequest : CieloRequest
	{
		public EftRequest(): base()
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