using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.comission
{
    public class Order
    {
        public enum TipoOrdem { Comissao, Bonus }

        public int IdOrder { get; set; }
        public double Amount { get; set; }
        public TipoOrdem tipo { get; set; }
        public bool Baixa { get; set; }
        public bool Reduzido { get; set; }

    }
}
