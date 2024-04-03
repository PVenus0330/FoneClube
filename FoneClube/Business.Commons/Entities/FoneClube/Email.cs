using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Email
    {
        public string Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int TemplateType { get; set; }

        public string TargetName { get; set; }
        public string TargetTextBlue { get; set; }
        public string TargetSecondaryText { get; set; }
        public string DiscountPrice { get; set; }
        public string TargetTextComment { get; set; }

        public enum TemplateTypes
        {
            None,
            CardCharged,
            BoletoCharged,
            Debito = 22,
            Pix = 30,
        }
    }
}
