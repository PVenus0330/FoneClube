//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace CieloLib.Boleto
//{
//    public class BoletoHelper
//    {
//        string _merchantId;
//        string _securityKey;
//        bool _useSandbox;
//        ILogger _logger = null;

//        public BoletoHelper(string merchantId, string securityKey, bool useSandbox = false, ILogger logger = null)
//        {
//            _securityKey = securityKey;
//            _merchantId = merchantId;
//            _useSandbox = useSandbox;
//            _logger = logger;

//            if (_logger == null)
//                _logger = new Logger();
//        }
//        private string GetCieloPaymentGatewayUrl()
//        {
//            return (_useSandbox ? "https://apisandbox.cieloecommerce.cielo.com.br" : "https://api.cieloecommerce.cielo.com.br");
//        }

//        //public string GetProvider()
//        //{
//        //    return _useSandbox ? "Simulado" : "Simulado";
//        //}

//        public BoletoResponse PostRequest(BoletoRequest CieloBoletoRequest)
//        {

//            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
//                                       SecurityProtocolType.Tls11 |
//                                       SecurityProtocolType.Tls12;

//            BoletoResponse CieloBoletoResponse;
//            string serviceUrl = $"{GetCieloPaymentGatewayUrl()}/1/sales/";
//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);
//            request.Method = "POST";
//            request.ContentType = "application/json";
//            request.Accept = "application/json";
//            request.Headers["MerchantId"] = _merchantId;
//            request.Headers["MerchantKey"] = _securityKey;
//            var requestData = JsonConvert.SerializeObject(CieloBoletoRequest,
//                            Newtonsoft.Json.Formatting.None,
//                            new JsonSerializerSettings
//                            {
//                                NullValueHandling = NullValueHandling.Ignore
//                            });
//            var responseData = "";
//            byte[] postData = Encoding.UTF8.GetBytes(requestData);
//            request.ContentLength = (long)((int)postData.Length);
//            try
//            {
//                _logger.InsertLog(MessageLevel.Information, $"Boleto Debit Request Message to {serviceUrl}: {CieloBoletoRequest.MerchantOrderId}", JsonConvert.SerializeObject(CieloBoletoRequest));
//                using (Stream stream = request.GetRequestStream())
//                {
//                    stream.Write(postData, 0, (int)postData.Length);
//                }
//                using (StreamReader streamReader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
//                {
//                    responseData = streamReader.ReadToEnd();
//                    CieloBoletoResponse = JsonConvert.DeserializeObject<BoletoResponse>(responseData);
//                    _logger.InsertLog(MessageLevel.Information, $"Boleto Debit Response Message: {CieloBoletoRequest.MerchantOrderId}", JsonConvert.SerializeObject(CieloBoletoResponse));
//                }
//            }
//            catch (WebException webException)
//            {
//                WebException ex = webException;
//                try
//                {
//                    if (ex.Response != null)
//                    {
//                        using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
//                        {
//                            string response = streamReader.ReadToEnd();
//                            CieloBoletoResponse = JsonConvert.DeserializeObject<BoletoResponse>(response);
//                        }
//                    }
//                    else
//                    {
//                        CieloBoletoResponse = new BoletoResponse() { Code = "-1", Message =  ex.Message };
//                    }
//                    _logger.Error($"Cielo error. Order {CieloBoletoRequest.MerchantOrderId}: {JsonConvert.SerializeObject(CieloBoletoResponse)}", ex);
//                }
//                catch (Exception exception)
//                {
//                    _logger.Error("Cielo error: failed to recieve from payment gateway", exception);
//                    CieloBoletoResponse = null;
//                }
//            }
//            catch (Exception exception1)
//            {
//                _logger.Error("Cielo error: cannot connect to payment gateway", exception1);
//                CieloBoletoResponse = null;
//            }
//            return CieloBoletoResponse;
//        }

//        public BoletoResponse GetRequest(string url, string paymentId)
//        {
//            BoletoResponse CieloBoletoResponse;
//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//            request.Method = "GET";
//            request.Accept = "application/json";
//            request.Headers["MerchantId"] = _merchantId;
//            request.Headers["MerchantKey"] = _securityKey;
//            var responseData = "";

//            try
//            {
//                _logger.InsertLog(MessageLevel.Information, $"Transaction Query Message for {paymentId}: {url}", $"transaction Query Message to {url}");

//                using (StreamReader streamReader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
//                {
//                    responseData = streamReader.ReadToEnd();
//                    _logger.InsertLog(MessageLevel.Information, $"Query Response Message for {paymentId}:", responseData);
//                    CieloBoletoResponse = JsonConvert.DeserializeObject<BoletoResponse>(responseData);
//                }
//            }
//            catch (WebException webException)
//            {
//                WebException ex = webException;
//                try
//                {
//                    using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
//                    {
//                        string response = streamReader.ReadToEnd();
//                        _logger.Error($"Cielo error. PaymentId {paymentId}: {response}", ex);
//                        CieloBoletoResponse = JsonConvert.DeserializeObject<BoletoResponse>(response);
//                    }
//                }
//                catch (Exception exception)
//                {
//                    _logger.Error("Cielo error: failed to recieve from payment gateway", exception);
//                    CieloBoletoResponse = null;
//                }
//            }
//            catch (Exception exception1)
//            {
//                _logger.Error("Cielo error: cannot connect to payment gateway", exception1);
//                CieloBoletoResponse = null;
//            }
//            return CieloBoletoResponse;
//        }
//    }
//}
