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
    
    public partial class tblPersons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblPersons()
        {
            this.tblComissionOrder = new HashSet<tblComissionOrder>();
            this.tblComissionTokens = new HashSet<tblComissionTokens>();
            this.tblCommissionOrders = new HashSet<tblCommissionOrders>();
            this.tblDiscountPrice = new HashSet<tblDiscountPrice>();
            this.tblEmailLog = new HashSet<tblEmailLog>();
            this.tblLineRecord = new HashSet<tblLineRecord>();
            this.tblPersonsAddresses = new HashSet<tblPersonsAddresses>();
            this.tblPersonsImages = new HashSet<tblPersonsImages>();
            this.tblPersonsPhones = new HashSet<tblPersonsPhones>();
            this.tblPersosAffiliateLinks = new HashSet<tblPersosAffiliateLinks>();
            this.tblPlans = new HashSet<tblPlans>();
            this.tblReferred = new HashSet<tblReferred>();
            this.tblReferred1 = new HashSet<tblReferred>();
            this.tblRenovaSenha = new HashSet<tblRenovaSenha>();
            this.tblChargingHistory = new HashSet<tblChargingHistory>();
            this.tblChargingScheduled = new HashSet<tblChargingScheduled>();
            this.tblInternationalDeposits = new HashSet<tblInternationalDeposits>();
            this.tblInternationalUserBalance = new HashSet<tblInternationalUserBalance>();
        }
    
        public int intIdPerson { get; set; }
        public Nullable<int> intContactId { get; set; }
        public System.DateTime dteRegister { get; set; }
        public string txtDocumentNumber { get; set; }
        public string txtName { get; set; }
        public string txtNickName { get; set; }
        public string txtEmail { get; set; }
        public Nullable<System.DateTime> dteBorn { get; set; }
        public Nullable<int> intGender { get; set; }
        public Nullable<int> intIdPagarme { get; set; }
        public Nullable<int> intIdRole { get; set; }
        public Nullable<int> intIdCurrentOperator { get; set; }
        public Nullable<long> matricula { get; set; }
        public Nullable<bool> bitManual { get; set; }
        public Nullable<bool> bitDelete { get; set; }
        public Nullable<bool> bitDesativoManual { get; set; }
        public Nullable<int> intIdLoja { get; set; }
        public string txtPassword { get; set; }
        public Nullable<bool> bitDadosPessoaisCadastrados { get; set; }
        public Nullable<bool> bitSenhaCadastrada { get; set; }
        public Nullable<bool> bitMultinivel { get; set; }
        public string txtPromoCode { get; set; }
        public Nullable<int> intDftBillPaymentDay { get; set; }
        public Nullable<int> intDftVerificar { get; set; }
        public string txtDefaultWAPhones { get; set; }
        public bool bitIntl { get; set; }
        public Nullable<bool> bitShopifyUser { get; set; }
        public Nullable<System.DateTime> dteNextActionDate { get; set; }
        public string txtNextAction { get; set; }
        public bool bitUsar2Preços { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblComissionOrder> tblComissionOrder { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblComissionTokens> tblComissionTokens { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblCommissionOrders> tblCommissionOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblDiscountPrice> tblDiscountPrice { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblEmailLog> tblEmailLog { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblLineRecord> tblLineRecord { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPersonsAddresses> tblPersonsAddresses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPersonsImages> tblPersonsImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPersonsPhones> tblPersonsPhones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPersosAffiliateLinks> tblPersosAffiliateLinks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPlans> tblPlans { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblReferred> tblReferred { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblReferred> tblReferred1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblRenovaSenha> tblRenovaSenha { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblChargingHistory> tblChargingHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblChargingScheduled> tblChargingScheduled { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblInternationalDeposits> tblInternationalDeposits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblInternationalUserBalance> tblInternationalUserBalance { get; set; }
    }
}
