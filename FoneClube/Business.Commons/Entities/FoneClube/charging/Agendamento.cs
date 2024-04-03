using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.charging
{
    public class Agendamento
    {
        public int Id { get; set; }
        public DateTime DataExecucao { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime Vingencia { get; set; }
        public DateTime Vencimento { get; set; }
        public bool Executado { get; set; }
        public string Tipo { get; set; }
        public string ValorCobrado { get; set; }
        public string CommentEmail { get; set; }
        public string AdditionalComment { get; set; }
    }

    public class UpdateAgendamento
    {
        public int Id { get; set; }
        public string DataExecucao { get; set; }
        public string DataAgendamento { get; set; }
        public string Vingencia { get; set; }
        public string Vencimento { get; set; }
        public bool Executado { get; set; }
        public string Tipo { get; set; }
        public string ValorCobrado { get; set; }
        public string CommentEmail { get; set; }
        public string AdditionalComment { get; set; }
    }
}
