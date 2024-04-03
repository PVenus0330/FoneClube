using CieloLib.Credit.Domain;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit
{
	public class CreditPaymentSettings : PaymentSettings
	{

		public CieloCreditRequestType PaymentTransactionType
		{
			get;
			set;
		}

        public bool AuthenticateTransaction
        {
            get;
            set;
        }
        public int Installments { get; set; }
    }
}