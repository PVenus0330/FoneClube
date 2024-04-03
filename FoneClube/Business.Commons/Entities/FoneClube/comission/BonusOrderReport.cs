using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.comission
{
    public class BonusOrderReport
    {
        public Person Recebedor { get; set; }
        public Person Liberador { get; set; }
        public BonusOrder Order { get; set; }
    }
}
