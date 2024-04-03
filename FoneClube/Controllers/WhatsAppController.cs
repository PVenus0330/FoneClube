using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using FoneClube.DataAccess;
using FoneClube.Business.Commons;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.ViewModel.Plano;

namespace FoneClube.WebAPI.Controllers
{
    [RoutePrefix("api/whatsapp")]
    public class WhatsAppController : ApiController
    {
        WhatsAppAccess _service;

        public WhatsAppController()
        {
            _service = new WhatsAppAccess();
        }

        [HttpGet]
        [Route("start")]
        public string StartWPP()
        {
            return _service.StartWPP();
        }

        [HttpGet]
        [Route("restart")]
        public string ReStartWPP()
        {
            return _service.ReStartWPP();
        }

        [HttpGet]
        [Route("stop")]
        public string StopWPP()
        {
            return _service.StopWPP();
        }

        [HttpGet]
        [Route("battery")]
        public bool GetBatteryLevel()
        {
            var stat = _service.GetBatteryLevel();
            if (stat != null && !string.IsNullOrEmpty(stat.status) && stat.status == "Success")
            {
                //_service.SendMessageInfoToAdmin("Good Battery Level");
                return true;
            }
            else
            {
                _service.SendMessageInfoToAdmin("Poor Battery Level");
                return false;
            }
        }

        [HttpGet]
        [Route("manage/session/{type}")]
        public string ManageSession(string type)
        {
            return _service.ManageSession(type);
        }

        [HttpGet]
        [Route("generate/token/{tokenNum}")]
        public string GenerateToken(string tokenNum)
        {
            return _service.GenerateToken(tokenNum);
        }

        [HttpGet]
        [Route("status/connection")]
        public string CheckStatusConnection()
        {
            var str = _service.CheckStatusConnection();
            if (!string.IsNullOrEmpty(str) && (str.Contains("Disconnected") || str.Contains("Closed") || str.Contains("CLOSED") || str.Contains("false")))
            {
                _service.ManageSession("start");
                System.Threading.Thread.Sleep(5000);
                _service.SendMessageInfoToAdmin("*WPPConnect Disconnected and Auto started session again*");
            }
            else if (!string.IsNullOrEmpty(str) && str == "An error occurred while sending the request.")
            {
                _service.ReStartWPP();
                System.Threading.Thread.Sleep(5000);
                _service.SendMessageInfoToAdmin("*WPPConnect Disconnected and Auto started session again*");
            }
            return str;
        }

        [HttpGet]
        [Route("status/session")]
        public string ReturnStatusSession()
        {
            return _service.ReturnStatusSession();
        }

        [HttpPost]
        [Route("webhook")]
        public IHttpActionResult ProcessWebhookWhatsAppMessage()
        {
            try
            {
                var statusResponse = _service.ProcessWebhookWhatsAppMessage();
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("templates")]
        public List<WhatsAppMessageTemplates> GetTemplates()
        {
            try
            {
                return _service.GetWhatsAppMessageTemplates();
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpGet]
        [Route("whatsappconfig")]
        public WhatsAppConfigSettingsForTemplate GetWhatsAppConfigSettings()
        {
            try
            {
                return _service.GetWhatsAppConfigSettings();
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("whatsappconfig/save")]
        public bool SaveWhatsAppConfigSettings(WhatsAppConfigSettingsForTemplate objrequest)
        {
            try
            {
                return _service.SaveWhatsAppConfigSettings(objrequest);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("templates/save")]
        public bool SaveTemplates(WhatsAppMessageTemplates template)
        {
            try
            {
                return _service.SaveWhatsAppMessageTemplates(template);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("templates/delete/{templateId}")]
        public bool DeleteTemplates(string templateId)
        {
            try
            {
                return _service.DeleteWhatsAppMessageTemplates(Convert.ToInt32(templateId));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpGet]
        [Route("getuserstatus/{phone}")]
        public string CheckUserStatus(string phone)
        {
            try
            {
                return _service.CheckUserStatus(phone);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpPost]
        [Route("send")]
        public bool SendMessage(WhatsAppMessage model)
        {
            try
            {
                return _service.SendMessage(model);
            }
            catch (HttpResponseException error)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("send/test")]
        public bool SendMessageTest()
        {
            try
            {
                WhatsAppMessage model = new WhatsAppMessage()
                {
                    Message = "Teste from ChatX",
                    ClientIds = "5521920151599"
                };
                return _service.SendMessage(model);
            }
            catch (HttpResponseException error)
            {
                return false;
            }
        }

        [HttpPost]
        [Route("sendbutton")]
        public IHttpActionResult SendMessageWithButton(WhatsAppMessage model)
        {
            try
            {
                var statusResponse = _service.SendMessageWithButton(model);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("sendbuttonurl")]
        public IHttpActionResult SendMessageWithUrl(WhatsAppMessage model)
        {
            try
            {
                var statusResponse = _service.SendMessageWithButtonUrl(model, 0, 0);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.Accepted, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("validate/{phones}")]
        public string ValidatePhone(string phones)
        {
            try
            {
                return _service.ValidatePhoneNumbers(phones);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                throw error;
            }
        }

        [HttpGet]
        [Route("schedule/execute/{days}")]
        public bool ExecuteSchduleWhatsAppMessage(string days)
        {
            return _service.ExecuteSchduleWhatsAppMessage(Convert.ToInt32(days));
        }


        [HttpGet]
        [Route("send/chargesummary/{chargeId}/{phonenumbers}")]
        public string SendChargeSummary(string chargeId, string phonenumbers)
        {
            return _service.SendChargeSummary(Convert.ToInt32(chargeId), phonenumbers);
        }

        [HttpPost]
        [Route("send/marketing")]
        public string SendMarketingMessage(GenericTemplate marketing)
        {
            return _service.SendMarketingMessage(marketing);
        }

        [HttpPost]
        [Route("send/generic")]
        public string SendGenericMessage(GenericTemplate marketing)
        {
            return _service.SendGenericMessage(marketing);
        }

        [HttpPost]
        [Route("send/welcome")]
        public string SendWelcomeMessage(WelcomeTemplate welcome)
        {
            return _service.SendWelcomeMessage(welcome);
        }

        [HttpPost]
        [Route("cc/refused")]
        public string SendWhatsAppMessageCCRefused(Person person)
        {
            bool is3rdReminder = false;
            return _service.SendWhatsAppMessageCCRefused(person, ref is3rdReminder);
        }

        [HttpPost]
        [Route("update/status")]
        public string UpdateWhatsAppStatus(WhatsAppStatus status)
        {
            return _service.UpdateWhatsAppStatus(status);
        }

        [HttpGet]
        [Route("resend/failed")]
        public string ResendFailedMsgs()
        {
            return _service.ResendFailedMsgs();
        }

        [HttpPost]
        [Route("notify/cart/plans")]
        public void SendCartItemNotification(CartViewModel plans)
        {
            if (plans != null && !string.IsNullOrEmpty(plans.Plans))
            {
                _service.SendCartItemNotification(plans);
            }
        }

        [HttpGet]
        [Route("nontop/notify")]
        public string NonTopupInXDaysNotification()
        {
            return _service.NonTopupInXDaysNotification();
        }

        [HttpGet]
        [Route("intl/sales/notify/v1")]
        public string NotifyInternationSales()
        {
            _service.NotifyInternationSalesMonth();
            return _service.NotifyInternationSalesDay();
        }

        [HttpGet]
        [Route("intl/sales/notify/day")]
        public string NotifyInternationSalesDay()
        {
            return _service.NotifyInternationSalesDay();
        }

        [HttpGet]
        [Route("intl/sales/notify/month")]
        public string NotifyInternationSalesMonth()
        {
            return _service.NotifyInternationSalesMonth();
        }

        [HttpPost]
        [Route("send/message")]
        public List<SendMessageChatXResponse> SendMessage(SendMessageChatXRequest request)
        {
            return _service.SendMessageChatX(request);
        }
    }
}