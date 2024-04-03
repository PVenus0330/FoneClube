using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Eft
{
    public class EftProcessor
    {
        CommHelper helper = null;
        EftPaymentSettings settings;

        string txnStatus;
        public const int BRL = 986;
        public const string TS_PENDING = "PENDING";
        public const string TS_PAID = "PAID";
        public const string TS_ERROR = "ERROR";
        public const string TS_EXCEPTION = "EXCEPTION";
        public const string TS_AUTHORIZED = "AUTHORIZED";
        public const string TS_VOIDED = "VOIDED";
        public const string TS_REFUNDED = "REFUNDED";


        public EftProcessor(EftPaymentSettings settings, ILogger logger)
        {
            this.settings = settings;
            helper = new CommHelper(settings.MerchantId.ToString(), settings.SecurityKey, settings.UseSandbox);
        }

        public PaymentResponse<EftResponse> ProcessPayment(PaymentRequest paymentRequest, CieloLib.Domain.Customer customer)
        {
            try
            {
                txnStatus = TS_PENDING;

                if (string.IsNullOrWhiteSpace(customer.Identity) && !settings.UseSandbox)
                {
                    throw new Exception("Customer CPF required. Please upate before you checkout again.");
                }
                
                EftRequest eftRequest = new EftRequest()
                {
                    MerchantOrderId = paymentRequest.OrderGuid.ToString()
                };

                eftRequest.Payment.Provider = settings.Provider;
                eftRequest.Customer = customer;
                eftRequest.Payment.PaymentId = paymentRequest.OrderGuid;
                eftRequest.Payment.Type = settings.PaymentType;
                eftRequest.Payment.ReturnUrl = settings.ReturnUrl;
                eftRequest.Payment.Installments = settings.Installments;
                decimal amountInCents = paymentRequest.OrderTotal * new decimal(100);
                eftRequest.Payment.Amount = amountInCents;
                eftRequest.Payment.CurrencyIsoCode = BRL;

                EftResponse response = helper.PostRequest<EftRequest, EftResponse>(eftRequest);

                if (response == null)
                {
                    throw new Exception("Cielo Payment Gateway error");
                }
                else
                {
                    if (response.Payment.Status == 0)
                    {
                        if (!string.IsNullOrWhiteSpace(response.Payment.Url) && !IsErrorResponse(response))
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

                return new PaymentResponse<EftResponse> { Message = response.Payment.ReturnMessage, Status = txnStatus, Response = response };
            }
            catch (Exception ex)
            {
                return new PaymentResponse<EftResponse> { Message = ex.Message, Status = TS_EXCEPTION, Exception = ex, Response = null };
            }
        }

        private bool IsErrorResponse(EftResponse response)
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
