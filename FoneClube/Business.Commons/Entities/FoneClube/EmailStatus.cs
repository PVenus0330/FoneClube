using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class EmailStatus
    {
        public string email { get; set; }
        public string from { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public int TemplateType { get; set; }

        public string cc { get; set; }
        public string bcc { get; set; }
        public string TargetTextBlue { get; set; }
        public string TargetSecondaryText { get; set; }

        public enum TemplateTypes
        {
            None,
            CardCharged,
            BoletoCharged
        }
    }

    public class Attachments
    {
        public Stream FileStream { get; set; }
        public string Name { get; set; }
    }
}
