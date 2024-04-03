using CieloLib.Email;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CieloLib.Debit
{
	public class DebitProcessor
	{
        CommHelper helper = null;
        DebitPaymentSettings settings;

        string txnStatus;
        public const int BRL = 986;
        public const string TS_PENDING = "PENDING";
        public const string TS_PAID = "PAID";
        public const string TS_ERROR = "ERROR";
        public const string TS_EXCEPTION = "EXCEPTION";
        public const string TS_AUTHORIZED = "AUTHORIZED";
        public const string TS_VOIDED = "VOIDED";
        public const string TS_REFUNDED = "REFUNDED";

        public DebitProcessor(DebitPaymentSettings settings)
        {
            this.settings = settings;
            this.helper = new CommHelper(settings.MerchantId.ToString(), settings.SecurityKey, settings.UseSandbox);
        }





        public PaymentResponse<DebitResponse> ProcessPayment(PaymentRequest paymentRequest, CieloLib.Domain.Customer customer)
        {
            try
            {
                

                txnStatus = TS_PENDING;

                if (string.IsNullOrWhiteSpace(customer.Identity) && !settings.UseSandbox)
                {
                    throw new Exception("Customer CPF required. Please upate before you checkout again.");
                }

                DebitRequest request = new DebitRequest()
                {
                    MerchantOrderId = paymentRequest.OrderGuid.ToString()
                };

                request.Customer.Name = customer.Name;
                request.Customer.Status = customer.Status;

                request.Payment.PaymentId = paymentRequest.OrderGuid;
                request.Payment.Type = settings.PaymentType;
                request.Payment.Authenticate = settings.AuthenticateTransaction;
                request.Payment.ReturnUrl = settings.ReturnUrl;
                request.Payment.DebitCard.Brand = paymentRequest.CreditCardType;
                request.Payment.Installments = settings.Installments;
                request.Payment.Amount = paymentRequest.OrderTotal;
                request.Payment.CurrencyIsoCode = BRL;

                request.Payment.DebitCard.Holder = paymentRequest.CreditCardName;
                request.Payment.DebitCard.CardNumber = paymentRequest.CreditCardNumber;
                request.Payment.DebitCard.SecurityCode = paymentRequest.CreditCardCvv2;
                request.Payment.SoftDescriptor = paymentRequest.SoftDescriptor;
                Domain.DebitCard debitCard = request.Payment.DebitCard;
                int creditCardExpireMonth = paymentRequest.CreditCardExpireMonth;
                debitCard.ExpirationDate = string.Concat(creditCardExpireMonth.ToString("00"), "/", paymentRequest.CreditCardExpireYear);

               

                var response = helper.PostRequest<DebitRequest, DebitResponse>(request);

                //try
                //{
                //    new EmailUtil().SendEmail("rodrigocardozop@gmail.com", "Debug " + DateTime.Now.ToString(), "passou o PostRequest" + JsonConvert.SerializeObject(response));
                //}
                //catch (Exception) {
                //    new EmailUtil().SendEmail("rodrigocardozop@gmail.com", "Debug " + DateTime.Now.ToString(), "passou o PostRequest ex" + response);
                //}

                if (response == null)
                {
                    
                    throw new Exception("Cielo Payment Gateway error");
                }
                else
                {
                    
                    if (response.Payment.Status == 0)
                    {
                        if (!string.IsNullOrWhiteSpace(response.Payment.AuthenticationUrl) && !IsErrorResponse(response))
                        {
                            txnStatus = TS_PENDING;
                        }
                        else
                        {
                            
                            txnStatus = TS_ERROR;
                        }
                    }
                    else
                    {
                        txnStatus = TS_PENDING;

                        switch (response.Payment.Status)
                        {
                            case 1:
                                txnStatus = TS_AUTHORIZED;
                                break;
                            case 2:
                                txnStatus = TS_PAID;
                                break;
                            case 10:
                                txnStatus = TS_VOIDED;
                                break;
                            case 11:
                                txnStatus = TS_REFUNDED;
                                break;
                            default:
                                txnStatus = TS_ERROR;
                                //throw new Exception(response.Payment.ReturnMessage);
                                break;
                        }
                    }
                }

                response.PaymentUrl = response.Payment.AuthenticationUrl;
                return new PaymentResponse<DebitResponse> { Message = response.Payment.ReturnMessage, Status = txnStatus, Response = response };
            }
            catch (Exception ex)
            {
                return new PaymentResponse<DebitResponse> { Message = ex.Message, Status = TS_EXCEPTION, Exception = ex, Response = null };
            }
        }


        public T VerifyPayment<T>(string paymentQueryUrl)
        {
            if (!string.IsNullOrWhiteSpace(paymentQueryUrl))
            {
                var response = helper.GetRequest<T>(paymentQueryUrl);
                return response;
            }
            else
            {
                throw new ArgumentException("Invalid query url");
            }
        }

        private bool IsErrorResponse(DebitResponse response)
        {
            var msg = response.Payment.ReturnMessage;
            if (!string.IsNullOrWhiteSpace(msg) && msg.ToLower().IndexOf("erro") >= 0)
            {
                return true;
            }

            return false;
        }

    }
}