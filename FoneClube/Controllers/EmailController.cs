using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.Generic;
using FoneClube.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagarMe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Description;

namespace FoneClube.WebAPI.Controllers
{
    public class EmailController : Controller
    {
        [HttpPost]
        [Route("send")]
        public bool SendEmail(Email email)
        {
            try
            {
                return new EmailAccess().SendEmail(email);
            }
            catch (Exception)
            {

                return false;
            }
        }

        [HttpPost]
        [Route("template/save")]
        public bool SaveTemplates(EmailTemplate template)
        {
            try
            {
                return new EmailAccess().SaveTemplates(template);
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("template/delete/{templateId}")]
        public bool DeleteTemplates(string templateId)
        {
            try
            {
                return new EmailAccess().DeleteTemplate(Convert.ToInt32(templateId));
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("template/send")]
        public bool SendTemplates(EmailTemplate template)
        {
            try
            {
                return new EmailAccess().SendTemplate(template);
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpGet]
        [Route("templates")]
        public List<EmailTemplate> GetTemplatesEmail()
        {
            try
            {
                return new EmailAccess().GetTemplates();
            }
            catch (Exception)
            {
                return new List<EmailTemplate>();
            }
        }

        [HttpGet]
        [Route("sendemailstatus/{templateId}")]
        public tblEmailTemplates GetEmailDetails(string templateId)
        {
            return new EmailAccess().GetEmailDetails(Convert.ToInt32(templateId));
        }

        //apagar todo lixo gerado daqui para baixo por algum dev
        [HttpPost]
        [Route("sendemailstatus")]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new System.Web.Http.HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var root = HttpContext.Current.Server.MapPath("~/App_Data/UploadFile");
                Directory.CreateDirectory(root);
                //var provider = new MultipartFormDataStreamProvider(root);
                var provider = await Request.Content.ReadAsMultipartAsync<InMemoryMultipartFormDataStreamProvider>(new InMemoryMultipartFormDataStreamProvider());
                //var result = await Request.Content.ReadAsMultipartAsync(provider);


                var data = provider.FormData["model"];
                EmailStatus model = JsonConvert.DeserializeObject<EmailStatus>(data); // result.FormData["model"];
                if (model == null)
                {
                    throw new(HttpStatusCode.BadRequest);
                }
                //TODO: Do something with the JSON data.  

                List<Attachments> attachments = new List<Attachments>();
                //foreach (HttpContent ctnt in result.Contents)
                //{
                //    // You would get hold of the inner memory stream here
                //    Stream stream = ctnt.ReadAsStreamAsync().Result;
                //    //var stream = GetFileStream(file.LocalFileName).Result;
                //    attachments.Add(new Attachments { Name = "file.docx", FileStream = stream });
                //    // do something witht his stream now
                //}


                //get the posted files 
                foreach (var file in provider.Files)
                {
                    HttpContent files = file;
                    //var stream = GetFileStream(file.LocalFileName).Result;
                    Stream stream = await file.ReadAsStreamAsync();
                    var filename = file.Headers.ContentDisposition.FileName != null ? file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty) : "filename.doc";
                    attachments.Add(new Attachments { Name = filename, FileStream = stream });
                    //TODO: Do something with uploaded file.  ""
                }
                new EmailAccess().SendEmailStatus(model, attachments);

                return Request.CreateResponse(HttpStatusCode.OK, "success!");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "success!");
            }
        }

        private async Task<Stream> GetFileStream(string url)
        {

            try
            {
                var clientHttp = new HttpClient();
                clientHttp.Timeout = TimeSpan.FromMinutes(5);
                var myTask = clientHttp.GetByteArrayAsync(new Uri(url));

                var byteArray = await myTask;

                Stream stream = new MemoryStream(byteArray);

                return stream;

            }
            catch (AggregateException ex)
            {
                throw ex;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class InMemoryMultipartFormDataStreamProvider : MultipartStreamProvider
    {
        private NameValueCollection _formData = new NameValueCollection();
        private List<HttpContent> _fileContents = new List<HttpContent>();

        // Set of indexes of which HttpContents we designate as form data
        private Collection<bool> _isFormData = new Collection<bool>();

        /// <summary>
        /// Gets a <see cref="NameValueCollection"/> of form data passed as part of the multipart form data.
        /// </summary>
        public NameValueCollection FormData
        {
            get { return _formData; }
        }

        /// <summary>
        /// Gets list of <see cref="HttpContent"/>s which contain uploaded files as in-memory representation.
        /// </summary>
        public List<HttpContent> Files
        {
            get { return _fileContents; }
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            // For form data, Content-Disposition header is a requirement
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition != null)
            {
                // We will post process this as form data
                _isFormData.Add(String.IsNullOrEmpty(contentDisposition.FileName));

                return new MemoryStream();
            }

            // If no Content-Disposition header was present.
            throw new InvalidOperationException(string.Format("Did not find required '{0}' header field in MIME multipart body part..", "Content-Disposition"));
        }

        /// <summary>
        /// Read the non-file contents as form data.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecutePostProcessingAsync()
        {
            // Find instances of non-file HttpContents and read them asynchronously
            // to get the string content and then add that as form data
            for (int index = 0; index < Contents.Count; index++)
            {
                if (_isFormData[index])
                {
                    HttpContent formContent = Contents[index];
                    // Extract name from Content-Disposition header. We know from earlier that the header is present.
                    ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                    string formFieldName = UnquoteToken(contentDisposition.Name) ?? String.Empty;

                    // Read the contents as string data and add to form data
                    string formFieldValue = await formContent.ReadAsStringAsync();
                    FormData.Add(formFieldName, formFieldValue);
                }
                else
                {
                    _fileContents.Add(Contents[index]);
                }
            }
        }

        /// <summary>
        /// Remove bounding quotes on a token if present
        /// </summary>
        /// <param name="token">Token to unquote.</param>
        /// <returns>Unquoted token.</returns>
        private static string UnquoteToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }
    }
}