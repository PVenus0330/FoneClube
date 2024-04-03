using Business.Commons.Utils;
using Cie;
using CieloLib;
using CieloLib.Debit;
using CieloLib.Domain;
using FoneClube.Business.Commons.Entities.Cielo;
using FoneClube.Business.Commons.Entities.Generic;
using FoneClube.Business.Commons.Utils;
using HttpService;
using LoggerNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoneClube.DataAccess
{
    public class CieloAccess
    {
        public List<Header> headers = new List<Header>();

        public CieloAccess()
        {

            ApiGateway.EndPointApi = "https://apiquery.cieloecommerce.cielo.com.br/";
            headers.Add(new Header { Key = "MerchantId", value = "99879411-cc78-4e23-875a-95c91639260b" });
            headers.Add(new Header { Key = "MerchantKey", value = "B5fz29Vtfj3MuJ4KQGwSLDiEFiVPmeSZ6bMG1QQw" });
        }

        public CieloTransactionResult GetTransaction(string paymentId)
        {
            var link = "1/sales/{0}";
            var result = ApiGateway.GetConteudo(string.Format(link, paymentId), headers, SecurityProtocolType.Tls12);
            return JsonConvert.DeserializeObject<CieloTransactionResult>(result);
        }

        public Transaction.Tipo GetTransactionStatus(FoneClubeContext ctx, string transactionId)
        {
            var resultTransaction = GetTransaction(transactionId);
            return (Transaction.Tipo)Enum.Parse(typeof(Transaction.Tipo), resultTransaction.Payment.Status.ToString());
        }

        public bool RestoreCieloTransactionData()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var orders = new List<CieloOrder>();
                    var transactionsToRestore = ctx.GetCieloTransactionsToRestore().ToList();

                    foreach (var transaction in transactionsToRestore)
                    {
                        var chargingHistory = ctx.tblChargingHistory.FirstOrDefault(c => c.txtPaymentId == transaction.txtPaymentId);

                        if (chargingHistory != null)
                        {
                            var ordem = new CieloOrder
                            {
                                Transaction = GetTransaction(transaction.txtPaymentId),
                                CustomerId = chargingHistory.intIdCustomer.Value,
                                OrderId = transaction.txtOrderId
                            };

                            orders.Add(ordem);
                        }
                    }

                    UpdateTransactionsCielo(ctx);
                    //log aqui? pra identificar os que não são salvos
                    if (orders.Count > 0)
                        SaveTransactionsCielo(orders, ctx);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }


        }

        public HttpResponseMessage GetRestoreHistoryTransactionCielo(int historyId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var orders = new List<CieloOrder>();
                    var limite = DateTime.Now.AddDays(-90);
                    var cieloTransactions = ctx.tblTransactionsCielo
                        .Where(c => c.dteRegister > limite && c.intHistoryId == historyId ).ToList();

                    var history = ctx.tblChargingHistory.FirstOrDefault(c => c.intId == historyId);
                    var vingencia = Convert.ToDateTime(history.dteValidity);
                    var amount = (Convert.ToInt32(history.txtAmmountPayment) / 100.00).ToString("C", CultureInfo.CurrentCulture).Replace("$", string.Empty).Replace(".", ",");

                    foreach (var transaction in cieloTransactions)
                    {
                        var status = GetTransactionStatus(ctx, transaction.txtPaymentId);
                        transaction.intStatusPayment = (int)status;
                        ctx.SaveChanges();
                    }

                    var paymentDone = (int)Transaction.Tipo.PaymentConfirmed;
                    var pago = cieloTransactions.Any(a => a.intStatusPayment == paymentDone);

                    var template = ctx.tblEmailTemplates.FirstOrDefault(a => a.intId == 23).txtDescription;

                    //pago = true;

                    if (pago)
                    {
                        
                        var corpo = string.Format(template, "Pagamento realizado com Sucesso", "Seu pagamento de " + vingencia.Month + "/" + vingencia.Year + " no valor de R$:" + amount + "  foi realizado.", string.Empty, string.Empty);
                        var response = new HttpResponseMessage();
                        response.Content = new StringContent(corpo);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                        var thread = new Thread(() => EnviaEmailCieloConfirmacao(history.intIdCustomer.Value, corpo));
                        thread.Start();
                        //try
                        //{
                        //    EnviaEmailCieloConfirmacao(history.intIdCustomer.Value, corpo, ctx);
                        //}
                        //catch (Exception)
                        //{

                        //}

                        return response;
                    }
                    else
                    {
                        var corpo = string.Format(template, "Pagamento n&atildeo foi conclu&iacutedo", "Entre em contato conosco em caso de dificuldade de pagamento.", string.Empty, string.Empty);
                        var response = new HttpResponseMessage();
                        response.Content = new StringContent(corpo);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        return response;
                    }
                    
                }
            }
            catch (Exception e)
            {
                var response = new HttpResponseMessage();
                response.Content = new StringContent("<div>Ocorreu um erro</div>");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                return response;
            }
        }

        private void EnviaEmailCieloConfirmacao(int personId, string corpo)
        {
            using (var ctx = new FoneClubeContext())
            {
                var person = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == personId);
                var destino = person.txtEmail;
                if (ctx.Database.Connection.Database == "foneclube-homol")
                    destino = ConfigurationManager.AppSettings["EmailTestes"];

                var enviaEmail = new Utils().SendEmail(destino, "Pagamento realizado com Sucesso", corpo, true);
            }
        }

        public bool UpdateTransactionsCielo(FoneClubeContext ctx)
        {
            try
            {
                var orders = new List<CieloOrder>();
                var limite = DateTime.Now.AddDays(-90);
                var cieloTransactions = ctx.tblCieloPaymentLog.Where(c => c.dtePaymentDate > limite).ToList();

                foreach (var transaction in cieloTransactions)
                {
                    var ordem = new CieloOrder
                    {
                        Transaction = GetTransaction(transaction.txtPaymentId),
                        CustomerId = Convert.ToInt32(transaction.intCustomerId),
                        OrderId = transaction.txtOrderId
                    };

                    orders.Add(ordem);
                }

                UpdateTransactionsCielo(orders, ctx);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool UpdateTransactionsCielo(List<CieloOrder> orders, FoneClubeContext ctx)
        {
            try
            {
                foreach (var ordem in orders)
                {

                    try
                    {
                        var tblCieloStatus = ctx.tblCieloPaymentStatus.FirstOrDefault(c => c.txtPaymentId == ordem.Transaction.Payment.PaymentId);
                        tblCieloStatus.intStatus = ordem.Transaction.Payment.Status;
                        ctx.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        //log?
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SaveTransactionsCielo(List<CieloOrder> ordens, FoneClubeContext ctx)
        {
            try
            {
                foreach (var ordem in ordens)
                {
                    try
                    {

                        var status = new tblCieloPaymentStatus
                        {
                            txtOrderId = ordem.OrderId,
                            intCustomerId = ordem.CustomerId,
                            txtType = ordem.Transaction.Payment.Type,
                            txtPaymentId = ordem.Transaction.Payment.PaymentId,
                            txtAuthorizationCode = ordem.Transaction.Payment.AuthorizationCode,
                            txtProofOfSale = ordem.Transaction.Payment.ProofOfSale,
                            txtInterest = ordem.Transaction.Payment.Interest,
                            txtCountry = ordem.Transaction.Payment.Country,
                            txtCurrency = ordem.Transaction.Payment.Currency,
                            intAmount = ordem.Transaction.Payment.Amount,
                            intInstallments = ordem.Transaction.Payment.Installments,
                            intServiceTaxAmount = ordem.Transaction.Payment.ServiceTaxAmount,
                            intStatus = ordem.Transaction.Payment.Status,
                            bitAuthenticate = ordem.Transaction.Payment.Authenticate,
                            bitCapture = ordem.Transaction.Payment.Capture,
                            dteChargingDate = Convert.ToDateTime(ordem.Transaction.Payment.ReceivedDate)
                        };

                        try
                        {
                            if (ordem.Transaction.Payment.Type == "CreditCard")
                            {
                                status.txtHolder = ordem.Transaction.Payment.CreditCard.Holder;
                                status.txtBrand = ordem.Transaction.Payment.CreditCard.Brand;
                                status.txtCardNumber = ordem.Transaction.Payment.CreditCard.CardNumber;
                                status.txtExpirationDate = ordem.Transaction.Payment.CreditCard.ExpirationDate;
                            }

                            if (ordem.Transaction.Payment.Type == "DebitCard")
                            {
                                status.txtHolder = ordem.Transaction.Payment.DebitCard.Holder;
                                status.txtBrand = ordem.Transaction.Payment.DebitCard.Brand;
                                status.txtCardNumber = ordem.Transaction.Payment.DebitCard.CardNumber;
                                status.txtExpirationDate = ordem.Transaction.Payment.DebitCard.ExpirationDate;
                            }

                            if (ordem.Transaction.Payment.Type == "Boleto")
                            {
                                status.txtInstructions = ordem.Transaction.Payment.Instructions;
                                status.txtExpirationDate = ordem.Transaction.Payment.ExpirationDate;
                                status.txtDemostrative = ordem.Transaction.Payment.Demostrative;
                                status.txtUrl = ordem.Transaction.Payment.Url;
                                status.txtBoletoNumber = ordem.Transaction.Payment.BoletoNumber;
                                status.txtBarCodeNumber = ordem.Transaction.Payment.BarCodeNumber;
                                status.txtDigitableLine = ordem.Transaction.Payment.DigitableLine;
                                status.txtAssignor = ordem.Transaction.Payment.Assignor;
                                status.txtAddress = ordem.Transaction.Payment.Address;
                                status.txtIdentification = ordem.Transaction.Payment.Identification;
                            }
                        }
                        catch (Exception) { }

                        ctx.tblCieloPaymentStatus.Add(status);
                        ctx.SaveChanges();
                    }
                    catch (Exception) { }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ReleaseBenefitsCielo()
        {
            try
            {

                using (var ctx = new FoneClubeContext())
                {
                    var limite = DateTime.Now.AddYears(-1);
                    var gatewayCielo = Convert.ToInt32(Gateways.Tipo.Cielo);
                    var paymentConfirmed = Convert.ToInt32(Transaction.Tipo.PaymentConfirmed);
                    var tblChargings = ctx.tblChargingHistory.Where(c => c.dteCreate > limite && c.intIdGateway == gatewayCielo).ToList();

                    var ordensPagas = (from c in ctx.tblChargingHistory
                                       join t in ctx.tblCieloPaymentStatus on c.txtPaymentId equals t.txtPaymentId
                                       where c.dteCreate > limite
                                       && c.intIdGateway == gatewayCielo
                                       && c.bitComissionConceded != true
                                       && t.intStatus == paymentConfirmed
                                       select new CieloOrder
                                       {
                                           ChargingId = c.intId,
                                           Transaction = new CieloTransactionResult
                                           {
                                               Payment = new CieloPayment
                                               {
                                                   PaymentId = t.txtPaymentId
                                               }
                                           }
                                       }).ToList();

                    // liberação de bonus
                    // liberação de comissão
                    // fazer cadastramento fake na history das 3 que existem pra essa consulta retornar

                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GenerateFirstLinkDebito(CieloDebitoTransaction cieloDebitoTransaction)
        {
            try
            {
                var endpointAPI = "http://api.foneclube.com.br/";

                if(new StatusAPIAccess().GetDatabaseName() == "foneclube-homol")
                    endpointAPI = "http://homol-api.p2badpmtjj.us-east-2.elasticbeanstalk.com/";

                endpointAPI += "api/cielo/transacao/";

                return endpointAPI + EncryptCieloTransactionString(cieloDebitoTransaction);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public bool HasDebitCard(int customerId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var cards = ctx.tblEmailLog.Where(e => e.intIdPerson == customerId && e.intIdTypeEmail == CardTypes.Debito).ToList();

                    if (cards.Count == 0)
                        return false;

                    foreach(var card in cards)
                    {
                        var debitCard = new CardUtils().GetCard(card.txtEmail);
                        var cardLimit = new DateTime(Convert.ToInt32(debitCard.ExpirationYear), Convert.ToInt32(debitCard.ExpirationMonth), 1);

                        if(cardLimit > DateTime.Now)
                            return true;
                    }

                    return false;
                    
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GenerateDebitoCharge(string debitoTransaction)
        {
            var cieloDebitoTransaction = new CieloAccess().DecryptCieloTransactionString(debitoTransaction);

            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var customer = ctx.tblPersons.FirstOrDefault(c => c.intIdPerson == cieloDebitoTransaction.CustomerId);
                    var adress = ctx.tblPersonsAddresses.FirstOrDefault(a => a.intIdPerson == cieloDebitoTransaction.CustomerId);
                    var card = ctx.tblEmailLog.FirstOrDefault(e => e.intIdPerson == cieloDebitoTransaction.CustomerId && e.intIdTypeEmail == CardTypes.Debito);

                    var defaultMessage = "Entre em contato por Whatsapp +55 21 98190-8190 ou acesse nossa loja na seção de clientes: http://loja.foneclube.com.br";

                    if (customer == null)
                        return "Não existe cliente relacionado." + defaultMessage;

                    //o cliente ter uma tela listando cartoes e ele define qual
                    if (card == null)
                        return "Não existe cartão cadastro a esse cliente, primeiro cadastre um cartão" + defaultMessage; //um link para cadastrar

                    if (adress == null)
                        return "Existe um problema cadastral no endereço" + defaultMessage; //um link para erro

                    var settings = GetDebitoPaymentSettings(cieloDebitoTransaction);
                    var paymentRequest = GetPaymentRequest(customer, card, cieloDebitoTransaction, ctx);
                    var customerCielo = GetCieloCustomer(customer, adress, ctx);

                    var processor = new DebitProcessor(settings);
                    var paymentResponse = processor.ProcessPayment(paymentRequest, customerCielo);

                    if(paymentResponse.Status == "EXCEPTION")
                    {
                        return GenerateDebitoChargeRetry(debitoTransaction);
                    }

                    var transaction = new tblTransactionsCielo
                    {
                        intGatewayId = CieloGatewayType.Id,
                        intHistoryId = cieloDebitoTransaction.HistoryId,
                        intPersonId = cieloDebitoTransaction.CustomerId,
                        txtPaymentId = paymentResponse.Response.Payment.PaymentId.ToString(),
                        intValor = cieloDebitoTransaction.Valor,
                        dteRegister = DateTime.Now
                    };

                    ctx.tblTransactionsCielo.Add(transaction);
                    ctx.SaveChanges();

                    return paymentResponse.Response.PaymentUrl;
                }
            }
            catch (Exception e)
            {
                //todo link de erro e aviso com contato
                return string.Empty;
            }
        }

        public string GenerateDebitoChargeRetry(string debitoTransaction)
        {
            var cieloDebitoTransaction = new CieloAccess().DecryptCieloTransactionString(debitoTransaction);

            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var customer = ctx.tblPersons.FirstOrDefault(c => c.intIdPerson == cieloDebitoTransaction.CustomerId);
                    var adress = ctx.tblPersonsAddresses.FirstOrDefault(a => a.intIdPerson == cieloDebitoTransaction.CustomerId);
                    var card = ctx.tblEmailLog.FirstOrDefault(e => e.intIdPerson == cieloDebitoTransaction.CustomerId && e.intIdTypeEmail == CardTypes.Debito);

                    var defaultMessage = "Entre em contato por Whatsapp +55 21 98190-8190 ou acesse nossa loja na seção de clientes: http://loja.foneclube.com.br";

                    if (customer == null)
                        return "Não existe cliente relacionado." + defaultMessage;

                    //o cliente ter uma tela listando cartoes e ele define qual
                    if (card == null)
                        return "Não existe cartão cadastro a esse cliente, primeiro cadastre um cartão" + defaultMessage; //um link para cadastrar

                    if (adress == null)
                        return "Existe um problema cadastral no endereço" + defaultMessage; //um link para erro

                    var settings = GetDebitoPaymentSettings();
                    var paymentRequest = GetPaymentRequest(customer, card, cieloDebitoTransaction, ctx);
                    var customerCielo = GetCieloCustomerStatic(customer, adress, ctx);

                    var processor = new DebitProcessor(settings);
                    var paymentResponse = processor.ProcessPayment(paymentRequest, customerCielo);

                    string teste = "teste";

                    try
                    {
                        teste = JsonConvert.SerializeObject(paymentResponse);
                    }
                    catch (Exception) { }

                    if (paymentResponse.Status == "EXCEPTION")
                        return "Ocorreu um erro junto ao gateway de pagamento do banco." + cieloDebitoTransaction.Valor.ToString() + teste;

                    var transaction = new tblTransactionsCielo
                    {
                        intGatewayId = CieloGatewayType.Id,
                        intHistoryId = cieloDebitoTransaction.HistoryId,
                        intPersonId = cieloDebitoTransaction.CustomerId,
                        txtPaymentId = paymentResponse.Response.Payment.PaymentId.ToString(),
                        intValor = cieloDebitoTransaction.Valor
                    };

                    ctx.tblTransactionsCielo.Add(transaction);
                    ctx.SaveChanges();

                    return paymentResponse.Response.PaymentUrl;
                }
            }
            catch (Exception e)
            {
                //todo link de erro e aviso com contato
                return string.Empty;
            }
        }

        public string EncryptCieloTransactionString(CieloDebitoTransaction cieloDebitoTransaction)
        {
            var formated = string.Format("{0}_{1}_{2}_{3}_{4}", 
                cieloDebitoTransaction.CustomerId, 
                cieloDebitoTransaction.Ano, 
                cieloDebitoTransaction.Mes, 
                cieloDebitoTransaction.HistoryId, 
                cieloDebitoTransaction.Valor);
            return new Utils().Base64Encode(formated);
        }

        public CieloDebitoTransaction DecryptCieloTransactionString(string cieloDebitoTransaction)
        {
            var lista = new Utils().Base64Decode(cieloDebitoTransaction).Split('_');
            return new CieloDebitoTransaction {
                CustomerId = Convert.ToInt32(lista[0]),
                Ano = Convert.ToInt32(lista[1]),
                Mes = Convert.ToInt32(lista[2]),
                HistoryId = Convert.ToInt32(lista[3]),
                Valor = Convert.ToInt32(lista[4]),
            };
        }

        private PaymentRequest GetPaymentRequest(tblPersons customer, tblEmailLog card, CieloDebitoTransaction cieloDebitoTransaction, FoneClubeContext ctx)
        {
            var debitCard = new CardUtils().GetCard(card.txtEmail);

            return new PaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                OrderTotal = cieloDebitoTransaction.Valor,
                CreditCardCvv2 = debitCard.Cvv,
                CreditCardExpireMonth = Convert.ToInt32(debitCard.ExpirationMonth),
                CreditCardExpireYear = Convert.ToInt32(debitCard.ExpirationYear),
                CreditCardName = debitCard.HolderName,
                CreditCardNumber = debitCard.Number,
                CreditCardType = debitCard.Flag    //visa, Master, Discover, Amex, Elo, Aura, JCB, Diners, Hicard
            };
        }

        private CieloLib.Domain.Customer GetCieloCustomer(tblPersons customer, tblPersonsAddresses adress, FoneClubeContext ctx)
        {
            return new CieloLib.Domain.Customer
            {
                Address = new CustomerAddress
                {
                    City = adress.txtCity,
                    Country = "Brasil",
                    State = adress.txtState,
                    Number = adress.intStreetNumber.ToString(),
                    ZipCode = adress.txtCep,
                    District = adress.txtCity,
                    Street = adress.txtStreet
                },
                Identity = customer.txtDocumentNumber,
                Name = customer.txtName
            };
        }

        private CieloLib.Domain.Customer GetCieloCustomerStatic(tblPersons customer, tblPersonsAddresses adress, FoneClubeContext ctx)
        {

            return new CieloLib.Domain.Customer
            {
                Address = new CustomerAddress
                {
                    City = "Rio de Janeiro",
                    Country = "Brasil",
                    State = "RJ",
                    Number = "10021",
                    ZipCode = "22631910",
                    District = "Rio de Janeiro",
                    Street = "Avenida das Américas"
                },
                Identity = customer.txtDocumentNumber,
                Name = customer.txtName
            };
        }

        private PaymentRequest GetPaymentRequest(tblPersons customer, tblEmailLog card, FoneClubeContext ctx)
        {
            var debitCard = new CardUtils().GetCard(card.txtEmail);

            return new PaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                //OrderTotal = 10.05m,
                OrderTotal = 1005,
                CreditCardCvv2 = "820",
                CreditCardExpireMonth = 11,
                CreditCardExpireYear = 2019,
                CreditCardName = "Marcio Guiamaraes Franco",
                CreditCardNumber = "4830420087436307",
                CreditCardType = "visa"    //visa, Master, Discover, Amex, Elo, Aura, JCB, Diners, Hicard

            };

            return new PaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                OrderTotal = 8,

                CreditCardCvv2 = debitCard.Cvv,
                CreditCardExpireMonth = Convert.ToInt32(debitCard.ExpirationMonth),
                CreditCardExpireYear = Convert.ToInt32(debitCard.ExpirationYear),
                CreditCardName = debitCard.HolderName,
                CreditCardNumber = debitCard.Number,
                CreditCardType = debitCard.Flag    //visa, Master, Discover, Amex, Elo, Aura, JCB, Diners, Hicard
            };


        }

        private DebitPaymentSettings GetDebitoPaymentSettings(CieloDebitoTransaction cieloDebitoTransaction)
        {

            var endpointAPI = "http://api.foneclube.com.br/";

            if (new StatusAPIAccess().GetDatabaseName() == "foneclube-homol")
                endpointAPI = "http://homol-api.p2badpmtjj.us-east-2.elasticbeanstalk.com/";


            endpointAPI += "api/cielo/transaction/restore/history/" + cieloDebitoTransaction.HistoryId;

            bool useSandbox = false;
            var returnURL = endpointAPI;
            var provider = useSandbox ? "Simulado" : "Bradesco";
            var merchantId = useSandbox ? "b27f29ac-64b8-407d-9297-c9828cc838dd" : "99879411-cc78-4e23-875a-95c91639260b";
            var securityKey = useSandbox ? "CJJTEDHWIKYQECLIIBFTFXWEPVUSTZTNBLIQEQUV" : "B5fz29Vtfj3MuJ4KQGwSLDiEFiVPmeSZ6bMG1QQw";

            return new DebitPaymentSettings()
            {
                AdditionalFee = 0,
                AdditionalFeePercentage = false,
                MerchantId = new Guid(merchantId),
                PaymentType = "DebitCard",
                Installments = 1,
                Provider = provider,
                ReturnUrl = returnURL,
                SecurityKey = securityKey,
                UseSandbox = useSandbox,
                AuthenticateTransaction = true
            };
        }

        public DebitPaymentSettings GetDebitoPaymentSettings()
        {
            //todo
            //webconfig
            bool useSandbox = false;
            var returnURL = "https://loja.foneclube.com.br/CieloDebit/Verify";
            var provider = useSandbox ? "Simulado" : "Bradesco";
            var merchantId = useSandbox ? "b27f29ac-64b8-407d-9297-c9828cc838dd" : "99879411-cc78-4e23-875a-95c91639260b";
            var securityKey = useSandbox ? "CJJTEDHWIKYQECLIIBFTFXWEPVUSTZTNBLIQEQUV" : "B5fz29Vtfj3MuJ4KQGwSLDiEFiVPmeSZ6bMG1QQw";

            return new DebitPaymentSettings()
            {
                AdditionalFee = 0,
                AdditionalFeePercentage = false,
                MerchantId = new Guid(merchantId),
                PaymentType = "DebitCard",
                Installments = 1,
                Provider = provider,
                ReturnUrl = returnURL,
                SecurityKey = securityKey,
                UseSandbox = useSandbox,
                AuthenticateTransaction = true
            };
        }

        public CieloDebitoTransaction DecryptCieloTransaction(string cieloTransaction)
        {
            return JsonConvert.DeserializeObject<CieloDebitoTransaction>(new Utils().Base64Decode(cieloTransaction));
        }

        public string EncryptCieloTransaction(CieloDebitoTransaction cieloTransaction)
        {
            return new Utils().Base64Encode(JsonConvert.SerializeObject(cieloTransaction));
        }

        public void ExecuteDebito()
        {
            var cpf = "90616693753";

            var settings = GetDebitoPaymentSettings();

            var paymentRequest = new PaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                OrderTotal = 8,

                CreditCardCvv2 = "820",
                CreditCardExpireMonth = 11,
                CreditCardExpireYear = 2019,
                CreditCardName = "Marcio Guiamaraes Franco",
                CreditCardNumber = "4830420087436307",
                CreditCardType = "visa"    //visa, Master, Discover, Amex, Elo, Aura, JCB, Diners, Hicard

            };

            var customer = new CieloLib.Domain.Customer
            {
                Address = new CustomerAddress
                {
                    City = "Rio de Janeiro",
                    Country = "Brasil",
                    State = "RJ",
                    Number = "10021",
                    ZipCode = "22631910",
                    District = "Rio de Janeiro",
                    Street = "Avenida das Américas"
                },
                Identity = cpf,
                Name = "Marcio Guiamaraes Franco"
            };

            var processor = new DebitProcessor(settings);
            var paymentResponse = processor.ProcessPayment(paymentRequest, customer);
        }

        public bool UpdateStatusTransactionsCielo()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var orders = new List<CieloOrder>();
                    var limite = DateTime.Now.AddDays(-90);
                    var cieloTransactions = ctx.tblTransactionsCielo
                        .Where(c => c.dteRegister > limite &&
                        c.intStatusPayment != (int)Transaction.Tipo.PaymentConfirmed &&
                        c.intStatusPayment != (int)Transaction.Tipo.Cancelado
                        ).ToList();

                    foreach (var transaction in cieloTransactions)
                    {
                        var status = GetTransactionStatus(ctx, transaction.txtPaymentId);
                        transaction.intStatusPayment = (int)status;
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
    }
}
