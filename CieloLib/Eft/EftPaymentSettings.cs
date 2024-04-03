using CieloLib.Eft.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Eft
{
    public class EftPaymentSettings : PaymentSettings
    {

        public EftRequestType PaymentTransactionType
        {
            get;
            set;
        }
        public long Installments { get; set; }

        public EftPaymentSettings()
        {
        }
    }
}
