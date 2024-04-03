using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.estoque
{
    public class Estoque
    {
        public int IdLinha { get; set; }
        public string linhaLivreOperadora { get; set; }
        public Nullable<int> operadora { get; set; }
        public string descricao { get; set; }
        public string propriedadeInterna { get; set; }
        public int propriedadeInternaId { get; set; }

    }
}
