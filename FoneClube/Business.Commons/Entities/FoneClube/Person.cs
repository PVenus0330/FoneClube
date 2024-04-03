using FoneClube.Business.Commons.Entities.FoneClube;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace FoneClube.Business.Commons.Entities
{
    public class Person
    {

        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Register { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NickName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Born { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Gender { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdPlanOption { get; set; } //vai morrer

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdPagarme { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdRole { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdCurrentOperator { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Int64 SinglePrice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DescriptionSinglePrice { get; set; }

        //listas pertencentes a cliente
        //public List<int> Plans { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Adress> Adresses { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Phone> Phones { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Images { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Photo> Photos { get; set; }

        //Pai do cliente
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdParent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneDDDParent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumberParent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NameParent { get; set; } // por enquanto não usado, pois não precisa

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdCommissionLevel { get; set; }

        //entidade cobrança
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Charging Charging { get; set; } //todo remove

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Charging> ChargingValidity { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Charged { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ServiceOrder ServiceOrder { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Charging> ChargingHistory { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int TotalBoletoCharges { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<DateTime> LastChargeDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastPaidDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int LastPaidAmount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Benefit Benefit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SoftDelete { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Orphan { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Desativo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool HasSinglePrice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long TotalAmountCustomer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LastCharging LastCharge { get; set; }

        public Pai Pai { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SemPagamento { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DataUltimaCobranca { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ComissionDetails ComissionDetails { get; set; }
        public WhatsappClient WClient { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool PendingFlagInteraction { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Flag> Flags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CardFoneclube Card { get; set; }

        public ChargeAndServiceOrderHistory ChargeAndServiceOrderHistory { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AffiliateLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalAffiliateLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DDDPhone { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastChargePaid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DefaultPaymentDay { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DefaultVerificar { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultWAPhones { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SchduleCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastScheduleDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int WhatsAppStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WhatsAppStatusDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? NextActionDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NextActionText { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Use2Prices { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrecoPromoSum { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrecoFCSum { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsVIP { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLinhaAtiva { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsShowICCID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsShowPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LineStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VIPSum { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Referral { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IntlPhone IntlPhone { get; set; }

    }

    public class IntlPhone { 
        public string CountryCode { get; set; }
        public string Phone { get; set; }
    }

    public class PersonParentModel
    {
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string WhatsAppNumber { get; set; }
        public string CPF { get; set; }
    }

    public enum LineStatus
    {
        CONTEL = 1,
        CONTEL_BLOCKED = 2,
        NON_CONTEL = 3
    }

    public class BlockRequest
    {
        public int PersonId { get; set; }
    }
}
