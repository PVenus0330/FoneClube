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
    
    public partial class tblPhonesReadyToReset
    {
        public int intId { get; set; }
        public string txtPhone { get; set; }
        public string txtICCID { get; set; }
        public System.DateTime dteActivated { get; set; }
        public System.DateTime dteInsert { get; set; }
        public Nullable<System.DateTime> dteUpdate { get; set; }
        public bool bitResetSuccess { get; set; }
        public string txtError { get; set; }
    }
}
