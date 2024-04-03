using FoneClube.Business.Commons.Entities.FoneClube.chat2desk.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.chat2desk.message
{
    public class ClientChatData
    {
        public List<ClientGroupedMessages> Messages { get; set; }
        public ClientsDataModel C2D_Client { get; set; }
    }
    public class ClientMessages : WhatsappMessageData
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public string SendBy { get; set; }
    }

    public class ClientGroupedMessages
    {
        public DateTime Date { get; set; }
        public string FormattedDate { get; set; }
        public List<ClientMessages> Messages { get; set; }
    }
}