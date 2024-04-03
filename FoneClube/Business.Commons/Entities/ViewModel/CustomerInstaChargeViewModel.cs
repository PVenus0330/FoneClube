using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class CustomerInstaChargeViewModel
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string WhatsAppNumber { get; set; }
        public string Email { get; set; }
        public int Amount { get; set; }
        public int CpfType { get; set; }
        public int PaymentType { get; set; }
        public string SelectedPlans { get; set; }
        public int ShipmentType { get; set; }
        public string Comment { get; set; }
        public string Vencimento { get; set; }
        public string Vigencia { get; set; }
        public UserAddress ShipmentAddress { get; set; }
        public bool IsChargeReq { get; set; }
        public PersonParentModel Parent { get; set; }
        public bool Bonus { get; set; }
        public bool Esim { get; set; }
    }

    public class UserAddress
    {
        public string Bairro { get; set; }
        public string CEP { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Numero { get; set; }
        public string Rua { get; set; }
        public string complemento { get; set; }
    }
}
