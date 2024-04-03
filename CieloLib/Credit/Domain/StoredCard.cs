using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Credit.Domain
{
    public class StoredCard
    {
        public string CardToken { get; set; }
        public string CardBrand { get; set; }
        public string CreditCardCvv2 { get; set; }
    }
}
