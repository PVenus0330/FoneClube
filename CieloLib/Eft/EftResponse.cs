using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Eft
{
	public class EftResponse: CieloResponse
	{

		public EftResponse(): base()
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