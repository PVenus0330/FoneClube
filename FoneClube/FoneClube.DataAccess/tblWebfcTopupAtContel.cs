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
    
    public partial class tblWebfcTopupAtContel
    {
        public int intId { get; set; }
        public System.DateTime dteDateAdded { get; set; }
        public Nullable<bool> bitUpdateFailed { get; set; }
        public string txtLinha { get; set; }
        public string txtPortIn { get; set; }
        public string txtRecarga_automatica_plano { get; set; }
        public string txtdata_renovacao { get; set; }
        public string txtSaldoPre { get; set; }
        public string txtMinutesPre { get; set; }
        public string txtSMSPre { get; set; }
        public int intIdPlanPre { get; set; }
        public int intIdPlanPost { get; set; }
        public string txtSaldoPost { get; set; }
        public string txtMinutesPost { get; set; }
        public string txtSMSPost { get; set; }
        public string txtTotalSaldo { get; set; }
        public Nullable<System.DateTime> dteVigencia { get; set; }
        public Nullable<bool> bitRecarga_Manul_Extra { get; set; }
        public Nullable<bool> bitManual { get; set; }
    }
}