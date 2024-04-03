using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class PhoneService
    {
        public int Id { get; set; }
        public int? AmountOperadora { get; set; }
        public int? AmountFoneclube { get; set; }
        public string Descricao { get; set; }
        public bool? ExtraOption { get; set; }
        public bool? Assinatura { get; set; }
        public bool? Editavel { get; set; }
    }

    public class PhoneLines
    {
        public int Id { get; set; }
        public string TopUp { get; set; }
        public bool TopUpHistory { get; set; }
        public int IdPerson { get; set; }
        public string ContelBlockStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string ICCID { get; set; }
        public string Propriedade { get; set; }
        public string CPF_FC { get; set; }
        public string CPF_DR { get; set; }
        public string ClienteAtivo_FC { get; set; }
        public string Nome_FC { get; set; }
        public string Nome_DR { get; set; }
        public bool LinhaSemUso { get; set; }
        public string Ativa { get; set; }
        public string Linha_FC { get; set; }
        public string Linha_DR { get; set; }
        public string Total_DR { get; set; }
        public string PrecoUnico { get; set; }
        public string Total_FC { get; set; }
        public string Preco_FC { get; set; }
        public string PrecoVIP { get; set; }
        public string Plano_FC { get; set; }
        public string Plugin_DR { get; set; }
        public string Plano_DR { get; set; }
        public string Roaming { get; set; }
        public string Apelido { get; set; }
        public string Plano_Contel { get; set; }
        public string Saldo { get; set; }
        public bool? Recarga_Automatica { get; set; }
        public string Cancelation_Date { get; set; }
        public string PortNumber { get; set; }
        public string Ativacao { get; set; }
        public string InicioPlano { get; set; }
        public string Bloqueada { get; set; }
        public string Esim { get; set; }
        public string VIPSum { get; set; }
        public string FCSum { get; set; }
        public int UltPagDias { get; set; }
        public string StatusCob { get; set; }
        public bool? IsContelLine { get; set; }
        public string AutoRec { get; set; }
        public string FimPlano { get; set; }
        public string ContelStatus { get; set; }
        public string PortIn { get; set; }
        public string DocContel { get; set; }
        public string LastPaidAmount { get; set; }
        public string RecAutFC { get; set; }
        public string ValorPago { get; set; }
        public bool RecAutFCFlag { get; set; }
        public int DaysSinceLastTopup { get; set; }
        public bool Delete { get; set; }
        public int Agendado { get; set; }
        public DateTime? DteRegistered { get; set; }
    }
}
