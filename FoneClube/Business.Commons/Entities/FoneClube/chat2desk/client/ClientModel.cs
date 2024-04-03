using FoneClube.Business.Commons.Entities.FoneClube.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.chat2desk.client
{
    public class CreateClientModel
    {
        public string Phone { get; set; }
        public string Transport { get; set; }
        public string Channel_Id { get; set; }
    }

    public class GetClientModel : WhatsappBase
    {
        public ClientsDataModel Data { get; set; }
    }

    public class ClientListModel
    {
        public List<ClientsDataModel> Data { get; set; }
        public WhatsappListMeta Meta { get; set; }
        public string Status { get; set; }
    }


    public class ClientsDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public string Assigned_Name { get; set; }
        public object Comment { get; set; }
        public string Client_Phone { get; set; }
        public object Region_Id { get; set; }
        public int? Country_Id { get; set; }
        public object External_Id { get; set; }
        public object External_Ids { get; set; }
        public object Extra_Comment_1 { get; set; }
        public object Extra_Comment_2 { get; set; }
        public Dictionary<string, string> Custom_Fields { get; set; }
        public object Extra_Comment_3 { get; set; }
        public List<Tag> Tags { get; set; }
    }
    public class Tag
    {
        public int id { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public int group_id { get; set; }
        public string group_name { get; set; }
    }
}
