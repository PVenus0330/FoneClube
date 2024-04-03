using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.message
{
    public class WhatsappBase
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
    }
}
