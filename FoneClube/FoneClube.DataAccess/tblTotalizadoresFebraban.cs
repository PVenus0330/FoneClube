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
    
    public partial class tblTotalizadoresFebraban
    {
        public int intId { get; set; }
        public Nullable<int> intContaId { get; set; }
        public string txtControleSequencial { get; set; }
        public string txtCodigoCliente { get; set; }
        public string txtVencimento { get; set; }
        public string txtEmissao { get; set; }
        public string txtQuantidadeRegistros { get; set; }
        public string txtQuantidadeLinhas { get; set; }
        public string txtSinalTotal { get; set; }
        public string txtValorTotal { get; set; }
        public string txtFiller { get; set; }
    
        public virtual tblContasFebraban tblContasFebraban { get; set; }
    }
}
