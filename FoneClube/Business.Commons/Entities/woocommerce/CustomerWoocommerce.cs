using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.woocommerce
{


    public class Billing
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string number { get; set; }
        public string neighborhood { get; set; }
        public string persontype { get; set; }
        public string cpf { get; set; }
        public string rg { get; set; }
        public string cnpj { get; set; }
        public string ie { get; set; }
        public string birthdate { get; set; }
        public string sex { get; set; }
        public string cellphone { get; set; }
    }

    public class Shipping
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string number { get; set; }
        public string neighborhood { get; set; }
    }

    public class MetaData
    {
        public int id { get; set; }
        public string key { get; set; }
        public object value { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Collection
    {
        public string href { get; set; }
    }

    public class Links
    {
        public List<Self> self { get; set; }
        public List<Collection> collection { get; set; }
    }

    public class CustomerWoocommerce
    {
        public int id { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_created_gmt { get; set; }
        public DateTime date_modified { get; set; }
        public DateTime date_modified_gmt { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string role { get; set; }
        public string username { get; set; }
        public Billing billing { get; set; }
        public Shipping shipping { get; set; }
        public bool is_paying_customer { get; set; }
        public string avatar_url { get; set; }
        public List<MetaData> meta_data { get; set; }
        public Links _links { get; set; }
    }
  
}
