using FoneClube.BoletoSimples.Common;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
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
using System.Web.Http;
using System.Web.Http.Description;

namespace FoneClube.WebAPI.Controllers
{
    public class MVNOController : Controller
    {
        [HttpGet]
        [Route("validate/iccid/{iccid}")]
        public ValidateICCIDResponse ValidateICCID(string iccid)
        {
            try
            {
                return new MVNOAccess().ValidateICCID(iccid);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("validate/cpf/{cpf}")]
        public CPFUIResponse ValidateCPF(string cpf)
        {
            try
            {
                return new MVNOAccess().ValidateCPF(cpf);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("port/operator/list")]
        public PortabilidadeResponse GetPortabilidadeOperators()
        {
            try
            {
                return new MVNOAccess().GetPortabilidadeOperators();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/port/{from}/{to}/{ddd}")]
        public ActivatePortResponse Port(string from, string to, string ddd)
        {
            try
            {
                ActivatePortRequest portRequest = new ActivatePortRequest();
                portRequest.numero_contel = from;
                portRequest.doador_numero = to;
                portRequest.doador_id_operadora = ddd == "0" ? 52 : Convert.ToInt32(ddd);

                return new MVNOAccess().PortNumber(portRequest);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/sync/lines")]
        public bool SyncAllLinesFromContel()
        {
            try
            {
                return new MVNOAccess().SyncAllLinesFromContel();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/sync/lines-saldo")]
        public bool SyncAllLinesFromContelWithSaldo()
        {
            try
            {
                return new MVNOAccess().SyncAllLinesFromContelWithSaldo();
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/sync/inlt/saldo")]
        public bool SyncSaldoIntl()
        {
            try
            {
                return new MVNOAccess().SyncSaldoIntl();
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/sync/iccid")]
        public bool SyncESIMICCIDPool()
        {
            try
            {
                return new MVNOAccess().GetNewICCIDsForESIM();
            }
            catch (Exception ex)
            {
                throw ex;
                //return false;
            }
        }

        [HttpGet]
        [Route("contel/sync/lines-saldo/customer/{personId}")]
        public bool SyncAllLinesFromContelWithSaldo(string personId)
        {
            try
            {
                return new MVNOAccess().SyncAllLinesFromContelWithSaldoPerson(Convert.ToInt32(personId));
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/get/{phone}")]
        public ContelPhoneData GetContelLinesByPhone(string phone)
        {
            try
            {
                return new MVNOAccess().GetContelLinesByPhone(phone);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/get/saldo/{phone}")]
        public SaldoResponse GetContelLinesBySaldoPhone(string phone)
        {
            try
            {
                var mvnoAccess = new MVNOAccess();
                var saldo = mvnoAccess.GetSaldo(phone);
                mvnoAccess.UpdateSlado(saldo, phone);
                return saldo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/add/line/{phone}")]
        public bool AddContelLineManual(string phone)
        {
            try
            {
                var mvnoAccess = new MVNOAccess();
                mvnoAccess.AddContelLineManual(phone);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/plans")]
        public PlanosList GetContelPlans()
        {
            try
            {
                return new MVNOAccess().GetContelPlans();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/deliver/sim/{transactionId}")]
        public bool DeliverSim(string transactionId)
        {
            try
            {
                new ProfileAccess().DeliverStoreSim(Convert.ToInt32(transactionId));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        [HttpGet]
        [Route("contel/activate/{transactionId}")]
        public bool ActivatePlan(string transactionId)
        {
            try
            {
                new ProfileAccess().ActivateStoreEsim(Convert.ToInt32(transactionId));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        [Route("contel/topup")]
        public TopUpUIPlanResponse AddTopupPlan(TopUpUIPlanRequest request)
        {
            try
            {
                return new MVNOAccess().AddTopupPlan(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("contel/block")]
        public BlockLineResponse BlockLine(BlockLine request)
        {
            try
            {
                request.motivo = "BLOQUEIO DE IMEI";
                request.observacoes = "";
                var mvno = new MVNOAccess();
                var result = mvno.BlockLine(request);
                mvno.UpdateContelBlockUnBlockLine(request.numero);
                mvno.LogBlockUnBlock(request.numero, "B");
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("contel/unblock")]
        public BlockLineResponse UnBlockLine(UnBlockLine request)
        {
            try
            {
                var mvno = new MVNOAccess();
                var result = mvno.UnBlockLine(request);
                mvno.UpdateContelBlockUnBlockLine(request.numero);
                mvno.LogBlockUnBlock(request.numero, "U");
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("contel/block/customer")]
        public List<BlockLineResponseUI> BlockLineByCustomer(BlockRequest request)
        {
            try
            {
                return new MVNOAccess().BlockLineByCustomer(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("contel/unblock/customer")]
        public List<BlockLineResponseUI> UnBlockLineByCustomer(BlockRequest request)
        {
            try
            {
                return new MVNOAccess().UnBlockLineByCustomer(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("contel/auto/block/customer/nonvip")]
        public bool AutoBlockLineByCustomer()
        {
            try
            {
                new MVNOAccess().AutoBlockLineByCustomer();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/auto/unblock/{transactionId}")]
        public bool AutoUnBlockLineByCustomer(string transactionId)
        {
            try
            {
                new MVNOAccess().AutoUnBlockLineByCustomer(Convert.ToInt32(transactionId));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [HttpGet]
        [Route("contel/topup/history/{line}")]
        public TopupHistoryUIResponse GetTopupHistory(string line)
        {
            try
            {
                return new MVNOAccess().GetTopupHistory(line);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("history/mobimatter")]
        public FacilHistoryResponse GetMobimatterHistory()
        {
            try
            {
                return new MVNOAccess().GetMobimatterHistory();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/notify/lessgb")]
        public bool GetLessDBLines()
        {
            try
            {
                new MVNOAccess().GetSaldoLessThanGBs();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/get/saldo")]
        public RemainingSaldo GetRemainingSaldoForCompany()
        {
            try
            {
                return new MVNOAccess().GetRemainingSaldoForCompany();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("contel/get/saldo/log")]
        public bool GetRemainingSaldoForCompanyLog()
        {
            try
            {
                MVNOAccess mvno = new MVNOAccess();
                return mvno.LogContelSaldoStartOfDay();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/sync/topup/history")]
        public bool SyncTopupHistory()
        {
            try
            {
                new MVNOAccess().SyncTopupHistory();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        [Route("contel/import/topup/history")]
        public bool ImportTopupHistory(List<ImportTopupHistory> lstHistory)
        {
            try
            {
                new MVNOAccess().ImportTopupHistory(lstHistory);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/update/apelido")]
        public bool UpdateApelidoForAllLines()
        {
            try
            {
                return new MVNOAccess().UpdateApelidoForAllLines();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/update/auth/token/{env}")]
        public bool UpdateAuthToken(string env)
        {
            try
            {
                return new MVNOAccess().GetAuthToken(env);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("contel/recover/lines")]
        public bool SyncAllLinesFromContelManual()
        {
            try
            {
                MVNOAccess objMVNOAccess = new MVNOAccess();
                return objMVNOAccess.SyncAllLinesFromContel();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //[HttpGet]
        //[Route("contel/sync/lines/paging")]
        //public bool SyncAllLinesFromContelPaging()
        //{
        //    try
        //    {
        //        return new MVNOAccess().SyncAllLinesFromContelPaging();
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        [HttpGet]
        [Route("contel/pdf/read/{phone}")]
        public bool ReadPdf(string phone)
        {
            try
            {
                return new MVNOAccess().DownloadActivationFile(string.Empty, phone);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("lines/to/pool")]
        public bool GetAllEligibleLinesForPool()
        {
            try
            {
                MVNOAccess objMVNOAccess = new MVNOAccess();
                objMVNOAccess.GetAllEligibleLinesForPool();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        [Route("line/reset/{phone}")]
        public ResetLineRes ResetLine(string phone)
        {
            try
            {
                MVNOAccess objMVNOAccess = new MVNOAccess();
                ResetLine resetReq = new ResetLine()
                {
                    linha = phone,
                    motivo = "Troca para eSIM",
                    novo_iccid = ""
                };
                return objMVNOAccess.ResetLine(resetReq);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}