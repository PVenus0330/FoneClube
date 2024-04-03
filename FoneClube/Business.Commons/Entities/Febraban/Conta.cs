using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Conta
    {
        public Head Head { get; set; }
        public List<Resumo> Resumos { get; set; }
        public List<Endereco> Enderecos { get; set; }
        public List<Bilhetacao> Bilhetacoes { get; set; }
        public List<Servico> Servicos { get; set; }
        public List<Desconto> Descontos { get; set; }
        public Totalizador Totalizadores { get; set; }
        public int TipoOperadora { get; set; }

    }
}
