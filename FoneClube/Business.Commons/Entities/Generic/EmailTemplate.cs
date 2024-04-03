using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Generic
{
    public class EmailTemplate
    {
        public int Id { get; set; } 
        public string Tipo { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public Boolean ShowInAction { get; set; }
    }
}
