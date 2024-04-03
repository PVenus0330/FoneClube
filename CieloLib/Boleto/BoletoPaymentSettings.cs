using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Boleto
{
    public class BoletoPaymentSettings: PaymentSettings
    {
        

        public string PaymentInstruction
        {
            get;
            set;
        }


        public int DaysBeforeLinkExpiration
        {
            get;
            set;
        }

        public BoletoPaymentSettings()
        {
        }
    }
}
