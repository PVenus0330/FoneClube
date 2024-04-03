using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class CieloPaymentModel
    {
        //[StringLength(40)]
        public string OrderId { get; set; }

        //store ID
        public int CustomerId { get; set; }

        //ERP ID
        public int CustomerIdERP { get; set; }

        //[StringLength(40)]
        public string PaymentId { get; set; }

        public int PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        //[StringLength(3)]
        public string Currency { get; set; }

        public DateTime PaymentDate { get; set; }

        //[StringLength(10)]
        public string PaymentGateway { get; set; }

        //encripted card
        public string Card { get; set; }

        public Adress Address { get; set; }
    }
}
