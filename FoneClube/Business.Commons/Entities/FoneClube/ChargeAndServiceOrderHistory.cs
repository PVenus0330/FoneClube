using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class ChargeAndServiceOrderHistory
    {
        public DateTime CreatedDate { get; set; }
        public bool IsCharge { get; set; }
        public bool IsServiceOrder { get; set; }
        public Charging Charges { get; set; }
        public ServiceOrder ServiceOrders { get; set; }
        public int PersonId { get; set; }
    }
}
