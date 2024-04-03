using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Utils;
using HttpService;
using IronBarCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagarMe;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static FoneClube.DataAccess.pagarme.PagarmeAccess;
using QRCode = FoneClube.Business.Commons.Utils.QRCode;
using BUtils = Business.Commons.Utils;
using FoneClube.DataAccess.Utilities;
using System.Transactions;

namespace FoneClube.DataAccess
{
    public class TransactionAccess
    {
        public bool UpdateTransactions(List<Transaction> transactions)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var listaTransaction = new List<tblFoneclubePagarmeTransactions>();
                    var count = 0;
                    try
                    {
                        MVNOAccess mvnoAccess = new MVNOAccess();
                        mvnoAccess.SyncAllLinesFromContel();

                        foreach (var transaction in transactions)
                        {
                            count++;
                            int? idCustomer = 0;
                            long transId = Convert.ToInt64(transaction.Id);
                            try
                            {
                                idCustomer = Convert.ToInt32(transaction.Customer.Id);
                            }
                            catch (Exception)
                            {
                                idCustomer = null;
                            }

                            var registro = new tblFoneclubePagarmeTransactions
                            {
                                txtOutdadetStatus = transaction.Status.ToString(),
                                txtAcquirer_id = transaction.AcquirerResponseCode,
                                dteDate_created = transaction.DateCreated,
                                dteDate_updated = transaction.DateUpdated,
                                intAmount = transaction.Amount,
                                intAuthorized_amount = transaction.AuthorizationAmount,
                                intPaid_amount = transaction.PaidAmount,
                                intInstallments = transaction.Installments,
                                intIdTransaction = transId,
                                intCost = transaction.Cost,
                                txtPayment_method = transaction.PaymentMethod.ToString(),
                                txtBoleto_url = transaction.BoletoUrl,
                                txtBoleto_barcode = transaction.BoletoBarcode,
                                txtCard_holder_name = transaction.CardHolderName,
                                txtCard_first_digits = transaction.CardFirstDigits,
                                txtCard_last_digits = transaction.CardLastDigits,
                                dteBoleto_expiration_date = transaction.BoletoExpirationDate,
                                intIdCustomer = !bool.Equals(idCustomer, null) ? idCustomer : 0,
                                txtPixCode = transaction.PixQrCode,
                                dtePix_expiration_date = transaction.PixExpirationDate
                            };

                            //try
                            //{
                            //    registro.intTid = int.TryParse(transaction.Tid, out int tid) ? tid : 0;
                            //    registro.intNsu = int.TryParse(transaction.Nsu, out int nsu) ? nsu : 0;
                            //}
                            //catch (Exception)
                            //{
                            //    registro.intTid = null;
                            //    registro.intNsu = null;
                            //}

                            if (transaction.Status == TransactionStatus.Paid)
                            {
                                ProfileAccess profile = new ProfileAccess();

                                var msgNotSendBefore = !ctx.tblPagarmeTransactionsUserUpdateStatus.Any(x => x.intIdTransaction == transId);
                                if (msgNotSendBefore)
                                {
                                    WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                                    whatsAppAccess.SendPaymentConfirmationMsgs(transId, idCustomer.HasValue ? idCustomer.Value : 0);

                                    //MVNO plan activation
                                    profile.ActivateMVNOPlansPostPayment(transId);

                                    new MVNOAccess().AutoUnBlockLineByCustomer(transId);
                                }

                                // Store Activation
                                var storeplans = ctx.tblStoreCustomerPlans.Where(x => x.intIdTransaction == transId && !x.bitActivated).FirstOrDefault();
                                if (storeplans != null)
                                {
                                    profile.ActivateStoreEsim(transId);
                                    profile.DeliverStoreSim(transId);
                                    profile.UpdateOrderStatus(transId);
                                }
                            }

                            listaTransaction.Add(registro);
                        }
                    }
                    catch (Exception e)
                    {
                        var teste = 1;
                    }


                    ctx.tblFoneclubePagarmeTransactions.AddRange(listaTransaction);

                    ctx.Database
                        .ExecuteSqlCommand("truncate table tblFoneclubePagarmeTransactions");

                    ctx.SaveChanges();

                    try
                    {
                        var lista = new List<tblFoneclubePagarmeTransactionsSecond>();
                        var novasTransacoes = ctx.GetNewTransactionsPagarme().ToList();
                        foreach (var transacao in novasTransacoes)
                        {
                            var registro = new tblFoneclubePagarmeTransactionsSecond
                            {
                                txtOutdadetStatus = transacao.txtOutdadetStatus,
                                txtAcquirer_id = transacao.txtAcquirer_id,
                                dteDate_created = transacao.dteDate_created,
                                dteDate_updated = transacao.dteDate_updated,
                                intAmount = transacao.intAmount,
                                intAuthorized_amount = transacao.intAuthorized_amount,
                                intPaid_amount = transacao.intPaid_amount,
                                intInstallments = transacao.intInstallments,
                                intIdTransaction = Convert.ToInt64(transacao.intIdTransaction),
                                intCost = transacao.intCost,
                                txtPayment_method = transacao.txtPayment_method.ToString(),
                                txtBoleto_url = transacao.txtBoleto_url,
                                txtBoleto_barcode = transacao.txtBoleto_barcode,
                                txtCard_holder_name = transacao.txtCard_holder_name,
                                txtCard_first_digits = transacao.txtCard_first_digits,
                                txtCard_last_digits = transacao.txtCard_last_digits,
                                dteBoleto_expiration_date = transacao.dteBoleto_expiration_date,
                                intIdCustomer = transacao.intIdCustomer,
                                txtPixCode = transacao.txtPixCode,
                                dtePix_expiration_date = transacao.dtePix_expiration_date
                            };

                            lista.Add(registro);
                        }

                        ctx.tblFoneclubePagarmeTransactionsSecond.AddRange(lista);
                        ctx.SaveChanges();

                    }
                    catch (Exception) { }

                    try
                    {
                        ctx.Database
                        .ExecuteSqlCommand("truncate table tblPastPagarmeUpdate");

                        ctx.tblPastPagarmeUpdate.Add(new tblPastPagarmeUpdate { dteUpdate = DateTime.Now });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }


                    try
                    {
                        #region Missing Transactions restore
                        var startDate = new DateTime(2022, 01, 01);
                        var chargeHistTrans = ctx.tblChargingHistory.Select(x => x.intIdTransaction).Distinct();
                        var foneClubeTrans = ctx.tblFoneclubePagarmeTransactions.Where(y => y.dteDate_updated > startDate).Select(x => x.intIdTransaction).Distinct();
                        var allUsers = ctx.tblPersons.ToList();

                        var lstNotFoundTrans = foneClubeTrans.Except(chargeHistTrans).ToList();

                        var nonWebFcTrans = ctx.tblFoneclubePagarmeTransactions.Where(x => lstNotFoundTrans.Any(y => y == x.intIdTransaction)).ToList();

                        if (nonWebFcTrans != null && nonWebFcTrans.Count > 0)
                        {
                            int iaddedCount = 0;
                            foreach (var tran in nonWebFcTrans)
                            {
                                var user = allUsers.Where(x => x.intIdPagarme == tran.intIdCustomer).FirstOrDefault();
                                if (user != null)
                                {
                                    ctx.tblLog.Add(new tblLog
                                    {
                                        dteTimeStamp = DateTime.Now,
                                        intIdTipo = 1,
                                        txtAction = "User found:" + user.intIdPerson
                                    });
                                    ctx.SaveChanges();

                                    var charge = new tblChargingHistory()
                                    {
                                        bitActive = true,
                                        dtePayment = tran.dteDate_created.Value,
                                        dteChargingDate = tran.dteDate_created,
                                        dteCreate = tran.dteDate_created,
                                        dteValidity = tran.dteDate_created,
                                        dteDueDate = tran.dtePix_expiration_date.HasValue ? tran.dtePix_expiration_date.Value.AddYears(-1) : tran.dteDate_created,
                                        dteExpiryDate = tran.dtePix_expiration_date.HasValue ? tran.dtePix_expiration_date.Value : (DateTime?)null,
                                        intIdTransaction = tran.intIdTransaction,
                                        intIdCustomer = user.intIdPerson,
                                        intIdPaymentType = tran.txtPayment_method == "CreditCard" ? 1 : tran.txtPayment_method == "Boleto" ? 2 : 3,
                                        pixCode = tran.txtPixCode,
                                        txtAmmountPayment = tran.intAmount.ToString(),
                                        txtChargingComment = "Recovered",
                                        txtAcquireId = tran.txtAcquirer_id,
                                        intChargeStatusId = 1,
                                        txtComment = "🤖 FoneClube: FoneBot \nPrezado *namevariable*,\nSegue resumo da sua última cobrança que que será enviada por email e whatsapp.  \n*Detalhes.Cobrança* \nVigencia: *vigenciavariable* \nVencimento: *vencimentovariable* \nTotal:*R$ amountvariable* \nCaso tenha alguma dúvida envie um* whatsapp para *\n *${ 'https://wa.me/5521981908190'}* \nou email para\n*financeiro@foneclube.com.br *.\nObrigado pela Atenção:\n*FoneClube *  👍",
                                        txtCommentEmail = "🤖 FoneClube: FoneBot \nPrezado *namevariable*,\nSegue resumo da sua última cobrança que que será enviada por email e whatsapp.  \n*Detalhes.Cobrança* \nVigencia: *vigenciavariable* \nVencimento: *vencimentovariable* \nTotal:*R$ amountvariable* \nCaso tenha alguma dúvida envie um* whatsapp para *\n *${ 'https://wa.me/5521981908190'}* \nou email para\n*financeiro@foneclube.com.br *.\nObrigado pela Atenção:\n*FoneClube *  👍"
                                    };
                                    iaddedCount++;

                                    ctx.tblChargingHistory.Add(charge);
                                    ctx.SaveChanges();

                                    ctx.tblLog.Add(new tblLog
                                    {
                                        dteTimeStamp = DateTime.Now,
                                        intIdTipo = 1,
                                        txtAction = "Charge created for " + user.intIdPerson
                                    });
                                    ctx.SaveChanges();
                                }
                            }
                            ctx.tblLog.Add(new tblLog
                            {
                                dteTimeStamp = DateTime.Now,
                                intIdTipo = 1,
                                txtAction = "Recovered charges from Pagarme total count:" + nonWebFcTrans.Count + " added count:" + iaddedCount
                            });
                            ctx.SaveChanges();
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = e.ToString()
                        });
                        ctx.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public bool UpdateTransactionsById(Transaction transaction)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        int? idCustomer = 0;

                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = string.Format("Refressing transaction for id: {0}, amount: {1}", transaction.Id, transaction.Amount),
                        });
                        ctx.SaveChanges();

                        try
                        {
                            idCustomer = Convert.ToInt32(transaction.Customer.Id);
                        }
                        catch (Exception)
                        {
                            idCustomer = null;
                        }

                        var registro = new tblFoneclubePagarmeTransactions
                        {
                            txtOutdadetStatus = transaction.Status.ToString(),
                            txtAcquirer_id = transaction.AcquirerResponseCode,
                            dteDate_created = transaction.DateCreated,
                            dteDate_updated = transaction.DateUpdated,
                            intAmount = transaction.Amount,
                            intAuthorized_amount = transaction.AuthorizationAmount,
                            intPaid_amount = transaction.PaidAmount,
                            intInstallments = transaction.Installments,
                            intIdTransaction = Convert.ToInt64(transaction.Id),
                            intCost = transaction.Cost,
                            txtPayment_method = transaction.PaymentMethod.ToString(),
                            txtBoleto_url = transaction.BoletoUrl,
                            txtBoleto_barcode = transaction.BoletoBarcode,
                            txtCard_holder_name = transaction.CardHolderName,
                            txtCard_first_digits = transaction.CardFirstDigits,
                            txtCard_last_digits = transaction.CardLastDigits,
                            dteBoleto_expiration_date = transaction.BoletoExpirationDate,
                            intIdCustomer = !bool.Equals(idCustomer, null) ? idCustomer : 0,
                            txtPixCode = transaction.PixQrCode,
                            dtePix_expiration_date = transaction.PixExpirationDate
                        };


                        var tblTrans = ctx.tblFoneclubePagarmeTransactions.Where(x => x.intIdTransaction.Value.ToString() == transaction.Id).FirstOrDefault();
                        if (tblTrans != null)
                        {
                            ctx.tblLog.Add(new tblLog
                            {
                                dteTimeStamp = DateTime.Now,
                                intIdTipo = 1,
                                txtAction = string.Format("Refressing transaction for id: {0}, amount: {1}, already exists", transaction.Id, transaction.Amount),
                            });

                            tblTrans.txtOutdadetStatus = transaction.Status.ToString();
                            tblTrans.txtAcquirer_id = transaction.AcquirerResponseCode;
                            tblTrans.dteDate_created = transaction.DateCreated;
                            tblTrans.dteDate_updated = transaction.DateUpdated;
                            tblTrans.intAmount = transaction.Amount;
                            tblTrans.intAuthorized_amount = transaction.AuthorizationAmount;
                            tblTrans.intPaid_amount = transaction.PaidAmount;
                            tblTrans.intInstallments = transaction.Installments;
                            tblTrans.intIdTransaction = Convert.ToInt64(transaction.Id);
                            tblTrans.intCost = transaction.Cost;
                            tblTrans.txtPayment_method = transaction.PaymentMethod.ToString();
                            tblTrans.txtBoleto_url = transaction.BoletoUrl;
                            tblTrans.txtBoleto_barcode = transaction.BoletoBarcode;
                            tblTrans.txtCard_holder_name = transaction.CardHolderName;
                            tblTrans.txtCard_first_digits = transaction.CardFirstDigits;
                            tblTrans.txtCard_last_digits = transaction.CardLastDigits;
                            tblTrans.dteBoleto_expiration_date = transaction.BoletoExpirationDate;
                            tblTrans.intIdCustomer = !bool.Equals(idCustomer, null) ? idCustomer : 0;
                            tblTrans.txtPixCode = transaction.PixQrCode;
                            tblTrans.dtePix_expiration_date = transaction.PixExpirationDate;
                        }
                        else
                        {
                            ctx.tblFoneclubePagarmeTransactions.Add(registro);
                            ctx.tblLog.Add(new tblLog
                            {
                                dteTimeStamp = DateTime.Now,
                                intIdTipo = 1,
                                txtAction = string.Format("Refressing transaction for id: {0}, amount: {1}, adding new", transaction.Id, transaction.Amount),
                            });
                        }

                        ctx.SaveChanges();

                        if (transaction.Status == TransactionStatus.Paid)
                        {
                            ProfileAccess profile = new ProfileAccess();
                            MVNOAccess mvnoAccess = new MVNOAccess();
                            long transId = Convert.ToInt64(transaction.Id);
                            var msgNotSendBefore = !ctx.tblPagarmeTransactionsUserUpdateStatus.Any(x => x.intIdTransaction == Convert.ToInt64(transaction.Id));
                            if (msgNotSendBefore)
                            {
                                WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                                whatsAppAccess.SendPaymentConfirmationMsgs(transId, idCustomer.HasValue ? idCustomer.Value : 0);

                                //MVNO plan activation
                                profile.ActivateMVNOPlansPostPayment(transId);

                                mvnoAccess.SyncAllLinesFromContel();
                                mvnoAccess.AutoUnBlockLineByCustomer(transId);
                                mvnoAccess.SyncAllLinesFromContel();
                            }

                            // Store Activation
                            var storeplans = ctx.tblStoreCustomerPlans.Where(x => x.intIdTransaction == transId && !x.bitActivated).FirstOrDefault();
                            if (storeplans != null)
                            {
                                profile.ActivateStoreEsim(transId);
                                profile.DeliverStoreSim(transId);
                                profile.UpdateOrderStatus(transId);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        var teste = 1;
                    }

                    try
                    {
                        var lista = new List<tblFoneclubePagarmeTransactionsSecond>();
                        var novasTransacoes = ctx.GetNewTransactionsPagarme().ToList();
                        foreach (var transacao in novasTransacoes)
                        {
                            var registro = new tblFoneclubePagarmeTransactionsSecond
                            {
                                txtOutdadetStatus = transacao.txtOutdadetStatus,
                                txtAcquirer_id = transacao.txtAcquirer_id,
                                dteDate_created = transacao.dteDate_created,
                                dteDate_updated = transacao.dteDate_updated,
                                intAmount = transacao.intAmount,
                                intAuthorized_amount = transacao.intAuthorized_amount,
                                intPaid_amount = transacao.intPaid_amount,
                                intInstallments = transacao.intInstallments,
                                intIdTransaction = Convert.ToInt64(transacao.intIdTransaction),
                                intCost = transacao.intCost,
                                txtPayment_method = transacao.txtPayment_method.ToString(),
                                txtBoleto_url = transacao.txtBoleto_url,
                                txtBoleto_barcode = transacao.txtBoleto_barcode,
                                txtCard_holder_name = transacao.txtCard_holder_name,
                                txtCard_first_digits = transacao.txtCard_first_digits,
                                txtCard_last_digits = transacao.txtCard_last_digits,
                                dteBoleto_expiration_date = transacao.dteBoleto_expiration_date,
                                intIdCustomer = transacao.intIdCustomer,
                                txtPixCode = transacao.txtPixCode,
                                dtePix_expiration_date = transacao.dtePix_expiration_date
                            };

                            lista.Add(registro);
                        }

                        ctx.tblFoneclubePagarmeTransactionsSecond.AddRange(lista);
                        ctx.SaveChanges();

                    }
                    catch (Exception) { }

                    try
                    {
                        ctx.Database
                        .ExecuteSqlCommand("truncate table tblPastPagarmeUpdate");

                        ctx.tblPastPagarmeUpdate.Add(new tblPastPagarmeUpdate { dteUpdate = DateTime.Now });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public bool InsertCheckoutPagarMeLog(TransactionLog transactionLog)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    int ammount = 0;
                    int daysLimit = 0;
                    try
                    {
                        ammount = transactionLog.Checkout.Amount;
                        daysLimit = transactionLog.Checkout.DaysLimit;
                    }
                    catch (Exception)
                    {
                        ammount = -1;
                        daysLimit = -1;
                    }

                    ctx.tblCheckoutPagarMeLog.Add(new tblCheckoutPagarMeLog
                    {
                        txtBoletoInstructions = transactionLog.Checkout.BoletoInstructions,
                        txtNome = transactionLog.Checkout.Nome,
                        txtEmail = transactionLog.Checkout.Email,
                        txtDocumentNumber = transactionLog.Checkout.DocumentNumber,
                        txtStreet = transactionLog.Checkout.Street,
                        txtStreetNumber = transactionLog.Checkout.StreetNumber,
                        txtNeighborhood = transactionLog.Checkout.Neighborhood,
                        txtZipcode = transactionLog.Checkout.Zipcode,
                        txtDdd = transactionLog.Checkout.Ddd,
                        txtNumber = transactionLog.Checkout.Number,
                        txtTransactionId = transactionLog.IdTransaction,
                        txtLinkBoleto = transactionLog.txtLinkBoleto,
                        bitCard = transactionLog.BoletoTransaction,
                        bitBoleto = transactionLog.CartaoTransaction,
                        dteRegistro = DateTime.Now,
                        intTipoLog = transactionLog.TipoLog,
                        intAmount = ammount,
                        intDaysLimit = daysLimit
                    });

                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public DateTime? GetLastTransactionPaid(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var transaction = ctx.tblFoneclubePagarmeTransactions
                        .Where(t => t.intIdCustomer == person.IdPagarme)
                        .ToList().OrderByDescending(d => d.dteDate_updated)
                        .FirstOrDefault(t => t.txtOutdadetStatus == "Paid" || t.intPaid_amount > 0);

                    if (bool.Equals(transaction, null))
                        return null;
                    else
                        return transaction.dteDate_updated;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IOrderedEnumerable<tblFoneclubePagarmeTransactions> GetAllLastTransactionPaid()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblFoneclubePagarmeTransactions
                        .Where(t => t.txtOutdadetStatus == "Paid" || t.intPaid_amount > 0)
                        .ToList()
                        .OrderByDescending(d => d.dteDate_updated);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<tblFoneclubePagarmeTransactions> GetAllLastTransactions()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblFoneclubePagarmeTransactions
                        .ToList()
                        .OrderByDescending(d => d.dteDate_updated).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IOrderedEnumerable<tblFoneclubePagarmeTransactions> GetCustomerLastTransactionPaid(int pagarmeCustomerId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblFoneclubePagarmeTransactions
                        .Where(t => t.txtOutdadetStatus == "Paid" || t.intPaid_amount > 0 || t.intIdCustomer == pagarmeCustomerId)
                        .ToList()
                        .OrderByDescending(d => d.dteDate_updated);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IOrderedEnumerable<tblFoneclubePagarmeTransactions> GetCustomerLastTransactionPaid(int pagarmeCustomerId, List<tblFoneclubePagarmeTransactions> tblFoneclubePagarmeTransactions)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return tblFoneclubePagarmeTransactions
                        .Where(t => t.txtOutdadetStatus == "Paid" && t.intPaid_amount > 0 && t.intIdCustomer == pagarmeCustomerId)
                        .ToList()
                        .OrderByDescending(d => d.dteDate_updated);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<tblFoneclubePagarmeTransactions> GetLastTransactions(List<tblChargingHistory> tblCharges)
        {
            try
            {
                var idsPagarme = tblCharges.Select(c => c.intIdTransaction).ToList();
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblFoneclubePagarmeTransactions
                        .Where(t => idsPagarme.Contains(t.intIdTransaction))
                        .ToList()
                        .OrderByDescending(d => d.dteDate_updated).ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public DateTime? GetPagarmeUpdateDate()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPastPagarmeUpdate.FirstOrDefault().dteUpdate;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<RetornoTransaction> GetTransactionsSemCliente()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var transactionsSoltas = ctx.GetTransactionsSoltasPagarme().ToList();

                    PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "1000"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", "1"));

                    var listaRestore = new List<RestoreLista>();
                    var transactionsComCliente = new List<Transaction>();
                    var transactionsSemCliente = new List<RetornoTransaction>();

                    var transactions = new List<Transaction>();
                    transactions.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    //var update = new TransactionAccess().UpdateTransactions(transactions)

                    var pessoas = ctx.tblPersons.ToList();
                    //var idsPagarme = transactions
                    //    .Where(a => a.Customer.Id != null)
                    //    .Select(a => new
                    //    {
                    //        Id = a.Customer.Id
                    //    }).ToList();

                    //var docsPagarme = transactions
                    //    .Where(a => a.Customer.Id != null)
                    //    .Select(a => new
                    //    {
                    //        DocumentNumber = a.Customer.DocumentNumber
                    //    }).ToList();



                    try
                    {
                        foreach (var transaction in transactionsSoltas)
                        {
                            var detalhes = transactions.FirstOrDefault(t => t.Id == transaction.intIdTransaction.Value.ToString());

                            if (!bool.Equals(detalhes.Customer, null))
                            {
                                var idPagarmeCliente = detalhes.Customer.Id;

                                var customerFc = pessoas.FirstOrDefault(p => p.intIdPagarme.ToString() == idPagarmeCliente.ToString());
                                if (!bool.Equals(customerFc, null))
                                {
                                    transactionsComCliente.Add(detalhes);

                                    listaRestore.Add(new RestoreLista
                                    {
                                        Transaction = detalhes,
                                        Pessoa = customerFc
                                    });
                                }
                                else
                                {
                                    var documento = detalhes.Customer.DocumentNumber;
                                    var customerDocumentFC = pessoas.FirstOrDefault(p => p.txtDocumentNumber == documento);
                                    if (!bool.Equals(customerDocumentFC, null))
                                    {
                                        transactionsComCliente.Add(detalhes);
                                        listaRestore.Add(new RestoreLista
                                        {
                                            Transaction = detalhes,
                                            Pessoa = customerDocumentFC
                                        });
                                    }
                                    else
                                    {
                                        try
                                        {
                                            transactionsSemCliente.Add(new RetornoTransaction
                                            {
                                                nome = detalhes.Customer.Name,
                                                email = detalhes.Customer.Email,
                                                documento = detalhes.Customer.DocumentNumber,
                                                dataCobranca = detalhes.DateCreated.ToString()
                                            });
                                        }
                                        catch (Exception e)
                                        {
                                            var teste = 1;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    transactionsSemCliente.Add(new RetornoTransaction
                                    {
                                        nome = detalhes.Customer.Name,
                                        email = detalhes.Customer.Email,
                                        documento = detalhes.Customer.DocumentNumber,
                                        dataCobranca = detalhes.DateCreated.ToString()
                                    });
                                }
                                catch (Exception e)
                                {
                                    var teste = 1;
                                }
                            }
                        }

                        return transactionsSemCliente;
                    }
                    catch (Exception e)
                    {
                        return new List<RetornoTransaction>();
                    }


                }
            }
            catch (Exception e)
            {
                return new List<RetornoTransaction>();
            }
        }

        public class RetornoTransaction
        {
            public string nome { get; set; }
            public string email { get; set; }
            public string documento { get; set; }
            public string dataCobranca { get; set; }
        }

        public bool RestoreTransactions()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var transactionsSoltas = ctx.GetTransactionsSoltasPagarme().ToList();

                    PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "1000"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", "1"));

                    var listaRestore = new List<RestoreLista>();
                    var transactionsComCliente = new List<Transaction>();
                    var transactionsSemCliente = new List<Transaction>();

                    var transactions = new List<Transaction>();
                    transactions.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    //var update = new TransactionAccess().UpdateTransactions(transactions)

                    var pessoas = ctx.tblPersons.ToList();
                    //var idsPagarme = transactions
                    //    .Where(a => a.Customer.Id != null)
                    //    .Select(a => new
                    //    {
                    //        Id = a.Customer.Id
                    //    }).ToList();

                    //var docsPagarme = transactions
                    //    .Where(a => a.Customer.Id != null)
                    //    .Select(a => new
                    //    {
                    //        DocumentNumber = a.Customer.DocumentNumber
                    //    }).ToList();



                    try
                    {
                        foreach (var transaction in transactionsSoltas)
                        {
                            var detalhes = transactions.FirstOrDefault(t => t.Id == transaction.intIdTransaction.Value.ToString());

                            if (!bool.Equals(detalhes.Customer, null))
                            {
                                var idPagarmeCliente = detalhes.Customer.Id;

                                var customerFc = pessoas.FirstOrDefault(p => p.intIdPagarme.ToString() == idPagarmeCliente.ToString());
                                if (!bool.Equals(customerFc, null))
                                {
                                    transactionsComCliente.Add(detalhes);

                                    listaRestore.Add(new RestoreLista
                                    {
                                        Transaction = detalhes,
                                        Pessoa = customerFc
                                    });
                                }
                                else
                                {
                                    var documento = detalhes.Customer.DocumentNumber;
                                    var customerDocumentFC = pessoas.FirstOrDefault(p => p.txtDocumentNumber == documento);
                                    if (!bool.Equals(customerDocumentFC, null))
                                    {
                                        transactionsComCliente.Add(detalhes);
                                        listaRestore.Add(new RestoreLista
                                        {
                                            Transaction = detalhes,
                                            Pessoa = customerDocumentFC
                                        });
                                    }
                                    else
                                    {
                                        transactionsSemCliente.Add(detalhes);
                                    }
                                }
                            }
                            else
                            {
                                transactionsSemCliente.Add(detalhes);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    var history = new List<tblChargingHistory>();
                    var listaFiltrada = listaRestore.Where(a => a.Transaction.DateCreated >= new DateTime(2018, 10, 1)).ToList();
                    foreach (var item in listaRestore)
                    {
                        var teste = item;

                        if (item.Transaction.DateCreated >= new DateTime(2018, 10, 1))
                        {
                            var chargingHistory = new tblChargingHistory
                            {
                                txtAmmountPayment = item.Transaction.Amount.ToString(),
                                intIdCollector = 1,
                                intIdCustomer = item.Pessoa.intIdPerson,
                                intIdPaymentType = item.Transaction.PaymentMethod == PaymentMethod.CreditCard ? 1 : 2,
                                txtCollectorName = "Restore",
                                txtComment = "Coletado em restauração automática",
                                //txtCommentBoleto = item.Transaction.BoletoInstructions,
                                //txtCommentEmail = string.Empty,
                                //txtTokenTransaction = string.Empty,
                                intIdBoleto = item.Transaction.PaymentMethod == PaymentMethod.CreditCard ? 0 : Convert.ToInt64(item.Transaction.Id),
                                txtAcquireId = item.Transaction.AcquirerName,
                                dteValidity = new DateTime(1999, 9, 9),
                                intChargeStatusId = 1,
                                dteCreate = item.Transaction.DateCreated,
                                bitCash = false,
                                dtePayment = Convert.ToDateTime(item.Transaction.DateCreated),
                                intIdTransaction = Convert.ToInt64(item.Transaction.Id)
                            };

                            history.Add(chargingHistory);
                        }
                    }

                    ctx.tblChargingHistory.AddRange(history);
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public class RestoreLista
        {
            public Transaction Transaction { get; set; }
            public tblPersons Pessoa { get; set; }
        }

        public List<CustomerPagarme> GetClientesApenasPagarme()
        {
            ApiGateway.EndPointApi = "https://api.pagar.me/1/customers/";
            var link = "?count=1000000&api_key=ak_live_fP7ceLSpdBe8gCXGTywVRmC5VTkvN0";


            using (var ctx = new FoneClubeContext())
            {
                var clients = ctx.tblPersons.Distinct().ToList();
                var clientsSemPagarme = ctx.tblPersons.Where(p => p.intIdPagarme == null).Distinct().ToList();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var result = ApiGateway.GetConteudo(link);
                var customers = JsonConvert.DeserializeObject<JArray>(result.Replace("object", "objecta"));
                var pagarmeCustomers = new List<CustomerPagarme>();
                foreach (var customer in customers)
                {
                    var id = customer["id"].Value<int>();
                    var nome = customer["name"].Value<string>();
                    var documentNumber = customer["document_number"].Value<string>();

                    pagarmeCustomers.Add(new CustomerPagarme
                    {
                        ID = id,
                        Name = nome,
                        Document = documentNumber
                    });
                }

                var clientes = ctx.tblPersons.Select(a => a.txtDocumentNumber).ToList();

                //db.Questions.Where(q => !db.QuestionCounters.Any(qc => qc.QuestionsID == q.QuestionsID))

                var clientesSomentePagarme = pagarmeCustomers.Where(c => !clientes.Any(a => a == c.Document)).ToList();
                return clientesSomentePagarme;

            }
        }

        public void GetQrCode()
        {
            new QRCode().GetQRCode();
        }


        public string GetQRCodeBitMap()
        {
            var barcode = BarcodeWriter.CreateBarcode("https://ironsoftware.com/csharp/barcode", BarcodeWriterEncoding.QRCode);
            var image = barcode.Image;
            var imageUrl = barcode.ToDataUrl();
            var imageTag = barcode.ToHtmlTag();


            return imageTag;
        }

        public Image GetQRCodeImage(int chargingId)
        {
            using (var ctx = new FoneClubeContext())
            {
                var code = ctx.tblChargingHistory.FirstOrDefault(c => c.intId == chargingId);

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(code.pixCode, QRCodeGenerator.ECCLevel.Q);
                QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                return (Image)qrCodeImage;
            }

        }


        public JArray GetTodosClientesPagarme()
        {
            ApiGateway.EndPointApi = "https://api.pagar.me/1/customers/";
            var link = "?count=1000000&api_key=ak_live_fP7ceLSpdBe8gCXGTywVRmC5VTkvN0";


            using (var ctx = new FoneClubeContext())
            {
                var clients = ctx.tblPersons.Distinct().ToList();
                var clientsSemPagarme = ctx.tblPersons.Where(p => p.intIdPagarme == null).Distinct().ToList();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var result = ApiGateway.GetConteudo(link);
                var customers = JsonConvert.DeserializeObject<JArray>(result);

                return customers;


            }
        }

        public HttpStatusCode ProcessWebhookMessage(out string transId)
        {
            string json = string.Empty;
            try
            {
                string status = string.Empty;
                transId = string.Empty;
                System.IO.Stream req = HttpContext.Current.Request.InputStream;
                json = new System.IO.StreamReader(req).ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage response: {0}", json));

                    var result = json.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                    if (result != null)
                    {
                        if (result.TryGetValue("current_status", out status))
                        {
                            string transactionId = string.Empty;
                            result.TryGetValue("id", out transactionId);

                            string paymentMode = string.Empty;
                            result.TryGetValue("transaction%5Bpayment_method%5D", out paymentMode);

                            long intIdTrans = Convert.ToInt64(transactionId);
                            transId = transactionId;

                            if (!string.IsNullOrEmpty(status) && status.ToLower() == "paid")
                            {
                                LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage transaction id : {0} paid", transactionId));
                                using (var ctx = new FoneClubeContext())
                                {
                                    var pagarmeLog = ctx.tblPagarmeWebhookStatusLog.Where(x => x.intIdTransaction == intIdTrans).FirstOrDefault();
                                    if (pagarmeLog == null)
                                    {
                                        ctx.tblPagarmeWebhookStatusLog.Add(new tblPagarmeWebhookStatusLog()
                                        {
                                            intIdTransaction = intIdTrans,
                                            dteTimeStamp = DateTime.Now,
                                            txtStatus = "Paid"
                                        });
                                        ctx.SaveChanges();

                                        ProfileAccess profile = new ProfileAccess();
                                        MVNOAccess mvno = new MVNOAccess();
                                        WhatsAppAccess whatsAppAccess = new WhatsAppAccess();

                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage sending payment confirmation for id : {0}", transactionId));
                                        whatsAppAccess.SendPaymentConfirmationMsgs(intIdTrans);

                                        if (paymentMode != "credit_card")
                                        {
                                            LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage ActivateMVNOPlansPostPayment for id : {0}", transactionId));
                                            profile.ActivateMVNOPlansPostPayment(intIdTrans);
                                        }

                                        //MVNO plan activation
                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage SyncAllLinesFromContel for id : {0}", transactionId));
                                        mvno.SyncAllLinesFromContel();
                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage AutoUnBlockLineByCustomer for id : {0}", transactionId));
                                        mvno.AutoUnBlockLineByCustomer(intIdTrans);

                                        // Store Activation
                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage ActivateStoreEsim for id : {0}", transactionId));
                                        profile.ActivateStoreEsim(intIdTrans);
                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage DeliverStoreSim for id : {0}", transactionId));
                                        profile.DeliverStoreSim(intIdTrans);
                                        LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage UpdateOrderStatus for id : {0}", transactionId));
                                        profile.UpdateOrderStatus(intIdTrans);
                                    }
                                }
                            }
                        }
                    }
                }

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                var exMessage = BUtils.Utils.ProcessException(ex);
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = string.Format("ProcessWebhookMessage error message: {0}: JSON:{1}", exMessage, json),
                    });
                    ctx.SaveChanges();
                }

                throw new HttpResponseException(new BUtils.Utils().GetErrorPostMessage(exMessage));
            }
        }

        public void UpdateTransactionPostCharge(string transactionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(transactionId))
                {
                    PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                    PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "100"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", "1"));

                    var transactionsLoop = new List<Transaction>();
                    transactionsLoop.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    if (transactionsLoop.Count() > 0)
                    {
                        var lastTransaction = transactionsLoop.Where(c => c.Id != null && c.Id == transactionId).FirstOrDefault();

                        if (lastTransaction != null)
                        {
                            var result = new TransactionAccess().UpdateTransactionsById(lastTransaction);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        public class CustomerPagarme
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Document { get; set; }
        }
    }
}
