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
    
    public partial class tblGenericPhoneFlags
    {
        public int intId { get; set; }
        public Nullable<int> intIdPhone { get; set; }
        public Nullable<System.DateTime> dteRegister { get; set; }
        public Nullable<System.DateTime> dteUpdate { get; set; }
        public Nullable<bool> bitPendingInteraction { get; set; }
        public string txtDescription { get; set; }
        public Nullable<int> intIdFlag { get; set; }
        public Nullable<int> intIdPerson { get; set; }
    
        public virtual tblGenericFlags tblGenericFlags { get; set; }
    }
}