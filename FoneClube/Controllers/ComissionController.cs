using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using FoneClube.WebAPI.Models;
using FoneClube.WebAPI.Providers;
using FoneClube.WebAPI.Results;
using Business.Commons.Entities;
using FoneClube.Business.Commons.Entities;
using FoneClube.DataAccess;
using System.Net;
using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using static FoneClube.DataAccess.ComissionAccess;
using FoneClube.Business.Commons.Entities.FoneClube.comission;
using System.Linq;
using FoneClube.Business.Commons.Entities.ViewModel.Comissao;
using FoneClube.Business.Commons.Entities.ViewModel.Amigos;
using Microsoft.AspNetCore.Mvc;

namespace FoneClube.WebAPI.Controllers
{
   
    public class ComissionController : Controller
    {
        [HttpGet]
        [Route("customer/{customerID}")]
        public Benefit GetCustomerBenefit(string customerID)
        {
            return new ComissionAccess().GetComissionAmmount(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("customer/{customerID}/hierarchy")]
        public List<Comissao> GetCustomerhierarchy(string customerID)
        {

            return new ComissionAccess().GetHierarquiaCliente(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("customer/document/{document}/hierarchy")]
        public List<Comissao> GetCustomerhierarchyDocument(string document)
        {
            return new ComissionAccess().GetHierarquiaClienteDocument(document);
        }

        [HttpGet]
        [Route("customer/{customerID}/paying/hierarchy")]
        public List<Person> GetCustomerPayingHierarchy(string customerID)
        {
            return new ComissionAccess().GetHierarquiaPagante(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("customer/{customerID}/nonpaying/hierarchy")]
        public List<Person> GetCustomerNonPayingHierarchy(string customerID)
        {
            return new ComissionAccess().GetHierarquiaNaoPagante(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("customer/{customerID}/hierarchy/details")]
        public List<Comissao> GetCustomerhierarchyDetails(string customerID)
        {
            return new ComissionAccess().GetComissoesCliente(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("customers/orphans")]
        public List<GetFilhosSemPai_Result> GetFilhosSemPai()
        {
            return new ComissionAccess().GetFilhosSemPai();
        }

        [HttpGet]
        [Route("customers/paternidade/info")]
        public List<CustomerComission> GetFiGetClientesListadosPaternidadeInfolhosSemPai()
        {
            return new ComissionAccess().GetClientesListadosPaternidadeInfo();
        }

        [HttpPost]
        [Route("customer/{customerID}/dispatched")]
        public bool InsertDispatchedCustomerBenefit(string customerID)
        {
            return new ComissionAccess().ReleaseComissions(Convert.ToInt32(customerID));
        }

        //deprecated
        [HttpPost]
        [Route("customer/{customerID}/bonus/insert")]
        public bool GetDispatchedCustomerBenefit(string customerID)
        {
            return new ComissionAccess().InsertBonusComission(new Person { Id = Convert.ToInt32(customerID) });
        }

        //[HttpGet]
        //[Route("bonus/setup")]
        //public bool SetupBonus()
        //{
        //    return new ComissionAccess().SetupBonus();
        //}

        [HttpGet]
        [Route("customer/{customerID}/bonus/")]
        public decimal GetCustomerBonus(string customerID)
        {
            return new ComissionAccess().GetCustomerBonus(Convert.ToInt32(customerID));
        }

        [HttpPost]
        [Route("customer/{customerID}/bonus/dispatched")]
        public decimal SetCustomerBonusDispatched(string customerID)
        {
            return new ComissionAccess().GetCustomerBonus(Convert.ToInt32(customerID));
        }

        [HttpGet]
        [Route("bonus/lista/log")]
        public ListaLogBonus GetBonusListaLog()
        {
            return new ComissionAccess().GetBonusLog();
        }

        [HttpGet]
        [Route("bonus/order/history")]
        public List<BonusOrderReport> GetHistoryBonusOrder(int total)
        {
            return new ComissionAccess().GetHistoryBonusOrder(total);
        }

        [HttpGet]
        [Route("comission/order/history")]
        public List<ComissionOrderReport> GetHistoryComissionOrder(int total)
        {
            return new ComissionAccess().GetHistoryComissionOrder(total);
        }

        [HttpGet]
        [Route("comission/totais/{customerID}")]
        public TotalizadoresComissao GetTotalizadoresComissao(int customerID)
        {
            return new ComissionAccess().GetTotalizadoresComissao(customerID);
        }

        [HttpGet]
        [Route("detalhes/{customerID}")]
        public DetalhesComissao GetDetalhesComissao(int customerID)
        {
            return new ComissionAccess().GetDetalhesComissao(customerID);
        }

        [HttpGet]
        [Route("comission/detalhes/quatro-amigos")]
        public List<CustomerViewModel> GetCustomerAmigosQuatroFilhos()
        {
            return new ComissionAccess().GetCustomerAmigosQuatroFilhos();
        }

        [HttpGet]
        [Route("comission/detalhes/dois-amigos")]
        public List<CustomerViewModel> GetCustomerAmigosDoisFilhos()
        {
            return new ComissionAccess().GetCustomerAmigosDoisFilhos();
        }

        [HttpGet]
        [Route("comission/customer/{customer}/promo/{promo}")]
        public Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel GetPromoCodeOwner(string customer, string promo)
        {
            return new ComissionAccess().GetPromoCodeOwner(customer, promo);
        }




    }
}