using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Claro
{
    public class LinhaRegistro
    {

        public int Id { get; set; }
        public string DDD { get; set; }
        public string Telefone { get; set; }
        public string Secao { get; set; }
        public string Data { get; set; }
        public string Hora { get; set; }
        public string OrigemUFDestino { get; set; }
        public string Numero { get; set; }
        public string DuracaoQuantidade { get; set; }
        public string Tarifa { get; set; }
        public string Valor { get; set; }
        public string ValorCobrado { get; set; }
        public string Nome { get; set; }
        public string CC { get; set; }
        public string Matricula { get; set; }
        public string SubSecao { get; set; }
        public string TipoImposto { get; set; }
        public string Descricao { get; set; }
        public string Cargo { get; set; }
        public string NomeLocalOrigem { get; set; }
        public string NomeLocalDestino { get; set; }
        public string CodigoLocalOrigem { get; set; }
        public string CodigoLocalDestino { get; set; }

    }
}
