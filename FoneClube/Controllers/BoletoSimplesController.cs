
using Newtonsoft.Json.Linq;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace FoneClube.WebAPI.Controllers
{
    public class BoletoSimplesController : Controller
    {

        // POST callbacks/boletosimples/bankbillets
        [Route("bankbillets")]
        public IHttpActionResult UpdateBankbilletStatus([FromBody] JToken body)
        {

            var result = new ChargingAcess().UpdateBankBilletStatus(body);
            return ResponseMessage(result);
        }

        private IHttpActionResult ResponseMessage(HttpResponseMessage result)
        {
            throw new NotImplementedException();
        }
    }
}