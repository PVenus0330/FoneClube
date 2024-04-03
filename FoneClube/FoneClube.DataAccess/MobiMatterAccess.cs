using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.DataAccess.security;
using FoneClube.DataAccess.Utilities;

namespace FoneClube.DataAccess
{
    public class FacilAccess
    {
        string InternalError = "Internal Error, Please contact Facil.tel";
        static string AdminMsgsTo = "552192051599";
        static string PdfUrl = "https://api.foneclube.com.br/api/facil/line/activation/pdf/";

        public FacilAccess()
        {
            using (var ctx = new FoneClubeContext())
            {
                var numbers = ctx.tblConfigSettings.Where(x => x.txtConfigName == "WhatsAppMsgToAdmins").FirstOrDefault();
                AdminMsgsTo = numbers.txtConfigValue;
            }
        }

        private static int GetUserIdFromToken(string token)
        {
            int userId = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var decrpyt = EncryptionHelper.DecryptString("FacilApiToken", token);
                    if (!string.IsNullOrEmpty(decrpyt))
                    {
                        string[] splt = decrpyt.Split('-');
                        if (splt != null && splt.Length > 0)
                        {
                            userId = Convert.ToInt32(splt[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:ValidateToken:" + ex.ToString());
            }
            return userId;
        }

        public static bool IsNewActivationEnabled()
        {
            bool enabled = false;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    enabled = Convert.ToBoolean(ctx.tblConfigSettings.Where(x => x.txtConfigName == "UseActivationV2").FirstOrDefault().txtConfigValue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(0, "FacilAccess:IsNewActivationEnabled:" + ex.ToString());
            }
            return enabled;
        }

        private static string GetAmountByPlan(int id, int personId, ref string strContelPrice)
        {
            string price = null;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    tblContelPlanMapping plan = new tblContelPlanMapping();
                    plan = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == id && x.intIdPerson == personId).FirstOrDefault();
                    if (plan is null)
                        plan = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == id && x.intIdPerson == 1).FirstOrDefault();
                    price = "U$ " + Convert.ToDouble(plan.txtPrice, CultureInfo.InvariantCulture);
                    strContelPrice = "R$ " + Convert.ToDouble(plan.txtContelPrice, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(personId, "FacilAccess:GetAmountByPlan:" + ex.ToString());
            }
            return price;
        }

        private static string GetPlanNameByPlanId(int id, int personId, ref string strContelPrice)
        {
            string planv = null;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    tblContelPlanMapping plan = new tblContelPlanMapping();
                    plan = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == id && x.intIdPerson == personId).FirstOrDefault();
                    if (plan is null)
                        planv = plan.txtPlanName;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(personId, "FacilAccess:GetAmountByPlan:" + ex.ToString());
            }
            return planv;
        }


        private static int GetPlanIdByGB(int gb)
        {

            int id = 315;
            switch (gb)
            {
                case 4: id = 315; break;
                case 7: id = 316; break;
                case 12: id = 317; break;
                case 20: id = 318; break;
                case 42: id = 319; break;
            }
            return id;
        }

        private static int GetGBById(int gb)
        {

            int id = 4;
            switch (gb)
            {
                case 315: id = 4; break;
                case 316: id = 7; break;
                case 317: id = 12; break;
                case 318: id = 20; break;
                case 319: id = 42; break;
            }
            return id;
        }

        private static tblPersons GetNameById(int id)
        {
            tblPersons person = null;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    person = ctx.tblPersons.Where(x => x.intIdPerson == id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(id, "FacilAccess:GetNameById:" + ex.ToString());
            }
            return person;
        }

        private void DownloadAndSendESIMPdf(string phone, ref string iccid, ref string activationcode)
        {
            MVNOAccess mvno = new MVNOAccess();
            try
            {
                var lineInfo = mvno.GetContelLinesByPhoneManual(phone);
                if (lineInfo != null)
                {
                    var qrcode = mvno.DownloadActivationFileContel(lineInfo.detalhes.esim_pdf, lineInfo.detalhes.linha, ref iccid, ref activationcode);
                    using (var ctx = new FoneClubeContext())
                    {
                        var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == lineInfo.detalhes.linha && x.txtICCID == lineInfo.detalhes.iccid);
                        if (tblEsim != null)
                        {
                            tblEsim.txtActivationCode = activationcode;
                            tblEsim.txtActivationDate = lineInfo.detalhes.data_inicio_plano;
                            tblEsim.txtActivationImage = qrcode;
                            tblEsim.txtActivationPdfUrl = lineInfo.detalhes.esim_pdf;
                            tblEsim.txtICCID = iccid;
                            tblEsim.txtLinha = lineInfo.detalhes.linha;
                            tblEsim.txtPlano = lineInfo.detalhes.plano;
                            tblEsim.dteInsert = DateTime.Now;
                        }
                        else
                        {
                            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                            {
                                txtActivationCode = activationcode,
                                txtActivationDate = lineInfo.detalhes.data_inicio_plano,
                                txtActivationImage = qrcode,
                                txtActivationPdfUrl = lineInfo.detalhes.esim_pdf,
                                txtICCID = iccid,
                                txtLinha = lineInfo.detalhes.linha,
                                txtPlano = lineInfo.detalhes.plano,
                                dteInsert = DateTime.Now
                            });
                        }
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(0, "FacilAccess:ValidateToken:" + ex.ToString());
            }
        }

        public tblESimICCIDPool ValidateNumberByICCIDInLoop(int userId, string uniqueId)
        {
            tblESimICCIDPool valResp = null;
            int maxRetry = 10;
            bool FailedOnFirst = false;
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var newiccid = ctx.tblESimICCIDPool.FirstOrDefault(x => x.bitActivated == false && x.bitValidated && x.txtTipo == "eSIM");
                    if (newiccid != null)
                    {
                        var response = mVNOAccess.ValidatePhoneByICCID(newiccid.txtICCID);
                        if (response != null && response.retorno && string.IsNullOrEmpty(response.numero))
                        {
                            newiccid.bitActivated = true;
                            newiccid.dteUpdate = DateTime.Now;
                            newiccid.dteActivated = DateTime.Now;
                            newiccid.intIdPersonActivated = userId;
                            ctx.SaveChanges();

                            LogHelper.LogMessage(userId, "FacilAccess:ValidateNumberByICCIDInLoop: ICCID valid at first attempt", uniqueId);
                            valResp = newiccid;
                        }
                        else
                        {
                            newiccid.dteUpdate = DateTime.Now;
                            newiccid.bitValidated = false;
                            ctx.SaveChanges();

                            FailedOnFirst = true;
                        }

                        if (FailedOnFirst)
                        {
                            for (int i = 1; i <= maxRetry; i++)
                            {
                                try
                                {
                                    var newiccid1 = ctx.tblESimICCIDPool.FirstOrDefault(x => x.bitActivated == false && x.bitValidated && x.txtTipo == "eSIM");
                                    var response1 = mVNOAccess.ValidatePhoneByICCID(newiccid1.txtICCID);
                                    if (response1 != null && response1.retorno && string.IsNullOrEmpty(response1.numero))
                                    {
                                        newiccid1.bitActivated = true;
                                        newiccid1.dteUpdate = DateTime.Now;
                                        newiccid1.dteActivated = DateTime.Now;
                                        newiccid1.intIdPersonActivated = userId;
                                        ctx.SaveChanges();

                                        valResp = newiccid1;
                                        LogHelper.LogMessage(userId, string.Format("FacilAccess:ValidateNumberByICCIDInLoop: taken iccid from pool at: {0}", i), uniqueId);
                                        break;
                                    }
                                    else
                                    {
                                        newiccid1.dteUpdate = DateTime.Now;
                                        newiccid1.bitValidated = false;
                                        ctx.SaveChanges();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.LogMessage(userId, "ValidateNumberByICCIDInLoop error:" + ex.ToString(), uniqueId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:ValidateNumberByICCIDInLoop:" + ex.ToString(), uniqueId);
            }
            return valResp;
        }

        private ValidateICCIDResponse ValidateInLoop(int userId, string uniqueId, string iccid, string env)
        {
            ValidateICCIDResponse response = null;
            int maxRetry = 20;
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                System.Threading.Thread.Sleep(2000);
                response = mVNOAccess.ValidateICCID(iccid, env);
                if (response != null && response.retorno && response.info != null && !string.IsNullOrEmpty(response.info.numero_ativado))
                {
                    LogHelper.LogMessage(userId, "FacilAccess:ValidateInLoop: Recieved Line Number within 5sec", uniqueId);
                    return response;
                }
                else
                {
                    for (int i = 1; i < maxRetry; i++)
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(2000);
                            response = mVNOAccess.ValidateICCID(iccid, env);
                            if (response != null && response.retorno && response.info != null && !string.IsNullOrEmpty(response.info.numero_ativado))
                            {
                                LogHelper.LogMessage(userId, string.Format("FacilAccess:ValidateInLoop: Recieved Line Number with {0}sec:", 5 + (2 * i)), uniqueId);
                                break;
                            }
                        }
                        catch
                        { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:ValidateInLoop:" + ex.ToString(), uniqueId);
            }
            return response;
        }

        public string DownloadActivationFileContelInloop(int userId, string uniqueId, string pdf, string phone, ref string iccid, ref string activationcode)
        {
            string response = null;
            int maxRetry = 15;
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                System.Threading.Thread.Sleep(2000);
                response = mVNOAccess.DownloadActivationFileContel(pdf, phone, ref iccid, ref activationcode);
                if (!string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(response))
                {
                    LogHelper.LogMessage(userId, "FacilAccess:DownloadActivationFileContelInloop: Recieved Activation code within 2sec", uniqueId);
                    return response;
                }
                else
                {
                    for (int i = 1; i < maxRetry; i++)
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(2000);
                            response = mVNOAccess.DownloadActivationFileContel(pdf, phone, ref iccid, ref activationcode);
                            if (!string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(response))
                            {
                                LogHelper.LogMessage(userId, string.Format("FacilAccess:DownloadActivationFileContelInloop: Recieved Activation code within {0}sec:", 2 + (2 * i)), uniqueId);
                                break;
                            }
                        }
                        catch
                        { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:DownloadActivationFileContelInloop:" + ex.ToString(), uniqueId);
            }
            return response;
        }

        private static string GetuniqueId(int userId)
        {
            string uniqueId = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var currentDate = DateTime.Now;
                    int current = Convert.ToInt32(ctx.tblConfigSettings.Where(x => x.txtConfigName == "ActivationCount").FirstOrDefault().txtConfigValue);
                    var lastDate = Convert.ToDateTime(ctx.tblConfigSettings.Where(x => x.txtConfigName == "ActivationCountTime").FirstOrDefault().txtConfigValue);
                    if (lastDate.Date.Equals(currentDate.Date) == false)
                    {
                        current = 0;
                    }
                    var ftr = DateTime.Now.ToString("yyyy.MM.dd");
                    uniqueId = string.Format("{0}.{1}", ftr, current + 1);
                    ctx.tblConfigSettings.FirstOrDefault(x => x.txtConfigName == "ActivationCount").txtConfigValue = Convert.ToString(current + 1);
                    ctx.tblConfigSettings.FirstOrDefault(x => x.txtConfigName == "ActivationCountTime").txtConfigValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:GetuniqueId:" + ex.ToString());
            }
            return uniqueId;
        }

        public static ValidateTokenResponse ValidateToken(string token)
        {
            ValidateTokenResponse response = new ValidateTokenResponse();
            int id = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var decrpyt = EncryptionHelper.DecryptString("FacilApiToken", token);
                    if (!string.IsNullOrEmpty(decrpyt))
                    {
                        string[] splt = decrpyt.Split('-');
                        if (splt != null && splt.Length > 0)
                        {
                            id = Convert.ToInt32(splt[0]);
                            var apiToken = ctx.tblApiTokenInfo.FirstOrDefault(x => x.intIdPerson == id && x.txtToken == token);
                            if (apiToken is null)
                            {
                                response.Status = false;
                                response.Error = "Invalid Token";
                            }
                            else
                            {
                                response.Status = true;
                            }
                        }
                        else
                        {
                            response.Status = false;
                            response.Error = "Invalid Token";
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Error = "Invalid Token";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(id, "FacilAccess:ValidateToken:" + ex.ToString());

                response.Status = false;
                response.Error = "Invalid Token";
            }
            return response;
        }

        public static bool IsContelSaldoAvailable(ref double saldo)
        {
            saldo = 0;
            try
            {
                var result = new MVNOAccess().GetRemainingSaldoForCompany();
                if (result != null && result.saldo != null && !string.IsNullOrEmpty(result.saldo))
                {
                    var dbVal = Convert.ToDouble(result.saldo, CultureInfo.InvariantCulture);
                    saldo = dbVal;
                    if (dbVal > 1000)
                    {
                        return true;
                    }
                    else if (dbVal <= 1000)
                    {
                        WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                        whatsAppAccess.SendMessage(new WhatsAppMessage()
                        {
                            Message = string.Format("Você tem apenas R$ {0} para recarga automática em sua conta: R$", dbVal),
                            ClientIds = AdminMsgsTo
                        });

                        if (dbVal > 100 && dbVal <= 1000)
                        {
                            return true;
                        }
                        else
                        {
                            LogHelper.LogMessage(1, "FacilAccess:IsContelSaldoAvailable: Saldo having less than R$100 ");
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                {
                    LogHelper.LogMessage(1, "FacilAccess:IsContelSaldoAvailable: Contel API no response but returing true");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(1, "FacilAccess:IsContelSaldoAvailable:" + ex.ToString());
                return false;
            }
        }

        public static bool IsUserSaldoAvailable(int personId, int planId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var balance = ctx.tblInternationalUserBalance.Where(x => x.intIdPerson == personId).FirstOrDefault();
                    var negativeAllowance = Convert.ToInt32(ctx.tblConfigSettings.Where(x => x.txtConfigName == "IntlNegativeAllowance").FirstOrDefault().txtConfigValue);
                    if (balance != null)
                    {
                        tblContelPlanMapping plan = new tblContelPlanMapping();
                        plan = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == planId && x.intIdPerson == personId).FirstOrDefault();
                        if (plan is null)
                            plan = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == planId && x.intIdPerson == 1).FirstOrDefault();
                        if (plan != null && balance.intAmountBalance > Convert.ToDecimal(plan.txtPrice, CultureInfo.InvariantCulture))
                        {
                            return true;
                        }
                        else if ((-negativeAllowance) < balance.intAmountBalance && (-negativeAllowance < (balance.intAmountBalance - Convert.ToDecimal(plan.txtPrice, CultureInfo.InvariantCulture))))
                        {
                            LogHelper.LogMessage(personId, "FacilAccess:IsUserSaldoAvailable: User don't have enough balance but within Negative allowance limit, hence allowing activation: " + personId);
                            return true;
                        }
                        else
                        {
                            LogHelper.LogMessage(personId, "FacilAccess:IsUserSaldoAvailable: User not has any balance to activate a line: " + personId);
                            return false;
                        }
                    }
                    else
                    {
                        LogHelper.LogMessage(personId, "FacilAccess:IsUserSaldoAvailable: User not has any balance to activate a line: " + personId);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(personId, "FacilAccess:IsUserSaldoAvailable:" + ex.ToString());
                return false;
            }
        }

        public static bool IsResetICCIDAvailableInPool(int userId, string uniqueId, int plan, int personId, out ResettedInfo resettedInfo, ref bool isTopupNeeded)
        {
            resettedInfo = null;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    bool IsRequiredAvl = true;
                    DateTime now = DateTime.Now;
                    DateTime nowto23hrs = now.AddHours(-23);
                    int gb = GetGBById(plan);
                    var activationPoolWithOutRecharge = ctx.tblInternationActivationPool.FirstOrDefault(x => x.bitReadyForReActivation == true && (x.bitFailedPostActivation.HasValue && x.bitFailedPostActivation.Value) && (x.dteReset > nowto23hrs && x.dteReset <= now) && (x.intIdPlan.HasValue && x.intIdPlan.Value == plan) && x.txtStatus == "Resell Pool + Blocked");
                    tblInternationActivationPool activationPoolWithRecharge = ctx.tblInternationActivationPool.FirstOrDefault(x => x.bitReadyForReActivation == true && (!x.bitFailedPostActivation.HasValue || !x.bitFailedPostActivation.Value) && x.txtStatus == "Resell Pool + Topup" && (x.intRequiredSale.HasValue && x.intRequiredSale.Value == gb));
                    if (activationPoolWithRecharge == null)
                    {
                        IsRequiredAvl = false;
                        activationPoolWithRecharge = ctx.tblInternationActivationPool.FirstOrDefault(x => x.bitReadyForReActivation == true && (!x.bitFailedPostActivation.HasValue || !x.bitFailedPostActivation.Value) && x.txtStatus == "Resell Pool + Topup");
                    }

                    if (activationPoolWithOutRecharge != null)
                    {
                        resettedInfo = new ResettedInfo()
                        {
                            ResettedICCID = activationPoolWithOutRecharge.txtICCID,
                            ResettedPhone = activationPoolWithOutRecharge.txtPhoneNumber
                        };
                        LogHelper.LogMessage(userId, string.Format("FacilAccess:IsResetICCIDAvailableInPool: Sending failed activated ICCID:{0} and Phone: {1} for Re-sale and making bitReadyForReActivation as false so that it won't pickup again", activationPoolWithOutRecharge.txtICCID, activationPoolWithOutRecharge.txtPhoneNumber), uniqueId);
                        activationPoolWithOutRecharge.bitReadyForReActivation = false;
                        activationPoolWithOutRecharge.txtResetStatus = "Re-Activated";
                        activationPoolWithOutRecharge.txtStatus = "Resold";
                        ctx.SaveChanges();

                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                        {
                            intActivationPoolId = activationPoolWithOutRecharge.intId,
                            txtICCID = activationPoolWithOutRecharge.txtICCID,
                            txtPhone = activationPoolWithOutRecharge.txtPhoneNumber,
                            txtStatus = "Resold",
                            dteAction = DateTime.Now,
                            txtDoneBy = "System"
                        });
                        ctx.SaveChanges();

                        isTopupNeeded = false;
                        return true;
                    }
                    else if (activationPoolWithRecharge != null)
                    {
                        resettedInfo = new ResettedInfo()
                        {
                            ResettedICCID = activationPoolWithRecharge.txtResetICCID,
                            ResettedPhone = activationPoolWithRecharge.txtPhoneNumber,
                            RequiredTopup = IsRequiredAvl ? activationPoolWithRecharge.intRequiredTopup : null
                        };
                        LogHelper.LogMessage(userId, string.Format("FacilAccess:IsResetICCIDAvailableInPool: Sending resetted ICCID:{0} and Phone: {1} for Topup and making bitReadyForReActivation as false so that another popup not done", activationPoolWithRecharge.txtResetICCID, activationPoolWithRecharge.txtPhoneNumber), uniqueId);
                        activationPoolWithRecharge.bitReadyForReActivation = false;
                        activationPoolWithRecharge.txtResetStatus = "Re-Activated";
                        activationPoolWithRecharge.txtStatus = "Resold";
                        ctx.SaveChanges();

                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                        {
                            intActivationPoolId = activationPoolWithRecharge.intId,
                            txtICCID = activationPoolWithRecharge.txtResetICCID,
                            txtPhone = activationPoolWithRecharge.txtPhoneNumber,
                            txtStatus = "Resold",
                            dteAction = DateTime.Now,
                            txtDoneBy = "System"
                        });
                        ctx.SaveChanges();

                        isTopupNeeded = true;
                        return true;
                    }
                    else
                    {
                        LogHelper.LogMessage(userId, "FacilAccess:IsResetICCIDAvailableInPool: There are no resetted sim available, proceed with new activation", uniqueId);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(userId, "FacilAccess:IsResetICCIDAvailableInPool:" + ex.ToString(), uniqueId);
                return false;
            }
        }

        public TokenResponse GenerateToken(TokenRequest request)
        {
            TokenResponse token = new TokenResponse();
            int id = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var hashedPassword = new Security().EncryptPassword(request.Password);
                    var user = ctx.tblPersons.FirstOrDefault(x => x.txtEmail == request.Email && x.txtPassword == hashedPassword.Password);
                    if (user != null)
                    {
                        id = user.intIdPerson;
                        string strtoken = EncryptionHelper.EncryptString("FacilApiToken", user.intIdPerson + "-" + new Random().Next(0, 1000000).ToString("D6"));
                        var apiToken = ctx.tblApiTokenInfo.FirstOrDefault(x => x.intIdPerson == user.intIdPerson);
                        if (apiToken is null)
                        {
                            LogHelper.LogMessage(id, "New Token Generated for " + user.txtName);
                            ctx.tblApiTokenInfo.Add(new tblApiTokenInfo()
                            {
                                txtToken = strtoken,
                                txtSource = user.txtName,
                                dteCreated = DateTime.Now,
                                intIdPerson = user.intIdPerson
                            });
                        }
                        else
                        {
                            apiToken.txtToken = strtoken;
                            apiToken.dteUpdated = DateTime.Now;
                        }
                        ctx.SaveChanges();

                        token.Status = true;
                        token.Token = strtoken;
                    }
                    else
                    {
                        token.Status = false;
                        token.Error = "Invalid Password";
                    }
                }
            }
            catch (Exception ex)
            {
                token.Status = false;
                token.Error = InternalError;
                LogHelper.LogMessage(id, "FacilAccess:GenerateToken:" + ex.ToString());
            }
            return token;
        }

        public FacilICCIDResponse ValidateICCID(string ICCID)
        {
            FacilICCIDResponse facilGenericResponse = new FacilICCIDResponse();
            try
            {
                MVNOAccess mVNOAccess = new MVNOAccess();
                var result = mVNOAccess.ValidateICCID(ICCID);
                if (result != null)
                {
                    if (result.retorno && result.info != null)
                    {
                        string iccid = result.info.cliente, activationcode = string.Empty;
                        DownloadAndSendESIMPdf(result.info.numero_ativado, ref iccid, ref activationcode);
                        facilGenericResponse.Status = true;
                        facilGenericResponse.Data = new FacilICCIDRes()
                        {
                            ActivationDate = result.info.data_ativacao,
                            Client = result.info.cliente,
                            ICCID = result.info.iccid,
                            PhoneNumber = result.info.numero_ativado,
                            Plan = result.info.plano,
                            PortedNumber = result.info.numero_portado,
                            ActivationPDFLink = !string.IsNullOrEmpty(activationcode) ? PdfUrl + result.info.numero_ativado + "/" + iccid : "",
                            ActivationCode = activationcode
                        };
                    }
                    else
                    {
                        facilGenericResponse.Status = false;
                        facilGenericResponse.Error = result.mensagem;
                    }
                }
                else
                {
                    facilGenericResponse.Status = false;
                    facilGenericResponse.Error = InternalError;
                }
            }
            catch (Exception ex)
            {
                facilGenericResponse.Status = false;
                facilGenericResponse.Error = InternalError;
                LogHelper.LogMessage(0, "FacilAccess:ValidateICCID:" + ex.ToString());
            }
            return facilGenericResponse;
        }

        public FacilPlanResponse GetAllPlans(string key)
        {
            FacilPlanResponse facilPlanResponse = new FacilPlanResponse();
            try
            {
                var userId = GetUserIdFromToken(key);
                using (var ctx = new FoneClubeContext())
                {
                    List<tblContelPlanMapping> plans = new List<tblContelPlanMapping>();
                    plans = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == userId).ToList();
                    if (plans == null || plans.Count == 0)
                        plans = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == 1).ToList();
                    if (plans != null && plans.Count > 0)
                    {
                        facilPlanResponse.Plans = new List<FacilPlanRes>();
                        foreach (var plan in plans)
                        {
                            var planId = new FacilPlanRes()
                            {
                                PlanId = plan.intIdPlan,
                                PlanName = plan.txtPlanName,
                                Price = "U$ " + Convert.ToDouble(plan.txtPrice, CultureInfo.InvariantCulture)
                            };
                            facilPlanResponse.Plans.Add(planId);
                        };
                        facilPlanResponse.Status = true;
                    }
                    else
                    {
                        facilPlanResponse.Status = false;
                        facilPlanResponse.Error = InternalError;
                    }
                }
            }
            catch (Exception ex)
            {
                facilPlanResponse.Status = false;
                facilPlanResponse.Error = InternalError;
                LogHelper.LogMessage(0, "FacilAccess:ValidateICCID:" + ex.ToString());
            }
            return facilPlanResponse;
        }

        public string GetTokenByUserId(int userId)
        {
            string token = "";
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    token = ctx.tblApiTokenInfo.Where(x => x.intIdPerson == userId).FirstOrDefault().txtToken;
                }
            }
            catch (Exception ex)
            {
                token = "";
            }
            return token;
        }

        public FacilGenericResponse BlockLine(FacilGenericRequest Line)
        {
            FacilGenericResponse facilGenericResponse = new FacilGenericResponse();
            int userId = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    userId = GetUserIdFromToken(Line.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == Line.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        BlockLine request = new BlockLine()
                        {
                            numero = Line.Phone.PhoneNumber,
                            motivo = "BLOQUEIO DE IMEI",
                            observacoes = ""
                        };
                        LogHelper.LogMessage(userId, "Recieved block request for line: " + Line.Phone.PhoneNumber);
                        var result = new MVNOAccess().BlockLine(request, Line.Enviroment.ToString());
                        if (result != null)
                        {
                            if (result.status)
                            {
                                facilGenericResponse.Status = true;
                                facilGenericResponse.Info = "Blocked Successfully";
                            }
                            else
                            {
                                facilGenericResponse.Status = result.status;
                                facilGenericResponse.Error = FacilUtil.GetStatusMapping(result.mensagem);
                            }
                            LogHelper.LogMessage(userId, string.Format("Block request for line: {0} Blocked Successfully", Line.Phone.PhoneNumber));
                        }
                        else
                        {
                            facilGenericResponse.Status = false;
                            facilGenericResponse.Error = InternalError;
                            LogHelper.LogMessage(userId, string.Format("Block request for line: {0} Blocked Successfully", Line.Phone.PhoneNumber));
                        }
                    }
                    else
                    {
                        facilGenericResponse.Status = false;
                        facilGenericResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", Line.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                facilGenericResponse.Status = false;
                facilGenericResponse.Error = InternalError;
                LogHelper.LogMessage(userId, "FacilAccess:BlockLine:" + ex.ToString());
            }
            return facilGenericResponse;
        }

        public FacilGenericResponse UnBlockLine(FacilGenericRequest Line)
        {
            int userId = 0;
            FacilGenericResponse facilGenericResponse = new FacilGenericResponse();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    userId = GetUserIdFromToken(Line.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == Line.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        UnBlockLine request = new UnBlockLine()
                        {
                            numero = Line.Phone.PhoneNumber,
                        };
                        LogHelper.LogMessage(userId, "Recieved unblock request for line: " + Line.Phone.PhoneNumber);
                        var result = new MVNOAccess().UnBlockLine(request, Line.Enviroment.ToString());
                        if (result != null)
                        {
                            if (result.status)
                            {
                                facilGenericResponse.Status = true;
                                facilGenericResponse.Info = "UnBlocked Successfully";
                            }
                            else
                            {
                                facilGenericResponse.Status = result.status;
                                facilGenericResponse.Error = result.mensagem;
                            }
                            LogHelper.LogMessage(userId, string.Format("UnBlock request for line: {0} UnBlocked Successfully", Line.Phone.PhoneNumber));
                        }
                        else
                        {
                            facilGenericResponse.Status = false;
                            facilGenericResponse.Error = InternalError;
                            LogHelper.LogMessage(userId, string.Format("UnBlock request for line: {0} UnBlocked Successfully", Line.Phone.PhoneNumber));
                        }
                    }
                    else
                    {
                        facilGenericResponse.Status = false;
                        facilGenericResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", Line.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                facilGenericResponse.Status = false;
                facilGenericResponse.Error = InternalError;
                LogHelper.LogMessage(userId, "FacilAccess:UnBlockLine:" + ex.ToString());
            }
            return facilGenericResponse;
        }

        public GetPhoneDetailResponse GetPhoneDetail(FacilGenericRequest request)
        {
            int userId = 0;
            GetPhoneDetailResponse phoneResponse = new GetPhoneDetailResponse();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    userId = GetUserIdFromToken(request.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == request.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        var result = new MVNOAccess().GetContelLinesByPhoneManual(request.Phone.PhoneNumber, request.Enviroment.ToString());
                        if (result != null && result.detalhes != null)
                        {
                            phoneResponse.Status = true;
                            string iccid = result.detalhes.iccid, activationcode = string.Empty;
                            DownloadAndSendESIMPdf(result.detalhes.linha, ref iccid, ref activationcode);

                            phoneResponse.Data = new GetPhoneDetailRes()
                            {
                                Id = result.detalhes.id,
                                LineId = result.detalhes.id_linha,
                                Line = result.detalhes.linha,
                                NickName = result.detalhes.linha_apelido,
                                ICCID = iccid,
                                Owner = result.detalhes.titular,
                                OwnerNickName = result.detalhes.titular_apelido,
                                NameIdentifier = Convert.ToString(result.detalhes.nome_identificacao),
                                Emoji = result.detalhes.emoji,
                                ActivationDate = result.detalhes.data_ativacao,
                                StartDate = result.detalhes.data_inicio_plano,
                                EndDate = result.detalhes.data_fim_plano,
                                RenewalDate = result.detalhes.data_renovacao,
                                OperatorDeleteDate = result.detalhes.data_perda_numero_falta_recarga,
                                Plan = result.detalhes.plano,
                                PlanType = FacilUtil.GetStatusMapping(result.detalhes.tipo_linha),
                                OwnerUniqueId = result.detalhes.documento_titular,
                                LineStatus = FacilUtil.GetStatusMapping(result.detalhes.status),
                                LineCancellationReason = result.detalhes.motivo_cancelamento_linha,
                                LineCancellationDate = Convert.ToString(result.detalhes.data_cancelamento_linha),
                                RecurrenceCard = FacilUtil.GetStatusMapping(result.detalhes.recorrencia_cartao),
                                PortIn = result.detalhes.portin == "SIM" ? true : false,
                                Blocked = result.detalhes.bloqueada == "NÃO" ? false : true,
                                BlockedDate = result.detalhes.bloqueada != "NÃO" ? result.detalhes.bloqueada : "",
                                eSim = result.detalhes.esim == "SIM" ? true : false,
                                AutomaticRecharge = FacilUtil.GetStatusMapping(result.detalhes.recarga_automatica),
                                AutomaticRechargePlan = result.detalhes.recarga_automatica_plano,
                                PortabilityDonarLine = result.detalhes.portabilidade_linha_doadora,
                                PortabilityStatus = result.detalhes.portabilidade_status,
                                PortabilityRegistrationDate = result.detalhes.portabilidade_data_cadastro,
                                PortabilityScheduleDate = result.detalhes.portabilidade_data_agendamento,
                                PortabilityAcceptedDate = result.detalhes.portabilidade_data_aceite,
                                PortabilityCompletedDate = result.detalhes.portabilidade_data_sucesso,
                                ActivationPDFLink = result.detalhes.esim == "SIM" ? PdfUrl + result.detalhes.linha + "/" + result.detalhes.iccid : "",
                                ActivationCode = activationcode
                            };
                        }
                        else
                        {
                            phoneResponse.Status = false;
                            phoneResponse.Error = InternalError;
                        }
                    }
                    else
                    {
                        phoneResponse.Status = false;
                        phoneResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", request.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                phoneResponse.Status = false;
                phoneResponse.Error = InternalError;
                LogHelper.LogMessage(userId, "FacilAccess:GetPhoneDetail:" + ex.ToString());
            }
            return phoneResponse;
        }

        public GetAllPhoneDetailResponse GetAllPhoneDetail(ValidateApiKeyRequest request)
        {
            GetAllPhoneDetailResponse phoneResponse = new GetAllPhoneDetailResponse();
            var intIDPerson = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = FacilAccess.GetUserIdFromToken(request.ApiKey);
                    var lines = ctx.tblPersonsPhones.Where(x => x.intIdPerson == intIDPerson && x.intIdOperator.Value == 4 && x.bitPhoneClube.Value).ToList();
                    MVNOAccess objMVNOAccess = new MVNOAccess();
                    if (lines != null && lines.Count > 0)
                    {
                        phoneResponse.Status = true;
                        phoneResponse.Data = new List<GetPhoneDetailRes>();
                        foreach (var line in lines)
                        {
                            var result = objMVNOAccess.GetContelLinesByPhoneManual(string.Concat(line.intDDD, line.intPhone));
                            if (result != null && result.detalhes != null)
                            {
                                string iccid = result.detalhes.iccid, activationcode = string.Empty;
                                DownloadAndSendESIMPdf(result.detalhes.linha, ref iccid, ref activationcode);

                                phoneResponse.Data.Add(new GetPhoneDetailRes()
                                {
                                    Id = result.detalhes.id,
                                    LineId = result.detalhes.id_linha,
                                    Line = result.detalhes.linha,
                                    NickName = result.detalhes.linha_apelido,
                                    ICCID = iccid,
                                    Owner = result.detalhes.titular,
                                    OwnerNickName = result.detalhes.titular_apelido,
                                    NameIdentifier = Convert.ToString(result.detalhes.nome_identificacao),
                                    Emoji = result.detalhes.emoji,
                                    ActivationDate = result.detalhes.data_ativacao,
                                    StartDate = result.detalhes.data_inicio_plano,
                                    EndDate = result.detalhes.data_fim_plano,
                                    RenewalDate = result.detalhes.data_renovacao,
                                    OperatorDeleteDate = result.detalhes.data_perda_numero_falta_recarga,
                                    Plan = result.detalhes.plano,
                                    PlanType = FacilUtil.GetStatusMapping(result.detalhes.tipo_linha),
                                    OwnerUniqueId = result.detalhes.documento_titular,
                                    LineStatus = FacilUtil.GetStatusMapping(result.detalhes.status),
                                    LineCancellationReason = result.detalhes.motivo_cancelamento_linha,
                                    LineCancellationDate = Convert.ToString(result.detalhes.data_cancelamento_linha),
                                    RecurrenceCard = FacilUtil.GetStatusMapping(result.detalhes.recorrencia_cartao),
                                    PortIn = result.detalhes.portin == "SIM" ? true : false,
                                    Blocked = result.detalhes.bloqueada == "NÃO" ? false : true,
                                    BlockedDate = result.detalhes.bloqueada != "NÃO" ? result.detalhes.bloqueada : "",
                                    eSim = result.detalhes.esim == "SIM" ? true : false,
                                    AutomaticRecharge = FacilUtil.GetStatusMapping(result.detalhes.recarga_automatica),
                                    AutomaticRechargePlan = result.detalhes.recarga_automatica_plano,
                                    PortabilityDonarLine = result.detalhes.portabilidade_linha_doadora,
                                    PortabilityStatus = result.detalhes.portabilidade_status,
                                    PortabilityRegistrationDate = result.detalhes.portabilidade_data_cadastro,
                                    PortabilityScheduleDate = result.detalhes.portabilidade_data_agendamento,
                                    PortabilityAcceptedDate = result.detalhes.portabilidade_data_aceite,
                                    PortabilityCompletedDate = result.detalhes.portabilidade_data_sucesso,
                                    ActivationCode = activationcode,
                                    ActivationPDFLink = result.detalhes.esim == "SIM" ? PdfUrl + result.detalhes.linha + "/" + result.detalhes.iccid : ""
                                });
                            }
                        }
                    }
                    else
                    {
                        phoneResponse.Status = true;
                        phoneResponse.Info = "No lines found";
                    }
                }
            }
            catch (Exception ex)
            {
                phoneResponse.Status = false;
                phoneResponse.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:GetAllPhoneDetail:" + ex.ToString());
            }
            return phoneResponse;
        }

        public FacilLineBalanceResponse GetLineBalance(FacilGenericRequest request)
        {
            FacilLineBalanceResponse phoneResponse = new FacilLineBalanceResponse();
            int userId = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    userId = GetUserIdFromToken(request.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == request.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        var result = new MVNOAccess().GetSaldo(request.Phone.PhoneNumber);
                        if (result != null && result.data != null)
                        {
                            phoneResponse.Data = new FacilLineBalanceRes();
                            phoneResponse.Data.Data = result.data.restante_dados;
                            phoneResponse.Data.Minutes = result.data.restante_minutos;
                            phoneResponse.Data.SMS = result.data.restante_sms;
                        }
                        else
                        {
                            phoneResponse.Status = false;
                            phoneResponse.Error = InternalError;
                        }
                    }
                    else
                    {
                        phoneResponse.Status = false;
                        phoneResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", request.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                phoneResponse.Status = false;
                phoneResponse.Error = InternalError;
                LogHelper.LogMessage(userId, "FacilAccess:GetLineBalance:" + ex.ToString());
            }
            return phoneResponse;
        }

        public FacilTopupHistoryResponse GetTopupHistory(FacilGenericRequest request)
        {
            FacilTopupHistoryResponse phoneResponse = new FacilTopupHistoryResponse();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var userId = GetUserIdFromToken(request.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == request.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        var result = new MVNOAccess().GetContelTopupHistory(request.Phone.PhoneNumber, request.Enviroment.ToString());
                        if (result != null && result.historico != null && result.historico.Count > 0)
                        {
                            phoneResponse.History = new List<FacilTopupHistoryRes>();
                            foreach (var plan in result.historico)
                            {
                                var hist = new FacilTopupHistoryRes()
                                {
                                    Plan = plan.plano,
                                    RechargeDate = plan.data_recarga
                                };
                                phoneResponse.History.Add(hist);
                                phoneResponse.Status = true;
                            }
                        }
                        else
                        {
                            phoneResponse.Status = false;
                            phoneResponse.Error = InternalError;
                        }
                    }
                    else
                    {
                        phoneResponse.Status = false;
                        phoneResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", request.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                phoneResponse.Status = false;
                phoneResponse.Error = InternalError;
                LogHelper.LogMessage(0, "FacilAccess:GetTopupHistory:" + ex.ToString());
            }
            return phoneResponse;
        }

        public FacilBalanceResponse GetBalance(string token)
        {
            FacilBalanceResponse balanceResponse = new FacilBalanceResponse();
            int intIDPerson = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = FacilAccess.GetUserIdFromToken(token);

                    var balance = ctx.tblInternationalUserBalance.Where(x => x.intIdPerson == intIDPerson).FirstOrDefault();

                    if (balance != null)
                    {
                        balanceResponse.Balance = Convert.ToDecimal(balance.intAmountBalance, CultureInfo.InvariantCulture);
                        balanceResponse.Status = true;
                    }
                    else
                    {
                        balanceResponse.Status = false;
                        balanceResponse.Error = InternalError;
                    }
                }
            }
            catch (Exception ex)
            {
                balanceResponse.Status = false;
                balanceResponse.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:GetBalance:" + ex.ToString());
            }
            return balanceResponse;
        }

        public FacilDebitResponse GetUserDebits(string token)
        {
            int intIDPerson = 0;
            FacilDebitResponse debitResponse = new FacilDebitResponse();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = FacilAccess.GetUserIdFromToken(token);

                    var debits = ctx.tblInternationalUserPurchases.Where(x => x.intIdPerson == intIDPerson && !x.bitTest.Value).ToList();

                    if (debits != null && debits.Count > 0)
                    {
                        debitResponse.Status = true;
                        debitResponse.Data = new List<FacilDebitRes>();
                        foreach (var debit in debits)
                        {
                            debitResponse.Data.Add(new FacilDebitRes()
                            {
                                Amount = Convert.ToDecimal(debit.intAmountDeducted, CultureInfo.InvariantCulture),
                                DateDebited = debit.dteDeducted,
                                Phone = debit.txtPhone,
                                Plan = debit.txtPlan,
                                PurchaseType = debit.intPurchaseType == 1 ? "Activation" : "Topup"
                            });
                        }
                    }
                    else
                    {
                        debitResponse.Status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                debitResponse.Status = false;
                debitResponse.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:GetBalance:" + ex.ToString());
            }
            return debitResponse;
        }

        public FacilTopupResponse TopupPlan(FacilTopupRequest request)
        {
            FacilTopupResponse facilTopupResponse = new FacilTopupResponse() { Status = false };
            int intIDPerson = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = GetUserIdFromToken(request.ApiKey);
                    tblContelPlanMapping amount = new tblContelPlanMapping();
                    amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.TopupInfo.PlanId && x.intIdPerson == intIDPerson).FirstOrDefault();
                    if (amount is null)
                        amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.TopupInfo.PlanId && x.intIdPerson == 1).FirstOrDefault();
                    if (amount != null)
                    {
                        double saldo = 0;
                        if (IsContelSaldoAvailable(ref saldo))
                        {
                            if (IsUserSaldoAvailable(intIDPerson, request.TopupInfo.PlanId))
                            {
                                var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == request.TopupInfo.PhoneNumber && x.intIdPerson == intIDPerson && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                                if (phones != null)
                                {
                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:TopupPlan: Topping up plan fo Phone: {0}", request.TopupInfo.PhoneNumber));
                                    TopUpPlanRequest topupRequest = new TopUpPlanRequest()
                                    {
                                        metodo_pagamento = "SALDO",
                                        numeros = new List<Numero>()
                                    };
                                    var num = new Numero();
                                    num.id_plano = request.TopupInfo.PlanId;
                                    num.numero = request.TopupInfo.PhoneNumber;
                                    topupRequest.numeros.Add(num);

                                    var topupResponse = new MVNOAccess().TopupPlan(topupRequest);
                                    if (topupResponse != null && topupResponse.retorno)
                                    {
                                        if (topupResponse.recarga != null)
                                        {
                                            ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                            {
                                                intIdPerson = intIDPerson,
                                                intPurchaseType = 2,
                                                intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                dteDeducted = DateTime.Now,
                                                txtComments = "Deducted by FACIL for topup" + intIDPerson,
                                                txtPhone = request.TopupInfo.PhoneNumber,
                                                txtPlan = amount.txtPlanName,
                                                txtICCID = "",
                                                intContelPrice = Convert.ToDecimal(amount.txtContelPrice, CultureInfo.InvariantCulture) - Convert.ToDecimal("2.95", CultureInfo.InvariantCulture)
                                            });
                                            ctx.SaveChanges();

                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:TopupPlan: Added entry to tblInternationalUserPurchases for Topup");

                                            var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);
                                            if (balance != null)
                                            {
                                                balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                balance.dteUpdated = DateTime.Now;
                                                ctx.SaveChanges();

                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:TopupPlan: Current balance updated post topup in tblInternationalUserBalance");
                                            }

                                            facilTopupResponse.Status = true;
                                            facilTopupResponse.Data = new FacilTopupRes()
                                            {
                                                Plan = amount.txtPlanName,
                                                DateRecharged = topupResponse.recarga.data_cadastro
                                            };
                                        }
                                        else
                                        {
                                            facilTopupResponse.Status = true;
                                            facilTopupResponse.Data = new FacilTopupRes()
                                            {
                                                Plan = amount.txtPlanName,
                                                DateRecharged = topupResponse.recarga.data_cadastro
                                            };
                                        }
                                    }
                                    else
                                    {
                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:TopupPlan: Topup failed with reason: {0} for User: {1}", topupResponse.mensagem, intIDPerson));

                                        facilTopupResponse.Error = "Topup failed with reason:" + topupResponse.mensagem;
                                    }
                                }
                                else
                                {
                                    facilTopupResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", request.TopupInfo.PhoneNumber);
                                }
                            }
                            else
                            {
                                facilTopupResponse.Error = "You do not have enough balance available to topup this line.  Please contact +5521982008200 for more details.";
                            }
                        }
                        else
                        {
                            facilTopupResponse.Error = "We have encountered a temporary but and will try to resolve it in the next 30 minutes.  Please contact +5521982008200 if it is not resolved in 30 minutes.";
                        }
                    }
                    else
                    {
                        facilTopupResponse.Error = "Requested PlanId doesn't exist.  Please contact +5521982008200.";
                    }
                }
            }
            catch (Exception ex)
            {
                facilTopupResponse.Status = false;
                facilTopupResponse.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:TopupPlan:" + ex.ToString());
            }
            return facilTopupResponse;
        }

        public FacilActivateESIMResponse ActivateESim(FacilActivateESIMRequest request)
        {
            FacilActivateESIMResponse actRes = new FacilActivateESIMResponse() { Status = false };
            ResettedInfo resetted = null;
            MVNOAccess mVNOAccess = new MVNOAccess();
            int intIDPerson = 0;
            string idUnique = "";
            var watch = Stopwatch.StartNew();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = GetUserIdFromToken(request.ApiKey);
                    idUnique = GetuniqueId(intIDPerson);

                    var person1 = GetNameById(intIDPerson);
                    tblContelPlanMapping amount = new tblContelPlanMapping();
                    amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.ActivationInfo.LineInfo.PlanId && x.intIdPerson == intIDPerson).FirstOrDefault();
                    if (amount is null)
                        amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.ActivationInfo.LineInfo.PlanId && x.intIdPerson == 1).FirstOrDefault();

                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESim: Entry : New activation request from {0} for plan {1}", intIDPerson, amount.txtPlanName), idUnique);

                    if (amount != null)
                    {
                        double saldo = 0;
                        if (IsContelSaldoAvailable(ref saldo))
                        {
                            if (IsUserSaldoAvailable(intIDPerson, request.ActivationInfo.LineInfo.PlanId))
                            {
                                bool isTopupNeeded = false;
                                var resetInfo = IsResetICCIDAvailableInPool(intIDPerson, idUnique, request.ActivationInfo.LineInfo.PlanId, intIDPerson, out resetted, ref isTopupNeeded);

                                if (resetInfo && isTopupNeeded)
                                {
                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Getting info for Phone:{0}", resetted.ResettedPhone), idUnique);

                                    var phoneInfo = mVNOAccess.GetContelLinesByPhoneManual(resetted.ResettedPhone);
                                    if (phoneInfo != null && phoneInfo.retorno && phoneInfo.detalhes != null)
                                    {
                                        TopUpPlanRequest topupRequest = new TopUpPlanRequest()
                                        {
                                            metodo_pagamento = "SALDO",
                                            numeros = new List<Numero>()
                                        };
                                        var num = new Numero();
                                        num.id_plano = request.ActivationInfo.LineInfo.PlanId;
                                        num.numero = resetted.ResettedPhone;
                                        topupRequest.numeros.Add(num);

                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Doing topup for Phone:{0}", phoneInfo.detalhes.linha), idUnique);

                                        var topupResponse = mVNOAccess.TopupPlan(topupRequest);

                                        if (topupResponse != null && topupResponse.retorno)
                                        {
                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Topup done for Phone:{0}", phoneInfo.detalhes.linha), idUnique);

                                            if (topupResponse.recarga != null)
                                            {
                                                string iccid = phoneInfo.detalhes.iccid, activationcode = string.Empty;
                                                var qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, phoneInfo.detalhes.esim_pdf, phoneInfo.detalhes.iccid, ref iccid, ref activationcode);
                                                if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                                {
                                                    TimeSpan timeTaken = watch.Elapsed;
                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Topup done for {0} in {1} seconds, deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                    if (timeTaken.TotalSeconds <= 80)
                                                    {
                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Time taken before deducting amount for user: {0}: is {1}", intIDPerson, timeTaken.TotalSeconds), idUnique);


                                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == phoneInfo.detalhes.iccid);

                                                        //Update plans
                                                        if (tblphones != null && tblphones.Count() > 0)
                                                        {
                                                            foreach (var ph in tblphones)
                                                            {
                                                                ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                ph.bitAtivo = true;
                                                                ph.bitPhoneClube = true;
                                                                ph.intDDD = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(0, 2));
                                                                ph.intPhone = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(2));
                                                                ph.intIdPerson = intIDPerson;
                                                            }
                                                            ctx.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                            {
                                                                intDDD = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(0, 2)),
                                                                intPhone = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(2)),
                                                                intCountryCode = 55,
                                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                bitAtivo = true,
                                                                bitPhoneClube = true,
                                                                intIdOperator = 4,
                                                                intIdPerson = intIDPerson,
                                                                txtICCID = iccid,
                                                                bitEsim = true
                                                            });
                                                            ctx.SaveChanges();
                                                        }

                                                        var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == phoneInfo.detalhes.linha && x.txtICCID == iccid);
                                                        if (tblEsim != null)
                                                        {
                                                            tblEsim.txtActivationCode = activationcode;
                                                            tblEsim.txtActivationImage = qrcode;
                                                            tblEsim.txtActivationPdfUrl = phoneInfo.detalhes.esim_pdf;
                                                            tblEsim.txtICCID = iccid;
                                                            tblEsim.txtLinha = phoneInfo.detalhes.linha;
                                                            tblEsim.txtPlano = amount.txtPlanName;
                                                            tblEsim.dteInsert = DateTime.Now;
                                                        }
                                                        else
                                                        {
                                                            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                            {
                                                                txtActivationCode = activationcode,
                                                                txtActivationImage = qrcode,
                                                                txtActivationPdfUrl = phoneInfo.detalhes.esim_pdf,
                                                                txtICCID = iccid,
                                                                txtLinha = phoneInfo.detalhes.linha,
                                                                txtPlano = amount.txtContelPlanName,
                                                                dteInsert = DateTime.Now
                                                            });
                                                        }
                                                        ctx.SaveChanges();
                                                        var dataAdded = mVNOAccess.AddContelLineManual(phoneInfo.detalhes.linha);

                                                        var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);

                                                        decimal remainingBal = 0;
                                                        if (balance != null)
                                                        {
                                                            remainingBal = balance.intAmountBalance;
                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                            balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                            balance.dteUpdated = DateTime.Now;
                                                            remainingBal = balance.intAmountBalance;
                                                            ctx.SaveChanges();

                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, iccid, remainingBal), idUnique);
                                                        }

                                                        ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                        {
                                                            intIdPerson = intIDPerson,
                                                            intPurchaseType = 2,
                                                            intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                            dteDeducted = DateTime.Now,
                                                            txtComments = string.Format("Deducted by FACIL for Top-up for User: {0}, ICCID:{1}", intIDPerson, iccid),
                                                            txtPhone = phoneInfo.detalhes.linha,
                                                            txtPlan = amount.txtPlanName,
                                                            bitTest = false,
                                                            bitRefund = false,
                                                            txtICCID = iccid
                                                        });
                                                        ctx.SaveChanges();
                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Added entry to tblInternationalUserPurchases", idUnique);

                                                        actRes.Info = "Line Activated successfully";
                                                        actRes.Status = true;

                                                        actRes.ActivatedInfo = new ActivatedInfo()
                                                        {
                                                            ActivatedNumber = phoneInfo.detalhes.linha,
                                                            ActivatedPlan = amount.txtPlanName,
                                                            ActivationDate = phoneInfo.detalhes.data_ativacao,
                                                            ICCID = iccid,
                                                            ActivationPDFLink = PdfUrl + phoneInfo.detalhes.linha + "/" + iccid,
                                                            ActivationCode = activationcode
                                                        };

                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Response to Caller : {0}, {1}, {2}, {3}, {4}",
                                                            phoneInfo.detalhes.linha, amount.txtPlanName, phoneInfo.detalhes.data_ativacao, iccid, activationcode));

                                                        try
                                                        {

                                                            //string strContelPrice = string.Empty;
                                                            //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                            DateTime today = DateTime.Now;

                                                            var todayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));
                                                            var monthCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                && x.dteDeducted <= today);

                                                            var totaltodayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                               .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));

                                                            string msgAdm = string.Format("*Topup: {0}*\n\n" +
                                       "Line: *{1}* \n" +
                                       "Plan: {2} \n" +
                                       "Dados: {3} \n\n" +
                                       "Today: *{4}* ${5}\n" +
                                       "Month: *{6}* ${7}\n" +
                                       "Balance: *${8}*\n\n" +
                                       "Total Today: *{9}* *${10}*"
                                       , person1.txtName, phoneInfo.detalhes.linha, amount.txtContelPlanName, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted), monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted));

                                                            new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Timeout occured post topup", idUnique);
                                                        //var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == iccid && x.txtPhoneNumber == phoneInfo.detalhes.linha);
                                                        //if (poolData is null)
                                                        //{
                                                        //    var dataPool = new tblInternationActivationPool()
                                                        //    {
                                                        //        bitReadyForReActivation = true,
                                                        //        bitFailedPostActivation = false,
                                                        //        intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                        //        dteActivation = DateTime.Now,
                                                        //        intIdPerson = intIDPerson,
                                                        //        txtICCID = iccid,
                                                        //        intIdPersonReset = 1,
                                                        //        dteReset = DateTime.Now,
                                                        //        txtPhoneNumber = phoneInfo.detalhes.linha,
                                                        //        txtResetStatus = "Pending",
                                                        //        txtStatus = "Resell Pool + Topup"
                                                        //    };
                                                        //    ctx.tblInternationActivationPool.Add(dataPool);
                                                        //    ctx.SaveChanges();

                                                        //    int poolId = dataPool.intId;

                                                        //    ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                        //    {
                                                        //        intActivationPoolId = poolId,
                                                        //        txtICCID = iccid,
                                                        //        txtPhone = activatedPhone,
                                                        //        txtStatus = "Resell Pool + Blocked",
                                                        //        dteAction = DateTime.Now,
                                                        //        txtDoneBy = "System"
                                                        //    });
                                                        //    ctx.SaveChanges();

                                                        //    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                        //    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Blocking line:" + activatedPhone, idUnique);

                                                        //    BlockLine blockLine = new BlockLine()
                                                        //    {
                                                        //        numero = activatedPhone,
                                                        //        motivo = "BLOQUEIO DE IMEI",
                                                        //        observacoes = ""
                                                        //    };
                                                        //    mVNOAccess.BlockLine(blockLine);

                                                        //}

                                                        //LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Line activated by {0} successfully but timeout, hence not deducting money adding to pool", person1.txtName), idUnique);
                                                        //string msgAdm = string.Format("*Failed Activation due to timeout: {0}*\n\n" +
                                                        //        "Line: *{1}* \n" +
                                                        //        "Plan: {2} \n\n" +

                                                        //        "*Adding to Pool*"
                                                        //        , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome);

                                                        //new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                    }
                                                }
                                                else
                                                {
                                                    actRes.Status = false;
                                                    actRes.Error = "Activation failed with reason: Internal Error : T101";

                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Topup done but failed to get activation code, Activation failed with reason: Internal Error : T101", idUnique);
                                                }
                                            }
                                            else
                                            {
                                                actRes.Status = false;
                                                actRes.Error = "Activation failed with reason: Internal Error : T101";
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Activation failed with reason: {0} for User: {1}, Activation failed with reason: Internal Error : T102", topupResponse.mensagem, intIDPerson), idUnique);

                                                try
                                                {
                                                    string msgAdm = string.Format("*Topup error: {0}* \nError code: T102 \nContel Error on post topup but failed to retrieve recharge date: {1}", person1.txtName, topupResponse.mensagem);
                                                    new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                }
                                                catch { }
                                            }
                                        }
                                        else
                                        {
                                            actRes.Status = false;
                                            actRes.Error = "Activation failed with reason: Internal Error : T102";

                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Activation failed with reason: {0} for User: {1}, Activation failed with reason: Internal Error : T102", topupResponse.mensagem, intIDPerson), idUnique);

                                            try
                                            {
                                                string msgAdm = string.Format("*Topup error: {0}* \nError code: T102 \nContel Error Topup failed with error message: {1}", person1.txtName, topupResponse.mensagem);
                                                new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                            }
                                            catch { }
                                        }
                                    }
                                    else
                                    {
                                        actRes.Status = false;
                                        actRes.Error = "Activation failed with reason: Internal Error : T103";
                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Line not found in contel: {0}, Activation failed with reason: Internal Error : T103", resetted.ResettedPhone), idUnique);
                                        try
                                        {
                                            string msgAdm = string.Format("*Topup error: {0}* \nError code: T103 \nContel Error on Getting Line infor before topup: {1}", person1.txtName, phoneInfo.mensagem);
                                            new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                        }
                                        catch { }
                                    }
                                }
                                else if (resetInfo && !isTopupNeeded)
                                {
                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Failed activation esim available with ICCID:{0} and Phone:{1} hence doing re-sale", resetted.ResettedICCID, resetted.ResettedPhone), idUnique);

                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Unblocking Phone:{0} hence doing re-sale", resetted.ResettedPhone), idUnique);

                                    var unblock = new UnBlockLine() { numero = resetted.ResettedPhone };
                                    mVNOAccess.UnBlockLine(unblock);

                                    var iccidPhoneData = ValidateInLoop(intIDPerson, idUnique, resetted.ResettedICCID, request.Enviroment.ToString());

                                    if (iccidPhoneData != null && iccidPhoneData.retorno && iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                    {
                                        var activatedPhone = iccidPhoneData.info.numero_ativado;

                                        actRes.Status = true;

                                        string iccid = resetted.ResettedICCID, activationcode = string.Empty;
                                        var qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, iccidPhoneData.info.esim, activatedPhone, ref iccid, ref activationcode);
                                        if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                        {
                                            var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                            //Update plans
                                            if (tblphones != null && tblphones.Count() > 0)
                                            {
                                                foreach (var ph in tblphones)
                                                {
                                                    ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                    ph.bitAtivo = true;
                                                    ph.bitPhoneClube = true;
                                                    ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                    ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                    ph.intIdPerson = intIDPerson;
                                                }
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                {
                                                    intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                    intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                    intCountryCode = 55,
                                                    intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                    bitAtivo = true,
                                                    bitPhoneClube = true,
                                                    intIdOperator = 4,
                                                    intIdPerson = intIDPerson,
                                                    txtICCID = iccidPhoneData.info.iccid,
                                                    bitEsim = true
                                                });
                                                ctx.SaveChanges();
                                            }

                                            var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == activatedPhone && x.txtICCID == iccid);
                                            if (tblEsim != null)
                                            {
                                                tblEsim.txtActivationCode = activationcode;
                                                tblEsim.txtActivationDate = iccidPhoneData.info.data_ativacao;
                                                tblEsim.txtActivationImage = qrcode;
                                                tblEsim.txtActivationPdfUrl = iccidPhoneData.info.esim;
                                                tblEsim.txtICCID = iccid;
                                                tblEsim.txtLinha = activatedPhone;
                                                tblEsim.txtPlano = iccidPhoneData.info.plano_nome;
                                                tblEsim.dteInsert = DateTime.Now;
                                            }
                                            else
                                            {
                                                ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                {
                                                    txtActivationCode = activationcode,
                                                    txtActivationDate = iccidPhoneData.info.data_ativacao,
                                                    txtActivationImage = qrcode,
                                                    txtActivationPdfUrl = iccidPhoneData.info.esim,
                                                    txtICCID = iccid,
                                                    txtLinha = activatedPhone,
                                                    txtPlano = iccidPhoneData.info.plano_nome,
                                                    dteInsert = DateTime.Now
                                                });
                                            }
                                            ctx.SaveChanges();
                                            var dataAdded = mVNOAccess.AddContelLineManual(activatedPhone);
                                            decimal remainingBal = 0;

                                            var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);
                                            if (balance != null)
                                            {
                                                remainingBal = balance.intAmountBalance;
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                balance.dteUpdated = DateTime.Now;
                                                remainingBal = balance.intAmountBalance;
                                                ctx.SaveChanges();

                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, resetted.ResettedICCID, remainingBal), idUnique);
                                            }

                                            var tblEsimPurc = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.txtPhone == activatedPhone && !x.bitRefund.Value);
                                            if (tblEsimPurc is null)
                                            {
                                                ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                {
                                                    intIdPerson = intIDPerson,
                                                    intPurchaseType = 1,
                                                    intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                    dteDeducted = DateTime.Now,
                                                    txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, resetted.ResettedICCID),
                                                    txtPhone = activatedPhone,
                                                    txtPlan = amount.txtPlanName,
                                                    bitTest = false,
                                                    bitRefund = false,
                                                    txtICCID = iccid
                                                });
                                                ctx.SaveChanges();
                                            }

                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Added entry to tblInternationalUserPurchases", idUnique);

                                            actRes.Info = "Line Activated successfully";

                                            actRes.ActivatedInfo = new ActivatedInfo()
                                            {
                                                ActivatedNumber = activatedPhone,
                                                ActivatedPlan = iccidPhoneData.info.plano_nome,
                                                ActivationDate = iccidPhoneData.info.data_ativacao,
                                                ICCID = iccidPhoneData.info.iccid,
                                                ActivationPDFLink = PdfUrl + activatedPhone + "/" + iccid,
                                                ActivationCode = activationcode
                                            };

                                            try
                                            {
                                                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                DateTime today = DateTime.Now;

                                                var todayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                    .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));
                                                var monthCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                    .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                    && x.dteDeducted <= today);
                                                var totaltodayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                              .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));


                                                DateTime now = DateTime.Now;
                                                DateTime nowto23hrs = now.AddHours(-23);
                                                var poolRemainingCount = ctx.tblInternationActivationPool.Where(x => x.bitReadyForReActivation == true && x.intIdPerson == intIDPerson && (x.dteReset > nowto23hrs && x.dteReset <= now)).Count();

                                                string msgAdm = string.Format("*Activation from Pool: {0}*\n\n" +
                                                    "Line: *{1}* \n" +
                                                    "Plan: {2} \n" +
                                                    "Dados: {3} \n\n" +
                                                    "Today: *{4}* ${5}\n" +
                                                    "Month: *{6}* ${7}\n" +
                                                    "Balance: *${8}*\n\n" +
                                                    "Total Today: *{10}* *${11}*\n" +
                                                    "23 hour Pool: {9}"
                                                    , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted),
                                                    monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, poolRemainingCount, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted));

                                                new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                            }
                                            catch (Exception ex)
                                            {
                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                            }
                                        }
                                        else
                                        {
                                            actRes.Status = false;
                                            actRes.Error = "Failed to get activation code post activation hence cancelling sale and no payment deducted";
                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Returing activation failure due to not get activation code after 15 retries", idUnique);

                                            try
                                            {
                                                string msgAdm = string.Format("*Attention:*\nActivation success but failed to get Activation Code even after 15 retries. Pushing below line detail to activation pool\nLine:{0}\nICCID:{1}", activatedPhone, resetted.ResettedICCID);
                                                new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == resetted.ResettedICCID && x.txtPhoneNumber == activatedPhone);
                                                if (poolData is null)
                                                {
                                                    var dataPool = new tblInternationActivationPool()
                                                    {
                                                        bitReadyForReActivation = true,
                                                        bitFailedPostActivation = true,
                                                        intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                        dteActivation = DateTime.Now,
                                                        intIdPerson = intIDPerson,
                                                        txtICCID = iccid,
                                                        intIdPersonReset = 1,
                                                        dteReset = DateTime.Now,
                                                        txtPhoneNumber = activatedPhone,
                                                        txtResetStatus = "Pending",
                                                        txtStatus = "Resell Pool + Blocked"
                                                    };
                                                    ctx.tblInternationActivationPool.Add(dataPool);
                                                    ctx.SaveChanges();

                                                    int poolId = dataPool.intId;

                                                    ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                    {
                                                        intActivationPoolId = poolId,
                                                        txtICCID = iccid,
                                                        txtPhone = activatedPhone,
                                                        txtStatus = "Resell Pool + Blocked",
                                                        dteAction = DateTime.Now,
                                                        txtDoneBy = "System"
                                                    });
                                                    ctx.SaveChanges();

                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Blocking line:" + activatedPhone, idUnique);

                                                    BlockLine blockLine = new BlockLine()
                                                    {
                                                        numero = activatedPhone,
                                                        motivo = "BLOQUEIO DE IMEI",
                                                        observacoes = ""
                                                    };
                                                    mVNOAccess.BlockLine(blockLine);
                                                }
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                    else
                                    {
                                        actRes.Status = true;

                                        ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                        {
                                            intIdPerson = intIDPerson,
                                            intPurchaseType = 1,
                                            intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                            dteDeducted = DateTime.Now,
                                            txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, resetted.ResettedICCID),
                                            txtPhone = "111111111",
                                            txtPlan = amount.txtPlanName,
                                            bitTest = false,
                                            bitRefund = false,
                                            txtICCID = resetted.ResettedICCID
                                        });
                                        ctx.SaveChanges();

                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                        //Update plans
                                        if (tblphones != null && tblphones.Count() > 0)
                                        {
                                            foreach (var ph in tblphones)
                                            {
                                                ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                ph.bitAtivo = true;
                                                ph.bitPhoneClube = true;
                                                ph.intIdPerson = intIDPerson;
                                            }
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                            {
                                                intCountryCode = 55,
                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                bitAtivo = true,
                                                bitPhoneClube = true,
                                                intIdOperator = 4,
                                                intIdPerson = intIDPerson,
                                                bitEsim = true
                                            });
                                            ctx.SaveChanges();
                                        }
                                        actRes.Info = "Line Activated successfully but failed to get activated line details, please call validate iccid api to get line details";
                                        actRes.ActivatedInfo = new ActivatedInfo()
                                        {
                                            ActivatedPlan = iccidPhoneData.info.plano_nome,
                                            ActivationDate = iccidPhoneData.info.data_ativacao,
                                            ICCID = iccidPhoneData.info.iccid
                                        };

                                        string msgAdmin = string.Format("User Id: {0} has activated new line but failed to get line detail for iccid: {1}", intIDPerson, iccidPhoneData.info.iccid);
                                        new WhatsAppAccess().SendMessageInfoToAdmin(msgAdmin);

                                        try
                                        {
                                            //string strContelPrice = string.Empty;
                                            //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                            DateTime today = DateTime.Now;

                                            var todayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).Count();
                                            var monthCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                && x.dteDeducted <= today).Count();

                                            string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                "ICCID: *{1}* \n" +
                                                "Plan: {2} \n\n" +
                                                "Today: *{3}*\n" +
                                                "Month: *{4}*"
                                                , person1.txtName, iccidPhoneData.info.iccid, iccidPhoneData.info.plano_nome, todayCount, monthCount);

                                            new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                        }
                                    }
                                }
                                else
                                {
                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: There are no resetted phonenumber hence proceed with eSIM activation", idUnique);

                                    var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == intIDPerson);
                                    if (person != null)
                                    {
                                        ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                                        activatePlanRequest.metodo_pagamento = "SALDO";
                                        activatePlanRequest.nome = string.IsNullOrEmpty(request.ActivationInfo.CustomerInfo.Name) ? person.txtName : request.ActivationInfo.CustomerInfo.Name;

                                        //if (person.txtDocumentNumber.Length == 11)
                                        //    activatePlanRequest.cpf = person.txtDocumentNumber;
                                        //else
                                        activatePlanRequest.cnpj = "08453543000176";

                                        activatePlanRequest.email = string.IsNullOrEmpty(request.ActivationInfo.CustomerInfo.Email) ? "suporte@foneclube.com.br" : request.ActivationInfo.CustomerInfo.Email;
                                        activatePlanRequest.telefone = "21981908190";
                                        activatePlanRequest.data_nascimento = "1900-01-01";
                                        activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();

                                        activatePlanRequest.endereco.rua = "Avenida das americas";
                                        activatePlanRequest.endereco.numero = "3434";
                                        activatePlanRequest.endereco.complemento = "305 bloco 2";
                                        activatePlanRequest.endereco.bairro = "Barra da Tijuca";
                                        activatePlanRequest.endereco.cep = "22640102";
                                        activatePlanRequest.endereco.municipio = "Rio de Janeiro";
                                        activatePlanRequest.endereco.uf = "RJ";


                                        activatePlanRequest.chips = new List<Chip>();
                                        var chip = new Chip()
                                        {
                                            ddd = request.ActivationInfo.LineInfo.DDD == 0 ? 21 : request.ActivationInfo.LineInfo.DDD,
                                            id_plano = request.ActivationInfo.LineInfo.PlanId,
                                            esim = "SIM"
                                        };

                                        activatePlanRequest.chips.Add(chip);

                                        if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                                        {
                                            var response = mVNOAccess.ActivatePlan(activatePlanRequest, request.Enviroment.ToString());

                                            if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                                            {
                                                foreach (var pho in response.info.chips)
                                                {
                                                    var isSameIccidExists = ctx.tblInternationalUserPurchases.Where(x => x.txtICCID == pho.iccid).Count();

                                                    if (isSameIccidExists == 0)
                                                    {
                                                        var iccidPhoneData = ValidateInLoop(intIDPerson, idUnique, pho.iccid, request.Enviroment.ToString());

                                                        if (iccidPhoneData != null && iccidPhoneData.retorno && iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                                        {
                                                            var activatedPhone = iccidPhoneData.info.numero_ativado;

                                                            actRes.Status = true;

                                                            string iccid = pho.iccid, activationcode = string.Empty;
                                                            var qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, iccidPhoneData.info.esim, activatedPhone, ref iccid, ref activationcode);
                                                            if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                                            {
                                                                TimeSpan timeTaken = watch.Elapsed;
                                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Line activated for {0} in {1} seconds, deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                                if (timeTaken.TotalSeconds <= 75)
                                                                {
                                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Time taken before deducting amount for user: {0}: is {1}", intIDPerson, timeTaken.TotalSeconds), idUnique);


                                                                    var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                                                    //Update plans
                                                                    if (tblphones != null && tblphones.Count() > 0)
                                                                    {
                                                                        foreach (var ph in tblphones)
                                                                        {
                                                                            ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                            ph.bitAtivo = true;
                                                                            ph.bitPhoneClube = true;
                                                                            ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                                            ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                                            ph.intIdPerson = person.intIdPerson;
                                                                        }
                                                                        ctx.SaveChanges();
                                                                    }
                                                                    else
                                                                    {
                                                                        ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                                        {
                                                                            intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                                            intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                                            intCountryCode = 55,
                                                                            intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                            bitAtivo = true,
                                                                            bitPhoneClube = true,
                                                                            intIdOperator = 4,
                                                                            intIdPerson = person.intIdPerson,
                                                                            txtICCID = iccidPhoneData.info.iccid,
                                                                            bitEsim = true
                                                                        });
                                                                        ctx.SaveChanges();
                                                                    }

                                                                    var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == activatedPhone && x.txtICCID == iccid);
                                                                    if (tblEsim != null)
                                                                    {
                                                                        tblEsim.txtActivationCode = activationcode;
                                                                        tblEsim.txtActivationDate = response.info.data_cadastro;
                                                                        tblEsim.txtActivationImage = qrcode;
                                                                        tblEsim.txtActivationPdfUrl = response.link_esim;
                                                                        tblEsim.txtICCID = iccid;
                                                                        tblEsim.txtLinha = activatedPhone;
                                                                        tblEsim.txtPlano = iccidPhoneData.info.plano_nome;
                                                                        tblEsim.dteInsert = DateTime.Now;
                                                                    }
                                                                    else
                                                                    {
                                                                        ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                                        {
                                                                            txtActivationCode = activationcode,
                                                                            txtActivationDate = response.info.data_cadastro,
                                                                            txtActivationImage = qrcode,
                                                                            txtActivationPdfUrl = response.link_esim,
                                                                            txtICCID = iccid,
                                                                            txtLinha = activatedPhone,
                                                                            txtPlano = iccidPhoneData.info.plano_nome,
                                                                            dteInsert = DateTime.Now
                                                                        });
                                                                    }
                                                                    ctx.SaveChanges();
                                                                    var dataAdded = mVNOAccess.AddContelLineManual(activatedPhone);

                                                                    var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);

                                                                    decimal remainingBal = 0;
                                                                    if (balance != null)
                                                                    {
                                                                        remainingBal = balance.intAmountBalance;
                                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                                        balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                                        balance.dteUpdated = DateTime.Now;
                                                                        remainingBal = balance.intAmountBalance;
                                                                        ctx.SaveChanges();

                                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, pho.iccid, remainingBal), idUnique);
                                                                    }

                                                                    ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                                    {
                                                                        intIdPerson = intIDPerson,
                                                                        intPurchaseType = 1,
                                                                        intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                                        dteDeducted = DateTime.Now,
                                                                        txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, pho.iccid),
                                                                        txtPhone = activatedPhone,
                                                                        txtPlan = amount.txtPlanName,
                                                                        bitTest = false,
                                                                        bitRefund = false,
                                                                        txtICCID = iccidPhoneData.info.iccid
                                                                    });
                                                                    ctx.SaveChanges();
                                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Added entry to tblInternationalUserPurchases", idUnique);

                                                                    actRes.Info = "Line Activated successfully";

                                                                    actRes.ActivatedInfo = new ActivatedInfo()
                                                                    {
                                                                        ActivatedNumber = activatedPhone,
                                                                        ActivatedPlan = iccidPhoneData.info.plano_nome,
                                                                        ActivationDate = iccidPhoneData.info.data_ativacao,
                                                                        ICCID = iccidPhoneData.info.iccid,
                                                                        ActivationPDFLink = PdfUrl + activatedPhone + "/" + iccid,
                                                                        ActivationCode = activationcode
                                                                    };

                                                                    try
                                                                    {

                                                                        //string strContelPrice = string.Empty;
                                                                        //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                                        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                        DateTime today = DateTime.Now;

                                                                        var todayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                            .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));
                                                                        var monthCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                            .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                            && x.dteDeducted <= today);
                                                                        var totaltodayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                              .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now));

                                                                        string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                   "Line: *{1}* \n" +
                                                   "Plan: {2} \n" +
                                                   "Dados: {3} \n\n" +
                                                   "Today: *{4}* ${5}\n" +
                                                   "Month: *{6}* ${7}\n" +
                                                   "Balance: *${8}*\n\n" +
                                                   "Total Today: *{9}* *${10}*\n\n"
                                                   , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted), monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted));

                                                                        new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == pho.iccid && x.txtPhoneNumber == activatedPhone);
                                                                    if (poolData is null)
                                                                    {
                                                                        var dataPool = new tblInternationActivationPool()
                                                                        {
                                                                            bitReadyForReActivation = true,
                                                                            bitFailedPostActivation = true,
                                                                            intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                            dteActivation = DateTime.Now,
                                                                            intIdPerson = intIDPerson,
                                                                            txtICCID = iccid,
                                                                            intIdPersonReset = 1,
                                                                            dteReset = DateTime.Now,
                                                                            txtPhoneNumber = activatedPhone,
                                                                            txtResetStatus = "Pending",
                                                                            txtStatus = "Resell Pool + Blocked"
                                                                        };
                                                                        ctx.tblInternationActivationPool.Add(dataPool);
                                                                        ctx.SaveChanges();

                                                                        int poolId = dataPool.intId;

                                                                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                                        {
                                                                            intActivationPoolId = poolId,
                                                                            txtICCID = iccid,
                                                                            txtPhone = activatedPhone,
                                                                            txtStatus = "Resell Pool + Blocked",
                                                                            dteAction = DateTime.Now,
                                                                            txtDoneBy = "System"
                                                                        });
                                                                        ctx.SaveChanges();

                                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Blocking line:" + activatedPhone, idUnique);

                                                                        BlockLine blockLine = new BlockLine()
                                                                        {
                                                                            numero = activatedPhone,
                                                                            motivo = "BLOQUEIO DE IMEI",
                                                                            observacoes = ""
                                                                        };
                                                                        mVNOAccess.BlockLine(blockLine);

                                                                    }

                                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Line activated by {0} successfully but timeout, hence not deducting money adding to pool", person1.txtName), idUnique);
                                                                    string msgAdm = string.Format("*Failed Activation due to timeout: {0}*\n\n" +
                                                                            "Line: *{1}* \n" +
                                                                            "Plan: {2} \n\n" +

                                                                            "*Adding to Pool*"
                                                                            , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome);

                                                                    new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                actRes.Status = false;
                                                                actRes.Error = "Failed to get activation code post activation hence cancelling sale and no payment deducted";
                                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Returing activation failure due to not get activation code after 15 retries", idUnique);

                                                                try
                                                                {
                                                                    string msgAdm = string.Format("*Attention:*\nActivation success but failed to get Activation Code even after 15 retries. Pushing below line detail to activation pool\nLine:{0}\nICCID:{1}", activatedPhone, pho.iccid);
                                                                    new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                                    var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == pho.iccid && x.txtPhoneNumber == activatedPhone);
                                                                    if (poolData is null)
                                                                    {
                                                                        ctx.tblInternationActivationPool.Add(new tblInternationActivationPool()
                                                                        {
                                                                            bitReadyForReActivation = true,
                                                                            bitFailedPostActivation = true,
                                                                            intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                            dteActivation = DateTime.Now,
                                                                            intIdPerson = intIDPerson,
                                                                            txtICCID = iccid,
                                                                            intIdPersonReset = 1,
                                                                            dteReset = DateTime.Now,
                                                                            txtPhoneNumber = activatedPhone,
                                                                            txtResetICCID = iccid,
                                                                            txtResetStatus = "Pending",
                                                                            txtStatus = "Resell Pool + Blocked"
                                                                        });
                                                                        ctx.SaveChanges();

                                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                                    }
                                                                }
                                                                catch (Exception ex) { }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            actRes.Status = true;

                                                            ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                            {
                                                                intIdPerson = intIDPerson,
                                                                intPurchaseType = 1,
                                                                intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                                dteDeducted = DateTime.Now,
                                                                txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, pho.iccid),
                                                                txtPhone = "111111111",
                                                                txtPlan = amount.txtPlanName,
                                                                bitTest = false,
                                                                bitRefund = false,
                                                                txtICCID = iccidPhoneData.info.iccid
                                                            });
                                                            ctx.SaveChanges();

                                                            var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                                            //Update plans
                                                            if (tblphones != null && tblphones.Count() > 0)
                                                            {
                                                                foreach (var ph in tblphones)
                                                                {
                                                                    ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                    ph.bitAtivo = true;
                                                                    ph.bitPhoneClube = true;
                                                                    ph.intIdPerson = intIDPerson;
                                                                }
                                                                ctx.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                                {
                                                                    intCountryCode = 55,
                                                                    intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                    bitAtivo = true,
                                                                    bitPhoneClube = true,
                                                                    intIdOperator = 4,
                                                                    intIdPerson = intIDPerson,
                                                                    bitEsim = true
                                                                });
                                                                ctx.SaveChanges();
                                                            }
                                                            actRes.Info = "Line Activated successfully but failed to get activated line details, please call validate iccid api to get line details";
                                                            actRes.ActivatedInfo = new ActivatedInfo()
                                                            {
                                                                ActivatedPlan = iccidPhoneData.info.plano_nome,
                                                                ActivationDate = iccidPhoneData.info.data_ativacao,
                                                                ICCID = iccidPhoneData.info.iccid
                                                            };

                                                            string msgAdmin = string.Format("User Id: {0} has activated new line but failed to get line detail for iccid: {1}", intIDPerson, iccidPhoneData.info.iccid);
                                                            new WhatsAppAccess().SendMessageInfoToAdmin(msgAdmin);

                                                            try
                                                            {
                                                                //string strContelPrice = string.Empty;
                                                                //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                DateTime today = DateTime.Now;

                                                                var todayCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                    .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).Count();
                                                                var monthCount = ctx.tblInternationalUserPurchases.OrderByDescending(x => x.dteDeducted)
                                                                                    .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                    && x.dteDeducted <= today).Count();

                                                                string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                                    "ICCID: *{1}* \n" +
                                                                    "Plan: {2} \n\n" +
                                                                    "Today: *{3}*\n" +
                                                                    "Month: *{4}*"
                                                                    , person1.txtName, iccidPhoneData.info.iccid, iccidPhoneData.info.plano_nome, todayCount, monthCount);

                                                                new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        actRes.Status = false;
                                                        actRes.Error = "Contel issue - duplicate ICCID found, no amount deducted";

                                                        var dataPool = new tblInternationActivationPool()
                                                        {
                                                            intIdPerson = intIDPerson,
                                                            bitFailedPostActivation = false,
                                                            bitReadyForReActivation = false,
                                                            intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                            txtICCID = pho.iccid,
                                                            txtPhoneNumber = "",
                                                            txtResetStatus = "Not Applicable",
                                                            txtStatus = "Request Refund from Contel - Duplicate ICCID",
                                                            dteActivation = DateTime.Now
                                                        };
                                                        ctx.tblInternationActivationPool.Add(dataPool);
                                                        ctx.SaveChanges();

                                                        int poolId = dataPool.intId;

                                                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                        {
                                                            intActivationPoolId = poolId,
                                                            txtICCID = pho.iccid,
                                                            txtPhone = "",
                                                            txtStatus = "Request Refund from Contel - Duplicate ICCID",
                                                            dteAction = DateTime.Now,
                                                            txtDoneBy = "System"
                                                        });
                                                        ctx.SaveChanges();
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESim: Activation failed with reason: {0} for User: {1}", response.mensagem, intIDPerson), idUnique);

                                                if (!response.retorno && response.mensagem == "CPF n\u00e3o \u00e9 v\u00e1lido na base da receita."
                                                    || response.mensagem == "CPF não é válido na base da receita."
                                                    || response.mensagem == "CNPJ n\u00e3o \u00e9 v\u00e1lido na base da receita."
                                                    || response.mensagem == "CNPJ não é válido na base da receita.")
                                                    actRes.Error = "Activation failed with reason: Not a valid CPF at Internal Revenue Service database.";
                                                else
                                                    actRes.Error = "Activation failed with reason:" + response.mensagem;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        actRes.Error = "Client details not available, Please check with Facil.tel";
                                    }
                                }
                            }
                            else
                            {
                                actRes.Error = "You do not have enough balance available to activate a new line.  Please contact +5521982008200 for more details.";
                            }
                        }
                        else
                        {
                            actRes.Error = "We have encountered a temporary but and will try to resolve it in the next 30 minutes.  Please contact +5521982008200 if it is not resolved in 30 minutes.";
                        }
                    }
                    else
                    {
                        actRes.Error = "Requested PlanId doesn't exist.  Please contact +5521982008200.";
                    }

                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESim: Exit : New activation from {0} for plan {1}", intIDPerson, amount.txtPlanName), idUnique);

                    TimeSpan timeTaken1 = watch.Elapsed;
                    watch.Stop();
                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESim: Total time taken in seconds :{0} for {1}", timeTaken1.TotalSeconds, intIDPerson), idUnique);
                }
            }
            catch (Exception ex)
            {
                actRes.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESim: Error" + ex.ToString(), idUnique);
                TimeSpan timeTaken1 = watch.Elapsed;
                watch.Stop();
                LogHelper.LogMessage(intIDPerson, string.Format("ActivateESim: Total time taken in seconds : ", intIDPerson, timeTaken1.TotalSeconds), idUnique);
            }
            return actRes;
        }

        public FacilActivateESIMResponse ActivateESimNew(FacilActivateESIMRequest request)
        {
            FacilActivateESIMResponse actRes = new FacilActivateESIMResponse() { Status = false };
            ResettedInfo resetted = null;
            MVNOAccess mVNOAccess = new MVNOAccess();
            int intIDPerson = 0;
            string idUnique = "";
            var watch = Stopwatch.StartNew();
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = GetUserIdFromToken(request.ApiKey);

                    idUnique = GetuniqueId(intIDPerson);

                    var person1 = GetNameById(intIDPerson);
                    tblContelPlanMapping amount = new tblContelPlanMapping();
                    amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.ActivationInfo.LineInfo.PlanId && x.intIdPerson == intIDPerson).FirstOrDefault();
                    if (amount is null)
                        amount = ctx.tblContelPlanMapping.Where(x => x.intIdPlan == request.ActivationInfo.LineInfo.PlanId && x.intIdPerson == 1).FirstOrDefault();

                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESimNew: Entry : New activation request from {0} for plan {1}", intIDPerson, amount.txtPlanName), idUnique);

                    if (amount != null)
                    {
                        double saldo = 0;
                        if (IsContelSaldoAvailable(ref saldo))
                        {
                            if (IsUserSaldoAvailable(intIDPerson, request.ActivationInfo.LineInfo.PlanId))
                            {
                                bool isTopupNeeded = false;
                                var resetInfo = IsResetICCIDAvailableInPool(intIDPerson, idUnique, request.ActivationInfo.LineInfo.PlanId, intIDPerson, out resetted, ref isTopupNeeded);

                                if (resetInfo && isTopupNeeded)
                                {
                                    string planrquested = amount.txtContelPlanName;
                                    string planTopup = "";
                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Getting info for Phone:{0}", resetted.ResettedPhone), idUnique);

                                    var phoneInfo = mVNOAccess.GetContelLinesByPhoneManual(resetted.ResettedPhone);
                                    if (phoneInfo != null && phoneInfo.retorno && phoneInfo.detalhes != null)
                                    {
                                        TopUpPlanRequest topupRequest = new TopUpPlanRequest()
                                        {
                                            metodo_pagamento = "SALDO",
                                            numeros = new List<Numero>()
                                        };
                                        var num = new Numero();
                                        num.id_plano = resetted.RequiredTopup.HasValue ? GetPlanIdByGB(resetted.RequiredTopup.Value) : request.ActivationInfo.LineInfo.PlanId;
                                        num.numero = resetted.ResettedPhone;
                                        topupRequest.numeros.Add(num);

                                        string strContelPrice = string.Empty;
                                        planTopup = GetPlanNameByPlanId(num.id_plano, intIDPerson, ref strContelPrice);

                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Requested Plan:{0}", request.ActivationInfo.LineInfo.PlanId), idUnique);
                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Doing topup for Phone:{0} Plan:{1}", phoneInfo.detalhes.linha, num.id_plano), idUnique);

                                        var topupResponse = mVNOAccess.TopupPlan(topupRequest);

                                        if (topupResponse != null && topupResponse.retorno)
                                        {
                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Topup done for Phone:{0}", phoneInfo.detalhes.linha), idUnique);

                                            if (topupResponse.recarga != null)
                                            {
                                                string iccid = phoneInfo.detalhes.iccid, activationcode = string.Empty;
                                                var qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, phoneInfo.detalhes.esim_pdf, phoneInfo.detalhes.iccid, ref iccid, ref activationcode);
                                                if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                                {
                                                    TimeSpan timeTaken = watch.Elapsed;
                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Topup done for {0} in {1} seconds, deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                    if (timeTaken.TotalSeconds >= 20 && timeTaken.TotalSeconds <= 75)
                                                    {
                                                        string msgAdm = "*Attention:* \n Topup took more than 20s but sending *success* to caller as total time taken is less than 75s";
                                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                    }

                                                    if (timeTaken.TotalSeconds >= 20 && timeTaken.TotalSeconds >= 75)
                                                    {
                                                        string msgAdm = "*Attention:* \n Topup took more than 20s but sending *failure* to caller as total time taken is less than 75s";
                                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                    }

                                                    if (timeTaken.TotalSeconds <= 75)
                                                    {
                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Time taken before deducting amount for user: {0}: is {1}", intIDPerson, timeTaken.TotalSeconds), idUnique);


                                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == phoneInfo.detalhes.iccid);

                                                        //Update plans
                                                        if (tblphones != null && tblphones.Count() > 0)
                                                        {
                                                            foreach (var ph in tblphones)
                                                            {
                                                                ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                ph.bitAtivo = true;
                                                                ph.bitPhoneClube = true;
                                                                ph.intDDD = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(0, 2));
                                                                ph.intPhone = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(2));
                                                                ph.intIdPerson = intIDPerson;
                                                            }
                                                            ctx.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                            {
                                                                intDDD = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(0, 2)),
                                                                intPhone = Convert.ToInt32(phoneInfo.detalhes.linha.Substring(2)),
                                                                intCountryCode = 55,
                                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                bitAtivo = true,
                                                                bitPhoneClube = true,
                                                                intIdOperator = 4,
                                                                intIdPerson = intIDPerson,
                                                                txtICCID = iccid,
                                                                bitEsim = true
                                                            });
                                                            ctx.SaveChanges();
                                                        }

                                                        var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == phoneInfo.detalhes.linha && x.txtICCID == iccid);
                                                        if (tblEsim != null)
                                                        {
                                                            tblEsim.txtActivationCode = activationcode;
                                                            tblEsim.txtActivationImage = qrcode;
                                                            tblEsim.txtActivationPdfUrl = phoneInfo.detalhes.esim_pdf;
                                                            tblEsim.txtICCID = iccid;
                                                            tblEsim.txtLinha = phoneInfo.detalhes.linha;
                                                            tblEsim.txtPlano = amount.txtPlanName;
                                                            tblEsim.dteInsert = DateTime.Now;
                                                        }
                                                        else
                                                        {
                                                            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                            {
                                                                txtActivationCode = activationcode,
                                                                txtActivationImage = qrcode,
                                                                txtActivationPdfUrl = phoneInfo.detalhes.esim_pdf,
                                                                txtICCID = iccid,
                                                                txtLinha = phoneInfo.detalhes.linha,
                                                                txtPlano = amount.txtContelPlanName,
                                                                dteInsert = DateTime.Now
                                                            });
                                                        }
                                                        ctx.SaveChanges();
                                                        var dataAdded = mVNOAccess.AddContelLineManual(phoneInfo.detalhes.linha);

                                                        var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);

                                                        decimal remainingBal = 0;
                                                        if (balance != null)
                                                        {
                                                            remainingBal = balance.intAmountBalance;
                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                            balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                            balance.dteUpdated = DateTime.Now;
                                                            remainingBal = balance.intAmountBalance;
                                                            ctx.SaveChanges();

                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, iccid, remainingBal), idUnique);
                                                        }

                                                        double differnce = 0;
                                                        try
                                                        {
                                                            double postSaldo = 0;
                                                            var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                                            if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                                            {
                                                                postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                                differnce = Math.Round(saldo - postSaldo, 1);
                                                            }
                                                        }
                                                        catch (Exception ex) { }

                                                        ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                        {
                                                            intIdPerson = intIDPerson,
                                                            intPurchaseType = 2,
                                                            intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                            dteDeducted = DateTime.Now,
                                                            txtComments = string.Format("Deducted by FACIL for Top-up for User: {0}, ICCID:{1}", intIDPerson, iccid),
                                                            txtPhone = phoneInfo.detalhes.linha,
                                                            txtPlan = amount.txtPlanName,
                                                            bitTest = false,
                                                            bitRefund = false,
                                                            txtICCID = iccid,
                                                            intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture),
                                                        });
                                                        ctx.SaveChanges();
                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Added entry to tblInternationalUserPurchases", idUnique);

                                                        actRes.Info = "Line Activated successfully";
                                                        actRes.Status = true;

                                                        DateTime dtAt = DateTime.Now;
                                                        try
                                                        {
                                                            dtAt = DateTime.ParseExact(phoneInfo.detalhes.data_inicio_plano, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                        }
                                                        catch (Exception) { dtAt = DateTime.Now; }

                                                        actRes.ActivatedInfo = new ActivatedInfo()
                                                        {
                                                            ActivatedNumber = phoneInfo.detalhes.linha,
                                                            ActivatedPlan = amount.txtPlanName,
                                                            ActivationDate = dtAt.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                                            ICCID = iccid,
                                                            ActivationPDFLink = PdfUrl + phoneInfo.detalhes.linha + "/" + iccid,
                                                            ActivationCode = activationcode
                                                        };

                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Response to Caller : {0}, {1}, {2}, {3}, {4}",
                                                            phoneInfo.detalhes.linha, amount.txtPlanName, phoneInfo.detalhes.data_ativacao, iccid, activationcode), idUnique);

                                                        try
                                                        {


                                                            //string strContelPrice = string.Empty;
                                                            //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                            DateTime today = DateTime.Now;
                                                            LogHelper.LogMessage(intIDPerson, string.Format("Sending whatsapp msg to admin"), idUnique);

                                                            var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                                            var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                && x.dteDeducted <= today).ToList();
                                                            var totaltodayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();


                                                            LogHelper.LogMessage(intIDPerson, string.Format("Sending whatsapp msg to admin - got count"), idUnique);
                                                            string msgAdm = string.Format("*Topup: {0}*\n\n" +
                                       "Line: *{1}* \n" +
                                       "Plan: {2} \n" +
                                       "Dados: {3} \n\n" +
                                       "Today: *{4}* ${5}\n" +
                                       "Month: *{6}* ${7}\n" +
                                       "Balance: *${8}*\n\n" +
                                       "Total Today: {9} ${10}\n" +
                                       "Saldo Contel: R$ {11}\n" +
                                       "Debito Contel: R$ {12}"
                                       , person1.txtName, phoneInfo.detalhes.linha, planrquested, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted), monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted), saldo, totaltodayCount.Sum(x => x.intContelPrice.Value));

                                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                            LogHelper.LogMessage(intIDPerson, string.Format("Sent whatsapp msg to admin"), idUnique);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Timeout occured post topup", idUnique);
                                                        //var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == iccid && x.txtPhoneNumber == phoneInfo.detalhes.linha);
                                                        //if (poolData is null)
                                                        //{
                                                        //    var dataPool = new tblInternationActivationPool()
                                                        //    {
                                                        //        bitReadyForReActivation = true,
                                                        //        bitFailedPostActivation = false,
                                                        //        intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                        //        dteActivation = DateTime.Now,
                                                        //        intIdPerson = intIDPerson,
                                                        //        txtICCID = iccid,
                                                        //        intIdPersonReset = 1,
                                                        //        dteReset = DateTime.Now,
                                                        //        txtPhoneNumber = phoneInfo.detalhes.linha,
                                                        //        txtResetStatus = "Pending",
                                                        //        txtStatus = "Resell Pool + Topup"
                                                        //    };
                                                        //    ctx.tblInternationActivationPool.Add(dataPool);
                                                        //    ctx.SaveChanges();

                                                        //    int poolId = dataPool.intId;

                                                        //    ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                        //    {
                                                        //        intActivationPoolId = poolId,
                                                        //        txtICCID = iccid,
                                                        //        txtPhone = activatedPhone,
                                                        //        txtStatus = "Resell Pool + Blocked",
                                                        //        dteAction = DateTime.Now,
                                                        //        txtDoneBy = "System"
                                                        //    });
                                                        //    ctx.SaveChanges();

                                                        //    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                        //    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Blocking line:" + activatedPhone, idUnique);

                                                        //    BlockLine blockLine = new BlockLine()
                                                        //    {
                                                        //        numero = activatedPhone,
                                                        //        motivo = "BLOQUEIO DE IMEI",
                                                        //        observacoes = ""
                                                        //    };
                                                        //    mVNOAccess.BlockLine(blockLine);

                                                        //}

                                                        //LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Line activated by {0} successfully but timeout, hence not deducting money adding to pool", person1.txtName), idUnique);
                                                        //string msgAdm = string.Format("*Failed Activation due to timeout: {0}*\n\n" +
                                                        //        "Line: *{1}* \n" +
                                                        //        "Plan: {2} \n\n" +

                                                        //        "*Adding to Pool*"
                                                        //        , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome);

                                                        //new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                    }
                                                }
                                                else
                                                {
                                                    TimeSpan timeTaken = watch.Elapsed;
                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Topup done for {0} in {1} seconds", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                    if (timeTaken.TotalSeconds <= 30)
                                                    {
                                                        string msgAdm = string.Format("*Attention:* \nTopup success but failed to get line details hence putting this number to pool to cancel current sale and retrying another as time elapsed is less than 60s for line: {0} and iccid: {1}", resetted.ResettedPhone, resetted.ResettedICCID);
                                                        LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                        return ActivateESimNew(request);
                                                    }
                                                    else
                                                    {
                                                        actRes.Status = false;
                                                        actRes.Error = "Activation failed with reason: Internal Error : T101";

                                                        string msgAdm = string.Format("*Attention:* \nError code *T101*:\nTopup success but failed to get line details, Timeout passed for line: {0} and iccid: {1} hence cancelling current sale and not retrying another number\n*No amount deducted from user*", resetted.ResettedPhone, resetted.ResettedICCID);
                                                        LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                TimeSpan timeTaken = watch.Elapsed;
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Topup done for {0} in {1} seconds", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                if (timeTaken.TotalSeconds <= 30)
                                                {
                                                    string msgAdm = string.Format("*Attention:* \nTopup success but failed to get topup details hence putting this number to pool to cancel current sale and retrying another as time elapsed is less than 60s for line: {0} and iccid: {1}", resetted.ResettedPhone, resetted.ResettedICCID);
                                                    LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                                    var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };

                                                    return ActivateESimNew(request);
                                                }
                                                else
                                                {
                                                    actRes.Status = false;
                                                    actRes.Error = "Activation failed with reason: Internal Error : T102";

                                                    string msgAdm = string.Format("*Attention:* \nError code *T102*:\nTopup success but failed to get topup details, Timeout passed for line: {0} and iccid: {1} hence cancelling current sale and not retrying another number", resetted.ResettedPhone, resetted.ResettedICCID);
                                                    LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                                    var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                }
                                            }
                                        }
                                        else if (topupResponse != null && !topupResponse.retorno && topupResponse.mensagem == "Timeout error")
                                        {
                                            string msgAdm = string.Format("*Attention:* \n Topup took more than 20s for line: {0} and iccid: {1} hence cancelling current sale and retrying another number from pool", resetted.ResettedPhone, resetted.ResettedICCID);
                                            LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };

                                            TimeSpan timeTaken = watch.Elapsed;
                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Topup cancelled for {0} in {1} seconds, not deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                            if (timeTaken.TotalSeconds >= 20 && timeTaken.TotalSeconds <= 60)
                                            {
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Retrying activation with same request"), idUnique);
                                                var tasks1 = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin("*Attention:* \nRetry activation on timeout error")) };
                                                return ActivateESimNew(request);
                                            }
                                        }
                                        else
                                        {
                                            actRes.Status = false;
                                            actRes.Error = "Activation failed with reason: Internal Error : T102";

                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Activation failed with reason: {0} for User: {1}, Activation failed with reason: Internal Error : T102", topupResponse.mensagem, intIDPerson), idUnique);

                                            string msgAdm = string.Format("*Attention:* \nError code *T102*:\nTopup failure and Timeout passed for line: {0} and iccid: {1} hence cancelling current sale and not retrying another number\n*No amount deducted from user*", resetted.ResettedPhone, resetted.ResettedICCID);
                                            LogHelper.LogMessage(intIDPerson, msgAdm, idUnique);
                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                        }
                                    }
                                    else
                                    {
                                        TimeSpan timeTaken = watch.Elapsed;

                                        if (timeTaken.TotalSeconds <= 30)
                                        {
                                            string msgAdm1 = string.Format("*Attention:* \nLine not found in contel: {0} so cancelling current sale and retrying another as time elapsed is less than 30s for line: {1} and iccid: {2}", resetted.ResettedPhone, resetted.ResettedPhone, resetted.ResettedICCID);
                                            LogHelper.LogMessage(intIDPerson, msgAdm1, idUnique);
                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm1)) };

                                            return ActivateESimNew(request);
                                        }
                                        else
                                        {
                                            actRes.Status = false;
                                            actRes.Error = "Activation failed with reason: Internal Error : T103";

                                            string msgAdm2 = string.Format("*Attention:* \nError code *T102*:\nLine not found in contel: {0}, Timeout passed for line: {1} and iccid: {2} hence cancelling current sale and not retrying another number", resetted.ResettedPhone, resetted.ResettedPhone, resetted.ResettedICCID);
                                            LogHelper.LogMessage(intIDPerson, msgAdm2, idUnique);
                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm2)) };
                                        }
                                    }
                                }
                                else if (resetInfo && !isTopupNeeded)
                                {
                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Failed activation esim available with ICCID:{0} and Phone:{1} hence doing re-sale", resetted.ResettedICCID, resetted.ResettedPhone), idUnique);

                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Unblocking Phone:{0} hence doing re-sale", resetted.ResettedPhone), idUnique);

                                    var unblock = new UnBlockLine() { numero = resetted.ResettedPhone };
                                    mVNOAccess.UnBlockLine(unblock);

                                    //var iccidPhoneData = ValidateInLoop(intIDPerson, idUnique, resetted.ResettedICCID,Convert.ToString(request.Enviroment));
                                    var iccidPhoneData = mVNOAccess.GetContelLinesByPhoneManual(resetted.ResettedPhone);

                                    if (iccidPhoneData != null && iccidPhoneData.retorno && iccidPhoneData.detalhes != null && !string.IsNullOrEmpty(iccidPhoneData.detalhes.linha))
                                    {
                                        var activatedPhone = iccidPhoneData.detalhes.linha;

                                        actRes.Status = true;

                                        string iccid = resetted.ResettedICCID, activationcode = string.Empty;
                                        var qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, iccidPhoneData.detalhes.esim_pdf, activatedPhone, ref iccid, ref activationcode);
                                        if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                        {
                                            var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.detalhes.iccid);

                                            //Update plans
                                            if (tblphones != null && tblphones.Count() > 0)
                                            {
                                                foreach (var ph in tblphones)
                                                {
                                                    ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                    ph.bitAtivo = true;
                                                    ph.bitPhoneClube = true;
                                                    ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                    ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                    ph.intIdPerson = intIDPerson;
                                                }
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                {
                                                    intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                    intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                    intCountryCode = 55,
                                                    intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                    bitAtivo = true,
                                                    bitPhoneClube = true,
                                                    intIdOperator = 4,
                                                    intIdPerson = intIDPerson,
                                                    txtICCID = iccidPhoneData.detalhes.iccid,
                                                    bitEsim = true
                                                });
                                                ctx.SaveChanges();
                                            }

                                            var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == activatedPhone && x.txtICCID == iccid);
                                            if (tblEsim != null)
                                            {
                                                tblEsim.txtActivationCode = activationcode;
                                                tblEsim.txtActivationDate = iccidPhoneData.detalhes.data_ativacao;
                                                tblEsim.txtActivationImage = qrcode;
                                                tblEsim.txtActivationPdfUrl = iccidPhoneData.detalhes.esim_pdf;
                                                tblEsim.txtICCID = iccid;
                                                tblEsim.txtLinha = activatedPhone;
                                                tblEsim.txtPlano = iccidPhoneData.detalhes.plano;
                                                tblEsim.dteInsert = DateTime.Now;
                                            }
                                            else
                                            {
                                                ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                {
                                                    txtActivationCode = activationcode,
                                                    txtActivationDate = iccidPhoneData.detalhes.data_ativacao,
                                                    txtActivationImage = qrcode,
                                                    txtActivationPdfUrl = iccidPhoneData.detalhes.esim_pdf,
                                                    txtICCID = iccid,
                                                    txtLinha = activatedPhone,
                                                    txtPlano = iccidPhoneData.detalhes.plano,
                                                    dteInsert = DateTime.Now
                                                });
                                            }
                                            ctx.SaveChanges();
                                            var dataAdded = mVNOAccess.AddContelLineManual(activatedPhone);
                                            decimal remainingBal = 0;

                                            var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);
                                            if (balance != null)
                                            {
                                                remainingBal = balance.intAmountBalance;
                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                balance.dteUpdated = DateTime.Now;
                                                remainingBal = balance.intAmountBalance;
                                                ctx.SaveChanges();

                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, resetted.ResettedICCID, remainingBal), idUnique);
                                            }

                                            var tblEsimPurc = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.txtPhone == activatedPhone && !x.bitRefund.Value);

                                            double differnce = 0;
                                            try
                                            {
                                                double postSaldo = 0;
                                                var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                                if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                                {
                                                    postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                    differnce = Math.Round(saldo - postSaldo, 1);
                                                }
                                            }
                                            catch (Exception ex) { }


                                            if (tblEsimPurc is null)
                                            {
                                                ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                {
                                                    intIdPerson = intIDPerson,
                                                    intPurchaseType = 1,
                                                    intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                    dteDeducted = DateTime.Now,
                                                    txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, resetted.ResettedICCID),
                                                    txtPhone = activatedPhone,
                                                    txtPlan = amount.txtPlanName,
                                                    bitTest = false,
                                                    bitRefund = false,
                                                    txtICCID = iccid,
                                                    intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture)
                                                });
                                                ctx.SaveChanges();
                                            }

                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Added entry to tblInternationalUserPurchases", idUnique);

                                            actRes.Info = "Line Activated successfully";

                                            actRes.ActivatedInfo = new ActivatedInfo()
                                            {
                                                ActivatedNumber = activatedPhone,
                                                ActivatedPlan = iccidPhoneData.detalhes.plano,
                                                ActivationDate = iccidPhoneData.detalhes.data_ativacao,
                                                ICCID = iccidPhoneData.detalhes.iccid,
                                                ActivationPDFLink = PdfUrl + activatedPhone + "/" + iccid,
                                                ActivationCode = activationcode
                                            };

                                            try
                                            {
                                                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                DateTime today = DateTime.Now;

                                                var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                    .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                                var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                    .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                    && x.dteDeducted <= today).ToList();
                                                var totaltodayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                    .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();


                                                DateTime now = DateTime.Now;
                                                DateTime nowto23hrs = now.AddHours(-23);
                                                var poolRemainingCount = ctx.tblInternationActivationPool.Where(x => x.bitReadyForReActivation == true && x.intIdPerson == intIDPerson && (x.dteReset > nowto23hrs && x.dteReset <= now)).Count();

                                                string msgAdm = string.Format("*Activation from Pool: {0}*\n\n" +
                                                    "Line: *{1}* \n" +
                                                    "Plan: {2} \n" +
                                                    "Dados: {3} \n\n" +
                                                    "Today: *{4}* ${5}\n" +
                                                    "Month: *{6}* ${7}\n" +
                                                    "Balance: *${8}*\n\n" +
                                                    "Total Today: *{10}* *${11}*\n" +
                                                    "Debito Contel: R$ {12}\n" +
                                                    "23 hour Pool: {9}"
                                                    , person1.txtName, activatedPhone, iccidPhoneData.detalhes.plano, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted),
                                                    monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, poolRemainingCount, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted), totaltodayCount.Sum(x => x.intContelPrice.Value));

                                                var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                            }
                                            catch (Exception ex)
                                            {
                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                            }
                                        }
                                        else
                                        {
                                            actRes.Status = false;
                                            actRes.Error = "Failed to get activation code post activation hence cancelling sale and no payment deducted";
                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Returing activation failure due to not get activation code after 15 retries", idUnique);

                                            try
                                            {
                                                string msgAdm = string.Format("*Attention:*\nActivation success but failed to get Activation Code even after 15 retries. Pushing below line detail to activation pool\nLine:{0}\nICCID:{1}", activatedPhone, resetted.ResettedICCID);
                                                var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == resetted.ResettedICCID && x.txtPhoneNumber == activatedPhone);
                                                if (poolData is null)
                                                {
                                                    var dataPool = new tblInternationActivationPool()
                                                    {
                                                        bitReadyForReActivation = true,
                                                        bitFailedPostActivation = true,
                                                        intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                        dteActivation = DateTime.Now,
                                                        intIdPerson = intIDPerson,
                                                        txtICCID = iccid,
                                                        intIdPersonReset = 1,
                                                        dteReset = DateTime.Now,
                                                        txtPhoneNumber = activatedPhone,
                                                        txtResetStatus = "Pending",
                                                        txtStatus = "Resell Pool + Blocked"
                                                    };
                                                    ctx.tblInternationActivationPool.Add(dataPool);
                                                    ctx.SaveChanges();

                                                    int poolId = dataPool.intId;

                                                    ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                    {
                                                        intActivationPoolId = poolId,
                                                        txtICCID = iccid,
                                                        txtPhone = activatedPhone,
                                                        txtStatus = "Resell Pool + Blocked",
                                                        dteAction = DateTime.Now,
                                                        txtDoneBy = "System"
                                                    });
                                                    ctx.SaveChanges();

                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Blocking line:" + activatedPhone, idUnique);

                                                    BlockLine blockLine = new BlockLine()
                                                    {
                                                        numero = activatedPhone,
                                                        motivo = "BLOQUEIO DE IMEI",
                                                        observacoes = ""
                                                    };
                                                    mVNOAccess.BlockLine(blockLine);
                                                }
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                    else
                                    {
                                        actRes.Status = true;

                                        double differnce = 0;
                                        try
                                        {
                                            double postSaldo = 0;
                                            var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                            if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                            {
                                                postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                differnce = Math.Round(saldo - postSaldo, 1);
                                            }
                                        }
                                        catch (Exception ex) { }

                                        ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                        {
                                            intIdPerson = intIDPerson,
                                            intPurchaseType = 1,
                                            intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                            dteDeducted = DateTime.Now,
                                            txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, resetted.ResettedICCID),
                                            txtPhone = "111111111",
                                            txtPlan = amount.txtPlanName,
                                            bitTest = false,
                                            bitRefund = false,
                                            txtICCID = resetted.ResettedICCID,
                                            intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture)
                                        });
                                        ctx.SaveChanges();

                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == resetted.ResettedICCID);

                                        //Update plans
                                        if (tblphones != null && tblphones.Count() > 0)
                                        {
                                            foreach (var ph in tblphones)
                                            {
                                                ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                ph.bitAtivo = true;
                                                ph.bitPhoneClube = true;
                                                ph.intIdPerson = intIDPerson;
                                            }
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                            {
                                                intCountryCode = 55,
                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                bitAtivo = true,
                                                bitPhoneClube = true,
                                                intIdOperator = 4,
                                                intIdPerson = intIDPerson,
                                                bitEsim = true
                                            });
                                            ctx.SaveChanges();
                                        }
                                        actRes.Info = "Line Activated successfully but failed to get activated line details, please call validate iccid api to get line details, No amount deducted";
                                        actRes.ActivatedInfo = new ActivatedInfo()
                                        {
                                            ActivatedPlan = "",
                                            ActivationDate = "",
                                            ICCID = resetted.ResettedICCID
                                        };

                                        string msgAdmin = string.Format("User Id: {0} has activated new line but failed to get line detail for iccid: {1}, No amount deducted", intIDPerson, resetted.ResettedICCID);
                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdmin)) };

                                        try
                                        {
                                            //string strContelPrice = string.Empty;
                                            //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                            DateTime today = DateTime.Now;

                                            var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).Count();
                                            var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                && x.dteDeducted <= today).Count();

                                            string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                "ICCID: *{1}* \n" +
                                                "Plan: {2} \n\n" +
                                                "Today: *{3}*\n" +
                                                "Month: *{4}*"
                                                , person1.txtName, resetted.ResettedICCID, "", todayCount, monthCount);

                                            var tasks1 = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                        }
                                    }
                                }
                                else
                                {
                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: There are no resetted phonenumber hence proceed with eSIM activation", idUnique);

                                    var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == intIDPerson);
                                    string activationcode = string.Empty;
                                    if (person != null)
                                    {
                                        var iccidPool = ValidateNumberByICCIDInLoop(intIDPerson, idUnique);

                                        if (iccidPool != null)
                                        {
                                            string newICCID = iccidPool.txtICCID;
                                            activationcode = iccidPool.txtActivationCodeLPA;

                                            ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                                            activatePlanRequest.metodo_pagamento = "SALDO";
                                            activatePlanRequest.nome = string.IsNullOrEmpty(request.ActivationInfo.CustomerInfo.Name) ? person.txtName : request.ActivationInfo.CustomerInfo.Name;

                                            //if (person.txtDocumentNumber.Length == 11)
                                            //    activatePlanRequest.cpf = person.txtDocumentNumber;
                                            //else
                                            activatePlanRequest.cnpj = "06940292000129";

                                            activatePlanRequest.email = string.IsNullOrEmpty(request.ActivationInfo.CustomerInfo.Email) ? "suporte@foneclube.com.br" : request.ActivationInfo.CustomerInfo.Email;
                                            activatePlanRequest.telefone = "21981908190";
                                            activatePlanRequest.data_nascimento = "1900-01-01";
                                            activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();

                                            activatePlanRequest.endereco.rua = "Avenida das americas";
                                            activatePlanRequest.endereco.numero = "3434";
                                            activatePlanRequest.endereco.complemento = "305 bloco 2";
                                            activatePlanRequest.endereco.bairro = "Barra da Tijuca";
                                            activatePlanRequest.endereco.cep = "22640102";
                                            activatePlanRequest.endereco.municipio = "Rio de Janeiro";
                                            activatePlanRequest.endereco.uf = "RJ";


                                            activatePlanRequest.chips = new List<Chip>();
                                            var chip = new Chip()
                                            {
                                                ddd = request.ActivationInfo.LineInfo.DDD == 0 ? 21 : request.ActivationInfo.LineInfo.DDD,
                                                id_plano = request.ActivationInfo.LineInfo.PlanId,
                                                esim = newICCID
                                            };

                                            activatePlanRequest.chips.Add(chip);

                                            if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                                            {
                                                var response = mVNOAccess.ActivatePlan(activatePlanRequest, request.Enviroment.ToString());

                                                if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                                                {
                                                    foreach (var pho in response.info.chips)
                                                    {
                                                        var isSameIccidExists = ctx.tblInternationalUserPurchases.Where(x => x.txtICCID == pho.iccid).Count();

                                                        if (isSameIccidExists == 0)
                                                        {
                                                            var iccidPhoneData = ValidateInLoop(intIDPerson, idUnique, pho.iccid, request.Enviroment.ToString());

                                                            if (iccidPhoneData != null && iccidPhoneData.retorno && iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                                            {
                                                                var activatedPhone = iccidPhoneData.info.numero_ativado;

                                                                actRes.Status = true;

                                                                string oldFileName = string.Format(@"C:\Temp\Contel\ActivationFiles\OrignalV2\{0}.pdf", pho.iccid);
                                                                string newFileName = string.Format(@"C:\inetroot\FacilActivationPdfs\{0}_{1}.pdf", activatedPhone, pho.iccid);

                                                                string iccid = pho.iccid;
                                                                string qrcode = string.Empty;

                                                                if (System.IO.File.Exists(oldFileName))
                                                                {
                                                                    var tbliccid = ctx.tblESimICCIDPool.FirstOrDefault(x => x.txtICCID == pho.iccid);
                                                                    if (tbliccid != null)
                                                                    {
                                                                        activationcode = tbliccid.txtActivationCodeLPA;
                                                                        qrcode = QRCodeHelper.GenerateQRCode(activationcode);
                                                                    }

                                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Skipping download as file is already present for iccid: {0}", pho.iccid), idUnique);
                                                                    try
                                                                    {
                                                                        System.IO.File.Copy(oldFileName, newFileName);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Error occured while copying file for iccid : {0}", ex.ToString()), idUnique);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Downloading file again as it not exists: {0}", pho.iccid), idUnique);

                                                                    qrcode = DownloadActivationFileContelInloop(intIDPerson, idUnique, iccidPhoneData.info.esim, activatedPhone, ref iccid, ref activationcode);
                                                                }

                                                                if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                                                {
                                                                    TimeSpan timeTaken = watch.Elapsed;
                                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Line activated for {0} in {1} seconds, deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                                    if (timeTaken.TotalSeconds <= 80)
                                                                    {
                                                                        LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Time taken before deducting amount for user: {0}: is {1}", intIDPerson, timeTaken.TotalSeconds), idUnique);


                                                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                                                        //Update plans
                                                                        if (tblphones != null && tblphones.Count() > 0)
                                                                        {
                                                                            foreach (var ph in tblphones)
                                                                            {
                                                                                ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                                ph.bitAtivo = true;
                                                                                ph.bitPhoneClube = true;
                                                                                ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                                                ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                                                ph.intIdPerson = person.intIdPerson;
                                                                            }
                                                                            ctx.SaveChanges();
                                                                        }
                                                                        else
                                                                        {
                                                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                                            {
                                                                                intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                                                intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                                                intCountryCode = 55,
                                                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                                bitAtivo = true,
                                                                                bitPhoneClube = true,
                                                                                intIdOperator = 4,
                                                                                intIdPerson = person.intIdPerson,
                                                                                txtICCID = iccidPhoneData.info.iccid,
                                                                                bitEsim = true
                                                                            });
                                                                            ctx.SaveChanges();
                                                                        }

                                                                        var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == activatedPhone && x.txtICCID == iccid);
                                                                        if (tblEsim != null)
                                                                        {
                                                                            tblEsim.txtActivationCode = activationcode;
                                                                            tblEsim.txtActivationDate = response.info.data_cadastro;
                                                                            tblEsim.txtActivationImage = qrcode;
                                                                            tblEsim.txtActivationPdfUrl = response.link_esim;
                                                                            tblEsim.txtICCID = iccid;
                                                                            tblEsim.txtLinha = activatedPhone;
                                                                            tblEsim.txtPlano = iccidPhoneData.info.plano_nome;
                                                                            tblEsim.dteInsert = DateTime.Now;
                                                                        }
                                                                        else
                                                                        {
                                                                            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                                            {
                                                                                txtActivationCode = activationcode,
                                                                                txtActivationDate = response.info.data_cadastro,
                                                                                txtActivationImage = qrcode,
                                                                                txtActivationPdfUrl = response.link_esim,
                                                                                txtICCID = iccid,
                                                                                txtLinha = activatedPhone,
                                                                                txtPlano = iccidPhoneData.info.plano_nome,
                                                                                dteInsert = DateTime.Now
                                                                            });
                                                                        }
                                                                        ctx.SaveChanges();
                                                                        var dataAdded = mVNOAccess.AddContelLineManual(activatedPhone);

                                                                        var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);

                                                                        decimal remainingBal = 0;
                                                                        if (balance != null)
                                                                        {
                                                                            remainingBal = balance.intAmountBalance;
                                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                                            balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                                            balance.dteUpdated = DateTime.Now;
                                                                            remainingBal = balance.intAmountBalance;
                                                                            ctx.SaveChanges();

                                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, pho.iccid, remainingBal), idUnique);
                                                                        }

                                                                        double differnce = 0;
                                                                        try
                                                                        {
                                                                            double postSaldo = 0;
                                                                            var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                                                            if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                                                            {
                                                                                postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                                                differnce = Math.Round(saldo - postSaldo, 1);
                                                                            }
                                                                        }
                                                                        catch (Exception ex) { }

                                                                        ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                                        {
                                                                            intIdPerson = intIDPerson,
                                                                            intPurchaseType = 1,
                                                                            intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                                            dteDeducted = DateTime.Now,
                                                                            txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, pho.iccid),
                                                                            txtPhone = activatedPhone,
                                                                            txtPlan = amount.txtPlanName,
                                                                            bitTest = false,
                                                                            bitRefund = false,
                                                                            txtICCID = iccidPhoneData.info.iccid,
                                                                            intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture)
                                                                        });
                                                                        ctx.SaveChanges();
                                                                        LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Added entry to tblInternationalUserPurchases", idUnique);

                                                                        actRes.Info = "Line Activated successfully";

                                                                        actRes.ActivatedInfo = new ActivatedInfo()
                                                                        {
                                                                            ActivatedNumber = activatedPhone,
                                                                            ActivatedPlan = iccidPhoneData.info.plano_nome,
                                                                            ActivationDate = iccidPhoneData.info.data_ativacao,
                                                                            ICCID = iccidPhoneData.info.iccid,
                                                                            ActivationPDFLink = PdfUrl + activatedPhone + "/" + iccid,
                                                                            ActivationCode = activationcode
                                                                        };

                                                                        try
                                                                        {

                                                                            //string strContelPrice = string.Empty;
                                                                            //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                            DateTime today = DateTime.Now;

                                                                            var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                                                            var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                                && x.dteDeducted <= today).ToList();
                                                                            var totaltodayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();


                                                                            string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                       "Line: *{1}* \n" +
                                                       "Plan: {2} \n" +
                                                       "Dados: {3} \n\n" +
                                                       "Today: *{4}* ${5}\n" +
                                                       "Month: *{6}* ${7}\n" +
                                                       "Balance: *${8}*\n\n" +
                                                       "Total Today: *{9}* *${10}*\n" +
                                                       "Saldo Contel: R$ {11}\n" +
                                                       "Debito Contel: R$ {12}"
                                                       , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome, dataAdded, todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted), monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted), saldo, totaltodayCount.Sum(x => x.intContelPrice.Value));

                                                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (intIDPerson == 6464)
                                                                        {
                                                                            var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == pho.iccid && x.txtPhoneNumber == activatedPhone);
                                                                            if (poolData is null)
                                                                            {
                                                                                var dataPool = new tblInternationActivationPool()
                                                                                {
                                                                                    bitReadyForReActivation = true,
                                                                                    bitFailedPostActivation = true,
                                                                                    intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                                    dteActivation = DateTime.Now,
                                                                                    intIdPerson = intIDPerson,
                                                                                    txtICCID = iccid,
                                                                                    intIdPersonReset = 1,
                                                                                    dteReset = DateTime.Now,
                                                                                    txtPhoneNumber = activatedPhone,
                                                                                    txtResetStatus = "Pending",
                                                                                    txtStatus = "Resell Pool + Blocked"
                                                                                };
                                                                                ctx.tblInternationActivationPool.Add(dataPool);
                                                                                ctx.SaveChanges();

                                                                                int poolId = dataPool.intId;

                                                                                ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                                                {
                                                                                    intActivationPoolId = poolId,
                                                                                    txtICCID = iccid,
                                                                                    txtPhone = activatedPhone,
                                                                                    txtStatus = "Resell Pool + Blocked",
                                                                                    dteAction = DateTime.Now,
                                                                                    txtDoneBy = "System"
                                                                                });
                                                                                ctx.SaveChanges();

                                                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                                                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Blocking line:" + activatedPhone, idUnique);

                                                                                BlockLine blockLine = new BlockLine()
                                                                                {
                                                                                    numero = activatedPhone,
                                                                                    motivo = "BLOQUEIO DE IMEI",
                                                                                    observacoes = ""
                                                                                };
                                                                                mVNOAccess.BlockLine(blockLine);

                                                                            }

                                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Line activated by {0} successfully but timeout, hence not deducting money adding to pool", person1.txtName), idUnique);
                                                                            string msgAdm = string.Format("*Failed Activation due to timeout: {0}*\n\n" +
                                                                                    "Line: *{1}* \n" +
                                                                                    "Plan: {2} \n\n" +

                                                                                    "*Adding to Pool*"
                                                                                    , person1.txtName, activatedPhone, iccidPhoneData.info.plano_nome);

                                                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                                        }
                                                                        else
                                                                        {
                                                                            LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Line activated for {0} in {1} seconds, failed fetching phone but still proceeding - deducting money", person1.txtName, timeTaken.TotalSeconds), idUnique);

                                                                            var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == newICCID);

                                                                            //Update plans
                                                                            if (tblphones != null && tblphones.Count() > 0)
                                                                            {
                                                                                foreach (var ph in tblphones)
                                                                                {
                                                                                    ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                                    ph.bitAtivo = true;
                                                                                    ph.bitPhoneClube = true;
                                                                                    ph.intDDD = 99;
                                                                                    ph.intPhone = 999999999;
                                                                                    ph.intIdPerson = person.intIdPerson;
                                                                                }
                                                                                ctx.SaveChanges();
                                                                            }
                                                                            else
                                                                            {
                                                                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                                                {
                                                                                    intDDD = 99,
                                                                                    intPhone = 999999999,
                                                                                    intCountryCode = 55,
                                                                                    intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                                    bitAtivo = true,
                                                                                    bitPhoneClube = true,
                                                                                    intIdOperator = 4,
                                                                                    intIdPerson = person.intIdPerson,
                                                                                    txtICCID = newICCID,
                                                                                    bitEsim = true
                                                                                });
                                                                                ctx.SaveChanges();
                                                                            }

                                                                            var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == activatedPhone && x.txtICCID == iccid);
                                                                            if (tblEsim != null)
                                                                            {
                                                                                tblEsim.txtActivationCode = activationcode;
                                                                                tblEsim.txtActivationDate = response.info.data_cadastro;
                                                                                tblEsim.txtActivationImage = qrcode;
                                                                                tblEsim.txtActivationPdfUrl = response.link_esim;
                                                                                tblEsim.txtICCID = newICCID;
                                                                                tblEsim.txtLinha = "99999999999";
                                                                                tblEsim.txtPlano = iccidPhoneData.info.plano_nome;
                                                                                tblEsim.dteInsert = DateTime.Now;
                                                                            }
                                                                            else
                                                                            {
                                                                                ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                                                                {
                                                                                    txtActivationCode = activationcode,
                                                                                    txtActivationDate = response.info.data_cadastro,
                                                                                    txtActivationImage = qrcode,
                                                                                    txtActivationPdfUrl = response.link_esim,
                                                                                    txtICCID = newICCID,
                                                                                    txtLinha = "99999999999",
                                                                                    txtPlano = iccidPhoneData.info.plano_nome,
                                                                                    dteInsert = DateTime.Now
                                                                                });
                                                                            }
                                                                            ctx.SaveChanges();

                                                                            var balance = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == intIDPerson);

                                                                            decimal remainingBal = 0;
                                                                            if (balance != null)
                                                                            {
                                                                                remainingBal = balance.intAmountBalance;
                                                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance before activation in tblInternationalUserBalance for user: {0}: ${1}", intIDPerson, remainingBal), idUnique);

                                                                                balance.intAmountBalance = balance.intAmountBalance - Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture);
                                                                                balance.dteUpdated = DateTime.Now;
                                                                                remainingBal = balance.intAmountBalance;
                                                                                ctx.SaveChanges();

                                                                                LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Current balance updated post activation in tblInternationalUserBalance for user: {0} with ICCID: {1} : ${2}", intIDPerson, pho.iccid, remainingBal), idUnique);
                                                                            }

                                                                            double differnce = 0;
                                                                            try
                                                                            {
                                                                                double postSaldo = 0;
                                                                                var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                                                                if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                                                                {
                                                                                    postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                                                    differnce = Math.Round(saldo - postSaldo, 1);
                                                                                }
                                                                            }
                                                                            catch (Exception ex) { }

                                                                            ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                                            {
                                                                                intIdPerson = intIDPerson,
                                                                                intPurchaseType = 1,
                                                                                intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                                                dteDeducted = DateTime.Now,
                                                                                txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, pho.iccid),
                                                                                txtPhone = "99999999999",
                                                                                txtPlan = amount.txtPlanName,
                                                                                bitTest = false,
                                                                                bitRefund = false,
                                                                                txtICCID = newICCID,
                                                                                intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture)
                                                                            });
                                                                            ctx.SaveChanges();
                                                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Added entry to tblInternationalUserPurchases", idUnique);

                                                                            actRes.Status = true;
                                                                            actRes.Info = "Line Activated successfully but failed to get phone, please call phone detail API";

                                                                            actRes.ActivatedInfo = new ActivatedInfo()
                                                                            {
                                                                                ActivatedNumber = "99999999999",
                                                                                ActivatedPlan = amount.txtPlanName,
                                                                                ActivationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                                                                ICCID = newICCID,
                                                                                ActivationCode = activationcode
                                                                            };

                                                                            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                            DateTime today = DateTime.Now;

                                                                            var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                                                            var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                                && x.dteDeducted <= today).ToList();

                                                                            var totaltodayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                                .Where(x => DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).ToList();

                                                                            string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                                                             "Line: *{1}* \n" +
                                                                                             "Plan: {2} \n" +
                                                                                             "Dados: {3} \n\n" +
                                                                                             "Today: *{4}* ${5}\n" +
                                                                                             "Month: *{6}* ${7}\n" +
                                                                                             "Balance: *${8}*\n\n" +
                                                                                             "Total Today: *{9}* *${10}*"
                                                                                             , person1.txtName, "99999999999", iccidPhoneData.info.plano_nome, "0.00 GB", todayCount.Count(), todayCount.Sum(x => x.intAmountDeducted), monthCount.Count(), monthCount.Sum(x => x.intAmountDeducted), remainingBal, totaltodayCount.Count(), totaltodayCount.Sum(x => x.intAmountDeducted));

                                                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    actRes.Status = false;
                                                                    actRes.Error = "Failed to get activation code post activation hence cancelling sale and no payment deducted";
                                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Returing activation failure due to not get activation code after 15 retries", idUnique);

                                                                    try
                                                                    {
                                                                        string msgAdm = string.Format("*Attention:*\nActivation success but failed to get Activation Code even after 15 retries. Pushing below line detail to activation pool\nLine:{0}\nICCID:{1}", activatedPhone, pho.iccid);
                                                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                                        var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtICCID == pho.iccid && x.txtPhoneNumber == activatedPhone);
                                                                        if (poolData is null)
                                                                        {
                                                                            ctx.tblInternationActivationPool.Add(new tblInternationActivationPool()
                                                                            {
                                                                                bitReadyForReActivation = true,
                                                                                bitFailedPostActivation = true,
                                                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                                dteActivation = DateTime.Now,
                                                                                intIdPerson = intIDPerson,
                                                                                txtICCID = iccid,
                                                                                intIdPersonReset = 1,
                                                                                dteReset = DateTime.Now,
                                                                                txtPhoneNumber = activatedPhone,
                                                                                txtResetICCID = iccid,
                                                                                txtResetStatus = "Pending",
                                                                                txtStatus = "Resell Pool + Blocked"
                                                                            });
                                                                            ctx.SaveChanges();

                                                                            LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Adding line to tblInternationActivationPool:" + activatedPhone, idUnique);
                                                                        }
                                                                    }
                                                                    catch (Exception ex) { }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                actRes.Status = true;

                                                                double differnce = 0;
                                                                try
                                                                {
                                                                    double postSaldo = 0;
                                                                    var dbval = mVNOAccess.GetRemainingSaldoForCompany();
                                                                    if (dbval != null && !string.IsNullOrEmpty(dbval.saldo))
                                                                    {
                                                                        postSaldo = Convert.ToDouble(dbval.saldo, CultureInfo.InvariantCulture);
                                                                        differnce = Math.Round(Math.Round(saldo - postSaldo, 1), 1);
                                                                    }
                                                                }
                                                                catch (Exception ex) { }

                                                                ctx.tblInternationalUserPurchases.Add(new tblInternationalUserPurchases()
                                                                {
                                                                    intIdPerson = intIDPerson,
                                                                    intPurchaseType = 1,
                                                                    intAmountDeducted = Convert.ToDecimal(amount.txtPrice, CultureInfo.InvariantCulture),
                                                                    dteDeducted = DateTime.Now,
                                                                    txtComments = string.Format("Deducted by FACIL for purchase of new eSIM for User: {0}, ICCID:{1}", intIDPerson, pho.iccid),
                                                                    txtPhone = "111111111",
                                                                    txtPlan = amount.txtPlanName,
                                                                    bitTest = false,
                                                                    bitRefund = false,
                                                                    txtICCID = pho.iccid,
                                                                    intContelPrice = Convert.ToDecimal(differnce, CultureInfo.InvariantCulture)
                                                                });
                                                                ctx.SaveChanges();

                                                                var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == pho.iccid);

                                                                //Update plans
                                                                if (tblphones != null && tblphones.Count() > 0)
                                                                {
                                                                    foreach (var ph in tblphones)
                                                                    {
                                                                        ph.intIdPlan = request.ActivationInfo.LineInfo.PlanId;
                                                                        ph.bitAtivo = true;
                                                                        ph.bitPhoneClube = true;
                                                                        ph.intIdPerson = intIDPerson;
                                                                    }
                                                                    ctx.SaveChanges();
                                                                }
                                                                else
                                                                {
                                                                    ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                                    {
                                                                        intCountryCode = 55,
                                                                        intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                        bitAtivo = true,
                                                                        bitPhoneClube = true,
                                                                        intIdOperator = 4,
                                                                        intIdPerson = intIDPerson,
                                                                        bitEsim = true
                                                                    });
                                                                    ctx.SaveChanges();
                                                                }
                                                                actRes.Info = "Line Activated successfully but failed to get activated line details, please call validate iccid api to get line details";
                                                                actRes.ActivatedInfo = new ActivatedInfo()
                                                                {
                                                                    ActivatedPlan = iccidPhoneData.info.plano_nome,
                                                                    ActivationDate = iccidPhoneData.info.data_ativacao,
                                                                    ICCID = pho.iccid
                                                                };

                                                                string msgAdmin = string.Format("User Id: {0} has activated new line but failed to get line detail for iccid: {1}", intIDPerson, pho.iccid);
                                                                var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdmin)) };

                                                                try
                                                                {
                                                                    //string strContelPrice = string.Empty;
                                                                    //string price = GetAmountByPlan(request.ActivationInfo.LineInfo.PlanId, intIDPerson, ref strContelPrice);
                                                                    DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                    DateTime today = DateTime.Now;

                                                                    var todayCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                        .Where(x => x.intIdPerson == intIDPerson && DbFunctions.TruncateTime(x.dteDeducted) == DbFunctions.TruncateTime(DateTime.Now)).Count();
                                                                    var monthCount = ctx.tblInternationalUserPurchases.AsNoTracking().OrderByDescending(x => x.dteDeducted)
                                                                                        .Where(x => x.intIdPerson == intIDPerson && x.dteDeducted >= startDate
                                                                                        && x.dteDeducted <= today).Count();

                                                                    string msgAdm = string.Format("*Activation: {0}*\n\n" +
                                                                        "ICCID: *{1}* \n" +
                                                                        "Plan: {2} \n\n" +
                                                                        "Today: *{3}*\n" +
                                                                        "Month: *{4}*"
                                                                        , person1.txtName, pho.iccid, iccidPhoneData.info.plano_nome, todayCount, monthCount);

                                                                    var tasks1 = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: SendMessageInfoToAdmin error:" + ex.ToString(), idUnique);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            actRes.Status = false;
                                                            actRes.Error = "Contel issue - duplicate ICCID found, no amount deducted";

                                                            var dataPool = new tblInternationActivationPool()
                                                            {
                                                                intIdPerson = intIDPerson,
                                                                bitFailedPostActivation = false,
                                                                bitReadyForReActivation = false,
                                                                intIdPlan = request.ActivationInfo.LineInfo.PlanId,
                                                                txtICCID = pho.iccid,
                                                                txtPhoneNumber = "",
                                                                txtResetStatus = "Not Applicable",
                                                                txtStatus = "Request Refund from Contel - Duplicate ICCID",
                                                                dteActivation = DateTime.Now
                                                            };
                                                            ctx.tblInternationActivationPool.Add(dataPool);
                                                            ctx.SaveChanges();

                                                            int poolId = dataPool.intId;

                                                            ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                            {
                                                                intActivationPoolId = poolId,
                                                                txtICCID = pho.iccid,
                                                                txtPhone = "",
                                                                txtStatus = "Request Refund from Contel - Duplicate ICCID",
                                                                dteAction = DateTime.Now,
                                                                txtDoneBy = "System"
                                                            });
                                                            ctx.SaveChanges();
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    string msgAdm = string.Format("*Failed Activation due to error from Contel: {0}*\n\n" +
                                                                                "ICCID: {1} \n\n" + "Plan: {2} \n\n" +
                                                                                "Error: {3} \n\n"
                                                                                , person1.txtName, newICCID, amount.txtPlanName, response.mensagem);

                                                    var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };

                                                    LogHelper.LogMessage(intIDPerson, string.Format("FacilAccess:ActivateESimNew: Activation failed with reason: {0} for User: {1}", response.mensagem, intIDPerson), idUnique);

                                                    if (!response.retorno && response.mensagem == "CPF n\u00e3o \u00e9 v\u00e1lido na base da receita."
                                                        || response.mensagem == "CPF não é válido na base da receita."
                                                        || response.mensagem == "CNPJ n\u00e3o \u00e9 v\u00e1lido na base da receita."
                                                        || response.mensagem == "CNPJ não é válido na base da receita.")
                                                        actRes.Error = "Activation failed with reason: Not a valid CPF at Internal Revenue Service database.";
                                                    else
                                                        actRes.Error = "Activation failed with reason:" + response.mensagem;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            actRes.Error = "eSIMs unavailable";
                                            LogHelper.LogMessage(intIDPerson, "ActivateESimNew: Exit : eSIMs not available now", idUnique);

                                            string msgAdm = string.Format("*Failed Activation due to eSIMs not available now, Please sync esims: {0}*\n\n", person1.txtName);
                                            var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                        }
                                    }
                                    else
                                    {
                                        actRes.Error = "Client details not available, Please check with Facil.tel";
                                        string msgAdm = string.Format("*Failed Activation due to Client details not available, Please check with Facil.tel: {0}*\n\n", person1.txtName);
                                        var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                                    }
                                }
                            }
                            else
                            {
                                actRes.Error = "You do not have enough balance available to activate a new line.  Please contact +5521982008200 for more details.";
                                string msgAdm = string.Format(person1.txtName + " do not have enough balance available to activate a new line. Hence cancelling sale");
                                var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
                            }
                        }
                        else
                        {
                            actRes.Error = "We have encountered a temporary but and will try to resolve it in the next 30 minutes.  Please contact +5521982008200 if it is not resolved in 30 minutes.";
                        }
                    }
                    else
                    {
                        actRes.Error = "Requested PlanId doesn't exist.  Please contact +5521982008200.";
                    }

                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESimNew: Exit : New activation from {0} for plan {1}", intIDPerson, amount.txtPlanName), idUnique);

                    TimeSpan timeTaken1 = watch.Elapsed;
                    watch.Stop();
                    LogHelper.LogMessage(intIDPerson, string.Format("ActivateESimNew: Total time taken in seconds :{0} for {1}", timeTaken1.TotalSeconds, intIDPerson), idUnique);
                }
            }
            catch (Exception ex)
            {
                actRes.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:ActivateESimNew: Error" + ex.ToString(), idUnique);
                TimeSpan timeTaken1 = watch.Elapsed;
                watch.Stop();
                LogHelper.LogMessage(intIDPerson, string.Format("ActivateESimNew: Total time taken in seconds : ", intIDPerson, timeTaken1.TotalSeconds), idUnique);

                string msgAdm = string.Format("*Activation Failed due to runtime error* : \n{1}", ex.ToString());
                var tasks = new[] { System.Threading.Tasks.Task.Run(() => new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm)) };
            }
            return actRes;
        }

        public bool DepositBalance(FacilUpdateBalanceRequest request)
        {
            try
            {
                if (request != null && request.FinalValue != null && request.DateAdded != null)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        DateTime dteAdded = DateTime.Now; try
                        {
                            dteAdded = DateTime.ParseExact(request.DateAdded, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex) { }

                        ctx.tblInternationalDeposits.Add(new tblInternationalDeposits()
                        {
                            intIdPerson = request.IdPerson,
                            intIdPaymentType = request.PaymentType,
                            dteDateAdded = dteAdded,
                            intUSDAmount = Convert.ToDecimal(request.USDAmount, CultureInfo.InvariantCulture),
                            intBRAmount = Convert.ToDecimal(request.BRAmount, CultureInfo.InvariantCulture),
                            txtHandlingCharges = request.HandlingCharges,
                            txtBankCharges = request.BankCharges,
                            txtCCCharges = request.CCCharages,
                            txtConversionRate = Convert.ToDecimal(request.ConversionRate, CultureInfo.InvariantCulture),
                            intFinalValue = Convert.ToDecimal(request.FinalValue, CultureInfo.InvariantCulture),
                            txtComment = request.Comment,
                            bitRefund = request.Refund
                        });
                        ctx.SaveChanges();
                        LogHelper.LogMessage(request.IdPerson, "FacilAccess:DepositBalance:tblInternationalDeposits updated for user:" + request.IdPerson);

                        var currentBal = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == request.IdPerson);
                        if (currentBal is null)
                        {
                            ctx.tblInternationalUserBalance.Add(new tblInternationalUserBalance()
                            {
                                intIdPerson = request.IdPerson,
                                intAmountBalance = Convert.ToDecimal(request.FinalValue, CultureInfo.InvariantCulture),
                                dteAdded = dteAdded
                            });
                        }
                        else
                        {
                            currentBal.intAmountBalance += Convert.ToDecimal(request.FinalValue, CultureInfo.InvariantCulture);
                            currentBal.dteUpdated = dteAdded;
                        }
                        ctx.SaveChanges();
                    }
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(request.IdPerson, "FacilAccess:DepositBalance:" + ex.ToString());
                return false;
            }
            return true;
        }

        public FacilResetLineResponse ResetLine(FacilResetLineRequest request)
        {
            FacilResetLineResponse resetResponse = new FacilResetLineResponse() { Status = false };
            int userId = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    userId = GetUserIdFromToken(request.ApiKey);
                    var phones = ctx.tblPersonsPhones.Where(x => string.Concat(x.intDDD, x.intPhone) == request.LineInfo.Phone.PhoneNumber && x.intIdPerson == userId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value).FirstOrDefault();
                    if (phones != null)
                    {
                        ResetLine resetReq = new ResetLine()
                        {
                            linha = request.LineInfo.Phone.PhoneNumber,
                            motivo = string.IsNullOrEmpty(request.LineInfo.Reason) ? "Troca para eSIM" : FacilUtil.GetStatusMapping(request.LineInfo.Reason),
                            novo_iccid = ""
                        };
                        LogHelper.LogMessage(userId, "Recieved reset request for line: " + request.LineInfo.Phone.PhoneNumber);
                        var result = new MVNOAccess().ResetLine(resetReq, request.Enviroment.ToString());
                        if (result != null && !string.IsNullOrEmpty(result.message))
                        {
                            if (string.Equals(result.message, "Troca de chip realizada com sucesso.", StringComparison.OrdinalIgnoreCase))
                            {
                                string iccid = resetResponse.ICCID, activationcode = string.Empty;
                                var qrcode = new MVNOAccess().DownloadActivationFileContel(result.esim_pdf, request.LineInfo.Phone.PhoneNumber, ref iccid, ref activationcode);
                                var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == request.LineInfo.Phone.PhoneNumber && x.txtICCID == iccid);
                                if (tblEsim != null)
                                {
                                    tblEsim.txtActivationCode = activationcode;
                                    tblEsim.txtActivationDate = DateTime.Now.ToString("yyyy-MM-dd");
                                    tblEsim.txtActivationImage = qrcode;
                                    tblEsim.txtActivationPdfUrl = result.esim_pdf;
                                    tblEsim.txtICCID = iccid;
                                    tblEsim.txtLinha = request.LineInfo.Phone.PhoneNumber;
                                    tblEsim.dteInsert = DateTime.Now;
                                }
                                else
                                {
                                    ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                    {
                                        txtActivationCode = activationcode,
                                        txtActivationDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                        txtActivationImage = qrcode,
                                        txtActivationPdfUrl = result.esim_pdf,
                                        txtICCID = iccid,
                                        txtLinha = request.LineInfo.Phone.PhoneNumber,
                                        dteInsert = DateTime.Now
                                    });
                                }
                                ctx.SaveChanges();

                                resetResponse.Status = true;
                                resetResponse.ICCID = iccid;
                                resetResponse.ActivationPDF = PdfUrl + request.LineInfo.Phone.PhoneNumber + "/" + result.iccid;
                                resetResponse.ActivationCode = activationcode;

                                LogHelper.LogMessage(userId, string.Format("Reset request for line: {0} Resetted Successfully", request.LineInfo.Phone.PhoneNumber));
                            }
                            else
                            {
                                resetResponse.Status = false;
                                resetResponse.Error = FacilUtil.GetStatusMapping(result.message);
                            }
                        }
                        else
                        {
                            resetResponse.Status = false;
                            resetResponse.Error = InternalError;
                            LogHelper.LogMessage(userId, string.Format("Reset request for line: {0} Resetted Successfully", request.LineInfo.Phone.PhoneNumber));
                        }
                    }
                    else
                    {
                        resetResponse.Status = false;
                        resetResponse.Error = string.Format("Phone: {0} unavailable, Please contact Facil.tel", request.LineInfo.Phone.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                resetResponse.Status = false;
                resetResponse.Error = InternalError;
                LogHelper.LogMessage(userId, "FacilAccess:ResetLine:" + ex.ToString());
            }
            return resetResponse;
        }

        public FacilHistoryResponse GetHistory(string token)
        {
            FacilHistoryResponse historyResponse = new FacilHistoryResponse();
            int intIDPerson = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    intIDPerson = FacilAccess.GetUserIdFromToken(token);

                    var histories = ctx.tblInternationalUserPurchases.Where(x => x.intIdPerson == intIDPerson && !x.bitTest.Value).OrderByDescending(y => y.dteDeducted).ToList();

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
                        historyResponse.Error = InternalError;
                    }
                }
            }
            catch (Exception ex)
            {
                historyResponse.Status = false;
                historyResponse.Error = InternalError;
                LogHelper.LogMessage(intIDPerson, "FacilAccess:GetBalance:" + ex.ToString());
            }
            return historyResponse;
        }

        public void ResetPhoneForTopup(int countTotake)
        {
            MVNOAccess mvno = new MVNOAccess();
            try
            {
                LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup request for count: {0}", countTotake));
                using (var ctx = new FoneClubeContext())
                {
                    var expData = ctx.tblPhonesReadyToReset.OrderBy(x => x.dteActivated).Where(y => y.bitResetSuccess == false && !y.dteUpdate.HasValue).Take(countTotake).ToList();
                    if (expData != null && expData.Count() > 0)
                    {
                        foreach (var exp in expData)
                        {
                            System.Threading.Thread.Sleep(2000);

                            LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup GetContelTopupHistory for line : {0}", exp.txtPhone));

                            var topupHistory = mvno.GetContelTopupHistory(exp.txtPhone);
                            if (topupHistory != null && topupHistory.retorno && topupHistory.historico != null && topupHistory.historico.Count > 0 && topupHistory.historico[0] != null)
                            {
                                var lastTopupDate = DateTime.ParseExact(topupHistory.historico[0].data_recarga, "yyyy-MM-dd HH:mm:ss.fff",
                                                                  System.Globalization.CultureInfo.InvariantCulture);
                                var DaysSinceLastTopup = Convert.ToInt32((DateTime.Now - lastTopupDate).TotalDays);
                                LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup DaysSinceLastTopup for line : {0}", DaysSinceLastTopup));
                                if (DaysSinceLastTopup > 30)
                                {
                                    var getLine = mvno.GetContelLinesByPhoneLite(exp.txtPhone);
                                    if (getLine != null)
                                    {
                                        ResetLine resetReq = new ResetLine()
                                        {
                                            linha = exp.txtPhone,
                                            motivo = "Troca para eSIM",
                                            novo_iccid = ""
                                        };
                                        var resetRes = mvno.ResetLine(resetReq);
                                        if (resetRes != null && string.Equals(resetRes.message, "Troca de chip realizada com sucesso.", StringComparison.OrdinalIgnoreCase))
                                        {
                                            LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup Reset success for line : {0} New iccid:{1}", exp.txtPhone, resetRes.iccid));

                                            var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.intId == exp.intId);
                                            if (resetSuccess != null)
                                            {
                                                resetSuccess.bitResetSuccess = true;
                                                resetSuccess.dteUpdate = DateTime.Now;
                                                ctx.SaveChanges();
                                            }

                                            var ctxPool = ctx.tblInternationActivationPool.Any(x => x.txtPhoneNumber == exp.txtPhone && x.txtResetICCID == resetRes.iccid);
                                            if (!ctxPool)
                                            {
                                                DateTime activationd = exp.dteActivated;
                                                try
                                                {
                                                    activationd = DateTime.ParseExact(getLine.data_inicio_plano, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                                                }
                                                catch (Exception) { }

                                                var poolFF = new tblInternationActivationPool()
                                                {
                                                    bitFailedPostActivation = false,
                                                    bitReadyForReActivation = true,
                                                    dteActivation = activationd,
                                                    dteReset = DateTime.Now,
                                                    intIdPerson = 1,
                                                    intIdPersonReset = 1,
                                                    txtICCID = getLine.iccid,
                                                    txtResetICCID = resetRes.iccid,
                                                    txtPhoneNumber = exp.txtPhone,
                                                    txtResetStatus = "Pending",
                                                    txtStatus = "Resell Pool + Topup",
                                                };
                                                ctx.tblInternationActivationPool.Add(poolFF);
                                                ctx.SaveChanges();

                                                int poolId = poolFF.intId;

                                                ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                {
                                                    intActivationPoolId = poolId,
                                                    txtICCID = resetRes.iccid,
                                                    txtPhone = exp.txtPhone,
                                                    txtStatus = "Resell Pool + Topup",
                                                    dteAction = DateTime.Now,
                                                    txtDoneBy = "System"
                                                });
                                                ctx.SaveChanges();

                                                var esimPool = ctx.tblESimICCIDPool.FirstOrDefault(x => x.txtICCID == resetRes.iccid);
                                                if (esimPool != null)
                                                {
                                                    esimPool.bitActivated = true;
                                                    esimPool.dteUpdate = DateTime.Now;
                                                    ctx.SaveChanges();
                                                }
                                                LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup step complete for line : {0} New iccid:{1}", exp.txtPhone, resetRes.iccid));
                                            }
                                        }
                                        else
                                        {
                                            var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.intId == exp.intId);
                                            if (resetSuccess != null)
                                            {
                                                resetSuccess.bitResetSuccess = false;
                                                resetSuccess.txtError = resetRes != null ? resetRes.message : "Reset error";
                                                resetSuccess.dteUpdate = DateTime.Now;
                                                ctx.SaveChanges();
                                            }

                                            //string msg = "*Attention: Auto Reset failure*\nPhone:" + exp.txtPhone + "\nError:" + resetRes.message;
                                            //new WhatsAppAccess().SendMessageInfoToAdmin(msg);
                                        }
                                    }
                                    else
                                    {
                                        var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.intId == exp.intId);
                                        if (resetSuccess != null)
                                        {
                                            resetSuccess.bitResetSuccess = false;
                                            resetSuccess.txtError = "Line not found";
                                            resetSuccess.dteUpdate = DateTime.Now;
                                            ctx.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.intId == exp.intId);
                                    if (resetSuccess != null)
                                    {
                                        resetSuccess.bitResetSuccess = false;
                                        resetSuccess.txtError = "Topup found less than 30 days";
                                        resetSuccess.dteUpdate = DateTime.Now;
                                        ctx.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.intId == exp.intId);
                                if (resetSuccess != null)
                                {
                                    resetSuccess.bitResetSuccess = false;
                                    resetSuccess.txtError = "No topup history found";
                                    resetSuccess.dteUpdate = DateTime.Now;
                                    ctx.SaveChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup no data found"));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(1, string.Format("ResetPhoneForTopup error: " + ex.ToString()));
            }
        }
    }

    public class FacilUtil
    {
        public static object BadRequest()
        {
            return new FacilGenericResponse()
            {
                Status = false,
                Error = "Invalid Request"
            };
        }

        public static object InvalidToken()
        {
            return new FacilGenericResponse()
            {
                Status = false,
                Error = "Invalid Token"
            };
        }

        public static object ValidToken()
        {
            return new FacilGenericResponse()
            {
                Status = true,
                Info = "Valid Token"
            };
        }

        public static string GetStatusMapping(string status)
        {
            string stat = string.Empty;
            switch (status)
            {
                case "ATIVO":
                    stat = "ACTIVE";
                    break;
                case "INATIVA":
                    stat = "INACTIVE";
                    break;
                case "CANCELADO":
                    stat = "CANCELLED";
                    break;
                case "PRÉ PAGO":
                    stat = "PREPAID";
                    break;
                case "Linha já bloqueada.":
                    stat = "Line is already blocked";
                    break;
                case "Desbloqueio já realizado.":
                    stat = "Line is already unblocked";
                    break;
                case "Lost":
                    stat = "Perda";
                    break;
                case "Linha nu00e3o encontrada.":
                    stat = "Line Not Found";
                    break;
                case "Envie o ICCID.":
                    stat = "Send ICCID";
                    break;
                default:
                    stat = status;
                    break;
            }
            return stat;
        }
    }
}
