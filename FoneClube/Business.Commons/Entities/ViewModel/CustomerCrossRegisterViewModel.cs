using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class CustomerCrossRegisterViewModel
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string documento { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string documentType { get; set; }
        public string idPai { get; set; }
    }

    public class PhonesPendingToPortViewModel
    {
        public int userId { get; set; }
        public int numPorts { get; set; }
        public string phones { get; set; }
    }
}
