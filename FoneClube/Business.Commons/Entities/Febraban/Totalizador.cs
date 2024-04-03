using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Totalizador
    {
        public string ControleSequencial { get; set; }
        public string CodigoCliente { get; set; }
        public string ContaUnicaID { get; set; }
        public string Vencimento { get; set; }
        public string Emissao { get; set; }
        public string QuantidadeRegistros { get; set; }
        public string QuantidadeLinhas { get; set; }
        public string SinalTotal { get; set; }
        public string ValorTotal { get; set; }
        public string Filler { get; set; }
    }
}
