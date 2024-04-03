using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class CustomerComission : Person
    {
        public bool SemPai { get; set; }
        public bool InconsistenciaPaternidade { get; set; }
        public bool Listado { get; set; }
    }
}
