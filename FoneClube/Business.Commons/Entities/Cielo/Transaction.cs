using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.Cielo
{
    public class Transaction
    {
        public enum Tipo {
            NotFinished = 0,
            Authorized = 1,
            PaymentConfirmed = 2,
            Denied = 3,
            Cancelado = 10,
            Refunded = 11,
            Pending = 12,
            Aborted = 13,
            Scheduled = 20
        }

        public enum Gateway
        {
            Pagarme = 1,
            Cielo = 2,
            Loja = 3
        }

        public string Id { get; set; }
        public int? IdCustomer { get; set; }
        public Gateway TipoGateway { get; set; }


        //0	    NotFinished         ALL             Aguardando atualização de status
        //1	    Authorized          ALL             Pagamento apto a ser capturado ou definido como pago
        //2	    PaymentConfirmed    ALL             Pagamento confirmado e finalizado
        //3	    Denied              CC + CD + TF    Pagamento negado por Autorizador
        //10	Voided              ALL             Pagamento cancelado
        //11	Refunded            CC + CD         Pagamento cancelado após 23:59 do dia de autorização
        //12	Pending             ALL             Aguardando Status de instituição financeira
        //13	Aborted             ALL             Pagamento cancelado por falha no processamento ou por ação do AF
        //20	Scheduled           CC              Recorrência agendada
    }
}
