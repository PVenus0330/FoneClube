using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Adress
    {
        //please fill all that is possible

        public string Street { get; set; }
        public string Complement { get; set; }
        public string StreetNumber { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        //zipcode
        public string Cep { get; set; }

        public string Country { get; set; }
    }
}
