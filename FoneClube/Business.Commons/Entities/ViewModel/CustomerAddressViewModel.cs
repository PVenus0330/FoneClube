using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class CustomerAddressViewModel
    {
        public int? idCliente { get; set; }
        public string documento { get; set; }
        public string rua { get; set; }
        public string complemento { get; set; }
        public int? numero { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string cep { get; set; }
    }
}
