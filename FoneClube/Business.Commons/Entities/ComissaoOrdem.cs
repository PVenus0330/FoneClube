using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities
{
    public class ComissaoOrdem
    {
        public int? Id { get; set; }
        public int? IdRecebedor { get; set; }
        public int? TransactionId { get; set; }
        public int Level { get; set; }
        public int Ammount { get; set; }
        public bool Concedida { get; set; }
        public int? TotalLinhas { get; set; }
    }
}
