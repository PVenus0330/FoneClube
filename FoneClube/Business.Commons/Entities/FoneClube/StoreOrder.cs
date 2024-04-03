using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class StoreOrder
    {
        public long OrderId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int NumberOfPlans { get; set; }
        public int Total { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; }
        public long IdTransaction { get; set; }
        public int IdCharge { get; set; }
        public int IdPerson { get; set; }
        public List<StorePlans> Plans { get; set; }
    }

    public class StorePlans
    {
        public int Id { get; set; }
        public int IdPerson { get; set; }
        public bool ESim { get; set; }
        public bool Port { get; set; }
        public bool Activated { get; set; }
        public string PortNumber { get; set; }
        public int IdPlan { get; set; }
        public string PlanDescription { get; set; }
        public string PlanAmount { get; set; }
        public string ChipAmount { get; set; }
        public string ShippingAmount { get; set; }
        public long OrderId { get; set; }
    }

    public class StoreCharges
    {
        public int Id { get; set; }
        public long TransactionId { get; set; }
        public DateTime? Vencimento { get; set; }
        public DateTime? Vigencia { get; set; }
        public string Source { get; set; }
        public int Total { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string PaymentDate { get; set; }
    }
}
