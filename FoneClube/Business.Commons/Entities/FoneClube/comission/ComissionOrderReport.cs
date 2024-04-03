using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.comission
{
    public class ComissionOrderReport
    {
        public Person Recebedor { get; set; }
        public Person Liberador { get; set; }
        public ComissionOrder Order { get; set; }
    }
}
