using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.Claro;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.estoque;
using FoneClube.Business.Commons.Entities.FoneClube.linhas;
using FoneClube.Business.Commons.Entities.FoneClube.phone;
using FoneClube.Business.Commons.Entities.Vivo;
using FoneClube.DataAccess;
using PagarMe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Mvc.RouteAttribute;

namespace FoneClube.WebAPI.Controllers
{
	public class ManagerPhonesController : Controller
	{

		//deprecated It will be deleted
		[HttpGet]
		[Route("claro/status/linha/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool GetStatusBlockLinhaClaro(string ddd, string numeroLinha)
		{
			try
			{
				var line = new ClaroAccess().GetLineDetails(ddd + numeroLinha);
				return line.Bloqueada;
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("claro/status/linha/ddd/{ddd}/numeroLinha/{numeroLinha}/details")]
		public ClaroLineInfo GetStatusLinhaClaro(string ddd, string numeroLinha)
		{
			try
			{
				return new ClaroAccess().GetLineDetails(ddd + numeroLinha);
			}
			catch (Exception)
			{

				return new ClaroLineInfo();
			}
		}

		[HttpGet]
		[Route("vivo/status/linha/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public VivoLineInfo GetStatusLinhaVivo(string ddd, string numeroLinha)
		{
			return new VivoAccess().GetStatusLine(ddd + numeroLinha);
		}

		[HttpGet]
		[Route("claro/bloqueioRoubo/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool EmailBlockClaro(string ddd, string numeroLinha)
		{
			try
			{
				return new ManagerOperator().EmailBlockClaro(ddd, numeroLinha);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("claro/atualizacao/ddd/{ddd}/numeroLinha/{numeroLinha}/corpo/{corpo}")]
		public bool EmailAtualizaClaro(string ddd, string numeroLinha, string corpo)
		{
			try
			{
				return new ManagerOperator().EmailUpdateClaro(ddd, numeroLinha, corpo);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("vivo/bloqueio/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool EmailBloqueioVivo(string ddd, string numeroLinha)
		{
			try
			{
				return new ManagerOperator().EmailBlockVivo(ddd, numeroLinha);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("vivo/suspensao/roubo/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool EmailBloqueioRouboVivo(string ddd, string numeroLinha)
		{
			try
			{
				return new ManagerOperator().EmailBlockRouboVivo(ddd, numeroLinha);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("vivo/desbloqueio/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool EmailDesbloqueioVivo(string ddd, string numeroLinha)
		{
			try
			{
				return new ManagerOperator().EmailDesbloqueioVivo(ddd, numeroLinha);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("vivo/ativacao/ddd/{ddd}/numeroLinha/{numeroLinha}")]
		public bool EmailAtivacaoVivo(string ddd, string numeroLinha)
		{
			try
			{
				return new ManagerOperator().EmailAtivacaoVivo(ddd, numeroLinha);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("estoque")]
		public List<Estoque> GetLinhasFoneclubeEstoque()
		{
			try
			{
				return new PhoneAccess().GetLinhasFoneclubeEstoque();
			}
			catch (Exception)
			{

				return new List<Estoque>();
			}
		}

        [HttpPost]
        [Route("estoque/propriedade/interna")]
        public bool SetPropriedadeIterna(Estoque phoneEstoque)
        {
            try
            {
                return new PhoneAccess().SetPropriedadeIterna(phoneEstoque);
            }
            catch (Exception)
            {

                return false;
            }
        }

        [HttpGet]
		[Route("all")]
		public List<LinhaDetalhesMinimos> GetLinhasFoneclubeMinimal()
		{
			try
			{
				return new PhoneAccess().GetLinhasFoneclubeMinimal();
			}
			catch (Exception)
			{

				return new List<LinhaDetalhesMinimos>();
			}
		}


		[HttpGet]
		[Route("status")]
		public List<tblPhoneFlags> GetStatusLinhasOperadora()
		{
			try
			{
				return new PhoneAccess().GetStatusLinhasOperadora();
			}
			catch (Exception)
			{

				return new List<tblPhoneFlags>();
			}
		}

		[HttpGet]
		[Route("plans")]
		public List<Business.Commons.Entities.FoneClube.Plan> GetPlansOptions()
		{
			try
			{
				return new PhoneAccess().GetPlansOptions();
			}
			catch (Exception)
			{

				return new List<Business.Commons.Entities.FoneClube.Plan>();
			}
		}

		[HttpGet]
		[Route("all/plans")]
		public List<Business.Commons.Entities.FoneClube.Plan> GetAllPlansOptions()
		{
			try
			{
				return new PhoneAccess().GetAllPlansOptions();
			}
			catch (Exception)
			{

				return new List<Business.Commons.Entities.FoneClube.Plan>();
			}
		}

		[HttpPost]
		[Route("plan/update")]
		public bool SetUpdatePhonePlan(Business.Commons.Entities.FoneClube.Phone phone)
		{
			try
			{
				return new PhoneAccess().UpdatePhonePlan(phone);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("desassociar")]
		public bool SetDesassociarLinha(int phoneId)
		{
			try
			{
				return new PhoneAccess().UpdatePhoneDesassociation(phoneId);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("associar")]
		public bool SetDesassociarLinha(Person person)
		{
			try
			{
				return new PhoneAccess().InsertPhoneAssociation(person);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpGet]
		[Route("divergencia")]
		public List<GetDivergencias_Result> GetDivergentes()
		{
			try
			{
				return new PhoneAccess().GetStatusDivergencia();
			}
			catch (Exception)
			{

				return new List<GetDivergencias_Result>();
			}
		}

		[HttpGet]
		[Route("extra/services")]
		public List<PhoneService> GetServices()
		{
			try
			{
				return new PhoneAccess().GetServices();
			}
			catch (Exception)
			{

				return new List<PhoneService>();
			}
		}

		[HttpGet]
		[Route("extra/all/services")]
		public List<PhoneService> GetAllPhoneServices()
		{
			try
			{
				return new PhoneAccess().GetAllPhoneServices();
			}
			catch (Exception)
			{

				return new List<PhoneService>();
			}
		}

		[HttpGet]
		[Route("{id}/extra/services")]
		public FoneClube.Business.Commons.Entities.FoneClube.Phone GetPhoneServices(string id)
		{
			try
			{
				return new PhoneAccess()
					.GetPhoneServices(new FoneClube.Business.Commons.Entities.FoneClube.Phone
					{
						Id = Convert.ToInt32(id)
					});
			}
			catch (Exception)
			{
				return new FoneClube.Business.Commons.Entities.FoneClube.Phone();
			}
		}

		[HttpPost]
		[Route("extra/service/insert")]
		public bool SetInsertService(FoneClube.Business.Commons.Entities.FoneClube.Phone phone)
		{
			try
			{
				return new PhoneAccess().InsertPhoneService(phone);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("extra/service/insert/deactive")]
		public bool SetInsertDeactiveService(FoneClube.Business.Commons.Entities.FoneClube.Phone phone)
		{
			try
			{
				return new PhoneAccess().InsertDeactivePhoneService(phone);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("service/insert")]
		public bool InsertNewService(PhoneService service)
		{
			try
			{
				return new PhoneAccess().InsertNewService(service);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("service/update")]
		public bool UpdateService(PhoneService service)
		{
			try
			{
				return new PhoneAccess().UpdateService(service);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("plan/insert")]
		public bool InsertNewPlan(Business.Commons.Entities.FoneClube.Plan plan)
		{
			try
			{
				return new PhoneAccess().InsertNewPlan(plan);
			}
			catch (Exception)
			{

				return false;
			}
		}

		[HttpPost]
		[Route("plan/foneclube/update")]
		public bool UpdatePlan(Business.Commons.Entities.FoneClube.Plan plan)
		{
			try
			{
				return new PhoneAccess().UpdatePlan(plan);
			}
			catch (Exception)
			{

				return false;
			}
		}

		#region Phones Services [JAS com melhorias]
		[HttpGet]
		[HttpPost]
		[Route("single-price/update")]
		public bool UpdatePhonePrice(Person person)
		{ 
			return new ProfileAccess().SaveSinglePrice(person);

		}


		[HttpGet]
		[Route("all/phones")]
		public List<PhoneViewModel> GetAllPhones()
		{
			return new PhoneAccess().GetAllPhonesNumbers();
		}


		[HttpPost]
		[Route("insert")]
		public bool SavePersonPhoneNumber(PhoneViewModel phoneModel)
		{
			return new ProfileAccess().SavePersonPhoneNumber(phoneModel);
		}

		[HttpGet]
		[Route("available/numbers")]
		public List<PhoneViewModel> GetAvailablePhoneNumber(string number)
		{
			return new PhoneAccess().GetAvailablePhoneNumbers(number);
		}


		[HttpGet]
		[Route("customer/{personId}")]
		public List<PhoneViewModel> GetAllCustomerPhones(string personId)
		{
            return new PhoneAccess().GetAllCustomerPhones(Convert.ToInt32(personId));
		}


		[HttpGet]
		[Route("services/{personId}")]
		public List<PhoneServiceViewModel> GetActivePhoneServices(string personId)
		{
			return new PhoneAccess().GetActivePhoneServices(Convert.ToInt32(personId));
		}


		[HttpGet]
		[Route("monthly/amount")]
		public string GetMonthlyAmount(int personId)
		{
			return new PhoneAccess().GetMonthlyAmount(personId).ToString("0.00");
		}


        #endregion

        [HttpPost]
        [Route("flags/insert")]
        public bool InsertPhoneFlag(FoneClube.Business.Commons.Entities.FoneClube.Phone phone)
        {
            return new PhoneAccess().InsertPhoneFlag(phone);
        }

        [Route("customer/{personId}/property/history")]
        [HttpGet]
        public List<PropertyHistoryViewModel> SetCustomerParent(int personId)
        {
            return new PhoneAccess().GetPersonPropertyHistory(new Person { Id = personId });
        }

        [HttpGet]
        [Route("alllines")]
        public List<PhoneLines> GetAllPhonesLines()
        {
            try
            {
                return new PhoneAccess().GetAllPhonesLines();
            }
            catch (Exception ex)
            {
                return new List<PhoneLines>();
            }
        }

        [HttpPost]
		[Route("line/edit")]
		public bool AllPhoneLinesEdit(Business.Commons.Entities.FoneClube.EditParam param)
		{
			try
			{
				return new PhoneAccess().AllPhoneLinesEdit(param);
			}
			catch (Exception)
			{

				return false;
			}
		}
	}
}
