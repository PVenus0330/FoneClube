using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.BoletoSimples.Common
{
    /// <summary>
    /// Representa os parametros de conexão utilizados pelo client
    /// </summary>
    public class ClientConnection
    {
        public readonly string UserAgent;
        public readonly string ApiToken;
        public readonly string ApiVersion;
        public readonly string ApiUrl;

        public ClientConnection() : this(ConfigurationManager.AppSettings["boletosimple-api-url"],
                                         ConfigurationManager.AppSettings["boletosimple-api-version"],
                                         ConfigurationManager.AppSettings["boletosimple-api-token"],
                                         ConfigurationManager.AppSettings["boletosimple-useragent"])
                                       
        { }

        public ClientConnection(string apiUrl, string apiVersion, string apiToken, string userAgent)
        {
            ApiUrl = apiUrl;
            ApiVersion = apiVersion;
            ApiToken = apiToken;
            UserAgent = userAgent;
            
        }
       
        public Uri GetBaseUri() => new Uri($"{ApiUrl}/{ApiVersion}");
    }
}
