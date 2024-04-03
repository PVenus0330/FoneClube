using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoneClube.DataAccess.Utilities;
using FoneClube.Business.Commons.Entities.FoneClube;

namespace FoneClube.DataAccess
{
    public class FacilIntlAccess
    {
        public FacilActionResponse PerformAction(FacilActionRequest request)
        {
            FacilActionResponse response = new FacilActionResponse();
            int idPerson = 0;
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                if (request != null)
                {
                    response.Id = request.Id;
                    response.Info = new List<string>();

                    // Refund
                    if (request.IsRefund)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var purchases = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.intId == request.Id && x.txtPhone == request.Phone && x.txtICCID == request.ICCID);
                            if (purchases != null)
                            {
                                var client = ctx.tblPersons.Where(x => x.intIdPerson == purchases.intIdPerson).FirstOrDefault();
                                tblContelPlanMapping plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == purchases.intIdPerson && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                if (plan is null)
                                    plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == 1 && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                var saldo = mVNOAccess.GetSaldo(request.Phone);

                                // Saldo less than plan - User consumed some MB - No refund
                                if (saldo != null && saldo.data != null && saldo.data.restante_dados < plan.intDataMB.Value)
                                {
                                    var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtPhoneNumber == request.Phone && x.txtICCID == purchases.txtICCID);
                                    if (poolData is null)
                                    {
                                        ctx.tblInternationActivationPool.Add(new tblInternationActivationPool()
                                        {
                                            intIdPerson = client.intIdPerson,
                                            bitFailedPostActivation = false,
                                            bitReadyForReActivation = false,
                                            intIdPlan = plan.intIdPlan,
                                            txtICCID = purchases.txtICCID,
                                            txtPhoneNumber = purchases.txtPhone,
                                            txtResetStatus = "Not Applicable",
                                            txtStatus = "Refuse Refund: Data Used",
                                            dteActivation = purchases.dteDeducted
                                        });
                                        ctx.SaveChanges();
                                    }
                                    response.Info.Add(string.Format("Refuse Refund: Data Used : {0} MB out of {1} is used by Customer", (plan.intDataMB.Value / 1024), (saldo.data.restante_dados / 1024)));
                                }
                                else
                                {
                                    //// 2 days old
                                    ctx.tblInternationalDeposits.Add(new tblInternationalDeposits()
                                    {
                                        bitRefund = true,
                                        dteDateAdded = DateTime.Now,
                                        intFinalValue = purchases.intAmountDeducted,
                                        intIdPaymentType = "Refund",
                                        intIdPerson = purchases.intIdPerson,
                                        txtComment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Format("Refunded by FACIL for Phone:{0} on {1}", request.Phone, DateTime.Now.ToShortDateString()),
                                        intUSDAmount = purchases.intAmountDeducted
                                    });
                                    ctx.SaveChanges();

                                    var user = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == client.intIdPerson);
                                    if (user != null)
                                    {
                                        user.intAmountBalance = user.intAmountBalance + purchases.intAmountDeducted;
                                        ctx.SaveChanges();
                                    }

                                    idPerson = purchases.intIdPerson;
                                    purchases.bitRefund = true;
                                    purchases.txtComments = request.Comment;
                                    ctx.SaveChanges();

                                    response.IsRefundSuccess = true;

                                    response.Info.Add("Refund success");
                                }

                            }
                            else
                            {
                                response.Info.Add("Invalid Id passed for refund");
                            }
                        }
                    }

                    // Revoke refund
                    else if (request.IsRevokeRefund)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var purchases = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.intId == request.Id && x.txtPhone == request.Phone && x.txtICCID == request.ICCID && x.bitRefund.HasValue && x.bitRefund.Value);
                            if (purchases != null)
                            {
                                //// 2 days old
                                ctx.tblInternationalDeposits.Add(new tblInternationalDeposits()
                                {
                                    bitRefund = false,
                                    dteDateAdded = DateTime.Now,
                                    intFinalValue = (-purchases.intAmountDeducted),
                                    intIdPaymentType = "Refund Revoke",
                                    intIdPerson = purchases.intIdPerson,
                                    txtComment = !string.IsNullOrEmpty(request.Comment) ? request.Comment : string.Format("Revoked refund by FACIL for Phone:{0} on {1}", request.Phone, DateTime.Now.ToShortDateString()),
                                    intUSDAmount = (-purchases.intAmountDeducted)
                                });
                                ctx.SaveChanges();

                                var client = ctx.tblPersons.Where(x => x.intIdPerson == purchases.intIdPerson).FirstOrDefault();
                                var user = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == client.intIdPerson);
                                if (user != null)
                                {
                                    user.intAmountBalance = (user.intAmountBalance - purchases.intAmountDeducted);
                                    ctx.SaveChanges();
                                }

                                idPerson = purchases.intIdPerson;
                                purchases.bitRefund = false;
                                purchases.txtComments = request.Comment;
                                ctx.SaveChanges();

                                response.IsRevokeRefundSuccess = true;
                                response.Info.Add("Revoke refund success");
                            }
                            else
                            {
                                response.Info.Add("Invalid Id passed or this line is not refunded yet");
                            }
                        }
                    }

                    // Resell Pool and Block line
                    if (request.IsResellPoolAndBlock)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var purchases = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.intId == request.Id && x.txtPhone == request.Phone && x.txtICCID == request.ICCID);
                            var pool = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtPhoneNumber == request.Phone && x.txtICCID == request.ICCID);
                            if (pool is null)
                            {
                                var client = ctx.tblPersons.Where(x => x.intIdPerson == purchases.intIdPerson).FirstOrDefault();
                                tblContelPlanMapping plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == purchases.intIdPerson && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                if (plan is null)
                                    plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == 1 && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                var isLessThan2Days = (DateTime.Now - purchases.dteDeducted).TotalDays <= 2;
                                if (isLessThan2Days)
                                {
                                    try
                                    {
                                        var dataPool = new tblInternationActivationPool()
                                        {
                                            intIdPerson = client.intIdPerson,
                                            bitFailedPostActivation = true,
                                            bitReadyForReActivation = true,
                                            intIdPlan = plan.intIdPlan,
                                            txtICCID = purchases.txtICCID,
                                            txtResetICCID = purchases.txtICCID,
                                            txtPhoneNumber = purchases.txtPhone,
                                            dteReset = DateTime.Now,
                                            intIdPersonReset = 1,
                                            txtResetStatus = "Pending",
                                            txtStatus = "Resell Pool + Blocked",
                                            dteActivation = purchases.dteDeducted
                                        };
                                        ctx.tblInternationActivationPool.Add(dataPool);
                                        ctx.SaveChanges();

                                        int poolId = dataPool.intId;

                                        ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                        {
                                            intActivationPoolId = poolId,
                                            txtICCID = purchases.txtICCID,
                                            txtPhone = purchases.txtPhone,
                                            txtStatus = "Resell Pool + Blocked",
                                            dteAction = DateTime.Now,
                                            txtDoneBy = "System"
                                        });
                                        ctx.SaveChanges();

                                        response.Info.Add("Resell Pool and blocked success");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.LogMessage(purchases.intIdPerson, string.Format("Error adding to Pool: {0}", ex.ToString()));
                                    }
                                }
                                else
                                {
                                    tblInternationActivationPool dataPool = new tblInternationActivationPool()
                                    {
                                        intIdPerson = client.intIdPerson,
                                        bitFailedPostActivation = false,
                                        bitReadyForReActivation = false,
                                        intIdPlan = plan.intIdPlan,
                                        txtICCID = purchases.txtICCID,
                                        txtPhoneNumber = purchases.txtPhone,
                                        txtResetStatus = "Not Applicable",
                                        txtStatus = "Request Refund to Contel",
                                        dteActivation = purchases.dteDeducted,
                                        txtResetICCID = purchases.txtICCID
                                    };
                                    ctx.tblInternationActivationPool.Add(dataPool);
                                    ctx.SaveChanges();

                                    int poolId = dataPool.intId;

                                    ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                    {
                                        intActivationPoolId = poolId,
                                        txtICCID = purchases.txtICCID,
                                        txtPhone = purchases.txtPhone,
                                        txtStatus = "Request Refund to Contel",
                                        dteAction = DateTime.Now,
                                        txtDoneBy = "System"
                                    });
                                    ctx.SaveChanges();

                                    response.Info.Add("Resell Pool failed due to line activated more than 2 days ago, hence requesting refund to contel");
                                }

                                BlockLine blockLine = new BlockLine()
                                {
                                    numero = request.Phone,
                                    motivo = "BLOQUEIO DE IMEI",
                                    observacoes = ""
                                };
                                mVNOAccess.BlockLine(blockLine);
                            }
                        }
                    }

                    if (request.IsReset)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            ResetLine resetReq = new ResetLine()
                            {
                                linha = request.Phone,
                                motivo = "Troca para eSIM",
                                novo_iccid = ""
                            };
                            var resetRes = mVNOAccess.ResetLine(resetReq);
                            if (resetRes != null && string.Equals(resetRes.message, "Troca de chip realizada com sucesso.", StringComparison.OrdinalIgnoreCase))
                            {
                                var resetSuccess = ctx.tblPhonesReadyToReset.FirstOrDefault(x => x.txtICCID == resetRes.iccid && x.txtPhone == request.Phone);
                                if (resetSuccess != null)
                                {
                                    resetSuccess.bitResetSuccess = true;
                                    resetSuccess.dteUpdate = DateTime.Now;
                                    ctx.SaveChanges();
                                }

                                var esimPool = ctx.tblESimICCIDPool.FirstOrDefault(x => x.txtICCID == resetRes.iccid);
                                if (esimPool != null)
                                {
                                    esimPool.bitActivated = true;
                                    esimPool.dteUpdate = DateTime.Now;
                                    ctx.SaveChanges();
                                }
                                response.Info.Add("Reset Success");
                            }
                            else
                            {
                                response.Info.Add("Reset Failed with error: " + resetRes.message);
                            }
                        }
                    }

                    if (request.IsRequestRefundFromContel)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var purchases = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.intId == request.Id && x.txtPhone == request.Phone && x.txtICCID == request.ICCID);
                            if (purchases != null)
                            {
                                var client = ctx.tblPersons.Where(x => x.intIdPerson == purchases.intIdPerson).FirstOrDefault();
                                tblContelPlanMapping plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == purchases.intIdPerson && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                if (plan is null)
                                    plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == 1 && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                                tblInternationActivationPool dataPool = new tblInternationActivationPool()
                                {
                                    intIdPerson = client.intIdPerson,
                                    bitFailedPostActivation = false,
                                    bitReadyForReActivation = false,
                                    intIdPlan = plan.intIdPlan,
                                    txtICCID = purchases.txtICCID,
                                    txtPhoneNumber = purchases.txtPhone,
                                    txtResetStatus = "Not Applicable",
                                    txtStatus = "Request Refund to Contel",
                                    dteActivation = purchases.dteDeducted,
                                    txtResetICCID = purchases.txtICCID
                                };
                                ctx.tblInternationActivationPool.Add(dataPool);
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    response.Info.Add("Invalid request");
                }
            }
            catch (Exception ex)
            {
                response.Info.Add("Generic exception occured: " + ex.ToString());
            }
            return response;
        }
    }
}
