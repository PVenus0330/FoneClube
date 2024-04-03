using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Linq;


namespace FoneClube.DataAccess
{
    
    public partial class FoneClubeContext : DbContext
    {
        public FoneClubeContext()
            : base("name=FoneClubeContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tblCommissions> tblCommissions { get; set; }
        public virtual DbSet<tblLog> tblLog { get; set; }
        public virtual DbSet<tblLogTipo> tblLogTipo { get; set; }
        public virtual DbSet<tblOperadoras> tblOperadoras { get; set; }
        public virtual DbSet<tblPaymentTypes> tblPaymentTypes { get; set; }
        public virtual DbSet<tblPlanDetails> tblPlanDetails { get; set; }
        public virtual DbSet<tblPlanDetailType> tblPlanDetailType { get; set; }
        public virtual DbSet<tblPlans> tblPlans { get; set; }
        public virtual DbSet<tblReferred> tblReferred { get; set; }
        public virtual DbSet<tblBilhetacoesFebraban> tblBilhetacoesFebraban { get; set; }
        public virtual DbSet<tblContasFebraban> tblContasFebraban { get; set; }
        public virtual DbSet<tblDescontosFebraban> tblDescontosFebraban { get; set; }
        public virtual DbSet<tblEnderecosFebraban> tblEnderecosFebraban { get; set; }
        public virtual DbSet<tblHeadsFebraban> tblHeadsFebraban { get; set; }
        public virtual DbSet<tblResumosFebraban> tblResumosFebraban { get; set; }
        public virtual DbSet<tblServicosFebraban> tblServicosFebraban { get; set; }
        public virtual DbSet<tblTotalizadoresFebraban> tblTotalizadoresFebraban { get; set; }
        public virtual DbSet<tblDiscountPrice> tblDiscountPrice { get; set; }
        public virtual DbSet<tblPersonsImages> tblPersonsImages { get; set; }
        public virtual DbSet<tblLineRecord> tblLineRecord { get; set; }
        public virtual DbSet<tblPersonsParents> tblPersonsParents { get; set; }
        public virtual DbSet<tblServiceOrders> tblServiceOrders { get; set; }
        public virtual DbSet<CobFullClaro_Sum> CobFullClaro_Sum { get; set; }
        public virtual DbSet<tblHistoryEvents> tblHistoryEvents { get; set; }
        public virtual DbSet<tblPhoneStatus> tblPhoneStatus { get; set; }
        public virtual DbSet<tblPhones> tblPhones { get; set; }
        public virtual DbSet<tblFoneclubePagarmeTransactions> tblFoneclubePagarmeTransactions { get; set; }
        public virtual DbSet<tblCheckoutPagarMeLog> tblCheckoutPagarMeLog { get; set; }
        public virtual DbSet<tblCommisionLevels> tblCommisionLevels { get; set; }
        public virtual DbSet<tblComissionTokens> tblComissionTokens { get; set; }
        public virtual DbSet<tblBonusOptions> tblBonusOptions { get; set; }
        public virtual DbSet<tblEmailTemplates> tblEmailTemplates { get; set; }
        public virtual DbSet<tblChargingLog> tblChargingLog { get; set; }
        public virtual DbSet<tblPhoneFlags> tblPhoneFlags { get; set; }
        public virtual DbSet<tblPhonePropertyHistory> tblPhonePropertyHistory { get; set; }
        public virtual DbSet<tblPersonAssociationHistory> tblPersonAssociationHistory { get; set; }
        public virtual DbSet<CobFullVivo_Sum> CobFullVivo_Sum { get; set; }
        public virtual DbSet<tblPastPagarmeUpdate> tblPastPagarmeUpdate { get; set; }
        public virtual DbSet<tblLogBackupCharging> tblLogBackupCharging { get; set; }
        public virtual DbSet<tblLogComissionOrder> tblLogComissionOrder { get; set; }
        public virtual DbSet<tblBonusOrderLog> tblBonusOrderLog { get; set; }
        public virtual DbSet<tblBonusOrderException> tblBonusOrderException { get; set; }
        public virtual DbSet<tblCommissionOrders> tblCommissionOrders { get; set; }
        public virtual DbSet<tblComissionValidationLog> tblComissionValidationLog { get; set; }
        public virtual DbSet<tblCieloPaymentLog> tblCieloPaymentLog { get; set; }
        public virtual DbSet<tblEmailLog> tblEmailLog { get; set; }
        public virtual DbSet<tblPhonesServices> tblPhonesServices { get; set; }
        public virtual DbSet<tblServices> tblServices { get; set; }
        public virtual DbSet<tblBonusOrder> tblBonusOrder { get; set; }
        public virtual DbSet<tblCieloPaymentStatus> tblCieloPaymentStatus { get; set; }
        public virtual DbSet<tblComissionOrder> tblComissionOrder { get; set; }
        public virtual DbSet<tblWhatsappMessages> tblWhatsappMessages { get; set; }
        public virtual DbSet<tblGenericPhoneFlags> tblGenericPhoneFlags { get; set; }
        public virtual DbSet<tblGenericFlags> tblGenericFlags { get; set; }
        public virtual DbSet<CobFullClaroMax_Sum> CobFullClaroMax_Sum { get; set; }
        public virtual DbSet<tblTransactionsCielo> tblTransactionsCielo { get; set; }
        public virtual DbSet<tblMassChargingLog> tblMassChargingLog { get; set; }
        public virtual DbSet<tblPhoneStock> tblPhoneStock { get; set; }
        public virtual DbSet<tblRegisterCustomersLog> tblRegisterCustomersLog { get; set; }
        public virtual DbSet<tblPersosAffiliateLinks> tblPersosAffiliateLinks { get; set; }
        public virtual DbSet<tblPersonsAddresses> tblPersonsAddresses { get; set; }
        public virtual DbSet<tblPhonesStockProperty> tblPhonesStockProperty { get; set; }
        public virtual DbSet<tblPlansOptions> tblPlansOptions { get; set; }
        public virtual DbSet<tblRenovaSenha> tblRenovaSenha { get; set; }
        public virtual DbSet<tblPersonsPhones> tblPersonsPhones { get; set; }
        public virtual DbSet<tblDescontoOrder> tblDescontoOrder { get; set; }
        public virtual DbSet<tblPersons> tblPersons { get; set; }
        public virtual DbSet<tblChargingHistory> tblChargingHistory { get; set; }
        public virtual DbSet<tblFoneclubePagarmeTransactionsSecond> tblFoneclubePagarmeTransactionsSecond { get; set; }
        public virtual DbSet<tblChargingScheduled> tblChargingScheduled { get; set; }
        public virtual DbSet<tblWhatsAppMessageTemplates> tblWhatsAppMessageTemplates { get; set; }
        public virtual DbSet<tblPagarmeTransactionsUserUpdateStatus> tblPagarmeTransactionsUserUpdateStatus { get; set; }
        public virtual DbSet<tblDrCelularTemp> tblDrCelularTemp { get; set; }
        public virtual DbSet<tblWhatsAppStatus> tblWhatsAppStatus { get; set; }
        public virtual DbSet<tblWhatsAppFailedStatus> tblWhatsAppFailedStatus { get; set; }
        public virtual DbSet<tblUserSettings> tblUserSettings { get; set; }
        public virtual DbSet<tblWhatsAppBotUserData> tblWhatsAppBotUserData { get; set; }
        public virtual DbSet<tblPhonesPendingToPort> tblPhonesPendingToPort { get; set; }
        public virtual DbSet<tblPagarmeWebhookStatusLog> tblPagarmeWebhookStatusLog { get; set; }
        public virtual DbSet<tblWhatsAppLog> tblWhatsAppLog { get; set; }
        public virtual DbSet<tblContelLinhasList> tblContelLinhasList { get; set; }
        public virtual DbSet<tblAgGridState> tblAgGridState { get; set; }
        public virtual DbSet<tblConfigSettings> tblConfigSettings { get; set; }
        public virtual DbSet<tblWebfcTopupAtContel> tblWebfcTopupAtContel { get; set; }
        public virtual DbSet<tblStoreCustomerPlans> tblStoreCustomerPlans { get; set; }
        public virtual DbSet<tblSaldoReminders> tblSaldoReminders { get; set; }
        public virtual DbSet<tblContelTopupHistory> tblContelTopupHistory { get; set; }
        public virtual DbSet<tblContelBlockedStatus> tblContelBlockedStatus { get; set; }
        public virtual DbSet<tblUnPlacedCartItems> tblUnPlacedCartItems { get; set; }
        public virtual DbSet<tblStoreOrders> tblStoreOrders { get; set; }
        public virtual DbSet<tblCCRefusedLog> tblCCRefusedLog { get; set; }
        public virtual DbSet<tblContelTopupHistoryExcel> tblContelTopupHistoryExcel { get; set; }
        public virtual DbSet<tblApiTokenInfo> tblApiTokenInfo { get; set; }
        public virtual DbSet<tblExternalLog> tblExternalLog { get; set; }
        public virtual DbSet<tblContelPlanMapping> tblContelPlanMapping { get; set; }
        public virtual DbSet<tblInternationActivationPool> tblInternationActivationPool { get; set; }
        public virtual DbSet<tblInternationalDeposits> tblInternationalDeposits { get; set; }
        public virtual DbSet<tblInternationalUserBalance> tblInternationalUserBalance { get; set; }
        public virtual DbSet<tblInternationalUserPurchases> tblInternationalUserPurchases { get; set; }
        public virtual DbSet<tbleSimActivationInfo> tbleSimActivationInfo { get; set; }
        public virtual DbSet<tblIntlFailedActivations> tblIntlFailedActivations { get; set; }
        public virtual DbSet<tblInternationActivationPoolLog> tblInternationActivationPoolLog { get; set; }
        public virtual DbSet<tblESimICCIDPool> tblESimICCIDPool { get; set; }
        public virtual DbSet<tblLinesForTopupPool> tblLinesForTopupPool { get; set; }
        public virtual DbSet<tblShopifySalesInfo> tblShopifySalesInfo { get; set; }
        public virtual DbSet<tblPhonesReadyToReset> tblPhonesReadyToReset { get; set; }
    
        public virtual ObjectResult<GetTransacoesAptasComissao_Result> GetTransacoesAptasComissao()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTransacoesAptasComissao_Result>("GetTransacoesAptasComissao");
        }
    
        public virtual ObjectResult<GetFilhos_Result> GetFilhos(Nullable<int> intIdPai)
        {
            var intIdPaiParameter = intIdPai.HasValue ?
                new ObjectParameter("intIdPai", intIdPai) :
                new ObjectParameter("intIdPai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFilhos_Result>("GetFilhos", intIdPaiParameter);
        }
    
        public virtual ObjectResult<GetMutiplosFilhos_Result> GetMutiplosFilhos(string pais)
        {
            var paisParameter = pais != null ?
                new ObjectParameter("pais", pais) :
                new ObjectParameter("pais", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMutiplosFilhos_Result>("GetMutiplosFilhos", paisParameter);
        }
    
        public virtual ObjectResult<GetFilhosHierarquia_Result> GetFilhosHierarquia(Nullable<int> intIdPai)
        {
            var intIdPaiParameter = intIdPai.HasValue ?
                new ObjectParameter("intIdPai", intIdPai) :
                new ObjectParameter("intIdPai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFilhosHierarquia_Result>("GetFilhosHierarquia", intIdPaiParameter);
        }
    
        public virtual ObjectResult<GetFilhosHierarquiaNaoPagante_Result> GetFilhosHierarquiaNaoPagante(Nullable<int> intIdPai)
        {
            var intIdPaiParameter = intIdPai.HasValue ?
                new ObjectParameter("intIdPai", intIdPai) :
                new ObjectParameter("intIdPai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFilhosHierarquiaNaoPagante_Result>("GetFilhosHierarquiaNaoPagante", intIdPaiParameter);
        }
    
        public virtual ObjectResult<GetFilhosHierarquiaPagante_Result> GetFilhosHierarquiaPagante(Nullable<int> intIdPai)
        {
            var intIdPaiParameter = intIdPai.HasValue ?
                new ObjectParameter("intIdPai", intIdPai) :
                new ObjectParameter("intIdPai", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFilhosHierarquiaPagante_Result>("GetFilhosHierarquiaPagante", intIdPaiParameter);
        }
    
        public virtual ObjectResult<CobFullClaro_Extract_Result> CobFullClaro_Extract(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<CobFullClaro_Extract_Result>("CobFullClaro_Extract", mesParameter, anoParameter);
        }
    
        public virtual int CobFullClaro_Load(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CobFullClaro_Load", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<CobFullVivo_Extract_Result> CobFullVivo_Extract(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<CobFullVivo_Extract_Result>("CobFullVivo_Extract", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> sp_DeletePerson(Nullable<int> idPerson)
        {
            var idPersonParameter = idPerson.HasValue ?
                new ObjectParameter("idPerson", idPerson) :
                new ObjectParameter("idPerson", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("sp_DeletePerson", idPersonParameter);
        }
    
        public virtual ObjectResult<GetComissoesCustomer_Result> GetComissoesCustomer(Nullable<int> intCustomerReceiver)
        {
            var intCustomerReceiverParameter = intCustomerReceiver.HasValue ?
                new ObjectParameter("intCustomerReceiver", intCustomerReceiver) :
                new ObjectParameter("intCustomerReceiver", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetComissoesCustomer_Result>("GetComissoesCustomer", intCustomerReceiverParameter);
        }
    
        public virtual ObjectResult<GetFilhosSemPai_Result> GetFilhosSemPai()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFilhosSemPai_Result>("GetFilhosSemPai");
        }
    
        public virtual ObjectResult<GetLinhasStatusUso_Result> GetLinhasStatusUso()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLinhasStatusUso_Result>("GetLinhasStatusUso");
        }
    
        public virtual int UpdatePhoneFlags()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdatePhoneFlags");
        }
    
        public virtual ObjectResult<GetLinhasFoneclubeEstoque_Result> GetLinhasFoneclubeEstoque()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLinhasFoneclubeEstoque_Result>("GetLinhasFoneclubeEstoque");
        }
    
        public virtual ObjectResult<GetLastChargingHistory_Result> GetLastChargingHistory(Nullable<int> intIdPerson)
        {
            var intIdPersonParameter = intIdPerson.HasValue ?
                new ObjectParameter("intIdPerson", intIdPerson) :
                new ObjectParameter("intIdPerson", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLastChargingHistory_Result>("GetLastChargingHistory", intIdPersonParameter);
        }
    
        public virtual ObjectResult<GetAllLastCharging_Result> GetAllLastCharging()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllLastCharging_Result>("GetAllLastCharging");
        }
    
        public virtual ObjectResult<GetAllBasicCustomers_Result> GetAllBasicCustomers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllBasicCustomers_Result>("GetAllBasicCustomers");
        }
    
        public virtual int AtualizaDadosPais()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("AtualizaDadosPais");
        }
    
        public virtual ObjectResult<GetLinhasComDetalhesCobrancaMassiva_Result> GetLinhasComDetalhesCobrancaMassiva()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLinhasComDetalhesCobrancaMassiva_Result>("GetLinhasComDetalhesCobrancaMassiva");
        }
    
        public virtual ObjectResult<GetValoresCobrarClaro_Result> GetValoresCobrarClaro(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetValoresCobrarClaro_Result>("GetValoresCobrarClaro", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<GetValoresCobrarVivo_Result> GetValoresCobrarVivo(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetValoresCobrarVivo_Result>("GetValoresCobrarVivo", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<GetDivergencias_Result> GetDivergencias()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetDivergencias_Result>("GetDivergencias");
        }
    
        public virtual ObjectResult<GetTransacoesPagas_Result> GetTransacoesPagas()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTransacoesPagas_Result>("GetTransacoesPagas");
        }
    
        public virtual ObjectResult<Nullable<int>> GetCountComissionOrder()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetCountComissionOrder");
        }
    
        public virtual ObjectResult<GetClientesSemPagamento_Result> GetClientesSemPagamento()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetClientesSemPagamento_Result>("GetClientesSemPagamento");
        }
    
        public virtual ObjectResult<GetLinhasFoneclubeAtivas_Result> GetLinhasFoneclubeAtivas(Nullable<int> intIdCustomer)
        {
            var intIdCustomerParameter = intIdCustomer.HasValue ?
                new ObjectParameter("intIdCustomer", intIdCustomer) :
                new ObjectParameter("intIdCustomer", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLinhasFoneclubeAtivas_Result>("GetLinhasFoneclubeAtivas", intIdCustomerParameter);
        }
    
        public virtual ObjectResult<GetTransactionsSoltasPagarme_Result> GetTransactionsSoltasPagarme()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTransactionsSoltasPagarme_Result>("GetTransactionsSoltasPagarme");
        }
    
        public virtual ObjectResult<GetMutiplosFilhosHierarquia_Result> GetMutiplosFilhosHierarquia(string pais)
        {
            var paisParameter = pais != null ?
                new ObjectParameter("pais", pais) :
                new ObjectParameter("pais", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMutiplosFilhosHierarquia_Result>("GetMutiplosFilhosHierarquia", paisParameter);
        }
    
        public virtual ObjectResult<GetLinhasFoneclubeMinimal_Result> GetLinhasFoneclubeMinimal()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetLinhasFoneclubeMinimal_Result>("GetLinhasFoneclubeMinimal");
        }
    
        public virtual ObjectResult<Nullable<int>> GetPrimeirasCobrancasSemTratamento()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetPrimeirasCobrancasSemTratamento");
        }
    
        public virtual int UpdateAmountBonus()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateAmountBonus");
        }
    
        public virtual ObjectResult<GetCieloTransactionsToRestore_Result> GetCieloTransactionsToRestore()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCieloTransactionsToRestore_Result>("GetCieloTransactionsToRestore");
        }
    
        public virtual ObjectResult<GetTransacoesAptasComissaoCielo_Result> GetTransacoesAptasComissaoCielo()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTransacoesAptasComissaoCielo_Result>("GetTransacoesAptasComissaoCielo");
        }
    
        public virtual ObjectResult<PrAvailablePhoneNumbers_Result> PrAvailablePhoneNumbers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<PrAvailablePhoneNumbers_Result>("PrAvailablePhoneNumbers");
        }
    
        public virtual ObjectResult<GetAllPhoneNumbersByPerson_Result> GetAllPhoneNumbersByPerson(Nullable<int> personId)
        {
            var personIdParameter = personId.HasValue ?
                new ObjectParameter("PersonId", personId) :
                new ObjectParameter("PersonId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllPhoneNumbersByPerson_Result>("GetAllPhoneNumbersByPerson", personIdParameter);
        }
    
        public virtual ObjectResult<GetAllPhoneNumbers_Result> GetAllPhoneNumbers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllPhoneNumbers_Result>("GetAllPhoneNumbers");
        }
    
        public virtual ObjectResult<GetCustomersPendingFlags_Result> GetCustomersPendingFlags()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCustomersPendingFlags_Result>("GetCustomersPendingFlags");
        }
    
        public virtual int CobFullClaro(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CobFullClaro", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<GetValoresOperadoraClaro_Result> GetValoresOperadoraClaro(Nullable<int> mes, Nullable<int> ano)
        {
            var mesParameter = mes.HasValue ?
                new ObjectParameter("mes", mes) :
                new ObjectParameter("mes", typeof(int));
    
            var anoParameter = ano.HasValue ?
                new ObjectParameter("ano", ano) :
                new ObjectParameter("ano", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetValoresOperadoraClaro_Result>("GetValoresOperadoraClaro", mesParameter, anoParameter);
        }
    
        public virtual ObjectResult<GetTransacoesAptasComissaoNovaLoja_Result> GetTransacoesAptasComissaoNovaLoja()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTransacoesAptasComissaoNovaLoja_Result>("GetTransacoesAptasComissaoNovaLoja");
        }
    
        public virtual ObjectResult<GetCustomerPhonesUltimasCobrancasPagas_Result> GetCustomerPhonesUltimasCobrancasPagas()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCustomerPhonesUltimasCobrancasPagas_Result>("GetCustomerPhonesUltimasCobrancasPagas");
        }
    
        public virtual ObjectResult<GetHistoricoPagamento_Result> GetHistoricoPagamento(Nullable<int> intIdCustomer)
        {
            var intIdCustomerParameter = intIdCustomer.HasValue ?
                new ObjectParameter("intIdCustomer", intIdCustomer) :
                new ObjectParameter("intIdCustomer", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetHistoricoPagamento_Result>("GetHistoricoPagamento", intIdCustomerParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> IsCustomerPayd(Nullable<int> matricula)
        {
            var matriculaParameter = matricula.HasValue ?
                new ObjectParameter("matricula", matricula) :
                new ObjectParameter("matricula", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("IsCustomerPayd", matriculaParameter);
        }
    
        public virtual ObjectResult<GetCustomerBonusDetails_Result> GetCustomerBonusDetails(Nullable<int> matricula)
        {
            var matriculaParameter = matricula.HasValue ?
                new ObjectParameter("matricula", matricula) :
                new ObjectParameter("matricula", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCustomerBonusDetails_Result>("GetCustomerBonusDetails", matriculaParameter);
        }
    
        public virtual ObjectResult<GetClientesComPagamento_Result> GetClientesComPagamento()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetClientesComPagamento_Result>("GetClientesComPagamento");
        }
    
        public virtual ObjectResult<GetCustomerPhonesUltimasCobrancasPagasGeneric_Result> GetCustomerPhonesUltimasCobrancasPagasGeneric()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCustomerPhonesUltimasCobrancasPagasGeneric_Result>("GetCustomerPhonesUltimasCobrancasPagasGeneric");
        }
    
        public virtual ObjectResult<GetPaternidadeStatusFilhos_Result> GetPaternidadeStatusFilhos()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPaternidadeStatusFilhos_Result>("GetPaternidadeStatusFilhos");
        }
    
        public virtual ObjectResult<GetNewTransactionsPagarme_Result> GetNewTransactionsPagarme()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetNewTransactionsPagarme_Result>("GetNewTransactionsPagarme");
        }
    
        public virtual ObjectResult<GetTodosClientes_Result> GetTodosClientes()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTodosClientes_Result>("GetTodosClientes");
        }
    
        public virtual ObjectResult<getList24hsLate_Result> getList24hsLate(Nullable<System.DateTime> startDate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<getList24hsLate_Result>("getList24hsLate", startDateParameter);
        }
    
        public virtual ObjectResult<GetPlansForComparison_Result> GetPlansForComparison()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPlansForComparison_Result>("GetPlansForComparison");
        }
    
        public virtual ObjectResult<GetAllPhoneLines_Result> GetAllPhoneLines()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllPhoneLines_Result>("GetAllPhoneLines");
        }
    
        public virtual ObjectResult<Nullable<int>> GetAllCustomersToBlock()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetAllCustomersToBlock");
        }
    
        public virtual ObjectResult<GetAllContelLinesMappedToOtherOperator_Result> GetAllContelLinesMappedToOtherOperator()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllContelLinesMappedToOtherOperator_Result>("GetAllContelLinesMappedToOtherOperator");
        }
    
        public virtual ObjectResult<Nullable<decimal>> GetSumpVIPByCustomer(Nullable<int> intIdPerson)
        {
            var intIdPersonParameter = intIdPerson.HasValue ?
                new ObjectParameter("intIdPerson", intIdPerson) :
                new ObjectParameter("intIdPerson", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<decimal>>("GetSumpVIPByCustomer", intIdPersonParameter);
        }
    
        public virtual ObjectResult<GetAllContelLinesNotInFC_Result> GetAllContelLinesNotInFC()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllContelLinesNotInFC_Result>("GetAllContelLinesNotInFC");
        }
    
        public virtual ObjectResult<GetPhoneLinesByPerson_Result> GetPhoneLinesByPerson(Nullable<int> personId)
        {
            var personIdParameter = personId.HasValue ?
                new ObjectParameter("PersonId", personId) :
                new ObjectParameter("PersonId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPhoneLinesByPerson_Result>("GetPhoneLinesByPerson", personIdParameter);
        }
    
        public virtual ObjectResult<GetAllCustomers_Result> GetAllCustomers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllCustomers_Result>("GetAllCustomers");
        }
    
        public virtual ObjectResult<GetNonTopLinesInXDays_Result> GetNonTopLinesInXDays(Nullable<int> start, Nullable<int> end)
        {
            var startParameter = start.HasValue ?
                new ObjectParameter("Start", start) :
                new ObjectParameter("Start", typeof(int));
    
            var endParameter = end.HasValue ?
                new ObjectParameter("End", end) :
                new ObjectParameter("End", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetNonTopLinesInXDays_Result>("GetNonTopLinesInXDays", startParameter, endParameter);
        }
    
        public virtual int UpdateContelToChargeHistory(Nullable<int> lastId)
        {
            var lastIdParameter = lastId.HasValue ?
                new ObjectParameter("LastId", lastId) :
                new ObjectParameter("LastId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateContelToChargeHistory", lastIdParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> GetCustomersWithSameVigenicaUnPaid()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetCustomersWithSameVigenicaUnPaid");
        }
    
        public virtual ObjectResult<GetAllContelLinesInWebFCButNotInContel_Result> GetAllContelLinesInWebFCButNotInContel()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllContelLinesInWebFCButNotInContel_Result>("GetAllContelLinesInWebFCButNotInContel");
        }
    
        public virtual ObjectResult<GetRefusedCCChargeForResend_Result> GetRefusedCCChargeForResend()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetRefusedCCChargeForResend_Result>("GetRefusedCCChargeForResend");
        }
    
        public virtual ObjectResult<GetFacilSaleStats_Result> GetFacilSaleStats(Nullable<int> type)
        {
            var typeParameter = type.HasValue ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFacilSaleStats_Result>("GetFacilSaleStats", typeParameter);
        }
    
        public virtual ObjectResult<GetFacilSaleStatsByFilter_Result> GetFacilSaleStatsByFilter(Nullable<int> id, string startDate, string endDate, string operation, string choice)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("Id", id) :
                new ObjectParameter("Id", typeof(int));
    
            var startDateParameter = startDate != null ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(string));
    
            var endDateParameter = endDate != null ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(string));
    
            var operationParameter = operation != null ?
                new ObjectParameter("Operation", operation) :
                new ObjectParameter("Operation", typeof(string));
    
            var choiceParameter = choice != null ?
                new ObjectParameter("Choice", choice) :
                new ObjectParameter("Choice", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFacilSaleStatsByFilter_Result>("GetFacilSaleStatsByFilter", idParameter, startDateParameter, endDateParameter, operationParameter, choiceParameter);
        }
    
        public virtual ObjectResult<GetPagarmeTransactionReport_Result> GetPagarmeTransactionReport(Nullable<System.DateTime> fromDate, Nullable<System.DateTime> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(System.DateTime));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPagarmeTransactionReport_Result>("GetPagarmeTransactionReport", fromDateParameter, toDateParameter);
        }
    
        public virtual ObjectResult<GetInternationalDepositsReport_Result> GetInternationalDepositsReport(Nullable<System.DateTime> fromDate, Nullable<System.DateTime> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(System.DateTime));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetInternationalDepositsReport_Result>("GetInternationalDepositsReport", fromDateParameter, toDateParameter);
        }
    
        public virtual ObjectResult<GetInternationalTransactionReport_Result> GetInternationalTransactionReport(Nullable<System.DateTime> fromDate, Nullable<System.DateTime> toDate)
        {
            var fromDateParameter = fromDate.HasValue ?
                new ObjectParameter("FromDate", fromDate) :
                new ObjectParameter("FromDate", typeof(System.DateTime));
    
            var toDateParameter = toDate.HasValue ?
                new ObjectParameter("ToDate", toDate) :
                new ObjectParameter("ToDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetInternationalTransactionReport_Result>("GetInternationalTransactionReport", fromDateParameter, toDateParameter);
        }
    
        public virtual ObjectResult<GetFacilSaleStatsPercentage_Result> GetFacilSaleStatsPercentage(Nullable<int> type)
        {
            var typeParameter = type.HasValue ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFacilSaleStatsPercentage_Result>("GetFacilSaleStatsPercentage", typeParameter);
        }
    
        public virtual ObjectResult<GetFacilSalePlanPercentage_Result> GetFacilSalePlanPercentage(Nullable<int> type)
        {
            var typeParameter = type.HasValue ?
                new ObjectParameter("Type", type) :
                new ObjectParameter("Type", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetFacilSalePlanPercentage_Result>("GetFacilSalePlanPercentage", typeParameter);
        }
    }
}
