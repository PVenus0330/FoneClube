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
using System.Web.Http.Description;
namespace FoneClube.WebAPI.Controllers
{
    public class FacilController : Controller
    {
        FacilAccess _service;
        public FacilController()
        {
            _service = new FacilAccess();
        }

        [HttpPost]
        [Route("token/generate")]
        public IHttpActionResult GenerateToken(TokenRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.Password))
                {
                    var token = _service.GenerateToken(request);
                    return (IHttpActionResult)Ok(token);
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("token/validate")]
        public IHttpActionResult ValidateToken(ValidateTokenRequest token)
        {
            try
            {
                if (token != null && !string.IsNullOrEmpty(token.Token))
                {
                    return (IHttpActionResult)Ok(FacilAccess.ValidateToken(token.Token));
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("plans")]
        public IHttpActionResult GetAllPlans(ValidateApiKeyRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var result = _service.GetAllPlans(request.ApiKey);
                        return (IHttpActionResult)Ok(result);
                    }
                    else
                    {
                        return (IHttpActionResult)Ok(valid);
                    }
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("iccid/validate")]
        public IHttpActionResult ValidateICCID(FacilICCIDRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && !string.IsNullOrEmpty(request.ICCID))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.ValidateICCID(request.ICCID);
                        return (IHttpActionResult)Ok(status);
                    }
                    else
                    {
                        return (IHttpActionResult)Ok(valid);
                    }
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("line/block")]
        public IHttpActionResult BlockLine(FacilGenericRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.Phone != null && !string.IsNullOrEmpty(request.Phone.PhoneNumber))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.BlockLine(request);
                        return (IHttpActionResult)Ok(status);
                    }
                    else
                    {
                        return (IHttpActionResult)Ok(valid);
                    }
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("line/unblock")]
        public IHttpActionResult UnBlockLine(FacilGenericRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.Phone != null && !string.IsNullOrEmpty(request.Phone.PhoneNumber))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.UnBlockLine(request);
                        return (IHttpActionResult)Ok(status);
                    }
                    else
                    {
                        return (IHttpActionResult)Ok(valid);
                    }
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("line/detail")]
        public IHttpActionResult GetPhoneDetails(FacilGenericRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.Phone != null && !string.IsNullOrEmpty(request.Phone.PhoneNumber))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var result = _service.GetPhoneDetail(request);
                        return Ok(result);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("line/all")]
        public IHttpActionResult GetAllPhoneDetails(ValidateApiKeyRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var result = _service.GetAllPhoneDetail(request);
                        return Ok(result);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("line/activate/esim")]
        public IHttpActionResult ActivateESim(FacilActivateESIMRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.ActivationInfo != null && request.ActivationInfo.LineInfo != null && request.ActivationInfo.CustomerInfo != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        if (FacilAccess.IsNewActivationEnabled())
                        {
                            var status = _service.ActivateESimNew(request);
                            return Ok(status);
                        }
                        else
                        {
                            var status = _service.ActivateESim(request);
                            return Ok(status);
                        }
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("line/topup")]
        public IHttpActionResult TopupPlan(FacilTopupRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.TopupInfo != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.TopupPlan(request);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("line/topup/history")]
        public IHttpActionResult GetTopupHistory(FacilGenericRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.Phone != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.GetTopupHistory(request);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("line/balance")]
        public IHttpActionResult GetLineBalance(FacilGenericRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ApiKey) && request.Phone != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.GetLineBalance(request);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("user/balance")]
        public IHttpActionResult GetBalance(ValidateApiKeyRequest request)
        {
            try
            {
                if (request != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.GetBalance(request.ApiKey);
                        return (IHttpActionResult)Ok(status);
                    }
                    else
                    {
                        return (IHttpActionResult)Ok(valid);
                    }
                }
                else
                {
                    return (IHttpActionResult)Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("user/debits")]
        public IHttpActionResult GetUserDebits(ValidateApiKeyRequest request)
        {
            try
            {
                if (request != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.GetUserDebits(request.ApiKey);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("line/activation/pdf/{phone}/{iccid}")]
        public HttpResponseMessage GetActivationDetails(string phone, string iccid)
        {
            try
            {
                if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(iccid))
                {
                    var localFilePath = string.Format(@"C:\inetroot\FacilActivationPdfs\{0}_{1}.pdf", phone, iccid);
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = string.Format("{0}_{1}.pdf", phone, iccid);
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    return response;
                }
                else
                {
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest };
                }
            }
            catch (FileNotFoundException ex)
            {
                var ret = new MVNOAccess().ReDownload(phone, iccid);
                if (ret)
                {
                    var localFilePath = string.Format(@"C:\inetroot\FacilActivationPdfs\{0}_{1}.pdf", phone, iccid);
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = string.Format("{0}_{1}.pdf", phone, iccid);
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    return response;
                }
                else
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return responseMessage;
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return responseMessage;
            }
        }

        [HttpPost]
        [Route("deposit/balance")]
        public IHttpActionResult DepositBalance(FacilUpdateBalanceRequest request)
        {
            try
            {
                if (request != null)
                {
                    var status = _service.DepositBalance(request);
                    return Ok(status);
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpPost]
        [Route("line/resetSIM")]
        public IHttpActionResult ResetLine(FacilResetLineRequest request)
        {
            try
            {
                if (request != null && request.LineInfo != null && request.LineInfo.Phone != null && !string.IsNullOrEmpty(request.LineInfo.Phone.PhoneNumber))
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.ResetLine(request);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("history")]
        public IHttpActionResult GetHistory(ValidateApiKeyRequest request)
        {
            try
            {
                if (request != null)
                {
                    var valid = FacilAccess.ValidateToken(request.ApiKey);
                    if (valid.Status)
                    {
                        var status = _service.GetHistory(request.ApiKey);
                        return Ok(status);
                    }
                    else
                    {
                        return Ok(valid);
                    }
                }
                else
                {
                    return Ok(FacilUtil.BadRequest());
                }
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }


        [HttpGet]
        [Route("reset/phone/for/topup/{selcount}")]
        public IHttpActionResult ResetPhoneForTopup(string selcount)
        {
            try
            {
                int count = Convert.ToInt32(selcount);
                if (count > 0)
                {
                    _service.ResetPhoneForTopup(count);
                }
                return Ok(true);

            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("gighubinc/line/detail/{phone}")]
        public IHttpActionResult GetPhoneDetailsGughubInc(string phone)
        {
            try
            {
                FacilGenericRequest request = new FacilGenericRequest() { ApiKey = _service.GetTokenByUserId(6526), Phone = new FacilPhone() { PhoneNumber = phone } };
                var result = _service.GetPhoneDetail(request);
                return Ok(result);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }

        [HttpGet]
        [Route("gighubinc/line/balance/{phone}")]
        public IHttpActionResult GetLineBalanceGughubInc(string phone)
        {
            try
            {
                FacilGenericRequest request = new FacilGenericRequest() { ApiKey = _service.GetTokenByUserId(6526), Phone = new FacilPhone() { PhoneNumber = phone } };
                var status = _service.GetLineBalance(request);
                return Ok(status);
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }
    }
}
