using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using FoneClube.DataAccess.Utilities;
using FoneClube.Business.Commons;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.WhatsApp;
using FoneClube.Business.Commons.Entities.ViewModel.Plano;
using Business.Commons.Utils;
using Newtonsoft.Json.Serialization;
using System.Data.Entity;
using System.Globalization;
using System.Diagnostics;

namespace FoneClube.DataAccess
{

    public class SendMessageChatXRequest
    {
        public string PhoneNumbers { get; set; }
        public string Message { get; set; }
    }

    public class SendMessageChatXResponse
    {
        public string PhoneNumber { get; set; }
        public ChatXResponse Response { get; set; }
    }


    public class GenericTemplate
    {
        public int PersonId { get; set; }
        public int TypeId { get; set; }
        public string PhoneNumbers { get; set; }
        public string Invitee { get; set; }
        public WhatsAppMessageTemplates Template { get; set; }
    }

    public class WelcomeTemplate
    {
        public string Name { get; set; }
        public string PhoneNumbers { get; set; }
        public WhatsAppMessageTemplates Template { get; set; }
    }
    public class WhatsAppAccess
    {
        string BaseAddress { get; set; }
        string SessionId { get; set; }
        string BearerToken { get; set; }
        string WebhookBaseUrl { get; set; }
        string WebhookUrl { get; set; }

        string AdminMsgsTo = "5521982008200";
        public WhatsAppAccess()
        {
            BaseAddress = ConfigurationManager.AppSettings["WPPBaseAddress"];
            SessionId = ConfigurationManager.AppSettings["WPPSessionId"];
            BearerToken = ConfigurationManager.AppSettings["WPPBearerToken"];

            using (var ctx = new FoneClubeContext())
            {
                var numbers = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppMsgToAdmins").FirstOrDefault();
                AdminMsgsTo = numbers.txtConfigValue;
            }
        }

        public string StartWPP()
        {
            StringBuilder outputLog = new StringBuilder();
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\inetroot\api.foneclube.com.br\wppstart.bat",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                proc.Start();

                outputLog.Append("WhatsApp started");
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "StartWPP failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                return ex.ToString();
            }
            return outputLog.ToString();
        }

        public string StopWPP()
        {
            StringBuilder outputLog = new StringBuilder();
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\inetroot\api.foneclube.com.br\wppstop.bat",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                proc.Start();
                proc.WaitForExit();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                int exitCode = proc.ExitCode;

                outputLog.AppendLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
                outputLog.AppendLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
                outputLog.AppendLine("ExitCode: " + exitCode.ToString());

                proc.Close();
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "StartWPP failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                return ex.ToString();
            }
            return outputLog.ToString();
        }

        public string ReStartWPP()
        {
            string status = string.Empty;
            try
            {
                string conStatus = ReturnStatusSession();
                if (!string.IsNullOrEmpty(conStatus) && (conStatus.ToLower().Contains("error") || conStatus.ToLower().Contains("disconnected")))
                {
                    status = StartWPP();
                    SendMessage(new WhatsAppMessage() { Message = "WhatsApp is restarted and connected again", ClientIds = "5521969640935" });
                }
                else
                {
                    //SendMessage(new WhatsAppMessage() { Message = "WhatsApp is connected", ClientIds = "5521969640935" });
                    status = conStatus;
                }
            }
            catch (Exception ex)
            {
                status = ex.ToString();
            }
            return status;
        }

        public GetbatteryLevelResponse GetBatteryLevel()
        {
            GetbatteryLevelResponse response = new GetbatteryLevelResponse();
            try
            {
                string apiMethod = string.Format("api/{0}/get-battery-level", SessionId);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage responseMsg = new HttpResponseMessage();

                    // HTTP GET  
                    responseMsg = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var response1 = responseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    response = JsonConvert.DeserializeObject<GetbatteryLevelResponse>(response1);
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetBatteryLevel failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                return null;
            }
            return response;
        }

        public string ManageSession(string sessionType)
        {
            string response = string.Empty;
            string apiMethod = string.Empty;
            try
            {
                switch (sessionType.ToLower())
                {
                    case "start":
                        apiMethod = string.Format("api/{0}/start-session", SessionId);
                        break;
                    case "close":
                        apiMethod = string.Format("api/{0}/close-session", SessionId);
                        break;
                    case "logout":
                        apiMethod = string.Format("api/{0}/logout-session", SessionId);
                        break;
                }
                WhatsAppSessionRequest objRequest = new WhatsAppSessionRequest()
                {
                    webhook = "https://api.foneclube.com.br/api/whatsapp/webhook"
                };
                var JsonStr = JsonConvert.SerializeObject(objRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");


                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage responseMsg = new HttpResponseMessage();

                    // HTTP GET  
                    responseMsg = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                    response = responseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ManageSession failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                return ex.ToString();
            }
            return response;
        }

        public string GenerateToken(string token)
        {
            string response = string.Empty;
            try
            {
                string apiMethod = string.Format("api/{0}/{1}/generate-token", SessionId, token);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage responseMsg = new HttpResponseMessage();

                    // HTTP GET  
                    responseMsg = client.PostAsync(apiMethod, null).GetAwaiter().GetResult();

                    response = responseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return response;
        }

        public string CheckStatusConnection()
        {
            string response = string.Empty;
            try
            {
                string apiMethod = apiMethod = string.Format("api/{0}/check-connection-session", SessionId);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage responseMsg = new HttpResponseMessage();

                    // HTTP GET  
                    responseMsg = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    response = responseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return response;
        }

        public string ReturnStatusSession()
        {
            string response = string.Empty;
            try
            {
                string apiMethod = apiMethod = string.Format("api/{0}/status-session", SessionId);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage responseMsg = new HttpResponseMessage();

                    // HTTP GET  
                    responseMsg = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    response = responseMsg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return response;
        }

        public HttpStatusCode ProcessWebhookWhatsAppMessage()
        {
            try
            {
                string status = string.Empty;
                System.IO.Stream req = HttpContext.Current.Request.InputStream;
                string json = new System.IO.StreamReader(req).ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json) && !string.IsNullOrEmpty(json))
                {
                    //using (var ctx = new FoneClubeContext())
                    //{
                    //    ctx.tblLog.Add(new tblLog
                    //    {
                    //        dteTimeStamp = DateTime.Now,
                    //        intIdTipo = 1,
                    //        txtAction = string.Format("ProcessWebhookWhatsAppMessage message: {0}", json),
                    //    });
                    //    ctx.SaveChanges();
                    //}

                    WhatsAppWebHookResponse waModel = JsonConvert.DeserializeObject<WhatsAppWebHookResponse>(json);
                    WhatsAppUtil.ProcessWhatsAppResponse(waModel);
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = string.Format("ProcessWebhookWhatsAppMessage error: {0}", ex.Message),
                    });
                    ctx.SaveChanges();
                }
                var exMessage = Utils.ProcessException(ex);
                throw new HttpResponseException(new Utils().GetErrorPostMessage(exMessage));
            }
        }

        public List<WhatsAppMessageTemplates> GetWhatsAppMessageTemplates()
        {
            List<WhatsAppMessageTemplates> lstTemplates = new List<WhatsAppMessageTemplates>();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var watemplates = ctx.tblWhatsAppMessageTemplates.ToList();
                    foreach (var template in watemplates)
                    {
                        lstTemplates.Add(new WhatsAppMessageTemplates
                        {
                            Id = template.intId,
                            Trigger = template.txtTrigger.Split(','),
                            Comment = template.txtComment,
                            Title = template.txtTitle,
                            Footer = template.txtFooter,
                            Buttons = template.txtButtons,
                            MessageType = template.txtMessageType,
                            CallBackAction = template.txtCallBackAction,
                            TemplateName = template.txtTemplateName,
                            Internal = template.bitInternal.HasValue ? template.bitInternal.Value : false,
                            Urls = template.txtUrls,
                            ListButton = template.txtListButton,
                            ListSections = template.txtListSections,
                            ListSectionRows = template.txtListSectionRows,
                        });
                    }
                }
                catch (Exception ex) { }
            }
            return lstTemplates;
        }

        public WhatsAppConfigSettingsForTemplate GetWhatsAppConfigSettings()
        {
            WhatsAppConfigSettingsForTemplate objres = new WhatsAppConfigSettingsForTemplate();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var watemplates = ctx.tblConfigSettings.ToList();
                    foreach (var template in watemplates)
                    {
                        if (template.txtConfigName == "WhatsAppTemplateUseButton")
                            if (Convert.ToBoolean(template.txtConfigValue))
                                objres.useButton = true;
                        if (template.txtConfigName == "WhatsAppTemplateUseList")
                            if (Convert.ToBoolean(template.txtConfigValue))
                                objres.useList = true;
                        if (template.txtConfigName == "WhatsAppTemplateUseURL")
                            if (Convert.ToBoolean(template.txtConfigValue))
                                objres.useURL = true;
                        if (template.txtConfigName == "UseRocketAPIForWhatsApp")
                            if (Convert.ToBoolean(template.txtConfigValue))
                                objres.useRocket = true;
                        if (template.txtConfigName == "useChatX")
                            if (Convert.ToBoolean(template.txtConfigValue))
                                objres.useChatX = true;
                    }
                }
                catch (Exception ex) { }
            }
            return objres;
        }

        public bool SaveWhatsAppConfigSettings(WhatsAppConfigSettingsForTemplate request)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    if (request != null)
                    {
                        var watemplates = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppTemplateUseURL").FirstOrDefault();
                        watemplates.txtConfigValue = request.useURL.ToString();
                        ctx.SaveChanges();

                        var watemplates1 = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppTemplateUseButton").FirstOrDefault();
                        watemplates1.txtConfigValue = request.useButton.ToString();
                        ctx.SaveChanges();

                        var watemplates2 = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppTemplateUseList").FirstOrDefault();
                        watemplates2.txtConfigValue = request.useList.ToString();
                        ctx.SaveChanges();

                        var watemplates3 = ctx.tblConfigSettings.Where(x => x.txtConfigName == "UseRocketAPIForWhatsApp").FirstOrDefault();
                        watemplates3.txtConfigValue = request.useRocket.ToString();
                        ctx.SaveChanges();

                        //var watemplates4 = ctx.tblConfigSettings.Where(x => x.txtConfigName == "useChatX").FirstOrDefault();
                        //watemplates4.txtConfigValue = request.useChatX.ToString();
                        //ctx.SaveChanges();

                    }

                }
                catch (Exception ex) { }
            }
            return true;
        }

        public bool SaveWhatsAppMessageTemplates(WhatsAppMessageTemplates template)
        {
            bool isSuccess = false;
            if (template != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        var watemplate = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.intId == template.Id);

                        if (watemplate != null)
                        {
                            watemplate.txtTrigger = string.Join(",", template.Trigger);
                            watemplate.txtTitle = template.Title;
                            watemplate.txtMessageType = template.MessageType;
                            watemplate.txtCallBackAction = template.CallBackAction;
                            watemplate.txtComment = template.Comment;
                            watemplate.txtButtons = template.Buttons;
                            watemplate.txtFooter = template.Footer;
                            watemplate.txtTemplateName = template.TemplateName;
                            watemplate.bitInternal = template.Internal;
                            watemplate.txtUrls = template.Urls;
                            watemplate.txtListButton = template.ListButton;
                            watemplate.txtListSections = template.ListSections;
                            watemplate.txtListSectionRows = template.ListSectionRows;

                            ctx.SaveChanges();

                            isSuccess = true;
                        }
                        else
                        {
                            ctx.tblWhatsAppMessageTemplates.Add(new tblWhatsAppMessageTemplates()
                            {
                                txtTrigger = string.Join(",", template.Trigger),
                                txtTitle = template.Title,
                                txtMessageType = template.MessageType,
                                txtCallBackAction = template.CallBackAction,
                                txtComment = template.Comment,
                                txtButtons = template.Buttons,
                                txtFooter = template.Footer,
                                txtTemplateName = template.TemplateName,
                                bitInternal = template.Internal,
                                txtUrls = template.Urls,
                                txtListButton = template.ListButton,
                                txtListSections = template.ListSections,
                                txtListSectionRows = template.ListSectionRows,
                            });
                            ctx.SaveChanges();
                            isSuccess = true;

                        }
                    }
                    catch (Exception ex)
                    {
                        isSuccess = false;
                    }
                }
            }
            return isSuccess;
        }

        public bool DeleteWhatsAppMessageTemplates(int templateId)
        {
            if (templateId > 0)
            {
                using (var ctx = new FoneClubeContext())
                {
                    var deleted = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.intId == templateId);
                    ctx.tblWhatsAppMessageTemplates.Remove(deleted);
                    ctx.SaveChanges();

                    return true;
                }
            }
            return false;
        }

        public void SendMessageInfoToAdminAndParent(SendMessageToAdminAndParent objSend)
        {
            var wmessage = new WhatsAppMessage();
            if (objSend.Type == "RegistrationSuccess")
                wmessage.Message = string.Format("Cliente {0} registrado com sucesso. Por favor, encontre abaixo os detalhes do usuário registrado \n CPF:{1} \n WhatsApp Número: {2} \n Convidado por:{3}", objSend.Name, objSend.CPF, objSend.WhatsAppNumber, objSend.ParentName);
            else if (objSend.Type == "CartItemUpdate")
                wmessage.Message = objSend.Message;
            wmessage.ClientIds = AdminMsgsTo + (string.IsNullOrEmpty(objSend.ParentWhatsAppNumber) ? "" : "," + objSend.ParentWhatsAppNumber);
            SendMessage(wmessage);
        }

        public void SendMessageInfoToAdmin(SendMessageToAdmin objSend)
        {
            var wmessage = new WhatsAppMessage();
            if (objSend.Type == "UserPaid")
                wmessage.Message = string.Format("*Pagamento recebido de:*  \n\n*Cliente:* {0}, \n*Pagar.me ID:* {7} \n*WhatsApp Número:* {2}, \n*CPF:* {1}, \n \n*Vencimento:* {4}, \n*Vigencia: {3}*, \n*Total: {5}* \n*Tipo: {8}*\n\n👍FoneClube agradece pela atenção! \n {6}", objSend.Name, objSend.CPF, objSend.WhatsAppNumber, objSend.Vigencia, objSend.Vencimento, objSend.Amount, objSend.CurrentDateTime, objSend.TransactionId, objSend.Tipo);
            wmessage.ClientIds = AdminMsgsTo;
            SendMessage(wmessage);
        }
        public void SendMessageInfoToAdmin(string objSend)
        {
            var wmessage = new WhatsAppMessage();
            wmessage.Message = objSend;
            wmessage.ClientIds = AdminMsgsTo;
            SendMessage(wmessage);
        }

        public string CheckUserStatus(string phone)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    string countryCode = phone.Substring(0, 2);
                    string areaCode = phone.Substring(2, 2);
                    string phoneNumber = phone.Substring(4);
                    var phones = ctx.tblPersonsPhones.Where(x => x.intCountryCode.Value.ToString() == countryCode
                    && x.intDDD.Value.ToString() == areaCode && x.intPhone.Value.ToString() == phoneNumber).ToList();

                    if (phones != null && phones.Count > 0)
                    {
                        var orderbyPerson = phones.OrderBy(x => x.intIdPerson).FirstOrDefault();
                        var personName = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == orderbyPerson.intIdPerson).txtName;
                        var chargeHistory = ctx.tblChargingHistory.Where(x => x.intIdCustomer == orderbyPerson.intIdPerson).FirstOrDefault();
                        if (chargeHistory != null)
                        {
                            return "Client|" + personName;
                        }
                        else
                        {
                            return "Registered|" + personName;
                        }
                    }
                    else
                    {
                        return "Unregistered|";
                    }
                }
                catch (Exception ex)
                {
                    return "Error|";
                }
            }
        }

        public bool SendMessage(WhatsAppMessage model)
        {
            try
            {
                if (GetWhatsAppConfigSettings().useChatX)
                {
                    return new SuperZapBotAccess().SendMessageChatXNew(model);
                }
                else if (GetWhatsAppConfigSettings().useRocket)
                {
                    return SendMessageViaRocketAPI(model);
                }
                else
                {
                    SendTextRequest sendTextRequest = new SendTextRequest();
                    sendTextRequest.phone = model.ClientIds;
                    sendTextRequest.message = model.Message;
                    sendTextRequest.isGroup = false;

                    var JsonStr = JsonConvert.SerializeObject(sendTextRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("api/{0}/send-message", SessionId);

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "WhatsApp SendMessage response:" + contentresponse });
                            ctx.SaveChanges();
                        }

                        WhatsAppSendResponseModel waModel = JsonConvert.DeserializeObject<WhatsAppSendResponseModel>(contentresponse);
                        // Verification  
                        if (response.IsSuccessStatusCode && waModel.status == "success")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "WhatsApp SendMessage error:" + ex.Message });
                    ctx.SaveChanges();
                }
                return false;
            }
        }

        public List<SendMessageChatXResponse> SendMessageChatX(SendMessageChatXRequest model)
        {
            try
            {
                if (GetWhatsAppConfigSettings().useChatX)
                {
                    WhatsAppMessage appMessage = new WhatsAppMessage()
                    {
                        Message = model.Message,
                        ClientIds = model.PhoneNumbers
                    };
                    return new SuperZapBotAccess().SendMessageChatXNewMulti(appMessage);
                }
            }
            catch (Exception ex) { }
            return null;
        }

        public List<string> SendImage(string phoneNumber, string img)
        {
            List<string> lstReplies = new List<string>();
            try
            {
                SendImageRequest sendButtonsRequest = new SendImageRequest();
                sendButtonsRequest.phone = phoneNumber;
                sendButtonsRequest.filename = "ActivationCode";
                sendButtonsRequest.base64 = img;

                var JsonStr = JsonConvert.SerializeObject(sendButtonsRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                string apiMethod = string.Format("api/{0}/send-image", SessionId);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // Verification  
                    if (response.IsSuccessStatusCode)
                    {
                        lstReplies.Add("Y");
                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Message sent to " + phoneNumber + " with message :" + "" });
                            ctx.SaveChanges();
                        }
                    }
                    else
                    {
                        lstReplies.Add("N");

                        LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                        {
                            intIdCharge = 0,
                            intIdPerson = 0,
                            txtPhoneNumber = phoneNumber,
                            dteDateTime = DateTime.Now,
                            txtError = contentresponse,
                            txtMessage = "",
                            bitChargeSummary = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Sending message with button to " + phoneNumber + " with message :" + "" + "failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }

                LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                {
                    intIdCharge = 0,
                    intIdPerson = 0,
                    txtPhoneNumber = phoneNumber,
                    dteDateTime = DateTime.Now,
                    txtError = ex.Message,
                    txtMessage = "SendImage",
                    bitChargeSummary = false
                });
            }
            return lstReplies;
        }

        public List<string> SendMessageWithButton(WhatsAppMessage model)
        {
            List<string> lstReplies = new List<string>();
            try
            {
                var phoneNumbers = model.ClientIds.Split(',');
                if (phoneNumbers != null && phoneNumbers.Count() > 0)
                {
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        SendButtonsRequest sendButtonsRequest = new SendButtonsRequest();
                        sendButtonsRequest.phone = phoneNumber;
                        sendButtonsRequest.message = model.Message;

                        if (model.ButtonList != null)
                        {
                            sendButtonsRequest.options = new SendButtonOptions();
                            sendButtonsRequest.options.useTemplateButtons = true;
                            sendButtonsRequest.options.title = model.Title;
                            sendButtonsRequest.options.footer = model.Footer;

                            if (model.ButtonList.buttons != null && model.ButtonList.buttons.Count > 0)
                            {
                                sendButtonsRequest.options.buttons = new List<SendButtonOptionsReq>();
                                int icount = 1;
                                foreach (var btn in model.ButtonList.buttons)
                                {
                                    SendButtonOptionsReq obj = new SendButtonOptionsReq();
                                    obj.id = icount.ToString();
                                    obj.text = btn.text;
                                    sendButtonsRequest.options.buttons.Add(obj);
                                    icount++;
                                }
                            }
                        }

                        var JsonStr = JsonConvert.SerializeObject(sendButtonsRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                        //string apiMethod = string.Format("api/{0}/send-message", SessionId);
                        string apiMethod = string.Format("api/{0}/send-buttons", SessionId);

                        using (var client = new HttpClient())
                        {
                            // Setting Authorization.  
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                            // Setting Base address.  
                            client.BaseAddress = new Uri(BaseAddress);

                            client.DefaultRequestHeaders.Accept.Clear();

                            // Setting content type.  
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Initialization.  
                            HttpResponseMessage response = new HttpResponseMessage();

                            // HTTP GET  
                            response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                            var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            // Verification  
                            if (response.IsSuccessStatusCode)
                            {
                                lstReplies.Add("Y");
                                using (var ctx = new FoneClubeContext())
                                {
                                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Message sent to " + model.ClientIds + " with message :" + model.Message });
                                    ctx.SaveChanges();
                                }
                            }
                            else
                            {
                                lstReplies.Add("N");

                                LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                                {
                                    intIdCharge = 0,
                                    intIdPerson = 0,
                                    txtPhoneNumber = phoneNumber,
                                    dteDateTime = DateTime.Now,
                                    txtError = contentresponse,
                                    txtMessage = model.Message,
                                    bitChargeSummary = false
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Sending message with button to " + model.ClientIds + " with message :" + model.Message + "failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }

                LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                {
                    intIdCharge = 0,
                    intIdPerson = 0,
                    txtPhoneNumber = model.ClientIds,
                    dteDateTime = DateTime.Now,
                    txtError = ex.Message,
                    txtMessage = model.Message,
                    bitChargeSummary = false
                });
            }
            return lstReplies;
        }

        public List<string> SendMessageWithButtonList(WhatsAppMessage model)
        {
            List<string> lstReplies = new List<string>();
            SendListRequest objSendListRequest = new SendListRequest();
            try
            {
                var phoneNumbers = model.ClientIds.Split(',');
                if (phoneNumbers != null && phoneNumbers.Count() > 0)
                {
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        objSendListRequest.phone = phoneNumber;
                        objSendListRequest.description = model.Message;
                        if (model.SendList != null)
                        {
                            objSendListRequest.buttonText = model.SendList.ButtonText;
                            objSendListRequest.sections = new List<Section>();
                            foreach (var sec in model.SendList.Sections)
                            {
                                var section = new Section();
                                section.title = sec.Title;
                                section.rows = new List<Row>();

                                int iCount = 0;
                                foreach (var row in sec.Rows)
                                {
                                    section.rows.Add(new Row()
                                    {
                                        description = row.Description,
                                        title = row.Title,
                                        rowId = iCount.ToString()
                                    }); ;
                                    iCount++;
                                }
                                objSendListRequest.sections.Add(section);
                            }
                        }

                        var JsonStr = JsonConvert.SerializeObject(objSendListRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "List contentresponse: " + JsonStr });
                            ctx.SaveChanges();
                        }

                        StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                        string apiMethod = string.Format("api/{0}/send-list-message", SessionId);

                        using (var client = new HttpClient())
                        {
                            // Setting Authorization.  
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                            // Setting Base address.  
                            client.BaseAddress = new Uri(BaseAddress);

                            client.DefaultRequestHeaders.Accept.Clear();

                            // Setting content type.  
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Initialization.  
                            HttpResponseMessage response = new HttpResponseMessage();

                            // HTTP GET  
                            response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                            var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            using (var ctx = new FoneClubeContext())
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "List contentresponse: " + contentresponse });
                                ctx.SaveChanges();
                            }
                            if (response.IsSuccessStatusCode)
                            {
                                lstReplies.Add("Y");
                            }
                            else
                            {
                                lstReplies.Add("N");
                            }

                            // Verification  
                            if (response.IsSuccessStatusCode)
                            {
                                lstReplies.Add("Y");
                                using (var ctx = new FoneClubeContext())
                                {
                                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Message sent to " + model.ClientIds + " with message :" + model.Message });
                                    ctx.SaveChanges();
                                }
                            }
                            else
                            {
                                lstReplies.Add("N");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Sending message with list to " + model.ClientIds + " with message :" + model.Message + "failed with error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return lstReplies;
        }

        public List<string> SendMessageWithButtonForChargeSummary(WhatsAppMessage model, int chargeId, int personId)
        {
            List<string> lstReplies = new List<string>();
            string phone = string.Empty;
            try
            {
                var phoneNumbers = model.ClientIds.Split(',');
                if (phoneNumbers != null && phoneNumbers.Count() > 0)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var idsToDelete = ctx.tblWhatsAppStatus.Where(x => x.intIdCharge == chargeId && x.intIdPerson == personId);
                        ctx.tblWhatsAppStatus.RemoveRange(idsToDelete);
                        ctx.SaveChanges();
                    }
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        phone = phoneNumber;
                        SendButtonsRequest sendButtonsRequest = new SendButtonsRequest();
                        sendButtonsRequest.phone = phoneNumber;
                        sendButtonsRequest.message = model.Message;

                        if (model.ButtonList != null)
                        {
                            sendButtonsRequest.options = new SendButtonOptions();
                            sendButtonsRequest.options.useTemplateButtons = true;
                            sendButtonsRequest.options.title = model.Title;
                            sendButtonsRequest.options.footer = model.Footer;

                            if (model.ButtonList.buttons != null && model.ButtonList.buttons.Count > 0)
                            {
                                sendButtonsRequest.options.buttons = new List<SendButtonOptionsReq>();
                                int icount = 1;
                                foreach (var btn in model.ButtonList.buttons)
                                {
                                    SendButtonOptionsReq obj = new SendButtonOptionsReq();
                                    obj.id = icount.ToString();
                                    obj.text = btn.text;
                                    sendButtonsRequest.options.buttons.Add(obj);
                                    icount++;
                                }
                            }
                        }

                        var JsonStr = JsonConvert.SerializeObject(sendButtonsRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                        //string apiMethod = string.Format("api/{0}/send-message", SessionId);
                        string apiMethod = string.Format("api/{0}/send-buttons", SessionId);

                        using (var client = new HttpClient())
                        {
                            // Setting Authorization.  
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                            // Setting Base address.  
                            client.BaseAddress = new Uri(BaseAddress);

                            client.DefaultRequestHeaders.Accept.Clear();

                            // Setting content type.  
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Initialization.  
                            HttpResponseMessage response = new HttpResponseMessage();

                            // HTTP GET  
                            response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                            var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            using (var ctx = new FoneClubeContext())
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "WPPConnect content response: summary button message" + contentresponse });
                                ctx.SaveChanges();
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                lstReplies.Add("Y");
                            }
                            else
                            {
                                lstReplies.Add("N");
                            }

                            WhatsAppSendResponseModel waModel = JsonConvert.DeserializeObject<WhatsAppSendResponseModel>(contentresponse);
                            using (var ctx = new FoneClubeContext())
                            {
                                if (waModel != null && waModel.response != null)
                                {
                                    ctx.tblWhatsAppStatus.Add(new tblWhatsAppStatus()
                                    {
                                        intIdCharge = chargeId,
                                        intIdPerson = personId,
                                        intStatus = waModel.status == "success" ? 1 : -1,
                                        dteMsgSentDateTime = DateTime.Now,
                                        txtPhoneNumber = waModel.response[0].to,
                                        intIdChat = waModel.response[0].id
                                    });
                                    ctx.SaveChanges();
                                }
                            }


                            lstReplies.Add("N");
                            LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                            {
                                intIdCharge = chargeId,
                                intIdPerson = personId,
                                txtPhoneNumber = phoneNumber,
                                dteDateTime = DateTime.Now,
                                txtError = waModel.status,
                                txtMessage = model.Message,
                                bitChargeSummary = true
                            });

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageWithButtonForChargeSummary Error" + ex.ToString() });
                    ctx.SaveChanges();

                    LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                    {
                        intIdCharge = chargeId,
                        intIdPerson = personId,
                        txtPhoneNumber = phone,
                        dteDateTime = DateTime.Now,
                        txtError = ex.Message,
                        txtMessage = model.Message,
                        bitChargeSummary = true
                    });
                }
            }
            return lstReplies;
        }

        public List<string> SendMessageWithButtonUrl(WhatsAppMessage model, int chargeId, int personId)
        {
            string phone = string.Empty;
            List<string> lstReplies = new List<string>();
            try
            {
                var phoneNumbers = model.ClientIds.Split(',');
                if (phoneNumbers != null && phoneNumbers.Count() > 0)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var idsToDelete = ctx.tblWhatsAppStatus.Where(x => x.intIdCharge == chargeId && x.intIdPerson == personId);
                        if (idsToDelete != null)
                        {
                            ctx.tblWhatsAppStatus.RemoveRange(idsToDelete);
                            ctx.SaveChanges();
                        }
                    }
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        phone = phoneNumber;
                        SendButtonsUrlRequest sendButtonsUrlRequest = new SendButtonsUrlRequest();
                        sendButtonsUrlRequest.phone = phoneNumber;
                        sendButtonsUrlRequest.message = model.Message;

                        if (model.UrlList != null)
                        {
                            sendButtonsUrlRequest.options = new SendButtonUrlOptions();

                            sendButtonsUrlRequest.options.useTemplateButtons = true;
                            sendButtonsUrlRequest.options.title = model.Title;
                            sendButtonsUrlRequest.options.footer = model.Footer;

                            if (model.UrlList.buttonsUrl != null && model.UrlList.buttonsUrl.Count > 0)
                            {
                                sendButtonsUrlRequest.options.buttons = new List<System.Dynamic.ExpandoObject>();
                                int icount = 1;
                                foreach (var btn in model.UrlList.buttonsUrl)
                                {
                                    dynamic obj = new System.Dynamic.ExpandoObject();
                                    obj.id = icount.ToString();
                                    obj.text = btn.text;
                                    if (!string.IsNullOrEmpty(btn.url))
                                        obj.url = btn.url;
                                    sendButtonsUrlRequest.options.buttons.Add(obj);
                                    icount++;
                                }
                            }
                        }

                        var JsonStr = JsonConvert.SerializeObject(sendButtonsUrlRequest, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                        //string apiMethod = string.Format("api/{0}/send-message", SessionId);
                        string apiMethod = string.Format("api/{0}/send-buttons", SessionId);

                        using (var client = new HttpClient())
                        {
                            // Setting Authorization.  
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                            // Setting Base address.  
                            client.BaseAddress = new Uri(BaseAddress);

                            client.DefaultRequestHeaders.Accept.Clear();

                            // Setting content type.  
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Initialization.  
                            HttpResponseMessage response = new HttpResponseMessage();

                            // HTTP GET  
                            response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                            var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            using (var ctx = new FoneClubeContext())
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageWithButtonUrl WPPConnect content response: summary button message" + contentresponse });
                                ctx.SaveChanges();
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                lstReplies.Add("Y");
                            }
                            else
                            {
                                lstReplies.Add("N");
                            }

                            WhatsAppSendButtonUrlResponseModel waModel = JsonConvert.DeserializeObject<WhatsAppSendButtonUrlResponseModel>(contentresponse);

                            if (waModel != null && waModel.response != null)
                            {
                                using (var ctx = new FoneClubeContext())
                                {
                                    ctx.tblWhatsAppStatus.Add(new tblWhatsAppStatus()
                                    {
                                        intIdCharge = chargeId,
                                        intIdPerson = personId,
                                        intStatus = waModel.status == "success" ? 1 : -1,
                                        dteMsgSentDateTime = DateTime.Now,
                                        txtPhoneNumber = phone,
                                        intIdChat = waModel.response[0]
                                    });
                                    ctx.SaveChanges();
                                }
                            }

                            // Verification  

                            if (waModel.status.ToLower() != "success")
                            {
                                using (var ctx = new FoneClubeContext())
                                {
                                    LogFailedWhatsAppMessages(new tblWhatsAppFailedStatus()
                                    {
                                        intIdCharge = chargeId,
                                        intIdPerson = personId,
                                        txtPhoneNumber = phoneNumber,
                                        dteDateTime = DateTime.Now,
                                        txtError = waModel.status,
                                        txtMessage = model.Message,
                                        bitChargeSummary = chargeId > 0 ? true : false
                                    });
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageWithButtonUrl Error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return lstReplies;
        }

        public string ValidatePhoneNumbers(string phoneNumbers)
        {
            var phoneNumArr = phoneNumbers.Split(',');
            List<string> validPhones = new List<string>();
            List<string> invalidPhones = new List<string>();
            try
            {
                foreach (var phone in phoneNumArr)
                {
                    if (!string.IsNullOrEmpty(phone))
                    {
                        string apiMethod = string.Format("api/{0}/check-number-status/{1}", SessionId, phone.Trim());
                        using (var client = new HttpClient())
                        {
                            // Setting Authorization.  
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                            // Setting Base address.  
                            client.BaseAddress = new Uri(BaseAddress);

                            client.DefaultRequestHeaders.Accept.Clear();

                            // Setting content type.  
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Initialization.  
                            HttpResponseMessage response = new HttpResponseMessage();

                            // HTTP GET  
                            response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                            var contentresponse = response.Content.ReadAsAsync<WANumberCheckResponse>().GetAwaiter().GetResult();

                            if (contentresponse != null)
                            {
                                // Verification  
                                if (Convert.ToString(contentresponse.status) == "success")
                                {
                                    if (contentresponse.response != null && contentresponse.response.numberExists)
                                    {
                                        validPhones.Add(phone.Trim());
                                    }
                                    else
                                    {
                                        invalidPhones.Add(phone.Trim());
                                    }
                                }
                                else
                                {
                                    invalidPhones.Add(phone.Trim());
                                }
                            }
                            else
                            {
                                invalidPhones.Add(phone.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            var result = string.Join(",", validPhones) + "|" + string.Join(",", invalidPhones);
            return result;
        }

        public bool ExecuteSchduleWhatsAppMessage(int days)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = string.Format("WhatsApp Automatic message for {0} days: ", days)
                    });

                    ctx.SaveChanges();

                    var whatsAppTemplates = ctx.tblWhatsAppMessageTemplates.ToList();
                    var clients = ctx.tblPersons.ToList();

                    var currentDate = DateTime.Now.AddDays(days).Date;
                    var today = DateTime.Now;
                    //only for pix and boleto, CC excluded
                    var cobrancas = ctx.tblChargingHistory
                        .Where(x => DbFunctions.TruncateTime(x.dteDueDate) == DbFunctions.TruncateTime(currentDate) && x.bitActive.HasValue && x.bitActive.Value && x.intIdPaymentType.Value != 1).ToList();

                    if (cobrancas != null && cobrancas.Count > 0)
                    {
                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = string.Format("WhatsApp Automatic message for {0} days {1} count: ", days, cobrancas.Count)
                        });

                        ctx.SaveChanges();

                        var transactions = new TransactionAccess().GetAllLastTransactions();

                        foreach (var charge in cobrancas)
                        {
                            var person = clients.FirstOrDefault(x => x.intIdPerson == charge.intIdCustomer);
                            var transactionPagarme = transactions.FirstOrDefault(l => l.intIdTransaction == charge.intIdTransaction);
                            if (transactionPagarme != null)
                            {
                                if (transactionPagarme.txtOutdadetStatus != "Paid")
                                {
                                    if (days >= 0)
                                    {
                                        string triggerName = string.Format("t.{0}Lembrete", days).ToLower();

                                        //-1 day trigger

                                        if (!string.IsNullOrEmpty(charge.txtWAPhones))
                                        {
                                            var messageTemplate = whatsAppTemplates.FirstOrDefault(x => x.txtTrigger.ToLower() == triggerName);
                                            WhatsAppMessage whatsAppMessage = Helper.FormatRequest(messageTemplate, charge, person, transactionPagarme);

                                            if (!string.IsNullOrEmpty(messageTemplate.txtMessageType))
                                            {
                                                switch (messageTemplate.txtMessageType.ToLower())
                                                {
                                                    case "button":
                                                        SendMessageWithButton(whatsAppMessage);
                                                        break;
                                                    case "url":
                                                        SendMessageWithButtonUrl(whatsAppMessage, 0, 0);
                                                        break;
                                                    case "list":
                                                        SendMessageWithButtonList(whatsAppMessage);
                                                        break;
                                                    case "text":
                                                        SendMessage(whatsAppMessage);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                            //Helper.SendChargeSummary(charge);
                                        }
                                    }
                                    else
                                    {
                                        //greater than 1 day trigger
                                        string triggerName = string.Format("t.atrasado{0}dia", days).ToLower();
                                        var messageTemplate = whatsAppTemplates.FirstOrDefault(x => x.txtTrigger.ToLower() == triggerName);
                                        //greater than 1 day trigger
                                        if (!string.IsNullOrEmpty(charge.txtWAPhones))
                                        {
                                            WhatsAppMessage whatsAppMessage = Helper.FormatRequest(messageTemplate, charge, person, transactionPagarme);

                                            if (!string.IsNullOrEmpty(messageTemplate.txtMessageType))
                                            {
                                                switch (messageTemplate.txtMessageType.ToLower())
                                                {
                                                    case "button":
                                                        SendMessageWithButton(whatsAppMessage);
                                                        break;
                                                    case "url":
                                                        SendMessageWithButtonUrl(whatsAppMessage, 0, 0);
                                                        break;
                                                    case "list":
                                                        SendMessageWithButtonList(whatsAppMessage);
                                                        break;
                                                    case "text":
                                                        SendMessage(whatsAppMessage);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                            //Helper.SendChargeSummary(charge);
                                        }
                                    }
                                }
                                else
                                {
                                    if (days > 0)
                                    {
                                        //-1 day trigger
                                        var messageTemplate = whatsAppTemplates.FirstOrDefault(x => x.txtTrigger.ToLower() == "t.pagamento.recebido");
                                        //greater than 1 day trigger
                                        if (!string.IsNullOrEmpty(charge.txtWAPhones))
                                        {
                                            WhatsAppMessage whatsAppMessage = Helper.FormatRequest(messageTemplate, charge, person, transactionPagarme);

                                            if (!string.IsNullOrEmpty(messageTemplate.txtMessageType))
                                            {
                                                switch (messageTemplate.txtMessageType.ToLower())
                                                {
                                                    case "button":
                                                        SendMessageWithButton(whatsAppMessage);
                                                        break;
                                                    case "url":
                                                        SendMessageWithButtonUrl(whatsAppMessage, 0, 0);
                                                        break;
                                                    case "list":
                                                        SendMessageWithButtonList(whatsAppMessage);
                                                        break;
                                                    case "text":
                                                        SendMessage(whatsAppMessage);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public string SendChargeSummary(int chargeId, string phonenumbers)
        {
            try
            {
                //var validPhones = ValidatePhoneNumbers(phonenumbers);
                if (!string.IsNullOrEmpty(phonenumbers))
                {
                    //var validNumbers = validPhones.Split('|')[0];

                    using (var ctx = new FoneClubeContext())
                    {
                        var chargingHistory = ctx.tblChargingHistory.FirstOrDefault(x => x.intId == chargeId);
                        Helper.SendChargeSummaryText(chargingHistory, phonenumbers);
                        return Helper.SendChargeSummary(chargingHistory, phonenumbers);
                    }
                }
                return "Error";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string SendMarketingMessage(GenericTemplate marketing)
        {
            try
            {
                return Helper.SendMarketingMessage(marketing);
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string SendGenericMessage(GenericTemplate marketing)
        {
            try
            {
                return Helper.SendGenericMessage(marketing);
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string SendWelcomeMessage(WelcomeTemplate welcome)
        {
            try
            {
                return Helper.SendWelcomeMessage(welcome);
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string SendWhatsAppMessageCCRefused(Person person, ref bool is3rdReminder)
        {
            string triggerName = "ccrefused";
            is3rdReminder = false;
            try
            {
                if (person != null && person.Charging != null)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        DateTime vingenciaday = new DateTime(2000, 1, 1, 0, 0, 0);
                        try
                        {
                            vingenciaday = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                        }
                        catch (Exception)
                        {
                            vingenciaday = new DateTime(2000, 1, 1, 0, 0, 0);
                        }

                        var tblRef = ctx.tblCCRefusedLog.FirstOrDefault(x => x.intIdPerson == person.Id && x.dteVigencia == vingenciaday);
                        if (tblRef == null)
                        {
                            ctx.tblCCRefusedLog.Add(new tblCCRefusedLog()
                            {
                                intIdPerson = person.Id,
                                txtChargeIds = person.Charging.Id.ToString(),
                                dteVigencia = vingenciaday,
                                dteCreate = DateTime.Now,
                                intReminderCount = 1,
                                IsActive = true
                            });
                            ctx.SaveChanges();
                            triggerName = "ccrefused";
                        }
                        else if (tblRef != null && tblRef.IsActive && tblRef.intReminderCount == 1)
                        {
                            triggerName = "CreditCardRefused-2nd";
                            tblRef.intReminderCount = tblRef.intReminderCount + 1;
                            ctx.SaveChanges();
                        }
                        else if (tblRef != null && tblRef.IsActive && tblRef.intReminderCount == 2)
                        {
                            triggerName = "CreditCardRefused-3nd";
                            tblRef.intReminderCount = tblRef.intReminderCount + 1;
                            ctx.SaveChanges();

                            is3rdReminder = true;
                        }

                        var personData = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == person.Id);
                        var whatsAppTemplates = ctx.tblWhatsAppMessageTemplates.ToList();

                        //-1 day trigger
                        var messageTemplate = whatsAppTemplates.FirstOrDefault(x => x.txtTrigger.ToLower() == triggerName);

                        var phone = !string.IsNullOrEmpty(person.Charging.TxtWAPhones) ? person.Charging.TxtWAPhones : personData.txtDefaultWAPhones;

                        //greater than 1 day trigger
                        if (!string.IsNullOrEmpty(phone))
                        {

                            DateTime vencimentotoday = DateTime.Now.Date;

                            var vencimentoday = vencimentotoday.Date.ToString(@"dd", new CultureInfo("PT-br"));
                            var vencimentomonthTemp = vencimentotoday.Date.ToString(@"MMMM", new CultureInfo("PT-br"));
                            var vencimentomonth = char.ToUpper(vencimentomonthTemp[0]) + vencimentomonthTemp.Substring(1);
                            var vencimento = (!string.IsNullOrEmpty(vencimentoday) && !string.IsNullOrEmpty(vencimentomonth)) ? string.Format(@"{0} de {1}", vencimentoday, vencimentomonth) : string.Empty;

                            var vigenciaYear = vingenciaday.Date.ToString(@"yyyy", new CultureInfo("PT-br"));
                            var vigenciamonthTemp = vingenciaday.Date.ToString(@"MMMM", new CultureInfo("PT-br"));
                            var vigenciamonth = char.ToUpper(vigenciamonthTemp[0]) + vigenciamonthTemp.Substring(1);
                            var vigencia = (!string.IsNullOrEmpty(vigenciamonth) && !string.IsNullOrEmpty(vigenciaYear)) ? string.Format(@"{0} de {1}", vigenciamonth, vigenciaYear) : string.Empty;

                            var amount = float.Parse(person.Charging.Ammount) / 100;

                            ButtonData objButtonData = new ButtonData();
                            objButtonData.title = messageTemplate.txtTitle;
                            objButtonData.footer = messageTemplate.txtFooter;
                            objButtonData.buttons = new List<ButtonRows>();
                            foreach (var btn in Helper.ReplaceMessage(messageTemplate.txtButtons, null, null, null).Split(',').ToList())
                            {
                                ButtonRows buttonRows = new ButtonRows();
                                buttonRows.text = btn;
                                objButtonData.buttons.Add(buttonRows);
                            }

                            WhatsAppMessage whatsAppMessage = new WhatsAppMessage()
                            {
                                ClientIds = person.Charging.TxtWAPhones + ",5521981908190",
                                Title = messageTemplate.txtTitle,
                                Footer = messageTemplate.txtFooter,
                                Message = messageTemplate.txtComment.Replace("namevariable", personData.txtName).Replace("commentvariable", person.Charging.ChargingComment).Replace("vencimentovariable", vencimento).Replace("vigenciavariable", vigencia).Replace("amountvariable", amount.ToString()),
                                ButtonList = objButtonData
                            };
                            if (!string.IsNullOrEmpty(messageTemplate.txtMessageType))
                            {
                                switch (messageTemplate.txtMessageType.ToLower())
                                {
                                    case "button":
                                        SendMessageWithButton(whatsAppMessage);
                                        break;
                                    case "url":
                                        SendMessageWithButtonUrl(whatsAppMessage, 0, personData.intIdPerson);
                                        break;
                                    case "list":
                                        SendMessageWithButtonList(whatsAppMessage);
                                        break;
                                    case "text":
                                        SendMessage(whatsAppMessage);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            ctx.tblLog.Add(new tblLog
                            {
                                dteTimeStamp = DateTime.Now,
                                intIdTipo = 1,
                                txtAction = string.Format("WhatsApp Automatic message for CC Refused - Not send for {0}: ", person.Id)
                            });
                            ctx.SaveChanges();
                        }
                    }
                    return "Sent";
                }
                else
                    return "Error";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string SendPaymentConfirmationMsgs(long transactionId, int customerId = 0)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var messageTemplate = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.txtTrigger.ToLower() == "t.pagamento.recebido");
                    var person = (from c in ctx.tblChargingHistory
                                  join p in ctx.tblPersons on c.intIdCustomer equals p.intIdPerson
                                  where c.intIdTransaction.Value == transactionId
                                  select p).FirstOrDefault();
                    var ch = ctx.tblChargingHistory.ToList().Where(x => x.intIdTransaction == transactionId).LastOrDefault();

                    if (person != null && ch != null)
                    {
                        var foneclubePhone = ctx.tblPersonsPhones.Where(x => x.intIdPerson == person.intIdPerson && x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value).FirstOrDefault();
                        var pagarme = ctx.tblFoneclubePagarmeTransactions.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                        WhatsAppMessage whatsAppMessage = Helper.FormatRequest(messageTemplate, ch, person, pagarme, null, string.IsNullOrEmpty(person.txtDefaultWAPhones) ? string.Concat(foneclubePhone.intDDD, foneclubePhone.intPhone) : person.txtDefaultWAPhones);

                        if (!string.IsNullOrEmpty(messageTemplate.txtMessageType))
                        {
                            bool status = false;
                            switch (messageTemplate.txtMessageType.ToLower())
                            {
                                case "button":
                                    SendMessageWithButton(whatsAppMessage);
                                    break;
                                case "url":
                                    SendMessageWithButtonUrl(whatsAppMessage, 0, person.intIdPerson);
                                    break;
                                case "list":
                                    SendMessageWithButtonList(whatsAppMessage);
                                    break;
                                case "text":
                                    status = SendMessage(whatsAppMessage);
                                    break;
                                default:
                                    break;
                            }

                            if (status)
                            {
                                var updateStatus = ctx.tblPagarmeTransactionsUserUpdateStatus.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                                if (updateStatus != null)
                                {
                                    updateStatus.bitUserUpdateStatus = true;
                                }
                                else
                                {
                                    ctx.tblPagarmeTransactionsUserUpdateStatus.Add(new tblPagarmeTransactionsUserUpdateStatus()
                                    {
                                        intIdTransaction = transactionId,
                                        intIdCustomer = customerId,
                                        bitUserUpdateStatus = true
                                    });
                                }
                                ctx.SaveChanges();


                                var ccHistory = ctx.tblChargingHistory.ToList().LastOrDefault(x => x.intIdTransaction == transactionId);
                                if (ccHistory != null)
                                {
                                    var personData = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == ccHistory.intIdCustomer);

                                    if (personData != null)
                                    {
                                        var iccid = !string.IsNullOrEmpty(ccHistory.txtInstaRegsiterData) ? ccHistory.txtInstaRegsiterData.Split('#')[0].Split('|')[0] : "-";
                                        var vigencia = ccHistory.dteValidity.HasValue ? ccHistory.dteValidity.Value.ToString(@"dd MMMM yyyy", new CultureInfo("PT-br")) : "";
                                        var vencimento = ccHistory.dteDueDate.HasValue ? ccHistory.dteDueDate.Value.ToString(@"dd MMMM yyyy", new CultureInfo("PT-br")) : "";
                                        var amount = ccHistory.txtAmmountPayment.Insert(ccHistory.txtAmmountPayment.Length - 2, ".");

                                        string tipo = "-";
                                        switch (ccHistory.intIdPaymentType.Value)
                                        {
                                            case 1:
                                                {
                                                    tipo = "Cartão " + pagarme.txtCard_last_digits;
                                                }
                                                break;
                                            case 2:
                                                {
                                                    tipo = "Boleto";
                                                }
                                                break;
                                            case 3:
                                                {
                                                    tipo = "Pix";
                                                }
                                                break;
                                        }

                                        var objSend = new SendMessageToAdmin()
                                        {
                                            Type = "UserPaid",
                                            TransactionId = transactionId.ToString(),
                                            Name = personData.txtName,
                                            CPF = personData.txtDocumentNumber,
                                            WhatsAppNumber = personData.txtDefaultWAPhones,
                                            ICCID = iccid,
                                            Vigencia = vigencia,
                                            Vencimento = vencimento,
                                            Amount = "R$ " + amount,
                                            CurrentDateTime = DateTime.Now.ToString("dd/MMM/yy hh:mm"),
                                            Tipo = tipo
                                        };
                                        SendMessageInfoToAdmin(objSend);

                                    }
                                }
                            }
                            else
                            {
                                //Failed to sent customer and then disable notification to admin - Send only one msg to admin
                                var updateStatus = ctx.tblPagarmeTransactionsUserUpdateStatus.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                                if (updateStatus != null)
                                {
                                    updateStatus.bitUserUpdateStatus = true;
                                }
                                else
                                {
                                    SendMessage(new WhatsAppMessage()
                                    {
                                        ClientIds = AdminMsgsTo,
                                        Message = string.Format("Payment received message not send to client *{0}* due to invalid number :{1}", person.txtName, string.IsNullOrEmpty(person.txtDefaultWAPhones) ? string.Concat(foneclubePhone.intDDD, foneclubePhone.intPhone) : person.txtDefaultWAPhones)
                                    });

                                    ctx.tblPagarmeTransactionsUserUpdateStatus.Add(new tblPagarmeTransactionsUserUpdateStatus()
                                    {
                                        intIdTransaction = transactionId,
                                        intIdCustomer = customerId,
                                        bitUserUpdateStatus = true
                                    });
                                }
                                ctx.SaveChanges();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "SendPaymentConfirmationMsgs error:" + ex.ToString());
            }
            return "Sent";
        }

        public string UpdateWhatsAppStatus(WhatsAppStatus status)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var oStatus = ctx.tblWhatsAppStatus.FirstOrDefault(x => x.intIdCharge == status.ChargeId && x.txtPhoneNumber == status.Phone);
                    if (oStatus != null)
                    {
                        string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                        var dateTime = DateTime.ParseExact(status.AckDateTime, formats, new CultureInfo("pt-BR"), DateTimeStyles.None);
                        switch (status.Ack)
                        {
                            case 1:
                                oStatus.dteMsgSentDateTime = dateTime;
                                break;
                            case 2:
                                oStatus.dteMsgReceivedDateTime = dateTime;
                                break;
                            case 3:
                                oStatus.dteMsgReadDateTime = dateTime;
                                break;
                            default:
                                break;
                        }
                        oStatus.intStatus = status.Ack;
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = string.Format("Error occured while saving whatsapp status:" + ex.Message.ToString())
                    });
                    ctx.SaveChanges();
                }
            }
            return "Success";
        }

        public string ResendFailedMsgs()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var failedMsgs = ctx.tblWhatsAppFailedStatus.Where(x => (!x.bitResentSuccess.HasValue || !x.bitResentSuccess.Value) && (x.bitChargeSummary.HasValue && x.bitChargeSummary.Value)).GroupBy(x => new { x.intIdCharge, x.txtPhoneNumber }).ToList();

                    if (failedMsgs != null && failedMsgs.Count() > 0)
                    {
                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = string.Format("ResendFailedMsgs count: {0}: ", failedMsgs.Count())
                        });
                        ctx.SaveChanges();

                        foreach (var failed in failedMsgs)
                        {
                            if (failed.Key.intIdCharge.HasValue)
                            {
                                var status = SendChargeSummary(failed.Key.intIdCharge.Value, failed.Key.txtPhoneNumber);
                                var oStatus = ctx.tblWhatsAppFailedStatus.Where(x => x.intIdCharge == failed.Key.intIdCharge.Value && x.txtPhoneNumber == failed.Key.txtPhoneNumber);
                                if (oStatus != null)
                                {
                                    foreach (var stat in oStatus)
                                    {
                                        stat.bitResentSuccess = true;
                                    }
                                    ctx.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return "Success";
        }

        public string NonTopupInXDaysNotification()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var x32Days = ctx.GetNonTopLinesInXDays(31, 64).ToList();
                    var x65Days = ctx.GetNonTopLinesInXDays(65, 90).ToList();

                    if (x32Days != null && x32Days.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(string.Format("Linhas com possivel atraso na recarga!\n\nTotal: {0} linhas\n\n", x32Days.Count));
                        foreach (var dd in x32Days)
                        {
                            sb.Append("*Linha*: " + dd.Line + "\n" + "*Perde Numero*: " + dd.CancellationDate + "\n" + "*Cliente*: " + dd.Name + "\n*Dias*:" + dd.Days + "\n\n");
                        }
                        SendMessageInfoToAdmin(sb.ToString());
                    }
                    if (x65Days != null && x65Days.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(string.Format("Linhas que serão cancelandas e podem perder o numero nos proximos 5 dias! \n\nTotal: {0} linhas\n\n", x65Days.Count));
                        foreach (var dd in x65Days)
                        {
                            sb.Append("*Linha*: " + dd.Line + "\n" + "*Perde Numero*: " + dd.CancellationDate + "\n" + "*Cliente*: " + dd.Name + "\n*Dias*:" + dd.Days + "\n\n");
                        }
                        SendMessageInfoToAdmin(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return "Success";
        }

        public string NotifyInternationSalesDay()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    #region Today's Count
                    var listToday = ctx.GetFacilSaleStats(1).ToList();
                    var listTodayPercent = ctx.GetFacilSaleStatsPercentage(1).ToList();
                    var listTodayPlansPercent = ctx.GetFacilSalePlanPercentage(1).ToList();
                    if (listToday != null && listToday.Count > 0 && listTodayPercent != null && listTodayPercent.Count > 0)
                    {
                        sb.AppendLine("Sales : *Today: " + DateTime.Now.ToString("dd/MMM").ToUpper() + "*");
                        var results = listToday.GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.AppendLine("--------------------");

                                sb.AppendLine("*" + user.Name + "*");
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("{0}:{1}  U$ {2}", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    //sb.AppendLine();
                                    var totalUser = listTodayPercent.Where(x => x.txtName == user.Name).FirstOrDefault();
                                    if (totalUser != null)
                                    {
                                        sb.AppendLine(string.Format("Total: *{0}*  *{1}%*", totalUser.TotalCount, totalUser.CountPercentage)).AppendLine();
                                        sb.AppendLine(string.Format("PAID: *U$ {0}*", totalUser.TotalAmount));
                                        sb.AppendLine(string.Format("USED: *R$ {0}*", totalUser.TotalContel));
                                        sb.AppendLine(string.Format("Pencent of Sales: *{0}%*", totalUser.AmountPercentage));
                                    }
                                }
                                else
                                    sb.AppendLine("*No plans sold*");
                            }

                            if (listTodayPlansPercent != null)
                            {
                                sb.AppendLine("--------------------");
                                sb.AppendLine("*Today's Total*");
                                foreach (var plan in listTodayPlansPercent)
                                {
                                    sb.AppendLine(string.Format("{0}:{1}  U$ {2}  *{3}%*", plan.Plan, plan.TotalCount, plan.TotalAmount, plan.CountPercentage));
                                }
                                sb.AppendLine(string.Format("*Total:{0}  U$ {1}*", listTodayPlansPercent.Sum(x => x.TotalCount), listTodayPlansPercent.Sum(x => x.TotalAmount)));
                            }

                            var dbval = ctx.tblConfigSettings.Where(x => x.txtConfigName == "ContelSaldoToday").FirstOrDefault();
                            var saldoStart = Convert.ToDouble(dbval.txtConfigValue, CultureInfo.InvariantCulture);
                            var endSal = new MVNOAccess().GetRemainingSaldoForCompany();
                            var salVal = endSal != null && !string.IsNullOrEmpty(endSal.saldo) ? endSal.saldo : "0";
                            var saldoEnd = Convert.ToDouble(salVal, CultureInfo.InvariantCulture);
                            var differenceSold = saldoStart - saldoEnd;
                            var totalSaldo = ctx.tblInternationalUserPurchases.Where(x => x.bitTest.HasValue == false || x.bitTest.Value == false && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).Sum(x => x.intContelPrice);
                            var diff = differenceSold - Convert.ToDouble(totalSaldo, CultureInfo.InvariantCulture);

                            sb.AppendLine().AppendLine(string.Format("Used Today: R$ {0}", String.Format("{0:0.00}", totalSaldo)));
                            sb.AppendLine(string.Format("Debito Saldo Difference: R${0}", String.Format("{0:0.00}", diff)));

                            SendMessageInfoToAdmin(sb.ToString());

                        }
                    }
                    #endregion

                    sb.Clear();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "NotifyInternationSalesDay error:" + ex.ToString());
                return ex.ToString();
            }
            return "Success";
        }

        public string NotifyInternationSalesMonth()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    #region Month's Count
                    var listMonth = ctx.GetFacilSaleStats(2).ToList();
                    var listMonthPercent = ctx.GetFacilSaleStatsPercentage(2).ToList();
                    var listPlansPercent = ctx.GetFacilSalePlanPercentage(2).ToList();
                    if (listMonth != null && listMonth.Count > 0 && listMonthPercent != null && listMonthPercent.Count > 0)
                    {
                        sb.AppendLine("Sales : *" + DateTime.Now.ToString("MMM-yyyy").ToUpper() + "*");
                        var results = listMonth.GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.AppendLine("--------------------");
                                sb.AppendLine("*" + user.Name + "*");
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("{0}:{1}  U$ {2}", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    var totalUser = listMonthPercent.Where(x => x.txtName == user.Name).FirstOrDefault();
                                    if (totalUser != null)
                                    {
                                        sb.AppendLine(string.Format("Total: *{0}*  *{1}%*", totalUser.TotalCount, totalUser.CountPercentage));
                                        sb.AppendLine(string.Format("Total: *U$ {0}*  *{1}%*", totalUser.TotalAmount, totalUser.AmountPercentage));
                                    }
                                }
                                else
                                    sb.AppendLine("*No plans sold*");
                            }
                            if (listPlansPercent != null)
                            {
                                sb.AppendLine("--------------------");
                                sb.AppendLine("*Months's Total*");
                                foreach (var plan in listPlansPercent)
                                {
                                    sb.AppendLine(string.Format("{0}:{1}  U$ {2}  *{3}%*", plan.Plan, plan.TotalCount, plan.TotalAmount, plan.CountPercentage));
                                }
                                sb.AppendLine(string.Format("*Total:{0}  U$ {1}*", listPlansPercent.Sum(x => x.TotalCount), listPlansPercent.Sum(x => x.TotalAmount)));
                            }
                            SendMessageInfoToAdmin(sb.ToString());

                        }
                    }
                    #endregion


                    sb.Clear();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "NotifyInternationSalesMonth error:" + ex.ToString());
                return ex.ToString();
            }
            return "Success";
        }

        public void SendCartItemNotification(CartViewModel plans)
        {
            using (var ctx = new FoneClubeContext())
            {
                var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == plans.PersonId);

                if (person != null)
                {
                    var personParent = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == person.intIdPerson);
                    if (personParent != null)
                    {
                        var parentName = ctx.tblPersons.Where(x => x.intIdPerson == personParent.intIdParent).FirstOrDefault();
                        string parentNome = string.Empty;
                        string parentPhone = string.Empty;
                        if (parentName != null)
                        {
                            parentNome = parentName.txtName;
                            var pphone = ctx.tblPersonsPhones.FirstOrDefault(x => x.intIdPerson == parentName.intIdPerson && x.bitPhoneClube == true);
                            if (pphone != null)
                                parentPhone = "55" + pphone.intDDD.ToString() + pphone.intPhone.ToString();
                        }

                        string message = string.Format("Cliente *{0}* com CPF {1} e whatsappnumber {2} tem planos abaixo no carrinho, mas ainda não fez check-out \n \n", person.txtName, person.txtDocumentNumber, person.txtDefaultWAPhones);
                        int iplan = 1;
                        foreach (var plan in plans.Plans.Split('#'))
                        {
                            message += string.Format("{0}. Nome do plano: {1}, Valor do plano: R${2}.00 \n", iplan, plan.Split('|')[0], (Convert.ToInt32(plan.Split('|')[1]) / 100));
                            iplan++;
                        }

                        message += string.Format("\n \n Número total de planos no carrinho: *{0}*", plans.Plans.Split('#').Length);

                        SendMessageToAdminAndParent objSend = new SendMessageToAdminAndParent()
                        {
                            Type = "CartItemUpdate",
                            Name = person.txtName,
                            CPF = person.txtDocumentNumber,
                            ParentName = parentNome,
                            WhatsAppNumber = person.txtDefaultWAPhones,
                            ParentWhatsAppNumber = parentPhone,
                            Message = message
                        };
                        SendMessageInfoToAdminAndParent(objSend);
                    }
                }
                //else
                //{
                //    string message = "Pessoa desconhecida tem planos abaixo no carrinho, mas ainda não finalizou a compra \n \n";
                //    int iplan = 1;
                //    foreach (var plan in plans.Plans.Split('#'))
                //    {
                //        message += string.Format("{0}. Nome do plano: {1}, Valor do plano: R${2}.00 \n", iplan, plan.Split('|')[0], (Convert.ToInt32(plan.Split('|')[1]) / 100));
                //        iplan++;
                //    }

                //    message += string.Format("\n \n Número total de planos no carrinho: *{0}*", plans.Plans.Split('#').Length);

                //    SendMessageToAdminAndParent objSend = new SendMessageToAdminAndParent()
                //    {
                //        Type = "CartItemUpdate",
                //        Message = message
                //    };
                //    SendMessageInfoToAdminAndParent(objSend);
                //}
            }
        }

        private void LogFailedWhatsAppMessages(tblWhatsAppFailedStatus objtblWhatsAppFailedStatus)
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.tblWhatsAppFailedStatus.Add(objtblWhatsAppFailedStatus);
                ctx.SaveChanges();
            }
        }

        public bool SendMessageViaRocketAPI(WhatsAppMessage model)
        {
            bool isAnyOneSuccess = false;
            try
            {
                if (model != null)
                {
                    var splitPhones = model.ClientIds.Split(',');
                    var msg = model.Message.Replace("#", "%23");
                    foreach (var phone in splitPhones)
                    {
                        var phone1 = phone.Length == 11 ? "55" + phone : phone;
                        string apiMethod = string.Format("http://rc.foneclube.com.br/connector/09A2C786D923497BA270/inbound?phone={0}@ENTRADA&text={1} ", phone1, msg);
                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageViaRocketAPI Url: " + apiMethod });
                            ctx.SaveChanges();
                        }
                        try
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            WebRequest request = WebRequest.Create(apiMethod);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response != null && response.StatusCode == HttpStatusCode.OK && !isAnyOneSuccess)
                                isAnyOneSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            using (var ctx = new FoneClubeContext())
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageViaRocketAPI error in loop: " + ex.ToString() });
                                ctx.SaveChanges();
                            }
                            if (!isAnyOneSuccess)
                                isAnyOneSuccess = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SendMessageViaRocketAPI error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                return false;
            }
            return isAnyOneSuccess;
        }
    }
}
