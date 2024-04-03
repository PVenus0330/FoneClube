using FoneClube.Business.Commons.Entities.FoneClube.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.chat2desk.message
{
    public class WhatsappGetMessage
    {
        public List<WhatsappMessageData> data { get; set; }
        public WhatsappListMeta Meta { get; set; }
    }

    public class WhatsappMessageData
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public object Coordinates { get; set; }
        public string Transport { get; set; }
        public string Type { get; set; }
        public int Read { get; set; }
        public DateTime Created { get; set; }
        public string Pdf { get; set; }
        public object Remote_Id { get; set; }
        public object Recipient_Status { get; set; }
        public object Ai_Tips { get; set; }
        public List<object> attachments { get; set; }
        public string Photo { get; set; }
        public string Video { get; set; }
        public string Audio { get; set; }
        public int Operator_Id { get; set; }
        public int Channel_Id { get; set; }
        public int Dialog_Id { get; set; }
        public long Client_Id { get; set; }
    }

    public class WebhookMessageResponse : WhatsappMessageData
    {
        public long Message_Id { get; set; }
        public DateTime Event_Time { get; set; }
    }
}
