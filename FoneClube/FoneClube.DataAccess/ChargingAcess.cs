using Business.Commons.Utils;
using FoneClube.BoletoSimples;
using FoneClube.BoletoSimples.APIs.BankBillets.Models;
using FoneClube.BoletoSimples.Common;
using FoneClube.BoletoSimples.Utils;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FoneClube.Business.Commons.Entities.Cielo;
using FoneClube.Business.Commons.Utils;
using FoneClube.Business.Commons.Entities.FoneClube.charging;
using System.Web.Http;
using FoneClube.Business.Commons.Entities.Claro.proceduresResults;
using FoneClube.DataAccess.pagarme;
using static FoneClube.DataAccess.pagarme.PagarmeAccess;
using FoneClube.Business.Commons.Entities.woocommerce;
using FoneClube.DataAccess.Utilities;

namespace FoneClube.DataAccess
{
    public class ChargingAcess
    {

        protected readonly BoletoSimplesClient Client;


        public ChargingAcess()
        {

            var customClient = new HttpClient();

            Client = new BoletoSimplesClient(customClient, new ClientConnection());

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                Converters = new List<JsonConverter> { new BrazilianCurrencyJsonConverter() }
            };
        }

        public List<Person> GetClients(int year, int month)
        {
            var persons = new List<Person>();
            var chargingDate = new DateTime(year, month, 1);
            int[] chargedStatuses = { (int)BankBilletStatus.Generating, (int)BankBilletStatus.Opened, (int)BankBilletStatus.Paid };

            var telefonesComCpf = new List<string>();

            using (var ctx = new FoneClubeContext())
            {

                var chargingList = new List<CobrancaResult>();

                chargingList
                    .AddRange(ctx.Database.SqlQuery<CobrancaResult>("GetDadosCobrancaClaro @mes, @ano",
                    new SqlParameter("@mes", month),
                    new SqlParameter("@ano", year)).ToList());

                chargingList.ForEach(y => y.IdOperator = 1);

                chargingList.AddRange(ctx.Database.SqlQuery<CobrancaResult>("GetDadosCobrancaVIvo @mes, @ano",
                    new SqlParameter("@mes", month),
                    new SqlParameter("@ano", year)).ToList());

                chargingList.Where(y => y.IdOperator != 1).ToList().ForEach(t => t.IdOperator = 2);


                var cpfs = chargingList.Select(x => x.txtCPF).Distinct().ToArray();
                var numbers = chargingList.Select(x => x.txtTelefone.Replace(" ", "").Trim()).Distinct().ToArray();

                //var telefones = ctx.tblPersonsPhones.Where(x => numbers.Contains(x.intDDD + "" + x.intPhone) && x.bitAtivo == true).ToList();

                var query = ctx.tblPersons.Where(p => cpfs.Contains(p.txtDocumentNumber)).ToList();


                persons = query.Select(x => new Person()
                {
                    Id = x.intIdPerson,
                    IdPagarme = x.intIdPagarme,
                    Email = x.txtEmail,
                    DocumentNumber = x.txtDocumentNumber,
                    Name = x.txtName,
                    IdParent = x.intContactId,
                    Adresses = x.tblPersonsAddresses.Select(y => new Adress()
                    {
                        Street = y.txtStreet,
                        Complement = y.txtComplement,
                        Neighborhood = y.txtNeighborhood,
                        City = y.txtCity,
                        State = y.txtState,
                        Country = y.txtCountry,
                        Cep = y.txtCep
                    }).ToList(),
                    Phones = x.tblPersonsPhones.Where(w => numbers.Contains(string.Concat(w.intDDD.ToString().Trim(), w.intPhone.ToString().Trim()))).Select(z => new Phone()
                    {
                        Id = z.intId,
                        DDD = z.intDDD.ToString().Trim(),
                        Number = z.intPhone.ToString().Trim(),
                        NickName = z.txtNickname
                    }).ToList(),
                    ChargingHistory = x.tblChargingHistory.Where(r => r.dteCreate <= chargingDate).OrderByDescending(c => c.dteCreate).Select(h => new Charging()
                    {
                        Id = h.intId,
                        ChargingDate = h.dteCreate ?? DateTime.MinValue,
                        Ammount = h.txtAmmountPayment,
                        PaymentStatus = h.intPaymentStatus ?? (int)BankBilletStatus.NotApplicable,
                        PaymentType = h.intIdPaymentType,
                        Comment = h.txtComment,
                        ChargingComment = h.txtChargingComment,
                        ExpireDate = h.dteDueDate ?? DateTime.MinValue,
                        Charged = true
                    }
                        ).ToList()
                }
                ).ToList();


                foreach (var person in persons)
                {

                    foreach (var phone in person.Phones)
                    {
                        telefonesComCpf.Add(phone.DDD + phone.Number);
                        var charg = chargingList.FirstOrDefault(x => x.txtTelefone.Replace(" ", "").Trim() == string.Concat(phone.DDD, phone.Number));
                        // phone.Ammount = (charg.txtPrecoUnicoFracao ?? "0") != "0" ? charg.txtPrecoUnicoFracao : charg.txtValorCobranca;
                        phone.Ammount = IsPrecoUnicoFracao(charg.txtPrecoUnicoFracao) ? charg.txtPrecoUnicoFracao : charg.txtValorCobranca;
                        phone.IdOperator = charg.IdOperator;
                    }

                    var lastCharging = person.ChargingHistory.OrderByDescending(x => x.ChargingDate).FirstOrDefault();

                    if (person.ChargingHistory.All(x => x.ChargingDate != chargingDate))
                    {

                        person.Charging = new Charging()
                        {
                            ChargingComment = lastCharging?.ChargingComment,
                            Comment = lastCharging?.ChargingComment,
                            Ammount = CalcTotalByClient(person.Phones.Select(x => x.Ammount).ToArray())
                        };
                    }
                    else
                    {
                        person.Charging = lastCharging;
                        person.Charging.Charged = chargedStatuses.Contains(lastCharging.PaymentStatus);
                    }

                }

                var BaseFoneclube = new Person
                {
                    DocumentNumber = "000000000000",
                    Name = "Linhas Foneclube (sem cliente associado)",
                    Phones = new List<Phone>()
                };

                var telefonesLivres = numbers.Where(x => !telefonesComCpf.Distinct().ToList().Contains(x)).ToList();

                foreach (var telefoneLivre in telefonesLivres)
                {
                    BaseFoneclube.Phones.Add(new Phone
                    {
                        DDD = telefoneLivre.Substring(0, 2),
                        Number = telefoneLivre.Substring(2, 9)
                    });
                }


                foreach (var phone in BaseFoneclube.Phones)
                {

                    var charg = chargingList.FirstOrDefault(x => x.txtTelefone.Replace(" ", "").Trim() == string.Concat(phone.DDD, phone.Number));
                    phone.Ammount = IsPrecoUnicoFracao(charg.txtPrecoUnicoFracao) ? charg.txtPrecoUnicoFracao : charg.txtValorCobranca;
                    phone.IdOperator = charg.IdOperator;
                }

                BaseFoneclube.Charging = new Charging()
                {
                    ChargingComment = string.Empty,
                    Comment = string.Empty,
                    Ammount = CalcTotalByClient(BaseFoneclube.Phones.Select(x => x.Ammount).ToArray())
                };


                if (BaseFoneclube.Phones.Count > 0)
                    persons.Add(BaseFoneclube);

            }
            return persons;

        }

        public CieloChargingResponse InsertCieloTransaction(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    DateTime vingencia;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    try
                    {
                        if (person.Card != null)
                            SaveCard(person.Card, person, CardTypes.Debito);
                    }
                    catch (Exception) { }


                    var chargingId = SaveChargingHistory(person);
                    var linkDebito = new CieloAccess().GenerateFirstLinkDebito(new CieloDebitoTransaction
                    {
                        Ano = vingencia.Year,
                        Mes = vingencia.Month,
                        CustomerId = person.Id,
                        HistoryId = chargingId,
                        Valor = Convert.ToInt32(person.Charging.Ammount)
                    });

                    var customer = ctx.tblPersons.FirstOrDefault(c => c.intIdPerson == person.Id);

                    var post = new EmailAccess().SendEmail(new Email
                    {
                        To = customer.txtEmail,
                        TargetName = customer.txtName,
                        TargetTextBlue = linkDebito,
                        TargetSecondaryText = @"Valor total da sua fatura:" + (Convert.ToInt32(person.Charging.Ammount) / 100.00) + "R$. Ao clicar no botão acima você será redirecionado para o pagamento por débito.",
                        TemplateType = Convert.ToInt32(Email.TemplateTypes.Debito)
                    });

                    return new CieloChargingResponse
                    {
                        Charged = true,
                        Url = linkDebito
                    };
                }
            }
            catch (Exception e)
            {
                return new CieloChargingResponse
                {
                    Charged = false,
                    Message = e.Message
                };
            }
        }

        public int SaveChargingHistory(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    DateTime vingencia;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    var chargingHistory = new tblChargingHistory
                    {
                        txtAmmountPayment = person.Charging.Ammount,
                        intIdCollector = person.Charging.IdCollector,
                        intIdCustomer = person.Id,
                        intIdPaymentType = person.Charging.PaymentType,
                        txtCollectorName = person.Charging.CollectorName,
                        txtComment = person.Charging.Comment,
                        txtCommentBoleto = person.Charging.CommentBoleto,
                        txtCommentEmail = person.Charging.CommentEmail,
                        txtTokenTransaction = person.Charging.Token,
                        intIdBoleto = person.Charging.BoletoId,
                        txtAcquireId = person.Charging.AcquireId,
                        dteValidity = vingencia,
                        intChargeStatusId = person.Charging.ChargeStatus,
                        dteCreate = DateTime.Now,
                        bitCash = person.Charging.CacheTransaction,
                        dtePayment = DateTime.UtcNow
                    };

                    try
                    {
                        chargingHistory.intIdTransaction = Convert.ToInt64(person.Charging.TransactionId);
                    }
                    catch (Exception)
                    {
                        chargingHistory.intIdTransaction = -1;
                    }

                    try
                    {
                        chargingHistory.txtCartHash = person.Charging.CartHash;
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                        {
                            dteRegister = DateTime.Now,
                            txtLog = JsonConvert.SerializeObject(chargingHistory)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }

                    ctx.tblChargingHistory.Add(chargingHistory);
                    ctx.SaveChanges();

                    return chargingHistory.intId;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool ExecuteCharges()
        {
            //TODO registrar log replicando da outra tabela, do pix, cartão e boleto, colocar data e testar;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "agendamento de cobrança"
                    });

                    var pagarmeAccess = new PagarmeAccess();
                    var today = DateTime.Now;
                    var cobrancas = ctx.tblChargingScheduled
                                       .Where(a => a.dteExecution.Month == today.Month
                                                && a.dteExecution.Year == today.Year
                                                && a.dteExecution.Day == today.Day
                                                && a.bitExecuted != true).ToList();

                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "agendamento de cobrança" + cobrancas.Count
                    });

                    foreach (var charge in cobrancas)
                    {
                        var tblperson = ctx.tblPersons.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                        var chargingHistory = new tblChargingHistory
                        {
                            txtAmmountPayment = charge.txtAmmountPayment,
                            intIdCollector = charge.intIdCollector,
                            intIdCustomer = charge.intIdCustomer,
                            intIdPaymentType = charge.intIdPaymentType,
                            txtCollectorName = charge.txtCollectorName,
                            txtComment = Helper.ReplaceMessageScheduled(charge.txtComment, charge, tblperson, null, null),
                            txtCommentBoleto = charge.txtCommentBoleto,
                            txtCommentEmail = charge.txtCommentEmail,
                            txtTokenTransaction = charge.txtTokenTransaction,
                            intIdBoleto = charge.intIdBoleto,
                            txtAcquireId = charge.txtAcquireId,
                            dteValidity = charge.dteValidity,
                            intChargeStatusId = charge.intChargeStatusId,
                            dteCreate = DateTime.Now,
                            bitCash = charge.bitCash,
                            dtePayment = DateTime.UtcNow
                        };

                        var execute = pagarmeAccess.ExecuteChargingScheduled(charge);

                        try
                        {
                            chargingHistory.intIdTransaction = Convert.ToInt32(execute.Id);
                        }
                        catch (Exception)
                        {
                            chargingHistory.intIdTransaction = -1;
                        }

                        try
                        {
                            ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                            {
                                dteRegister = DateTime.Now,
                                txtLog = JsonConvert.SerializeObject(chargingHistory)
                            });
                        }
                        catch (Exception) { }
                        if (chargingHistory.intIdTransaction > 0)
                            charge.bitExecuted = true;
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool ExecuteChargesFor5DaysReminder(int days)
        {
            //TODO registrar log replicando da outra tabela, do pix, cartão e boleto, colocar data e testar;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "D-5 de cobrança"
                    });
                    var currentDate = DateTime.Now.AddDays(days).Date;
                    var pagarmeAccess = new PagarmeAccess();
                    var today = DateTime.Now;
                    var cobrancas = ctx.tblChargingScheduled
                        .Where(x => DbFunctions.TruncateTime(x.dteExecution) == DbFunctions.TruncateTime(currentDate) && x.intIdPaymentType != 1
                        && x.bitExecuted == false).ToList();

                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "D-5 de cobrança" + cobrancas.Count
                    });

                    foreach (var charge in cobrancas)
                    {
                        var tblperson = ctx.tblPersons.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                        var chargingHistory = new tblChargingHistory
                        {
                            txtAmmountPayment = charge.txtAmmountPayment,
                            intIdCollector = charge.intIdCollector,
                            intIdCustomer = charge.intIdCustomer,
                            intIdPaymentType = charge.intIdPaymentType,
                            txtCollectorName = charge.txtCollectorName,
                            txtComment = Helper.ReplaceMessageScheduled(charge.txtComment, charge, tblperson, null),
                            txtCommentBoleto = charge.txtCommentBoleto,
                            txtCommentEmail = charge.txtCommentEmail,
                            txtTokenTransaction = charge.txtTokenTransaction,
                            intIdBoleto = charge.intIdBoleto,
                            txtAcquireId = charge.txtAcquireId,
                            dteValidity = charge.dteValidity,
                            intChargeStatusId = charge.intChargeStatusId,
                            dteCreate = DateTime.Now,
                            bitCash = charge.bitCash,
                            dtePayment = DateTime.UtcNow,
                            dteDueDate = charge.dteExecution,
                            txtWAPhones = charge.txtWAPhones,
                            bitSendWAText = charge.bitSendWAText,
                            bitSendMarketing1 = charge.bitSendMarketing1,
                            bitSendMarketing2 = charge.bitSendMarketing2,
                        };

                        charge.dteDueDate = charge.dteExecution;
                        var execute = pagarmeAccess.ExecuteChargingScheduled(charge);

                        try
                        {
                            chargingHistory.intIdTransaction = Convert.ToInt32(execute.Id);
                        }
                        catch (Exception)
                        {
                            chargingHistory.intIdTransaction = -1;
                        }

                        try
                        {
                            ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                            {
                                dteRegister = DateTime.Now,
                                txtLog = JsonConvert.SerializeObject(chargingHistory)
                            });
                        }
                        catch (Exception) { }
                        try
                        {
                            var tblchargingSch = ctx.tblChargingScheduled.FirstOrDefault(x => x.intId == charge.intId);
                            if (tblchargingSch != null)
                            {
                                LogHelper.LogMessageOld(1, "ExecuteCHargesFor5DaysReminder changing bitExecuted:" + charge.intId);
                                tblchargingSch.bitExecuted = true;
                                tblchargingSch.dteModify = DateTime.Now;
                                ctx.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogMessageOld(1, "ExecuteCHargesFor5DaysReminder error:" + ex.ToString());
                        }
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool ExecuteChargesForCC()
        {
            //TODO registrar log replicando da outra tabela, do pix, cartão e boleto, colocar data e testar;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "CC 10:AM cobrança"
                    });
                    var currentDate = DateTime.Now.Date;
                    var pagarmeAccess = new PagarmeAccess();

                    var cobrancas = ctx.tblChargingScheduled
                        .Where(x => DbFunctions.TruncateTime(x.dteExecution) == DbFunctions.TruncateTime(currentDate)
                        && x.intIdPaymentType == 1 && x.bitExecuted != true).ToList();

                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "CC 10:AM cobrança" + cobrancas.Count
                    });

                    foreach (var charge in cobrancas)
                    {
                        var tblperson = ctx.tblPersons.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                        var chargingHistory = new tblChargingHistory
                        {
                            txtAmmountPayment = charge.txtAmmountPayment,
                            intIdCollector = charge.intIdCollector,
                            intIdCustomer = charge.intIdCustomer,
                            intIdPaymentType = charge.intIdPaymentType,
                            txtCollectorName = charge.txtCollectorName,
                            txtComment = Helper.ReplaceMessageScheduled(charge.txtComment, charge, tblperson, null),
                            txtCommentBoleto = charge.txtCommentBoleto,
                            txtCommentEmail = charge.txtCommentEmail,
                            txtTokenTransaction = charge.txtTokenTransaction,
                            intIdBoleto = charge.intIdBoleto,
                            txtAcquireId = charge.txtAcquireId,
                            dteValidity = charge.dteValidity,
                            intChargeStatusId = charge.intChargeStatusId,
                            dteCreate = DateTime.Now,
                            bitCash = charge.bitCash,
                            dtePayment = DateTime.UtcNow,
                            txtTransactionComment = charge.txtTransactionComment,
                            dteDueDate = charge.dteExecution,
                            txtWAPhones = charge.txtWAPhones,
                            bitSendWAText = charge.bitSendWAText,
                            bitSendMarketing1 = charge.bitSendMarketing1,
                            bitSendMarketing2 = charge.bitSendMarketing2,
                            intInstallments = charge.intInstallments,
                        };

                        charge.dteDueDate = charge.dteExecution;
                        var execute = pagarmeAccess.ExecuteChargingScheduled(charge);

                        try
                        {
                            chargingHistory.intIdTransaction = Convert.ToInt32(execute.Id);
                        }
                        catch (Exception)
                        {
                            chargingHistory.intIdTransaction = -1;
                        }

                        try
                        {
                            ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                            {
                                dteRegister = DateTime.Now,
                                txtLog = JsonConvert.SerializeObject(chargingHistory)
                            });
                        }
                        catch (Exception) { }

                        if (chargingHistory.intIdTransaction > 0)
                            charge.bitExecuted = true;
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool SetChargingFlagByUser(int chargeId, bool blnEnable)
        {
            using (var ctx = new FoneClubeContext())
            {
                var charg = ctx.tblChargingHistory.Where(x => x.intId == chargeId).FirstOrDefault();
                if (charg != null)
                {
                    charg.bitActive = blnEnable;
                    ctx.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }
        public int SaveChargingHistoryLoja(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    DateTime vingencia;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    var chargingHistory = new tblChargingHistory
                    {
                        txtAmmountPayment = person.Charging.Ammount,
                        intIdCollector = person.Charging.IdCollector,
                        intIdCustomer = person.Id,
                        intIdPaymentType = person.Charging.PaymentType,
                        txtCollectorName = person.Charging.CollectorName,
                        txtComment = person.Charging.Comment,
                        txtCommentBoleto = person.Charging.CommentBoleto,
                        txtCommentEmail = person.Charging.CommentEmail,
                        txtTokenTransaction = person.Charging.Token,
                        intIdBoleto = person.Charging.BoletoId,
                        txtAcquireId = person.Charging.AcquireId,
                        dteValidity = vingencia,
                        intChargeStatusId = person.Charging.ChargeStatus,
                        dteCreate = DateTime.Now,
                        bitCash = person.Charging.CacheTransaction,
                        dtePayment = DateTime.UtcNow,
                        bitPago = person.Charging.Payd,
                        txtStatusPaymentLoja = person.Charging.StatusDescription,
                        intIdCheckoutLoja = person.Charging.IdLoja
                    };

                    try
                    {
                        chargingHistory.intIdTransaction = Convert.ToInt64(person.Charging.TransactionId);
                    }
                    catch (Exception)
                    {
                        chargingHistory.intIdTransaction = -1;
                    }

                    try
                    {
                        chargingHistory.txtCartHash = person.Charging.CartHash;
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                        {
                            dteRegister = DateTime.Now,
                            txtLog = JsonConvert.SerializeObject(chargingHistory)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }

                    ctx.tblChargingHistory.Add(chargingHistory);
                    ctx.SaveChanges();

                    return chargingHistory.intId;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public int UpdateChargingHistoryLoja(Person person, int chargeId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    DateTime vingencia;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    var chargingHistory = ctx.tblChargingHistory.FirstOrDefault(c => c.intIdCheckoutLoja == chargeId);

                    chargingHistory.txtAmmountPayment = person.Charging.Ammount;
                    chargingHistory.intIdCollector = person.Charging.IdCollector;
                    chargingHistory.intIdCustomer = person.Id;
                    chargingHistory.intIdPaymentType = person.Charging.PaymentType;
                    chargingHistory.txtCollectorName = person.Charging.CollectorName;
                    chargingHistory.txtComment = person.Charging.Comment;
                    chargingHistory.txtCommentBoleto = person.Charging.CommentBoleto;
                    chargingHistory.txtCommentEmail = person.Charging.CommentEmail;
                    chargingHistory.txtTokenTransaction = person.Charging.Token;
                    chargingHistory.intIdBoleto = person.Charging.BoletoId;
                    chargingHistory.txtAcquireId = person.Charging.AcquireId;
                    chargingHistory.dteValidity = vingencia;
                    chargingHistory.intChargeStatusId = person.Charging.ChargeStatus;
                    chargingHistory.dteCreate = DateTime.Now;
                    chargingHistory.bitCash = person.Charging.CacheTransaction;
                    chargingHistory.dtePayment = DateTime.Now;
                    chargingHistory.bitPago = person.Charging.Payd;
                    chargingHistory.txtStatusPaymentLoja = person.Charging.StatusDescription;
                    chargingHistory.intIdCheckoutLoja = person.Charging.IdLoja;


                    try
                    {
                        chargingHistory.intIdTransaction = Convert.ToInt64(person.Charging.TransactionId);
                    }
                    catch (Exception)
                    {
                        chargingHistory.intIdTransaction = -1;
                    }

                    try
                    {
                        chargingHistory.txtCartHash = person.Charging.CartHash;
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                        {
                            dteRegister = DateTime.Now,
                            txtLog = JsonConvert.SerializeObject(chargingHistory)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }


                    ctx.SaveChanges();

                    return chargingHistory.intId;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool UpdateChargingHistoryLojaComplete(StatusPagamento checkout)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var charging = ctx.tblChargingHistory.FirstOrDefault(c => c.intIdCheckoutLoja == checkout.arg);

                    if (charging != null)
                    {
                        charging.bitPago = true;
                        charging.txtStatusPaymentLoja = "completed";
                        ctx.SaveChanges();
                    }
                    else
                    {

                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool InsertCieloCharging(CieloPaymentModel cieloPayment)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblCieloPaymentLog.Add(new tblCieloPaymentLog
                    {
                        intCustomerId = cieloPayment.CustomerId,
                        intCustomerIdERP = cieloPayment.CustomerIdERP,
                        intPaymentMethod = cieloPayment.PaymentMethod,
                        txtCurrency = cieloPayment.Currency,
                        txtOrderId = cieloPayment.OrderId,
                        txtPaymentGateway = cieloPayment.PaymentGateway,
                        txtPaymentId = cieloPayment.PaymentId,
                        intAmount = cieloPayment.Amount,
                        dtePaymentDate = cieloPayment.PaymentDate
                    });

                    //obfuscado
                    ctx.tblEmailLog.Add(new tblEmailLog
                    {
                        intIdTypeEmail = cieloPayment.PaymentMethod,
                        intIdPerson = cieloPayment.CustomerIdERP,
                        txtEmail = cieloPayment.Card,
                        dteRegister = DateTime.Now
                    });

                    var addresses = new List<Adress>();
                    addresses.Add(cieloPayment.Address);

                    if (cieloPayment.Address != null)
                    {
                        new ProfileAccess().SavePersonAddress(new Person
                        {
                            Id = cieloPayment.CustomerIdERP,
                            Adresses = addresses
                        }, ctx);
                    }

                    ctx.SaveChanges();

                    try
                    {
                        ctx.tblChargingHistory.Add(new tblChargingHistory
                        {
                            intIdCustomer = cieloPayment.CustomerIdERP,
                            txtCollectorName = "Checkout Loja Cielo",
                            intIdPaymentType = cieloPayment.PaymentMethod,
                            txtAmmountPayment = cieloPayment.Amount.ToString(),
                            dteCreate = DateTime.Now,
                            dtePayment = DateTime.Now,
                            intIdGateway = CieloGatewayType.Id, // cielo
                            txtPaymentId = cieloPayment.PaymentId,
                            dteValidity = new DateTime(1999, 9, 9)
                        });
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SetChargingLog(object serializedCharging, string id)
        {
            throw new NotImplementedException();
        }

        public List<CobFullVivo_Extract_Result> GetCobrancaFullVivoExtract(int mes, int ano)
        {
            using (FoneClubeContext ctx = new FoneClubeContext())
            {
                return ctx.CobFullVivo_Extract(mes, ano).ToList();
            }

        }

        private bool IsPrecoUnicoFracao(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var price = Convert.ToDecimal(value);
            return price > 0;
        }

        private string CalcTotalByClient(string[] arrayAmmounts)
        {
            var arrCleaned = arrayAmmounts.ToList().Where(x => x != null).ToArray();
            var converted = Array.ConvertAll(arrCleaned, Double.Parse);
            return converted.Sum().ToString();
        }


        enum TipoPagamento { card = 1, boletoPG = 2, boletoBS = 3 }; // mover pra commons

        public async Task<int> ChargeClient(Charging charging)
        {

            using (var ctx = new FoneClubeContext())
            {
                switch (charging.PaymentType)
                {
                    case (int)TipoPagamento.card:
                        return CreateChargingHistory(charging);
                    case (int)TipoPagamento.boletoPG:
                        return CreateChargingHistory(charging);
                    case (int)TipoPagamento.boletoBS:
                        charging.Id = CreateChargingHistory(charging);
                        var charg = await CreateBankBillet(charging, ctx);
                        UpdateChargingHistory(charg);
                        return charging.Id;
                }

                return 0;
            }

        }

        private async Task<Charging> CreateBankBillet(Charging charging, FoneClubeContext ctx)
        {
            ApiResponse<BankBillet> response;
            BankBillet successResponse;


            var clientData = (from p in ctx.tblPersons
                              join pa in ctx.tblPersonsAddresses
                              on p.intIdPerson equals pa.intIdPerson
                              where p.intIdPerson == charging.ClientId
                              select new
                              {
                                  txtName = p.txtName,
                                  txtEmail = p.txtEmail,
                                  txtDocumentNumber = p.txtDocumentNumber,
                                  zipcode = pa.txtCep,
                                  address = pa.txtStreet,
                                  cityName = pa.txtCity,
                                  state = pa.txtState,
                                  neighborhood = pa.txtNeighborhood,
                                  addressNumber = pa.intStreetNumber,
                                  addressComplement = pa.txtComplement

                              }
                                 )
                                 .FirstOrDefault();


            var billet = new BankBillet();

            billet.CustomerPersonName = clientData.txtName;
            billet.CustomerCnpjCpf = clientData.txtDocumentNumber;
            billet.CustomerZipcode = clientData.zipcode;
            billet.CustomerEmail = clientData.txtEmail;
            billet.CustomerAddress = clientData.address;
            billet.CustomerAddressNumber = clientData.addressNumber.Value.ToString();
            billet.CustomerAddressComplement = clientData.addressComplement;
            billet.CustomerNeighborhood = clientData.neighborhood;
            billet.CustomerCityName = clientData.cityName;
            billet.CustomerState = clientData.state;

            billet = ValidateAddress(billet);

            billet.Amount = Convert.ToDecimal(charging.Ammount) / 100;

            billet.ExpireAt = DateTime.Now.AddDays(5);
            billet.BankBilletAccountId = Convert.ToInt32(ConfigurationManager.AppSettings["boletosimple-bankbillet_account"]);
            billet.Description = ConfigurationManager.AppSettings["boletosimple-bankbillet_description"];



            response = await Client.BankBillets.PostAsync(billet).ConfigureAwait(false);


            successResponse = await response.GetSuccessResponseAsync().ConfigureAwait(false);



            charging.BoletoId = successResponse.Id;
            charging.TransactionComment = successResponse.Status;
            charging.ExpireDate = successResponse.ExpireAt;
            charging.BoletoLink = successResponse.ShortenUrl;
            charging.Charged = response.IsSuccess;
            charging.ChargingDate = DateTime.Now;

            if (successResponse.ShortenUrl != null)
            {
                string name = (clientData.txtDocumentNumber + "_" + DateTime.Now.ToShortDateString() + ".pdf");
                SendBankBilletMailWithLink(successResponse.ShortenUrl, clientData.txtEmail, name);
                //SendBankBilletMail(successResponse.ShortenUrl, clientData.txtEmail, name );
            }


            Client.Dispose();

            return charging;

        }



        public async Task<Charging> CreateBankBilletDirect(Person person, Charging charging)
        {
            ApiResponse<BankBillet> response;
            BankBillet successResponse;


            var billet = new BankBillet();

            billet.CustomerPersonName = person.DocumentNumber;
            billet.CustomerCnpjCpf = person.DocumentNumber;
            billet.CustomerEmail = person.Email;

            billet.CustomerZipcode = person.Adresses[0].Cep;
            billet.CustomerAddress = person.Adresses[0].Street;
            billet.CustomerAddressNumber = person.Adresses[0].StreetNumber;
            billet.CustomerAddressComplement = person.Adresses[0].Complement;
            billet.CustomerNeighborhood = person.Adresses[0].Neighborhood;
            billet.CustomerCityName = person.Adresses[0].City;
            billet.CustomerState = person.Adresses[0].State;

            billet = ValidateAddress(billet);

            billet.Amount = Convert.ToDecimal(charging.Ammount) / 100;

            billet.ExpireAt = DateTime.Now.AddDays(5);
            billet.BankBilletAccountId = Convert.ToInt32(ConfigurationManager.AppSettings["boletosimple-bankbillet_account"]);
            billet.Description = ConfigurationManager.AppSettings["boletosimple-bankbillet_description"];



            response = await Client.BankBillets.PostAsync(billet).ConfigureAwait(false);


            successResponse = await response.GetSuccessResponseAsync().ConfigureAwait(false);



            charging.BoletoId = successResponse.Id;
            charging.TransactionComment = successResponse.Status;
            charging.ExpireDate = successResponse.ExpireAt;
            charging.BoletoLink = successResponse.ShortenUrl;
            charging.Charged = response.IsSuccess;
            charging.ChargingDate = DateTime.Now;

            if (successResponse.ShortenUrl != null)
            {
                string name = (person.DocumentNumber + "_" + DateTime.Now.ToShortDateString() + ".pdf");
                SendBankBilletMailWithLink(successResponse.ShortenUrl, person.Email, name);
            }


            Client.Dispose();

            return charging;
        }

        public Boolean SendBankBilletMailWithLink(string url, string mail, string name)
        {
            var testMail = ConfigurationManager.AppSettings["boletosimple-bankbillet_emailTest"];
            if (!string.IsNullOrEmpty(testMail))
                mail = testMail;

            var b = new Utils().SendEmail(mail, "Boleto FoneClube - Seu boleto FoneClube chegou!", "Prezado Cliente, seu boleto Foneclube chegou, acesse o link para visualiza-lo: " + url);


            return true;
        }

        public Boolean SendBankBilletMail(string url, string mail, string name)
        {
            var testMail = ConfigurationManager.AppSettings["boletosimple-bankbillet_emailTest"];
            if (!string.IsNullOrEmpty(testMail))
                mail = testMail;

            var pdf = GetFile(url).Result;
            var b = new Utils().SendEmailWithAttachment(mail, "Boleto FoneClube", "Seu boleto FoneClube chegou!", pdf, name);


            return true;
        }

        private async Task<Stream> GetFile(string url)
        {

            try
            {
                var clientHttp = new HttpClient();
                clientHttp.Timeout = TimeSpan.FromMinutes(5);
                var myTask = clientHttp.GetByteArrayAsync(new Uri(url));

                var byteArray = await myTask;

                Stream stream = new MemoryStream(byteArray);

                return stream;

            }
            catch (AggregateException ex)
            {
                throw ex;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public int CreateChargingHistory(Charging charging)
        {

            using (var ctx = new FoneClubeContext())
            {

                int id;
                var charg = ctx.tblChargingHistory.Where(y => y.intIdCustomer == charging.ClientId && y.dteCreate == charging.ChargingDate).FirstOrDefault();

                if (charg != null)
                {
                    charg.dteCreate = DateTime.Now;
                    charg.txtAmmountPayment = charging.Ammount;
                    charg.intIdPaymentType = charging.PaymentType;
                    charg.txtCollectorName = charging.CollectorName;
                    charg.txtComment = charging.Comment;
                    charg.txtChargingComment = charging.ChargingComment;
                    charg.txtTokenTransaction = charging.Token;
                    charg.txtAcquireId = charging.AcquireId;
                    charg.txtTransactionComment = "Generating";
                    charg.intPaymentStatus = (int)BankBilletStatus.Generating;
                    charg.dteModify = DateTime.Now;
                    charg.intIdBoleto = 0;
                    charg.bitComissionConceded = charging.ComissionConceded;

                    if (Convert.ToInt32(charging.ChargeStatus) > 0)
                        charg.intChargeStatusId = charging.ChargeStatus;

                    ctx.SaveChanges();

                    if (Convert.ToBoolean(charging.ComissionConceded))
                    {
                        var comissionsAccess = new ComissionAccess();
                        comissionsAccess.AddComissionLog(comissionsAccess.GetComissoesCustomer(charging.ClientId), false);
                    }

                    id = charg.intId;
                }
                else
                {


                    var charHist = new tblChargingHistory
                    {
                        txtAmmountPayment = charging.Ammount,
                        intIdCollector = charging.IdCollector,
                        intIdCustomer = charging.ClientId,
                        intIdPaymentType = charging.PaymentType,
                        txtCollectorName = charging.CollectorName,
                        txtComment = charging.Comment,
                        txtChargingComment = charging.ChargingComment,
                        txtTokenTransaction = charging.Token,
                        txtAcquireId = charging.AcquireId,
                        txtTransactionComment = "Generating",
                        dteCreate = DateTime.Now,
                        intPaymentStatus = (int)BankBilletStatus.Generating,
                        intChargeStatusId = charging.ChargeStatus,
                        bitComissionConceded = charging.ComissionConceded
                    };

                    try
                    {
                        charHist.intIdTransaction = Convert.ToInt64(charging.TransactionId);
                    }
                    catch (Exception)
                    {
                        charHist.intIdTransaction = -1;
                    }

                    ctx.tblChargingHistory.Add(charHist);

                    ctx.SaveChanges();

                    id = charHist.intId;
                }

                return id;
            }

        }

        public bool UpdateChargingHistory(Charging charging)
        {

            using (var ctx = new FoneClubeContext())
            {
                var charg = ctx.tblChargingHistory.Where(x => x.intId == charging.Id).FirstOrDefault();
                if (charg != null)
                {
                    if (charging.BoletoId != default(int))
                        charg.intIdBoleto = charging.BoletoId;
                    if (charging.ExpireDate != default(DateTime))
                        charg.dteDueDate = charging.ExpireDate;

                    charg.txtTokenTransaction = charging.Token;
                    charg.intIdBoleto = charging.BoletoId;
                    charg.txtAcquireId = charging.AcquireId;
                    charg.dteModify = DateTime.Now;
                    charg.intPaymentStatus = charging.PaymentStatus;
                    charg.txtTransactionComment = charging.TransactionComment;


                    ctx.SaveChanges();
                    return true;

                }
                else
                    return false;


            }
        }


        public HttpResponseMessage UpdateBankBilletStatus(JToken jToken)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            try
            {
                var bankBillet = JsonConvert.DeserializeObject<BankBillet>(jToken["object"].ToString());

                var eventCode = jToken["event_code"].ToString();

                if (eventCode != "bank_billet.created")
                {
                    var changes = JsonConvert.DeserializeObject<BankBilletChanges>(jToken["changes"].ToString());

                }

                var status = (BankBilletStatus)Enum.Parse(typeof(BankBilletStatus), bankBillet.Status, true);

                using (var ctx = new FoneClubeContext())
                {

                    var charging = ctx.tblChargingHistory.FirstOrDefault(y => y.intIdBoleto == bankBillet.Id);

                    if (charging != null)
                    {
                        charging.dteModify = DateTime.Now;
                        charging.intPaymentStatus = (int)status;
                        charging.txtTransactionComment = eventCode;

                        if (status == BankBilletStatus.Paid)
                        {
                            charging.dtePayment = bankBillet.PaidAt.Value;
                        }


                        if (ctx.SaveChanges() == 0)
                        {
                            responseMessage.ReasonPhrase = "Status não alterado.";
                            responseMessage.StatusCode = HttpStatusCode.Conflict;
                        }

                    }
                    else
                    {
                        responseMessage.StatusCode = HttpStatusCode.NotFound;
                    }


                }

                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                responseMessage.ReasonPhrase = ex.Message;
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
            }

            return responseMessage;

        }


        private BankBillet ValidateAddress(BankBillet bankBillet)
        {
            bankBillet.CustomerAddress = ValidateStringData(bankBillet.CustomerAddress, "Rua da Glória");
            bankBillet.CustomerNeighborhood = ValidateStringData(bankBillet.CustomerNeighborhood, "Glória");
            bankBillet.CustomerCityName = ValidateStringData(bankBillet.CustomerCityName, "Rio de Janeiro");
            bankBillet.CustomerState = ValidateStringData(bankBillet.CustomerState, "RJ");

            return bankBillet;
        }

        private string ValidateStringData(string data, string dummy)
        {
            if (!string.IsNullOrEmpty(data.Trim()) && !data.Trim().Equals("Empty"))
                return data;
            else
                return dummy;
        }

        public bool CobrancaClaroRefresh(int mesParam, int anoParam)
        {

            using (var ctx = new FoneClubeContext())
            {
                var mes = new SqlParameter("@mes", mesParam);
                var ano = new SqlParameter("@ano", anoParam);
                var listaCobranca = ctx.Database.SqlQuery<int>("prcCargaClaro @mes, @ano", mes, ano).FirstOrDefault();

                return listaCobranca == 1;
            }


        }

        public bool CobrancaVivoRefresh(int mesParam, int anoParam)
        {

            using (var ctx = new FoneClubeContext())
            {
                var mes = new SqlParameter("@mes", mesParam);
                var ano = new SqlParameter("@ano", anoParam);
                var listaCobranca = ctx.Database.SqlQuery<int>("prcCargaVivo @mes, @ano", mes, ano).FirstOrDefault();

                return listaCobranca == 1;
            }


        }

        public List<tblChargingHistory> GetValidityCharginHistory(Charging charging)
        {
            try
            {
                var vingencia = new DateTime(Convert.ToInt32(charging.AnoVingencia), Convert.ToInt32(charging.MesVingencia), 1, 0, 0, 0);

                using (var ctx = new FoneClubeContext())
                    return ctx.tblChargingHistory.Where(c => c.dteValidity.Value.Month == vingencia.Month && c.dteValidity.Value.Year == vingencia.Year).ToList();

            }
            catch (Exception e)
            {
                return new List<tblChargingHistory>();
            }

        }

        public List<tblChargingHistory> GetCustomerValidityCharginHistory(Charging charging, int customerId)
        {
            try
            {
                var vingencia = new DateTime(Convert.ToInt32(charging.AnoVingencia), Convert.ToInt32(charging.MesVingencia), 1, 0, 0, 0);

                using (var ctx = new FoneClubeContext())
                    return ctx.tblChargingHistory
                        .Where(c => c.dteValidity.Value.Month == vingencia.Month
                        && c.dteValidity.Value.Year == vingencia.Year
                        && c.intIdCustomer == customerId).ToList();

            }
            catch (Exception e)
            {
                return new List<tblChargingHistory>();
            }

        }

        public List<Person> GetValidityPayments(Charging charging)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var ano = Convert.ToInt32(charging.AnoVingencia);
                    var mes = Convert.ToInt32(charging.MesVingencia);

                    //var dataGeral = (from t in ctx.CobFullClaro_Sum
                    //                 where t.Report_Month == mes && t.Report_Year == ano
                    //                 select new
                    //                 {
                    //                     Telefone = t.Telefone,
                    //                     Result = t.Resultado___linha
                    //                 }).ToList();


                    //var dataVivo = (from t in ctx.CobFullVivo_Sum
                    //                where t.Report_Month == mes && t.Report_Year == ano
                    //                select new
                    //                {
                    //                    Telefone = t.Telefone.Replace(" ", ""),
                    //                    Result = t.Resultado___da_linha
                    //                }).ToList();


                    //dataGeral.AddRange(dataVivo);

                    var planDetails = ctx.tblPlansOptions.Where(p => p.txtDescription.Length > 0).ToList();
                    var totalBoletoCharges = 0;
                    var userSettings = ctx.tblUserSettings.ToList();

                    var allPersons = ctx.tblPersons
                        .Where(p => p.txtDocumentNumber.Length > 0)
                        .Select(p => new Person
                        {
                            Id = p.intIdPerson,
                            NickName = p.txtNickName,
                            Name = p.txtName,
                            Email = p.txtEmail,
                            DocumentNumber = p.txtDocumentNumber,
                            IdPagarme = p.intIdPagarme,
                            Desativo = p.bitDesativoManual == null || p.bitDesativoManual == false ? false : true,
                            DefaultVerificar = p.intDftVerificar,
                            DefaultWAPhones = p.txtDefaultWAPhones,
                            NextActionText = p.txtNextAction,
                            NextActionDate = p.dteNextActionDate,
                            Phones = ctx.tblPersonsPhones
                                .Where(pp => pp.intIdPerson == p.intIdPerson && pp.bitAtivo == true && pp.bitPhoneClube == true)
                                .Select(pp => new Phone
                                {
                                    DDD = pp.intDDD.Value.ToString(),
                                    Number = pp.intPhone.Value.ToString(),
                                    IdPlanOption = pp.intIdPlan.Value,
                                    NickName = pp.txtNickname,
                                    ICCID = pp.txtICCID,
                                    PortNumber = pp.txtPortNumber
                                }).ToList()
                        }).ToList().Distinct();

                    foreach (var person in allPersons)
                    {
                        person.Use2Prices = userSettings.Any(x => x.intPerson == person.Id) ? userSettings.Where(x => x.intPerson == person.Id).FirstOrDefault().bitUse2Prices : false;
                    }

                    //eh possivel reaproveitar ra n pegar telefone novamente ali a baixo
                    var clientes = new List<Person>();
                    var allPhones = new List<Phone>();
                    var transactions = new TransactionAccess().GetAllLastTransactions();

                    //var chargingList = GetValidityCharginHistory(charging); // No need to check for current month - Praveena
                    var chargingList = ctx.tblChargingHistory.Where(x => x.bitActive == true).ToList();

                    foreach (var charge in chargingList)
                    {
                        if (bool.Equals(clientes.FirstOrDefault(p => p.Id == charge.intIdCustomer.Value), null))
                        {
                            var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == charge.intIdCustomer.Value);
                            var phones = ctx.tblPersonsPhones
                                //.Where(p => p.intIdPerson == pessoa.intIdPerson && p.bitAtivo == true && p.bitPhoneClube == true)
                                .Where(p => p.intIdPerson == pessoa.intIdPerson && p.bitPhoneClube == true)
                                .Select(p => new Phone
                                {
                                    DDD = p.intDDD.Value.ToString(),
                                    Number = p.intPhone.Value.ToString(),
                                    IdPlanOption = p.intIdPlan.Value,
                                    ICCID = p.txtICCID,
                                    PortNumber = p.txtPortNumber

                                }).ToList();

                            allPhones.AddRange(phones);

                            var cliente = new Person
                            {
                                Id = charge.intIdCustomer.Value,
                                Name = pessoa.txtName.Trim(),
                                Email = pessoa.txtEmail.Trim(),
                                IdPagarme = pessoa.intIdPagarme,
                                Phones = bool.Equals(phones, null) ? new List<Phone>() : phones,
                                Charged = true,
                                DefaultVerificar = pessoa.intDftVerificar,
                                DefaultWAPhones = pessoa.txtDefaultWAPhones,
                                NextActionText = pessoa.txtNextAction,
                                NextActionDate = pessoa.dteNextActionDate,
                                Use2Prices = userSettings.Any(x => x.intPerson == pessoa.intIdPerson) ? userSettings.Where(x => x.intPerson == pessoa.intIdPerson).FirstOrDefault().bitUse2Prices : false
                            };

                            cliente.ChargingValidity = new List<Charging>();
                            var clienteChargingList = chargingList.Where(c => c.intIdCustomer == cliente.Id).ToList();

                            foreach (var chargeCustomer in clienteChargingList)
                            {
                                if (chargeCustomer.intIdPaymentType == (int)TipoPagamento.boletoPG)
                                {
                                    totalBoletoCharges++;
                                }

                                var transactionPagarme = transactions.FirstOrDefault(l => l.intIdTransaction == chargeCustomer.intIdTransaction);
                                DateTime expirationDate;
                                try
                                {
                                    expirationDate = Convert.ToDateTime(transactionPagarme.dteBoleto_expiration_date);
                                }
                                catch (Exception)
                                {
                                    expirationDate = DateTime.Now;
                                }

                                var expirationDay = expirationDate.DayOfWeek;

                                //Para caso de feriado sexta ou segunda, para ambos colocar 2 dias
                                expirationDate.AddDays(1);

                                if (expirationDay == DayOfWeek.Saturday)
                                    expirationDate.AddDays(2);

                                if (expirationDay == DayOfWeek.Sunday)
                                    expirationDate.AddDays(1);

                                var charg = new Charging();

                                try
                                {
                                    var statusCharging = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? "PAGO" : "CARREGANDO";
                                    var paydCharging = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? true : false;
                                    var statusChargingPagarme = string.Empty;

                                    if (!bool.Equals(transactionPagarme, null))
                                    {
                                        statusChargingPagarme = transactionPagarme.txtOutdadetStatus;

                                        if (transactionPagarme.txtOutdadetStatus == "Refunded")
                                        {
                                            statusCharging = "Refunded";
                                            paydCharging = false;

                                        }
                                    }
                                    else if (transactionPagarme == null && chargeCustomer.intIdTransaction == 999999999)
                                    {
                                        statusCharging = "Paid";
                                        paydCharging = true;
                                    }


                                    charg = new Charging
                                    {
                                        Id = chargeCustomer.intId,
                                        Ammount = string.IsNullOrEmpty(chargeCustomer.txtAmmountPayment) == true ? string.Empty : chargeCustomer.txtAmmountPayment,
                                        ChargingComment = string.IsNullOrEmpty(chargeCustomer.txtChargingComment) == true ? string.Empty : chargeCustomer.txtChargingComment,
                                        TransactionId = Convert.ToInt64(chargeCustomer.intIdTransaction),
                                        Charged = true,
                                        Payd = paydCharging,
                                        PaymentType = chargeCustomer.intIdPaymentType,
                                        BoletoId = chargeCustomer.intIdBoleto.HasValue ? chargeCustomer.intIdBoleto.Value : -1,
                                        StatusDescription = statusCharging,
                                        BoletoExpires = transactionPagarme == null ? null : transactionPagarme.dteBoleto_expiration_date,
                                        BoletoLink = transactionPagarme == null ? string.Empty : Convert.ToString(transactionPagarme.txtBoleto_url),
                                        Expired = expirationDate < DateTime.Now && statusChargingPagarme != "Paid",
                                        CreateDate = chargeCustomer.dteCreate == null ? DateTime.MinValue : Convert.ToDateTime(chargeCustomer.dteCreate),
                                        Canceled = Convert.ToBoolean(chargeCustomer.bitCanceled),
                                        PixCode = chargeCustomer.pixCode,
                                        PaymentStatusType = transactionPagarme == null ? chargeCustomer.intIdTransaction == 999999999 ? "Paid" : null : transactionPagarme.txtOutdadetStatus,
                                        ExpireDate = Convert.ToDateTime(chargeCustomer.dteExpiryDate),
                                        AnoVingencia = Convert.ToDateTime(chargeCustomer.dteValidity).Year.ToString(),
                                        MesVingencia = Convert.ToDateTime(chargeCustomer.dteValidity).Month.ToString(),
                                        DueDate = Convert.ToDateTime(chargeCustomer.dteDueDate),
                                        TransactionLastUpdate = transactionPagarme == null ? chargeCustomer.intIdTransaction == 999999999 ? chargeCustomer.dtePayment.ToString("yyyy-MM-dd h:mm tt") : string.Empty : Convert.ToDateTime(transactionPagarme.dteDate_updated).ToString("yyyy-MM-dd h:mm tt"),
                                        TxtWAPhones = chargeCustomer.txtWAPhones,
                                        SendWAText = chargeCustomer.bitSendWAText,
                                        SendMarketing1 = chargeCustomer.bitSendMarketing1,
                                        SendMarketing2 = chargeCustomer.bitSendMarketing2,
                                        Installments = chargeCustomer.intInstallments.HasValue ? chargeCustomer.intInstallments.Value : 0
                                    };
                                }
                                catch (Exception e) { }

                                cliente.ChargingValidity.Add(charg);
                            }

                            foreach (var client in clientes)
                            {
                                client.TotalBoletoCharges = totalBoletoCharges;
                            }

                            clientes.Add(cliente);
                        }
                    }

                    foreach (var person in allPersons)
                    {
                        if (bool.Equals(clientes.FirstOrDefault(c => c.Id == person.Id), null))
                            clientes.Add(person);
                    }

                    var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();
                    List<int> clientIds = clientes.Select(x => x.Id).ToList();
                    var chargeAndOrderHistory = new ProfileAccess().GetChargingServiceOrdersHistory(clientIds);
                    var chargingHistoryScheduled = ctx.tblChargingScheduled.Where(x => x.bitExecuted == false).ToList();

                    foreach (var person in clientes)
                    {
                        try
                        {
                            person.ChargeAndServiceOrderHistory = chargeAndOrderHistory.FirstOrDefault(x => x != null && x.PersonId == person.Id);

                            var chargingHst = chargingList.Where(x => x.intIdCustomer == person.Id).Select(x => x.intIdTransaction);

                            var transaction = (from c in chargingHst
                                               join t in lastPayments on c equals t.intIdTransaction
                                               select t).OrderByDescending(x => x.dteDate_updated).FirstOrDefault();
                            if (transaction != null)
                            {
                                person.LastPaidDate = Convert.ToDateTime(transaction.dteDate_updated).ToString("yyyy/MM/dd");
                                person.LastPaidAmount = transaction.intPaid_amount.HasValue ? transaction.intPaid_amount.Value : 0;

                                var tIds = new List<int> { 999999999 };
                                var transactionContel = (from c in chargingHst
                                                         join t in tIds on c equals t
                                                         select t).LastOrDefault();
                                if (transactionContel != 0)
                                {
                                    var lastTransaction999 = ctx.tblChargingHistory.Where(x => x.intIdTransaction == transactionContel && x.intIdCustomer == person.Id).LastOrDefault();
                                    if (lastTransaction999.dtePayment > transaction.dteDate_updated)
                                    {
                                        person.LastPaidDate = lastTransaction999.dtePayment.ToString("yyyy/MM/dd");
                                        person.LastPaidAmount = Convert.ToInt32(lastTransaction999.txtAmmountPayment);
                                    }
                                }
                            }
                            else if (transaction == null)
                            {
                                var tIds = new List<long> { 999999999 };
                                var transactionContel = (from c in chargingHst
                                                         join t in tIds on c equals t
                                                         select t).LastOrDefault();
                                if (transactionContel != 0)
                                {
                                    person.LastPaidDate = ctx.tblChargingHistory.Where(x => x.intIdTransaction == transactionContel && x.intIdCustomer == person.Id).LastOrDefault().dtePayment.ToString("yyyy/MM/dd");
                                    person.LastPaidAmount = Convert.ToInt32(ctx.tblChargingHistory.Where(x => x.intIdTransaction == transactionContel && x.intIdCustomer == person.Id).LastOrDefault().txtAmmountPayment);
                                }
                            }

                            if (chargingHistoryScheduled != null && chargingHistoryScheduled.Count > 0)
                            {
                                string lastScheduleDate = "";
                                var scheduledCount = chargingHistoryScheduled.Where(x => x.intIdCustomer == person.Id && x.dteExecution > DateTime.Now).Count();
                                var lastScheduled = chargingHistoryScheduled.Where(x => x.intIdCustomer == person.Id && x.dteExecution > DateTime.Now).OrderByDescending(x => x.dteExecution).FirstOrDefault();
                                if (lastScheduled != null)
                                {
                                    lastScheduleDate = lastScheduled.dteExecution.ToString("dd/MMM", new System.Globalization.CultureInfo("PT-br"));
                                }
                                person.LastScheduleDate = lastScheduleDate;
                                person.SchduleCount = scheduledCount;
                            }

                            if (person.ChargeAndServiceOrderHistory != null && person.ChargeAndServiceOrderHistory.Charges != null)
                            {
                                var maxStatusId = ctx.tblWhatsAppStatus.Where(x => x.intIdCharge == person.ChargeAndServiceOrderHistory.Charges.Id).Max(y => y.intStatus);
                                if (maxStatusId.HasValue && maxStatusId.Value > 0)
                                {
                                    var whatsappStatus = ctx.tblWhatsAppStatus.Where(x => x.intIdCharge == person.ChargeAndServiceOrderHistory.Charges.Id && x.intStatus == maxStatusId).FirstOrDefault();
                                    person.WhatsAppStatus = whatsappStatus.intStatus.Value;
                                    switch (whatsappStatus.intStatus.Value)
                                    {
                                        case 1:
                                            person.WhatsAppStatusDate = whatsappStatus.dteMsgSentDateTime.Value.ToString("dd/MMM HH:mm", new System.Globalization.CultureInfo("PT-br"));
                                            break;
                                        case 2:
                                            person.WhatsAppStatusDate = whatsappStatus.dteMsgReceivedDateTime.Value.ToString("dd/MMM HH:mm", new System.Globalization.CultureInfo("PT-br"));
                                            break;
                                        case 3:
                                            person.WhatsAppStatusDate = whatsappStatus.dteMsgReadDateTime.Value.ToString("dd/MMM HH:mm", new System.Globalization.CultureInfo("PT-br"));
                                            break;
                                        default:
                                            person.WhatsAppStatusDate = "";
                                            break;
                                    }
                                }
                                else
                                {
                                    person.WhatsAppStatus = 0;
                                    person.WhatsAppStatusDate = "";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            person.LastPaidDate = null;
                            person.LastPaidAmount = 0;
                        }

                        foreach (var phone in person.Phones)
                        {
                            if (phone.IdPlanOption.HasValue)
                            {
                                var detail = planDetails.FirstOrDefault(d => d.intIdPlan == phone.IdPlanOption);
                                phone.PlanDescription = !bool.Equals(detail, null) ? detail.txtDescription : "";
                                phone.IdOperator = !bool.Equals(detail, null) ? detail.intIdOperator : 0;
                            }
                            //var claro = 1;
                            //var vivo = 2;

                            //var cobranca = dataGeral.FirstOrDefault(l => l.Telefone == phone.DDD + phone.Number);
                            //phone.ResultValue = !bool.Equals(cobranca, null) ? cobranca.Result.ToString() : "null";
                            //if(string.IsNullOrEmpty(phone.ResultValue))
                            //{
                            //    phone.ResultValue = "null";
                            //}

                        }
                    }


                    return clientes
                         .OrderBy(c => c.Charged == false).ToList();
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = ex.ToString()
                    });
                    ctx.SaveChanges();
                }
                return null;
            }
        }


        public List<Person> GetMonthChargings(Charging charging)
        {
            using (var ctx = new FoneClubeContext())
            {
                var ano = Convert.ToInt32(charging.AnoVingencia);
                var mes = Convert.ToInt32(charging.MesVingencia);

                var planDetails = ctx.tblPlansOptions.Where(p => p.txtDescription.Length > 0).ToList();
                var totalBoletoCharges = 0;

                var allPersons = ctx.tblPersons
                    .Where(p => p.txtDocumentNumber.Length > 0)
                    .Select(p => new Person
                    {
                        Id = p.intIdPerson,
                        Name = p.txtName,
                        Email = p.txtEmail,
                        DocumentNumber = p.txtDocumentNumber,
                        IdPagarme = p.intIdPagarme,
                        Desativo = p.bitDesativoManual == null || p.bitDesativoManual == false ? false : true,
                        Phones = ctx.tblPersonsPhones
                            .Where(pp => pp.intIdPerson == p.intIdPerson && pp.bitAtivo == true && pp.bitPhoneClube == true)
                            .Select(pp => new Phone
                            {
                                DDD = pp.intDDD.Value.ToString(),
                                Number = pp.intPhone.Value.ToString(),
                                IdPlanOption = pp.intIdPlan.Value

                            }).ToList()
                    }).ToList().Distinct();


                //eh possivel reaproveitar ra n pegar telefone novamente ali a baixo
                var clientes = new List<Person>();
                var allPhones = new List<Phone>();
                var transactions = new TransactionAccess().GetAllLastTransactions();

                var chargingList = GetMonthChargingsHistory(charging);

                foreach (var charge in chargingList)
                {
                    if (bool.Equals(clientes.FirstOrDefault(p => p.Id == charge.intIdCustomer.Value), null))
                    {
                        var pessoa = allPersons.FirstOrDefault(p => p.Id == charge.intIdCustomer.Value);
                        var phones = ctx.tblPersonsPhones
                            .Where(p => p.intIdPerson == pessoa.Id && p.bitAtivo == true && p.bitPhoneClube == true)
                            .Select(p => new Phone
                            {
                                DDD = p.intDDD.Value.ToString(),
                                Number = p.intPhone.Value.ToString(),
                                IdPlanOption = p.intIdPlan.Value

                            }).ToList();

                        allPhones.AddRange(phones);

                        var cliente = new Person
                        {
                            Id = charge.intIdCustomer.Value,
                            Name = pessoa.Name.Trim(),
                            Email = pessoa.Email.Trim(),
                            IdPagarme = pessoa.IdPagarme,
                            Phones = bool.Equals(phones, null) ? new List<Phone>() : phones,
                            Charged = true
                        };

                        cliente.ChargingValidity = new List<Charging>();
                        var clienteChargingList = chargingList.Where(c => c.intIdCustomer == cliente.Id).ToList();

                        foreach (var chargeCustomer in clienteChargingList)
                        {
                            if (chargeCustomer.intIdPaymentType == (int)TipoPagamento.boletoPG)
                            {
                                totalBoletoCharges++;
                            }

                            var transactionPagarme = transactions.FirstOrDefault(l => l.intIdTransaction == chargeCustomer.intIdTransaction);
                            DateTime expirationDate;
                            try
                            {
                                expirationDate = Convert.ToDateTime(transactionPagarme.dteBoleto_expiration_date);
                            }
                            catch (Exception)
                            {
                                expirationDate = DateTime.Now;
                            }

                            var expirationDay = expirationDate.DayOfWeek;

                            //Para caso de feriado sexta ou segunda, para ambos colocar 2 dias
                            expirationDate.AddDays(1);

                            if (expirationDay == DayOfWeek.Saturday)
                                expirationDate.AddDays(2);

                            if (expirationDay == DayOfWeek.Sunday)
                                expirationDate.AddDays(1);

                            var charg = new Charging();

                            try
                            {
                                var statusCharging = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? "PAGO" : "CARREGANDO";
                                var paydCharging = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? true : false;
                                var statusChargingPagarme = string.Empty;

                                if (!bool.Equals(transactionPagarme, null))
                                {
                                    statusChargingPagarme = transactionPagarme.txtOutdadetStatus;

                                    if (transactionPagarme.txtOutdadetStatus == "Refunded")
                                    {
                                        statusCharging = "Refunded";
                                        paydCharging = false;

                                    }
                                }


                                charg = new Charging
                                {
                                    Id = chargeCustomer.intId,
                                    Ammount = string.IsNullOrEmpty(chargeCustomer.txtAmmountPayment) == true ? string.Empty : chargeCustomer.txtAmmountPayment,
                                    ChargingComment = string.IsNullOrEmpty(chargeCustomer.txtChargingComment) == true ? string.Empty : chargeCustomer.txtChargingComment,
                                    TransactionId = Convert.ToInt64(chargeCustomer.intIdTransaction),
                                    Charged = true,
                                    Payd = paydCharging,
                                    PaymentType = chargeCustomer.intIdPaymentType,
                                    BoletoId = chargeCustomer.intIdBoleto.Value,
                                    StatusDescription = statusCharging,
                                    BoletoExpires = transactionPagarme == null ? null : transactionPagarme.dteBoleto_expiration_date,
                                    BoletoLink = transactionPagarme == null ? string.Empty : Convert.ToString(transactionPagarme.txtBoleto_url),
                                    Expired = expirationDate < DateTime.Now && statusChargingPagarme != "Paid",
                                    CreateDate = chargeCustomer.dteCreate == null ? DateTime.MinValue : Convert.ToDateTime(chargeCustomer.dteCreate),
                                    Canceled = Convert.ToBoolean(chargeCustomer.bitCanceled)
                                };
                            }
                            catch (Exception e) { }

                            cliente.ChargingValidity.Add(charg);
                        }

                        foreach (var client in clientes)
                        {
                            client.TotalBoletoCharges = totalBoletoCharges;
                        }

                        clientes.Add(cliente);
                    }
                }

                foreach (var person in allPersons)
                {
                    if (bool.Equals(clientes.FirstOrDefault(c => c.Id == person.Id), null))
                        clientes.Add(person);
                }

                var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();

                foreach (var person in clientes)
                {

                    try
                    {
                        var transaction = lastPayments.FirstOrDefault(p => p.intIdCustomer == person.IdPagarme);
                        person.LastPaidDate = Convert.ToDateTime(lastPayments.FirstOrDefault(p => p.intIdCustomer == person.IdPagarme).dteDate_updated).ToString("yyyy/MM/dd");
                    }
                    catch (Exception)
                    {
                        person.LastPaidDate = null;
                    }

                    foreach (var phone in person.Phones)
                    {
                        var detail = planDetails.FirstOrDefault(d => d.intIdPlan == phone.IdPlanOption);
                        phone.PlanDescription = !bool.Equals(detail, null) ? detail.txtDescription : "";
                        phone.IdOperator = !bool.Equals(detail, null) ? detail.intIdOperator : 0;

                    }
                }


                return clientes
                    .OrderBy(c => c.Charged == false).ToList();
            }
        }


        public List<tblChargingHistory> GetMonthChargingsHistory(Charging charging)
        {
            try
            {
                var vingencia = new DateTime(Convert.ToInt32(charging.AnoVingencia), Convert.ToInt32(charging.MesVingencia), 1, 0, 0, 0);

                using (var ctx = new FoneClubeContext())
                    return ctx.tblChargingHistory.Where(c => c.dteCreate.Value.Month == vingencia.Month && c.dteCreate.Value.Year == vingencia.Year).ToList();

            }
            catch (Exception e)
            {
                return new List<tblChargingHistory>();
            }

        }


        public List<Person> GetCustomerValidityPayments(Charging charging, int customerId)
        {
            if (customerId == 0)
                return new List<Person>();

            using (var ctx = new FoneClubeContext())
            {
                var ano = Convert.ToInt32(charging.AnoVingencia);
                var mes = Convert.ToInt32(charging.MesVingencia);

                var dataGeral = (from t in ctx.CobFullClaro_Sum
                                 where t.Report_Month == mes && t.Report_Year == ano
                                 select new
                                 {
                                     Telefone = t.Telefone,
                                     Result = t.Resultado___linha.ToString()
                                 }).ToList();


                var dataVivo = (from t in ctx.CobFullVivo_Sum
                                where t.Report_Month == mes && t.Report_Year == ano
                                select new
                                {
                                    Telefone = t.Telefone.Replace(" ", ""),
                                    Result = t.Resultado___da_linha
                                }).ToList();


                dataGeral.AddRange(dataVivo);

                var planDetails = ctx.tblPlansOptions.Where(p => p.txtDescription.Length > 0).ToList();
                var totalBoletoCharges = 0;

                var customer = ctx.tblPersons
                    .Where(p => p.txtDocumentNumber.Length > 0 && p.intIdPerson == customerId)
                    .Select(p => new Person
                    {
                        Id = p.intIdPerson,
                        Name = p.txtName,
                        Email = p.txtEmail,
                        DocumentNumber = p.txtDocumentNumber,
                        IdPagarme = p.intIdPagarme,
                        Phones = ctx.tblPersonsPhones
                            .Where(pp => pp.intIdPerson == p.intIdPerson && pp.bitAtivo == true && pp.bitPhoneClube == true)
                            .Select(pp => new Phone
                            {
                                DDD = pp.intDDD.Value.ToString(),
                                Number = pp.intPhone.Value.ToString(),
                                IdPlanOption = pp.intIdPlan.Value

                            }).ToList()
                    }).ToList().Distinct();


                //eh possivel reaproveitar ra n pegar telefone novamente ali a baixo
                var clientes = new List<Person>();
                var allPhones = new List<Phone>();
                var chargingList = GetCustomerValidityCharginHistory(charging, customerId);

                foreach (var charge in chargingList)
                {
                    if (bool.Equals(clientes.FirstOrDefault(p => p.Id == charge.intIdCustomer.Value), null))
                    {
                        var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == charge.intIdCustomer.Value);
                        var phones = ctx.tblPersonsPhones
                            .Where(p => p.intIdPerson == pessoa.intIdPerson && p.bitAtivo == true && p.bitPhoneClube == true)
                            .Select(p => new Phone
                            {
                                DDD = p.intDDD.Value.ToString(),
                                Number = p.intPhone.Value.ToString(),
                                IdPlanOption = p.intIdPlan.Value

                            }).ToList();

                        allPhones.AddRange(phones);

                        var cliente = new Person
                        {
                            Id = charge.intIdCustomer.Value,
                            Name = pessoa.txtName.Trim(),
                            Email = pessoa.txtEmail.Trim(),
                            IdPagarme = pessoa.intIdPagarme,
                            Phones = bool.Equals(phones, null) ? new List<Phone>() : phones,
                            Charged = true
                        };

                        cliente.ChargingValidity = new List<Charging>();
                        var clienteChargingList = chargingList.Where(c => c.intIdCustomer == cliente.Id).ToList();

                        foreach (var chargeCustomer in clienteChargingList)
                        {
                            if (chargeCustomer.intIdPaymentType == (int)TipoPagamento.boletoPG)
                            {
                                totalBoletoCharges++;
                            }

                            cliente.ChargingValidity.Add(new Charging
                            {
                                Ammount = chargeCustomer.txtAmmountPayment,
                                ChargingComment = chargeCustomer.txtChargingComment,
                                ChargingDate = chargeCustomer.dteCreate.Value,
                                Charged = true,
                                Payd = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? true : false,
                                Expired = false,
                                PaymentType = chargeCustomer.intIdPaymentType,
                                BoletoId = chargeCustomer.intIdBoleto.HasValue ? chargeCustomer.intIdBoleto.Value : -1,
                                StatusDescription = (chargeCustomer.intIdPaymentType == (int)TipoPagamento.card) ? "PAGO" : "CARREGANDO"

                            });
                        }

                        foreach (var client in clientes)
                        {
                            client.TotalBoletoCharges = totalBoletoCharges;
                        }

                        clientes.Add(cliente);
                    }
                }

                foreach (var person in customer)
                {
                    if (bool.Equals(clientes.FirstOrDefault(c => c.Id == person.Id), null))
                        clientes.Add(person);
                }

                var lastPayments = new TransactionAccess().GetCustomerLastTransactionPaid(Convert.ToInt32(customer.FirstOrDefault().IdPagarme));

                foreach (var person in clientes)
                {
                    try
                    {
                        person.LastPaidDate = Convert.ToDateTime(lastPayments.FirstOrDefault(p => p.intIdCustomer == person.IdPagarme).dteDate_updated).ToString("yyyy/MM/dd");
                    }
                    catch (Exception)
                    {
                        person.LastPaidDate = null;
                    }

                    foreach (var phone in person.Phones)
                    {
                        var detail = planDetails.FirstOrDefault(d => d.intIdPlan == phone.IdPlanOption);
                        phone.PlanDescription = !bool.Equals(detail, null) ? detail.txtDescription : "";
                        phone.IdOperator = !bool.Equals(detail, null) ? detail.intIdOperator : 0;

                        var claro = 1;
                        var vivo = 2;

                        //todo
                        //var cobranca = dataGeral.FirstOrDefault(l => l.Telefone == phone.DDD + phone.Number);
                        //phone.ResultValue = !bool.Equals(cobranca, null) ? cobranca.Result.ToString() : "null";
                        //if (string.IsNullOrEmpty(phone.ResultValue))
                        //{
                        //    phone.ResultValue = "null";
                        //}

                    }
                }


                return clientes.OrderBy(c => c.Charged == false).ToList();
            }
        }

        public List<CobFullClaro_Sum> GetCobrancaClaroProc(int mes, int ano)
        {
            using (var ctx = new FoneClubeContext())
            {
                return new List<CobFullClaro_Sum>();
            }
        }

        public List<CobFullClaro_Sum> GetCobrancaFullClaro(int mes, int ano)
        {
            using (var ctx = new FoneClubeContext())
            {
                return ctx.CobFullClaro_Sum.Where(c => c.Report_Month == mes && c.Report_Year == ano).ToList();
            }
        }

        public List<CobFullVivo_Sum> GetCobrancaFullVivo(int mes, int ano)
        {
            using (var ctx = new FoneClubeContext())
            {
                return ctx.CobFullVivo_Sum.Where(c => c.Report_Month == mes && c.Report_Year == ano).ToList();
            }
        }

        public bool SetChargingLog(string charging, string matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblChargingLog.Add(new tblChargingLog
                    {
                        intIdPerson = Convert.ToInt32(matricula),
                        txtChargingLog = charging,
                        dteUpdate = DateTime.Now
                    });

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public List<String> GetChargingList(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var listaCobrancas = new List<string>();
                    var listaChargingLog = ctx.tblChargingLog
                                            .Where(c => c.intIdPerson == matricula)
                                            .OrderByDescending(l => l.dteUpdate)
                                            .ToList();

                    foreach (var charging in listaChargingLog)
                        listaCobrancas.Add(charging.txtChargingLog);

                    return listaCobrancas;
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public GetLastChargingHistory_Result GetLastCharge(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetLastChargingHistory(matricula).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return new GetLastChargingHistory_Result();
            }
        }

        public List<LastCharging> GetLastChargings(List<int> matriculas)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var chargings = new List<LastCharging>();
                    var historico = ctx.tblChargingHistory.OrderByDescending(m => m.dtePayment);
                    foreach (var matricula in matriculas.Where(n => n > 0).ToList())
                    {
                        var convertido = Convert.ToInt32(matricula);
                        var charge = historico.FirstOrDefault(c => c.intIdCustomer == convertido);
                        if (!bool.Equals(charge, null))
                        {
                            var amount = Convert.ToInt32(charge.txtAmmountPayment);
                            chargings.Add(new LastCharging
                            {
                                ClientId = convertido,
                                CommentBoleto = !string.IsNullOrEmpty(charge.txtCommentBoleto) ? charge.txtCommentBoleto : string.Empty,
                                CommentEmail = !string.IsNullOrEmpty(charge.txtCommentEmail) ? charge.txtCommentEmail : string.Empty,
                                CommentFoneclube = !string.IsNullOrEmpty(charge.txtComment) ? charge.txtComment : string.Empty,
                                Amount = !string.IsNullOrEmpty(charge.txtAmmountPayment) ? amount : 0,
                                ChargeType = charge.intIdPaymentType,
                                DateCharged = charge.dteCreate
                            });
                        }
                        else
                        {
                            chargings.Add(new LastCharging
                            {
                                ClientId = convertido
                            });
                        }

                    }

                    return chargings;
                }
            }
            catch (Exception e)
            {
                return new List<LastCharging>();
            }
        }


        public bool UpdateCharging(int chargingId, bool canceled)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var charging = ctx.tblChargingHistory.FirstOrDefault(c => c.intId == chargingId);
                    charging.bitCanceled = canceled;
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool UpdateChargingList(List<int> chargingListId, bool canceled)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    foreach (var chargingId in chargingListId)
                    {
                        var charging = ctx.tblChargingHistory.FirstOrDefault(c => c.intId == chargingId);
                        charging.bitCanceled = canceled;
                    }
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public List<Person> GetListaCobrancaMassiva(int mes, int ano)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var clientes = new List<Person>();
                    var linhasDetalhadas = ctx.GetLinhasComDetalhesCobrancaMassiva().ToList();
                    var documentoClientes = linhasDetalhadas.Select(c => c.txtDocumentNumber).Distinct().ToList();
                    var valoresCobrarVivo = ctx.GetValoresCobrarVivo(mes, ano).ToList();
                    var valoresCobrarClaro = ctx.GetValoresCobrarClaro(mes, ano).ToList();

                    var lastChargings = GetLastChargings(linhasDetalhadas.Select(a => a.intIdPerson.Value).Distinct().ToList());

                    var cobrancasVingencia = GetValidityPayments(new Charging
                    {
                        AnoVingencia = (ano).ToString(),
                        MesVingencia = (mes).ToString()
                    });

                    var clientesCobrados = cobrancasVingencia.Where(p => p.Charged).Select(q => new Person { Id = q.Id });

                    foreach (var documento in documentoClientes)
                    {
                        var linhasDetalhadasCliente = linhasDetalhadas.Where(p => p.txtDocumentNumber == documento).ToList();
                        var detalhesCliente = linhasDetalhadasCliente.FirstOrDefault(c => c.txtDocumentNumber == documento);
                        var detalhesCobrancaVivoCliente = valoresCobrarVivo.Where(a => a.cpf == documento).ToList();
                        var detalhesCobrancaClaroCliente = valoresCobrarClaro.Where(a => a.cpf == documento).ToList();
                        var clientId = Convert.ToInt32(detalhesCliente.intIdPerson);
                        var ultimaCobranca = lastChargings.FirstOrDefault(c => c.ClientId == clientId);

                        var cliente = new Person
                        {
                            Id = clientId,
                            DocumentNumber = documento,
                            Name = detalhesCliente.txtName,
                            Email = detalhesCliente.txtEmail,
                            IdPagarme = Convert.ToInt32(detalhesCliente.intIdPagarme),
                            SinglePrice = Convert.ToInt64(detalhesCliente.intAmmountPrecoUnico),
                            DescriptionSinglePrice = detalhesCliente.txtDescriptionPrecoUnico,
                            HasSinglePrice = Convert.ToInt64(detalhesCliente.intAmmountPrecoUnico) > 0,
                            TotalAmountCustomer = Convert.ToInt64(detalhesCliente.intAmmountPrecoUnico),
                            Phones = new List<Phone>(),
                            Charged = clientesCobrados.Any(p => p.Id == detalhesCliente.intIdPerson)
                        };

                        if (!bool.Equals(ultimaCobranca, null))
                            cliente.LastCharge = ultimaCobranca;

                        foreach (var telefone in linhasDetalhadasCliente)
                        {
                            var chargeAmount = 0;
                            var resultAmount = 0;
                            var operadora = 0;
                            var valorCobradoOperadora = 0;
                            var operadoraDescription = string.Empty;

                            var currentNumber = telefone.intDDD.ToString().Trim() + telefone.intPhone.ToString().Trim();
                            var phoneVivo = detalhesCobrancaVivoCliente.FirstOrDefault(c => c.telefone == currentNumber);

                            if (!bool.Equals(phoneVivo, null))
                            {
                                if (!bool.Equals(phoneVivo.valorTotalCobrarLinha, null))
                                {
                                    chargeAmount = Convert.ToInt32(phoneVivo.valorTotalCobrarLinha.ToString().Replace(".", ""));
                                    resultAmount = Convert.ToInt32(phoneVivo.resultadoLinha.ToString().Replace(".", ""));
                                    valorCobradoOperadora = Convert.ToInt32(phoneVivo.valorOperadora.ToString().Replace(".", ""));
                                    operadora = Utils.OperatorType.Vivo;
                                    operadoraDescription = Utils.OperatorType.VivoDescription;
                                }
                            }

                            var phoneClaro = detalhesCobrancaClaroCliente.FirstOrDefault(c => c.Telefone == currentNumber);
                            if (!bool.Equals(phoneClaro, null))
                            {
                                if (!bool.Equals(phoneClaro.valorTotalCobrarLinha, null))
                                {
                                    chargeAmount = Convert.ToInt32(phoneClaro.valorTotalCobrarLinha.ToString().Replace(".", ""));
                                    resultAmount = Convert.ToInt32(phoneClaro.resultadoLinha.ToString().Replace(".", ""));
                                    valorCobradoOperadora = Convert.ToInt32(phoneClaro.valorOperadora.ToString().Replace(".", ""));
                                    operadora = Utils.OperatorType.Claro;
                                    operadoraDescription = Utils.OperatorType.ClaroDescription;
                                }
                            }

                            var phone = new Phone
                            {
                                Number = telefone.intPhone.ToString(),
                                DDD = telefone.intDDD.ToString(),
                                IdPlanOption = telefone.intIdPlan,
                                PlanDescription = telefone.txtDescription,
                                Ammount = telefone.intCost.ToString(),
                                NickName = telefone.txtNickname,
                                AmmountPrecoVip = telefone.intAmmoutPrecoVip,
                                PrecoVipStatus = telefone.bitPrecoVip,
                                ChargeAmount = chargeAmount,
                                ResultAmount = resultAmount,
                                IdOperator = operadora,
                                OperatorDescription = operadoraDescription,
                                OperatorChargedPrice = valorCobradoOperadora
                            };

                            cliente.Phones.Add(phone);
                        }

                        clientes.Add(cliente);
                    }

                    return clientes;

                }
            }
            catch (Exception e)
            {
                return new List<Person>();
            }
        }

        public bool SaveChargingHistoryStore(Person person)
        {
            try
            {
                person.Charging.Comment = "Checkout Loja Pagarme";
                person.Charging.CollectorName = "Checkout Pagarme";
                person.Charging.AnoVingencia = "1999";
                person.Charging.MesVingencia = "09";
                person.Charging.IdCollector = 1;
                person.Charging.ChargeStatus = 1;
                person.Charging.CacheTransaction = false;

                using (var ctx = new FoneClubeContext())
                {
                    if (person.Id > 0)
                    {
                        return new ProfileAccess().SaveChargingHistory(person) == HttpStatusCode.OK;
                    }
                    else
                    {
                        var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == person.DocumentNumber);
                        person.Id = tblPerson.intIdPerson;
                        return new ProfileAccess().SaveChargingHistory(person) == HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public bool SaveCard(CardFoneclube card, Person person, int cardType = 2)
        {
            try
            {
                //todo
                using (var ctx = new FoneClubeContext())
                {
                    tblEmailLog email;

                    if (person.Id > 0)
                    {
                        // DebitCard=1, CreditCard=2, OnlineTransfer=3, Boleto=4
                        email = new tblEmailLog
                        {
                            intIdTypeEmail = cardType,
                            intIdPerson = person.Id,
                            txtEmail = new CardUtils().EncryptFC(new CardUtils().PrepareCard(card)),
                            dteRegister = DateTime.Now
                        };
                    }
                    else
                    {
                        var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == person.DocumentNumber);
                        person.Id = tblPerson.intIdPerson;
                        email = new tblEmailLog
                        {
                            intIdTypeEmail = cardType,
                            intIdPerson = person.Id,
                            txtEmail = new CardUtils().EncryptFC(new CardUtils().PrepareCard(card)),
                            dteRegister = DateTime.Now
                        };
                    }

                    ctx.tblEmailLog.Add(email);
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public MassChargingList GetMassChargingCustomers(int mes, int ano)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var chargingList = new MassChargingList();
                    var lastRecord = ctx.tblMassChargingLog.OrderByDescending(p => p.dteRegistro).FirstOrDefault();
                    chargingList.Id = lastRecord != null ? lastRecord.intId + 1 : 1;

                    chargingList.Mes = mes;
                    chargingList.Ano = ano;
                    chargingList.MassCharging = new List<MassChargingModel>();

                    var valoresOperadoraClaro = ctx.GetValoresOperadoraClaro(mes, ano).ToList();

                    var clientesAtivos = ctx.tblPersons.Where(p => (p.bitManual == false || p.bitManual == null) && (p.bitDesativoManual == false || p.bitDesativoManual == null));
                    var cpfs = valoresOperadoraClaro.Where(c => c.documento != null).Select(c => c.documento.Trim()).Distinct().ToList();
                    var clientesProc = ctx.tblPersons.Where(p => cpfs.Contains(p.txtDocumentNumber.Trim())).ToList();
                    var idsClientes = clientesProc.Select(c => c.intIdPerson).ToList();

                    var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();
                    var tblCharges = ctx.tblChargingHistory
                                        .Where(c => c.intIdCustomer.HasValue && idsClientes.Contains(c.intIdCustomer.Value))
                                        .ToList().OrderByDescending(x => x.dteCreate).ToList();

                    var operadoras = ctx.tblOperadoras.ToList();
                    var tblDiscount = ctx.tblDiscountPrice.ToList();
                    var telefones = ctx.tblPersonsPhones.Where(c => idsClientes.Contains(c.intIdPerson.Value) && c.bitPhoneClube == true).ToList();
                    var plans = ctx.tblPlansOptions.ToList();

                    var cobrancasMesVingente = ctx.tblChargingHistory
                        .Where(c => c.dteValidity == new DateTime(ano, mes, 1)).ToList();

                    //clientesAtivos
                    foreach (var cliente in clientesProc)
                    {
                        var discount = tblDiscount.FirstOrDefault(d => d.intIdPerson == cliente.intIdPerson);
                        var singlePrice = "-1";

                        if (discount != null)
                        {
                            if (discount.intAmmount != null)
                                singlePrice = discount.intAmmount.ToString();
                        }

                        var operadoraLinhas = valoresOperadoraClaro
                                                .Where(c => c.documento == cliente.txtDocumentNumber).ToList();

                        var valorTotalCobradoOperadora = operadoraLinhas
                                                            .Select(c => c.custoOperadora).ToList().Sum().Value.ToString("N2").Replace(".", string.Empty).Replace(",", string.Empty);

                        var cardType = 1;
                        //oder persons by name
                        var chargingResponse = new MassChargingModel
                        {
                            IdPerson = cliente.intIdPerson,
                            CPF = cliente.txtDocumentNumber,
                            Email = cliente.txtEmail,
                            Name = cliente.txtName,
                            PrecoUnico = singlePrice,
                            ValorOperadoraTotalLinhas = valorTotalCobradoOperadora,
                            Charged = cobrancasMesVingente.Any(p => p.intIdCustomer == cliente.intIdPerson),
                            HasCard = ctx.tblChargingHistory.Any(c => c.intIdPaymentType == cardType && c.intIdCustomer == cliente.intIdPerson),
                            ChargeDoMes = cobrancasMesVingente.Where(c => c.intIdCustomer == cliente.intIdPerson).Select(c => new Charging
                            {
                                Id = c.intId,
                                BoletoId = Convert.ToInt64(c.intIdBoleto),
                                TransactionId = c.intIdTransaction,
                                PaymentType = c.intIdPaymentType
                            }).FirstOrDefault()

                        };

                        var lastTransactionPaid = lastPayments.FirstOrDefault(p => p.intIdCustomer == cliente.intIdPagarme);
                        if (lastTransactionPaid != null)
                        {
                            var paidCharge = tblCharges.FirstOrDefault(c => c.intIdTransaction == lastTransactionPaid.intIdTransaction);
                            if (paidCharge != null)
                            {
                                chargingResponse.LastChargingPaid = new Charging
                                {
                                    CreateDate = paidCharge.dteCreate,
                                    PaymentType = paidCharge.intIdPaymentType,
                                    Ammount = paidCharge.txtAmmountPayment,
                                    Comment = paidCharge.txtComment,
                                    AnoVingencia = Convert.ToDateTime(paidCharge.dteValidity).Year.ToString(),
                                    MesVingencia = Convert.ToDateTime(paidCharge.dteValidity).Month.ToString(),
                                    Payd = true
                                };
                            }

                        }

                        var lastCharging = tblCharges.FirstOrDefault(c => c.intIdCustomer == cliente.intIdPerson);
                        if (lastCharging != null)
                        {
                            chargingResponse.LastCharging = new Charging
                            {
                                CreateDate = lastCharging.dteCreate,
                                PaymentType = lastCharging.intIdPaymentType,
                                Ammount = lastCharging.txtAmmountPayment,
                                Comment = lastCharging.txtComment,
                                AnoVingencia = Convert.ToDateTime(lastCharging.dteValidity).Year.ToString(),
                                MesVingencia = Convert.ToDateTime(lastCharging.dteValidity).Month.ToString(),
                            };
                        }

                        chargingResponse.Phones = telefones.Where(c => c.intIdPerson == cliente.intIdPerson)
                            .Select(p => new PhoneModel
                            {
                                Id = p.intId,
                                DDD = p.intDDD.ToString(),
                                Number = p.intPhone.ToString(),
                                IdPlanOption = p.intIdPlan,
                                //Operator = operadoras.FirstOrDefault(o => o.intIdOperator == p.intIdOperator).txtName, 
                                OperatorId = p.intIdOperator,
                                NickName = p.txtNickname,
                                Ammount = plans.FirstOrDefault(a => a.intIdPlan == p.intIdPlan) != null ? plans.FirstOrDefault(a => a.intIdPlan == p.intIdPlan).intCost.ToString() : string.Empty,
                                AmmountVIP = p.intAmmoutPrecoVip,
                                OperatorAmount = operadoraLinhas.FirstOrDefault(o => o.telefone == p.intDDD.ToString() + p.intPhone.ToString()) != null ? operadoraLinhas.FirstOrDefault(o => o.telefone == p.intDDD.ToString() + p.intPhone.ToString()).custoOperadora : null
                            }).ToList();

                        bool precoUnicoMenor = false;
                        bool precoFCMenor = false;

                        var totalFC = 0;
                        foreach (var phone in chargingResponse.Phones)
                        {
                            if (string.IsNullOrEmpty(phone.Ammount))
                                phone.Ammount = "0";

                            if (Convert.ToInt32(phone.Ammount) > 0)
                                totalFC += Convert.ToInt32(phone.Ammount);
                        }

                        try
                        {
                            if (Convert.ToInt32(chargingResponse.PrecoUnico) > 0)
                                precoUnicoMenor = Convert.ToInt32(chargingResponse.PrecoUnico) < Convert.ToInt32(chargingResponse.ValorOperadoraTotalLinhas);
                            else
                                precoFCMenor = totalFC < Convert.ToInt32(chargingResponse.ValorOperadoraTotalLinhas);
                        }
                        catch (Exception)
                        {
                            if (Convert.ToInt64(chargingResponse.PrecoUnico) > 0)
                                precoUnicoMenor = Convert.ToInt64(chargingResponse.PrecoUnico) < Convert.ToInt64(chargingResponse.ValorOperadoraTotalLinhas);
                            else
                                precoFCMenor = totalFC < Convert.ToInt64(chargingResponse.ValorOperadoraTotalLinhas);
                        }


                        chargingResponse.GoodToCharge = true;
                        chargingResponse.FonteDoTotalCobrar = Convert.ToInt64(chargingResponse.PrecoUnico) > 0 ? ChargingTarget.PrecoUnico : ChargingTarget.Plano;

                        if (precoUnicoMenor)
                        {
                            chargingResponse.Reason = "O cliente não dá lucro considerando o preço único";
                            chargingResponse.GoodToCharge = false;
                        }

                        if (precoFCMenor)
                        {
                            chargingResponse.Reason = "O cliente não dá lucro considerando os valores de plano Foneclube";
                            chargingResponse.GoodToCharge = false;
                        }


                        if (Convert.ToInt64(chargingResponse.PrecoUnico) == 0)
                        {
                            chargingResponse.GoodToCharge = false;
                            chargingResponse.Reason = "Não é possível realizar uma cobrança a custo zero";
                        }

                        chargingResponse.ValorTotalCobranca = chargingResponse.FonteDoTotalCobrar == ChargingTarget.PrecoUnico ? Convert.ToInt64(chargingResponse.PrecoUnico).ToString() : totalFC.ToString();
                        chargingList.MassCharging.Add(chargingResponse);
                    }

                    chargingList.MassCharging = chargingList.MassCharging.OrderBy(a => a.Name).ToList();

                    var ordemMassiva = new tblMassChargingLog
                    {
                        dteRegistro = DateTime.Now,
                        txtMassCharging = JsonConvert.SerializeObject(chargingList)
                    };

                    ctx.tblMassChargingLog.Add(ordemMassiva);
                    ctx.SaveChanges();

                    chargingList.Id = ordemMassiva.intId;
                    return chargingList;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public MassChargingList GetMassChargingCustomers(int mes, int ano, int customerId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var chargingList = new MassChargingList();

                    var lastRecord = ctx.tblMassChargingLog.OrderByDescending(p => p.dteRegistro).FirstOrDefault();
                    chargingList.Id = lastRecord != null ? lastRecord.intId + 1 : 1;

                    chargingList.Mes = mes;
                    chargingList.Ano = ano;
                    chargingList.MassCharging = new List<MassChargingModel>();

                    var valoresOperadoraClaro = ctx.GetValoresOperadoraClaro(mes, ano).ToList();

                    var cpfs = valoresOperadoraClaro.Where(c => c.documento != null).Select(c => c.documento.Trim()).Distinct().ToList();
                    var clientesProc = ctx.tblPersons.Where(p => cpfs.Contains(p.txtDocumentNumber.Trim())).ToList();
                    var idsClientes = clientesProc.Select(c => c.intIdPerson).ToList();

                    var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();
                    var tblCharges = ctx.tblChargingHistory
                                        .Where(c => c.intIdCustomer.HasValue && idsClientes.Contains(c.intIdCustomer.Value))
                                        .ToList().OrderByDescending(x => x.dteCreate).ToList();

                    var tblDiscount = ctx.tblDiscountPrice.ToList();
                    var telefones = ctx.tblPersonsPhones.Where(c => idsClientes.Contains(c.intIdPerson.Value) && c.bitPhoneClube == true).ToList();
                    var plans = ctx.tblPlansOptions.ToList();

                    foreach (var cliente in clientesProc)
                    {
                        var discount = tblDiscount.FirstOrDefault(d => d.intIdPerson == cliente.intIdPerson);
                        var singlePrice = "-1";

                        if (discount != null)
                        {
                            if (discount.intAmmount != null)
                                singlePrice = discount.intAmmount.ToString();
                        }

                        var operadoraLinhas = valoresOperadoraClaro
                                                .Where(c => c.documento == cliente.txtDocumentNumber).ToList();

                        var valorTotalCobradoOperadora = operadoraLinhas
                                                            .Select(c => c.custoOperadora).ToList().Sum().Value.ToString("N2").Replace(".", string.Empty).Replace(",", string.Empty);

                        var chargingResponse = new MassChargingModel
                        {
                            IdPerson = cliente.intIdPerson,
                            CPF = cliente.txtDocumentNumber,
                            Email = cliente.txtEmail,
                            Name = cliente.txtName,
                            PrecoUnico = singlePrice,
                            ValorOperadoraTotalLinhas = valorTotalCobradoOperadora
                        };

                        var lastTransactionPaid = lastPayments.FirstOrDefault(p => p.intIdCustomer == cliente.intIdPagarme);
                        if (lastTransactionPaid != null)
                        {
                            var paidCharge = tblCharges.FirstOrDefault(c => c.intIdTransaction == lastTransactionPaid.intIdTransaction);
                            if (paidCharge != null)
                            {
                                chargingResponse.LastChargingPaid = new Charging
                                {
                                    CreateDate = paidCharge.dteCreate,
                                    PaymentType = paidCharge.intIdPaymentType,
                                    Ammount = paidCharge.txtAmmountPayment,
                                    Comment = paidCharge.txtComment,
                                    AnoVingencia = Convert.ToDateTime(paidCharge.dteValidity).Year.ToString(),
                                    MesVingencia = Convert.ToDateTime(paidCharge.dteValidity).Month.ToString(),
                                    Payd = true
                                };
                            }

                        }

                        var lastCharging = tblCharges.FirstOrDefault(c => c.intIdCustomer == cliente.intIdPagarme);
                        if (lastCharging != null)
                        {
                            chargingResponse.LastCharging = new Charging
                            {
                                CreateDate = lastCharging.dteCreate,
                                PaymentType = lastCharging.intIdPaymentType,
                                Ammount = lastCharging.txtAmmountPayment,
                                Comment = lastCharging.txtComment,
                                BoletoId = Convert.ToInt64(lastCharging.intIdBoleto),
                                AnoVingencia = Convert.ToDateTime(lastCharging.dteValidity).Year.ToString(),
                                MesVingencia = Convert.ToDateTime(lastCharging.dteValidity).Month.ToString(),
                                TransactionId = lastCharging.intIdTransaction
                            };
                        }

                        chargingResponse.Phones = telefones.Where(c => c.intIdPerson == cliente.intIdPerson).Select(p => new PhoneModel
                        {
                            Id = p.intId,
                            DDD = p.intDDD.ToString(),
                            Number = p.intPhone.ToString(),
                            IdPlanOption = p.intIdPlan,
                            Ammount = plans.FirstOrDefault(a => a.intIdPlan == p.intIdPlan) != null ? plans.FirstOrDefault(a => a.intIdPlan == p.intIdPlan).intCost.ToString() : string.Empty,
                            AmmountVIP = p.intAmmoutPrecoVip,
                            OperatorAmount = operadoraLinhas.FirstOrDefault(o => o.telefone == p.intDDD.ToString() + p.intPhone.ToString()) != null ? operadoraLinhas.FirstOrDefault(o => o.telefone == p.intDDD.ToString() + p.intPhone.ToString()).custoOperadora : null
                        }).ToList();

                        bool precoUnicoMenor = false;
                        bool precoFCMenor = false;

                        var totalFC = 0;
                        foreach (var phone in chargingResponse.Phones)
                        {
                            if (string.IsNullOrEmpty(phone.Ammount))
                                phone.Ammount = "0";

                            if (Convert.ToInt32(phone.Ammount) > 0)
                                totalFC += Convert.ToInt32(phone.Ammount);
                        }

                        if (Convert.ToInt32(chargingResponse.PrecoUnico) > 0)
                            precoUnicoMenor = Convert.ToInt32(chargingResponse.PrecoUnico) < Convert.ToInt32(chargingResponse.ValorOperadoraTotalLinhas);
                        else
                            precoFCMenor = totalFC < Convert.ToInt32(chargingResponse.ValorOperadoraTotalLinhas);

                        chargingResponse.GoodToCharge = precoUnicoMenor || precoFCMenor;
                        chargingResponse.FonteDoTotalCobrar = Convert.ToInt32(chargingResponse.PrecoUnico) > 0 ? ChargingTarget.PrecoUnico : ChargingTarget.Plano;

                        if (precoUnicoMenor)
                            chargingResponse.Reason = "O cliente não dá lucro considerando o preço único";

                        if (precoFCMenor)
                            chargingResponse.Reason = "O cliente não dá lucro considerando os valores de plano Foneclube";

                        if (Convert.ToInt32(chargingResponse.PrecoUnico) == 0)
                            chargingResponse.GoodToCharge = false;
                        chargingResponse.Reason = "Não é possível realizar uma cobrança a custo zero";


                        chargingResponse.ValorTotalCobranca = chargingResponse.FonteDoTotalCobrar == ChargingTarget.PrecoUnico ? Convert.ToInt32(chargingResponse.PrecoUnico).ToString() : totalFC.ToString();
                        chargingList.MassCharging.Add(chargingResponse);
                    }

                    var ordemMassiva = new tblMassChargingLog
                    {
                        dteRegistro = DateTime.Now,
                        txtMassCharging = JsonConvert.SerializeObject(chargingList)
                    };

                    ctx.tblMassChargingLog.Add(ordemMassiva);
                    ctx.SaveChanges();

                    chargingList.Id = ordemMassiva.intId;
                    return chargingList;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public bool SetupAutomaticCharge(int ordemDeCobranca, MassChargingList chargingModel)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var massCharging = ctx.tblMassChargingLog.FirstOrDefault(c => c.intId == ordemDeCobranca);
                    var response = JsonConvert.DeserializeObject<MassChargingList>(massCharging.txtMassCharging);

                    var customersToCharge = response.MassCharging.Where(o => o.GoodToCharge == true).ToList();

                    //todo usar esse a baixo
                    var customersToChargeClient = chargingModel.MassCharging.Where(o => o.GoodToCharge == true).ToList();

                    foreach (var charge in customersToCharge)
                    {

                        var modelFromClient = chargingModel.MassCharging.FirstOrDefault(c => c.IdPerson == charge.IdPerson);
                        TransactionResult result;

                        if (charge.LastCharging == null)
                            charge.LastCharging = new Charging { PaymentType = (int)PaymentTypes.boleto };

                        if (charge.LastCharging.PaymentType == (int)PaymentTypes.boleto)
                        {
                            result = new PagarmeAccess().GeraCobrancaIntegrada(new Person
                            {
                                Id = charge.IdPerson,
                                Charging = new Charging
                                {
                                    Ammount = charge.ValorTotalCobranca,
                                    PaymentType = (int)PaymentTypes.boleto,
                                    CommentBoleto = string.Empty,
                                    Comment = string.Format("Cobrança massiva de: {0} de {1}, ordem: {2}", response.Mes, response.Ano, ordemDeCobranca)
                                }
                            });

                            result.PaymentType = (int)PaymentTypes.boleto;
                        }
                        else
                        {
                            result = new PagarmeAccess().GeraCobrancaIntegrada(new Person
                            {
                                Id = charge.IdPerson,
                                Charging = new Charging
                                {
                                    Ammount = charge.ValorTotalCobranca,
                                    PaymentType = (int)PaymentTypes.card,
                                    Comment = string.Format("Cobrança massiva de: {0} de {1}, ordem: {2}", response.Mes, response.Ano, ordemDeCobranca)
                                }
                            });

                            result.PaymentType = (int)PaymentTypes.card;
                        }

                        charge.ResulTransaction = result;
                        bool emailEnviado = false;

                        if (charge.ResulTransaction.StatusPaid)
                        {
                            if (charge.ResulTransaction.PaymentType == (int)PaymentTypes.card)
                            {
                                var valor = Convert.ToInt32(charge.ValorTotalCobranca);
                                emailEnviado = new EmailAccess().SendEmail(new Email
                                {
                                    To = charge.Email,
                                    TargetName = charge.Name,
                                    TargetTextBlue = (valor / 100).ToString("N2").Replace(".", ","),
                                    TemplateType = Convert.ToInt32(Email.TemplateTypes.CardCharged)
                                });
                            }

                            if (charge.ResulTransaction.PaymentType == (int)PaymentTypes.boleto)
                            {
                                emailEnviado = new EmailAccess().SendEmail(new Email
                                {
                                    To = charge.Email,
                                    TargetName = charge.Name,
                                    TargetSecondaryText = charge.ResulTransaction.LinkBoleto,
                                    TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                                });
                            }
                        }

                        charge.EmailEnviado = emailEnviado;
                    }

                    var saveLog = SaveMassChargeLog(customersToCharge, ordemDeCobranca, ctx, true);

                    if (!saveLog)
                        new Utils().SendEmail("rodrigocardozop@gmail.com", "Backup de execução de cobrança", JsonConvert.SerializeObject(customersToCharge), true);
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }

            return true;
        }

        private bool SaveMassChargeLog(List<MassChargingModel> customersToCharge, int chargingOrderId, FoneClubeContext ctx, bool charged = false)
        {
            try
            {
                ctx.tblMassChargingLog.Add(new tblMassChargingLog
                {
                    dteRegistro = DateTime.Now,
                    intIdMassCharging = chargingOrderId,
                    txtMassCharging = JsonConvert.SerializeObject(customersToCharge),
                    bitCharged = charged
                });
                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public List<MassChargingModel> GetMassChargeLog(int chargeOrderId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var log = ctx.tblMassChargingLog.FirstOrDefault(c => c.intIdMassCharging == chargeOrderId);
                    var result = JsonConvert.DeserializeObject<List<MassChargingModel>>(log.txtMassCharging);
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public string GetStatusBoletoBradesco()
        {
            try
            {
                var url = ConfigurationManager.AppSettings["LINK_BRADESCO_BOLETO"];
                int contagem = 0;

                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            var result = content.ReadAsStringAsync().Result;

                            var separetor = new string[] { "<br>" };
                            var statusResults = result.Split(separetor, StringSplitOptions.None);


                            foreach (string statusResponse in statusResults)
                            {
                                if (statusResponse.Contains("pago"))
                                {
                                    var nicho = statusResponse.Split(' ');
                                    var idTransactionLoja = Convert.ToInt32(nicho[2]);

                                    using (var ctx = new FoneClubeContext())
                                    {
                                        var charge = ctx.tblChargingHistory.FirstOrDefault(c => c.intIdCheckoutLoja == idTransactionLoja);

                                        if (charge == null)
                                        {
                                            charge.bitPago = true;
                                            ctx.SaveChanges();
                                            contagem++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return "Procedimento sem erro, contagem de pagamentos atualizados: " + contagem;


            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public List<Person> GetChargingsCustomers()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var chargings = ctx.tblChargingHistory.ToList();

                    var clientes = ctx.tblPersons
                                      .Where(a => a.bitDelete != true && a.bitDesativoManual != true)
                                      .Select(p => new Person { Name = p.txtName, DocumentNumber = p.txtDocumentNumber, Id = p.intIdPerson, IdPagarme = p.intIdPagarme })
                                      .ToList();

                    foreach (var cliente in clientes)
                    {
                        var charging = chargings
                                    .Where(p => p.intIdCustomer == cliente.Id)
                                    .OrderByDescending(x => x.dteCreate)
                                    .FirstOrDefault();

                        if (charging != null)
                        {
                            cliente.Charging = new Charging();
                            cliente.Charging.ChargingDate = charging.dteCreate;
                            var difference = (TimeSpan)(DateTime.Now - charging.dteCreate);
                            cliente.Charging.DiasSemCobrar = Convert.ToInt32(difference.TotalDays);
                        }

                    }

                    var lastPayments = new TransactionAccess().GetAllLastTransactionPaid();

                    foreach (var person in clientes)
                    {
                        try
                        {
                            var transaction = lastPayments.FirstOrDefault(p => p.intIdCustomer == person.IdPagarme);
                            person.LastPaidDate = Convert.ToDateTime(lastPayments.FirstOrDefault(p => p.intIdCustomer == person.IdPagarme).dteDate_updated).ToString("yyyy/MM/dd");
                        }
                        catch (Exception)
                        {
                            person.LastPaidDate = null;
                        }
                    }

                    return clientes;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public List<Agendamento> GetScheduleChargeHistory(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var listaAgendamento = new List<Agendamento>();
                    var lista = ctx.tblChargingScheduled.Where(a => a.intIdCustomer == matricula).ToList();

                    foreach (var a in lista)
                    {
                        var agendamento = new Agendamento
                        {
                            Id = a.intId,
                            DataAgendamento = Convert.ToDateTime(a.dteChargingDate),
                            DataExecucao = a.dteExecution,
                            Vencimento = Convert.ToDateTime(a.dteDueDate),
                            Executado = a.bitExecuted,
                            ValorCobrado = a.txtAmmountPayment,
                            Vingencia = Convert.ToDateTime(a.dteValidity),
                            Tipo = GetTipoLabel(a.intIdPaymentType),
                            CommentEmail = a.txtComment
                        };

                        listaAgendamento.Add(agendamento);
                    }

                    return listaAgendamento.OrderBy(c => c.DataExecucao).ToList();

                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public bool UpdateScheduleCharging(UpdateAgendamento agendamento)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    try
                    {
                        var registro = ctx.tblChargingScheduled.FirstOrDefault(a => a.intId == agendamento.Id);
                        if (registro != null)
                        {
                            registro.dteExecution = DateTime.ParseExact(agendamento.DataExecucao, "yyyy-MM-dd",
                                                           System.Globalization.CultureInfo.InvariantCulture);
                            registro.dteChargingDate = DateTime.Now;
                            registro.dteDueDate = DateTime.ParseExact(agendamento.Vencimento, "yyyy-MM-dd",
                                                           System.Globalization.CultureInfo.InvariantCulture);
                            registro.dteValidity = DateTime.ParseExact(agendamento.Vingencia, "yyyy-MM-dd",
                                                           System.Globalization.CultureInfo.InvariantCulture);
                            registro.intIdPaymentType = Convert.ToInt32(agendamento.Tipo);
                            registro.txtAmmountPayment = agendamento.ValorCobrado;
                            registro.txtComment = agendamento.CommentEmail;
                            registro.txtChargingComment = agendamento.AdditionalComment;
                            ctx.SaveChanges();
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public bool DeleteScheduleCharging(int id)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    try
                    {
                        var registro = ctx.tblChargingScheduled.FirstOrDefault(a => a.intId == id);
                        ctx.tblChargingScheduled.Remove(registro);
                        ctx.SaveChanges();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Ocorreu um erro ao tentar coletar a lista")));
            }
        }

        public DateTime? GetScheduleExecutedDate()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblLog.Where(a => a.txtAction == "agendamento de cobrança").OrderByDescending(a => a.dteTimeStamp).FirstOrDefault().dteTimeStamp;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string GetTipoLabel(int? intIdPaymentType)
        {


            var tipo = (PagarmeAccess.PaymentTypes)Enum.Parse(typeof(PagarmeAccess.PaymentTypes), intIdPaymentType.ToString());

            if (tipo == PagarmeAccess.PaymentTypes.card)
                return "CARTÃO";

            if (tipo == PagarmeAccess.PaymentTypes.boleto)
                return "BOLETO";

            if (tipo == PagarmeAccess.PaymentTypes.pix)
                return "PIX";

            return "TIPO INDEFINIDO";
        }

        public List<tblChargingScheduled> GetScheduleCharging(int days)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblChargingScheduled.Where(x => x.dteExecution.Date == DateTime.Now.AddDays(5).Date).ToList();
                }
            }
            catch (Exception)
            {
                return new List<tblChargingScheduled>();
            }
        }

        public Person GetChargingById(int personid, int chargingId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var person = ctx.tblPersons.Where(x => x.intIdPerson == personid).FirstOrDefault();
                    Person client = new Person()
                    {
                        Name = person.txtName
                    };
                    var charging = ctx.tblChargingHistory.Where(x => x.intId == chargingId).FirstOrDefault();
                    if (charging != null)
                    {
                        client.Charging = new Charging()
                        {
                            Id = charging.intId,
                            Ammount = charging.txtAmmountPayment,
                            PixCode = charging.pixCode,
                            ExpireDate = Convert.ToDateTime(charging.dteValidity),
                            AnoVingencia = Convert.ToDateTime(charging.dteValidity).Year.ToString(),
                            MesVingencia = Convert.ToDateTime(charging.dteValidity).Month.ToString(),
                            DueDate = Convert.ToDateTime(charging.dteDueDate),
                            ChargingComment = charging.txtChargingComment,
                            BoletoBarcode = charging.txtboletoBarcode,
                            BoletoUrl = charging.txtboletoUrl
                        };
                    }
                    return client;
                }
            }
            catch (Exception)
            {
                return new Person();
            }
        }

        public int GetChargingIdByTransactionId(int transactionId)
        {
            using (var ctx = new FoneClubeContext())
            {
                var result = ctx.tblChargingHistory.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                if (result != null)
                    return result.intId;
                else
                    return 0;
            }
        }

        #region Partials

        public partial class CobrancaResult
        {

            public string txtConta { get; set; }
            public string txtReferenciaInicio { get; set; }
            public string txtTelefone { get; set; }
            public string txtNome { get; set; }
            public string txtNickname { get; set; }
            public string txtCPF { get; set; }

            public string txtPrecoUnico { get; set; }
            public string txtPrecoUnicoFracao { get; set; }
            public string txtValorCobranca { get; set; }
            public string txtIdPai { get; set; }
            public int IdOperator { get; set; }

            //por enquanto deixar tudo string e convert pela API mais safe
            //public double? intPrecoUnico { get; set; }
            //public double? intPrecoUnicoFracao { get; set; }
            //public Int32? intValorCobranca { get; set; }
            //public Int32? intIdPai { get; set; }

        }


        public partial class BankBilletChanges
        {
            public List<string> Status { get; set; }

            public List<string> PaidAt { get; set; }

            public List<string> UpdatedAt { get; set; }

            public List<string> BancoRecebedor { get; set; }

            public List<string> AgenciaRecebedora { get; set; }

        }
        #endregion

        public bool IsExistsDrCelularData(int ano, int mes, string operadora, string empresa)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var result = ctx.tblDrCelularTemp.Where(x => x.ano == ano && x.mes == mes && x.txtOperadoraButton == operadora && x.txtEmpresa == empresa);
                    if (result != null && result.Count() > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public bool ImportDrCelularData(List<tblDrCelularTemp> tblDrCelular)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var firstItem = tblDrCelular[0];
                    var result = ctx.tblDrCelularTemp.Where(x => x.ano == firstItem.ano && x.mes == firstItem.mes && x.txtOperadoraButton == firstItem.txtOperadoraButton && x.txtEmpresa == firstItem.txtEmpresa);
                    if (result != null && result.Count() > 0)
                    {
                        ctx.tblDrCelularTemp.RemoveRange(result);
                        ctx.SaveChanges();
                    }

                    foreach (var row in tblDrCelular)
                    {
                        ctx.tblDrCelularTemp.Add(row);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool CreateExpiredCharge()
        {
            //TODO registrar log replicando da outra tabela, do pix, cartão e boleto, colocar data e testar;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var currentDate = DateTime.Now.Date;
                    var expiredCharge = (from p in ctx.tblPersons.ToList()
                                         join c in ctx.tblChargingHistory.ToList() on p.intIdPerson equals c.intIdCustomer
                                         join u in ctx.tblUserSettings.ToList() on p.intIdPerson equals u.intPerson
                                         join f in ctx.tblFoneclubePagarmeTransactions.ToList() on c.intIdTransaction equals f.intIdTransaction
                                         where u.bitUse2Prices = true && c.bitActive.HasValue && c.bitActive.Value && (c.intIdPaymentType.HasValue && c.intIdPaymentType.Value == 3) && c.dteExpiryDate.HasValue &&
                                         c.dteExpiryDate.Value == currentDate && f.txtOutdadetStatus != "Paid"
                                         select c).ToList();

                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "Recreate pix expired charge" + expiredCharge.Count
                    });

                    var pagarmeAccess = new PagarmeAccess();

                    foreach (var charge in expiredCharge)
                    {
                        var tblPhones = ctx.tblPersonsPhones.Where(x => x.intIdPerson == charge.intIdCustomer && x.bitPhoneClube == true && (x.bitAtivo.HasValue && x.bitAtivo.Value) && (!x.bitDelete.HasValue || (x.bitDelete.HasValue && !x.bitDelete.Value))).ToList();
                        var tblPersonPhones = (from c in ctx.tblPlansOptions.ToList()
                                               join p in tblPhones
                                                    on new { intIdPlan = c.intIdPlan, intIdOperator = c.intIdOperator } equals new { intIdPlan = p.intIdPlan.Value, intIdOperator = p.intIdOperator.Value }
                                               select new { c.intCost }).ToList();

                        var chargingHistory = new tblChargingHistory
                        {
                            txtAmmountPayment = charge.txtAmmountPayment,
                            intIdCollector = charge.intIdCollector,
                            intIdCustomer = charge.intIdCustomer,
                            intIdPaymentType = charge.intIdPaymentType,
                            txtCollectorName = charge.txtCollectorName,
                            txtComment = charge.txtComment,
                            txtCommentBoleto = charge.txtCommentBoleto,
                            txtCommentEmail = charge.txtCommentEmail,
                            txtTokenTransaction = charge.txtTokenTransaction,
                            intIdBoleto = charge.intIdBoleto,
                            txtAcquireId = charge.txtAcquireId,
                            dteValidity = charge.dteValidity,
                            intChargeStatusId = charge.intChargeStatusId,
                            dteCreate = DateTime.Now,
                            bitCash = charge.bitCash,
                            dtePayment = DateTime.Now,
                            dteDueDate = DateTime.Now,
                            txtWAPhones = charge.txtWAPhones,
                            bitSendWAText = charge.bitSendWAText,
                            bitSendMarketing1 = charge.bitSendMarketing1,
                            bitSendMarketing2 = charge.bitSendMarketing2,
                        };

                        charge.dteDueDate = DateTime.Now;
                        charge.txtAmmountPayment = Convert.ToString(tblPersonPhones.Sum(x => x.intCost));
                        charge.dteCreate = DateTime.Now;
                        charge.dtePayment = DateTime.Now;
                        charge.txtChargingComment = chargingHistory.txtChargingComment;

                        var execute = pagarmeAccess.ExecuteCharging(charge);

                        try
                        {
                            chargingHistory.intIdTransaction = Convert.ToInt32(execute.Id);
                        }
                        catch (Exception)
                        {
                            chargingHistory.intIdTransaction = -1;
                        }

                        try
                        {
                            ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                            {
                                dteRegister = DateTime.Now,
                                txtLog = JsonConvert.SerializeObject(chargingHistory)
                            });
                        }
                        catch (Exception) { }

                        //make exitsing charge inactive
                        try
                        {
                            var existingChare = ctx.tblChargingHistory.FirstOrDefault(x => x.intId == charge.intId);
                            existingChare.bitActive = false;

                            ctx.tblLog.Add(new tblLog
                            {
                                dteTimeStamp = DateTime.Now,
                                intIdTipo = 1,
                                txtAction = "Recreated new expired charge and inactive existing charge: Id: " + charge.intId
                            });
                        }
                        catch (Exception) { }
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool CreateRefusedCharge()
        {
            using (var ctx = new FoneClubeContext())
            {
                var lstRefused = ctx.GetRefusedCCChargeForResend().ToList();
                if (lstRefused != null && lstRefused.Count > 0)
                {
                    foreach (var refuse in lstRefused)
                    {
                        var tblRef = ctx.tblCCRefusedLog.FirstOrDefault(x => x.intIdPerson == refuse.intIdCustomer && x.dteVigencia == refuse.dteValidity);
                        if (tblRef is null)
                        {
                            //1st reminder
                            ctx.tblCCRefusedLog.Add(new tblCCRefusedLog()
                            {
                                intIdPerson = refuse.intIdCustomer.Value,
                                txtChargeIds = refuse.intIdCharge.ToString(),
                                dteVigencia = refuse.dteValidity.Value,
                                dteCreate = DateTime.Now,
                                intReminderCount = 1,
                                IsActive = true
                            });
                            ctx.SaveChanges();
                        }
                        else
                        {
                            if (tblRef.intReminderCount > 0 && tblRef.intReminderCount < 3 && tblRef.IsActive)
                            {
                                ReCreateCCCharge(refuse.intIdCharge, "*Tentamos cobrar seu cartão pela 2a vez mas ele foi recusado novamente. Vamos tentar nova cobrança amanha as 7:00.*");
                            }
                        }
                    }
                }
            }
            return true;
        }

        public List<GetPagarmeTransactionReport_Result> GetAllPagarmeTransactions(string startDate, string endDate)
        {
            List<GetPagarmeTransactionReport_Result> result = null;
            using (var ctx = new FoneClubeContext())
            {
                var start = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(endDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                result = ctx.GetPagarmeTransactionReport(start, end).ToList();
            }
            return result;
        }

        public List<GetInternationalTransactionReport_Result> GetAllIntlSalesTransactions(string startDate, string endDate)
        {
            List<GetInternationalTransactionReport_Result> result = null;
            using (var ctx = new FoneClubeContext())
            {
                var start = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(endDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                result = ctx.GetInternationalTransactionReport(start, end).ToList();
            }
            return result;
        }

        public List<GetInternationalDepositsReport_Result> GetAllIntlDeposits(string startDate, string endDate)
        {
            List<GetInternationalDepositsReport_Result> result = null;
            using (var ctx = new FoneClubeContext())
            {
                var start = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(endDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                result = ctx.GetInternationalDepositsReport(start, end).ToList();
            }
            return result;
        }

        public bool ReCreateCCCharge(int chargeId, string txtComment)
        {
            using (var ctx = new FoneClubeContext())
            {
                var charge = ctx.tblChargingHistory.FirstOrDefault(x => x.intId == chargeId);
                if (charge != null)
                {
                    var chargingHistory = new tblChargingHistory
                    {
                        txtAmmountPayment = charge.txtAmmountPayment,
                        intIdCollector = charge.intIdCollector,
                        intIdCustomer = charge.intIdCustomer,
                        intIdPaymentType = charge.intIdPaymentType,
                        txtCollectorName = charge.txtCollectorName,
                        txtComment = charge.txtComment,
                        txtCommentBoleto = charge.txtCommentBoleto,
                        txtCommentEmail = charge.txtCommentEmail,
                        txtTokenTransaction = charge.txtTokenTransaction,
                        intIdBoleto = charge.intIdBoleto,
                        txtAcquireId = charge.txtAcquireId,
                        dteValidity = charge.dteValidity,
                        intChargeStatusId = charge.intChargeStatusId,
                        dteCreate = DateTime.Now,
                        bitCash = charge.bitCash,
                        dtePayment = DateTime.UtcNow,
                        txtTransactionComment = charge.txtTransactionComment,
                        dteDueDate = DateTime.Now,
                        txtWAPhones = charge.txtWAPhones,
                        bitSendWAText = charge.bitSendWAText,
                        bitSendMarketing1 = false,
                        bitSendMarketing2 = false,
                        intInstallments = charge.intInstallments,
                        txtChargingComment = txtComment
                    };

                    var execute = new PagarmeAccess().ExecuteCharging(chargingHistory);

                    try
                    {
                        chargingHistory.intIdTransaction = Convert.ToInt32(execute.Id);
                        charge.bitActive = false;
                        ctx.SaveChanges();
                    }
                    catch (Exception)
                    {
                        chargingHistory.intIdTransaction = -1;
                    }

                    try
                    {
                        ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                        {
                            dteRegister = DateTime.Now,
                            txtLog = JsonConvert.SerializeObject(chargingHistory)
                        });
                    }
                    catch (Exception) { }
                }
            }
            return true;
        }
    }
}

