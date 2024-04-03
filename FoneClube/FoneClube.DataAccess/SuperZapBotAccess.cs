using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using FoneClube.DataAccess.Utilities;
using FoneClube.Business.Commons.Entities.FoneClube;

namespace FoneClube.DataAccess
{
    public class SuperZapBotAccess
    {
        string Token = "6560b058847476cc0ed6c20b";
        string AdminMsgsTo = string.Empty;
        string BaseAddress = "https://api.chat-x.tec.br/core/v2/api/";
        string apiMethod = "";

        public SuperZapBotAccess()
        {
            using (var ctx = new FoneClubeContext())
            {
                var token = ctx.tblConfigSettings.Where(x => x.txtConfigName == "ChatXApiKey").FirstOrDefault();
                if (token != null)
                    Token = token.txtConfigValue;

                var numbers = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppMsgToAdmins").FirstOrDefault();
                AdminMsgsTo = numbers.txtConfigValue;

                apiMethod = "https://api.chat-x.tec.br/core/v2/api/chats/send-text?accessToken={0}&forceSend=true&isWhisper=false&message={1}&number={2}&verifyContact=false";
            }
        }
        public bool SendMessageChatX(WhatsAppMessage model)
        {
            bool isAnyOneSuccess = false;
            try
            {
                if (model != null)
                {
                    var splitPhones = model.ClientIds.Split(',');
                    string msg = model.Message.Replace("#", "%23").Replace("\n", "%0a");
                    foreach (var phone in splitPhones)
                    {
                        var phone1 = phone.Length == 11 ? "55" + phone : phone;
                        try
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            WebRequest request = WebRequest.Create(apiMethod);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response != null && response.StatusCode == HttpStatusCode.OK && !isAnyOneSuccess)
                                isAnyOneSuccess = true;

                            LogHelper.LogMessageOld(0, string.Format("SendMessageChatX message sent to {0}, Message:{1}", phone1, msg));

                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogMessageOld(0, "SendMessageChatX inner error: " + ex.ToString());
                            if (!isAnyOneSuccess)
                                isAnyOneSuccess = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(0, "SendMessageChatX error: " + ex.ToString());
                return false;
            }
            return isAnyOneSuccess;
        }

        public bool SendMessageChatXNew(WhatsAppMessage model)
        {
            bool isAnyOneSuccess = false;
            try
            {
                if (model != null)
                {
                    var splitPhones = model.ClientIds.Split(',');
                    string msg = model.Message;//.Replace("#", "%23").Replace("\n", "%0a");
                    foreach (var phone in splitPhones)
                    {
                        var phone1 = phone.Length == 11 ? "55" + phone : phone;
                        try
                        {
                            var message = new ChatXRequest()
                            {
                                contactId = "",
                                delayInSeconds = 0,
                                forceSend = true,
                                isWhisper = false,
                                message = msg,
                                number = phone1,
                                verifyContact = false
                            };
                            var JsonStr = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                            StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                            string apiMethod = string.Format("chats/send-text");

                            using (var client = new HttpClient())
                            {

                                // Setting Base address.  
                                client.BaseAddress = new Uri(BaseAddress);

                                client.DefaultRequestHeaders.Accept.Clear();

                                client.DefaultRequestHeaders.Add("access-token", Token);
                                // Setting content type.  
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                // Initialization.  
                                HttpResponseMessage response = new HttpResponseMessage();

                                // HTTP GET  
                                response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                                var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                                LogHelper.LogMessageOld(0, "SendMessageChatXNew Response: " + contentresponse);

                                var chatXResponse = JsonConvert.DeserializeObject<ChatXResponse>(contentresponse);
                                if (chatXResponse != null && chatXResponse.status == "202")
                                {
                                    LogHelper.LogMessageOld(0, string.Format("SendMessageChatX message sent to {0}, Message:{1}", phone1, msg));
                                    isAnyOneSuccess = true;
                                }
                                else
                                {
                                    LogHelper.LogMessageOld(0, string.Format("SendMessageChatX message failed to {0}, Message:{1}", phone1, chatXResponse != null ? chatXResponse.msg : ""));
                                    if (!isAnyOneSuccess)
                                        isAnyOneSuccess = false;
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogMessageOld(0, "SendMessageChatX inner error: " + ex.ToString());
                            if (!isAnyOneSuccess)
                                isAnyOneSuccess = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(0, "SendMessageChatX error: " + ex.ToString());
                return false;
            }
            return isAnyOneSuccess;
        }

        public List<SendMessageChatXResponse> SendMessageChatXNewMulti(WhatsAppMessage model)
        {
            List<SendMessageChatXResponse> lstRes = new List<SendMessageChatXResponse>();
            try
            {
                if (model != null)
                {
                    var splitPhones = model.ClientIds.Split(',');
                    string msg = model.Message;//.Replace("#", "%23").Replace("\n", "%0a");
                    foreach (var phone in splitPhones)
                    {
                        var phone1 = phone.Length == 11 ? "55" + phone : phone;
                        try
                        {
                            var message = new ChatXRequest()
                            {
                                contactId = "",
                                delayInSeconds = 0,
                                forceSend = true,
                                isWhisper = false,
                                message = msg,
                                number = phone1,
                                verifyContact = false
                            };
                            var JsonStr = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                            StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                            string apiMethod = string.Format("chats/send-text");

                            using (var client = new HttpClient())
                            {

                                // Setting Base address.  
                                client.BaseAddress = new Uri(BaseAddress);

                                client.DefaultRequestHeaders.Accept.Clear();

                                client.DefaultRequestHeaders.Add("access-token", Token);
                                // Setting content type.  
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                // Initialization.  
                                HttpResponseMessage response = new HttpResponseMessage();

                                // HTTP GET  
                                response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                                var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                                LogHelper.LogMessageOld(0, "SendMessageChatXNewMulti Response: " + contentresponse);

                                var chatXResponse = JsonConvert.DeserializeObject<ChatXResponse>(contentresponse);

                                lstRes.Add(new SendMessageChatXResponse() { PhoneNumber = phone1, Response = chatXResponse });
                            }


                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogMessageOld(0, "SendMessageChatXNewMulti inner error: " + ex.ToString());

                            lstRes.Add(new SendMessageChatXResponse() { PhoneNumber = phone1, Response = new ChatXResponse() { status = "Failure", msg = "Error occured while sending message :" + ex.ToString() } });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(0, "SendMessageChatXNewMulti error: " + ex.ToString());
                return null;
            }
            return lstRes;
        }
    }

    public class ChatXResponse
    {
        public string status { get; set; }
        public string msg { get; set; }
        public string messageSentId { get; set; }
    }

    public class ChatXRequest
    {
        public string number { get; set; }
        public string contactId { get; set; }
        public string message { get; set; }
        public bool isWhisper { get; set; }
        public bool forceSend { get; set; }
        public bool verifyContact { get; set; }
        public int delayInSeconds { get; set; }
    }
}
