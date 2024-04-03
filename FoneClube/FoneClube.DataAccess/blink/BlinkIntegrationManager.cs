using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess.blink
{
    public class BlinkIntegrationManager
    {
        BlinkSettings settings;

        public BlinkIntegrationManager() {
            settings = new BlinkSettings
            {
                BlinkApiKey = "rdaK8Raz8PB2Q_xxrP3v5H5WQrPd461wTaqI6VByRbpIy",
                BlinkUserName = "urlshortener@foneclube.com.br",
                BlinkPassword = "fcshorturl",
                BlinkDomainId = "convite.foneclube.com.br"
            };
        }

        public string CreateLinkIndication(string urlSource, string urlMaskName)
        {
            var idTemplateLink = "50298";
            var blinkManager = new BlinkIntegrationManager();
            var token = blinkManager.GetAuthenticationToken();
            var links = blinkManager.CreateLink(token, idTemplateLink, urlSource, urlMaskName);


            return links.FirstOrDefault();
        }

        public string GetAuthenticationToken()
        {
            if (string.IsNullOrEmpty(settings.BlinkUserName) || string.IsNullOrEmpty(settings.BlinkPassword))
                return null;
            try
            {
                var userInfo = new
                {
                    email = settings.BlinkUserName,
                    password = settings.BlinkPassword
                };

                var infoJson = Newtonsoft.Json.JsonConvert.SerializeObject(userInfo);

                using (HttpClient httpclient = new HttpClient())
                {
                    var uri = "https://app.bl.ink";
                    var endpoint = "/api/v3/access_token";

                    httpclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpclient.BaseAddress = new Uri(uri);
                    httpclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", "Bearer " + settings.BlinkApiKey);
                    //httpclient.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.BLinkApiKey);


                    HttpResponseMessage response = httpclient.PostAsync(endpoint, new StringContent(infoJson, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                    var responseString = response.Content.ReadAsStringAsync();

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //_logger.Error("Error in BLink Authentication Token Get. Response Code:" + response.StatusCode + " Response Message:" + responseString);
                        return string.Empty;
                    }
                    else
                    {
                        var result = JObject.Parse(responseString.Result);
                        return result["access_token"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.InsertLog(LogLevel.Error, "Error in BLink Authentication Token Get. Exception: " + ex.Message, ex.StackTrace);
            }
            return string.Empty;
        }

        public IList<BlinkListItem> PrepareBlinkDomains(string accessToken)
        {
            IList<BlinkListItem> domains = new List<BlinkListItem>();
            if (!string.IsNullOrEmpty(accessToken))
            {
                using (HttpClient httpclient = new HttpClient())
                {
                    var uri = "https://app.bl.ink";
                    var endpoint = "/api/v3/domains";

                    httpclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpclient.BaseAddress = new Uri(uri);
                    // httpclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", "Bearer " + accessToken);
                    httpclient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    HttpResponseMessage response = httpclient.GetAsync(endpoint).GetAwaiter().GetResult();
                    var responseString = response.Content.ReadAsStringAsync();

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //_logger.Error("Error in BLink Domain get. Response Code:" + response.StatusCode + " Response Message:" + responseString);
                    }
                    else
                    {
                        var result = JObject.Parse(responseString.Result);
                        var domain = result["objects"].ToList();
                        foreach (var d in domain)
                        {
                            domains.Add(new BlinkListItem { Text = d["domain"].ToString(), Value = d["id"].ToString() });
                        }
                    }
                }
            }
            return domains;
        }

        public bool CheckExistingUrl(string accessToken, string domainId, string keyword)
        {
            try
            {
                UriBuilder builder = new UriBuilder("https://app.bl.ink/api/v3/" + domainId + "/links");
                builder.Query = "keyword=" + keyword + "";

                //Create a query
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                HttpResponseMessage response = client.GetAsync(builder.Uri).Result;
                var responseString = response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //_logger.Error("Error in Blink Links get. Response Code:" + response.StatusCode + " Response Message:" + responseString);
                }
                else
                {
                    var result = JObject.Parse(responseString.Result);
                    if (result["count"].ToString() != "0")
                        return true;
                }
            }
            catch (Exception ex)
            {
                //_logger.InsertLog(LogLevel.Error, ex.Message, ex.StackTrace);
            }

            return false;
        }

        public string[] CreateLink(string accessToken, string domainId, string url, string alias)
        {
            string[] shortUrl = new string[2];
            if (accessToken != null || domainId != null || url != null || alias != null)
            {
                var urlInfo = new
                {
                    // url = "http://ffstage-001-site40.dtempurl.com/register?affiliate=61848&invitecode=50",
                    url = url,
                    alias = alias
                };

                var infoJson = Newtonsoft.Json.JsonConvert.SerializeObject(urlInfo);

                UriBuilder builder = new UriBuilder("https://app.bl.ink/api/v3/" + domainId + "/links");

                //Create a query
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                HttpResponseMessage response = client.PostAsync(builder.Uri, new StringContent(infoJson)).Result;

                var responseString = response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //_logger.Error("Error in Blink Link create. Response Code:" + response.StatusCode + " Response Message:" + responseString.Result);
                }
                else
                {
                    var result = JObject.Parse(responseString.Result);
                    var data = result["objects"];
                    shortUrl[0] = (data["short_link"].ToString());
                    shortUrl[1] = (data["id"].ToString());
                }
            }
            return shortUrl;
        }

        public string[] GetExistingUrl(string accessToken, string domainId, string keyword)
        {
            string[] bLink = new string[2];
            try
            {
                keyword = keyword.Substring(keyword.IndexOf('?') + 1);
                UriBuilder builder = new UriBuilder("https://app.bl.ink/api/v3/" + domainId + "/links");
                builder.Query = "keyword=" + keyword + "";

                //Create a query
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                HttpResponseMessage response = client.GetAsync(builder.Uri).Result;
                var responseString = response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //_logger.Error("Error in Blink Links get. Response Code:" + response.StatusCode + " Response Message:" + responseString);
                }
                else
                {
                    var result = JObject.Parse(responseString.Result);
                    if (result["count"].ToString() != "0")
                    {
                        var data = result["objects"].ToList();
                        foreach (var d in data)
                        {
                            var text = d["url"].ToString();
                            if (text.Contains(keyword))
                            {
                                bLink[0] = d["short_link"].ToString();
                                bLink[1] = d["id"].ToString();

                                return bLink;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.InsertLog(LogLevel.Error, ex.Message, ex.StackTrace);
            }

            return bLink;

        }

        public string UpdateBlink(string accessToken, string domainId, int bLinkId, string url, string valueToReplace)
        {
            var shortUrl = string.Empty;
            if (accessToken != null || domainId != null)
            {
                var list = new List<object>();
                var urlInfo = new
                {
                    op = "replace",
                    path = "/url",
                    value = valueToReplace
                };
                list.Add(urlInfo);

                var infoJson = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                Uri builder = new Uri("https://app.bl.ink/api/v3/" + domainId + "/links/" + bLinkId);

                //Create a query
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var method = new HttpMethod("PATCH");

                var request = new HttpRequestMessage(method, builder)
                {
                    Content = new StringContent(infoJson, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = client.SendAsync(request).Result;

                var responseString = response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    //_logger.Error("Error in Blink Link update. Response Code:" + response.StatusCode + " Response Message:" + responseString.Result);
                }
                else
                {
                    var result = JObject.Parse(responseString.Result);
                    var data = result["objects"];
                    shortUrl = data["short_link"].ToString();
                }
            }
            return shortUrl;
        }
    }
}
