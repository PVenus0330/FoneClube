using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.chat2desk.message;
using FoneClube.Business.Commons.Entities.FoneClube.message;
using FoneClube.Business.Commons.Entities.Generic;
using FoneClube.DataAccess;
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
using Shopify = FoneClube.Business.Commons.Entities.FoneClube.Shopify;

namespace FoneClube.WebAPI.Controllers
{
    [RoutePrefix("api/shopify")]
    public class ShopifyController : ApiController
    {
        ShopifyAccess _service;
        public ShopifyController()
        {
            _service = new ShopifyAccess();
        }

        [HttpPost]
        [Route("webhook/payment")]
        public Shopify.ShopifyResponse ProcessPaymentWebhook()
        {
            return _service.ProcessPaymentWebhook();
        }
    }
}