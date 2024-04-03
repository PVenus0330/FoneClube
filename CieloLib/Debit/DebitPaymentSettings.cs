using CieloLib.Debit.Domain;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit
{
	public class DebitPaymentSettings : PaymentSettings
	{

		public CieloDebitRequestType PaymentTransactionType
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