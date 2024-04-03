using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.comission
{
    public class BonusOrder
    {
        public Person receiver;
        public double? intBonusValue;

        public int intPlanValue { get; set; }
        public int intIdComissionOrder { get; set; }
        public Nullable<int> intIdChargingHistory { get; set; }
        public Nullable<int> intIdTransaction { get; set; }
        public Nullable<int> intIdCustomerReceiver { get; set; }
        public Nullable<int> intIdPhone { get; set; }
        public Nullable<int> intIdPhonePlan { get; set; }
        public Nullable<int> intPercentBonus { get; set; }
        public Nullable<int> intIdAgent { get; set; }
        public bool bitComissionConceded { get; set; }
        public Nullable<System.DateTime> dteValidity { get; set; }
        public Nullable<System.DateTime> dteConceded { get; set; }
        public System.DateTime dteCreated { get; set; }
    }
}
