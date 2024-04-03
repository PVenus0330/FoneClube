using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.message
{
    public class WhatsappSendMessage
    {
        public long ClientId { get; set; }
        public string Text { get; set; }
        public string Attachment { get; set; }// supports only images
        public string Pdf { get; set; }
    }

    /// <summary>
    /// Class to receive the response of send message api
    /// </summary>
    public class SendMessageResponse: WhatsappBase
    {
        public SendMessageResponseDataModel Data { get; set; }        
    }

    public class SendMessageResponseDataModel
    {
        public long Message_Id { get; set; }
        public int Channel_Id { get; set; }
        public int Operator_Id { get; set; }
        public string Transport { get; set; }
        public string Type { get; set; }
        public long Client_Id { get; set; }
        public int Dialog_Id { get; set; }
        public int Request_Id { get; set; }
    }
}
