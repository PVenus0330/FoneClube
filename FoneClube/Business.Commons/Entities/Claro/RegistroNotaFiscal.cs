using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Claro
{
    public class RegistroNotaFiscal
    {
        public int Id { get; set; }

        public string NotaFiscal { get; set; }

        public string Codigo { get; set; }
                
        public string Tipo { get; set; }

        public string Aliquota { get; set; }

        public string BaseCalculo { get; set; }

        public string ValorImposto { get; set; }


    }
}
