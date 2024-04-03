using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib
{
    public class PaymentSettings
    {
        public decimal AdditionalFee
        {
            get;
            set;
        }

        public bool AdditionalFeePercentage
        {
            get;
            set;
        }

        public Guid MerchantId
        {
            get;
            set;
        }

        public string PaymentType
        {
            get;
            set;
        }

        public string SecurityKey
        {
            get;
            set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }

        public bool UseSandbox
        {
            get;
            set;
        }

        public string Provider { get; set; }

        public PaymentSettings()
        {
        }
    }
}
