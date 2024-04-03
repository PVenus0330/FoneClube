using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class UserLogin
    {
        public string username;
        public int id { get; set; }
        public string email { get; set; }
        public bool cadastroPendente { get; set; }
        public bool clienteMultinivel { get; set; }
        public bool indicado { get; set; }
    }
}
