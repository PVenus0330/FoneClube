using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using PagarMe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FoneClube.DataAccess.Utilities;

namespace FoneClube.DataAccess.pagarme
{
    public class PagarmeAccess
    {
        public TransactionResult GeraFaturaBoleto(CheckoutPagarMe checkout, bool saveHistoryStore = true)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
            PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

            try
            {
                var transaction = new Transaction();
                transaction.PaymentMethod = PaymentMethod.Boleto;
                transaction.BoletoExpirationDate = DateTime.Now.AddDays(checkout.DaysLimit);

                transaction.Amount = checkout.Amount;
                transaction.Customer = new PagarMe.Customer()
                {
                    Name = checkout.Nome,
                    Email = checkout.Email,
                    DocumentNumber = checkout.DocumentNumber,
                    BornAt = DateTime.ParseExact("1996-05-08 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture),
                    Address = new PagarMe.Address()
                    {
                        Street = checkout.Street,
                        StreetNumber = checkout.StreetNumber,
                        Neighborhood = checkout.Neighborhood,
                        Zipcode = checkout.Zipcode
                    },

                    Phone = new PagarMe.Phone
                    {
                        Ddd = checkout.Ddd,
                        Number = checkout.Number
                    }
                };

                transaction.BoletoInstructions = checkout.BoletoInstructions;
                transaction.PostbackUrl = "https://api.foneclube.com.br/api/pagarme/receive-webhook";
                try
                {

                    transaction.Customer.Save();
                    transaction.Save();
                }
                catch (PagarMe.PagarMeException e)
                {
                    return new TransactionResult
                    {
                        DescriptionMessage = e.Error.Errors.FirstOrDefault().Message,
                        StatusPaid = false
                    };
                }

                try
                {
                    new TransactionAccess().InsertCheckoutPagarMeLog(new TransactionLog
                    {
                        Checkout = checkout,
                        IdTransaction = transaction.Id,
                        BoletoTransaction = true,
                        txtLinkBoleto = transaction.BoletoUrl,
                        TipoLog = 1
                    });

                    var idBoleto = 2;

                    if (saveHistoryStore)
                    {
                        var saveHistory = new ChargingAcess()
                                                                .SaveChargingHistoryStore(new Person
                                                                {
                                                                    Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                                                                    DocumentNumber = checkout.DocumentNumber,
                                                                    Email = checkout.Email,
                                                                    Charging = new Charging
                                                                    {
                                                                        TransactionId = Convert.ToInt64(transaction.Id),
                                                                        Ammount = checkout.Amount.ToString(),
                                                                        PaymentType = idBoleto,
                                                                        BoletoId = Convert.ToInt64(transaction.Id)
                                                                    }
                                                                });
                    }

                }
                catch (Exception) { }

                return new TransactionResult
                {
                    BoletoBarcode = transaction.BoletoBarcode,
                    BoletoUrl = transaction.BoletoUrl,
                    LinkBoleto = transaction.BoletoUrl,
                    Id = transaction.Id,
                    StatusPaid = true
                };
            }
            catch (Exception e)
            {
                return new TransactionResult();
            }

        }

        public TransactionResult GeraFaturaPix(CheckoutPagarMe checkout, bool saveHistoryStore = true)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
            PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

            try
            {
                var transaction = new Transaction();
                transaction.PaymentMethod = PaymentMethod.Pix;
                transaction.BoletoExpirationDate = DateTime.Now.AddDays(checkout.DaysLimit);
                transaction.Amount = checkout.Amount;
                transaction.PixExpirationDate = DateTime.Now.AddDays(365);
                transaction.Customer = new PagarMe.Customer()
                {
                    Name = checkout.Nome,
                    Email = checkout.Email,
                    DocumentNumber = checkout.DocumentNumber,
                    Address = new PagarMe.Address()
                    {
                        Street = checkout.Street,
                        StreetNumber = checkout.StreetNumber,
                        Neighborhood = checkout.Neighborhood,
                        Zipcode = checkout.Zipcode
                    },

                    Phone = new PagarMe.Phone
                    {
                        Ddd = checkout.Ddd,
                        Number = checkout.Number
                    }
                };

                var fields = new List<PixAditionalField>();
                fields.Add(new PixAditionalField { Name = "Quantidade", Value = "1" });

                transaction.PixAditionalFields = fields.ToArray();
                transaction.PostbackUrl = "https://api.foneclube.com.br/api/pagarme/receive-webhook";
                try
                {

                    transaction.Save();
                }
                catch (PagarMe.PagarMeException e)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        ctx.tblLog.Add(new tblLog()
                        {
                            dteTimeStamp = DateTime.Now,
                            txtAction = "PagarMeException: " + e.ToString()
                        });
                        ctx.SaveChanges();
                    }

                    return new TransactionResult
                    {
                        DescriptionMessage = e.Error.Errors.FirstOrDefault().Message,
                        StatusPaid = false
                    };
                }

                try
                {
                    new TransactionAccess().InsertCheckoutPagarMeLog(new TransactionLog
                    {
                        Checkout = checkout,
                        IdTransaction = transaction.Id,
                        BoletoTransaction = true,
                        txtLinkBoleto = transaction.BoletoUrl,
                        TipoLog = 1
                    });



                    if (saveHistoryStore)
                    {
                        var saveHistory = new ChargingAcess()
                                                                .SaveChargingHistoryStore(new Person
                                                                {
                                                                    Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                                                                    DocumentNumber = checkout.DocumentNumber,
                                                                    Email = checkout.Email,
                                                                    Charging = new Charging
                                                                    {
                                                                        TransactionId = Convert.ToInt64(transaction.Id),
                                                                        Ammount = checkout.Amount.ToString(),
                                                                        PaymentType = (int)PaymentMethod.Pix,
                                                                        BoletoId = Convert.ToInt64(transaction.Id)
                                                                    }
                                                                });
                    }

                }
                catch (Exception) { }

                return new TransactionResult
                {
                    PixQRCode = transaction.PixQrCode,
                    Id = transaction.Id,
                    StatusPaid = true,
                    PixExpiryDate = transaction.PixExpirationDate,
                };
            }
            catch (Exception e)
            {
                return new TransactionResult();
            }

        }

        public enum PaymentTypes { card = 1, boleto = 2, pix = 3 };

        public TransactionResult GeraCobrancaIntegrada(Person person)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
            PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

            TransactionResult result;
            var transaction = new Transaction();
            var profileAccess = new ProfileAccess();
            var description = string.Empty;

            var checkoutProfile = profileAccess.GetCheckoutPerson(person.Id);
            checkoutProfile.Amount = Convert.ToInt32(person.Charging.Ammount);

            if (person.Charging.PaymentType == (int)PaymentTypes.card)
            {
                var idPagarme = checkoutProfile.IdCustomerPagarme.ToString();
                var cards = PagarMeService.GetDefaultService().Cards.FindAll(new Card() { Customer = new PagarMe.Customer() { Id = idPagarme } }).ToList();
                if (cards != null && cards.Count > 0)
                {
                    LogHelper.LogMessageOld(1, "Card found for user idPagarme: " + idPagarme);
                    var validCards = cards.Where(c => (c.Customer.Id == idPagarme || c.Customer.DocumentNumber == checkoutProfile.DocumentNumber) && c.Valid == true)
                        .ToList().OrderByDescending(x => x.DateUpdated); ;

                    string someValidCardId = string.Empty;
                    if (!string.IsNullOrEmpty(person.Charging.TransactionComment))
                        someValidCardId = person.Charging.TransactionComment;
                    else
                        someValidCardId = validCards.FirstOrDefault() != null ? validCards.FirstOrDefault().Id : string.Empty;
                    //salva se não tiver salvo na base FC

                    if (!string.IsNullOrEmpty(someValidCardId))
                    {
                        transaction.Amount = checkoutProfile.Amount;
                        transaction.Card = new Card { Id = someValidCardId };

                        transaction.Customer = new PagarMe.Customer()
                        {
                            Name = checkoutProfile.Nome,
                            Email = checkoutProfile.Email,
                            DocumentNumber = checkoutProfile.DocumentNumber,

                            Address = new PagarMe.Address()
                            {
                                Street = checkoutProfile.Street,
                                StreetNumber = checkoutProfile.StreetNumber,
                                Neighborhood = checkoutProfile.Neighborhood,
                                Zipcode = checkoutProfile.Zipcode
                            },

                            Phone = new PagarMe.Phone
                            {
                                Ddd = checkoutProfile.Ddd,
                                Number = checkoutProfile.Number
                            }
                        };

                        transaction.PostbackUrl = "https://api.foneclube.com.br/api/pagarme/receive-webhook";

                        transaction.Save();

                        switch (transaction.Status)
                        {
                            case TransactionStatus.Paid:
                                {
                                    person.Charging.TransactionId = Convert.ToInt64(transaction.Id);
                                    if (transaction.Card != null)
                                        person.Charging.TransactionComment = transaction.Card.LastDigits;
                                    var historySaved = profileAccess.SaveChargingHistory(person);
                                    if (historySaved != HttpStatusCode.OK)
                                        description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                                    return new TransactionResult
                                    {
                                        Id = transaction.Id,
                                        StatusPaid = true
                                    };
                                }
                            case TransactionStatus.Refused:
                                {
                                    person.Charging.TransactionId = Convert.ToInt64(transaction.Id);
                                    var historySaved = profileAccess.SaveChargingHistory(person);
                                    if (historySaved != HttpStatusCode.OK)
                                        description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                                    return new TransactionResult
                                    {
                                        StatusPaid = false,
                                        Recusado = true,
                                        DescriptionMessage = "Não foi possível realizar a compra o cartão associado ao cliente foi negado ou é inválido"
                                    };
                                }
                            case TransactionStatus.PendingRefund:
                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                                };

                            case TransactionStatus.Processing:
                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                                };

                            case TransactionStatus.WaitingPayment:
                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                                };

                            default:
                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    DescriptionMessage = "Não foi possível realizar a compra apesar de ter cartão válido associado, verifique os dados co cliente"
                                };
                        }

                    }
                    else
                    {
                        return new TransactionResult
                        {
                            StatusPaid = false,
                            DescriptionMessage = "Não foi possível realizar a compra pois não existe cartão associado ao cliente"
                        };
                    }
                }
                else
                {
                    LogHelper.LogMessageOld(1, "Unable to find card for user idPagarme: " + idPagarme);
                    return new TransactionResult
                    {
                        StatusPaid = false,
                        DescriptionMessage = "Não foi possível realizar a compra pois não existe cartão associado ao cliente"
                    };
                }
            }
            else if (person.Charging.PaymentType == (int)PaymentTypes.boleto)
            {
                checkoutProfile.DaysLimit = 5;
                checkoutProfile.BoletoInstructions = person.Charging.CommentBoleto;
                result = GeraFaturaBoleto(checkoutProfile, false);

                if (result.StatusPaid)
                {
                    person.Charging.TransactionId = Convert.ToInt64(result.Id);
                    person.Charging.BoletoLink = result.LinkBoleto;
                    person.Charging.BoletoBarcode = result.BoletoBarcode;
                    person.Charging.BoletoUrl = result.BoletoUrl;
                    var historySaved = profileAccess.SaveChargingHistory(person);

                    if (historySaved != HttpStatusCode.OK)
                        description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                    return new TransactionResult
                    {
                        LinkBoleto = result.LinkBoleto,
                        Id = result.Id,
                        StatusPaid = true,
                        DescriptionMessage = description,
                        ChargingHistorySaved = historySaved == HttpStatusCode.OK
                    };
                }
                else
                {
                    return new TransactionResult
                    {
                        DescriptionMessage = "Não foi possível realizar a cobrança via boleto, verifique os dados do cliente",
                        StatusPaid = false
                    };
                }
            }
            else
            {
                result = GeraFaturaPix(checkoutProfile, false);

                if (result.StatusPaid)
                {
                    person.Charging.TransactionId = Convert.ToInt64(result.Id);
                    person.Charging.PixCode = result.PixQRCode;
                    person.Charging.ExpireDate = result.PixExpiryDate.HasValue ? result.PixExpiryDate.Value : DateTime.Now;
                    var historySaved = profileAccess.SaveChargingHistory(person);

                    if (historySaved != HttpStatusCode.OK)
                        description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                    return new TransactionResult
                    {
                        PixQRCode = result.PixQRCode,
                        Id = result.Id,
                        StatusPaid = true,
                        DescriptionMessage = description,
                        ChargingHistorySaved = historySaved == HttpStatusCode.OK
                    };
                }
                else
                {
                    return new TransactionResult
                    {
                        DescriptionMessage = "Não foi possível realizar a cobrança via boleto, verifique os dados do cliente",
                        StatusPaid = false
                    };
                };
            }

        }

        public TransactionResult GeraCobrancaCard(Person person, string cardId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                var transaction = new Transaction();
                var profileAccess = new ProfileAccess();
                var description = string.Empty;

                var checkoutProfile = profileAccess.GetCheckoutPerson(person.Id);
                checkoutProfile.Amount = Convert.ToInt32(person.Charging.Ammount);

                var idPagarme = checkoutProfile.IdCustomerPagarme.ToString();
                LogHelper.LogMessageOld(1, "GeraCobrancaCard: Getting All cards");
                var cards = PagarMeService.GetDefaultService().Cards.FindAll(new Card() { Id = cardId }).ToList();
                if (cards != null)
                {
                    LogHelper.LogMessageOld(1, "GeraCobrancaCard: All cards count:" + cards.Count);
                }

                var validCards = cards.Where(c => (c.Customer.Id == idPagarme || c.Customer.DocumentNumber == checkoutProfile.DocumentNumber) && c.Valid == true && c.Id == cardId)
                    .ToList().OrderByDescending(x => x.DateUpdated); ;

                string someValidCardId = validCards.FirstOrDefault() != null ? validCards.FirstOrDefault().Id : string.Empty;
                //salva se não tiver salvo na base FC

                if (!string.IsNullOrEmpty(someValidCardId))
                {
                    transaction.Amount = checkoutProfile.Amount;
                    transaction.Card = new Card { Id = someValidCardId };

                    transaction.Customer = new PagarMe.Customer()
                    {
                        Name = checkoutProfile.Nome,
                        Email = checkoutProfile.Email,
                        DocumentNumber = checkoutProfile.DocumentNumber,

                        Address = new PagarMe.Address()
                        {
                            Street = checkoutProfile.Street,
                            StreetNumber = checkoutProfile.StreetNumber,
                            Neighborhood = checkoutProfile.Neighborhood,
                            Zipcode = checkoutProfile.Zipcode
                        },

                        Phone = new PagarMe.Phone
                        {
                            Ddd = checkoutProfile.Ddd,
                            Number = checkoutProfile.Number
                        }
                    };

                    transaction.PostbackUrl = "https://api.foneclube.com.br/api/pagarme/receive-webhook";

                    transaction.Save();

                    LogHelper.LogMessageOld(1, "GeraCobrancaCard: Charged with Status:" + transaction.Status + " PersonId:" + person.Id);

                    switch (transaction.Status)
                    {
                        case TransactionStatus.Paid:
                            {
                                var tId = Convert.ToInt64(transaction.Id);
                                person.Charging.TransactionId = Convert.ToInt64(transaction.Id);
                                if (transaction.Card != null)
                                    person.Charging.TransactionComment = transaction.Card.LastDigits;
                                var historySaved = profileAccess.SaveChargingHistory(person);
                                if (historySaved != HttpStatusCode.OK)
                                    description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                                ProfileAccess profile = new ProfileAccess();
                                MVNOAccess mvno = new MVNOAccess();
                                WhatsAppAccess whatsAppAccess = new WhatsAppAccess();

                                LogHelper.LogMessageOld(1, string.Format("GeraCobrancaCard ActivateMVNOPlansPostPayment for id : {0}", tId));
                                profile.ActivateMVNOPlansPostPayment(tId);

                                return new TransactionResult
                                {
                                    Id = transaction.Id,
                                    StatusPaid = true
                                };
                            }
                        case TransactionStatus.Refused:
                            {
                                person.Charging.TransactionId = Convert.ToInt64(transaction.Id);
                                var historySaved = profileAccess.SaveChargingHistory(person);
                                if (historySaved != HttpStatusCode.OK)
                                    description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    Recusado = true,
                                    DescriptionMessage = "Não foi possível realizar a compra o cartão associado ao cliente foi negado ou é inválido"
                                };
                            }
                        case TransactionStatus.PendingRefund:
                            return new TransactionResult
                            {
                                StatusPaid = false,
                                DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                            };

                        case TransactionStatus.Processing:
                            {
                                var tId = Convert.ToInt64(transaction.Id);
                                person.Charging.TransactionId = Convert.ToInt64(transaction.Id);
                                var historySaved = profileAccess.SaveChargingHistory(person);
                                if (historySaved != HttpStatusCode.OK)
                                    description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                                ProfileAccess profile = new ProfileAccess();
                                MVNOAccess mvno = new MVNOAccess();
                                WhatsAppAccess whatsAppAccess = new WhatsAppAccess();

                                LogHelper.LogMessageOld(1, string.Format("ProcessWebhookMessage ActivateMVNOPlansPostPayment for id : {0}", tId));
                                profile.ActivateMVNOPlansPostPayment(tId);

                                return new TransactionResult
                                {
                                    StatusPaid = false,
                                    DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                                };
                            }
                        case TransactionStatus.WaitingPayment:
                            return new TransactionResult
                            {
                                StatusPaid = false,
                                DescriptionMessage = "Compra em processamento cartão pendente de aprovação"
                            };

                        default:
                            return new TransactionResult
                            {
                                StatusPaid = false,
                                DescriptionMessage = "Não foi possível realizar a compra apesar de ter cartão válido associado, verifique os dados co cliente"
                            };
                    }

                }
                else
                {
                    return new TransactionResult
                    {
                        StatusPaid = false,
                        DescriptionMessage = "Não foi possível realizar a compra pois não existe cartão associado ao cliente"
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "GeraCobrancaCard: error : " + ex.ToString());
                return new TransactionResult
                {
                    StatusPaid = false,
                    DescriptionMessage = "Não foi possível realizar a compra pois não existe cartão associado ao cliente"
                };
            }
        }

        public TransactionResult ExecuteChargingScheduled(tblChargingScheduled charge)
        {
            var charging = new Charging();
            charging.Scheduled = true;
            charging.SendEmail = true; //todo, coletar do client
            charging.CommentBoleto = charge.txtCommentBoleto;
            charging.PaymentType = charge.intIdPaymentType;
            charging.Ammount = charge.txtAmmountPayment;
            charging.AnoVingencia = charge.dteValidity.Value.ToString("yyyy");
            charging.MesVingencia = charge.dteValidity.Value.ToString("MM");
            charging.CommentEmail = charge.txtCommentEmail;
            charging.Comment = charge.txtComment;
            charging.TransactionComment = charge.txtTransactionComment;
            charging.DueDate = charge.dteDueDate;
            charging.TxtWAPhones = charge.txtWAPhones;
            charging.SendWAText = charge.bitSendWAText;
            charging.SendMarketing1 = charge.bitSendMarketing1;
            charging.SendMarketing2 = charge.bitSendMarketing2;
            charging.Installments = charge.intInstallments.HasValue ? charge.intInstallments.Value : 0;
            //colocar toda cobrança por aqui ser enviada de email, validar campos adicionais de envio de email boleto
            //pendentecartão pra cobrar depois
            var person = new Person { Id = charge.intIdCustomer.Value, Charging = charging };

            var status = GeraCobrancaIntegrada(person);
            if (status != null && status.Recusado.HasValue && status.Recusado.Value)
            {
                WhatsAppAccess objWA = new WhatsAppAccess();
                bool is3rdReminder = false;
                objWA.SendWhatsAppMessageCCRefused(person, ref is3rdReminder);
            }
            return status;
        }

        public TransactionResult ExecuteCharging(tblChargingHistory charge)
        {
            var charging = new Charging();
            charging.Scheduled = false;
            charging.SendEmail = true; //todo, coletar do client
            charging.CommentBoleto = charge.txtCommentBoleto;
            charging.PaymentType = charge.intIdPaymentType;
            charging.Ammount = charge.txtAmmountPayment;
            charging.AnoVingencia = charge.dteValidity.Value.ToString("yyyy");
            charging.MesVingencia = charge.dteValidity.Value.ToString("MM");
            charging.CommentEmail = charge.txtCommentEmail;
            charging.Comment = charge.txtComment;
            charging.TransactionComment = charge.txtTransactionComment;
            charging.DueDate = charge.dteDueDate;
            charging.TxtWAPhones = charge.txtWAPhones;
            charging.SendWAText = charge.bitSendWAText;
            charging.SendMarketing1 = charge.bitSendMarketing1;
            charging.SendMarketing2 = charge.bitSendMarketing2;
            charging.Installments = charge.intInstallments.HasValue ? charge.intInstallments.Value : 0;
            charging.ChargingComment = charge.txtChargingComment;
            var person = new Person { Id = charge.intIdCustomer.Value, Charging = charging };

            var status = GeraCobrancaIntegrada(person);
            if (status != null && status.Recusado.HasValue && status.Recusado.Value)
            {
                WhatsAppAccess objWA = new WhatsAppAccess();
                bool is3rdReminder = false;
                objWA.SendWhatsAppMessageCCRefused(person, ref is3rdReminder);

                if (is3rdReminder)
                {
                    person.Charging.PaymentType = 3;
                    person.Charging.CreateDate = DateTime.Now;
                    person.Charging.DueDate = DateTime.Now;
                    person.Charging.ChargingComment = "*Tentamos cobrar seu cartão pela terceira vez, mas ele foi recusado novamente.* Para evitar bloqueio de serviços, estamos enviando um *PIX* conforme detalhado abaixo";
                    GeraCobrancaIntegrada(person);
                    DateTime vingenciaday = new DateTime(2000, 1, 1, 0, 0, 0);
                    try
                    {
                        vingenciaday = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingenciaday = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    using (var ctx = new FoneClubeContext())
                    {
                        var tblRef = ctx.tblCCRefusedLog.FirstOrDefault(x => x.intIdPerson == person.Id && x.dteVigencia == vingenciaday);
                        if (tblRef != null && tblRef.intReminderCount == 3 && tblRef.IsActive)
                        {
                            tblRef.IsActive = false;
                            ctx.SaveChanges();
                        }
                    }

                }
            }
            return status;
        }
    }
}
