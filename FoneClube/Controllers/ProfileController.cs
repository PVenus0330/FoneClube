using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using FoneClube.WebAPI.Models;
using FoneClube.WebAPI.Providers;
using FoneClube.WebAPI.Results;
using Business.Commons.Entities;
using FoneClube.Business.Commons.Entities;
using FoneClube.DataAccess;
using System.Net;
using FoneClube.Business.Commons.Entities.FoneClube;
using System.Linq;
using System.Data;
using FoneClube.Business.Commons.Entities.FoneClube.phone;
using FoneClube.Business.Commons.Entities.FoneClube.flag;
using FoneClube.Business.Commons.Entities.ViewModel;
using static FoneClube.DataAccess.ProfileAccess;
using static FoneClube.DataAccess.PhoneAccess;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Business.Commons.Utils;

namespace FoneClube.WebAPI.Controllers
{
    [RoutePrefix("api/profile")]
    public class ProfileController : ApiController
    {
        // GET api/profile/customers
        [Route("customers/all")]
        [HttpGet]
        public List<GetAllCustomers_Result> GetAllCustomers()
        {
            return new ProfileAccess().GetAllCustomersNew();
        }

        [Route("cliente/id/{id}")]
        public Person GetPersonData(int id)
        {
            return new ProfileAccess().GetPerson(id); //refatorar, nome zuado pq tem outro
        }

        [Route("cliente/id/minimal/{id}")]
        public Person GetPersonDataMinimal(int id)
        {
            return new ProfileAccess().GetPersonMinimal(id); //refatorar, nome zuado pq tem outro
        }

        // GET api/profile/customers
        [Route("customers")]
        [HttpGet]
        public List<Person> GetCustomers()
        {
            return new ProfileAccess().GetPersons();
        }

        // GET api/profile/customers
        [Route("customers/phone/{documentNumber}")]
        [HttpGet]
        public List<PhoneLines> GetCustomersPhoneByCPF(string documentNumber)
        {
            return new ProfileAccess().GetCustomersPhoneByCPF(documentNumber);
        }

        // GET api/profile/customers
        [Route("active/customers")]
        [HttpGet]
        public List<Person> GetActiveCustomers()
        {
            return new ProfileAccess().GetActiveCustomers();
        }

        // GET api/profile/customers
        [Route("active/customers/parents")]
        [HttpGet]
        public List<Person> GetActiveCustomersParentList()
        {
            return new ProfileAccess().GetActiveCustomersParentList();
        }

        [Route("all/customers")]
        [HttpGet]
        public List<Person> GetCustomersMinimum(bool minimal)
        {
            return new ProfileAccess().GetAllPersons(minimal);
        }

        [Route("all/customers/list")]
        [HttpGet]
        public List<CustomersListViewModel> GetAllCustomersNew()
        {
            return new ProfileAccess().GetAllCustomers();
        }

        // GET api/profile/validaconexao
        [Route("validaconexao")]
        [HttpGet]
        public bool ValidaConexao()
        {
            return new ContaAcesso().ValidaConexaoDB();
        }

        // GET api/profile/customers
        [Route("charges")]
        [HttpGet]
        public List<Charging> GetChargeHistory(int personID)
        {
            return new ProfileAccess().GetChargingHistory(personID);
        }

        // deprecated
        [Route("phoneOwner")]
        [HttpGet]
        public Person GetChargeHistory(int ddd, Int64 numero)
        {
            return new ProfileAccess().GetPhoneOwner(new Phone { DDD = ddd.ToString(), Number = numero.ToString() });
        }

        [Route("phone/owner")]
        [HttpGet]
        public Person GetChargeHistory(int ddd, int numero, bool onlyFoneclube)
        {
            return new ProfileAccess().GetPhoneOwner(new Phone { DDD = ddd.ToString(), Number = numero.ToString() }, onlyFoneclube);
        }

        // POST api/profile/cadastro
        [Route("register/insert")]
        public IHttpActionResult InsertCadastro(Person personCheckout)
        {
            try
            {
                var statusResponse = new ProfileAccess().SavePerson(personCheckout);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // POST api/profile/update
        [Route("update")]
        public IHttpActionResult UpdatePerson(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().InsertPersonData(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // POST api/profile/update
        [Route("customer/update")]
        public IHttpActionResult UpdateCustomer(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().UpdatePerson(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // POST api/profile/person
        [Route("insert")]
        public IHttpActionResult InitPerson(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().InsertPerson(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // POST api/profile/person
        [Route("updateAdress")]
        public IHttpActionResult UpdatePersonAdress(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().InsertPersonAdress(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // POST api/profile/person
        [Route("charging/insert")]
        public IHttpActionResult SaveChargingHistory(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveChargingHistoryNew(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("charging/schedule/insert")]
        public IHttpActionResult SaveScheduleHistory(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveScheduleHistory(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }


        // GET api/profile/customers
        [Route("delete/hard/customer")]
        [HttpPost]
        public IHttpActionResult HardDeletePerson(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().HardDeletePerson(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("delete/soft/customer")]
        [HttpPost]
        public IHttpActionResult SoftDeletePerson(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SoftDeletePerson(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("delete/undo/customer")]
        [HttpPost]
        public IHttpActionResult UnDeletePerson(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().UnDeletePerson(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("delete/hard/phone")]
        [HttpPost]
        public IHttpActionResult DeletePhoneNumber(Phone phone)
        {
            try
            {
                var statusResponse = new ProfileAccess().DeletePhoneNumber(phone);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("delete/soft/phone")]
        [HttpPost]
        public IHttpActionResult DeactivatePhoneNumber(Phone phone)
        {
            try
            {
                var statusResponse = new ProfileAccess().DeactivePhoneNumber(phone);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        // GET api/profile/customers
        [Route("service/order")]
        [HttpPost]
        public IHttpActionResult InsertServiceOrder(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().InsertServiceOrder(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("customer/parent/insert")]
        [HttpPost]
        public IHttpActionResult SetCustomerParentPhone(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SetCustomerParentPhone(person);
                if (statusResponse)
                    return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, true));
                else
                    return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.NotModified, true));

            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("customer/{idCustomer}/parent")]
        [HttpGet]
        public Person GetCustomerParentPhone(int idCustomer)
        {
            try
            {
                return new ProfileAccess().GetCustomerParent(new Person { Id = idCustomer });
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/phones/search/{nickname}")]
        [HttpGet]
        public List<Phone> GetSearchPhones(string nickname)
        {
            try
            {
                return new ProfileAccess().GetNickNameResults(nickname);
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/pagarme/id/insert")]
        [HttpPost]
        public IHttpActionResult SetPagarmeID(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SetPagarmeID(person);
                if (statusResponse)
                    return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, true));
                else
                    return (IHttpActionResult)ResponseMessage(Request.CreateResponse(HttpStatusCode.NotModified, true));

            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }


        // POST api/profile/comment   by SKG
        [Route("comment")]
        public IHttpActionResult InsertServiceOrderComment(tblServiceOrders order)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveServiceOrder(order);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }
        //GET api/profile/getorders   by SKG
        [Route("getorders")]
        public List<tblServiceOrders> GetAllServiceOrders(int personID)
        {
            List<tblServiceOrders> comments = new List<tblServiceOrders>();
            comments = new ProfileAccess().GetTblServiceOrders(personID);
            return comments;
        }

        [Route("customer/GetParentbyPhone")]
        [HttpGet]
        public PersonParent GetParentbyPhone(long? phoneparent, int personid)
        {
            try
            {
                return new ProfileAccess().GetParentByPhone(phoneparent, personid);
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/GetParentAll")]
        [HttpGet]
        public List<PersonParent> GetParentAll()
        {
            try
            {
                return new ProfileAccess().GetParentAll();
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/GetParentByParentName")]
        [HttpGet]
        public PersonParent GetParentByParentName(string parentname, int personid)
        {
            try
            {
                return new ProfileAccess().GetParentByParentName(parentname, personid);
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }


        [Route("customer/partial/register")]
        [HttpPost]
        public int SetPartialPerson(Person person)
        {
            try
            {
                return new ProfileAccess().SetPartialCustomer(person);
            }
            catch (HttpResponseException)
            {
                return 0;
            }
        }

        [Route("customer/edit")]
        [HttpPost]
        public int UpdateClientDetails(CustomersListViewModel person)
        {
            try
            {
                return new ProfileAccess().UpdateClientDetails(person);
            }
            catch (HttpResponseException)
            {
                return 0;
            }
        }

        [Route("customer/update/line")]
        [HttpPost]
        public int UpdateClientLineDetails(Phone phone)
        {
            try
            {
                return new ProfileAccess().UpdateClientLineDetails(phone);
            }
            catch (HttpResponseException)
            {
                return 0;
            }
        }

        [Route("customer/soft/delete/line")]
        [HttpPost]
        public int SoftDeleteLine(Phone phone)
        {
            try
            {
                return new ProfileAccess().SoftDeleteLine(phone);
            }
            catch (HttpResponseException)
            {
                return 0;
            }
        }

        [Route("customer/ativity")]
        [HttpPost]
        public bool SetPersonAtivity(Person person)
        {
            try
            {
                return new ProfileAccess().SetPersonAtivity(person);
            }
            catch (HttpResponseException)
            {
                return false;
            }
        }

        [Route("customer/nextaction")]
        [HttpPost]
        public bool SetCustomerNextAction(Person person)
        {
            try
            {
                return new ProfileAccess().SetCustomerNextAction(person);
            }
            catch (HttpResponseException)
            {
                return false;
            }
        }

        [Route("customer/parent/id/insert")]
        [HttpPost]
        public bool SetCustomerParent(Person person)
        {
            return new ProfileAccess().SetCustomerParent(person);
        }

        [Route("customer/parent/insta/insert")]
        [HttpPost]
        public int SetParentInfo(PersonParentModel person)
        {
            return new ProfileAccess().InstaSetParentInfo(person);
        }

        [HttpPost]
        [Route("flags/insert")]
        public FlagResponse InsertPhoneFlag(GenericFlag flag)
        {
            return new PhoneAccess().InsertGenericFlag(flag);
        }

        [Route("customer/{idPerson}/flags")]
        [HttpGet]
        public List<Flag> GetAllGenericFlags(string idPerson)
        {
            try
            {
                return new PhoneAccess().GetAllGenericFlags(Convert.ToInt32(idPerson));
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("flags/types")]
        [HttpGet]
        public List<Flag> GetGenericFlagsTypes(bool phoneFlagOnly)
        {
            try
            {
                return new PhoneAccess().GetGenericFlagsTypes(Convert.ToBoolean(phoneFlagOnly));
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/register/log")]
        [HttpGet]
        public List<NovosClientes> GetRegisteredPersonsLog()
        {
            try
            {
                return new PhoneAccess().GetRegisteredPersonsLog();
            }
            catch (HttpResponseException erro)
            {
                throw erro;
            }
        }

        [Route("customer/register/log/text")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var result = JsonConvert.SerializeObject(new PhoneAccess().GetRegisteredPersonsLog());
            var response = new HttpResponseMessage();

            response.Content = new StringContent(result);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [HttpPost]
        [Route("flag/update")]
        public bool UpdatePhoneFlag(GenericFlag flag)
        {
            return new PhoneAccess().UpdatePendingFlagStatus(flag);
        }


        #region Em Desuso

        //deprecated
        // GET api/profile/getlastpaymentmethod by SKG
        [Route("getpaymentmethod")]
        public TblChargingHistoryViewModel GetLastPaymentMethod(int personID)
        {
            TblChargingHistoryViewModel _tblChargingHistoryViewModel = new TblChargingHistoryViewModel();
            tblChargingHistory _tblChargingHistory = new tblChargingHistory();
            _tblChargingHistory = new ProfileAccess().GetLastPaymentMethodById(personID);

            _tblChargingHistoryViewModel.intId = _tblChargingHistory.intId;
            _tblChargingHistoryViewModel.intIdCustomer = _tblChargingHistory.intIdCustomer;
            _tblChargingHistoryViewModel.intIdCollector = _tblChargingHistory.intIdCollector;
            _tblChargingHistoryViewModel.txtCollectorName = _tblChargingHistory.txtCollectorName;
            _tblChargingHistoryViewModel.intIdPaymentType = _tblChargingHistory.intIdPaymentType;
            _tblChargingHistoryViewModel.txtComment = _tblChargingHistory.txtComment;
            _tblChargingHistoryViewModel.dtePayment = Convert.ToDateTime(_tblChargingHistory.dteCreate);
            _tblChargingHistoryViewModel.txtAmmountPayment = _tblChargingHistory.txtAmmountPayment;
            _tblChargingHistoryViewModel.txtTokenTransaction = _tblChargingHistory.txtTokenTransaction;
            _tblChargingHistoryViewModel.intIdBoleto = _tblChargingHistory.intIdBoleto;
            _tblChargingHistoryViewModel.txtAcquireId = _tblChargingHistory.txtAcquireId;
            _tblChargingHistoryViewModel.dteCreate = _tblChargingHistory.dteCreate;
            _tblChargingHistoryViewModel.dteModify = _tblChargingHistory.dteModify;
            _tblChargingHistoryViewModel.intPaymentStatus = _tblChargingHistory.intPaymentStatus;
            _tblChargingHistoryViewModel.txtTransactionComment = _tblChargingHistory.txtTransactionComment;
            _tblChargingHistoryViewModel.dteDueDate = _tblChargingHistory.dteDueDate;
            _tblChargingHistoryViewModel.PhoneId = _tblChargingHistory.PhoneId;
            _tblChargingHistoryViewModel.dteChargingDate = _tblChargingHistory.dteCreate;
            _tblChargingHistoryViewModel.txtChargingComment = _tblChargingHistory.txtChargingComment;

            return _tblChargingHistoryViewModel;
        }

        [Route("cliente")]
        [HttpGet]
        public Person GetPersonData(string documentRegister)
        {
            return new ProfileAccess().GetPerson(documentRegister);
        }

        [Route("cliente/id")]
        [HttpGet]
        public int GetPersonDataId(string documentRegister)
        {
            return new ProfileAccess().GetPerson(documentRegister).Id;
        }

        [Route("cliente/phone/status")]
        [HttpGet]
        public Person GetPersonDataPhoneStatus(string documentRegister)
        {
            return new ProfileAccess().GetPersonWithStatusPhone(documentRegister);
        }

        [Route("charges/document")]
        [HttpGet]
        public List<Charging> GetChargeHistoryDocument(string documentNumber)
        {
            return new ProfileAccess().GetChargingHistory(documentNumber);
        }


        [Route("customer/charges/services/history/{document}")]
        [HttpGet]
        public List<ChargeAndServiceOrderHistory> GetChargeAndServiceOrderHistoryDocument(string document)
        {
            try
            {
                return new ProfileAccess().GetChargingAndServiceOrderHistoryDocument(document);
            }
            catch (HttpResponseException error)
            {
                throw error;
            }
        }


        [Route("customer/plans")]
        [HttpGet]
        public List<Plan> GetCustomerPlans(string documentNumber)
        {
            return new ProfileAccess().GetCustomerPlan(documentNumber);
        }

        [Route("customer/status/new/document/{document}")]
        [HttpGet]
        public bool GetStatusNewDocument(string document)
        {
            return new ProfileAccess().IsNewDocument(document);
        }

        [Route("customer/active/status/{document}")]
        [HttpGet]
        public bool GetCustomerActiveExists(string document)
        {
            return new ProfileAccess().CustomerActiveExists(document);
        }


        #endregion


        #region 
        [Route("customer/basic/info/update")]
        [HttpPost]
        public bool UpdatePersonBasicInfo(PhoneViewModel person)
        {
            return new ProfileAccess().UpdatePersonBasicInfo(person);
        }

        [Route("phone/activation/update")]
        [HttpPost]
        [HttpGet]
        public bool ActivationChangeStatus(int personPhoneId, bool activate)
        {
            return new ProfileAccess().ActivationChangeStatus(personPhoneId, activate);
        }
        #endregion

        [Route("insta/insert/customer")]
        [HttpPost]
        public HttpStatusCode InsertNewCustomerCross(Person person)
        {
            try
            {
                return new ProfileAccess().InstaInsertPerson(person);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("insta/insert/phone")]
        [HttpPost]
        public bool InsertNewPhoneToCustomer(List<tblPersonsPhones> phone)
        {
            try
            {
                return new ProfileAccess().InsertNewPhoneToCustomer(phone);
            }
            catch (HttpResponseException erro)
            {
                return false;
            }
        }

        [Route("insta/register/customerorline")]
        [HttpPost]
        public string InstaInsertCustomerOrLine(InstaRegisterClientOrLineViewModel model)
        {
            try
            {
                tblPersons person;
                return new ProfileAccess().InstaInsertCustomerOrLine(model, out person);
            }
            catch (HttpResponseException erro)
            {
                return "Ocorreu um erro, tente novamente mais tarde";
            }
        }

        [Route("insta/register/customerorline/charge")]
        [HttpPost]
        public string InstaInsertCustomerOrLineAndCharge(InstaRCVCModel model)
        {
            try
            {
                tblPersons person;
                return new ProfileAccess().InstaInsertCustomerOrLine(model.Register, out person);
            }
            catch (HttpResponseException erro)
            {
                return "Ocorreu um erro, tente novamente mais tarde";
            }
        }

        [Route("insta/register/customerorlinewithcharge")]
        [HttpPost]
        public string InstaInsertCustomerOrLineWithCharge(InstaRCVCModel model)
        {
            try
            {
                tblPersons person;
                ProfileAccess profileAccess = new ProfileAccess();
                var stats = profileAccess.InstaInsertCustomerOrLine(model.Register, out person);
                if (stats == "sucesso" && person != null)
                {
                    var charging = new Charging()
                    {
                        DueDate = DateTime.ParseExact(model.ChargeData.DueDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        Comment = model.ChargeData.Comment,
                        Ammount = model.ChargeData.Ammount,
                        AnoVingencia = model.ChargeData.AnoVingencia,
                        MesVingencia = model.ChargeData.MesVingencia,
                        PaymentType = model.ChargeData.PaymentType,
                        InstaRegsiterData = model.Register.IsActivateBeforePayment ? "" : model.ChargeData.InstaRegsiterData
                    };
                    var status = profileAccess.InstaChargeClient(person, charging, model.ChargeData.CardId);
                    if (status == 0)
                        return stats;
                    else
                        return "Ocorreu um erro ao carregar o cliente";
                }
                else
                    return stats;
            }
            catch (Exception ex)
            {
                return "Ocorreu um erro, tente novamente mais tarde";
            }
        }

        [Route("cross/insert")]
        [HttpPost]
        public RegisterResponse InsertNewCustomerCross(CustomerCrossRegisterViewModel customeRegisterViewModel)
        {
            try
            {
                return new ProfileAccess().InsertNewCustomerCross(customeRegisterViewModel);
                //return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("cross/register/insert")]
        [HttpPost]
        public RegisterResponse InsertNewCustomerRegisterCross(CustomerCrossRegisterViewModel customeRegisterViewModel)
        {
            try
            {
                return new ProfileAccess().InsertNewCustomerRegisterCross(customeRegisterViewModel);
                //return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                if (string.IsNullOrEmpty(responseMessage.ReasonPhrase))
                {
                    throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Ocorreu um erro na tentativa de cadastro, entre em contato no número: +55 (21) 97338-9882. ( Celular e Whatsapp ) "));
                }



                throw erro;
            }
        }

        [Route("cross/address/insert")]
        [HttpPost]
        public bool InsertNewCustomerAddressCross(CustomerAddressViewModel customerAddressViewModel)
        {
            try
            {
                return new ProfileAccess().SavePersonAddressFC(customerAddressViewModel);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("cross/update/phone/port")]
        [HttpPost]
        public bool InsertPhonesPendingToPort(PhonesPendingToPortViewModel phonesPendingToPortViewModel)
        {
            try
            {
                return new ProfileAccess().InsertPhonesPendingToPort(phonesPendingToPortViewModel);
                //return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("client/insta-charge")]
        [HttpPost]
        public InstaChargeResponse InstaRegisterAndCharge(CustomerInstaChargeViewModel customerInstaChargeViewModel)
        {
            try
            {
                return new ProfileAccess().InstaRegisterAndCharge(customerInstaChargeViewModel);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("login/user/{user}/pass/{password}/")]
        [HttpGet]
        public UserLogin GetLoginAuthentication(string user, string password)
        {
            try
            {
                var senha = new Utils().Base64Decode(password);
                return new ProfileAccess().GetLoginAuthenticationUser(user, senha);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("password/hash/{password}")]
        [HttpGet]
        public string GetHashedPassword(string password)
        {
            try
            {
                return new FoneClube.DataAccess.security.Security().EncryptPassword(password).Password;
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/registrados/customer/{id}")]
        [HttpGet]
        public CustomerMinhaContaViewModel GetCadastrosRealizados(string id)
        {
            try
            {
                return new ProfileAccess().GetCadastrosRealizados(Convert.ToInt32(id));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/registrados/customer/{id}/indicado")]
        [HttpGet]
        public bool GetCustomerIndicado(string id)
        {
            try
            {
                return new ProfileAccess().GetCustomerIndicado(Convert.ToInt32(id));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/pessoais/registro/customer/inserir")]
        [HttpPost]
        public bool SetCadastroPessoal(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                return new ProfileAccess().SetCadastroPessoal(customerMinhaConta);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/senha/registro/customer/inserir")]
        [HttpPost]
        public bool SetCadastroSenha(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                return new ProfileAccess().SetCadastroSenha(customerMinhaConta);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/endereco/registro/customer/inserir")]
        [HttpPost]
        public bool SetCadastroEndereco(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                return new ProfileAccess().SetCadastroEndereco(customerMinhaConta);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/registrados/recover/{recover}")]
        [HttpGet]
        public bool GetRecoverPassword(string recover)
        {
            try
            {
                var recovered = new Utils().Base64Decode(recover);
                return new ProfileAccess().GetRecoverPassword(recovered);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        [Route("dados/registrados/recover/second/{recover}")]
        [HttpGet]
        public TrocaSenhaViewModel GetMensagemTrocaSenha(string recover)
        {
            try
            {
                return new ProfileAccess().GetMensagemTrocaSenha(recover);
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                throw erro;
            }
        }

        // POST api/profile/person
        [Route("saveDefaultPaymentInfo")]
        public IHttpActionResult SaveDefaultPaymentInfo(Person person)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveDefaultPaymentInfo(person);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("update/usar2precos/{personId}/{status}")]
        [HttpGet]
        public IHttpActionResult SaveUsar2Preços(string personId, string status)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveUsar2Preços(Convert.ToInt32(personId), Convert.ToBoolean(status));
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("update/settings")]
        [HttpPost]
        public IHttpActionResult SaveUserSettings(UserSettings userSettings)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveUserSettings(userSettings);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("activate/mvno/{transactionId}")]
        [HttpGet]
        public IHttpActionResult ActivateTransaction(string transactionId)
        {
            try
            {
                new ProfileAccess().ActivateMVNOPlansPostPayment(Convert.ToInt32(transactionId));
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(new HttpStatusCode() { }, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("grid/get/state/{gridname}")]
        [HttpGet]
        public List<tblAgGridState> GetGridStates(string gridname)
        {
            try
            {
                return new ProfileAccess().GetGridStates(gridname);
            }
            catch (HttpResponseException erro)
            {
                return null;
            }
        }

        [Route("grid/save/state")]
        [HttpPost]
        public IHttpActionResult SaveGridState(tblAgGridState gridState)
        {
            try
            {
                var statusResponse = new ProfileAccess().SaveGridState(gridState);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("grid/delete/state")]
        [HttpPost]
        public IHttpActionResult DeleteGridState(tblAgGridState gridState)
        {
            try
            {
                var statusResponse = new ProfileAccess().DeleteGridState(gridState);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("grid/update/default/state")]
        [HttpPost]
        public IHttpActionResult UpdateDefaultGridState(tblAgGridState gridState)
        {
            try
            {
                var statusResponse = new ProfileAccess().UpdateDefaultState(gridState);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("cart/update/unplaced")]
        [HttpPost]
        public IHttpActionResult UpdateUnplacedCart(tblUnPlacedCartItems cartItems)
        {
            try
            {
                var statusResponse = new ProfileAccess().UpdateUnplacedCart(cartItems);
                return (IHttpActionResult)ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException erro)
            {
                var responseMessage = new HttpResponseMessage(erro.Response.StatusCode);
                responseMessage.ReasonPhrase = erro.Response.ReasonPhrase;
                return (IHttpActionResult)ResponseMessage(responseMessage);
            }
        }

        [Route("cart/get/unplaced/{personId}")]
        [HttpGet]
        public tblUnPlacedCartItems GetUnplacedCart(string personId)
        {
            try
            {
                return new ProfileAccess().GetUnplacedCart(Convert.ToInt32(personId));
            }
            catch (HttpResponseException erro)
            {
                return null;
            }
        }

        [Route("get/orders/{personId}")]
        [HttpGet]
        public List<StoreOrder> GetCartOrders(string personId)
        {
            try
            {
                return new ProfileAccess().GetCartOrders(Convert.ToInt32(personId));
            }
            catch (HttpResponseException erro)
            {
                return new List<StoreOrder>();
            }
        }

        [Route("get/charges/{personId}")]
        [HttpGet]
        public List<StoreCharges> GetChargeHistoryForStore(string personId)
        {
            try
            {
                return new ProfileAccess().GetChargeHistoryForStore(Convert.ToInt32(personId));
            }
            catch (HttpResponseException erro)
            {
                return new List<StoreCharges>();
            }
        }

        [Route("intl/all")]
        [HttpGet]
        public List<Person> GetCustomersIntlMinimum()
        {
            return new ProfileAccess().GetIntlPersons();
        }

        [Route("register/user/intl")]
        [HttpPost]
        public string RegisterInternationCustomer(Person person)
        {
            try
            {
                return new ProfileAccess().RegisterInternationCustomer(person);
            }
            catch (Exception erro)
            {
                return "Error";
            }
        }

        [Route("user/intl/refund")]
        [HttpPost]
        public string InitiateRefund(InitiateRefund refund)
        {
            try
            {
                return new ProfileAccess().InitiateRefund(refund);
            }
            catch (Exception erro)
            {
                return "Error";
            }
        }

        [Route("intl/user")]
        [HttpPost]
        public IntlUserDashboard GetIntlUserData(GetIntlDataReq request)
        {
            return new ProfileAccess().GetIntlUserData(request);
        }
    }
}