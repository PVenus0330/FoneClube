using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.email
{
    public class SetupEmail
    {
        public static int CLARO_FINANCEIRO = 0;
        public static int TIM_FINANCEIRO = 1;

        public SmtpClient Client { get; set; }
        public MailAddress MailAddress { get; set; }
    }
}
