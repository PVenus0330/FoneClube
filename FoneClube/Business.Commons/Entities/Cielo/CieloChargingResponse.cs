using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class CieloChargingResponse
    {
        public bool Charged { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
    }
}
