using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Commons.Entities
{
    public class Endereco
    {
        public string ControleSequencial { get; set; }
        public string IDUnicoNRC { get; set; }
        public string DDD { get; set; }
        public string NumeroTelefone { get; set; }
        public string CaracteristicaRecurso { get; set; }
        public string CNLRecursoEnderecoPontaA { get; set; }
        public string NomeLocalidadePontaA { get; set; }
        public string UFLocalidadePontaA { get; set; }
        public string EnderecoPontaA { get; set; }
        public string NumeroEnderecoPontaA { get; set; }
        public string ComplementoPontaA { get; set; }
        public string BairroPontaA { get; set; }
        public string CNLRecursoEnderecoPontaB { get; set; }
        public string NomeLocalidadePontaB { get; set; }
        public string UFLocalidadePontaB { get; set; }
        public string EnderecoPontaB { get; set; }
        public string NumeroEnderecoPontaB { get; set; }
        public string ComplementoPontaB { get; set; }
        public string BairroPontaB { get; set; }
        public string Filler { get; set; }
    }
}
