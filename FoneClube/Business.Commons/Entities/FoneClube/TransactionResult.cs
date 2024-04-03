using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class TransactionResult
    {
        public string LinkBoleto { get; set; }
        public string PixQRCode { get; set; }
        public string Token { get; set; }
        public string Id { get; set; }
        public int Aquire_Id { get; set; }
        public bool StatusPaid { get; set; }
        public string DescriptionMessage { get; set; }
        public bool? Recusado { get; set; }
        public bool ChargingHistorySaved { get; set; }
        public int PaymentType { get; set; }
        public DateTime? PixExpiryDate { get; set; }
        public string BoletoBarcode { get; set; }
        public string BoletoUrl { get; set; }
    }
}
