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
    
    public partial class tblServices
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblServices()
        {
            this.tblPhonesServices = new HashSet<tblPhonesServices>();
        }
    
        public int intIdService { get; set; }
        public string ServiceDesc { get; set; }
        public Nullable<int> assinaturas { get; set; }
        public Nullable<bool> IsExtraOption { get; set; }
        public Nullable<int> intValorOperadora { get; set; }
        public Nullable<int> intValorFoneclube { get; set; }
        public Nullable<bool> bitEditavel { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPhonesServices> tblPhonesServices { get; set; }
    }
}
