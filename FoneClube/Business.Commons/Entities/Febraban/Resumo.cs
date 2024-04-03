using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Resumo
    {
        public string ControleSequencial { get; set; }
        public string ContaUnicaID { get; set; }
        public string Vencimento { get; set; }
        public string Emissao { get; set; }
        public string IDUnicoNRC { get; set; }
        public string RecursoCNL { get; set; }
        public string Localidade { get; set; }
        public string DDD { get; set; }
        public string NumeroTelefone { get; set; }
        public string TipoServico { get; set; }
        public string DescricaoServico { get; set; }
        public string CaracteristicaRecurso { get; set; }
        public string DegrauRecurso { get; set; }
        public string VelocidadeRecurso { get; set; }
        public string UnidadeVelocidadeRecurso { get; set; }
        public string InicioAssinatura { get; set; }
        public string FimAssinatura { get; set; }
        public string InicioPeriodoServico { get; set; }
        public string FimPeriodoServico { get; set; }
        public string UnidadeConsumo { get; set; }
        public string QuantidadeConsumo { get; set; }
        public string SinalValorConsumo { get; set; }
        public string ValorConsumo { get; set; }
        public string SinalAssinatura { get; set; }
        public string ValorAssinatura { get; set; }
        public string Aliquota { get; set; }
        public string SinalICMS { get; set; }
        public string ValorICMS { get; set; }
        public string SinalTotalOutrosImpostos { get; set; }
        public string ValorTotalImpostos { get; set; }
        public string NumeroNotaFiscal { get; set; }
        public string SinalValorConta { get; set; }
        public string ValorConta { get; set; }
        public string Filler { get; set; }

    }
}
