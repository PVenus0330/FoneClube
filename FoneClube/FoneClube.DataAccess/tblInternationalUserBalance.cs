//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FoneClube.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblInternationalUserBalance
    {
        public int intId { get; set; }
        public int intIdPerson { get; set; }
        public decimal intAmountBalance { get; set; }
        public Nullable<System.DateTime> dteAdded { get; set; }
        public Nullable<System.DateTime> dteUpdated { get; set; }
    
        public virtual tblPersons tblPersons { get; set; }
    }
}
