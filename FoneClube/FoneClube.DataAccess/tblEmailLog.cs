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
    
    public partial class tblEmailLog
    {
        public int intId { get; set; }
        public Nullable<int> intIdPerson { get; set; }
        public System.DateTime dteRegister { get; set; }
        public string txtEmail { get; set; }
        public Nullable<int> intIdTypeEmail { get; set; }
    
        public virtual tblPersons tblPersons { get; set; }
    }
}