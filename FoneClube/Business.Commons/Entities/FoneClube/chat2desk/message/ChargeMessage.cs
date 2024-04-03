using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.message
{
    public class ChargeMessage
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentDate { get; set; }

        public string AmountTemp { get; set; }
        public string ValorTotalLiberadoParaPagarCliente { get; set; }
        public string AmountTemp1 { get; set; }
        public string CustomerComment { get; set; }
        public string CommentBoleto { get; set; }
        public string Comment { get; set; }
    }
}
