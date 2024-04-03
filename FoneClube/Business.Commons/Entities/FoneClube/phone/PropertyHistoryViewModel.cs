using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.phone
{
    public class PropertyHistoryViewModel
    {
        public int IdHistory { get; set; }
        public Nullable<int> IdPerson { get; set; }     
        public Nullable<int> IdOperator { get; set; }
        public Nullable<int> IdPlan { get; set; }
        public Nullable<double> PlanPrice { get; set; }
        public Nullable<int> IdPhone { get; set; }
        public Nullable<int> PhoneNumber { get; set; }
        public Nullable<int> PhoneDDD { get; set; }
        public Nullable<int> EventType { get; set; }
        public Nullable<int> IdStatus { get; set; }
        public Nullable<System.DateTime> Change { get; set; }
        public Nullable<System.DateTime> Entrada { get; set; }
        public Nullable<System.DateTime> Saida { get; set; }
    }
}
