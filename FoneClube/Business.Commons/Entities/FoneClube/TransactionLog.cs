using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class TransactionLog
    {
        public string IdTransaction { get; set; }
        public string txtLinkBoleto { get; set; }
        public bool BoletoTransaction { get; set; }
        public bool CartaoTransaction { get; set; }
        public int TipoLog { get; set; }
        public CheckoutPagarMe Checkout { get; set; }
    }
}
