using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FoneClube.DataAccess;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.DataAccess.security;
using FoneClube.DataAccess.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FoneClube.WebAPI.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        [Route("action")]
        public FacilActionResponse PerformAction(FacilActionRequest actions)
        {
           return new FacilIntlAccess().PerformAction(actions);
        }
    }
}
