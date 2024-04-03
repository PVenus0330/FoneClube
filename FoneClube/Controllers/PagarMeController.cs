using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
using FoneClube.DataAccess.Utilities;
using System.Net;
using FoneClube.Business.Commons.Entities.FoneClube;
using PagarMe;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;
using static FoneClube.DataAccess.TransactionAccess;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace FoneClube.WebAPI.Controllers
{
    [System.Web.Http.RoutePrefix("api/pagarme")]
    public class PagarMeController : Controller
    {
        [DefaultValue(false)]
        public class TransactionResult
        {
            public string LinkBoleto { get; set; }
            public string Token { get; set; }
            public string Id { get; set; }
            public int Aquire_Id { get; set; }
            public bool StatusPaid { get; set; }
            public string DescriptionMessage { get; set; }
            public bool? Recusado { get; set; }
            public string PixQRCode { get; set; }
            public int PaymentType { get; set; }
            public long ChargeId { get; set; }
        }

        [Route("transacao/last/{documentNumber}")]
        [HttpGet]
        public string GetLastTransactionByUser(string documentNumber)
        {
            var transactions = new List<Transaction>();
            string lastTransactionId = "";
            try
            {
                PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                for (int icount = 1; icount <= 1; icount++)
                {
                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "10"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", icount.ToString()));

                    var transactionsLoop = new List<Transaction>();
                    transactionsLoop.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    if (transactionsLoop.Count() > 0)
                    {
                        var lastTransaction = transactionsLoop.Where(c => c.Customer != null && c.Customer.DocumentNumber == documentNumber).FirstOrDefault();
                        if (lastTransaction != null)
                        {
                            lastTransactionId = lastTransaction.Id;
                        }
                    }
                    else
                        break;
                }
            }
            catch (Exception ex) { }
            return lastTransactionId;
        }

        [Route("transacao/last/id/{transactionId}")]
        [HttpGet]
        public Transaction GetLastTransactionByTransactionId(string transactionId)
        {
            Transaction lastTransaction = new Transaction();
            try
            {
                PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                for (int icount = 1; icount <= 1; icount++)
                {
                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "1000"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", icount.ToString()));

                    var transactionsLoop = new List<Transaction>();
                    transactionsLoop.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    if (transactionsLoop.Count() > 0)
                    {
                        lastTransaction = transactionsLoop.Where(c => c.Id != null && c.Id == transactionId).FirstOrDefault();
                    }
                    else
                        break;
                }
            }
            catch (Exception ex) { }
            return lastTransaction;
        }

        [Route("transacao/update")]
        [HttpGet]
        public bool UpdateTransactions()
        {
            var transactions = new List<Transaction>();

            try
            {
                PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                int maxLoopCount = 100;
                for (int icount = 1; icount <= maxLoopCount; icount++)
                {
                    var pagarmeRequest = PagarMeService.GetDefaultService().Transactions.BuildFindQuery(new Transaction());
                    pagarmeRequest.Query.Add(Tuple.Create("count", "1000"));
                    pagarmeRequest.Query.Add(Tuple.Create("page", icount.ToString()));

                    var transactionsLoop = new List<Transaction>();
                    transactionsLoop.AddRange(PagarMeService.GetDefaultService().Transactions.FinishFindQuery(pagarmeRequest.Execute()));

                    if (transactionsLoop.Count() > 0)
                        transactions.AddRange(transactionsLoop);
                    else
                        break;
                }
                var teste = transactions.Where(c => c.Id == "100500741" || c.Id == "102086741").ToList();

                //não usamos
                //try
                //{
                //    new CieloAccess().RestoreCieloTransactionData();
                //}
                //catch (Exception) { }

                try
                {
                    new ComissionAccess().SetupComissionOrders();
                }
                catch (Exception) { }

                try
                {
                    new ComissionAccess().SetupBonus();
                }
                catch (Exception) { }
            }
            catch (Exception ex) { }
            return new TransactionAccess().UpdateTransactions(transactions);
        }

        [Route("transacao/update/bonus")]
        [HttpGet]
        public bool UpdateTransactionsBonus()
        {

            try
            {
                new ComissionAccess().SetupBonus();
            }
            catch (Exception) { }

            return true;
        }


        [Route("lista/clientes/apenas/pagarme")]
        [HttpGet]
        public List<CustomerPagarme> GetClientesPagarmeSomente()
        {

            return new TransactionAccess().GetClientesApenasPagarme();
        }

        [Route("lista/todos/clientes/pagarme")]
        [HttpGet]
        public JArray GetTodosClientesPagarme()
        {
            return new TransactionAccess().GetTodosClientesPagarme();
        }

        [Route("lista/todas/transacoes/sem/cliente")]
        [HttpGet]
        public List<RetornoTransaction> GetTransactionsSemCliente()
        {
            return new TransactionAccess().GetTransactionsSemCliente();
        }


        [Route("transacao/reintegrate/date")]
        [HttpGet]
        public DateTime? GetPagarmeUpdateDate()
        {
            return new TransactionAccess().GetPagarmeUpdateDate();
        }

        [Route("transacao/dataUltimoPagamento/{IdPagarme}")]
        [HttpGet]
        public DateTime? GetLastPaymentDate(string idPagarme)
        {
            return new TransactionAccess().GetLastTransactionPaid(new Person { IdPagarme = Convert.ToInt32(idPagarme) });
        }

        [Route("boleto")]
        [HttpPost]
        public TransactionResult GeraFaturaBoleto(CheckoutPagarMe checkout)
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

                    System.Threading.Thread.Sleep(5000);
                    new TransactionAccess().UpdateTransactionPostCharge(transaction.Id);
                    PhoneAccess savePhones = new PhoneAccess();

                    var saveHistory = new ChargingAcess()
                                        .SaveChargingHistoryStore(new Person
                                        {
                                            Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                                            DocumentNumber = checkout.DocumentNumber,
                                            Email = checkout.Email,
                                            Charging = new Charging
                                            {
                                                TransactionId = Convert.ToInt32(transaction.Id),
                                                Ammount = checkout.Amount.ToString(),
                                                PaymentType = idBoleto,
                                                BoletoId = Convert.ToInt32(transaction.Id),
                                                Frete = checkout.Frete,
                                                BoletoBarcode = transaction.BoletoBarcode,
                                                BoletoUrl = transaction.BoletoUrl,
                                                IsActive = true,
                                                CartHash = transaction.CardHash,
                                                Installments = 1,
                                                CreateDate = DateTime.Now,
                                                DueDate = DateTime.Now,
                                                MesVingencia = DateTime.Now.Month.ToString(),
                                                AnoVingencia = DateTime.Now.Year.ToString(),
                                                ExpireDate = transaction.PixExpirationDate.HasValue ? transaction.PixExpirationDate.Value : DateTime.Now.AddDays(365)
                                            }
                                        });
                    savePhones.SavePhonesStore(checkout.SelectedPlans, Convert.ToInt32(checkout.IdCustomerFoneclube));
                    savePhones.SaveStorePlanDetails(checkout.SelectedPlans, checkout.Frete.HasValue ? checkout.Frete.Value : 1, Convert.ToInt32(checkout.IdCustomerFoneclube));
                }
                catch (Exception) { }

                try
                {
                    var sendEmail = new EmailAccess().SendEmail(new Email
                    {
                        To = checkout.Email,
                        TargetName = "Rodrigo",
                        TargetSecondaryText = transaction.BoletoUrl,
                        TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                    });
                }
                catch (Exception) { }

                return new TransactionResult
                {
                    LinkBoleto = transaction.BoletoUrl,
                    PaymentType = (int)PaymentMethod.Boleto,
                    Id = transaction.Id,
                    StatusPaid = true
                };
            }
            catch (Exception e)
            {
                return new TransactionResult();
            }

        }

        [Route("pix")]
        [HttpPost]
        public TransactionResult GeraFaturaPix(CheckoutPagarMe checkout)
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

                var fields = new List<PixAditionalField>();
                fields.Add(new PixAditionalField { Name = "Quantidade", Value = "1" });

                transaction.PixAditionalFields = fields.ToArray();

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

                int chargingId = 0;
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


                    System.Threading.Thread.Sleep(5000);
                    new TransactionAccess().UpdateTransactionPostCharge(transaction.Id);
                    PhoneAccess savePhones = new PhoneAccess();
                    var saveHistory = new ChargingAcess()
                                                               .SaveChargingHistoryStore(new Person
                                                               {
                                                                   Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                                                                   DocumentNumber = checkout.DocumentNumber,
                                                                   Email = checkout.Email,
                                                                   Charging = new Charging
                                                                   {
                                                                       TransactionId = Convert.ToInt32(transaction.Id),
                                                                       Ammount = checkout.Amount.ToString(),
                                                                       PaymentType = (int)PaymentMethod.Pix,
                                                                       BoletoId = Convert.ToInt32(transaction.Id),
                                                                       PixCode = transaction.PixQrCode,
                                                                       IsActive = true,
                                                                       CartHash = transaction.CardHash,
                                                                       Installments = 1,
                                                                       CreateDate = DateTime.Now,
                                                                       DueDate = DateTime.Now,
                                                                       MesVingencia = DateTime.Now.Month.ToString(),
                                                                       AnoVingencia = DateTime.Now.Year.ToString(),
                                                                       ExpireDate = transaction.PixExpirationDate.HasValue ? transaction.PixExpirationDate.Value : DateTime.Now.AddDays(365)
                                                                   }
                                                               });

                    chargingId = new ChargingAcess().GetChargingIdByTransactionId(Convert.ToInt32(transaction.Id));
                    savePhones.SavePhonesStore(checkout.SelectedPlans, Convert.ToInt32(checkout.IdCustomerFoneclube));
                    savePhones.SaveStorePlanDetails(checkout.SelectedPlans, checkout.Frete.HasValue ? checkout.Frete.Value : 1, Convert.ToInt32(checkout.IdCustomerFoneclube));
                }
                catch (Exception) { }

                try
                {
                    var sendEmail = new EmailAccess().SendEmail(new Email
                    {
                        To = checkout.Email,
                        TargetName = checkout.Nome,
                        TargetTextBlue = ConfigurationManager.AppSettings["qrcodelink"] + transaction.Id,
                        TargetSecondaryText = @"<b>Total da sua conta: R$ " + (Convert.ToDouble(transaction.Amount) / 100).ToString("F").Replace(".", ",") + "</b>",
                        TemplateType = Convert.ToInt32(Email.TemplateTypes.Pix),
                    });
                }
                catch (Exception) { }

                return new TransactionResult
                {
                    PixQRCode = transaction.PixQrCode,
                    PaymentType = (int)PaymentMethod.Pix,
                    Id = transaction.Id,
                    StatusPaid = true,
                    ChargeId = chargingId
                };
            }
            catch (Exception e)
            {
                return new TransactionResult();
            }

        }

        [Route("cartao")]
        [HttpPost]
        public TransactionResult GeraCobrancaCartao(CheckoutPagarMe checkout)
        {
            try
            {

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["APIKEY"];
                    PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["ENCRYPTIONKEY"];

                    var transaction = new Transaction();
                    transaction.Amount = checkout.Amount;

                    var creditcard = new CardHash
                    {
                        CardHolderName = checkout.CardHolderName,
                        CardNumber = checkout.CardNumber,
                        CardExpirationDate = checkout.CardExpirationDate,
                        CardCvv = checkout.CardCvv
                    };

                    transaction.CardHash = creditcard.Generate();

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
                    transaction.PostbackUrl = "https://api.foneclube.com.br/api/pagarme/receive-webhook";
                    transaction.Customer.Save();
                    transaction.Save();

                    //status
                    //processing, authorized, paid, refunded, waiting_payment, pending_refund, refused
                    try
                    {

                        if (transaction.Status.ToString() == "Refused")
                            return new TransactionResult
                            {
                                Id = "0",
                                StatusPaid = false,
                                Recusado = true,
                                DescriptionMessage = "Pagamento não finalizado, cartão recusado."
                            };

                    }
                    catch (Exception) { }


                    try
                    {
                        new TransactionAccess().InsertCheckoutPagarMeLog(new TransactionLog
                        {
                            Checkout = checkout,
                            IdTransaction = transaction.Id,
                            CartaoTransaction = true,
                            TipoLog = 1
                        });


                        var idCartao = 1;
                        System.Threading.Thread.Sleep(5000);
                        new TransactionAccess().UpdateTransactionPostCharge(transaction.Id);
                        PhoneAccess savePhones = new PhoneAccess();
                        var saveHistory = new ChargingAcess().SaveChargingHistoryStore(
                                                            new Person
                                                            {
                                                                Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                                                                DocumentNumber = checkout.DocumentNumber,
                                                                Email = checkout.Email,
                                                                Charging = new Charging
                                                                {
                                                                    TransactionId = Convert.ToInt32(transaction.Id),
                                                                    Ammount = checkout.Amount.ToString(),
                                                                    PaymentType = idCartao,
                                                                    Frete = checkout.Frete,
                                                                    IsActive = true,
                                                                    CartHash = transaction.CardHash,
                                                                    Installments = 1,
                                                                    CreateDate = DateTime.Now,
                                                                    DueDate = DateTime.Now,
                                                                    MesVingencia = DateTime.Now.Month.ToString(),
                                                                    AnoVingencia = DateTime.Now.Year.ToString(),
                                                                    ExpireDate = transaction.PixExpirationDate.HasValue ? transaction.PixExpirationDate.Value : DateTime.Now.AddDays(365)
                                                                }
                                                            });
                        savePhones.SavePhonesStore(checkout.SelectedPlans, Convert.ToInt32(checkout.IdCustomerFoneclube));
                        savePhones.SaveStorePlanDetails(checkout.SelectedPlans, checkout.Frete.HasValue ? checkout.Frete.Value : 1, Convert.ToInt32(checkout.IdCustomerFoneclube));
                    }
                    catch (Exception) { }

                    try
                    {
                        new ChargingAcess().SaveCard(new CardFoneclube
                        {
                            Number = checkout.CardNumber,
                            Cvv = checkout.CardCvv,
                            HolderName = checkout.CardHolderName,
                            ExpirationMonth = checkout.CardExpirationDate.Substring(0, 2),
                            ExpirationYear = "20" + checkout.CardExpirationDate.Substring(2, 2),
                            Flag = "PagarMe"
                        },
                        new Person
                        {
                            Id = Convert.ToInt32(checkout.IdCustomerFoneclube),
                            DocumentNumber = checkout.DocumentNumber,
                            Email = checkout.Email
                        });
                    }
                    catch (Exception) { }

                    try
                    {
                        var sendEmail = new EmailAccess().SendEmail(new Email
                        {
                            To = checkout.Email,
                            TargetName = checkout.Nome.Split(' ')[0],
                            TargetTextBlue = (checkout.Amount / 100.00).ToString().Replace(".", ","),
                            TemplateType = Convert.ToInt32(Email.TemplateTypes.CardCharged),
                            TargetSecondaryText = String.Empty
                        });
                    }
                    catch (Exception) { }


                    return new TransactionResult
                    {
                        Id = transaction.Id,
                        StatusPaid = transaction.Status.ToString() == "Paid" ? true : false
                    };
                }
                catch (PagarMeException e)
                {
                    return new TransactionResult
                    {
                        Id = "0",
                        StatusPaid = false,
                        DescriptionMessage = "Ocorreu um erro ao tentar processar o pagamento junto ao Gateway (" + e.Error.Errors.FirstOrDefault().Message + " )"
                    };
                }
            }
            catch (Exception e)
            {
                return new TransactionResult
                {
                    Id = "0",
                    StatusPaid = false,
                    DescriptionMessage = "Ocorreu um erro ao tentar processar o pagamento junto ao Gateway (" + e.Message + " )"
                };
            }

        }

        public enum PaymentTypes { card = 1, boleto = 2 };
        [Route("integrada")]
        [HttpPost]
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
                var cards = PagarMeService.GetDefaultService().Cards.FindAll(new Card()).ToList();
                var validCards = cards.Where(c => (c.Customer.Id == idPagarme || c.Customer.DocumentNumber == checkoutProfile.DocumentNumber) && c.Valid)
                    .ToList().OrderByDescending(x => x.DateUpdated); ;

                var someValidCard = validCards.FirstOrDefault();


                if (!bool.Equals(someValidCard, null))
                {
                    transaction.Amount = checkoutProfile.Amount;
                    transaction.Card = new Card { Id = someValidCard.Id };

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

                    transaction.Save();

                    switch (transaction.Status)
                    {
                        case TransactionStatus.Paid:

                            person.Charging.TransactionId = Convert.ToInt32(transaction.Id);
                            var historySaved = profileAccess.SaveChargingHistory(person);
                            if (historySaved != HttpStatusCode.OK)
                                description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                            return new TransactionResult
                            {
                                Id = transaction.Id,
                                StatusPaid = true
                            };

                        case TransactionStatus.Refused:
                            return new TransactionResult
                            {
                                StatusPaid = false,
                                DescriptionMessage = "Não foi possível realizar a compra o cartão associado ao cliente foi negado ou é inválido"
                            };

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
                checkoutProfile.DaysLimit = 5;
                checkoutProfile.BoletoInstructions = person.Charging.CommentBoleto;
                result = GeraFaturaBoleto(checkoutProfile);

                if (result.StatusPaid)
                {
                    person.Charging.TransactionId = Convert.ToInt32(transaction.Id);

                    //estava salvando duas vezes
                    //var historySaved = profileAccess.SaveChargingHistory(person);

                    //if (historySaved != HttpStatusCode.OK)
                    //description = "Cobrança feita porém com problemas no histórico, importante! " + transaction.Id;

                    return new TransactionResult
                    {
                        LinkBoleto = result.LinkBoleto,
                        Id = transaction.Id,
                        StatusPaid = true,
                        DescriptionMessage = description
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

        }

        //[Route("pix/qrcode")]
        //[HttpGet]
        //public string GetQRCodeBitMap()
        //{
        //    return new TransactionAccess().GetQRCodeBitMap();
        //}

        [Route("pix/qrcode/{charging}")]
        [HttpGet]
        public HttpResponseMessage GetQrCode(string charging)
        {

            Image img = new TransactionAccess().GetQRCodeImage(Convert.ToInt32(charging));
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(ms.ToArray());
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return result;
            }
        }

        [HttpPost]
        [Route("receive-webhook")]
        public IHttpActionResult ProcessWebhookMessage()
        {
            try
            {
                string transactionId = string.Empty;
                var statusResponse = new TransactionAccess().ProcessWebhookMessage(out transactionId);
                System.Threading.Thread.Sleep(5000);
                if (!string.IsNullOrEmpty(transactionId))
                {
                    LogHelper.LogMessageOld(1, "ProcessWebhookMessage transactionId : " + transactionId);
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
                return ResponseMessage(Request.CreateResponse(statusResponse, true));
            }
            catch (HttpResponseException error)
            {
                var responseMessage = new HttpResponseMessage(error.Response.StatusCode);
                responseMessage.ReasonPhrase = error.Response.ReasonPhrase;
                return ResponseMessage(responseMessage);
            }
        }
    }
}