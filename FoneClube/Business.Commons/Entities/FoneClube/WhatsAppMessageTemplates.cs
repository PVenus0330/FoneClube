using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class WhatsAppMessageTemplates
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Trigger { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Footer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Buttons { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MessageType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CallBackAction { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Selected { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CommentTemp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TemplateName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Internal { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Urls { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ListButton { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ListSections { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ListSectionRows { get; set; }
    }

    public class WhatsAppConfigSettings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigValue { get; set; }
    }

    public class WhatsAppConfigSettingsForTemplate
    {
        public bool useRocket { get; set; }
        public bool useChatX { get; set; }
        public bool useButton { get; set; }

        public bool useList { get; set; }

        public bool useURL { get; set; }
    }
}
