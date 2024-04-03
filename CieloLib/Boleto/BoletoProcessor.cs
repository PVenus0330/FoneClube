using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Boleto
{
    public class BoletoProcessor
    {
        CommHelper helper = null;
        BoletoPaymentSettings settings;

        string txnStatus;
        public const int BRL = 986;
        public const string TS_PENDING = "PENDING";
        public const string TS_PAID = "PAID";
        public const string TS_ERROR = "ERROR";
        public const string TS_EXCEPTION = "EXCEPTION";

        public BoletoProcessor(BoletoPaymentSettings settings)
        {
            this.settings = settings;
            helper = new CommHelper(settings.MerchantId.ToString(), settings.SecurityKey, settings.UseSandbox);
        }

        public PaymentResponse<BoletoResponse> ProcessPayment(PaymentRequest paymentRequest, CieloLib.Domain.Customer customer)
        {
            try
            {
                txnStatus = TS_PENDING;

                if (string.IsNullOrWhiteSpace(customer.Identity) && !settings.UseSandbox)
                {
                    throw new Exception("Customer CPF required. Please upate before you checkout again.");
                }


                BoletoRequest request = new BoletoRequest()
                {
                    MerchantOrderId = paymentRequest.OrderGuid.ToString()
                };

                request.Payment.Provider = settings.Provider;
                request.Customer = customer;
                request.Payment.Type = settings.PaymentType;
                request.Payment.ReturnUrl = settings.ReturnUrl;
                request.Payment.Amount = Math.Round(Decimal.Parse(paymentRequest.OrderTotal.ToString()), 2);
                decimal amountInCents = paymentRequest.OrderTotal * new decimal(100);
                request.Payment.Amount = amountInCents;
                request.Payment.Instructions = settings.PaymentInstruction;
                request.Payment.Identification = customer.Identity;
                request.Payment.ExpirationDate = DateTime.UtcNow.AddDays(settings.DaysBeforeLinkExpiration).ToString("yyyy-MM-dd");
                BoletoResponse response = helper.PostRequest<BoletoRequest, BoletoResponse>(request);

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
                        switch (response.Payment.Status)
                        {
                            case 1:
                                txnStatus = TS_PENDING;
                                break;
                            default:
                                txnStatus = TS_ERROR;
                                break;
                        }
                    }
                }

                return new PaymentResponse<BoletoResponse> { Message = response.Payment.ReturnMessage, Status = txnStatus, Response = response };
            }
            catch (Exception ex)
            {
                return new PaymentResponse<BoletoResponse> { Message = ex.Message, Status = TS_EXCEPTION, Exception = ex, Response = null };
            }
        }

        private bool IsErrorResponse(BoletoResponse response)
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
