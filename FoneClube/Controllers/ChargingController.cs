using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.Cielo;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.charging;
using FoneClube.DataAccess;
using Microsoft.AspNetCore.Mvc;
using PagarMe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FoneClube.WebAPI.Controllers
{
    public class ChargingController : Controller
    {
      
        [Route("cobranca/refresh/claro/mes/{mes:int}/ano/{ano:int}")]
        public bool GetStatusRefreshClaro(int mes, int ano)
        {
            return new ChargingAcess().CobrancaClaroRefresh(mes, ano);
        }

        [HttpGet]
        [Route("cobranca/refresh/vivo/mes/{mes:int}/ano/{ano:int}")]
        public bool GetStatusRefreshVivo(int mes, int ano)
        {
            return new ChargingAcess().CobrancaVivoRefresh(mes, ano);
        }

        [HttpGet]
        [Route("cobranca/status/vingencia/mes/{mes:int}/ano/{ano:int}")]
        public List<Person> GetStatusVingencia(int mes, int ano)
        {
            return new ChargingAcess().GetValidityPayments(new Charging
            {
                AnoVingencia = (ano).ToString(),
                MesVingencia = (mes).ToString()
            });
        }

        [HttpGet]
        [Route("cobranca/status/mes/{mes:int}/ano/{ano:int}")]
        public List<Person> GetMonthChargings(int mes, int ano)
        {
            return new ChargingAcess().GetMonthChargings(new Charging
            {
                AnoVingencia = (ano).ToString(),
                MesVingencia = (mes).ToString()
            });
        }

        [HttpGet]
        [Route("cobranca/status/vingencia/cliente/{customerId:int}/mes/{mes:int}/ano/{ano:int}")]
        public List<Person> GetCustomerStatusVingencia(int customerId, int mes, int ano)
        {
            return new ChargingAcess().GetCustomerValidityPayments(new Charging
            {
                AnoVingencia = (ano).ToString(),
                MesVingencia = (mes).ToString()
            },
                customerId
            );
        }

        [HttpGet]
        [Route("cobranca/full/vivo/mes/{mes:int}/ano/{ano:int}")]
        public List<CobFullVivo_Extract_Result> GetCobrancaFullVivoExtract(int mes, int ano)
        {
            return new ChargingAcess().GetCobrancaFullVivoExtract(mes, ano);
        }

        [HttpPost]
        [Route("log/person/id/{id}")]
        public bool SetChargingLog(Charging charging, string id)
        {
            return new ChargingAcess().SetChargingLog(charging.SerializedCharging, id);
        }

        [HttpGet]
        [Route("history/log/person/id/{id}")]
        public List<string> GetChargingList(string id)
        {
            return new ChargingAcess().GetChargingList(Convert.ToInt32(id));
        }

        [HttpGet]
        [Route("last/customer/{personId}")]
        public GetLastChargingHistory_Result GetLastCharge(string personId)
        {
            return new ChargingAcess().GetLastCharge(Convert.ToInt32(personId));
        }

        [Route("history")]
        [HttpGet]
        public List<ChargeAndServiceOrderHistory> GetChargeAndServiceOrderHistory(int personID)
        {
            try
            {
                return new ProfileAccess().GetChargingServiceOrdersHistory(personID);
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                throw error;
            }
        }

        [HttpPost]
        [Route("update/id/{chargingId}/canceled/{canceled}")]
        public bool UpdateCharging(int chargingId, bool canceled)
        {
            return new ChargingAcess().UpdateCharging(chargingId, canceled);
        }

        [HttpPost]
        [Route("update/canceled/{canceled}")]
        public bool UpdateChargings(List<int> chargings, bool canceled)
        {
            return new ChargingAcess().UpdateChargingList(chargings, canceled);
        }

        [HttpPost]
        [Route("cielo/history/insert")]
        public bool InsertCieloCharging(CieloPaymentModel cieloPayment)
        {
            if (cieloPayment != null)
                return new ChargingAcess().InsertCieloCharging(cieloPayment);
            else
                return false;
        }

        [HttpPost]
        [Route("cielo/transaction/insert")]
        public CieloChargingResponse InsertCieloTransaction(Person person)
        {
            return new ChargingAcess().InsertCieloTransaction(person);
        }

        [HttpGet]
        [Route("last/customers/chargings")]
        public List<Person> GetLastChargeCustomer()
        {
            return new ChargingAcess().GetChargingsCustomers();
        }

        #region Desuso ( boleto simples )

        [Route("{year:int}/{month:int}/clients")]
        [System.Web.Http.Description.ResponseType(typeof(List<Person>))]
        public IHttpActionResult GetClients(int year, int month)
        {

            try
            {
                var clients = new ChargingAcess().GetClients(year, month);

                if (!clients.Any())
                {
                    return (IHttpActionResult)NotFound();
                }
                return (IHttpActionResult)Ok(clients);
            }
            catch (Exception ex)
            {
                return (IHttpActionResult)InternalServerError(ex);
            }

        }

        private IHttpActionResult InternalServerError(Exception ex)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{year:int}/{month:int}/clients/{clientId}/charging")]
        public async Task<IHttpActionResult> ChargeClient(Charging charging, int year, int month)
        {
            try
            {
                charging.ChargingDate = new DateTime(year, month, 1);
                var result = await new ChargingAcess().ChargeClient(charging);
                return (IHttpActionResult)Created("", result);
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{year:int}/{month:int}/clients/{clientId}/charging/{chargingId}")]
        public IHttpActionResult UpdateChargeStatus(Charging charging, int chargingId)
        {
            try
            {
                charging.Id = chargingId;
                var result = new ChargingAcess().UpdateChargingHistory(charging);
                if (result) return (IHttpActionResult)Ok();
                else return (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("boletosimples/direct")]
        public async Task<Charging> ChargeBoletoDirect(CheckoutBoletoSimples checkout)
        {

            try
            {
                var addresses = new List<Adress>();
                addresses.Add(new Adress
                {
                    Street = checkout.Street,
                    Neighborhood = checkout.Neighborhood,
                    State = checkout.State,
                    City = checkout.City,
                    Cep = checkout.Cep,
                    Complement = checkout.Complement,
                    StreetNumber = checkout.StreetNumber
                });

                var person = new Person
                {
                    DocumentNumber = checkout.DocumentNumber,
                    Name = checkout.Name,
                    Email = checkout.Email,
                    Adresses = addresses
                };
                var charging = new Charging { Ammount = checkout.Ammount, Comment = checkout.ChargingComment };
                return await new ChargingAcess().CreateBankBilletDirect(person, charging);
            }
            catch (Exception ex)
            {

                throw ex;
            }



            //return Ok();
        }

        [HttpGet]
        [Route("mass")]
        public List<Person> GetListaCobrancaMassiva(string mes, string ano)
        {
            return new ChargingAcess().GetListaCobrancaMassiva(Convert.ToInt32(mes), Convert.ToInt32(ano));
        }

        [HttpGet]
        [Route("last")]
        public List<LastCharging> GetListaCobrancaMassiva(List<int> matriculas)
        {
            return new ChargingAcess().GetLastChargings(matriculas);
        }

        [HttpGet]
        [Route("mass/full/mes/{mes}/ano/{ano}")]
        public MassChargingList GetMassChargingCustomers(string mes, string ano)
        {
            return new ChargingAcess().GetMassChargingCustomers(Convert.ToInt32(mes), Convert.ToInt32(ano));
        }

        [HttpGet]
        [Route("schedule/execute")]
        public bool ExecuteSchduleChargings()
        {
            return new ChargingAcess().ExecuteCharges();
        }

        [HttpGet]
        [Route("schedule/executed/date")]
        public DateTime? GetScheduleExecutedDate()
        {
            return new ChargingAcess().GetScheduleExecutedDate();
        }



        [Route("schedule/history/{matricula}")]
        [HttpGet]
        public List<Agendamento> GetScheduleChargeHistory(string matricula)
        {
            try
            {
                return new ChargingAcess().GetScheduleChargeHistory(Convert.ToInt32(matricula));
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                throw error;
            }
        }

        [HttpPost]
        [Route("schedule/update")]
        public bool ImportDrCelularData(UpdateAgendamento agendamento)
        {
            return new ChargingAcess().UpdateScheduleCharging(agendamento);
        }

        [Route("schedule/delete/{id}")]
        [HttpGet]
        public bool DeleteScheduleCharging(string id)
        {
            try
            {
                return new ChargingAcess().DeleteScheduleCharging(Convert.ToInt32(id));
            }
            catch (System.Web.Http.HttpResponseException error)
            {
                throw error;
            }
        }

        [HttpGet]
        [Route("history/{personId}/{chargingId}")]
        public Person GetChargingById(string personId, string chargingId)
        {
            return new ChargingAcess().GetChargingById(Convert.ToInt32(personId), Convert.ToInt32(chargingId));
        }


        [HttpGet]
        [Route("schedule/execute/fivedayemailreminder/{days}")]
        public bool ExecuteSchduleChargings(string days)
        {
            return new ChargingAcess().ExecuteChargesFor5DaysReminder(Convert.ToInt32(days));
        }

        [HttpGet]
        [Route("schedule/execute/creditcard")]
        public bool ExecuteSchduleChargingsCC()
        {
            return new ChargingAcess().ExecuteChargesForCC();
        }

        [HttpGet]
        [Route("set/flag/{chargeId}/{blnEnable}")]
        public bool SetChargingFlagByUser(string chargeId, bool blnEnable)
        {
            return new ChargingAcess().SetChargingFlagByUser(Convert.ToInt32(chargeId), blnEnable);
        }

        [HttpGet]
        [Route("drcelular/verify/{ano}/{mes}/{operanto}/{empresa}")]
        public bool ImportDrCelularData(string ano, string mes, string operanto, string empresa)
        {
            return new ChargingAcess().IsExistsDrCelularData(Convert.ToInt32(ano), Convert.ToInt32(mes), operanto, empresa);
        }

        [HttpPost]
        [Route("drcelular/import")]
        public bool ImportDrCelularData(DrCelularData tblDrCelular)
        {
            return new ChargingAcess().ImportDrCelularData(tblDrCelular.tblDrCelularTemps);
        }
        //GetMassChargingCustomers

        #endregion
        [HttpGet]
        [Route("expired/charge")]
        public bool CreateExpiredCharge()
        {
            return new ChargingAcess().CreateExpiredCharge();
        }

        [HttpGet]
        [Route("cc/refused/charge")]
        public bool CreateRefusedCharge()
        {
            return new ChargingAcess().CreateRefusedCharge();
        }

        [HttpGet]
        [Route("pagarme/transactions/{start}/{end}")]
        public List<GetPagarmeTransactionReport_Result> GetAllPagarmeTransactions(string start, string end)
        {
            return new ChargingAcess().GetAllPagarmeTransactions(start, end);
        }

        [HttpGet]
        [Route("intl/sales/{start}/{end}")]
        public List<GetInternationalTransactionReport_Result> GetAllIntlSalesTransactions(string start, string end)
        {
            return new ChargingAcess().GetAllIntlSalesTransactions(start, end);
        }

        [HttpGet]
        [Route("intl/deposits/{start}/{end}")]
        public List<GetInternationalDepositsReport_Result> GetAllIntlDeposits(string start, string end)
        {
            return new ChargingAcess().GetAllIntlDeposits(start, end);
        }
    }
}
