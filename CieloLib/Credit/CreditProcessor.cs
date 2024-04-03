using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CieloLib.Credit
{
	public class CreditProcessor
	{
        CommHelper helper = null;
        CreditPaymentSettings settings;

        string txnStatus;
        public const int BRL = 986;
        public const string TS_PENDING = "PENDING";
        public const string TS_PAID = "PAID";
        public const string TS_CAPTURED = "CAPTURED";
        public const string TS_ERROR = "ERROR";
        public const string TS_EXCEPTION = "EXCEPTION";
        public const string TS_AUTHORIZED = "AUTHORIZED";
        public const string TS_VOIDED = "VOIDED";
        public const string TS_REFUNDED = "REFUNDED";

        public CreditProcessor(CreditPaymentSettings settings)
        {
            this.settings = settings;
            helper = new CommHelper(settings.MerchantId.ToString(), settings.SecurityKey, settings.UseSandbox);
        }

        public PaymentResponse<CreditResponse> ProcessPayment(PaymentRequest paymentRequest, CieloLib.Domain.Customer customer, Domain.StoredCard storedCard = null, Domain.Customer saveCardCustomerInfo = null)
        {
            try
            {

                txnStatus = TS_PENDING;

                if (string.IsNullOrWhiteSpace(customer.Identity) && !settings.UseSandbox)
                {
                    throw new Exception("Customer CPF required. Please upate before you checkout again.");
                }

                CreditRequest request = new CreditRequest()
                {
                    MerchantOrderId = paymentRequest.OrderGuid.ToString()
                };

                request.Customer.Name = customer.Name;
                request.Customer.Status = customer.Status;

                request.Payment.PaymentId = paymentRequest.OrderGuid;
                request.Payment.Type = settings.PaymentType;
                request.Payment.Authenticate = settings.AuthenticateTransaction;
                request.Payment.ReturnUrl = settings.ReturnUrl;
                request.Payment.Installments = settings.Installments;
                request.Payment.SoftDescriptor = paymentRequest.SoftDescriptor;
                decimal amountInCents = paymentRequest.OrderTotal * new decimal(100);
                request.Payment.Amount = amountInCents;
                request.Payment.CurrencyIsoCode = BRL;

                if (storedCard != null)
                {
                    if (storedCard.CardToken == null || storedCard.CardBrand == null)
                    {
                        throw new ArgumentException("invalid tokenized cards");
                    }
                    request.Payment.CreditCard.CardToken = new Guid(storedCard.CardToken);
                    request.Payment.CreditCard.Brand = storedCard.CardBrand;
                    request.Payment.CreditCard.SecurityCode = storedCard.CreditCardCvv2;
                }
                else
                {
                    request.Payment.CreditCard.Holder = paymentRequest.CreditCardName;
                    request.Payment.CreditCard.CardNumber = paymentRequest.CreditCardNumber;
                    request.Payment.CreditCard.SecurityCode = paymentRequest.CreditCardCvv2;
                    request.Payment.CreditCard.Brand = paymentRequest.CreditCardType;
                    Domain.CreditCard creditCard = request.Payment.CreditCard;
                    int creditCardExpireMonth = paymentRequest.CreditCardExpireMonth;
                    creditCard.ExpirationDate = string.Concat(creditCardExpireMonth.ToString("00"), "/", paymentRequest.CreditCardExpireYear);
                    if (paymentRequest.SaveCard)
                    {
                        if (saveCardCustomerInfo == null)
                            throw new ArgumentNullException("Cutomer Info required to save card");
                        request.Payment.CreditCard.SaveCard = true;
                        request.Customer = saveCardCustomerInfo;
                    }
                }

                CreditResponse response = helper.PostRequest<CreditRequest, CreditResponse>(request);

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
                return new PaymentResponse<CreditResponse> { Message = response.Payment.ReturnMessage, Status = txnStatus, Response = response };
            }
            catch (Exception ex)
            {
                return new PaymentResponse<CreditResponse> { Message = ex.Message, Status = TS_EXCEPTION, Exception = ex, Response = null };
            }
        }

        public PaymentResponse<CaptureResponse> ProcessCapture(string paymentId)
        {
            try
            {

                if (!string.IsNullOrWhiteSpace(paymentId))
                {
                    string endPoint = $"/1/sales/{paymentId}/capture";
                    var response = helper.PutRequest<CieloBaseRequest, CaptureResponse>(null, endPoint);

                    if (response.ReturnCode == 6)
                    {
                        return new PaymentResponse<CaptureResponse> { Message = response.ReturnMessage, Status = TS_CAPTURED, Response = response };
                    }
                    else if (!string.IsNullOrWhiteSpace(response.Message))
                    {
                        return new PaymentResponse<CaptureResponse> { Message = response.Message, Response = response, Status = TS_ERROR };
                    }
                    else
                    {
                        return new PaymentResponse<CaptureResponse> { Message = response.ReturnMessage, Status = TS_ERROR, Response = response };
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid payment id");
                }
            }
            catch (Exception ex)
            {
                return new PaymentResponse<CaptureResponse> { Message = ex.Message, Status = TS_EXCEPTION };
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

        private bool IsErrorResponse(CreditResponse response)
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