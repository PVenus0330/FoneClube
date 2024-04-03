using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit
{
	public class CreditRequest : Domain.CieloCreditRequest
    {
		public CreditRequest(): base()
		{
            this.Payment = new Domain.Payment();
        }

        //[JsonProperty("Payment")]
        //public Domain.Payment Payment
        //{
        //    get;
        //    set;
        //}
    }
}