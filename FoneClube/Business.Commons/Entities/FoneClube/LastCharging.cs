using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class LastCharging
    {
        public int? ClientId { get; set; }
        public string CommentFoneclube { get; set; }
        public string CommentBoleto { get; set; }
        public string CommentEmail { get; set; }
        public int Amount { get; set; }
        public int? ChargeType { get; set; }
        public DateTime? DateCharged { get; set; }

        public static int Boleto = 2;
        public static int Card = 1;
    }
}
