using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Charging
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdCollector { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CollectorName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PaymentType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CommentBoleto { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CommentEmail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreationDateFormatted { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Ammount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long BoletoId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BoletoLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AcquireId { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreateDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ExpireDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ClientId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PhoneId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PaymentStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionComment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? TransactionId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ChargingDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Phone> Phones { get;set;}

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ChargingComment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MesVingencia { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AnoVingencia { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Charged { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Payd { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Expired { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StatusDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ChargeStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ComissionConceded { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? CacheTransaction { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentStatusDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? BoletoExpires { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BoletoCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedCharging { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreationDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Canceled { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionLastUpdate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? GatewayId { get; set; }

        public string CartHash { get; set; }
        public int IdLoja { get; set; }

        public bool? Pago { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DiasSemCobrar { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Frete { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Plan> Planos { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PixCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool SendEmail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Scheduled { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ScheduledMonth { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ScheduledYear { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ScheduledDay { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentStatusType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DueDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SchedledReminderDays { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TxtWAPhones { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendMarketing1 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendMarketing2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool SendWAText { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BoletoBarcode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BoletoUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DefaultPaymentDay { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int VerficarDay { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Installments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InstaRegsiterData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] MutliVigencias { get; set; }
    }


}
