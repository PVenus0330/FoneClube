using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.email
{
    public class FullEmail
    {
        public string To { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string BodyHtml { get; set; }
    }
}
