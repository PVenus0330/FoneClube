using Business.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess
{
    public class ManagerOperator
    {
        #region TODO Remove to tables
        public string ClaroMail = "marcio.franco@gmail.com";
        //public string ClaroMail = "rodrigocardozop@gmail.com";
        public string VivoMail = "marcio.franco@gmail.com";
        //public string VivoMail = "rodrigocardozop@gmail.com";

        public string DeveloperMail = "rodrigocardozop@gmail.com";
        public string GestorMail = "rodrigocardozop@gmail.com";
        //public string GestorMail = "marcio.franco@gmail.com";

        public string TituloBloqueioRouboClaro = "Claro: Perda/Roubo da linha";
        public string TituloAlteracaoClaro = "Claro: Alteração/Adição pacote de Dados";

        public string BloqueioPerdaRouboMensagemClaro = "Segue solicitações de alterações e as linhas de referencia para cada pacote:\nEmpresa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\nNº do Cliente: 939144068\nNº da Conta: 100387217\n\nMudanças: Bloqueio por perda/roubo da(s) seguinte(s) linha:{0}\n\n\n\nProtocolo: AAAAMMDDHHMM.CLARO.XXX\nAo responder a esta solicitação pedimos encarecidamente que inclua o numero do protocolo acima na resposta.\n\nAt\nMarcio Guiamaraes Franco\nGestor";
        public string AlteracaoMensagemClaro = "Segue solicitações de alterações e as linhas de referencia para cada pacote:\nEmpresa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\nNº do Cliente: 939144068\nNº da Conta: 100387217\n\nMudanças:\n\nLinha Referencia:\n{0}\n{1}\nAt\nMarcio Guiamaraes Franco\nGestor";


        public string TituloBloqueioVivo = "Vivo: Dados - Bloqueio";
        public string TituloBloqueioRouboVivo = "Vivo: Suspensão por Perda/Roubo";
        public string TituloAtivacaoVivo = "Vivo: Ativar Linha+Dados";
        public string TituloDesbloqueioVivo = "Vivo: Dados - Desbloqueio";

        public string BloqueiLinhaMensagemVivo = "Emprsa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\n\nProtocolo: AAAAMMDDHHMMSS.Vivo.XXX\nAo responder a esta solicitação pedimos que inclua o numero do protocolo acima na resposta.\n\nGostaria de solicitar o bloqueio de dados da(s) seguinte(s) linha(s):\n\n{0}\n\nCaso alguma linha ja esteja suspensa, favor manter a suspensão e nos informar a data que foi efetivada o bloqueio.\n\nAt\nMarcio Guiamaraes Franco\nGestor";
        public string BloqueiRouboLinhaMensagemVivo = "Emprsa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\n\nProtocolo: AAAAMMDDHHMMSS.Vivo.XXX\nAo responder a esta solicitação pedimos que inclua o numero do protocolo acima na resposta.\n\nGostaria de solicitar o bloqueio por perda/roubo temporario da(s) seguinte(s) linha(s):\n\n{0}\n\nCaso alguma linha ja esteja suspensa, favor manter a suspensão e nos informar a data que foi efetivada o bloqueio.\n\nAt\nMarcio Guiamaraes Franco\nGestor";
        public string DesbloqueioLinhaMensagemVivo = "Empresa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\n\nProtocolo: AAAAMMDDHHMMSS.Vivo.XXX\nAo responder a esta solicitação pedimos que inclua o numero do protocolo acima na resposta.\n\nGostaria de solicitar o desbloqueio de dados  da(s) seguinte(s) linha(s):\n\n{0}\n\nCaso alguma linha ja esteja suspensa, favor ativar a linha e ativar dados tambem.\n\nAt\nMarcio Guiamaraes Franco\nGestor";
        public string AtivacaoLinhaMensagemVivo = "Emprsa: Freenetcom Soluções em TI EIRELI\nCNPJ: 08.453.543/0001-76\n\nProtocolo: YYMMDDHHMM.Vivo.XXX\nAo responder a esta solicitação pedimos que inclua o numero do protocolo acima na resposta.\n\nGostaria de solicitar a retirada do bloqueio temporário por perda/roubo e ativação de dados  da(s) seguinte(s) linha(s):\n\n{0}\n\n\nAt\nMarcio Guiamaraes Franco\nGestor";

        public string MensagemErro = "Essa linha não possui usuário associado para receber notificação que será alterada {0}";
        #endregion

        public bool EmailBlockClaro(string ddd, string lineNumber)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloBloqueioRouboClaro, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(ClaroMail, TituloBloqueioRouboClaro, string.Format(BloqueioPerdaRouboMensagemClaro, ddd + lineNumber));
        }

        public bool EmailUpdateClaro(string ddd, string lineNumber, string corpo)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloAlteracaoClaro, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(ClaroMail, TituloAlteracaoClaro, string.Format(AlteracaoMensagemClaro, ddd + lineNumber, corpo));
        }

      

        public bool EmailBlockVivo(string ddd, string lineNumber)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloBloqueioVivo, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(VivoMail, TituloBloqueioVivo, string.Format(BloqueiLinhaMensagemVivo, ddd + lineNumber));
        }

        public bool EmailBlockRouboVivo(string ddd, string lineNumber)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloBloqueioRouboVivo, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(VivoMail, TituloBloqueioRouboVivo, string.Format(BloqueiRouboLinhaMensagemVivo, ddd + lineNumber));
        }

        public bool EmailAtivacaoVivo(string ddd, string lineNumber)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloAtivacaoVivo, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(VivoMail, TituloAtivacaoVivo, string.Format(AtivacaoLinhaMensagemVivo, ddd + lineNumber));
        }

        public bool EmailDesbloqueioVivo(string ddd, string lineNumber)
        {
            var person = new ProfileAccess().GetEmailFromLineNumber(Convert.ToInt32(ddd), Convert.ToInt32(lineNumber));

            if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                DispatchErrorMails(TituloDesbloqueioVivo, ddd + lineNumber);
                return false;
            }

            return new Utils().SendEmail(VivoMail, TituloDesbloqueioVivo, string.Format(DesbloqueioLinhaMensagemVivo, ddd + lineNumber));
        }

        public void DispatchErrorMails(string titulo, string telefone)
        {
            new Utils().SendEmail(GestorMail, titulo + " - Não enviado", string.Format(MensagemErro, telefone));
            new Utils().SendEmail(DeveloperMail, titulo + " - Não enviado", string.Format(MensagemErro, telefone));
        }
    }
}
