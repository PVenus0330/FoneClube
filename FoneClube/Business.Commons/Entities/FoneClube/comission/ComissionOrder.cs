using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.comission
{
    public class ComissionOrder
    {
        public int intIdComissionOrder { get; set; }
        public Nullable<int> intIdComission { get; set; }
        public Nullable<int> intIdBonus { get; set; }
        public int intIdTransaction { get; set; }
        public Nullable<int> intIdCustomerReceiver { get; set; }
        public Nullable<int> intIdAgent { get; set; }
        public bool bitComissionConceded { get; set; }
        public Nullable<System.DateTime> dteValidity { get; set; }
        public Nullable<System.DateTime> dteConceded { get; set; }
        public System.DateTime dteCreated { get; set; }
        public Nullable<int> intAmount { get; set; }
        public Nullable<int> intTotalLinhas { get; set; }
        public Nullable<int> intIdCustomerGiver { get; set; }
    }
}
