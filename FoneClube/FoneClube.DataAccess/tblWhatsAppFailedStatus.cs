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
    
    public partial class tblWhatsAppFailedStatus
    {
        public int intId { get; set; }
        public Nullable<int> intIdPerson { get; set; }
        public Nullable<int> intIdCharge { get; set; }
        public Nullable<System.DateTime> dteDateTime { get; set; }
        public string txtError { get; set; }
        public string txtPhoneNumber { get; set; }
        public string txtMessage { get; set; }
        public Nullable<bool> bitChargeSummary { get; set; }
        public Nullable<bool> bitResentSuccess { get; set; }
    }
}
