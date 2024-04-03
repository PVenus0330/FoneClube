using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.ViewModel
{
    public class InstaRCVCModel
    {
        public InstaRegisterClientOrLineViewModel Register { get; set; }
        public InstaChargingModel ChargeData { get; set; }
    }

    public class InstaChargingModel
    {
        public string Comment { get; set; }
        public string DueDate { get; set; }
        public string MesVingencia { get; set; }
        public string AnoVingencia { get; set; }
        public string InstaRegsiterData { get; set; }
        public string Ammount { get; set; }
        public int PaymentType { get; set; }
        public string CardId { get; set; }
    }
}
