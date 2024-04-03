using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel.Amigos
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public int IdPagarme { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime Register { get; set; }
        public string Name { get; set; }
        public List<Filho> Filhos { get; set; }
    }

    public class Filho
    {
        public int Id { get; set; }
        public int IdPagarme { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime Register { get; set; }
        public string Name { get; set; }
        public List<PhoneFilho> Phones { get; set; }
        public string UltimoPagamento { get; set; }
        public int DiasDoPagamento { get; set; }
    }

    public class PhoneFilho
    {
        public int Id { get; set; }
        public int IdPlan { get; set; }
    }
}
