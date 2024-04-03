using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.linhas
{
    public class LinhaDetalhesMinimos
    {
        public int? intIdPlan;
        public string linhaLivreOperadora { get; set; }
        public Nullable<int> operadora { get; set; }
        public Nullable<int> intIdPerson { get; set; }
        public string txtName { get; set; }
        public string txtNickname { get; set; }
        public Nullable<bool> bitLinhaAtiva { get; set; }
        public string txtPlanoFoneclube { get; set; }
        public Nullable<int> intPrecoFoneclube { get; set; }
        public Nullable<int> intPrecoVip { get; set; }
        public Nullable<bool> bitPrecoVip { get; set; }

        public Nullable<int> idPhone { get; set; }
        public Nullable<int> ddd { get; set; }
        public Nullable<int> numeroTelefone { get; set; }
        public string numeroTelefoneCompleto { get; set; }

        public bool? divergencia { get; set; }

        public List<Service> Servicos { get; set; }


        public string CodigoCliente { get; set; }
        public object RazaoSocial { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object CCID { get; set; }
    }
}
