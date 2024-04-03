using CieloLib.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CieloLib.Boleto.Domain
{
    public class Payment
    {
        public string Instructions { get; set; }
        public string ExpirationDate { get; set; }
        public string Demonstrative { get; set; }
        public string Url { get; set; }
        public string BoletoNumber { get; set; }
        public string BarCodeNumber { get; set; }
        public string DigitableLine { get; set; }
        public string Assignor { get; set; }
        public string Address { get; set; }
        public string Identification { get; set; }
        public decimal Amount { get; set; }
        public string ReceivedDate { get; set; }
        public string Provider { get; set; }
        public int Status { get; set; }
        public bool IsSplitted { get; set; }
        public string PaymentId { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public List<Link> Links { get; set; }


        public string ReturnUrl { get; set; }
        public string ReturnMessage { get; set; }
    }
}