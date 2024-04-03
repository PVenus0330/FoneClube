using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel.Plano
{
    public class PhonePlanViewModel
    {
        public string Telefone { get; set; }
        public string TelefoneFormatado { get; set; }
        public string DescricaoPlano { get; set; }
        public string DescricaoAbreviadaPlano { get; set; }
    }

    public class CartUserViewModel
    {
        public int id { get; set; }
    }

    public class CartViewModel
    {
        public int PersonId { get; set; }

        public string Plans { get; set; }
    }
}
