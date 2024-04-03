using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace FoneClube.Controllers
{
    public class AccountController : Controller
    {
        [Route("plans")]
        public List<Plan> GetPlans()
        {
            return new AccountAccess().GetPlans();
        }

        // GET api/Account/UserInfo
        [Route("plans/{id}")]
        public List<Plan> GetPlansById(int id)
        {
            return new AccountAccess().GetPlansById(id);
        }

        [Route("operators")]
        public List<Operator> GetOperators()
        {
            return new AccountAccess().GetOperators();
        }
    }
}
