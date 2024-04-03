using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Business.Commons.Entities;
using FoneClube.Business.Commons.Entities;
using FoneClube.DataAccess;
using System.Net;
using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using FoneClube.Business.Commons.Entities.woocommerce;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.WebHooks;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FoneClube.Business.Commons.Entities.woocommerce.order;
using FoneClube.WebAPI.Models;
using FoneClube.WebAPI.Providers;
using FoneClube.WebAPI.Results;

namespace FoneClube.WebAPI.Controllers
{
    [RoutePrefix("api/loja")]
    public class LojaController : ApiController
    {
        [Route("cliente")]
        public CheckoutPagarMe GetCheckoutPerson(string id)
        {
            return new LojaAccess().GetPersonCheckoutLoja(Convert.ToInt32(id));
        }

        [Route("customer/created")]
        [HttpPost]
        public bool SavePersonLojaRegister(CustomerWoocommerce customeRegisterViewModel)
        {
            try
            {
                return new ProfileAccess().SavePersonLojaRegister(customeRegisterViewModel);
            }
            catch (Exception) {
                return false;
            }   
        }


        [HttpPost]
        [Route("customer/update")]
        public bool ReadStringDataManual()
        {
            try
            {
                string raw = getRawPostData().Result;
                var jObject = JsonConvert.DeserializeObject<CustomerWoocommerce>(raw);
                new ProfileAccess().UpdatePersonLojaRegister(jObject);
                return true;
            }
            catch (Exception) {
                return false;
            }   
        }

        [HttpPost]
        [Route("customer/order/created")]
        public bool NewOrder()
        {
            try
            {
                string raw = getRawPostData().Result;
                var jObject = JsonConvert.DeserializeObject<Order>(raw);
                new ProfileAccess().CheckoutPersonLojaRegister(jObject);
                return true;
            }
            catch (Exception) {
                return false;
            }
            
        }

        [HttpPost]
        [Route("customer/order/update")]
        public bool UpdateOrder()
        {
            try
            {
                string raw = getRawPostData().Result;
                var jObject = JsonConvert.DeserializeObject<Order>(raw);
                new ProfileAccess().CheckoutUpdatePersonLojaRegister(jObject);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        [HttpPost]
        [Route("customer/order/complete")]
        public bool OrderComplete()
        {
            try
            {
                string raw = getRawPostData().Result;
                var jObject = JsonConvert.DeserializeObject<StatusPagamento>(raw);
                new ChargingAcess().UpdateChargingHistoryLojaComplete(jObject);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        [HttpGet]
        [Route("bradesco/valida/pacotes")]
        public string GetStatusBoletoBradesco()
        {
            return new ChargingAcess().GetStatusBoletoBradesco();
        }

        public class RawContentReader
        {
            public static async Task<string> Read(HttpRequestMessage req)
            {
                using (var contentStream = await req.Content.ReadAsStreamAsync())
                {
                    contentStream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(contentStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        public async Task<String> getRawPostData()
        {
            using (var contentStream = await this.Request.Content.ReadAsStreamAsync())
            {
                contentStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(contentStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}