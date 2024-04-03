using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit.Domain
{
	public class CreditCard
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

        [JsonProperty("CardToken")]
        public Guid CardToken
        {
            get;
            set;
        }

        public CreditCard()
        {
        }
    }
}