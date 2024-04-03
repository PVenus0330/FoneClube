using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities
{
    public class TokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class ValidateTokenRequest
    {
        public string Token { get; set; }
    }

    public class ValidateApiKeyRequest
    {
        public string ApiKey { get; set; }
    }

    public class TokenResponse
    {
        public bool Status { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }

    public class ValidateTokenResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
    }
    public enum EnvironmentType
    {
        PROD = 0,
        DEV = 1
    }
    public class FacilPhone
    {
        public int CountryCode { get; set; }
        public string PhoneNumber { get; set; }

    }
    public class FacilGenericRequest
    {
        public string ApiKey { get; set; }
        public FacilPhone Phone { get; set; }
        public EnvironmentType Enviroment { get; set; }
    }

    public class FacilGenericResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
    }

    public class FacilPlanResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public List<FacilPlanRes> Plans { get; set; }
    }

    public class FacilPlanRes
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public string Price { get; set; }

    }

    public class FacilICCIDRequest
    {
        public string ApiKey { get; set; }
        public string ICCID { get; set; }
    }

    public class FacilICCIDResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public FacilICCIDRes Data { get; set; }
    }

    public class FacilICCIDRes
    {
        public string ActivationDate { get; set; }
        public string Client { get; set; }
        public string Plan { get; set; }
        public string ICCID { get; set; }
        public string PhoneNumber { get; set; }
        public string PortedNumber { get; set; }
        public string ActivationPDFLink { get; set; }
        public string ActivationCode { get; set; }
    }

    public class GetPhoneDetailResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public GetPhoneDetailRes Data { get; set; }
    }

    public class GetAllPhoneDetailResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
        public List<GetPhoneDetailRes> Data { get; set; }
    }
    public class GetPhoneDetailRes
    {
        public string Id { get; set; }
        public string LineId { get; set; }
        public string Line { get; set; }
        public string NickName { get; set; }
        public string ICCID { get; set; }
        public string Owner { get; set; }
        public string OwnerNickName { get; set; }
        public string NameIdentifier { get; set; }
        public string Emoji { get; set; }
        public string ActivationDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string RenewalDate { get; set; }
        public string OperatorDeleteDate { get; set; }
        public string Plan { get; set; }
        public string PlanType { get; set; }
        public string OwnerUniqueId { get; set; }
        public string LineStatus { get; set; }
        public string LineCancellationReason { get; set; }
        public string LineCancellationDate { get; set; }
        public string RecurrenceCard { get; set; }
        public bool PortIn { get; set; }
        public bool Blocked { get; set; }
        public bool eSim { get; set; }
        public string BlockedDate { get; set; }
        public string AutomaticRecharge { get; set; }
        public string AutomaticRechargePlan { get; set; }
        public string PortabilityDonarLine { get; set; }
        public string PortabilityStatus { get; set; }
        public string PortabilityRegistrationDate { get; set; }
        public string PortabilityScheduleDate { get; set; }
        public string PortabilityAcceptedDate { get; set; }
        public string PortabilityCompletedDate { get; set; }
        public string ActivationPDFLink { get; set; }
        public string ActivationCode { get; set; }
    }

    public class FacilActivateESIMRequest
    {
        public string ApiKey { get; set; }
        public EnvironmentType Enviroment { get; set; }
        public FacilActivateESIMReq ActivationInfo { get; set; }
    }

    public class FacilActivateESIMResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
        public ActivatedInfo ActivatedInfo { get; set; }

    }

    public class ResettedInfo
    {
        public string ResettedPhone { get; set; }
        public string ResettedICCID { get; set; }
        public int? RequiredTopup { get; set; }
    }

    public class ActivatedInfo
    {
        public string ActivationCode { get; set; }
        public string ActivationQrCode { get; set; }
        public string ActivationPDFLink { get; set; }
        public string ActivatedNumber { get; set; }
        public string ActivatedPlan { get; set; }
        public string ICCID { get; set; }
        public string ActivationDate { get; set; }
    }

    public class FacilActivateESIMReq
    {
        public FacilCustomerReq CustomerInfo { get; set; }
        public FacilLineActivationReq LineInfo { get; set; }
    }

    public class FacilLineActivationReq
    {
        public int DDD { get; set; }
        public int PlanId { get; set; }
    }

    public class FacilCustomerReq
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class FacilTopupHistoryResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public List<FacilTopupHistoryRes> History { get; set; }
    }

    public class FacilLineBalanceResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public FacilLineBalanceRes Data { get; set; }
    }

    public class FacilLineBalanceRes
    {
        public double Data { get; set; }
        public double SMS { get; set; }
        public double Minutes { get; set; }
    }

    public class FacilTopupHistoryRes
    {
        public string RechargeDate { get; set; }
        public string Plan { get; set; }
    }

    public class FacilTopupReq
    {
        public string PhoneNumber { get; set; }
        public int PlanId { get; set; }
    }

    public class FacilTopupRequest
    {
        public string ApiKey { get; set; }
        public EnvironmentType Enviroment { get; set; }
        public FacilTopupReq TopupInfo { get; set; }
    }

    public class FacilTopupResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }

        public FacilTopupRes Data { get; set; }
    }

    public class FacilTopupRes
    {
        public string DateRecharged { get; set; }
        public string Plan { get; set; }
    }

    public class FacilBalanceResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public Decimal Balance { get; set; }
    }

    public class FacilDebitResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public List<FacilDebitRes> Data { get; set; }
    }

    public class FacilDebitRes
    {
        public decimal Amount { get; set; }
        public string PurchaseType { get; set; }
        public DateTime DateDebited { get; set; }
        public string Phone { get; set; }
        public string Plan { get; set; }
    }


    public class FacilUpdateBalanceRequest
    {
        public int IdPerson { get; set; }
        public string USDAmount { get; set; }
        public string BRAmount { get; set; }
        public string CCCharages { get; set; }
        public string BankCharges { get; set; }
        public string HandlingCharges { get; set; }
        public string ConversionRate { get; set; }
        public string PaymentType { get; set; }
        public string DateAdded { get; set; }
        public string FinalValue { get; set; }
        public string Comment { get; set; }
        public bool Refund { get; set; }
    }

    public class FacilResetLineRequest
    {
        public string ApiKey { get; set; }
        public EnvironmentType Enviroment { get; set; }
        public FacilResetLineReq LineInfo { get; set; }
    }
    public class FacilResetLineReq
    {
        public FacilPhone Phone { get; set; }
        public string NewICCID { get; set; }
        public string Reason { get; set; }
    }

    public class FacilResetLineResponse
    {
        public bool Status { get; set; }
        public string ICCID { get; set; }
        public string ActivationPDF { get; set; }
        public string ActivationCode { get; set; }
        public string Error { get; set; }
    }

    public class FacilHistoryResponse
    {
        public bool Status { get; set; }
        public string Error { get; set; }
        public string Saldo { get; set; }
        public List<FacilHistoryRes> History { get; set; }
    }

    public class FacilHistoryRes
    {
        public string Category { get; set; }
        public decimal AmountDeducted { get; set; }
        public DateTime DeductedDate { get; set; }
        public string Plan { get; set; }
        public string Phone { get; set; }
    }
}
