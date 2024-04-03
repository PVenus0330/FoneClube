using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib
{
    public class CieloBaseRequest
    {
        [JsonProperty("MerchantOrderId")]
        public string MerchantOrderId
        {
            get;
            set;
        }

        [JsonIgnore]
        public string TxnType { get; set; }
    }

	public class CieloRequest : CieloBaseRequest
    {
		[JsonProperty("Customer")]
		public Domain.Customer Customer
		{
			get;
			set;
		}

        public CieloRequest()
		{
			this.Customer = new Domain.Customer();
		}
	}
}