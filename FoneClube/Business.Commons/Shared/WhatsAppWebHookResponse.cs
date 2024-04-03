using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons
{
    public class WhatsAppWebHookResponse
    {
        public string @event { get; set; }
        public string session { get; set; }
        //public string id { get; set; }
        public string body { get; set; }
        public string type { get; set; }
        public long t { get; set; }
        public string notifyName { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string self { get; set; }
        public int ack { get; set; }
        public bool isNewMsg { get; set; }
        public bool star { get; set; }
        public bool recvFresh { get; set; }
        public List<object> interactiveAnnotations { get; set; }
        public string deprecatedMms3Url { get; set; }
        public string directPath { get; set; }
        public string mimetype { get; set; }
        public string filehash { get; set; }
        public string encFilehash { get; set; }
        public int size { get; set; }
        public string mediaKey { get; set; }
        public int mediaKeyTimestamp { get; set; }
        public bool isViewOnce { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string staticUrl { get; set; }
        public List<int> scanLengths { get; set; }
        public ScansSidecar scansSidecar { get; set; }
        public bool isFromTemplate { get; set; }
        public bool broadcast { get; set; }
        public List<object> mentionedJidList { get; set; }
        public bool isVcardOverMmsDocument { get; set; }
        public bool isForwarded { get; set; }
        public bool hasReaction { get; set; }
        public bool ephemeralOutOfSync { get; set; }
        public bool productHeaderImageRejected { get; set; }
        public int lastPlaybackProgress { get; set; }
        public bool isDynamicReplyButtonsMsg { get; set; }
        public bool isMdHistoryMsg { get; set; }
        public bool requiresDirectConnection { get; set; }
        public bool pttForwardedFeaturesEnabled { get; set; }
        public string chatId { get; set; }
        public bool fromMe { get; set; }
        public Sender sender { get; set; }
        public long timestamp { get; set; }
        public string content { get; set; }
        public bool isGroupMsg { get; set; }
        public bool isMedia { get; set; }
        public bool isNotification { get; set; }
        public bool isPSA { get; set; }
        public MediaData mediaData { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class MediaData
    {
        public string type { get; set; }
        public string mediaStage { get; set; }
        public int animationDuration { get; set; }
        public bool animatedAsNewMsg { get; set; }
        public bool isViewOnce { get; set; }
        public bool _swStreamingSupported { get; set; }
        public bool _listeningToSwSupport { get; set; }
        public bool isVcardOverMmsDocument { get; set; }
    }

    public class ProfilePicThumbObj
    {
        public string eurl { get; set; }
        public string id { get; set; }
        public string img { get; set; }
        public string imgFull { get; set; }
        public object raw { get; set; }
        public string tag { get; set; }
    }

    public class ScansSidecar
    {
    }

    public class Sender
    {
        public string id { get; set; }
        public string name { get; set; }
        public string shortName { get; set; }
        public string pushname { get; set; }
        public string type { get; set; }
        public bool isBusiness { get; set; }
        public bool isEnterprise { get; set; }
        public int isContactSyncCompleted { get; set; }
        public string formattedName { get; set; }
        public bool isMe { get; set; }
        public bool isMyContact { get; set; }
        public bool isPSA { get; set; }
        public bool isUser { get; set; }
        public bool isWAContact { get; set; }
        public ProfilePicThumbObj profilePicThumbObj { get; set; }
        public object msgs { get; set; }
    }

    public class CPFResponse
    {
        public int status { get; set; }
        public string cpf { get; set; }
        public string nome { get; set; }
        public int pacoteUsado { get; set; }
        public int saldo { get; set; }
        public string consultaID { get; set; }
        public double delay { get; set; }
    }

    public class GetbatteryLevelResponse
    {
        public string status { get; set; }
        public string session { get; set; }
    }
}
