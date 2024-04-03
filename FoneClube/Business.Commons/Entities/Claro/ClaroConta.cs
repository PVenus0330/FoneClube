using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Claro
{
    public class ClaroConta
    {
        public int Id { get; set; }

        public string IdUnico { get; set; }


        public string IdCliente { get; set; }

        public string Nome { get; set; }

        public string Endereco { get; set; }
        
        public string AnoMesCompetencia { get; set; }

        public DateTime DataCadastro { get; set; }

        public string NumeroCliente { get; set; }

        public string DataVencimento { get; set; }

        public string DataReferenciaInicio { get; set; }

        public string DataReferenciaFim { get; set; }

        public string Valor { get; set; }

        public string IdReferenciaDebitoAutomatico { get; set; }

        public List<RegistroNotaFiscal> Notas { get; set; }

        public List<LinhaRegistro> LinhasRegistro { get; set; }
    }
}
