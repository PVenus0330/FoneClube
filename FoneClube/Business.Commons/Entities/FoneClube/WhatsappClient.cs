using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class WhatsappClient
    {
        public bool IsRegisteredWithChat2Desk { get; set; }
        public string ProfilePicUrl { get; set; }
        public string PhoneNumber { get; set; }
        public int ClientId { get; set; }
        public int UnreadMessages { get; set; }
    }
}
