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
    
    public partial class tblCommissions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblCommissions()
        {
            this.tblReferred = new HashSet<tblReferred>();
            this.tblCommissionOrders = new HashSet<tblCommissionOrders>();
        }
    
        public int intIdComission { get; set; }
        public int intComissionCost { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblReferred> tblReferred { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblCommissionOrders> tblCommissionOrders { get; set; }
    }
}
