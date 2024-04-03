using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.email;
using FoneClube.Business.Commons.Entities.Generic;
//using FoneClube.Business.Commons.Entities.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Business.Commons.Utils
{
    public class Utils
    {
        //todo
        //colocar enum tipo log e tipo registros aqui

        public static class Commission
        {
            public static int FirstLevel = 1;
            public static int SecondLevel = 2;
            public static int ThirdLevel = 3;
        }

        public static class OperatorType
        {
            public static int Claro = 1;
            public static int Vivo = 2;
            public static string ClaroDescription = "Claro";
            public static string VivoDescription = "Vivo";
        }

        public static class TipoImagem
        {
            public static int Selfie = 1;
            public static int Front = 2;
            public static int Back = 3;
        }

        public class ApiConstants
        {
            public const string Chat2DeskBaseUrl = "https://api.chat2desk.com.mx",

                Chat2DeskAPIToken = "18a96c403ea71a83c5372ce26ea073";

            public const string WhatsAppWPP = "http://foneclube.com.br:21465",

                BearerToken = "$2b$10$VJii8iT1e4biJHCP82_OK.StdzlRg8GZ.nYyY.SKPyctg3sG_oCQO";
        }

        public class ReasonPhrase
        {
            public static string ExistentDocument = "Documento de registro existente";
            public static string ExistentPhone = "Phone number already exists with another person";
            public static string NonExistentDocument = "Documento de registro inexistente";
            public static string InsertAddressError = "Erro ao inserir endereço, possível valor pendente";
            public static string UpdateError = "Não foi possível fazer update, verifique os campos";
            public static string ImagensError = "Falha ao salvar lista de imagens";
            public static string ContactError = "Falha ao salvar Informações de contato";
        }

        public class EmailTo
        {
            public static string Vendas = ConfigurationManager.AppSettings["EmailFinanceiro"];
        }

        public HttpResponseMessage GetErrorPostMessage(string reason)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Ocorreu um erro ao tentar fazer POST."),
                ReasonPhrase = reason,
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        public void RollBackContext(DbContext context)
        {
            foreach (DbEntityEntry entry in context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    default: break;
                }
            }
        }

        public bool SendEmail(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {
                var email = "financeiro@foneclube.com.br";
                var password = "K59QnW9SxtZP";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }


        }

        public bool SendEmailShopify(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {
                var email = "shop@myesim.pro";
                var password = "@Shopify010203!@";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);


                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }


        }

        public bool SendEmailFoneclube(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {
                var email = "foneclube@foneclube.com.br";
                var password = "foneclube01x02x03x";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);


                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }


        }
        
        public bool SendEmailOperadora(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {
                var email = "gsinc@foneclube.com.br";
                var password = "foneclube010203";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);


                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }


        }

        public SetupEmail GetSetupClaroFinanceiro()
        {
            var setup = new SetupEmail();

            var email = "gsinc@foneclube.com.br";
            var password = "foneclube010203";

            var client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.zoho.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, password);

            setup.Client = client;

            var mailAddress = new MailAddress(email, "Financeiro Foneclube");

            setup.MailAddress = mailAddress;

            return setup;
        }

        public SetupEmail GetSetupTimFinanceiro()
        {
            var setup = new SetupEmail();
             
            //var email = "movel2@medgrupo.com.br";
            //var password = "@medfone@";

            var email = "rodrigo.pinto@medgrupo.com.br";
            var password = "123mudar";

            var client = new SmtpClient();
            client.Port = 465;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(email, password);

            setup.Client = client;

            var mailAddress = new MailAddress(email, "Cardozo Para Warmup");

            setup.MailAddress = mailAddress;

            return setup;
        }

        public bool SendEmailMultiple(string to, string title, string body, string cc, string bcc, bool bodyHtml = false, int setupId = 0)
        {
            try
            {
                var setupEmail = new SetupEmail();

                if(setupId == SetupEmail.CLARO_FINANCEIRO)
                    setupEmail = GetSetupClaroFinanceiro();

                if (setupId == SetupEmail.TIM_FINANCEIRO)
                    setupEmail = GetSetupTimFinanceiro();

                var mailMessage = new MailMessage();
                mailMessage.Subject = title;
                mailMessage.From = setupEmail.MailAddress;
                mailMessage.IsBodyHtml = bodyHtml;
                mailMessage.Body = body;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                foreach (var address in to.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(address);
                }

                if(!string.IsNullOrEmpty(cc))
                    foreach (var copy in cc.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.CC.Add(copy);
                    }

                if (!string.IsNullOrEmpty(bcc))
                    foreach (var ocult in bcc.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.Bcc.Add(ocult);
                    }

                setupEmail.Client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }
        }


        public bool SendEmailWithAttachment(string to, string title, string body, Stream contentStream, string attachmentName)
        {
            try
            {
                var email = "financeiro@foneclube.com.br";
                var password = "K59QnW9SxtZP";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(contentStream, attachmentName);
                mm.Attachments.Add(attachment);

                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);


                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    DisparoAlertaZoho();
                }
                catch (Exception) { }
                return false;
            }

        }

        public bool SendEmailWithAttachmentStatus(EmailStatus em, List<Attachments> attachments)
        {
            try
            {
                bool bodyHtml = true;
                string password = string.Empty;

                switch (em.from) {
                    case "marcio.franco@foneclube.com.br":
                        password = "QqVFd7DtMREw";
                        break;
                    case "nicholas.grugel@foneclube.com.br":
                        password = "A9H7gbrK135t";
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(password))
                {
                    SmtpClient client = new SmtpClient();
                    client.Port = 587;
                    client.Host = "smtp.zoho.com";
                    client.EnableSsl = true;
                    client.Timeout = 10000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(em.from, password);

                    MailMessage mm = new MailMessage(em.from, em.email, em.subject, em.body);
                    mm.IsBodyHtml = bodyHtml;
                    if (!String.IsNullOrEmpty(em.cc))
                    {
                        mm.CC.Add(em.cc);
                    }
                    //mm.To.Add(em.email);
                    //mm.To.Add("devbaloni1983@gmail.com");
                    if (!String.IsNullOrEmpty(em.bcc))
                    {
                        mm.Bcc.Add(em.bcc);
                    }
                    mm.BodyEncoding = UTF8Encoding.UTF8;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                    if (attachments != null)
                    {
                        foreach (var attach in attachments)
                        {
                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(attach.FileStream, attach.Name);
                            mm.Attachments.Add(attachment);
                        }
                    }

                    mm.BodyEncoding = UTF8Encoding.UTF8; 
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                    client.Send(mm);

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(@"F:\Freelance\Marcio\Test.txt"))
                {
                    writer.WriteLine(ex.ToString());
                }
                return false;
            }

        }

        public void DisparoAlertaZoho()
        {
            SendEmailCardozo("marcio.franco@gmail.com", "ATENÇÃO EMAIL ZOHO PAROU!", "Se esse email foi enviado é devido ao email zoho ter caído na malha de segurança e spam, importante parar todo uso do foneclube até isso ser normalizado");
            SendEmailCardozo("rodrigocardozop@gmail.com", "ATENÇÃO EMAIL ZOHO PAROU!", "Se esse email foi enviado é devido ao email zoho ter caído na malha de segurança e spam, importante parar todo uso do foneclube até isso ser normalizado");
            SendEmailCardozo("nicolasgrugel@gmail.com", "ATENÇÃO EMAIL ZOHO PAROU!", "Se esse email foi enviado é devido ao email zoho ter caído na malha de segurança e spam, importante parar todo uso do foneclube até isso ser normalizado");
        }


        public bool SendEmailCardozo(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {

                var email = "contato@rodrigocardozo.com.br";
                var password = "123!Mudar";

                SmtpClient client = new SmtpClient();
                client.Port = 2525;
                client.Host = "mail.rodrigocardozo.com.br";

                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public void RollBack()
        //{
        //    var context = DataContextFactory.GetDataContext();
        //    var changedEntries = context.ChangeTracker.Entries()
        //        .Where(x => x.State != EntityState.Unchanged).ToList();

        //    foreach (var entry in changedEntries)
        //    {
        //        switch (entry.State)
        //        {
        //            case EntityState.Modified:
        //                entry.CurrentValues.SetValues(entry.OriginalValues);
        //                entry.State = EntityState.Unchanged;
        //                break;
        //            case EntityState.Added:
        //                entry.State = EntityState.Detached;
        //                break;
        //            case EntityState.Deleted:
        //                entry.State = EntityState.Unchanged;
        //                break;
        //        }
        //    }
        //}
        public string BaseAddress = "";

        public enum ContentType
        {
            [Display(Name = "application/json")]
            Json = 1,
            [Display(Name = "application/x-www-form-urlencoded")]
            X_WWW_FormUrlencoded = 2
        }

        public async Task<ResponseModel<T>> MakeGetAPICall<T>(string apiUrl, string accessToken = "")
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var client = new HttpClient())
                {
                    //Passing service base url  
                    client.BaseAddress = new Uri("https://api.chat2desk.com");

                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", accessToken);
                    }
                    //send request

                    var result = await client.GetAsync(apiUrl);

                    if (result.IsSuccessStatusCode)
                    {
                        return new ResponseModel<T>
                        {
                            Success = true,
                            Data = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync()),
                            StatusCode = (int)result.StatusCode
                            //Data = await result.Content.ReadAsStringAsync()
                        };
                    }
                    else
                    {
                        string errorJson = await new StreamReader(await result.Content.ReadAsStreamAsync()).ReadToEndAsync();
                        //ModelStateErrors errors = JsonConvert.DeserializeObject<ModelStateErrors>(errorJson);
                        return new ResponseModel<T>
                        {
                            Success = false,
                            Data = JsonConvert.DeserializeObject<T>(errorJson),
                            StatusCode = (int)result.StatusCode
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " " + ex.InnerException.Message;
                }
                return new ResponseModel<T>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ResponseModel<T>> MakePOSTAPICall<T, JsonBodyData>(string apiUrl, JsonBodyData json, ContentType contentType = ContentType.Json, string accessToken = "")
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var client = new HttpClient())
                {
                    //Passing service base url  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", accessToken);
                    }
                    HttpResponseMessage result = null;

                    if (contentType == ContentType.Json)
                    {
                        var content = new StringContent(json != null ? JsonConvert.SerializeObject(json) : "", Encoding.UTF8, "application/json");
                        result = await client.PostAsync(apiUrl, content);
                    }
                    else
                    {
                        //send request
                        using (var content2 = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)json))
                        {
                            content2.Headers.Clear();
                            content2.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            result = await client.PostAsync(apiUrl, content2);
                            var Data = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.IsSuccessStatusCode)
                    {
                        return new ResponseModel<T>
                        {
                            Success = true,
                            Data = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync()),
                            StatusCode = (int)result.StatusCode
                        };
                    }
                    else
                    {
                        string errorJson = await new StreamReader(await result.Content.ReadAsStreamAsync()).ReadToEndAsync();
                        return new ResponseModel<T>
                        {
                            Success = false,
                            Data = JsonConvert.DeserializeObject<T>(errorJson),
                            StatusCode = (int)result.StatusCode
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ProcessException(ex);

                return new ResponseModel<T>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// Calling 3rd party web apis.
        /// </summary>
        /// <param name="destinationUrl"></param>
        /// <param name="methodName"></param>
        /// <param name="requestJSON"></param>
        /// <returns></returns>
        public ResponseModel<T> MakeWebRequest<T>(string destinationUrl, string methodName, string authToken = "", string requestJSON = "", ContentType contentType = ContentType.Json)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                request.Method = methodName;

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }
                if (methodName == "POST")
                {
                    byte[] bytes = null;
                    switch (contentType)
                    {
                        case ContentType.Json:
                            bytes = System.Text.Encoding.ASCII.GetBytes(requestJSON);
                            request.ContentType = "application/json";
                            break;
                        case ContentType.X_WWW_FormUrlencoded:
                            bytes = Encoding.UTF8.GetBytes(requestJSON);
                            request.ContentType = "application/x-www-form-urlencoded";
                            break;
                    }

                    request.ContentLength = bytes.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return new ResponseModel<T>
                        {
                            Success = true,
                            Data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd()),
                            StatusCode = (int)response.StatusCode
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ProcessException(ex);

                return new ResponseModel<T>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

            }
        }

        public ResponseModel<T> MakeWebRequestNew<T>(string destinationUrl, string methodName, string authToken = "", string requestJSON = "", ContentType contentType = ContentType.Json)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                request.Method = methodName;

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", "Bearer" + authToken);
                }
                if (methodName == "POST")
                {
                    byte[] bytes = null;
                    switch (contentType)
                    {
                        case ContentType.Json:
                            bytes = System.Text.Encoding.ASCII.GetBytes(requestJSON);
                            request.ContentType = "application/json";
                            break;
                        case ContentType.X_WWW_FormUrlencoded:
                            bytes = Encoding.UTF8.GetBytes(requestJSON);
                            request.ContentType = "application/x-www-form-urlencoded";
                            break;
                    }

                    request.ContentLength = bytes.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return new ResponseModel<T>
                        {
                            Success = true,
                            Data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd()),
                            StatusCode = (int)response.StatusCode
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ProcessException(ex);

                return new ResponseModel<T>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

            }
        }

        public static string ProcessException(Exception ex)
        {
            string errorMessage = ex.Message;

            if (ex.InnerException != null)
            {
                errorMessage += ". " + ex.InnerException.Message;
            }

            return errorMessage;
        }

        public string GenerateUrlencodedData(Dictionary<string, object> dataParams)
        {
            List<string> parametersList = new List<string>();
            foreach (var parameter in dataParams)
            {
                parametersList.Add(parameter.Key + "=" + parameter.Value);
            }

            return "&" + string.Join("&", parametersList);
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
     
        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}
