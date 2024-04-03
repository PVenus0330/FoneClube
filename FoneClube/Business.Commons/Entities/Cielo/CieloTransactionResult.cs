using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class CieloTransactionResult
    {
        public string MerchantOrderId { get; set; }
        public CieloCustomer Customer { get; set; }
        public CieloPayment Payment { get; set; }
    }

    public class CieloAddress
    {
    }

    public class CieloCustomer
    {
        public string Name { get; set; }
        public CieloAddress Address { get; set; }
    }

    public class CieloCreditCard
    {
        public string CardNumber { get; set; }
        public string Holder { get; set; }
        public string ExpirationDate { get; set; }
        public string Brand { get; set; }
    }

    public class CieloLink
    {
        public string Method { get; set; }
        public string Rel { get; set; }
        public string Href { get; set; }
    }

    public class CieloPayment
    {
        public int ServiceTaxAmount { get; set; }
        public int Installments { get; set; }
        public string Interest { get; set; }
        public bool Capture { get; set; }
        public bool Authenticate { get; set; }
        public CieloCreditCard CreditCard { get; set; }
        public CieloDebitoCard DebitCard { get; set; }
        public string ProofOfSale { get; set; }
        public string Tid { get; set; }
        public string AuthorizationCode { get; set; }
        public string Eci { get; set; }
        public string PaymentId { get; set; }
        public string Type { get; set; }
        public int Amount { get; set; }
        public string ReceivedDate { get; set; }
        public int CapturedAmount { get; set; }
        public string CapturedDate { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public string Provider { get; set; }
        public int Status { get; set; }
        public List<CieloLink> Links { get; set; }

        public string Instructions { get; set; }
        public string ExpirationDate { get; set; }
        public string Demostrative { get; set; }
        public string Url { get; set; }
        public string BoletoNumber { get; set; }
        public string BarCodeNumber { get; set; }
        public string DigitableLine { get; set; }
        public string Assignor { get; set; }
        public string Address { get; set; }
        public string Identification { get; set; }
    }


    public class CieloDebitoCard
    {
        public string CardNumber { get; set; }
        public string Holder { get; set; }
        public string ExpirationDate { get; set; }
        public string Brand { get; set; }
    }

}
