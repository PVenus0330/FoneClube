using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Claro
{
    public class ListasRegistros
    {
        public List<string> Identificacao { get; set; }
        public List<string> ResumoNotasList { get; set; }

        public List<string> IdentificacaoDebitoAutomatico { get; set; }
        public List<string> DetalhesList { get; set; }

    }

    public enum EClaroContaRegions
    {
        Identificacao = 0,
        ResumoNotas = 1,
        DebitoAutomatico = 2,
        Detalhes = 3,
        Final = 4
    }

}
