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
    
    public partial class tblPersonsAddresses
    {
        public int intId { get; set; }
        public Nullable<int> intIdPerson { get; set; }
        public string txtStreet { get; set; }
        public string txtComplement { get; set; }
        public Nullable<int> intStreetNumber { get; set; }
        public string txtNeighborhood { get; set; }
        public string txtCity { get; set; }
        public string txtState { get; set; }
        public string txtCep { get; set; }
        public string txtCountry { get; set; }
        public Nullable<int> intAdressType { get; set; }
    
        public virtual tblPersons tblPersons { get; set; }
    }
}
