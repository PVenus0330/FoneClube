using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoneClube.Business.Commons.Entities.FoneClube;

namespace FoneClube.Business.Commons.Entities.ViewModel
{

    public class InstaRegisterClientOrLineViewModel
    {
        public PersonInfo Person { get; set; }
        public Phone CustomerPhone { get; set; }
        public List<Phone> Phones { get; set; }
        public Adress Address { get; set; }
        public string ActivationPwd { get; set; }
        public string ActivationCPF { get; }
        public bool IsActivateBeforePayment { get; set; }
        public int ShipmentType { get; set; }
    }

    public class PersonInfo
    {
        public string CPF { get; set; }
        public int CPFType { get; set; }
        public string Nome { get; set; }
        public string WhatsAppNumber { get; set; }
        public string Email { get; set; }
        public PersonParentModel Parent { get; set; }
    }

}
