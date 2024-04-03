using FoneClube.Business.Commons.Entities.Claro;
using FoneClube.Business.Commons.Entities.Vivo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Phone
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DDD { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Number { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFoneclube { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int IdOperator { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Portability { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NickName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IdPlanOption { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Inative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Delete { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? LinhaAtiva { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? PreviousStatusLinhaAtiva { get; set; }

        //Charging 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Ammount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool OperatorBlockedLineStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OperatorStatusDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object PlanDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ResultValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int EventTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrecoVipStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? AmmountPrecoVip { get; set; }

        //fazer polimorfismo aqui nesses dois a baixo
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ClaroLineInfo StatusClaro { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public VivoLineInfo StatusVivo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ChargeAmount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ResultAmount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PhoneService> Servicos { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OperatorDescription { get; set; }

        public int OperatorChargedPrice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Owner { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerDocument { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Flag> Flags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DataEntrada { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CountryCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PluginDR { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PlanoDR { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool PlanoDivergente { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UsoLinha { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ICCID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PortNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PortDDD { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsContelLine { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PlanoContel { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool ESim { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FimPlano { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AutoContel { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AutoRecFC { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ContelStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Cancela { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Activate { get; set; }

        public enum PhoneEvents
        {
            AtivarLinha = 1,
            DesativarLinha = 2,
            DesativarLinhaTemp = 3,
            DesligarLinha = 4,
            LigarLinha = 5,
            AssociarLinha = 6,
            DesassociarLinha = 7,
            AtualizarPlano = 8
        }

        public enum PhoneStatus
        {
            SemStatus = 0,
            Ativa = 1,
            Desativa = 2,
            AguardandoAtivacao = 3,
            AguardandoDesativacao = 4,
            AguardandoDesativacaoTemporaria = 5,
            DesativaTemporariamente = 6,
            Desligada = 7,
            Ligada = 8
        }
    }
}
