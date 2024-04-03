using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class CieloDebitoTransaction
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public int CustomerId { get; set; }
        public int HistoryId { get; set; }
        public int Valor { get; set; }
    }
}
