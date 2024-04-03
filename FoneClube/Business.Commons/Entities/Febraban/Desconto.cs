using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Desconto
    {
        public string ControleSequencial { get; set; }
        public string Vencimento { get; set; }
        public string Emissao { get; set; }
        public string IDUnicoNRC { get; set; }
        public string ContaUnicaID { get; set; }
        public string RecursoCNL { get; set; }
        public string DDD { get; set; }
        public string NumeroTelefone { get; set; }
        public string GrupoCategoria { get; set; }
        public string DescricaoGrupoCategoria { get; set; }
        public string SinalValorLigacao { get; set; }
        public string BaseCalculoDesconto { get; set; }
        public string PercentualDesconto { get; set; }
        public string ValorLigacao { get; set; }
        public string DataInicioAcerto { get; set; }
        public string HoraInicioAcerto { get; set; }
        public string DataFimAcerto { get; set; }
        public string HoraFimAcerto { get; set; }
        public string ClasseServico { get; set; }
        public string Filler { get; set; }
    }
}
