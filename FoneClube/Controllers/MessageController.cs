using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.chat2desk.message;
using FoneClube.Business.Commons.Entities.FoneClube.message;
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
using System.Web.Http;
using System.Web.Http.Description;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
namespace FoneClube.WebAPI.Controllers
{
    public class MessageController : Controller
    {
        MessageAccess _service;
        public MessageController()
        {
            _service = new MessageAccess();
        }

        [HttpPost]
        [Route("send-invoice")]
        public IHttpActionResult SendChargeInvoice(ChargeMessage model)
        {
            try
            {
                var statusResponse = _service.SendChargeMessage(model);
                return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }
        [HttpPost]
        [Route("send")]
        public IHttpActionResult Send(WhatsappSendMessage model)
        {
            try
            {
                var statusResponse = _service.SendWhatsappMessage(model);
                return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("receive-webhook")]
        public IHttpActionResult ProcessWebhookMessage()
        {
            try
            {
                var statusResponse = _service.ProcessWebhookMessage();
                return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [Route("client/{id}")]
        [HttpGet]
        public ClientChatData GetClientMessages(long id, bool minimal)
        {
            return _service.GetClientMessages(id, minimal);
        }

        [Route("client/lateby24hr")]
        [HttpGet]
        public bool Send24HrsLateClientInfo()
        {
            EmailAccess emailAccess = new EmailAccess();
            return emailAccess.Send24HrsLateClientInfo();
        }
       
    }
}
