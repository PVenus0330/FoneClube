using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Bilhetacao
    {
        public string ControleSequencial { get; set; }
        public string Vencimento { get; set; }
        public string Emissao { get; set; }
        public string IDUnicoNRC { get; set; }
        public string RecursoCNL { get; set; }
        public string DDD { get; set; }
        public string NumeroTelefone { get; set; }
        public string CaracteristicaRecurso { get; set; }
        public string DegrauRecurso { get; set; }
        public string DataLigacao { get; set; }
        public string CNLLocalidadeChamada { get; set; }
        public string NomeLocalidadeChamada { get; set; }
        public string UFTelefoneChamado { get; set; }
        public string CODNacionalInternacional { get; set; }
        public string CODOperadora { get; set; }
        public string DescricaoOperadora { get; set; }
        public string CODPais { get; set; }
        public string AreaDDD { get; set; }
        public string NumeroTelefoneChamado { get; set; }
        public string ConjugadoNumeroTelefoneChamado { get; set; }
        public string DuracaoLigacao { get; set; }
        public string Categoria { get; set; }
        public string DescricaoCategoria { get; set; }
        public string HorarioLigacao { get; set; }
        public string TipoChamada { get; set; }
        public string GrupoHorarioTarifario { get; set; }
        public string DescricaoHorarioTarifario { get; set; }
        public string DegrauLigacao { get; set; }
        public string SinalValorLigacao { get; set; }
        public string AliquotaICMS { get; set; }
        public string ValorLigacaoComImposto { get; set; }
        public string ClasseServico { get; set; }
        public string Filler { get; set; }
    }
}
