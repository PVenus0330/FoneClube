using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess.blink
{
    public class BlinkSettings : ISettings
    {
        public bool Enabled { get; set; }

        public string BlinkUserName { get; set; }

        public string BlinkPassword { get; set; }

        public string BlinkApiKey { get; set; }

        public string BlinkDomainId { get; set; }
    }

    public interface ISettings
    {
    }
}
