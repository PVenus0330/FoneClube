using FoneClube.Business.Commons.Entities.ViewModel.MinhaConta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class CustomerMinhaContaViewModel
    {
        public int id { get; set; }

        public bool dadosPessoasCadastrados { get; set; }
        public bool senhaCadastrada { get; set; }
        public bool enderecoCadastrado { get; set; }

        public DadosPessoais dadosPessoais { get; set; }
        public Endereco endereco { get; set; }
        public Senha senha { get; set; }

        public bool clienteMultinivel { get; set; }
        public bool? indicado { get; set; }
    }
}
