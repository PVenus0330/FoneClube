using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.charging
{

    public enum ChargingTarget
    {
        PrecoUnico = 1,
        Plano = 2
    }

    public class MassChargingList
    {
        public int Id { get; set; }
        public List<MassChargingModel> MassCharging { get; set; }

        public int Mes { get; set; }
        public int Ano { get; set; }
    }


    public class MassChargingModel
    {
        public string Name { get; set; }

        public string CPF { get; set; }
        public string Email { get; set; }
        public string ValorTotalCobranca { get; set; }
        public string PrecoUnico { get; set; }

        public Charging LastChargingPaid { get; set; }
        public Charging LastCharging { get; set; }
        public string NovoComentario { get; set; }

        public string ValorOperadoraTotalLinhas { get; set; }
        public List<PhoneModel> Phones { get; set; }
        public int IdPerson { get; set; }
        public int? IdPagarme { get; set; }

        public bool? GoodToCharge { get; set; }
        public string Reason { get; set; }
        public ChargingTarget FonteDoTotalCobrar { get; set; }

        public TransactionResult ResulTransaction { get; set; }
        public bool EmailEnviado { get; set; }
        public bool Charged { get; set; }
        public bool HasCard { get; set; }
        public Charging ChargeDoMes { get; set; }
    }

    public class PhoneModel
    {
        public int? Id { get; set; }
        public string DDD { get; set; }
        public string Number { get; set; }
        public int? IdPlanOption { get; set; }
        public string Ammount { get; set; }
        public int? AmmountVIP { get; set; }
        public double? OperatorAmount { get; set; }
        public string NickName { get; set; }
        public string Operator { get; set; }
        public int? OperatorId { get; set; }
    }

 
}
