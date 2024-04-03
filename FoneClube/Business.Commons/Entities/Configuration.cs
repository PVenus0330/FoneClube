using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities
{
    public class Configuration
    {
        public string Database { get; set; }
        public string Version { get; set; }
        public string Pagarme { get; set; }
        public string Financeiro { get; set; }
        public string Localhost { get; set; }
        public string QrCode { get; set; }
    }
}
