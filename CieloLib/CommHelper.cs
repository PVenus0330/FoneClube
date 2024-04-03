using CieloLib.Email;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib
{
    public class CommHelper
    {
        string _merchantId;
        string _securityKey;
        bool _useSandbox;

        public CommHelper(string merchantId, string securityKey, bool useSandbox = false)
        {
            _securityKey = securityKey;
            _merchantId = merchantId;
            _useSandbox = useSandbox;
        }
        public string GetCieloPaymentGatewayUrl()
        {
            return (_useSandbox ? "https://apisandbox.cieloecommerce.cielo.com.br" : "https://api.cieloecommerce.cielo.com.br");
        }

        public R PostRequestD<Q, R>(Q cieloRequest) where Q:CieloBaseRequest where R:CieloResponse
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12;

            R cieloResponse;
            string serviceUrl = $"{GetCieloPaymentGatewayUrl()}/1/sales/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["MerchantId"] = _merchantId;
            request.Headers["MerchantKey"] = _securityKey;
            var requestData = JsonConvert.SerializeObject(cieloRequest,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            var responseData = "";
            byte[] postData = Encoding.UTF8.GetBytes(requestData);
            request.ContentLength = (long)((int)postData.Length);
            try
            {
                
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, (int)postData.Length);
                }
                using (StreamReader streamReader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
                {
                    responseData = streamReader.ReadToEnd();
                    cieloResponse = JsonConvert.DeserializeObject<R>(responseData);
                }
            }
            catch (WebException webException)
            {
                WebException ex = webException;
                string strResponse = null;
                try
                {
                    if (ex.Response != null)
                    {
                        using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                        {
                            strResponse = streamReader.ReadToEnd();
                            cieloResponse = JsonConvert.DeserializeObject<R>(strResponse);
                        }
                    }
                    else
                    {
                        cieloResponse = Activator.CreateInstance<R>();
                        cieloResponse.Code = "-1";
                        cieloResponse.Message =  ex.Message;
                    }
                }
                catch (Exception exception)
                {
                    var msg = strResponse != null ? strResponse : "Cielo error: failed to recieve from payment gateway";

                    cieloResponse = null;
                }
            }
            catch (Exception exception1)
            {
                cieloResponse = null;
            }
            return cieloResponse;
        }


        public R PostRequest<Q, R>(Q cieloRequest, string endPoint = "/1/sales/") where Q : CieloBaseRequest where R : CieloResponse
        {
            string serviceEndPoint = $"{GetCieloPaymentGatewayUrl()}{endPoint}";
            return DoRequest<Q,R>(cieloRequest, serviceEndPoint, "POST");
        }

        public R PutRequest<Q, R>(Q cieloRequest, string endPoint) where Q : CieloBaseRequest where R : CieloResponse
        {
            string serviceEndPoint = $"{GetCieloPaymentGatewayUrl()}{endPoint}";
            return DoRequest<Q, R>(cieloRequest, serviceEndPoint, "PUT");
        }

        private R DoRequest<Q, R>(Q cieloRequest, string serviceEndPoint, string method) where Q : CieloBaseRequest where R : CieloResponse
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12;

            R cieloResponse;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceEndPoint);
            request.Method = method;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["MerchantId"] = _merchantId;
            request.Headers["MerchantKey"] = _securityKey;

            var responseData = "";
            byte[] postData = new byte[0];

            if (cieloRequest == null)
            {
                postData = Encoding.UTF8.GetBytes("{}");
            }
            else
            {
                var requestData = JsonConvert.SerializeObject(cieloRequest,
                                Newtonsoft.Json.Formatting.None,
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                });

                postData = Encoding.UTF8.GetBytes(requestData);
            }

            //request.ContentLength = (long)((int)postData.Length);

            try
            {
                var txnType = method;

                if (cieloRequest == null)
                {

                }
                else
                {
                    txnType = cieloRequest.TxnType;
                }

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, (int)postData.Length);
                }

                using (StreamReader streamReader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
                {
                    responseData = streamReader.ReadToEnd();
                    cieloResponse = JsonConvert.DeserializeObject<R>(responseData);
                }
            }
            catch (WebException webException)
            {
                //try
                //{
                //    new EmailUtil().SendEmail("rodrigocardozop@gmail.com", "Debug " + DateTime.Now.ToString(), "webex" + JsonConvert.SerializeObject(webException));
                //}
                //catch (Exception) { }

                WebException ex = webException;
                string strResponse = null;
                try
                {
                    if (ex.Response != null)
                    {
                        using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                        {
                            strResponse = streamReader.ReadToEnd();
                            if (strResponse.StartsWith("["))
                            {
                                cieloResponse = JsonConvert.DeserializeObject<R[]>(strResponse)[0];
                            }
                            else
                            {
                                cieloResponse = JsonConvert.DeserializeObject<R>(strResponse);
                            }
                        }
                    }
                    else
                    {
                        cieloResponse = Activator.CreateInstance<R>();
                        cieloResponse.Code = "-1";
                        cieloResponse.Message = ex.Message;
                    }
                }
                catch (Exception exception)
                {
                    var msg = strResponse != null ? strResponse : "Cielo error: failed to recieve from payment gateway";
                    cieloResponse = null;
                }
            }
            catch (Exception exception1)
            {
                cieloResponse = null;
            }
            return cieloResponse;
        }

        public R GetRequest<R>(string queryUrl)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12;

            R cieloResponse;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryUrl);
            string paymentId = queryUrl.Substring(queryUrl.LastIndexOf("/sales/") + 7).TrimEnd('/');
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers["MerchantId"] = _merchantId;
            request.Headers["MerchantKey"] = _securityKey;
            var responseData = "";

            try
            {
                
                using (StreamReader streamReader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
                {
                    responseData = streamReader.ReadToEnd();
                    cieloResponse = JsonConvert.DeserializeObject<R>(responseData);
                }
            }
            catch (WebException webException)
            {
                WebException ex = webException;
                try
                {
                    using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                    {
                        string response = streamReader.ReadToEnd();
                        cieloResponse = JsonConvert.DeserializeObject<R>(response);
                    }
                }
                catch (Exception)
                {
                    cieloResponse = default(R);
                }
            }
            catch (Exception exception1)
            {
                cieloResponse = default(R);
            }
            return cieloResponse;
        }
    }
}
