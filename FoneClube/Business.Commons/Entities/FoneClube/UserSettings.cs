using Newtonsoft.Json;
using System;

namespace FoneClube.Business.Commons.Entities
{
    public class UserSettings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int IntIdPerson { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrecoPromoSum { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrecoFCSum { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsUse2Prices { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsVIP { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLinhaAtiva { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsShowICCID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsShowPort { get; set; }
    }
}
