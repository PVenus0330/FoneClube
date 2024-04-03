using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel.Comissao
{
    public class DetalhesComissao
    {
        public string totalProjetado { get; set; }
        public List<CustomerComissao> customersComissao { get; set; }
    }
}
