using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class CieloOrder
    {
        public int CustomerId { get; set; }
        public string OrderId { get; set; }
        public CieloTransactionResult Transaction { get; set; }

        public int ChargingId { get; set; }

    }
}
