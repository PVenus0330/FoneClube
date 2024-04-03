using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    
    public enum BankBilletStatus
    {

        /// <summary>
        /// N/A
        /// </summary>
        NotApplicable = 0,

        /// <summary>
        /// Gerando
        /// </summary>
        Generating = 1,

        /// <summary>
        /// Aberto
        /// </summary>
        Opened = 2,

        /// <summary>
        /// Cancelado
        /// </summary>
        Canceled = 3,

        /// <summary>
        /// Pago
        /// </summary>
        Paid = 4,

        /// <summary>
        ///  Vencido
        /// </summary>
        Overdue = 5,

        /// <summary>
        /// Bloqueado
        /// </summary>
        Blocked = 6,

        /// <summary>
        /// Estornado
        /// </summary>
        Chargeback = 7,

       


    }

}
