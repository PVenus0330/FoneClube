using HttpService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v2;
using WooCommerceNET.WooCommerce.v2.Extension;

namespace FoneClube.DataAccess.woocommerce
{
    public class WooAPI
    {
        
        public WooCommerceNET.WooCommerce.v2.Customer UpdateCustomer(int customerId, WooCommerceNET.WooCommerce.v2.Customer customer)
        {
            var headers = new List<Header>();

            headers.Add(new Header { Key = "Authorization", value = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings["CK_KEY"] + ":" + ConfigurationManager.AppSettings["CS_KEY"])) });
            ApiGateway.EndPointApi = ConfigurationManager.AppSettings["API_LOJA"];
            var response = ApiGateway.SetConteudo("/wp-json/wc/v2/customers/" + customerId, JsonConvert.SerializeObject(customer), headers, SecurityProtocolType.Tls12);
            return JsonConvert.DeserializeObject<WooCommerceNET.WooCommerce.v2.Customer>(response);
        }
    }
}
