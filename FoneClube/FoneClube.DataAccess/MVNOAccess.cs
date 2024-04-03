using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using FoneClube.DataAccess.Utilities;
using FoneClube.Business.Commons;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.WhatsApp;
using Business.Commons.Utils;
using Newtonsoft.Json.Serialization;
using System.Data.Entity;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FoneClube.DataAccess
{
    public class MVNOAccess
    {
        string BaseAddress = "https://api.conteltelecom.com.br/";
        // const string Token = "tXDG8Leh095FIokQPGvZQOlbOPK8CsBAh9jB6wmj";
        string Token = "nmvySMadOJFeZVolwdaO0BLOucj6uBsn6BerFM0A";
        string DevToken = "";
        string AdminMsgsTo = "552192051599";
        string AdminMsgsToSaldo = "5521920151599,919004453881,5521967316528";
        public MVNOAccess()
        {
            using (var ctx = new FoneClubeContext())
            {
                var token = ctx.tblConfigSettings.Where(x => x.txtConfigName == "MVNOBearerToken").FirstOrDefault();
                if (token != null)
                    Token = token.txtConfigValue;

                var devtoken = ctx.tblConfigSettings.Where(x => x.txtConfigName == "MVNOBearerTokenDev").FirstOrDefault();
                if (devtoken != null)
                    DevToken = devtoken.txtConfigValue;

                var numbers = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppMsgToAdmins").FirstOrDefault();
                AdminMsgsTo = numbers.txtConfigValue;
                AdminMsgsToSaldo = "552192051599";
            }
        }


        public ValidateICCIDResponse ValidateICCID(string iccid, string environment = "PROD")
        {
            ValidateICCIDResponse validateICCIDResponse = null;
            try
            {
                if (!string.IsNullOrEmpty(iccid))
                {
                    string apiMethod = string.Format("ativacao/detalhar?iccid=" + iccid);

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog()
                            {
                                dteTimeStamp = DateTime.Now,
                                txtAction = "MVNOAccess: ValidateICCID: iccid :" + iccid + " response: " + contentresponse
                            });
                            ctx.SaveChanges();
                        }

                        validateICCIDResponse = JsonConvert.DeserializeObject<ValidateICCIDResponse>(contentresponse);

                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = "MVNOAccess: ValidateICCID error: " + ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
            return validateICCIDResponse;
        }

        public ValidateNumberByICCIDRes ValidatePhoneByICCID(string iccid, string environment = "PROD")
        {
            ValidateNumberByICCIDRes validateICCIDResponse = null;
            try
            {
                if (!string.IsNullOrEmpty(iccid))
                {
                    string apiMethod = string.Format("linhas/numeroPeloIccid?iccid=" + iccid);

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        LogHelper.LogMessageOld(1, "ValidatePhoneByICCID : " + iccid + " : response:" + contentresponse);

                        validateICCIDResponse = JsonConvert.DeserializeObject<ValidateNumberByICCIDRes>(contentresponse);

                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = "MVNOAccess: ValidatePhoneByICCID error: " + ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
            return validateICCIDResponse;
        }

        public CPFUIResponse ValidateCPF(string cpf)
        {
            CPFUIResponse cpfRes = new CPFUIResponse() { status = -1 };
            try
            {

                string apiMethod = "validar/cpf";
                string JsonStr = "";
                var requst = new CPFRequest() { cpf = cpf };
                JsonStr = JsonConvert.SerializeObject(requst, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                if (!string.IsNullOrEmpty(cpf) && cpf.Length > 11)
                {
                    apiMethod = "validar/cnpj";
                    var requst1 = new CNPJRequest() { cnpj = cpf };
                    JsonStr = JsonConvert.SerializeObject(requst1, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                }
                StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog()
                        {
                            dteTimeStamp = DateTime.Now,
                            txtAction = "MVNOAccess: ValidateCPF: " + contentresponse
                        });
                        ctx.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(cpf) && cpf.Length > 11)
                    {
                        var resp = JsonConvert.DeserializeObject<CNPJConteResponse>(contentresponse);
                        if (resp != null)
                        {
                            cpfRes.status = resp.valido ? 1 : -1;
                            if (resp.data != null)
                                cpfRes.nome = resp.data.razao_social;
                        }
                    }
                    else
                    {
                        var resp = JsonConvert.DeserializeObject<CPFConteResponse>(contentresponse);
                        if (resp != null)
                        {
                            cpfRes.status = resp.valido ? 1 : -1;
                            if (resp.data != null)
                                cpfRes.nome = resp.data.titular;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return cpfRes;
        }

        public bool GetAuthToken(string env = "PROD")
        {
            AuthTokenResponse authTokenResponse = null;
            try
            {
                if (env == "PROD")
                {
                    var authReq = new AuthTokenRequest()
                    {
                        chave_acesso = "d5af44b8e838817d1f453472e42356eb980bdace83702953ecd75e92ddfd4054",
                        chave_acesso_franquia = "LIhAQDmfkeor0HrM7CfBww3gx957I41mT1pxME4f5TskXUuwXBgKAmAYOBMRHpXp"
                    };

                    var JsonStr = JsonConvert.SerializeObject(authReq, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("auth/token");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        authTokenResponse = JsonConvert.DeserializeObject<AuthTokenResponse>(contentresponse);

                        if (authTokenResponse != null && authTokenResponse.retorno)
                        {
                            using (var ctx = new FoneClubeContext())
                            {
                                var config = ctx.tblConfigSettings.FirstOrDefault(x => x.txtConfigName == "MVNOBearerToken");
                                config.txtConfigValue = authTokenResponse.token;
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    var authReq = new AuthTokenDevRequest()
                    {
                        chave_acesso = "d5af44b8e838817d1f453472e42356eb980bdace83702953ecd75e92ddfd4054",
                        chave_acesso_franquia = "LIhAQDmfkeor0HrM7CfBww3gx957I41mT1pxME4f5TskXUuwXBgKAmAYOBMRHpXp",
                        ambiente = "DEV"
                    };

                    var JsonStr = JsonConvert.SerializeObject(authReq, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("auth/token");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        authTokenResponse = JsonConvert.DeserializeObject<AuthTokenResponse>(contentresponse);

                        if (authTokenResponse != null && authTokenResponse.retorno)
                        {
                            using (var ctx = new FoneClubeContext())
                            {
                                var config = ctx.tblConfigSettings.FirstOrDefault(x => x.txtConfigName == "MVNOBearerTokenDev");
                                config.txtConfigValue = authTokenResponse.token;
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan error" + ex.ToString() });
                    ctx.SaveChanges();
                }
                return false;
            }
            return true;
        }

        public ActivatePlanResponse ActivatePlan(ActivatePlanRequest request, string environment = "PROD")
        {
            ActivatePlanResponse activatePlanResponse = null;
            try
            {
                if (request != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan Request: " + JsonStr });
                        ctx.SaveChanges();
                    }

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("ativacao/solicitar");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan Response:" + contentresponse });
                            ctx.SaveChanges();
                        }

                        activatePlanResponse = JsonConvert.DeserializeObject<ActivatePlanResponse>(contentresponse);
                    }
                }
            }
            catch (Exception ex)
            {
                activatePlanResponse = new ActivatePlanResponse();
                activatePlanResponse.mensagem = "Contel API error";

                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return activatePlanResponse;
        }

        public ActivatePortResponse PortNumber(ActivatePortRequest request)
        {
            ActivatePortResponse activatePortResponse = null;
            try
            {
                if (request != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("portabilidade/solicitar");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "PortNumber Response" + contentresponse });
                            ctx.SaveChanges();
                        }

                        activatePortResponse = JsonConvert.DeserializeObject<ActivatePortResponse>(contentresponse);
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "PortNumber error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return activatePortResponse;
        }

        public PortabilidadeResponse GetPortabilidadeOperators()
        {
            PortabilidadeResponse portabilidadeResponse = null;
            try
            {
                string apiMethod = string.Format("portabilidade/operadorasDisponiveis");

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    portabilidadeResponse = JsonConvert.DeserializeObject<PortabilidadeResponse>(contentresponse);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return portabilidadeResponse;
        }

        public bool GetNewICCIDsForESIM()
        {
            ESIMICCIDResponse eResponse = null;
            try
            {
                string apiMethod = string.Format("linhas/estoque");

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    eResponse = JsonConvert.DeserializeObject<ESIMICCIDResponse>(contentresponse);

                    if (eResponse != null && eResponse.retorno && eResponse.data != null && eResponse.data.Count > 0)
                    {
                        var listData = eResponse.data;
                        int intLimit = 40;
                        LogHelper.LogMessageOld(0, "GetNewICCIDsForESIM total count:" + listData.Count);
                        using (var ctx = new FoneClubeContext())
                        {
                            int intLoop = 1;
                            foreach (var data in listData)
                            {
                                if (intLoop % intLimit == 0)
                                {
                                    LogHelper.LogMessageOld(0, "Waiting for 2sec post 40 records");
                                    System.Threading.Thread.Sleep(1000);
                                }

                                var activationCode = DownloadActivationFileByICCID(data.pdf_esim, data.iccid);

                                if (!string.IsNullOrEmpty(activationCode))
                                {
                                    var esimExists = ctx.tblESimICCIDPool.Any(x => x.txtICCID == data.iccid && x.txtTipo == data.tipo);
                                    if (!esimExists && string.Equals(data.tipo, "eSIM", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctx.tblESimICCIDPool.Add(new tblESimICCIDPool()
                                        {
                                            txtICCID = data.iccid,
                                            bitActivated = false,
                                            txtTipo = data.tipo,
                                            intIdContel = Convert.ToInt32(data.id),
                                            dteInsert = DateTime.Now,
                                            dteUpdate = DateTime.Now,
                                            bitValidated = true,
                                            txtActivationCodeLPA = activationCode
                                        });
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        LogHelper.LogMessageOld(1, "ICCID" + data.iccid + " already exists in tblESimICCIDPool");
                                    }
                                }
                                else
                                {
                                    LogHelper.LogMessageOld(1, "Unable to get activation code for iccid:" + data.iccid);
                                }
                            }
                        }
                    }
                    else
                    {
                        LogHelper.LogMessageOld(0, "GetNewICCIDsForESIM null response");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(0, "GetNewICCIDsForESIM error:" + ex.ToString());
                return false;
            }
            return true;
        }

        public void UpdateSlado(SaldoResponse saldoResponse, string linha)
        {
            try
            {
                if (saldoResponse != null && saldoResponse.retorno)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var listContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == linha);
                        if (listContel != null)
                        {
                            var txtrestante_dados = saldoResponse != null && saldoResponse.data != null ? (saldoResponse.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                            listContel.txtrestante_dados = txtrestante_dados;
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public SaldoResponse GetSaldo(string linha)
        {
            var watch = Stopwatch.StartNew();
            SaldoResponse saldoResponse = null;
            try
            {
                string apiMethod = string.Format("linhas/saldo?linha=" + linha);

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    TimeSpan timeTaken = watch.Elapsed;
                    LogHelper.LogMessageOld(0, string.Format("GetSaldo response for line: {0}, Time taken: {1}::{2} ", linha, timeTaken.TotalSeconds, contentresponse));
                    watch.Stop();

                    saldoResponse = JsonConvert.DeserializeObject<SaldoResponse>(contentresponse);


                    if (saldoResponse != null && saldoResponse.retorno)
                        using (var ctx = new FoneClubeContext())
                        {
                            var listContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == linha);
                            if (listContel != null)
                            {
                                var txtrestante_dados = saldoResponse != null && saldoResponse.data != null ? (saldoResponse.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                                listContel.txtrestante_dados = txtrestante_dados;
                                ctx.SaveChanges();
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetSaldo error :" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return saldoResponse;
        }

        public bool SyncAllLinesFromContel()
        {
            LinhasPagingResponse linhasResponse = null;
            string apiMethod = string.Empty;
            List<LinhasDatum> lstLines = new List<LinhasDatum>();
            int start = 1;
            try
            {
                for (int icount = 0; icount < start; icount++)
                {
                    using (var client = new HttpClient())
                    {
                        apiMethod = string.Format("linhas/listar?page=" + (icount + 1));
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        linhasResponse = JsonConvert.DeserializeObject<LinhasPagingResponse>(contentresponse);
                        if (linhasResponse != null && linhasResponse.retorno && linhasResponse.data != null && linhasResponse.data.Count > 0)
                        {
                            if (linhasResponse.pagination != null)
                                start = linhasResponse.pagination.last_page;
                            lstLines.AddRange(linhasResponse.data);
                        }
                        else
                            break;
                    }
                }
                if (lstLines != null && lstLines.Count > 0)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var listData = lstLines.GroupBy(o => o.linha).Select(g => g.OrderByDescending(o => DateTime.ParseExact(o.data_ativacao, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)).First());

                        foreach (var line in listData)
                        {
                            var listContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == line.linha);
                            if (listContel != null)
                            {
                                listContel.txtlinha = line.linha;
                                listContel.txticcid = line.iccid;
                                listContel.txttitular = line.titular;
                                listContel.txttitular_apelido = line.titular_apelido;
                                listContel.txtdata_ex = Convert.ToString(line.data_ex);
                                listContel.txtdata_portout = Convert.ToString(line.data_portout);
                                listContel.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                listContel.txtemoji = Convert.ToString(line.emoji);
                                listContel.txtdata_ativacao = line.data_ativacao;
                                listContel.txtdata_inicio_plano = line.data_inicio_plano;
                                listContel.txtdata_fim_plano = line.data_fim_plano;
                                listContel.txtdata_renovacao = line.data_renovacao;
                                listContel.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                listContel.txtplano = line.plano;
                                listContel.txtdocumento_titular = line.documento_titular;
                                listContel.txtstatus = line.status;
                                listContel.txtrecorrencia_cartao = line.recorrencia_cartao;
                                listContel.txtportin = line.portin;
                                listContel.txtesim = line.esim;
                                listContel.txtbloqueada = line.bloqueada;
                                listContel.txtrecarga_automatica = line.recarga_automatica;
                                listContel.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                            }
                            else
                            {
                                ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                {
                                    txtlinha = line.linha,
                                    txticcid = line.iccid,
                                    txttitular = line.titular,
                                    txttitular_apelido = line.titular_apelido,
                                    txtdata_ex = Convert.ToString(line.data_ex),
                                    txtdata_portout = Convert.ToString(line.data_portout),
                                    txtnome_identificacao = Convert.ToString(line.nome_identificacao),
                                    txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga,
                                    txtemoji = Convert.ToString(line.emoji),
                                    txtdata_ativacao = line.data_ativacao,
                                    txtdata_inicio_plano = line.data_inicio_plano,
                                    txtdata_fim_plano = line.data_fim_plano,
                                    txtdata_renovacao = line.data_renovacao,
                                    txtplano = line.plano,
                                    txtdocumento_titular = line.documento_titular,
                                    txtstatus = line.status,
                                    txtrecorrencia_cartao = line.recorrencia_cartao,
                                    txtportin = line.portin,
                                    txtesim = line.esim,
                                    txtbloqueada = line.bloqueada,
                                    txtrecarga_automatica = line.recarga_automatica,
                                    dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano,
                                    bitRecAutoFC = true
                                });
                            }
                            ctx.SaveChanges();
                        }

                        var lstData = listData.ToList();

                        //ESIM

                        var esimList = lstLines.Where(x => x.esim == "SIM" && !string.IsNullOrEmpty(x.esim_pdf)).ToList();
                        if (esimList != null && esimList.Count > 0)
                        {
                            foreach (var esim in esimList)
                            {
                                var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == esim.linha && x.txtICCID == esim.iccid);
                                string iccid = esim.iccid, activationCode = string.Empty;
                                var img = DownloadActivationFileContel(esim.esim_pdf, esim.linha, ref iccid, ref activationCode);

                                if (tblEsim != null)
                                {
                                    tblEsim.txtActivationCode = activationCode;
                                    tblEsim.txtActivationDate = esim.data_inicio_plano;
                                    tblEsim.txtActivationImage = img;
                                    tblEsim.txtActivationPdfUrl = esim.esim_pdf;
                                    tblEsim.txtICCID = iccid;
                                    tblEsim.txtLinha = esim.linha;
                                    tblEsim.txtPlano = esim.plano;
                                    tblEsim.dteInsert = DateTime.Now;
                                }
                                else
                                {
                                    ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                    {
                                        txtActivationCode = activationCode,
                                        txtActivationDate = esim.data_inicio_plano,
                                        txtActivationImage = img,
                                        txtActivationPdfUrl = esim.esim_pdf,
                                        txtICCID = iccid,
                                        txtLinha = esim.linha,
                                        txtPlano = esim.plano,
                                        dteInsert = DateTime.Now
                                    });
                                }
                                ctx.SaveChanges();
                            }
                        }

                        //var notExistsInNew = ctx.tblContelLinhasList.ToList().Where(s => !lstData.Any(r => s.txtlinha == r.linha)).ToList();
                        //if (notExistsInNew != null && notExistsInNew.Count() > 0)
                        //{
                        //    foreach (var notex in notExistsInNew)
                        //    {
                        //        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Not exist in new sync: " + notex.txtlinha });
                        //        ctx.SaveChanges();
                        //        var lineExistsIndivid = GetContelLinesByPhoneLite(notex.txtlinha);
                        //        if (lineExistsIndivid == null)
                        //        {
                        //            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Not exist in GetContelLinesByPhoneLite: " + notex.txtlinha });
                        //            ctx.SaveChanges();
                        //            var deleteExs = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == notex.txtlinha);
                        //            ctx.tblContelLinhasList.Remove(deleteExs);
                        //            ctx.SaveChanges();
                        //        }
                        //        else
                        //        {
                        //            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Exist in GetContelLinesByPhoneLite hencing updating: " + notex.txtlinha });

                        //            var updateList = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == notex.txtlinha);
                        //            updateList.txticcid = lineExistsIndivid.iccid;
                        //            updateList.txttitular = lineExistsIndivid.titular;
                        //            updateList.txtdata_ativacao = lineExistsIndivid.data_ativacao;
                        //            updateList.txtdata_fim_plano = lineExistsIndivid.data_fim_plano;
                        //            updateList.txtdata_renovacao = lineExistsIndivid.data_renovacao;
                        //            updateList.txtplano = lineExistsIndivid.plano;
                        //            updateList.txtdocumento_titular = lineExistsIndivid.documento_titular;
                        //            updateList.txtstatus = lineExistsIndivid.status;
                        //            updateList.txtrecorrencia_cartao = lineExistsIndivid.recorrencia_cartao;
                        //            updateList.txtportin = lineExistsIndivid.portin;
                        //            updateList.txtesim = lineExistsIndivid.esim;
                        //            updateList.txtbloqueada = lineExistsIndivid.bloqueada;
                        //            updateList.txtrecarga_automatica = lineExistsIndivid.recarga_automatica;
                        //            updateList.dteAutoTopup = lineExistsIndivid.recarga_automatica == "ATIVA" ? lineExistsIndivid.data_renovacao : lineExistsIndivid.data_fim_plano;
                        //            ctx.SaveChanges();

                        //            var saldo = GetSaldo(notex.txtlinha);
                        //            UpdateSlado(saldo, notex.txtlinha);
                        //        }
                        //    }

                        //}

                        SyncWebFcLines();
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan error" + ex.ToString() });
                    ctx.SaveChanges();
                }

                return false;
            }
            return true;
        }

        public bool SyncAllLinesFromContel1()
        {
            try
            {
                LinhasResponse linhasResponse = null;
                string apiMethod = string.Format("linhas/listar");

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    linhasResponse = JsonConvert.DeserializeObject<LinhasResponse>(contentresponse);

                    if (linhasResponse.retorno)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var listData = linhasResponse.data.GroupBy(o => o.linha).Select(g => g.OrderByDescending(o => DateTime.ParseExact(o.data_ativacao, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)).First());

                            foreach (var line in listData)
                            {
                                var listContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == line.linha);
                                if (listContel != null)
                                {
                                    listContel.txtlinha = line.linha;
                                    listContel.txticcid = line.iccid;
                                    listContel.txttitular = line.titular;
                                    listContel.txttitular_apelido = line.titular_apelido;
                                    listContel.txtdata_ex = Convert.ToString(line.data_ex);
                                    listContel.txtdata_portout = Convert.ToString(line.data_portout);
                                    listContel.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                    listContel.txtemoji = Convert.ToString(line.emoji);
                                    listContel.txtdata_ativacao = line.data_ativacao;
                                    listContel.txtdata_inicio_plano = line.data_inicio_plano;
                                    listContel.txtdata_fim_plano = line.data_fim_plano;
                                    listContel.txtdata_renovacao = line.data_renovacao;
                                    listContel.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                    listContel.txtplano = line.plano;
                                    listContel.txtdocumento_titular = line.documento_titular;
                                    listContel.txtstatus = line.status;
                                    listContel.txtrecorrencia_cartao = line.recorrencia_cartao;
                                    listContel.txtportin = line.portin;
                                    listContel.txtesim = line.esim;
                                    listContel.txtbloqueada = line.bloqueada;
                                    listContel.txtrecarga_automatica = line.recarga_automatica;
                                    listContel.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                                }
                                else
                                {
                                    ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                    {
                                        txtlinha = line.linha,
                                        txticcid = line.iccid,
                                        txttitular = line.titular,
                                        txttitular_apelido = line.titular_apelido,
                                        txtdata_ex = Convert.ToString(line.data_ex),
                                        txtdata_portout = Convert.ToString(line.data_portout),
                                        txtnome_identificacao = Convert.ToString(line.nome_identificacao),
                                        txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga,
                                        txtemoji = Convert.ToString(line.emoji),
                                        txtdata_ativacao = line.data_ativacao,
                                        txtdata_inicio_plano = line.data_inicio_plano,
                                        txtdata_fim_plano = line.data_fim_plano,
                                        txtdata_renovacao = line.data_renovacao,
                                        txtplano = line.plano,
                                        txtdocumento_titular = line.documento_titular,
                                        txtstatus = line.status,
                                        txtrecorrencia_cartao = line.recorrencia_cartao,
                                        txtportin = line.portin,
                                        txtesim = line.esim,
                                        txtbloqueada = line.bloqueada,
                                        txtrecarga_automatica = line.recarga_automatica,
                                        dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano,
                                        bitRecAutoFC = true
                                    });
                                }
                                ctx.SaveChanges();
                            }

                            //var lstData = linhasResponse.data.ToList();
                            //var notExistsInNew = ctx.tblContelLinhasList.ToList().Where(s => !lstData.Any(r => s.txtlinha == r.linha)).ToList();
                            //if (notExistsInNew != null && notExistsInNew.Count() > 0)
                            //{
                            //    foreach (var notex in notExistsInNew)
                            //    {
                            //        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Not exist in new sync: " + notex.txtlinha });
                            //        ctx.SaveChanges();
                            //        var lineExistsIndivid = GetContelLinesByPhoneLite(notex.txtlinha);
                            //        if (lineExistsIndivid == null)
                            //        {
                            //            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Not exist in GetContelLinesByPhoneLite: " + notex.txtlinha });
                            //            ctx.SaveChanges();
                            //            var deleteExs = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == notex.txtlinha);
                            //            ctx.tblContelLinhasList.Remove(deleteExs);
                            //            ctx.SaveChanges();
                            //        }
                            //        else
                            //        {
                            //            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContel: Exist in GetContelLinesByPhoneLite hencing updating: " + notex.txtlinha });

                            //            var updateList = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == notex.txtlinha);
                            //            updateList.txticcid = lineExistsIndivid.iccid;
                            //            updateList.txttitular = lineExistsIndivid.titular;
                            //            updateList.txtdata_ativacao = lineExistsIndivid.data_ativacao;
                            //            updateList.txtdata_fim_plano = lineExistsIndivid.data_fim_plano;
                            //            updateList.txtdata_renovacao = lineExistsIndivid.data_renovacao;
                            //            updateList.txtplano = lineExistsIndivid.plano;
                            //            updateList.txtdocumento_titular = lineExistsIndivid.documento_titular;
                            //            updateList.txtstatus = lineExistsIndivid.status;
                            //            updateList.txtrecorrencia_cartao = lineExistsIndivid.recorrencia_cartao;
                            //            updateList.txtportin = lineExistsIndivid.portin;
                            //            updateList.txtesim = lineExistsIndivid.esim;
                            //            updateList.txtbloqueada = lineExistsIndivid.bloqueada;
                            //            updateList.txtrecarga_automatica = lineExistsIndivid.recarga_automatica;
                            //            updateList.dteAutoTopup = lineExistsIndivid.recarga_automatica == "ATIVA" ? lineExistsIndivid.data_renovacao : lineExistsIndivid.data_fim_plano;
                            //            ctx.SaveChanges();

                            //            var saldo = GetSaldo(notex.txtlinha);
                            //            UpdateSlado(saldo, notex.txtlinha);
                            //        }
                            //    }

                            //}

                            SyncWebFcLines();
                        }
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return false;
        }

        public bool SyncAllLinesFromContelWithSaldo()
        {
            try
            {
                //SyncAllLinesFromContel();
                using (var ctx = new FoneClubeContext())
                {
                    var cList = ctx.tblContelLinhasList.Where(s => !ctx.tblInternationalUserPurchases.Any(r => s.txtlinha == r.txtPhone));

                    foreach (var line in cList.ToList())
                    {
                        tblContelLinhasList cLine = (tblContelLinhasList)line;
                        var saldo = GetSaldo(line.txtlinha);
                        var txtrestante_dados = saldo != null && saldo.data != null ? (saldo.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                        cLine.txtrestante_dados = txtrestante_dados;
                        ctx.SaveChanges();
                    }

                    GetSaldoLessThanGBs();
                }

                return true;

            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContelWithSaldo error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return false;
        }

        public bool SyncSaldoIntl()
        {
            int intLimit = 40;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    LogHelper.LogMessageOld(0, "SyncSaldoIntl start");
                    var cList = ctx.tblContelLinhasList.Where(s => ctx.tblInternationalUserPurchases.Any(r => s.txtlinha == r.txtPhone && s.txtplano == "4 GB"));

                    if (cList.Count() > 0)
                    {
                        int intLoop = 1;
                        foreach (var line in cList.ToList())
                        {
                            if (intLoop % intLimit == 0)
                            {
                                LogHelper.LogMessageOld(0, "Waiting for 2sec post 30 records");
                                System.Threading.Thread.Sleep(1000);
                            }

                            tblContelLinhasList cLine = (tblContelLinhasList)line;
                            var saldo = GetSaldo(line.txtlinha);
                            var txtrestante_dados = saldo != null && saldo.data != null ? (saldo.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                            cLine.txtrestante_dados = txtrestante_dados;
                            ctx.SaveChanges();

                            intLoop++;
                        }

                    }
                    LogHelper.LogMessageOld(0, "SyncSaldoIntl end");
                }

                return true;

            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContelWithSaldo error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return false;
        }

        public bool SyncAllLinesFromContelWithSaldoPerson(int personId)
        {
            try
            {
                //SyncAllLinesFromContel();

                using (var ctx = new FoneClubeContext())
                {
                    var phoneLines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == personId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value && x.bitAtivo.HasValue && x.bitAtivo.Value && x.intIdOperator == 4).AsEnumerable();

                    var cList = ctx.tblContelLinhasList.Where(x => phoneLines.Any(y => string.Concat(y.intDDD, y.intPhone) == x.txtlinha));

                    foreach (var line in cList)
                    {
                        var saldo = GetSaldo(line.txtlinha);
                        var txtrestante_dados = saldo != null && saldo.data != null ? (saldo.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                        line.txtrestante_dados = txtrestante_dados;
                    }
                    ctx.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncSaldo error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return false;
        }

        public bool SyncAllLinesFromContelManual()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var allPhones = ctx.tblPersonsPhones.Where(x => x.intIdOperator == 4).Select(x => string.Concat(x.intDDD, x.intPhone)).ToList();
                    foreach (var phone in allPhones)
                    {
                        var phoneDetail = GetContelLinesByPhoneManual(phone);
                        if (phoneDetail != null && phoneDetail.detalhes != null)
                        {
                            ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                            {
                                txtlinha = phoneDetail.detalhes.linha,
                                txticcid = phoneDetail.detalhes.iccid,
                                txttitular = phoneDetail.detalhes.titular,
                                txttitular_apelido = phoneDetail.detalhes.titular_apelido,
                                txtdata_ex = Convert.ToString(phoneDetail.detalhes.data_ex),
                                txtdata_portout = Convert.ToString(phoneDetail.detalhes.data_portout),
                                txtnome_identificacao = Convert.ToString(phoneDetail.detalhes.nome_identificacao),
                                txtdata_cancelamento_linha = phoneDetail.detalhes.data_perda_numero_falta_recarga,
                                txtemoji = Convert.ToString(phoneDetail.detalhes.emoji),
                                txtdata_ativacao = phoneDetail.detalhes.data_ativacao,
                                txtdata_inicio_plano = phoneDetail.detalhes.data_inicio_plano,
                                txtdata_fim_plano = phoneDetail.detalhes.data_fim_plano,
                                txtdata_renovacao = phoneDetail.detalhes.data_renovacao,
                                txtplano = phoneDetail.detalhes.plano,
                                txtdocumento_titular = phoneDetail.detalhes.documento_titular,
                                txtstatus = phoneDetail.detalhes.status,
                                txtrecorrencia_cartao = phoneDetail.detalhes.recorrencia_cartao,
                                txtportin = phoneDetail.detalhes.portin,
                                txtesim = phoneDetail.detalhes.esim,
                                txtbloqueada = phoneDetail.detalhes.bloqueada,
                                txtrecarga_automatica = phoneDetail.detalhes.recarga_automatica,
                                dteAutoTopup = phoneDetail.detalhes.recarga_automatica == "ATIVA" ? phoneDetail.detalhes.data_renovacao : phoneDetail.detalhes.data_fim_plano,
                                bitRecAutoFC = true
                            });
                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan error" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return false;
        }

        public bool SyncAllLinesFromContelManualNew()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var allPhones = ctx.tblContelLinhasList.Where(x => x.txtstatus == "CANCELADO").Select(x => x.txtlinha).ToList();
                    foreach (var phone in allPhones)
                    {
                        var phoneDetail = GetContelLinesByPhoneManual(phone);
                        if (phoneDetail != null && phoneDetail.detalhes != null && !string.IsNullOrEmpty(phoneDetail.detalhes.linha))
                        {
                            var updStat = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == phoneDetail.detalhes.linha);
                            if (updStat != null)
                            {
                                var line = phoneDetail.detalhes;
                                updStat.txtlinha = line.linha;
                                updStat.txticcid = line.iccid;
                                updStat.txttitular = line.titular;
                                updStat.txttitular_apelido = line.titular_apelido;
                                updStat.txtdata_ex = Convert.ToString(line.data_ex);
                                updStat.txtdata_portout = Convert.ToString(line.data_portout);
                                updStat.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                updStat.txtemoji = Convert.ToString(line.emoji);
                                updStat.txtdata_ativacao = line.data_ativacao;
                                updStat.txtdata_inicio_plano = line.data_inicio_plano;
                                updStat.txtdata_fim_plano = line.data_fim_plano;
                                updStat.txtdata_renovacao = line.data_renovacao;
                                updStat.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                updStat.txtplano = line.plano;
                                updStat.txtdocumento_titular = line.documento_titular;
                                updStat.txtstatus = line.status;
                                updStat.txtrecorrencia_cartao = line.recorrencia_cartao;
                                updStat.txtportin = line.portin;
                                updStat.txtesim = line.esim;
                                updStat.txtbloqueada = line.bloqueada;
                                updStat.txtrecarga_automatica = line.recarga_automatica;
                                updStat.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                            }
                            else
                            {
                                ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                {
                                    txtlinha = phoneDetail.detalhes.linha,
                                    txticcid = phoneDetail.detalhes.iccid,
                                    txttitular = phoneDetail.detalhes.titular,
                                    txttitular_apelido = phoneDetail.detalhes.titular_apelido,
                                    txtdata_ex = Convert.ToString(phoneDetail.detalhes.data_ex),
                                    txtdata_portout = Convert.ToString(phoneDetail.detalhes.data_portout),
                                    txtnome_identificacao = Convert.ToString(phoneDetail.detalhes.nome_identificacao),
                                    txtdata_cancelamento_linha = phoneDetail.detalhes.data_perda_numero_falta_recarga,
                                    txtemoji = Convert.ToString(phoneDetail.detalhes.emoji),
                                    txtdata_ativacao = phoneDetail.detalhes.data_ativacao,
                                    txtdata_inicio_plano = phoneDetail.detalhes.data_inicio_plano,
                                    txtdata_fim_plano = phoneDetail.detalhes.data_fim_plano,
                                    txtdata_renovacao = phoneDetail.detalhes.data_renovacao,
                                    txtplano = phoneDetail.detalhes.plano,
                                    txtdocumento_titular = phoneDetail.detalhes.documento_titular,
                                    txtstatus = phoneDetail.detalhes.status,
                                    txtrecorrencia_cartao = phoneDetail.detalhes.recorrencia_cartao,
                                    txtportin = phoneDetail.detalhes.portin,
                                    txtesim = phoneDetail.detalhes.esim,
                                    txtbloqueada = phoneDetail.detalhes.bloqueada,
                                    txtrecarga_automatica = phoneDetail.detalhes.recarga_automatica,
                                    dteAutoTopup = phoneDetail.detalhes.recarga_automatica == "ATIVA" ? phoneDetail.detalhes.data_renovacao : phoneDetail.detalhes.data_fim_plano,
                                    bitRecAutoFC = true
                                });
                            }
                            ctx.SaveChanges();
                        }
                    }

                    var allPhones1 = ctx.GetAllContelLinesInWebFCButNotInContel().ToList();
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetAllContelLinesInWebFCButNotInContel count:" + allPhones1.Count });
                    ctx.SaveChanges();
                    foreach (var phone in allPhones1)
                    {
                        var phoneDetail = GetContelLinesByPhoneManual(phone.linha);
                        if (phoneDetail != null && phoneDetail.detalhes != null && !string.IsNullOrEmpty(phoneDetail.detalhes.linha))
                        {
                            var updStat = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == phoneDetail.detalhes.linha);
                            if (updStat != null)
                            {
                                var line = phoneDetail.detalhes;
                                updStat.txtlinha = line.linha;
                                updStat.txticcid = line.iccid;
                                updStat.txttitular = line.titular;
                                updStat.txttitular_apelido = line.titular_apelido;
                                updStat.txtdata_ex = Convert.ToString(line.data_ex);
                                updStat.txtdata_portout = Convert.ToString(line.data_portout);
                                updStat.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                updStat.txtemoji = Convert.ToString(line.emoji);
                                updStat.txtdata_ativacao = line.data_ativacao;
                                updStat.txtdata_inicio_plano = line.data_inicio_plano;
                                updStat.txtdata_fim_plano = line.data_fim_plano;
                                updStat.txtdata_renovacao = line.data_renovacao;
                                updStat.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                updStat.txtplano = line.plano;
                                updStat.txtdocumento_titular = line.documento_titular;
                                updStat.txtstatus = line.status;
                                updStat.txtrecorrencia_cartao = line.recorrencia_cartao;
                                updStat.txtportin = line.portin;
                                updStat.txtesim = line.esim;
                                updStat.txtbloqueada = line.bloqueada;
                                updStat.txtrecarga_automatica = line.recarga_automatica;
                                updStat.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                            }
                            else
                            {
                                ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                {
                                    txtlinha = phoneDetail.detalhes.linha,
                                    txticcid = phoneDetail.detalhes.iccid,
                                    txttitular = phoneDetail.detalhes.titular,
                                    txttitular_apelido = phoneDetail.detalhes.titular_apelido,
                                    txtdata_ex = Convert.ToString(phoneDetail.detalhes.data_ex),
                                    txtdata_portout = Convert.ToString(phoneDetail.detalhes.data_portout),
                                    txtnome_identificacao = Convert.ToString(phoneDetail.detalhes.nome_identificacao),
                                    txtdata_cancelamento_linha = phoneDetail.detalhes.data_perda_numero_falta_recarga,
                                    txtemoji = Convert.ToString(phoneDetail.detalhes.emoji),
                                    txtdata_ativacao = phoneDetail.detalhes.data_ativacao,
                                    txtdata_inicio_plano = phoneDetail.detalhes.data_inicio_plano,
                                    txtdata_fim_plano = phoneDetail.detalhes.data_fim_plano,
                                    txtdata_renovacao = phoneDetail.detalhes.data_renovacao,
                                    txtplano = phoneDetail.detalhes.plano,
                                    txtdocumento_titular = phoneDetail.detalhes.documento_titular,
                                    txtstatus = phoneDetail.detalhes.status,
                                    txtrecorrencia_cartao = phoneDetail.detalhes.recorrencia_cartao,
                                    txtportin = phoneDetail.detalhes.portin,
                                    txtesim = phoneDetail.detalhes.esim,
                                    txtbloqueada = phoneDetail.detalhes.bloqueada,
                                    txtrecarga_automatica = phoneDetail.detalhes.recarga_automatica,
                                    dteAutoTopup = phoneDetail.detalhes.recarga_automatica == "ATIVA" ? phoneDetail.detalhes.data_renovacao : phoneDetail.detalhes.data_fim_plano,
                                    bitRecAutoFC = true
                                });
                                ctx.SaveChanges();
                            }
                            ctx.SaveChanges();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncAllLinesFromContelManualNew error" + ex.ToString() });
                    ctx.SaveChanges();
                }
                return false;
            }
        }

        public string AddContelLineManual(string phone)
        {
            string dataAdded = "";
            string apiMethod = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    using (var client = new HttpClient())
                    {
                        apiMethod = string.Format("linhas/detalhes?numero=" + phone);

                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        var res = JsonConvert.DeserializeObject<DetalhesRes>(contentresponse);

                        if (res != null && res.detalhes != null)
                        {
                            var line = res.detalhes;
                            var listContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == line.linha);
                            if (listContel != null)
                            {
                                listContel.txtlinha = line.linha;
                                listContel.txticcid = line.iccid;
                                listContel.txttitular = line.titular;
                                listContel.txttitular_apelido = line.titular_apelido;
                                listContel.txtdata_ex = Convert.ToString(line.data_ex);
                                listContel.txtdata_portout = Convert.ToString(line.data_portout);
                                listContel.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                listContel.txtemoji = Convert.ToString(line.emoji);
                                listContel.txtdata_ativacao = line.data_ativacao;
                                listContel.txtdata_inicio_plano = line.data_inicio_plano;
                                listContel.txtdata_fim_plano = line.data_fim_plano;
                                listContel.txtdata_renovacao = line.data_renovacao;
                                listContel.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                listContel.txtplano = line.plano;
                                listContel.txtdocumento_titular = line.documento_titular;
                                listContel.txtstatus = line.status;
                                listContel.txtrecorrencia_cartao = line.recorrencia_cartao;
                                listContel.txtportin = line.portin;
                                listContel.txtesim = line.esim;
                                listContel.txtbloqueada = line.bloqueada;
                                listContel.txtrecarga_automatica = line.recarga_automatica;
                                listContel.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                            }
                            else
                            {
                                ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                {
                                    txtlinha = line.linha,
                                    txticcid = line.iccid,
                                    txttitular = line.titular,
                                    txttitular_apelido = line.titular_apelido,
                                    txtdata_ex = Convert.ToString(line.data_ex),
                                    txtdata_portout = Convert.ToString(line.data_portout),
                                    txtnome_identificacao = Convert.ToString(line.nome_identificacao),
                                    txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga,
                                    txtemoji = Convert.ToString(line.emoji),
                                    txtdata_ativacao = line.data_ativacao,
                                    txtdata_inicio_plano = line.data_inicio_plano,
                                    txtdata_fim_plano = line.data_fim_plano,
                                    txtdata_renovacao = line.data_renovacao,
                                    txtplano = line.plano,
                                    txtdocumento_titular = line.documento_titular,
                                    txtstatus = line.status,
                                    txtrecorrencia_cartao = line.recorrencia_cartao,
                                    txtportin = line.portin,
                                    txtesim = line.esim,
                                    txtbloqueada = line.bloqueada,
                                    txtrecarga_automatica = line.recarga_automatica,
                                    dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano,
                                    bitRecAutoFC = true
                                });
                            }
                            ctx.SaveChanges();

                            try
                            {
                                var listContel1 = ctx.tblContelLinhasList.AsNoTracking().FirstOrDefault(x => x.txtlinha == line.linha);
                                var saldo = GetSaldo(line.linha);
                                var txtrestante_dados = saldo != null && saldo.data != null ? (saldo.data.restante_dados / 1024).ToString("0.00") + " GB" : "";
                                listContel1.txtrestante_dados = txtrestante_dados;
                                dataAdded = txtrestante_dados;
                                ctx.SaveChanges();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return dataAdded;
            }
            return dataAdded;
        }

        public ContelPhoneData GetContelLinesByPhoneLite(string phone)
        {
            ContelPhoneData linhasResponse = null;
            string apiMethod = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    using (var client = new HttpClient())
                    {
                        apiMethod = string.Format("linhas/detalhes?numero=" + phone);

                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        var res = JsonConvert.DeserializeObject<DetalhesRes>(contentresponse);

                        if (res != null && res.retorno && res.detalhes != null)
                        {
                            linhasResponse = new ContelPhoneData();

                            linhasResponse.bloqueada = res.detalhes.bloqueada;
                            linhasResponse.data_ativacao = res.detalhes.data_ativacao;
                            linhasResponse.data_cancelamento_linha = res.detalhes.data_cancelamento_linha;
                            linhasResponse.data_fim_plano = res.detalhes.data_fim_plano;
                            linhasResponse.data_renovacao = res.detalhes.data_renovacao;
                            linhasResponse.linha = res.detalhes.linha;
                            linhasResponse.plano = res.detalhes.plano;
                            linhasResponse.esim = res.detalhes.esim;
                            linhasResponse.portin = res.detalhes.portin;
                            linhasResponse.recarga_automatica = res.detalhes.recarga_automatica;
                            linhasResponse.recarga_automatica_plano = res.detalhes.recarga_automatica_plano;
                            linhasResponse.status = res.detalhes.status;
                            linhasResponse.recorrencia_cartao = res.detalhes.recorrencia_cartao;
                            linhasResponse.iccid = res.detalhes.iccid;
                            linhasResponse.data_inicio_plano = res.detalhes.data_inicio_plano;
                            linhasResponse.titular = res.detalhes.titular;
                            linhasResponse.documento_titular = res.detalhes.documento_titular;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return linhasResponse;
        }

        public ContelPhoneData GetContelLinesByPhone(string phone)
        {
            ContelPhoneData linhasResponse = null;
            string apiMethod = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    using (var client = new HttpClient())
                    {
                        apiMethod = string.Format("linhas/detalhes?numero=" + phone);

                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        var res = JsonConvert.DeserializeObject<DetalhesRes>(contentresponse);
                        if (res != null && res.detalhes != null)
                        {
                            linhasResponse = new ContelPhoneData();

                            linhasResponse.bloqueada = res.detalhes.bloqueada;
                            linhasResponse.data_ativacao = res.detalhes.data_ativacao;
                            linhasResponse.data_cancelamento_linha = res.detalhes.data_cancelamento_linha;
                            linhasResponse.data_fim_plano = res.detalhes.data_fim_plano;
                            linhasResponse.data_renovacao = res.detalhes.data_renovacao;
                            linhasResponse.linha = res.detalhes.linha;
                            linhasResponse.plano = res.detalhes.plano;
                            linhasResponse.esim = res.detalhes.esim;
                            linhasResponse.portin = res.detalhes.portin;
                            linhasResponse.recarga_automatica = res.detalhes.recarga_automatica;
                            linhasResponse.recarga_automatica_plano = res.detalhes.recarga_automatica_plano;
                            linhasResponse.status = res.detalhes.status;
                            linhasResponse.recorrencia_cartao = res.detalhes.recorrencia_cartao;
                            linhasResponse.iccid = res.detalhes.iccid;
                            linhasResponse.titular = res.detalhes.titular;
                            linhasResponse.documento_titular = res.detalhes.documento_titular;
                        }
                    }

                    var saldo = GetSaldo(phone);
                    if (saldo != null && saldo.data != null)
                    {
                        linhasResponse.restante_dados = saldo.data.restante_dados;
                        linhasResponse.restante_minutos = saldo.data.restante_minutos;
                        linhasResponse.restante_sms = saldo.data.restante_sms;
                    }
                    linhasResponse.Recarregar = "Selecione Plano";

                    var topupHist = GetContelTopupHistory(phone);
                    if (topupHist != null && topupHist.retorno && topupHist.historico != null && topupHist.historico.Count > 0)
                    {
                        try
                        {
                            linhasResponse.Pago = topupHist.historico[0].valor_pago;
                            linhasResponse.PaidPlano = topupHist.historico[0].plano;

                            var topupDate = DateTime.ParseExact(topupHist.historico[0].data_recarga, "yyyy-MM-dd HH:mm:ss.fff",
                                                              System.Globalization.CultureInfo.InvariantCulture);
                            linhasResponse.LastRecharge = topupDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("pt-PT"));
                            linhasResponse.DaysSinceLastTopup = Convert.ToInt32((DateTime.Now - topupDate).TotalDays);
                        }
                        catch (Exception) { }
                    }

                    var person = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == phone
                    && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value
                    && x.bitAtivo.HasValue && x.bitAtivo.Value && x.intIdOperator == 4).FirstOrDefault();
                    if (person != null)
                    {
                        var chargingHst = ctx.tblChargingHistory.AsEnumerable().Where(x => x.bitActive == true && x.intIdCustomer == person.intIdPerson).Select(x => x.intIdTransaction);
                        var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();
                        var transaction = (from c in chargingHst
                                           join t in lastPayments on c equals t.intIdTransaction
                                           select t).OrderByDescending(x => x.dteDate_updated).FirstOrDefault();
                        if (transaction != null)
                        {
                            linhasResponse.LastPaidDate = Convert.ToDateTime(transaction.dteDate_updated).ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("pt-PT"));
                            linhasResponse.LastPaidAmount = "R$" + (transaction.intPaid_amount.HasValue ? transaction.intPaid_amount.Value / 100 : 0);
                            linhasResponse.LastPaidDays = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(transaction.dteDate_updated)).TotalDays);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return linhasResponse;
        }

        public DetalhesRes GetContelLinesByPhoneManual(string phone, string environment = "PROD")
        {
            string apiMethod = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    using (var client = new HttpClient())
                    {
                        apiMethod = string.Format("linhas/detalhes?numero=" + phone);

                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetContelLinesByPhoneManual Response:  " + contentresponse });
                        ctx.SaveChanges();

                        return JsonConvert.DeserializeObject<DetalhesRes>(contentresponse);
                    }
                }
            }
            catch (Exception ex)
            {
                return new DetalhesRes()
                {
                    retorno = false,
                    mensagem = ex.ToString()
                };
            }
        }

        public bool UpdateContelBlockUnBlockLine(string phone)
        {
            try
            {
                var line = GetContelLinesByPhoneManual(phone);
                if (line != null)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var ctContel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == phone);
                        if (ctContel != null)
                        {
                            ctContel.txtbloqueada = line.detalhes.bloqueada;
                        }
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }
        public PlanosList GetContelPlans()
        {
            PlanosList planosList = null;
            try
            {
                using (var client = new HttpClient())
                {
                    planosList = new PlanosList();
                    string apiMethod = string.Format("planos");

                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    planosList = JsonConvert.DeserializeObject<PlanosList>(contentresponse);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return planosList;
        }

        public CityResponse GetCityByState(string state)
        {
            CityResponse cityResponse = null;
            try
            {
                using (var client = new HttpClient())
                {
                    string apiMethod = string.Format("cidades/{0}", state);

                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    cityResponse = JsonConvert.DeserializeObject<CityResponse>(contentresponse);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return cityResponse;
        }

        public TopUpHistoryResponse GetContelTopupHistory(string line, string environment = "PROD")
        {
            TopUpHistoryResponse topUpHistoryResponse = null;
            try
            {
                using (var client = new HttpClient())
                {
                    topUpHistoryResponse = new TopUpHistoryResponse();
                    string apiMethod = string.Format("linhas/historico?numero=" + line);

                    // Setting Authorization.  
                    if (environment == "PROD")
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    else
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    JsonSerializerSettings settings = new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    };

                    topUpHistoryResponse = JsonConvert.DeserializeObject<TopUpHistoryResponse>(contentresponse, settings);

                    LogHelper.LogMessageOld(1, string.Format("GetContelTopupHistory response for :{0}, {1}", line, contentresponse));
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, string.Format("GetContelTopupHistory error for :{0}, {1}", line, ex.ToString()));
                return null;
            }
            return topUpHistoryResponse;
        }

        public bool LogContelSaldoStartOfDay()
        {
            using (var ctx = new FoneClubeContext())
            {
                var response = GetRemainingSaldoForCompany();
                if (response != null && response.saldo != null && !string.IsNullOrEmpty(response.saldo))
                {
                    var config = ctx.tblConfigSettings.FirstOrDefault(x => x.txtConfigName == "ContelSaldoToday");
                    config.txtConfigValue = response.saldo;
                    ctx.SaveChanges();
                }
            }
            return true;
        }

        public RemainingSaldo GetRemainingSaldoForCompany()
        {
            RemainingSaldo remainingSaldo = null;
            try
            {
                using (var client = new HttpClient())
                {
                    remainingSaldo = new RemainingSaldo();
                    string apiMethod = string.Format("saldo/saldo");

                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();

                    // HTTP GET  
                    response = client.GetAsync(apiMethod).GetAwaiter().GetResult();

                    var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    JsonSerializerSettings settings = new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    };

                    remainingSaldo = JsonConvert.DeserializeObject<RemainingSaldo>(contentresponse, settings);

                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetRemainingSaldoForCompany response: " + contentresponse });
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return remainingSaldo;
        }

        public bool AutoTopupContelLines()
        {
            var balance = GetRemainingSaldoForCompany();
            if (balance != null)
            {
                double doubleVal;
                Double.TryParse(balance.saldo, out doubleVal);

                if (doubleVal > 500)
                {

                }
                else
                {
                    WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                    whatsAppAccess.SendMessage(new WhatsAppMessage()
                    {
                        Message = string.Format("Você tem apenas R$ {0} para recarga automática em sua conta", balance.saldo),
                        ClientIds = AdminMsgsTo
                    });
                }
            }
            return true;
        }

        public TopUpPlanResponse TopupPlan(TopUpPlanRequest request, string environment = "PROD")
        {
            TopUpPlanResponse topUpPlanResponse = null;
            try
            {
                if (request != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("recarga/solicitar");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response1 = new HttpResponseMessage();

                        client.Timeout = TimeSpan.FromMinutes(20);

                        // HTTP GET  
                        response1 = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "TopupPlan response: " + contentresponse });
                            ctx.SaveChanges();
                        }

                        topUpPlanResponse = JsonConvert.DeserializeObject<TopUpPlanResponse>(contentresponse);

                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                topUpPlanResponse = new TopUpPlanResponse() { retorno = false, mensagem = "Timeout error" };
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "TopupPlan: " + ex.ToString() });
                    ctx.SaveChanges();
                }
                topUpPlanResponse = new TopUpPlanResponse() { retorno = false, mensagem = ex.ToString() };
            }
            return topUpPlanResponse;
        }

        public TopUpUIPlanResponse AddTopupPlan(TopUpUIPlanRequest request)
        {
            tblWebfcTopupAtContel tblWebfcTopupAtContel = null;
            TopUpPlanResponse response = null;
            TopUpPlanRequest request1 = new TopUpPlanRequest();
            TopUpUIPlanResponse topUpUIPlanResponse = null;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "AddTopupPlan started" });
                    ctx.SaveChanges();

                    if (request != null)
                    {
                        var line = request.numeros[0].numero;
                        var existingTopupIn30Days = ctx.tblWebfcTopupAtContel.Where(x => x.txtLinha == line).OrderByDescending(x => x.intId).FirstOrDefault();
                        var isExists = existingTopupIn30Days != null ? ((DateTime.Now - existingTopupIn30Days.dteDateAdded).TotalDays <= 30) ? true : false : false;
                        if (!isExists || request.extra)
                        {
                            ctx.tblWebfcTopupAtContel.Add(new tblWebfcTopupAtContel()
                            {
                                dteDateAdded = DateTime.Now.Date,
                                txtLinha = request.numeros[0].numero,
                                bitUpdateFailed = true,
                                bitRecarga_Manul_Extra = request.extra
                            });
                            ctx.SaveChanges();

                            request1.metodo_pagamento = request.metodo_pagamento;
                            request1.numeros = new List<Numero>();
                            request1.numeros.Add(new Numero() { id_plano = request.numeros[0].id_plano, numero = request.numeros[0].numero });

                            var saldoPre = GetSaldo(request1.numeros[0].numero);

                            var JsonStr = JsonConvert.SerializeObject(request1, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                            StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                            string apiMethod = string.Format("recarga/solicitar");

                            using (var client = new HttpClient())
                            {
                                // Setting Authorization.  
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                                // Setting Base address.  
                                client.BaseAddress = new Uri(BaseAddress);

                                client.DefaultRequestHeaders.Accept.Clear();

                                // Setting content type.  
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                // Initialization.  
                                HttpResponseMessage response1 = new HttpResponseMessage();

                                // HTTP GET  
                                response1 = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                                var contentresponse = response1.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "AddTopupPlan response: " + contentresponse });
                                ctx.SaveChanges();

                                response = JsonConvert.DeserializeObject<TopUpPlanResponse>(contentresponse);

                            }

                            TopUpUIPlanResponse topRes = new TopUpUIPlanResponse();

                            if (response != null)
                                topRes.bitTopupDone = response.retorno;

                            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

                            var preSync = ctx.tblContelLinhasList.Where(x => x.txtlinha == line).FirstOrDefault();

                            var saldoPost = SaveTopInfo(request, saldoPre, response);

                            tblWebfcTopupAtContel = ctx.tblWebfcTopupAtContel.AsNoTracking().Where(x => x.txtLinha == line).OrderByDescending(x => x.intId).FirstOrDefault();
                            if (tblWebfcTopupAtContel != null)
                            {
                                var linhas = ctx.tblContelLinhasList.Where(x => x.txtlinha == tblWebfcTopupAtContel.txtLinha).FirstOrDefault();
                                linhas.txtrestante_dados = tblWebfcTopupAtContel.txtSaldoPost;
                                ctx.SaveChanges();

                                SyncAllLinesFromContel();
                            }

                            var postSync = ctx.tblContelLinhasList.AsNoTracking().Where(x => x.txtlinha == line).FirstOrDefault();

                            try
                            {
                                //FIM
                                var fimIssue = (Convert.ToDateTime(postSync.txtdata_fim_plano) - Convert.ToDateTime(preSync.txtdata_fim_plano)).TotalDays >= 30;
                                topRes.PreFimPlano = preSync.txtdata_fim_plano;
                                topRes.PostFimPlano = postSync.txtdata_fim_plano;
                                topRes.DataFimPlano = fimIssue ? "OK" : "ERRO";

                                //SALDO
                                topRes.PreSaldo = string.Format("{0} GB, {1} minutos, {2} SMS", (saldoPre.data.restante_dados / 1024).ToString("0.00"), saldoPre.data.restante_minutos, saldoPre.data.restante_sms);
                                topRes.PostSaldo = string.Format("{0} GB, {1} minutos, {2} SMS", (saldoPost.data.restante_dados / 1024).ToString("0.00"), saldoPost.data.restante_minutos, saldoPost.data.restante_sms);
                                topRes.SaldoGBAdded = ((saldoPost.data.restante_dados - saldoPre.data.restante_dados) / 1024).ToString("0.00") + " GB";
                                topRes.StatusGB = saldoPost.data.restante_dados - saldoPre.data.restante_dados == 0 ? "PENDENCIAS" : "OK";


                                topRes.PortIn = postSync.txtportin;
                                if (postSync.txtportin == "SIM")
                                {
                                    var bonus = (saldoPost.data.restante_dados - saldoPre.data.restante_dados - (request.planGB * 1024)) >= 5120;
                                    var port = postSync.txtportin == "SIM" && !fimIssue && bonus;
                                    topRes.StatusPortabilidade = port ? "OK" : "ERRO";
                                }
                                else
                                    topRes.StatusPortabilidade = "OK";

                                if (topRes.DataFimPlano != "ERRO" && topRes.StatusPortabilidade != "ERRO" && topRes.StatusGB != "PENDENCIAS")
                                    topRes.Status = 1;
                                else if (topRes.DataFimPlano == "ERRO")
                                    topRes.Status = 2;
                                else if (topRes.StatusPortabilidade == "ERRO")
                                    topRes.Status = 3;
                                else if (topRes.StatusGB == "PENDENCIAS")
                                    topRes.Status = 4;

                                topRes.bitWarning = false;

                                topRes.Linha = line;
                                topRes.LastTopup = tblWebfcTopupAtContel.dteDateAdded;
                            }
                            catch (Exception ex) { }

                            return topRes;
                        }
                        else
                        {
                            return new TopUpUIPlanResponse()
                            {
                                bitWarning = true,
                                LastTopup = existingTopupIn30Days.dteDateAdded
                            };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "AddTopupPlan: " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return topUpUIPlanResponse;
        }

        public SaldoResponse SaveTopInfo(TopUpUIPlanRequest request, SaldoResponse saldoPre, TopUpPlanResponse response)
        {
            SaldoResponse saldoPost = null;
            try
            {
                if (response != null && response.retorno)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var addedLine = request.numeros[0].numero;
                        var addedLinePlano = request.numeros[0].id_plano;
                        var lineInfo = ctx.tblContelLinhasList.Where(x => x.txtlinha == addedLine).FirstOrDefault();

                        var tblPlanPre = ctx.tblPersonsPhones.Where(x => (x.intDDD + "" + x.intPhone) == addedLine && x.intIdOperator == 4).FirstOrDefault();
                        var tblPlanPost = ctx.tblPlansOptions.Where(x => x.intOperatorPlanId == addedLinePlano).FirstOrDefault();

                        saldoPost = GetSaldo(addedLine);

                        bool isSaldoError = saldoPost == null ? true : false;

                        var currentdate = DateTime.Now.Date;
                        var updateWebFc = ctx.tblWebfcTopupAtContel.Where(x => x.txtLinha == addedLine && x.bitUpdateFailed.HasValue && x.bitUpdateFailed.Value && x.dteDateAdded == currentdate).FirstOrDefault();

                        if (updateWebFc != null)
                        {
                            updateWebFc.dteDateAdded = Convert.ToDateTime(response.recarga.data_cadastro);
                            updateWebFc.dteVigencia = DateTime.Now;
                            updateWebFc.txtLinha = addedLine;
                            updateWebFc.txtPortIn = lineInfo.txtportin;
                            updateWebFc.txtdata_renovacao = lineInfo.txtdata_renovacao;
                            updateWebFc.txtRecarga_automatica_plano = lineInfo.txtrecarga_automatica;

                            updateWebFc.txtSaldoPre = isSaldoError ? "" : (saldoPre.data.restante_dados / 1024).ToString("0.00") + " GB";
                            updateWebFc.txtSMSPre = isSaldoError ? "" : saldoPre.data.restante_sms.ToString();
                            updateWebFc.txtMinutesPre = isSaldoError ? "" : saldoPre.data.restante_minutos.ToString();

                            updateWebFc.txtSaldoPost = isSaldoError ? "" : (saldoPost.data.restante_dados / 1024).ToString("0.00") + " GB";
                            updateWebFc.txtMinutesPost = isSaldoError ? "" : saldoPost.data.restante_minutos.ToString();
                            updateWebFc.txtSMSPost = isSaldoError ? "" : saldoPost.data.restante_sms.ToString();

                            updateWebFc.txtTotalSaldo = isSaldoError ? "" : (saldoPost.data.restante_dados / 1024).ToString("0.00") + " GB";

                            updateWebFc.intIdPlanPre = tblPlanPre.intIdPlan.Value;
                            updateWebFc.intIdPlanPost = tblPlanPost.intIdPlan;

                            updateWebFc.bitUpdateFailed = isSaldoError ? true : Math.Abs(saldoPost.data.restante_dados) < 0.001 ? true : false;
                            updateWebFc.bitRecarga_Manul_Extra = request.extra;
                            updateWebFc.bitManual = true;

                        }
                        else
                        {
                            ctx.tblWebfcTopupAtContel.Add(new tblWebfcTopupAtContel()
                            {
                                dteDateAdded = Convert.ToDateTime(response.recarga.data_cadastro),
                                dteVigencia = DateTime.Now,
                                txtLinha = addedLine,
                                txtPortIn = lineInfo.txtportin,
                                txtdata_renovacao = lineInfo.txtdata_renovacao,
                                txtRecarga_automatica_plano = lineInfo.txtrecarga_automatica,

                                txtSaldoPre = isSaldoError ? "" : (saldoPre.data.restante_dados / 1024).ToString("0.00") + " GB",
                                txtSMSPre = isSaldoError ? "" : saldoPre.data.restante_sms.ToString(),
                                txtMinutesPre = isSaldoError ? "" : saldoPre.data.restante_minutos.ToString(),

                                txtSaldoPost = isSaldoError ? "" : (saldoPost.data.restante_dados / 1024).ToString("0.00") + " GB",
                                txtMinutesPost = isSaldoError ? "" : saldoPost.data.restante_minutos.ToString(),
                                txtSMSPost = isSaldoError ? "" : saldoPost.data.restante_sms.ToString(),

                                txtTotalSaldo = isSaldoError ? "" : (saldoPost.data.restante_dados / 1024).ToString("0.00") + " GB",

                                intIdPlanPre = tblPlanPre.intIdPlan.Value,
                                intIdPlanPost = tblPlanPost.intIdPlan,

                                bitUpdateFailed = isSaldoError ? true : Math.Abs(saldoPost.data.restante_dados) < 0.001 ? true : false,
                                bitRecarga_Manul_Extra = request.extra,
                                bitManual = true
                            });
                        }
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SaveTopInfo: " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return saldoPost;
        }

        public BlockLineResponse BlockLine(BlockLine line, string environment = "PROD")
        {
            BlockLineResponse blockLineResponse = null;
            try
            {
                if (line != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(line, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("atendimento/bloqueio");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        }
                        else
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);
                        }

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "BlockLine Response: " + contentresponse });
                            ctx.SaveChanges();
                        }
                        blockLineResponse = JsonConvert.DeserializeObject<BlockLineResponse>(contentresponse);

                    }
                }
            }
            catch (Exception)
            {
                blockLineResponse = null;
            }
            return blockLineResponse;
        }

        public ResetLineRes ResetLine(ResetLine line, string environment = "PROD")
        {
            ResetLineRes resetLineRes = null;
            try
            {
                if (line != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(line, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("atendimento/trocaDeChip");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        }
                        else
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);
                        }

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                        try
                        {
                            resetLineRes = JsonConvert.DeserializeObject<ResetLineRes>(contentresponse);
                        }
                        catch (Exception)
                        {
                            resetLineRes = new ResetLineRes();
                            resetLineRes.message = contentresponse.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "");
                        }
                        LogHelper.LogMessageOld(1, string.Format("ResetLine response for :{0}, {1}", line, contentresponse));

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, string.Format("ResetLine response error for :{0}, {1}", line, ex.ToString()));

                resetLineRes = null;
            }
            return resetLineRes;
        }

        public BlockLineResponse UnBlockLine(UnBlockLine line, string environment = "PROD")
        {
            BlockLineResponse blockLineResponse = null;
            try
            {
                if (line != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(line, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    string apiMethod = string.Format("atendimento/desbloqueio");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        if (environment == "PROD")
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        else
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevToken);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        blockLineResponse = JsonConvert.DeserializeObject<BlockLineResponse>(contentresponse);
                    }
                }
            }
            catch (Exception)
            {
                blockLineResponse = null;
            }
            return blockLineResponse;
        }

        public ApelidoResponse UpdateApelido(ApelidoRequest request)
        {
            ApelidoResponse response = null;
            if (request != null)
            {
                var JsonStr = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                string apiMethod = string.Format("linhas/atualizar");

                using (var client = new HttpClient())
                {
                    // Setting Authorization.  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                    // Setting Base address.  
                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Accept.Clear();

                    // Setting content type.  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Initialization.  
                    HttpResponseMessage res = new HttpResponseMessage();

                    // HTTP GET  
                    res = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                    var contentresponse = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    response = JsonConvert.DeserializeObject<ApelidoResponse>(contentresponse);

                }
            }
            return response;
        }
        public List<BlockLineResponseUI> BlockLineByCustomer(BlockRequest line)
        {
            List<BlockLineResponseUI> blockLines = new List<BlockLineResponseUI>();
            using (var ctx = new FoneClubeContext())
            {
                var lines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == line.PersonId && x.bitAtivo == true && x.bitPhoneClube == true && x.intIdOperator == 4).ToList();
                foreach (var li in lines)
                {
                    var request = new BlockLine()
                    {
                        numero = string.Concat(li.intDDD, li.intPhone),
                        motivo = "BLOQUEIO DE IMEI",
                        observacoes = ""
                    };
                    var block = BlockLine(request);

                    if (block != null && block.status)
                    {
                        ctx.tblContelBlockedStatus.Add(new tblContelBlockedStatus()
                        {
                            intIdPerson = line.PersonId,
                            dteBlocked = DateTime.Now,
                            txtStatus = "B",
                            txtLinha = string.Concat(li.intDDD, li.intPhone),
                            bitAuto = false
                        });
                        ctx.SaveChanges();

                        BlockLineResponseUI blockLineResponseUI = new BlockLineResponseUI();
                        blockLineResponseUI.Linha = string.Concat(li.intDDD, li.intPhone);
                        blockLineResponseUI.BlockLineResponse = block;
                        blockLines.Add(blockLineResponseUI);
                    }

                }
            }
            return blockLines;
        }

        public void AutoBlockLineByCustomer()
        {
            List<string> blockedNonVIPLines = new List<string>();
            List<string> nonBlockedLines = new List<string>();
            using (var ctx = new FoneClubeContext())
            {
                var allCustToBlock = ctx.GetAllCustomersToBlock().ToList();

                var defaultDate = new DateTime(2022, 11, 01);
                var custSameVigencia = ctx.GetCustomersWithSameVigenicaUnPaid().ToList();

                var customersToBlock = allCustToBlock.Except(custSameVigencia);
                var customersToSendNotif = allCustToBlock.Intersect(custSameVigencia);

                var clients = ctx.tblPersons.ToList();
                foreach (var cust in customersToBlock)
                {
                    var NonVipContellines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == cust.Value && x.bitPhoneClube == true && x.bitAtivo == true && x.intIdOperator == 4).ToList();

                    if (NonVipContellines != null && NonVipContellines.Count() > 0)
                    {
                        foreach (var li in NonVipContellines)
                        {
                            var isAlreadyBlocked = GetContelLinesByPhoneLite(string.Concat(li.intDDD, li.intPhone));
                            if (isAlreadyBlocked != null && isAlreadyBlocked.bloqueada == "NÃO")
                            {
                                var request = new BlockLine()
                                {
                                    numero = string.Concat(li.intDDD, li.intPhone),
                                    motivo = "BLOQUEIO DE IMEI",
                                    observacoes = ""
                                };

                                var block = BlockLine(request);
                                if (block != null && block.status)
                                {
                                    GenericTemplate genericTemplate = new GenericTemplate();
                                    genericTemplate.PersonId = li.intIdPerson.Value;
                                    genericTemplate.Template = new WhatsAppMessageTemplates();
                                    genericTemplate.Template.TemplateName = "f.Atraso.Corte.txt";
                                    Helper.SendGenericMessage(genericTemplate);

                                    ctx.tblContelBlockedStatus.Add(new tblContelBlockedStatus()
                                    {
                                        intIdPerson = li.intIdPerson.HasValue ? li.intIdPerson.Value : 0,
                                        dteBlocked = DateTime.Now,
                                        txtStatus = "B",
                                        txtLinha = string.Concat(li.intDDD, li.intPhone),
                                        bitAuto = true
                                    });
                                    ctx.SaveChanges();
                                    var client = clients.Where(x => x.intIdPerson == li.intIdPerson).FirstOrDefault().txtName;
                                    blockedNonVIPLines.Add("*Linha:* " + string.Concat(li.intDDD, li.intPhone) + " \n*Cliente*:" + client + "\n\n");

                                    var blockedPhone = string.Concat(li.intDDD, li.intPhone);
                                    var postStatus = GetContelLinesByPhoneLite(blockedPhone);

                                    var tblcontel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == blockedPhone);
                                    tblcontel.txtbloqueada = postStatus.bloqueada;
                                    ctx.SaveChanges();
                                }
                            }
                            else
                            {
                                //Already blocked
                            }
                        }
                    }
                    else
                    {
                        // Send for TIM/CLARO
                    }
                }

                var blockedLines = string.Join("", blockedNonVIPLines);
                WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                whatsAppAccess.SendMessage(new WhatsAppMessage()
                {
                    Message = string.Format("Linhas bloqueadas automaticamente por atraso de pagamento.  Total de linhas: {0} \n\n {1}", blockedNonVIPLines.Count, blockedLines),
                    ClientIds = AdminMsgsTo
                });

                foreach (var cust in customersToSendNotif)
                {
                    var client = clients.Where(x => x.intIdPerson == cust.Value).FirstOrDefault().txtName;
                    nonBlockedLines.Add("*Cliente*:" + client + "\n\n");
                }
                var blockedLines1 = string.Join("", nonBlockedLines);
                whatsAppAccess.SendMessage(new WhatsAppMessage()
                {
                    Message = string.Format("As linhas abaixo não foram bloqueadas por terem cobranças com vigencia duplicada.  Total de clients: {0} \n\n {1}", nonBlockedLines.Count, blockedLines1),
                    ClientIds = AdminMsgsTo
                });
                //SyncAllLinesFromContel();
            }
        }

        public void AutoUnBlockLineByCustomer(long transId)
        {
            List<string> unblockedNonVIPLines = new List<string>();
            using (var ctx = new FoneClubeContext())
            {
                var clients = ctx.tblPersons.ToList();
                var personId = ctx.tblChargingHistory.Where(x => x.intIdTransaction.HasValue && x.intIdTransaction.Value == transId).FirstOrDefault();
                if (personId != null)
                {
                    var transactionIds = ctx.tblChargingHistory.Where(x => x.intIdCustomer == personId.intIdCustomer && x.bitActive.HasValue && x.bitActive.Value).Select(x => x.intIdTransaction).ToList();
                    var charges = ctx.tblFoneclubePagarmeTransactions.Where(c => transactionIds.Any(d => c.intIdTransaction == d && c.txtOutdadetStatus != "Paid")).ToList();

                    if (charges != null && charges.Count() == 0)
                    {
                        //SyncAllLinesFromContel();
                        var lines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == personId.intIdCustomer && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value && x.bitAtivo.HasValue && x.bitAtivo.Value && x.intIdOperator.Value == 4).ToList();

                        foreach (var line in lines)
                        {
                            var isBlocked = ctx.tblContelLinhasList.Any(x => x.txtlinha == line.intDDD + "" + line.intPhone && !string.IsNullOrEmpty(x.txtbloqueada) && x.txtbloqueada != "NÃO");
                            if (isBlocked)
                            {
                                var request = new UnBlockLine()
                                {
                                    numero = string.Concat(line.intDDD, line.intPhone),
                                };
                                var block = UnBlockLine(request);
                                if (block != null && block.status)
                                {
                                    GenericTemplate genericTemplate = new GenericTemplate();
                                    genericTemplate.PersonId = line.intIdPerson.Value;
                                    genericTemplate.Template = new WhatsAppMessageTemplates();
                                    genericTemplate.Template.TemplateName = "DesbloqueioAutomatico";
                                    Helper.SendGenericMessage(genericTemplate);

                                    ctx.tblContelBlockedStatus.Add(new tblContelBlockedStatus()
                                    {
                                        intIdPerson = line.intIdPerson.HasValue ? line.intIdPerson.Value : 0,
                                        dteUnblocked = DateTime.Now,
                                        txtStatus = "U",
                                        txtLinha = string.Concat(line.intDDD, line.intPhone),
                                        bitAuto = true
                                    });
                                    ctx.SaveChanges();
                                    var client = clients.Where(x => x.intIdPerson == line.intIdPerson).FirstOrDefault().txtName;
                                    unblockedNonVIPLines.Add("*Linha:* " + string.Concat(line.intDDD, line.intPhone) + " \n *Cliente*:" + client + "\n\n");
                                }
                            }
                        }
                        if (unblockedNonVIPLines != null && unblockedNonVIPLines.Count > 0)
                        {
                            var unblockedLines = string.Join(",", unblockedNonVIPLines);
                            WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                            whatsAppAccess.SendMessage(new WhatsAppMessage()
                            {
                                Message = string.Format("Pagamento recebido: Desbloqueio automatico realizado.  Total de linhas: {0} \n\n {1}", unblockedNonVIPLines.Count, unblockedLines),
                                ClientIds = AdminMsgsTo
                            });
                        }
                    }
                }
            }
        }

        public List<BlockLineResponseUI> UnBlockLineByCustomer(BlockRequest line)
        {
            List<BlockLineResponseUI> unblockLines = new List<BlockLineResponseUI>();
            using (var ctx = new FoneClubeContext())
            {
                var lines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == line.PersonId && x.bitPhoneClube == true && x.intIdOperator == 4).ToList();
                foreach (var li in lines)
                {
                    var request = new UnBlockLine()
                    {
                        numero = string.Concat(li.intDDD, li.intPhone)
                    };
                    var unblock = UnBlockLine(request);

                    if (unblock != null && unblock.status)
                    {
                        ctx.tblContelBlockedStatus.Add(new tblContelBlockedStatus()
                        {
                            intIdPerson = line.PersonId,
                            dteUnblocked = DateTime.Now,
                            txtStatus = "U",
                            txtLinha = string.Concat(li.intDDD, li.intPhone),
                            bitAuto = false
                        });
                        ctx.SaveChanges();

                        BlockLineResponseUI blockLineResponseUI = new BlockLineResponseUI();
                        blockLineResponseUI.Linha = string.Concat(li.intDDD, li.intPhone);
                        blockLineResponseUI.BlockLineResponse = unblock;
                        unblockLines.Add(blockLineResponseUI);
                    }
                }
            }
            return unblockLines;
        }

        public void LogBlockUnBlock(string line, string status)
        {
            using (var ctx = new FoneClubeContext())
            {
                tblContelBlockedStatus blockedStatus = new tblContelBlockedStatus();
                blockedStatus.intIdPerson = -1;
                if (status == "U")
                    blockedStatus.dteUnblocked = DateTime.Now;
                if (status == "B")
                    blockedStatus.dteBlocked = DateTime.Now;
                blockedStatus.txtStatus = status;
                blockedStatus.txtLinha = line;
                blockedStatus.bitAuto = false;

                ctx.tblContelBlockedStatus.Add(blockedStatus);
                ctx.SaveChanges();
            }
        }

        public FacilHistoryResponse GetMobimatterHistory()
        {
            FacilHistoryResponse historyResponse = new FacilHistoryResponse();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var histories = ctx.tblInternationalUserPurchases.Where(x => x.intIdPerson == 6464).OrderByDescending(y => y.dteDeducted).ToList();

                    var saldo = ctx.tblInternationalUserBalance.Where(x => x.intIdPerson == 6464).FirstOrDefault().intAmountBalance;

                    historyResponse.Saldo = Convert.ToString(saldo);

                    if (histories != null)
                    {
                        historyResponse.History = new List<FacilHistoryRes>();
                        foreach (var hist in histories)
                        {
                            historyResponse.History.Add(new FacilHistoryRes()
                            {
                                AmountDeducted = hist.intAmountDeducted,
                                Category = hist.intPurchaseType == 1 ? "Activation" : "Topup",
                                DeductedDate = hist.dteDeducted,
                                Phone = hist.txtPhone,
                                Plan = hist.txtPlan
                            });
                        }
                        historyResponse.Status = true;
                    }
                    else
                    {
                        historyResponse.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                historyResponse.Status = false;
                LogHelper.LogMessage(0, "MVNOAccess:GetMobimatterHistory:" + ex.ToString());
            }
            return historyResponse;
        }

        public TopupHistoryUIResponse GetTopupHistory(string line)
        {
            TopupHistoryUIResponse response = new TopupHistoryUIResponse();
            using (var ctx = new FoneClubeContext())
            {
                var lines = ctx.tblPersonsPhones.Where(x => x.intDDD + "" + x.intPhone == line && x.bitPhoneClube == true && x.intIdOperator == 4).FirstOrDefault();

                if (lines != null)
                {
                    response.Person = ctx.tblPersons.Where(x => x.intIdPerson == lines.intIdPerson).Select(y => new Person
                    {
                        Name = y.txtName,
                        DocumentNumber = y.txtDocumentNumber
                    }).FirstOrDefault();

                    response.ContelLineData = GetContelLinesByPhone(line);
                    var topupHistoryRes = GetContelTopupHistory(line);
                    if (topupHistoryRes != null)
                    {
                        var history = new HistoryResponse();
                        history.retorno = topupHistoryRes.retorno;
                        history.mensagem = topupHistoryRes.mensagem;
                        List<HistoricoUI> histories = new List<HistoricoUI>();
                        foreach (var his in topupHistoryRes.historico)
                        {
                            var hist = new HistoricoUI();
                            hist.data_cadastro = his.data_cadastro;
                            hist.data_recarga = his.data_recarga;
                            hist.formaPagto = his.formaPagto;
                            hist.plano = his.plano;
                            hist.solicitado_por = his.solicitado_por;
                            hist.valor = his.valor;
                            histories.Add(hist);
                        }
                        history.historico = histories;
                        response.TopUpHistoryData = history;
                    }
                }
            }
            return response;
        }

        private void SyncWebFcLines()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var contelLines = ctx.tblContelLinhasList.ToList();
                    var persons = ctx.tblPersons.ToList();
                    var plans = ctx.tblPlansOptions.ToList();
                    var contelLineswithDiffOperator = ctx.GetAllContelLinesMappedToOtherOperator().ToList();

                    if (contelLineswithDiffOperator != null)
                    {
                        foreach (var phone in contelLineswithDiffOperator)
                        {
                            var txtPhone = ctx.tblPersonsPhones.Where(x => x.intId == phone.intId).FirstOrDefault();
                            if (txtPhone != null)
                            {
                                var contelLine = contelLines.Where(x => x.txtlinha == phone.intDDD + "" + phone.intPhone).FirstOrDefault();
                                txtPhone.intIdOperator = 4;
                                txtPhone.txtICCID = contelLine.txticcid;
                                txtPhone.txtPortNumber = contelLine.txtlinha;
                                txtPhone.txtNickname = contelLine.txttitular_apelido;

                                var plano = contelLine.txtplano.Replace(" ", "");
                                txtPhone.intIdPlan = GetPlanId(plano);

                                ctx.SaveChanges();
                            }
                        }
                    }

                    var contelLinesNotInWebfc = ctx.GetAllContelLinesNotInFC().ToList();
                    if (contelLinesNotInWebfc != null && contelLinesNotInWebfc.Count > 0)
                    {
                        foreach (var line in contelLinesNotInWebfc)
                        {
                            var person = persons.Where(x => x.txtDocumentNumber == line.txtdocumento_titular).FirstOrDefault();

                            var plano = line.txtplano.Replace(" GB", "");
                            var intIdPlan = GetPlanId(plano);
                            var iccidExist = ctx.tblPersonsPhones.FirstOrDefault(x => x.txtICCID == line.txticcid);
                            if (iccidExist is null)
                            {
                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                {
                                    intIdPerson = person == null ? 4158 : person.intIdPerson,
                                    intDDD = Convert.ToInt32(line.txtLinha.Substring(0, 2)),
                                    intPhone = Convert.ToInt64(line.txtLinha.Substring(2)),
                                    intIdOperator = 4,
                                    bitPhoneClube = true,
                                    bitPortability = line.txtportin == "SIM" ? true : false,
                                    intIdPlan = intIdPlan,
                                    txtNickname = line.txttitular_apelido,
                                    bitAtivo = line.txtstatus == "ATIVO" ? true : false,
                                    bitPrecoVip = false,
                                    intAmmoutPrecoVip = plans.FirstOrDefault(x => x.intIdPlan == intIdPlan).intCost,
                                    dteEntradaLinha = DateTime.Now,
                                    txtICCID = line.txticcid,
                                    bitEsim = line.txtesim == "SIM" ? true : false,
                                });
                            }
                            else
                            {
                                iccidExist.intIdPerson = person == null ? 4158 : person.intIdPerson;
                                iccidExist.intDDD = Convert.ToInt32(line.txtLinha.Substring(0, 2));
                                iccidExist.intPhone = Convert.ToInt64(line.txtLinha.Substring(2));
                                iccidExist.intIdOperator = 4;
                                iccidExist.bitPhoneClube = true;
                                iccidExist.bitPortability = line.txtportin == "SIM" ? true : false;
                                iccidExist.intIdPlan = intIdPlan;
                                iccidExist.txtNickname = line.txttitular_apelido;
                                iccidExist.bitAtivo = line.txtstatus == "ATIVO" ? true : false;
                                iccidExist.bitPrecoVip = false;
                                iccidExist.intAmmoutPrecoVip = plans.FirstOrDefault(x => x.intIdPlan == intIdPlan).intCost;
                                iccidExist.dteEntradaLinha = DateTime.Now;
                                iccidExist.txtICCID = line.txticcid;
                                iccidExist.bitEsim = line.txtesim == "SIM" ? true : false;
                            }
                        }
                        ctx.SaveChanges();
                    }

                    var allPhones1 = ctx.GetAllContelLinesInWebFCButNotInContel().ToList();
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetAllContelLinesInWebFCButNotInContel count:" + allPhones1.Count });
                    ctx.SaveChanges();

                    foreach (var phone in allPhones1)
                    {
                        var phoneDetail = GetContelLinesByPhoneManual(phone.linha);
                        if (phoneDetail != null && phoneDetail.retorno && phoneDetail.detalhes != null && !string.IsNullOrEmpty(phoneDetail.detalhes.linha))
                        {
                            var updStat = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == phoneDetail.detalhes.linha);
                            if (updStat != null)
                            {
                                var line = phoneDetail.detalhes;
                                updStat.txtlinha = line.linha;
                                updStat.txticcid = line.iccid;
                                updStat.txttitular = line.titular;
                                updStat.txttitular_apelido = line.titular_apelido;
                                updStat.txtdata_ex = Convert.ToString(line.data_ex);
                                updStat.txtdata_portout = Convert.ToString(line.data_portout);
                                updStat.txtnome_identificacao = Convert.ToString(line.nome_identificacao);
                                updStat.txtemoji = Convert.ToString(line.emoji);
                                updStat.txtdata_ativacao = line.data_ativacao;
                                updStat.txtdata_inicio_plano = line.data_inicio_plano;
                                updStat.txtdata_fim_plano = line.data_fim_plano;
                                updStat.txtdata_renovacao = line.data_renovacao;
                                updStat.txtdata_cancelamento_linha = line.data_perda_numero_falta_recarga;
                                updStat.txtplano = line.plano;
                                updStat.txtdocumento_titular = line.documento_titular;
                                updStat.txtstatus = line.status;
                                updStat.txtrecorrencia_cartao = line.recorrencia_cartao;
                                updStat.txtportin = line.portin;
                                updStat.txtesim = line.esim;
                                updStat.txtbloqueada = line.bloqueada;
                                updStat.txtrecarga_automatica = line.recarga_automatica;
                                updStat.dteAutoTopup = line.recarga_automatica == "ATIVA" ? line.data_renovacao : line.data_fim_plano;
                            }
                            else
                            {
                                ctx.tblContelLinhasList.Add(new tblContelLinhasList()
                                {
                                    txtlinha = phoneDetail.detalhes.linha,
                                    txticcid = phoneDetail.detalhes.iccid,
                                    txttitular = phoneDetail.detalhes.titular,
                                    txttitular_apelido = phoneDetail.detalhes.titular_apelido,
                                    txtdata_ex = Convert.ToString(phoneDetail.detalhes.data_ex),
                                    txtdata_portout = Convert.ToString(phoneDetail.detalhes.data_portout),
                                    txtnome_identificacao = Convert.ToString(phoneDetail.detalhes.nome_identificacao),
                                    txtdata_cancelamento_linha = phoneDetail.detalhes.data_perda_numero_falta_recarga,
                                    txtemoji = Convert.ToString(phoneDetail.detalhes.emoji),
                                    txtdata_ativacao = phoneDetail.detalhes.data_ativacao,
                                    txtdata_inicio_plano = phoneDetail.detalhes.data_inicio_plano,
                                    txtdata_fim_plano = phoneDetail.detalhes.data_fim_plano,
                                    txtdata_renovacao = phoneDetail.detalhes.data_renovacao,
                                    txtplano = phoneDetail.detalhes.plano,
                                    txtdocumento_titular = phoneDetail.detalhes.documento_titular,
                                    txtstatus = phoneDetail.detalhes.status,
                                    txtrecorrencia_cartao = phoneDetail.detalhes.recorrencia_cartao,
                                    txtportin = phoneDetail.detalhes.portin,
                                    txtesim = phoneDetail.detalhes.esim,
                                    txtbloqueada = phoneDetail.detalhes.bloqueada,
                                    txtrecarga_automatica = phoneDetail.detalhes.recarga_automatica,
                                    dteAutoTopup = phoneDetail.detalhes.recarga_automatica == "ATIVA" ? phoneDetail.detalhes.data_renovacao : phoneDetail.detalhes.data_fim_plano,
                                    bitRecAutoFC = true
                                });
                                ctx.SaveChanges();
                            }
                            ctx.SaveChanges();
                        }
                        else
                        {
                            var pp = ctx.tblPersonsPhones.Where(x => x.intDDD + "" + x.intPhone == phone.linha).FirstOrDefault();
                            if (pp != null)
                            {
                                pp.bitAtivo = false;
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncWebFcLines Error: " + ex });
                    ctx.SaveChanges();
                }
            }
        }

        public int GetPlanId(string plan1)
        {
            int id = -1;
            using (var ctx = new FoneClubeContext())
            {
                var plans = ctx.tblPlansOptions.Where(x => x.intIdOperator == 4).ToList();
                foreach (var plan in plans)
                {
                    if (plan.txtDescription.Contains(plan1))
                    {
                        id = plan.intIdPlan;
                        break;
                    }
                }
            }
            return id;
        }

        public void GetSaldoLessThanGBs()
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var configs = ctx.tblConfigSettings.ToList();

                    var lines = ctx.tblContelLinhasList.AsEnumerable().Where(x => x.txtstatus == "ATIVO" && !string.IsNullOrEmpty(x.txtrestante_dados) && Convert.ToDouble(x.txtrestante_dados.Replace(" GB", "")) < 5).ToList();
                    var dateTimeNow = DateTime.Now;
                    foreach (var line in lines)
                    {
                        int lessGBCat = -1;
                        var dados = Convert.ToDouble(line.txtrestante_dados.Replace(" GB", ""));
                        var expiryDate = DateTime.Parse(line.txtdata_renovacao).ToString("dd/MMM", CultureInfo.GetCultureInfo("pt-PT"));

                        if (dados == 0)
                            lessGBCat = 0;
                        else if (dados < 1)
                            lessGBCat = 1;
                        else if (dados < 3)
                            lessGBCat = 3;
                        else if (dados < 5)
                            lessGBCat = 5;

                        if (lessGBCat != -1)
                        {
                            var isEligible = configs.Any(x => x.txtConfigName == string.Format("SaldoReminder{0}GB", lessGBCat) && string.Equals(x.txtConfigValue, "True", StringComparison.OrdinalIgnoreCase));
                            if (isEligible)
                            {
                                var person = (from pp in ctx.tblPersonsPhones
                                              join p in ctx.tblPersons on pp.intIdPerson equals p.intIdPerson
                                              where p.bitIntl == false && string.Concat(pp.intDDD, pp.intPhone) == line.txtlinha && pp.bitPhoneClube.HasValue && pp.bitPhoneClube.Value && pp.intIdOperator.Value == 4 && pp.bitAtivo.HasValue && pp.bitAtivo.Value
                                              select new { PersonId = p.intIdPerson, Name = p.txtName, Nickname = pp.txtNickname, WAPhones = p.txtDefaultWAPhones }).FirstOrDefault();
                                if (person != null)
                                {
                                    var template = ctx.tblWhatsAppMessageTemplates.AsEnumerable().Where(x => x.txtTrigger == string.Format("saldo<{0}GB", lessGBCat)).FirstOrDefault();

                                    var pastReminder = ctx.tblSaldoReminders.Where(x => x.txtLinha == line.txtlinha && x.txtReminderGB == lessGBCat.ToString()).FirstOrDefault();
                                    if (pastReminder == null)
                                    {
                                        var isSentToOwner = configs.Any(x => x.txtConfigName == "SaldoReminderToOwner" && string.Equals(x.txtConfigValue, "True", StringComparison.OrdinalIgnoreCase));

                                        var response = new WhatsAppAccess().SendMessage(new WhatsAppMessage()
                                        {
                                            ClientIds = isSentToOwner ? person.WAPhones + "," + AdminMsgsToSaldo : line.txtlinha + "," + AdminMsgsToSaldo,
                                            Message = Helper.ReplaceFooterHeaderMsg(template.txtComment.Replace("namevariable", person.Name)
                                            .Replace("apelidovariable", person.Nickname).Replace("phonenumbervariable", line.txtlinha)
                                            .Replace("planexpiryvariable", expiryDate).Replace("saldovariable", line.txtrestante_dados))
                                        });
                                        if (response)
                                        {
                                            var deletepastReminder = ctx.tblSaldoReminders.Where(x => x.txtLinha == line.txtlinha);
                                            foreach (var dr in deletepastReminder)
                                                ctx.tblSaldoReminders.Remove(dr);
                                            ctx.SaveChanges();

                                            ctx.tblSaldoReminders.Add(new tblSaldoReminders()
                                            {
                                                intIdPerson = person.PersonId,
                                                dteDateTime = dateTimeNow,
                                                txtLinha = line.txtlinha,
                                                txtReminderGB = lessGBCat.ToString()
                                            });
                                            ctx.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        var topupHistory = GetContelTopupHistory(line.txtlinha);
                                        if (topupHistory != null && topupHistory.retorno && topupHistory.historico != null && topupHistory.historico.Count > 0)
                                        {
                                            try
                                            {
                                                var topupDate = DateTime.ParseExact(topupHistory.historico[0].data_recarga, "yyyy-MM-dd HH:mm:ss.fff",
                                                           System.Globalization.CultureInfo.InvariantCulture);

                                                if (topupDate > pastReminder.dteDateTime)
                                                {
                                                    var isSentToOwner = configs.Any(x => x.txtConfigName == "SaldoReminderToOwner" && string.Equals(x.txtConfigValue, "True", StringComparison.OrdinalIgnoreCase));

                                                    var response = new WhatsAppAccess().SendMessage(new WhatsAppMessage()
                                                    {
                                                        ClientIds = isSentToOwner ? person.WAPhones + "," + AdminMsgsToSaldo : line.txtlinha + "," + AdminMsgsToSaldo,
                                                        Message = Helper.ReplaceFooterHeaderMsg(template.txtComment.Replace("namevariable", person.Name)
                                                            .Replace("apelidovariable", person.Nickname).Replace("phonenumbervariable", line.txtlinha)
                                                            .Replace("planexpiryvariable", expiryDate).Replace("saldovariable", line.txtrestante_dados))
                                                    });
                                                    if (response)
                                                    {
                                                        var deletepastReminder = ctx.tblSaldoReminders.Where(x => x.txtLinha == line.txtlinha);
                                                        foreach (var dr in deletepastReminder)
                                                            ctx.tblSaldoReminders.Remove(dr);
                                                        ctx.SaveChanges();

                                                        ctx.tblSaldoReminders.Add(new tblSaldoReminders()
                                                        {
                                                            intIdPerson = person.PersonId,
                                                            dteDateTime = dateTimeNow,
                                                            txtLinha = line.txtlinha,
                                                            txtReminderGB = lessGBCat.ToString()
                                                        });
                                                        ctx.SaveChanges();
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncWebFcLines Error: " + ex });
                    ctx.SaveChanges();
                }
            }
        }

        public void SyncTopupHistory()
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var contelLines = ctx.tblContelLinhasList.Select(x => x.txtlinha).ToList();
                    var contelHistory = ctx.tblContelTopupHistory.ToList();
                    foreach (var line in contelLines)
                    {
                        System.Threading.Thread.Sleep(5000);

                        var topupHistoryRes = GetContelTopupHistory(line);
                        if (topupHistoryRes != null && topupHistoryRes.retorno)
                        {
                            foreach (var his in topupHistoryRes.historico)
                            {
                                var hist = contelHistory.Any(x => x.txtLinha == line && x.txtDataRecarga == his.data_recarga);
                                if (!hist)
                                {
                                    ctx.tblContelTopupHistory.Add(new tblContelTopupHistory()
                                    {
                                        txtDataCadastro = his.data_cadastro,
                                        txtDataRecarga = his.data_recarga,
                                        txtFormaPagto = his.formaPagto,
                                        txtLinha = line,
                                        txtPlano = his.plano,
                                        txtSolicitadoPor = his.solicitado_por,
                                        txtValor = his.valor,
                                        txtValorPago = his.valor_pago,
                                        dteInsertTS = DateTime.Now
                                    });
                                }
                            }
                            ctx.SaveChanges();
                        }
                    }

                    var lastUpdId = ctx.tblConfigSettings.Where(x => x.txtConfigName == "LastContelTopupSyncId").FirstOrDefault();
                    ctx.UpdateContelToChargeHistory(Convert.ToInt32(lastUpdId.txtConfigValue));
                }
                catch (Exception ex)
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "SyncTopupHistory Error: " + ex });
                    ctx.SaveChanges();
                }
            }
        }

        public void ImportTopupHistory(List<ImportTopupHistory> lstHistory)
        {
            using (var ctx = new FoneClubeContext())
            {
                foreach (var lst in lstHistory)
                {
                    DateTime dteRecharge = DateTime.Now;
                    try
                    {
                        dteRecharge = DateTime.ParseExact(lst.DteDateRec, "M/d/yy H:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        dteRecharge = DateTime.ParseExact(lst.DteDateRec, "M/d/yy", System.Globalization.CultureInfo.InvariantCulture);
                    }

                    var hist = ctx.tblContelTopupHistoryExcel.FirstOrDefault(x => x.txtLinha == lst.txtLinha && x.dteDateRec == dteRecharge);
                    if (hist is null)
                    {
                        ctx.tblContelTopupHistoryExcel.Add(new tblContelTopupHistoryExcel()
                        {
                            dteDateRec = dteRecharge,
                            txtTipo = lst.txtTipo,
                            txtApelido = lst.txtApelido,
                            txtName = lst.txtName,
                            txtLinha = lst.txtLinha,
                            txtPlano = lst.txtPlano,
                            txtValorPlano = lst.txtValorPlano,
                            txtValor = lst.txtValor
                        });
                    }
                    else
                    {
                        hist.dteDateRec = dteRecharge;
                        hist.txtTipo = lst.txtTipo;
                        hist.txtApelido = lst.txtApelido;
                        hist.txtName = lst.txtName;
                        hist.txtLinha = lst.txtLinha;
                        hist.txtPlano = lst.txtPlano;
                        hist.txtValorPlano = lst.txtValorPlano;
                        hist.txtValor = lst.txtValor;
                    }

                    ctx.SaveChanges();
                }
            }
        }
        public CompraResponse DeliverSimToCustomer(CompraRequest request)
        {
            CompraResponse compraResponse = null;
            try
            {
                if (request != null)
                {
                    var JsonStr = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    StringContent content = new StringContent(JsonStr, Encoding.UTF8, "application/json");

                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "DeliverSimToCustomer Request: " + JsonStr });
                        ctx.SaveChanges();
                    }

                    string apiMethod = string.Format("compra/solicitar");

                    using (var client = new HttpClient())
                    {
                        // Setting Authorization.  
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                        // Setting Base address.  
                        client.BaseAddress = new Uri(BaseAddress);

                        client.DefaultRequestHeaders.Accept.Clear();

                        // Setting content type.  
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Initialization.  
                        HttpResponseMessage response = new HttpResponseMessage();

                        // HTTP GET  
                        response = client.PostAsync(apiMethod, content).GetAwaiter().GetResult();

                        var contentresponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        using (var ctx = new FoneClubeContext())
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "DeliverSimToCustomer Response: " + contentresponse });
                            ctx.SaveChanges();
                        }

                        compraResponse = JsonConvert.DeserializeObject<CompraResponse>(contentresponse);
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "DeliverSimToCustomer error: " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return compraResponse;
        }

        public bool UpdateApelidoForAllLines()
        {
            using (var ctx = new FoneClubeContext())
            {
                var lines = ctx.tblContelLinhasList.ToList();
                var personsPhones = ctx.tblPersonsPhones.ToList();
                var persons = ctx.tblPersons.ToList();

                foreach (var line in lines)
                {
                    var phone = personsPhones.Where(x => x.intDDD + "" + x.intPhone == line.txtlinha).FirstOrDefault();
                    if (phone != null)
                    {
                        var client = persons.Where(x => x.intIdPerson == phone.intIdPerson).FirstOrDefault();
                        ApelidoRequest apelidoRequest = new ApelidoRequest()
                        {
                            linha = line.txtlinha,
                            linha_apelido = string.Format("{0}, {1}, {2}", client.txtName, client.txtDocumentNumber, string.IsNullOrEmpty(phone.txtNickname) ? "" : phone.txtNickname)
                        };
                        var response = UpdateApelido(apelidoRequest);
                    }
                }
            }
            return true;
        }

        public bool DownloadActivationFile(string path, string phone)
        {
            try
            {
                string filename = @"C:\Temp\Contel\ActivationFiles\Orignal\" + phone + ".pdf";
                if (!string.IsNullOrEmpty(path))
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile(new System.Uri(path), filename);
                    }
                }
                if (File.Exists(filename))
                {
                    string iccid = string.Empty, activationCode = string.Empty;
                    var text = PdfHelper.ExtractTextFromPdf(filename);
                    var allData = text.Split(new string[] { "\n" }, StringSplitOptions.None);
                    iccid = Regex.Match(allData[1], @"\d+").Value;
                    activationCode = allData[5] + allData[6];
                    var qrcode = QRCodeHelper.GenerateQRCode(activationCode);

                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Activation code:  " + qrcode });
                        ctx.SaveChanges();
                    }
                    new WhatsAppAccess().SendImage(phone, qrcode);
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Error:  " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return true;
        }

        public string DownloadActivationFileByICCID(string path, string iccid)
        {
            string activationCode = string.Empty;
            try
            {
                if (!Directory.Exists(@"C:\Temp\Contel\ActivationFiles\OrignalV2"))
                {
                    Directory.CreateDirectory(@"C:\Temp\Contel\ActivationFiles\OrignalV2");
                }
                string filename = @"C:\Temp\Contel\ActivationFiles\OrignalV2\" + iccid + ".pdf";
                if (!string.IsNullOrEmpty(path))
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile(new System.Uri(path), filename);
                    }
                }
                if (File.Exists(filename))
                {
                    var text = PdfHelper.ExtractTextFromPdf(filename);
                    if (!string.IsNullOrEmpty(text))
                    {
                        var allData = text.Split(new string[] { "\n" }, StringSplitOptions.None);
                        string txtIccid = allData.Where(x => x.Contains("ICCID")).FirstOrDefault();
                        string txtLPA = allData.Where(x => x.Contains("LPA")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(txtIccid))
                            iccid = Regex.Match(txtIccid, @"\d+").Value;
                        if (!string.IsNullOrEmpty(txtLPA))
                        {
                            activationCode = txtLPA;
                        }
                    }
                    else
                    {
                        LogHelper.LogMessageOld(1, "ExtractTextFromPdf output string is null or empty");
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "DownloadActivationFileByICCID Error:  " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return activationCode;
        }

        public bool ReDownload(string line, string inputiccid)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == line && x.txtICCID == inputiccid);
                    string iccid = inputiccid, activationCode = string.Empty;
                    var linhaDetail = GetContelLinesByPhoneManual(line);
                    if (linhaDetail != null)
                    {
                        var img = DownloadActivationFileContel(linhaDetail.detalhes.esim_pdf, line, ref iccid, ref activationCode);

                        if (tblEsim != null)
                        {
                            tblEsim.txtActivationCode = activationCode;
                            tblEsim.txtActivationDate = linhaDetail.detalhes.data_inicio_plano;
                            tblEsim.txtActivationImage = img;
                            tblEsim.txtActivationPdfUrl = linhaDetail.detalhes.esim_pdf;
                            tblEsim.txtICCID = iccid;
                            tblEsim.txtLinha = linhaDetail.detalhes.linha;
                            tblEsim.txtPlano = linhaDetail.detalhes.plano;
                            tblEsim.dteInsert = DateTime.Now;
                        }
                        else
                        {
                            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                            {
                                txtActivationCode = activationCode,
                                txtActivationDate = linhaDetail.detalhes.data_inicio_plano,
                                txtActivationImage = img,
                                txtActivationPdfUrl = linhaDetail.detalhes.esim_pdf,
                                txtICCID = iccid,
                                txtLinha = linhaDetail.detalhes.linha,
                                txtPlano = linhaDetail.detalhes.plano,
                                dteInsert = DateTime.Now
                            });
                        }
                        ctx.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string DownloadActivationFileContel(string path, string phone, ref string iccid, ref string activationCode)
        {
            string qrcode = string.Empty;
            try
            {
                string filename = string.Format(@"C:\inetroot\FacilActivationPdfs\{0}_{1}.pdf", phone, iccid);
                if (!string.IsNullOrEmpty(path))
                {
                    if (!File.Exists(filename))
                    {
                        using (WebClient Client = new WebClient())
                        {
                            Client.DownloadFile(new System.Uri(path), filename);
                        }
                    }
                }
                if (File.Exists(filename))
                {
                    var text = PdfHelper.ExtractTextFromPdf(filename);
                    if (!string.IsNullOrEmpty(text))
                    {
                        var allData = text.Split(new string[] { "\n" }, StringSplitOptions.None);
                        string txtIccid = allData.Where(x => x.Contains("ICCID")).FirstOrDefault();
                        string txtLPA = allData.Where(x => x.Contains("LPA")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(txtIccid))
                            iccid = Regex.Match(txtIccid, @"\d+").Value;
                        if (!string.IsNullOrEmpty(txtLPA))
                        {
                            activationCode = txtLPA;
                            qrcode = QRCodeHelper.GenerateQRCode(activationCode);
                        }
                    }
                    else
                    {
                        LogHelper.LogMessageOld(1, "ExtractTextFromPdf output string is null or empty");
                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "Error:  " + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return qrcode;
        }

        public void GetAllEligibleLinesForPool()
        {
            StringBuilder sb = new StringBuilder();
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var currentDate = DateTime.Now.AddDays(-30);
                    var plines = ctx.tblInternationalUserPurchases.Where(p => p.dteDeducted < currentDate && !ctx.tblInternationActivationPool.Any(p2 => p2.txtPhoneNumber == p.txtPhone)).OrderBy(x => x.dteDeducted).ToList();
                    foreach (var data in plines)
                    {
                        var data1 = GetSaldo(data.txtPhone);
                        if (data1 != null && data1.retorno && data1.data != null && data1.data.restante_dados > 0)
                        {
                            var ctPool = ctx.tblLinesForTopupPool.FirstOrDefault(x => x.bitMovedToPool == false && x.txtPhone == data.txtPhone && x.txtICCID == data.txtICCID);
                            if (ctPool is null)
                            {
                                tblLinesForTopupPool objTopup = new tblLinesForTopupPool();
                                int requiredTopup = 4;
                                int requiredSale = 4;
                                double databalance = data1.data.restante_dados;
                                var iccid = string.IsNullOrEmpty(data.txtICCID) ? "" : data.txtICCID;
                                objTopup.txtPhone = data.txtPhone;
                                objTopup.txtICCID = iccid;
                                objTopup.txtData = (databalance / 1024).ToString("0.00") + " GB";
                                objTopup.bitMovedToPool = false;
                                objTopup.dteActivated = Convert.ToString(data.dteDeducted);
                                objTopup.dteAdded = DateTime.Now;

                                // data less then 3GB = Topup: 4GB = Sell as 4GB
                                // data greater then 3GB and less then 4GB = Topup: 4GB = Sell as 4GB
                                if (databalance <= 3072)
                                {
                                    requiredTopup = 4;
                                    requiredSale = 4;
                                }
                                else if (databalance > 3072 && databalance <= 4096)
                                {
                                    requiredTopup = 4;
                                    requiredSale = 7;
                                }
                                else if (databalance > 4096 && databalance <= 7168) // data greater then 4GB and less then 7GB = Topup 4GB = Sell as 7GB
                                {
                                    requiredTopup = 4; requiredSale = 7;
                                }
                                else if (databalance > 7168 && databalance <= 12288) // data greater then 7GB and less then 12GB = Topup 7GB = Sell as 12GB
                                {
                                    requiredTopup = 7; requiredSale = 12;
                                }
                                else if (databalance > 12288 && databalance <= 20480) // data greater then 12GB and less then 20GB = Topup 12GB = Sell as 20GB
                                {
                                    requiredTopup = 12; requiredSale = 20;
                                }
                                else if (databalance > 16384 && databalance <= 20480) // data greater then 16GB and less then 20GB = Topup 4GB = Sell as 20GB
                                {
                                    requiredTopup = 4; requiredSale = 20;
                                }
                                else if (databalance > 20480 && databalance <= 30720) // data greater then 20GB and less then 42GB = Topup 20GB = Sell as 42GB
                                {
                                    requiredTopup = 20; requiredSale = 42;
                                }
                                else if (databalance > 30720 && databalance <= 37888) // data greater then 30GB and less then 37GB = Topup 12GB = Sell as 42GB
                                {
                                    requiredTopup = 12; requiredSale = 42;
                                }
                                else if (databalance > 37888 && databalance <= 38912) // data greater then 37GB and less then 38GB = Topup 7GB = Sell as 42GB
                                {
                                    requiredTopup = 7; requiredSale = 42;
                                }
                                else if (databalance > 38912) // data greater then 42GB = Topup 4GB = Sell as 42GB
                                {
                                    requiredTopup = 4; requiredSale = 42;
                                }
                                objTopup.intRequiredTopup = requiredTopup;
                                objTopup.intRequiredSale = requiredSale;
                                ctx.tblLinesForTopupPool.Add(objTopup);
                                ctx.SaveChanges();

                                ResetLine resetReq = new ResetLine()
                                {
                                    linha = data.txtPhone,
                                    motivo = "Troca para eSIM",
                                    novo_iccid = ""
                                };
                                var resetRes = mVNOAccess.ResetLine(resetReq);
                                if (resetRes != null && string.Equals(resetRes.message, "Troca de chip realizada com sucesso.", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ctxPool = ctx.tblInternationActivationPool.Any(x => x.txtPhoneNumber == data.txtPhone && x.txtICCID == iccid && x.txtResetICCID == resetRes.iccid);
                                    if (!ctxPool)
                                    {
                                        var poolFF = new tblInternationActivationPool()
                                        {
                                            bitFailedPostActivation = false,
                                            bitReadyForReActivation = true,
                                            dteActivation = DateTime.Now,
                                            dteReset = DateTime.Now,
                                            intIdPerson = 1,
                                            intIdPersonReset = 1,
                                            intIdPlan = requiredSale,
                                            txtICCID = resetRes.iccid,
                                            txtResetICCID = resetRes.iccid,
                                            txtPhoneNumber = data.txtPhone,
                                            txtResetStatus = "Pending",
                                            txtStatus = "Resell Pool + Topup",
                                            intRequiredTopup = requiredTopup,
                                            intRequiredSale = requiredSale
                                        };
                                        ctx.tblInternationActivationPool.Add(poolFF);
                                        ctx.SaveChanges();

                                        int poolId = poolFF.intId;

                                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                        {
                                            intActivationPoolId = poolId,
                                            txtICCID = resetRes.iccid,
                                            txtPhone = data.txtPhone,
                                            txtStatus = "Resell Pool + Topup",
                                            dteAction = DateTime.Now,
                                            txtDoneBy = "System"
                                        });
                                        ctx.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(0, "GetAllEligibleLinesForPool error: " + ex.ToString());

            }
        }
    }
}
