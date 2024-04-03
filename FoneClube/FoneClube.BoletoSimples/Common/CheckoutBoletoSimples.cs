using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.BoletoSimples.Common
{
    public class CheckoutBoletoSimples
    {
        public string Street { get; set; }
        public string Neighborhood { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Complement { get; set; }
        public string StreetNumber { get; set; }
        public string Cep { get; set; }


        public string DocumentNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Ammount { get; set; }
        public string ChargingComment { get; set; }
    }
}
