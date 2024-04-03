using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel.Comissao
{
    public class CustomerComissao
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string telefone { get; set; }
        public string status { get; set; }
        public string bonus { get; set; }
        public bool statusConcedido { get; set; }
        public DateTime dataEntrada { get; set; }
    }
}
