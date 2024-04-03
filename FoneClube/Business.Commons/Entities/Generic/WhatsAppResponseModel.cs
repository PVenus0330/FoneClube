using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.WhatsApp
{
    public class WhatsAppSessionRequest
    {
        public string webhook { get; set; }
    }

    public class WhatsAppSendButtonUrlResponseModel
    {
        public string status { get; set; }
        public List<string> response { get; set; }
        public string mapper { get; set; }
    }
    public class WhatsAppSendResponseModel
    {
        public string status { get; set; }
        public List<Response> response { get; set; }
        public string mapper { get; set; }
        public string session { get; set; }
    }
    public class Response
    {
        public string id { get; set; }
        public string body { get; set; }
        public string type { get; set; }
        public object subtype { get; set; }
        public int t { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string self { get; set; }
        public int ack { get; set; }
        public bool isNewMsg { get; set; }
        public bool star { get; set; }
        public bool isFromTemplate { get; set; }
        public string title { get; set; }
        public List<object> mentionedJidList { get; set; }
        public string footer { get; set; }
        public object urlText { get; set; }
        public object urlNumber { get; set; }
        public bool isVcardOverMmsDocument { get; set; }
        public bool isForwarded { get; set; }
        public List<object> labels { get; set; }
        public bool hasReaction { get; set; }
        public bool productHeaderImageRejected { get; set; }
        public int lastPlaybackProgress { get; set; }
        public bool isDynamicReplyButtonsMsg { get; set; }
        public bool isMdHistoryMsg { get; set; }
        public bool requiresDirectConnection { get; set; }
        public bool pttForwardedFeaturesEnabled { get; set; }
        public string chatId { get; set; }
        public bool fromMe { get; set; }
        public Sender sender { get; set; }
        public int timestamp { get; set; }
        public string content { get; set; }
        public bool isGroupMsg { get; set; }
        public bool isMedia { get; set; }
        public bool isNotification { get; set; }
        public bool isPSA { get; set; }
    }

    public class Sender
    {
        public string id { get; set; }
        public string pushname { get; set; }
        public string type { get; set; }
        public string verifiedName { get; set; }
        public bool isBusiness { get; set; }
        public bool isEnterprise { get; set; }
        public int verifiedLevel { get; set; }
        public object privacyMode { get; set; }
        public List<object> labels { get; set; }
        public int isContactSyncCompleted { get; set; }
        public string formattedName { get; set; }
        public bool isMe { get; set; }
        public bool isMyContact { get; set; }
        public bool isPSA { get; set; }
        public bool isUser { get; set; }
        public bool isWAContact { get; set; }
        public object profilePicThumbObj { get; set; }
        public object msgs { get; set; }
    }

    #region SendText
    public class SendTextRequest
    {
        public string phone { get; set; }
        public string message { get; set; }

        [JsonProperty(PropertyName = "isGroup")]
        public bool isGroup { get; set; }
    }
    #endregion

    public class SendImageRequest
    {
        public string phone { get; set; }
        public string filename { get; set; }
        public string base64 { get; set; }
        public string message { get; set; }
    }


    #region SendButtons
    public class SendButtonsRequest
    {
        public string phone { get; set; }
        public string message { get; set; }
        public SendButtonOptions options { get; set; }
    }

    public class SendButtonOptions
    {
        [JsonProperty(PropertyName = "useTemplateButtons")]
        public bool useTemplateButtons { get; set; }
        public string title { get; set; }
        public string footer { get; set; }
        public List<SendButtonOptionsReq> buttons { get; set; }
    }

    public class SendButtonOptionsReq
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    #endregion

    #region SendButtonsUrl
    public class SendButtonsUrlRequest
    {
        public string phone { get; set; }
        public string message { get; set; }
        public SendButtonUrlOptions options { get; set; }
    }

    public class SendButtonUrlOptions
    {
        [JsonProperty(PropertyName = "useTemplateButtons")]
        public bool useTemplateButtons { get; set; }
        public string title { get; set; }
        public string footer { get; set; }
        public List<System.Dynamic.ExpandoObject> buttons { get; set; }
    }

    public class SendButtonUrlOptionsReq
    {
        public string id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }
    #endregion

    #region ListRequest and Response

    public class SendListRequest
    {
        public string phone { get; set; }

        [JsonProperty(PropertyName = "buttonText")]
        public string buttonText { get; set; }
        public string description { get; set; }
        public List<Section> sections { get; set; }
    }

    public class Row
    {
        [JsonProperty(PropertyName = "rowId")]
        public string rowId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class Section
    {
        public string title { get; set; }
        public List<Row> rows { get; set; }
    }
    #endregion
}
