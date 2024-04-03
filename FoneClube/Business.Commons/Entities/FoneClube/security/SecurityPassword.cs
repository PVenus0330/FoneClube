using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.security
{
    public class SecurityPassword
    {
        public string Password { get; set; }
        public string SaltKey { get; set; }
    }
}
