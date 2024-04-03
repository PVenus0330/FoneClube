using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class ServiceOrder
    {
        public string AgentName { get; set; }
        public int AgentId { get; set; }
        public bool PendingInteraction { get; set; }
        public string Description { get; set; }
        public int PersonId { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool Pending { get; set; }
        public int ServiceOrderId { get; set; }
    }
}
