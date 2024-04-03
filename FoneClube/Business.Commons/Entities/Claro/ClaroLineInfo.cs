using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Claro
{
    public class ClaroLineInfo
    {
        public string Line { get; set; }
        public string Subscriber { get; set; }
        public string Profile { get; set; }
        public string Voice { get; set; }
        public string SMS { get; set; }
        public string Data { get; set; }
        public bool Ativa { get; set; }
        public bool Bloqueada { get; set; }
    }
}
