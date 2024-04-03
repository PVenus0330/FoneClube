using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.message
{
    public class CreateClientModel
    {
        public string phone { get; set; }
        public string transport { get; set; }
        public string channel_id { get; set; }
    }

    public class GetClientModel : WhatsappBase
    {
        public ClientsDataModel data { get; set; }
    }

    public class ClientListModel
    {
        public List<ClientsDataModel> data { get; set; }
        public WhatsappListMeta meta { get; set; }
        public string status { get; set; }
    }


    public class ClientsDataModel
    {
        public long id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public string phone { get; set; }
        public string assigned_name { get; set; }
        public object comment { get; set; }
        public string client_phone { get; set; }
        public object region_id { get; set; }
        public int? country_id { get; set; }
        public object external_id { get; set; }
        public object external_ids { get; set; }
        public object extra_comment_1 { get; set; }
        public object extra_comment_2 { get; set; }
        public Dictionary<string, string> custom_fields { get; set; }
        public object extra_comment_3 { get; set; }
    }
}
