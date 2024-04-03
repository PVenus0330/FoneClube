using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib.Email
{
    public class EmailUtil
    {
        public bool SendEmail(string to, string title, string body, bool bodyHtml = false)
        {
            try
            {
                var email = "financeiro@foneclube.com.br";
                var password = "!@financeirofoneclube!@1";

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.zoho.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                MailMessage mm = new MailMessage(email, to, title, body);
                mm.IsBodyHtml = bodyHtml;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }
    }
}
