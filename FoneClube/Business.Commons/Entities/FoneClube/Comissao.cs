using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoneClube.Business.Commons.Entities.Cielo;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Comissao
    {
        public int ComissionLevel { get; set; }
        public int Ammount { get; set; }
        public List<Person> Filhos { get; set; }
        public int? PaiRecebedor { get; set; }
        public int? TransactionId { get; set; }
        public string TransactionIdValue { get; set; }
        public int TotalLinhas { get; set; }
        public int? LiberadorComissao { get; set; }
        public Transaction.Gateway TipoGateway { get; set; }
    }
}
