using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.estoque
{
    public class Estoque
    {
        public string linhaLivreOperadora { get; set; }
        public Nullable<int> operadora { get; set; }
        public string descricao { get; set; }

    }
}
