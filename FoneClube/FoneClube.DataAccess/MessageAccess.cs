using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.chat2desk.message;
using FoneClube.Business.Commons.Entities.FoneClube.message;
using FoneClube.Business.Commons.Entities.Generic;
using FoneClube.DataAccess.chat2desk;
using Newtonsoft.Json;
using SelectPdf;
using System.Configuration;
using static Business.Commons.Utils.Utils;

namespace FoneClube.DataAccess
{
    public class MessageAccess
    {
        Utils _util;
        public MessageAccess()
        {
            _util = new Utils { BaseAddress = ApiConstants.Chat2DeskBaseUrl };
        }
        public HttpStatusCode SendChargeMessage(ChargeMessage model)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblClient = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == model.ClientId);

                if (!bool.Equals(tblClient, null))
                {
                    var tblClientPhone = ctx.tblPersonsPhones.Where(a => a.intIdPerson.Value == model.ClientId && (a.bitPhoneClube.HasValue && a.bitPhoneClube.Value))
                   .Select(p => new
                   {
                       CustomerId = p.intIdPerson.Value,
                       Phone = new Phone
                       {
                           DDD = p.intDDD.ToString(),
                           Number = p.intPhone.ToString(),
                           IsFoneclube = p.bitPhoneClube,
                           Id = p.intId,
                           IdPlanOption = p.intIdPlan.Value,
                           NickName = p.txtNickname,
                           Portability = p.bitPortability,
                           LinhaAtiva = p.bitAtivo,
                           Status = p.intIdStatus,
                           AmmountPrecoVip = p.intAmmoutPrecoVip,
                           PrecoVipStatus = p.bitPrecoVip,
                           Delete = p.bitDelete
                       }
                   }).FirstOrDefault();

                    if (tblClientPhone == null)
                    {
                        throw new HttpResponseException(new Utils().GetErrorPostMessage("Client phone not found"));
                    }

                    // Get client info from chat2desk server
                    var clientInfo = getClientByPhone(tblClientPhone.Phone.DDD + tblClientPhone.Phone.Number);

                    if (!clientInfo.Success)
                    {
                        throw new HttpResponseException(new Utils().GetErrorPostMessage(clientInfo.Message));
                    }
                    else if (clientInfo.Data.data.Count == 0)
                    {
                        throw new HttpResponseException(new Utils().GetErrorPostMessage($"Client's phone number {tblClientPhone.Phone.Number} is not exists on chat2desk"));
                    }
                    //                        

                    string pdfUrl = generateChargeMessagePdf(model);

                    var dataToPost = new WhatsappSendMessage { Pdf = pdfUrl, ClientId = clientInfo.Data.data.FirstOrDefault()?.id ?? 0 };

                    var result = SendWhatsappMessage(dataToPost);
                    return HttpStatusCode.OK;
                }
                else
                {
                    throw new HttpResponseException(new Utils().GetErrorPostMessage("Client not found"));
                }
            }
        }

        public HttpStatusCode SendWhatsappMessage(WhatsappSendMessage model)
        {
            var keyValues = new Dictionary<string, object>();
            keyValues.Add("client_id", model.ClientId);
            keyValues.Add("text", model.Text);
            keyValues.Add("attachment", model.Attachment);
            keyValues.Add("pdf", model.Pdf);

            string dataToPost = _util.GenerateUrlencodedData(keyValues);// $"&client_id={model.ClientId}&text={model.Text}&attachment={model.Attachment}&pdf={model.Pdf}";
            var result = _util.MakeWebRequest<SendMessageResponse>($"{ApiConstants.Chat2DeskBaseUrl}/v1/messages", "POST", authToken: ApiConstants.Chat2DeskAPIToken, requestJSON: dataToPost, contentType: ContentType.X_WWW_FormUrlencoded);

            if (!result.Success)
            {
                string errorMessage = result.Message;
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = result.Data.Message;
                }

                throw new HttpResponseException(new Utils().GetErrorPostMessage(errorMessage));
            }

            //using (var ctx = new FoneClubeContext())
            //{
            //    ctx.tblWhatsappMessages.Add(new tblWhatsappMessages
            //    {
            //        intIdMessage = result.Data.Data.Message_Id,
            //        dteCreated = DateTime.Now,
            //        intIdClient = result.Data.Data.Client_Id,
            //        txtPdf = model.Pdf,
            //        txtText = model.Text,
            //        txtType = result.Data.Data.Type
            //    });

            //    ctx.SaveChanges();
            //}

            return HttpStatusCode.OK;
        }

        public HttpStatusCode SendWhatsappMessageNew(WhatsAppMessage model)
        {
            var stringJson = JsonConvert.SerializeObject(model.Message);
            string JsonStr = "{\r\n            \n    \"phone\": \""+model.ClientIds+"\",\r\n            \n    \"message\": "+ stringJson + ",\r\n            \n    \"isGroup\": false\r\n            \n}\r\n            \n";
            StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

            //string dataToPost = _util.GenerateUrlencodedData(keyValues);// $"&client_id={model.ClientId}&text={model.Text}&attachment={model.Attachment}&pdf={model.Pdf}";
            //var result = _util.MakeWebRequest<WhatsAppMessageResponse>($"{ApiConstants.WhatsAppWPP}/api/111/send-message", "POST", authToken: "Bearer " +  ApiConstants.BearerToken, requestJSON: dataToPost, contentType: ContentType.X_WWW_FormUrlencoded);

            // Initialization.  
            string responseObj = string.Empty;

            // HTTP GET.  
            using (var client = new HttpClient())
            {
                // Setting Authorization.  
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["WPPBearerToken"]);

                // Setting Base address.  
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WPPBaseAddress"]);

                client.DefaultRequestHeaders.Accept.Clear();

                // Setting content type.  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Initialization.  
                HttpResponseMessage response = new HttpResponseMessage();

                // HTTP GET  
                response = client.PostAsync(string.Format("api/{0}/send-message", ConfigurationManager.AppSettings["WPPSessionId"]), content).GetAwaiter().GetResult();

                var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.Write(content);

                // Verification  
                if (response.IsSuccessStatusCode)
                {
                    // Reading Response.  
                   
                }
            }
            //if (!result.Success)
            //{
            //    string errorMessage = result.Message;
            //    if (string.IsNullOrWhiteSpace(errorMessage))
            //    {
            //        errorMessage = result.Data.Message;
            //    }

            //    throw new HttpResponseException(new Utils().GetErrorPostMessage(errorMessage));
            //}

            //using (var ctx = new FoneClubeContext())
            //{
            //    ctx.tblWhatsappMessages.Add(new tblWhatsappMessages
            //    {
            //        intIdMessage = result.Data.Data.Message_Id,
            //        dteCreated = DateTime.Now,
            //        intIdClient = result.Data.Data.Client_Id,
            //        txtPdf = model.Pdf,
            //        txtText = model.Text,
            //        txtType = result.Data.Data.Type
            //    });

            //    ctx.SaveChanges();
            //}

            return HttpStatusCode.OK;
        }

        public HttpStatusCode ProcessWebhookMessage()
        {
            try
            {
                Stream req = HttpContext.Current.Request.InputStream;
                string json = new StreamReader(req).ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var messageData = JsonConvert.DeserializeObject<WebhookMessageResponse>(json);
                    if (messageData != null)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var message = ctx.tblWhatsappMessages.SingleOrDefault(x => x.intIdMessage == messageData.Message_Id);
                            if (message != null)
                            {
                                message.intIdClient = messageData.Client_Id;
                            }
                            else
                            {
                                ctx.tblWhatsappMessages.Add(new tblWhatsappMessages
                                {
                                    intIdMessage = messageData.Message_Id,
                                    dteCreated = messageData.Event_Time,
                                    intIdClient = messageData.Client_Id,
                                    txtPdf = messageData.Pdf,
                                    txtText = messageData.Text,
                                    txtType = messageData.Type
                                });
                            }

                            ctx.SaveChanges();
                        }
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                var exMessage = Utils.ProcessException(ex);
                throw new HttpResponseException(new Utils().GetErrorPostMessage(exMessage));
            }
        }

        public ClientChatData GetClientMessages(long clientId, bool minimal)
        {
            using (var ctx = new FoneClubeContext())
            {
                var result = new ClientChatData();
                var tblMessages = ctx.tblWhatsappMessages
                    .Where(x => x.intIdClient == clientId).ToList();

                var groupedMessages = tblMessages
                   .GroupBy(x => x.dteCreated.Date).Select(x => new ClientGroupedMessages
                   {
                       Date = x.Key,
                       Messages = x.Select(y => new ClientMessages
                       {
                           Id = y.intIdMessage,
                           Client_Id = y.intIdClient,
                           Text = y.txtText,
                           Pdf = y.txtPdf,
                           Type = y.txtType,
                           Created = y.dteCreated,
                           SendBy = y.txtSendBy
                       }).ToList()
                   }).OrderBy(x => x.Date).ToList();

                groupedMessages.ForEach(x =>
                {
                    var messageDate = x.Date.Date;
                    if (messageDate == DateTime.Now.Date)
                    {
                        x.FormattedDate = "Today";
                    }
                    else if (messageDate == DateTime.Now.Date.AddDays(-1))
                    {
                        x.FormattedDate = "Yesterday";
                    }
                    else
                    {
                        x.FormattedDate = messageDate.ToString("MMMM dd, yyyy");
                    }

                    x.Messages.ForEach(y =>
                    {
                        y.Time = y.Created.ToString("hh:mm tt");
                    });
                });

                result.Messages = groupedMessages;

                if (!minimal)
                {
                    var c2dClient = new ClientWhatsappAccess().GetClient(clientId);
                    if (c2dClient.Success)
                    {
                        result.C2D_Client = c2dClient.Data.Data;
                    }
                }
                //Mark messages as read
                var unreadMessages = tblMessages.Where(x => x.txtType == "from_client" && !x.bitRead).ToList();
                if (unreadMessages.Count > 0)
                {
                    unreadMessages.ForEach(x =>
                    {
                        x.bitRead = true;
                    });

                    ctx.SaveChanges();
                }

                return result;
            }
        }

        private ResponseModel<ClientListModel> getClientByPhone(string phoneNumber)
        {
            var result = _util.MakeWebRequest<ClientListModel>($"{ApiConstants.Chat2DeskBaseUrl}/v1/clients?phone={phoneNumber}", "GET", ApiConstants.Chat2DeskAPIToken);
            // var result = await _util.MakeGetAPICall<ClientListModel>($"/v1/clients?phone={phoneNumber}", ApiConstants.Chat2DeskAPIToken);

            return result;
        }

        private string generateChargeMessagePdf(ChargeMessage model)
        {
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/Content/templates/chargeTemplate.html")))
            {
                string fileName = "Charge_" + Guid.NewGuid().ToString() + ".pdf";
                string filePath = $"/temp/pdf/{fileName}";//HttpContext.Current.Server.MapPath(fileName);
                // instantiate a html to pdf converter object
                HtmlToPdf converter = new HtmlToPdf();

                string htmlContent = reader.ReadToEnd();
                htmlContent = htmlContent.Replace("[NAME]", model.ClientName)
                    .Replace("[ANO]", model.CurrentYear.ToString())
                    .Replace("[MES]", model.CurrentMonth.ToString())
                    .Replace("[DATE]", model.CurrentDate.ToString())
                    .Replace("[AmountTemp]", model.AmountTemp)
                    .Replace("[ValorTotalLiberadoParaPagarCliente]", model.ValorTotalLiberadoParaPagarCliente)
                    .Replace("[AmountTemp1]", model.AmountTemp1)
                    .Replace("[CustomerComment]", model.CustomerComment)
                    .Replace("[CommentBoleto]", model.CommentBoleto)
                    .Replace("[Comment]", model.Comment);
                // create a new pdf document converting a html string
                PdfDocument doc = converter.ConvertHtmlString(htmlContent);

                // save pdf document
                doc.Save(filePath);

                // close pdf document
                doc.Close();

                //return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/" + fileName;
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/File?fileName=" + fileName;
            }
        }

        private void insertMessage(SendMessageResponse response)
        {

        }
    }
}
