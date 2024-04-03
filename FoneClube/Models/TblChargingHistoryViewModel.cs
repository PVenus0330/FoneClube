using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoneClube.WebAPI.Models
{
    public class TblChargingHistoryViewModel
    {
        public int intId { get; set; }
        public Nullable<int> intIdCustomer { get; set; }
        public Nullable<int> intIdCollector { get; set; }
        public string txtCollectorName { get; set; }
        public Nullable<int> intIdPaymentType { get; set; }
        public string txtComment { get; set; }
        public System.DateTime dtePayment { get; set; }
        public string txtAmmountPayment { get; set; }
        public string txtTokenTransaction { get; set; }
        public Nullable<long> intIdBoleto { get; set; }
        public string txtAcquireId { get; set; }
        public Nullable<System.DateTime> dteCreate { get; set; }
        public Nullable<System.DateTime> dteModify { get; set; }
        public Nullable<int> intPaymentStatus { get; set; }
        public string txtTransactionComment { get; set; }
        public Nullable<System.DateTime> dteDueDate { get; set; }
        public Nullable<int> PhoneId { get; set; }
        public Nullable<System.DateTime> dteChargingDate { get; set; }
        public string txtChargingComment { get; set; }
    }
}