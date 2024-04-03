using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class WhatsAppStatus
    {
        public string Phone { get; set; }
        public int Ack { get; set; }
        public int ChargeId { get; set; }
        public string AckDateTime { get; set; }
    }
}
