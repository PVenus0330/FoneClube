using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class PersonParent
    {
        public int? DDDParent { get; set; }
        public long? PhoneParent { get; set; }
        public string NameParent { get; set; }
        public int? IdPerson { get; set; }
        public string CPF { get; set; }
    }
}
