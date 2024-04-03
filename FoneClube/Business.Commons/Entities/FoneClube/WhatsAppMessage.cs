using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class WhatsAppMessage
    {
        public string ClientIds { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Buttons { get; set; }
        public string Urls { get; set; }
        public ButtonData ButtonList { get; set; }
        public UrlData UrlList { get; set; }
        public ListData SendList { get; set; }
    }

    public class ButtonData
    {
        public bool useTemplateButtons { get; set; }
        public string title { get; set; }
        public string footer { get; set; }
        public List<ButtonRows> buttons { get; set; }
    }

    public class UrlData
    {
        public bool useTemplateButtons { get; set; }
        public string title { get; set; }
        public string footer { get; set; }
        public List<ButtonUrlRows> buttonsUrl { get; set; }
    }

    public class ButtonRows
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class ButtonUrlRows
    {
        public string id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }

    public class ListData
    {
        public string ButtonText { get; set; }
        public string Description { get; set; }
        public List<ListSection> Sections { get; set; }
    }

    public class ListRow
    {
        public string RowId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class ListSection
    {
        public string Title { get; set; }
        public List<ListRow> Rows { get; set; }
    }

    public class WhatsAppMessageResponse
    {
        public string Status { get; set; }
    }

    public class WANumberCheckResponse
    {
        public string status { get; set; }
        public WACheckResponse response { get; set; }
        public string session { get; set; }
    }

    public class WACheckResponse
    {
        public bool isBusiness { get; set; }
        public bool canReceiveMessage { get; set; }
        public bool numberExists { get; set; }
    }

    public class SendMessageToAdminAndParent
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string CPF { get; set; }
        public string WhatsAppNumber { get; set; }
        public string ParentName { get; set; }
        public string ParentWhatsAppNumber { get; set; }

        public string Message { get; set; }
    }

    public class SendMessageToAdmin
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string CPF { get; set; }
        public string WhatsAppNumber { get; set; }
        public string ICCID { get; set; }
        public string Vigencia { get; set; }
        public string Vencimento { get; set; }
        public string Amount { get; set; }
        public string TransactionId { get; set; }
        public string CurrentDateTime { get; set; }

        public string Tipo { get; set; }
    }
}
