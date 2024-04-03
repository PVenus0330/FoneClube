
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/19/2020 13:41:19
-- Generated from EDMX file: C:\GitProjects\FoneClube.NET\FoneClube.DataAccess\FoneClubeDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [foneclube-producao];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK__tblBilhet__intCo__06CD04F7]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblBilhetacoesFebraban] DROP CONSTRAINT [FK__tblBilhet__intCo__06CD04F7];
GO
IF OBJECT_ID(N'[dbo].[FK__tblChargi__intId__07C12930]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblChargingHistory] DROP CONSTRAINT [FK__tblChargi__intId__07C12930];
GO
IF OBJECT_ID(N'[dbo].[FK__tblChargi__intId__08B54D69]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblChargingHistory] DROP CONSTRAINT [FK__tblChargi__intId__08B54D69];
GO
IF OBJECT_ID(N'[dbo].[FK__tblChargi__Phone__09A971A2]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblChargingHistory] DROP CONSTRAINT [FK__tblChargi__Phone__09A971A2];
GO
IF OBJECT_ID(N'[dbo].[FK__tblComiss__intId__11D4A34F]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComissionOrder] DROP CONSTRAINT [FK__tblComiss__intId__11D4A34F];
GO
IF OBJECT_ID(N'[dbo].[FK__tblComiss__intId__12C8C788]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComissionOrder] DROP CONSTRAINT [FK__tblComiss__intId__12C8C788];
GO
IF OBJECT_ID(N'[dbo].[FK__tblComiss__intId__13BCEBC1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComissionOrder] DROP CONSTRAINT [FK__tblComiss__intId__13BCEBC1];
GO
IF OBJECT_ID(N'[dbo].[FK__tblComiss__intId__1699586C]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComissionTokens] DROP CONSTRAINT [FK__tblComiss__intId__1699586C];
GO
IF OBJECT_ID(N'[dbo].[FK__tblComiss__intId__178D7CA5]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComissionTokens] DROP CONSTRAINT [FK__tblComiss__intId__178D7CA5];
GO
IF OBJECT_ID(N'[dbo].[FK__tblCommis__intId__0A9D95DB]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblCommissionOrders] DROP CONSTRAINT [FK__tblCommis__intId__0A9D95DB];
GO
IF OBJECT_ID(N'[dbo].[FK__tblCommis__intId__0B91BA14]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblCommissionOrders] DROP CONSTRAINT [FK__tblCommis__intId__0B91BA14];
GO
IF OBJECT_ID(N'[dbo].[FK__tblDescon__intCo__0E6E26BF]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDescontosFebraban] DROP CONSTRAINT [FK__tblDescon__intCo__0E6E26BF];
GO
IF OBJECT_ID(N'[dbo].[FK__tblDiscou__intId__0F624AF8]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDiscountPrice] DROP CONSTRAINT [FK__tblDiscou__intId__0F624AF8];
GO
IF OBJECT_ID(N'[dbo].[FK__tblEmailL__intId__12E8C319]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblEmailLog] DROP CONSTRAINT [FK__tblEmailL__intId__12E8C319];
GO
IF OBJECT_ID(N'[dbo].[FK__tblEndere__intCo__10566F31]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblEnderecosFebraban] DROP CONSTRAINT [FK__tblEndere__intCo__10566F31];
GO
IF OBJECT_ID(N'[dbo].[FK__tblGeneri__intId__04659998]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblGenericPhoneFlags] DROP CONSTRAINT [FK__tblGeneri__intId__04659998];
GO
IF OBJECT_ID(N'[dbo].[FK__tblHeadsF__intCo__123EB7A3]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblHeadsFebraban] DROP CONSTRAINT [FK__tblHeadsF__intCo__123EB7A3];
GO
IF OBJECT_ID(N'[dbo].[FK__tblLineRe__intId__1332DBDC]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLineRecord] DROP CONSTRAINT [FK__tblLineRe__intId__1332DBDC];
GO
IF OBJECT_ID(N'[dbo].[FK__tblLineRe__intId__14270015]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLineRecord] DROP CONSTRAINT [FK__tblLineRe__intId__14270015];
GO
IF OBJECT_ID(N'[dbo].[FK__tblLog__intIdTip__151B244E]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLog] DROP CONSTRAINT [FK__tblLog__intIdTip__151B244E];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__160F4887]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsAddresses] DROP CONSTRAINT [FK__tblPerson__intId__160F4887];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__17036CC0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsImages] DROP CONSTRAINT [FK__tblPerson__intId__17036CC0];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__17F790F9]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsPhones] DROP CONSTRAINT [FK__tblPerson__intId__17F790F9];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPersos__intId__697C9932]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersosAffiliateLinks] DROP CONSTRAINT [FK__tblPersos__intId__697C9932];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPhones__intId__390E6C01]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPhonesServices] DROP CONSTRAINT [FK__tblPhones__intId__390E6C01];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPhones__intId__3A02903A]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPhonesServices] DROP CONSTRAINT [FK__tblPhones__intId__3A02903A];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlans__intIdO__18EBB532]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlans] DROP CONSTRAINT [FK__tblPlans__intIdO__18EBB532];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlans__intIdP__19DFD96B]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlans] DROP CONSTRAINT [FK__tblPlans__intIdP__19DFD96B];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlansO__intId__1AD3FDA4]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlansOptions] DROP CONSTRAINT [FK__tblPlansO__intId__1AD3FDA4];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__1BC821DD]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__1BC821DD];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__1CBC4616]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__1CBC4616];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__1DB06A4F]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__1DB06A4F];
GO
IF OBJECT_ID(N'[dbo].[FK__tblResumo__intCo__1EA48E88]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblResumosFebraban] DROP CONSTRAINT [FK__tblResumo__intCo__1EA48E88];
GO
IF OBJECT_ID(N'[dbo].[FK__tblServic__intCo__1F98B2C1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblServicosFebraban] DROP CONSTRAINT [FK__tblServic__intCo__1F98B2C1];
GO
IF OBJECT_ID(N'[dbo].[FK__tblTotali__intCo__2180FB33]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblTotalizadoresFebraban] DROP CONSTRAINT [FK__tblTotali__intCo__2180FB33];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[tblBilhetacoesFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBilhetacoesFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblBonusOptions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBonusOptions];
GO
IF OBJECT_ID(N'[dbo].[tblBonusOrder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBonusOrder];
GO
IF OBJECT_ID(N'[dbo].[tblBonusOrderException]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBonusOrderException];
GO
IF OBJECT_ID(N'[dbo].[tblBonusOrderLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBonusOrderLog];
GO
IF OBJECT_ID(N'[dbo].[tblChargingHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblChargingHistory];
GO
IF OBJECT_ID(N'[dbo].[tblChargingLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblChargingLog];
GO
IF OBJECT_ID(N'[dbo].[tblCheckoutPagarMeLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCheckoutPagarMeLog];
GO
IF OBJECT_ID(N'[dbo].[tblCieloPaymentLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCieloPaymentLog];
GO
IF OBJECT_ID(N'[dbo].[tblCieloPaymentStatus]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCieloPaymentStatus];
GO
IF OBJECT_ID(N'[dbo].[tblComissionOrder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblComissionOrder];
GO
IF OBJECT_ID(N'[dbo].[tblComissionTokens]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblComissionTokens];
GO
IF OBJECT_ID(N'[dbo].[tblComissionValidationLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblComissionValidationLog];
GO
IF OBJECT_ID(N'[dbo].[tblCommisionLevels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCommisionLevels];
GO
IF OBJECT_ID(N'[dbo].[tblCommissionOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCommissionOrders];
GO
IF OBJECT_ID(N'[dbo].[tblCommissions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblCommissions];
GO
IF OBJECT_ID(N'[dbo].[tblContasFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblContasFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblDescontosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblDescontosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblDiscountPrice]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblDiscountPrice];
GO
IF OBJECT_ID(N'[dbo].[tblEmailLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblEmailLog];
GO
IF OBJECT_ID(N'[dbo].[tblEmailTemplates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblEmailTemplates];
GO
IF OBJECT_ID(N'[dbo].[tblEnderecosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblEnderecosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblFoneclubePagarmeTransactions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblFoneclubePagarmeTransactions];
GO
IF OBJECT_ID(N'[dbo].[tblGenericFlags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblGenericFlags];
GO
IF OBJECT_ID(N'[dbo].[tblGenericPhoneFlags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblGenericPhoneFlags];
GO
IF OBJECT_ID(N'[dbo].[tblHeadsFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblHeadsFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblHistoryEvents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblHistoryEvents];
GO
IF OBJECT_ID(N'[dbo].[tblLineRecord]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLineRecord];
GO
IF OBJECT_ID(N'[dbo].[tblLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLog];
GO
IF OBJECT_ID(N'[dbo].[tblLogBackupCharging]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLogBackupCharging];
GO
IF OBJECT_ID(N'[dbo].[tblLogComissionOrder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLogComissionOrder];
GO
IF OBJECT_ID(N'[dbo].[tblLogTipo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLogTipo];
GO
IF OBJECT_ID(N'[dbo].[tblMassChargingLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblMassChargingLog];
GO
IF OBJECT_ID(N'[dbo].[tblOperadoras]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOperadoras];
GO
IF OBJECT_ID(N'[dbo].[tblPastPagarmeUpdate]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPastPagarmeUpdate];
GO
IF OBJECT_ID(N'[dbo].[tblPaymentTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPaymentTypes];
GO
IF OBJECT_ID(N'[dbo].[tblPersonAssociationHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonAssociationHistory];
GO
IF OBJECT_ID(N'[dbo].[tblPersons]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersons];
GO
IF OBJECT_ID(N'[dbo].[tblPersonsAddresses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonsAddresses];
GO
IF OBJECT_ID(N'[dbo].[tblPersonsImages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonsImages];
GO
IF OBJECT_ID(N'[dbo].[tblPersonsParents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonsParents];
GO
IF OBJECT_ID(N'[dbo].[tblPersonsPhones]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonsPhones];
GO
IF OBJECT_ID(N'[dbo].[tblPersosAffiliateLinks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersosAffiliateLinks];
GO
IF OBJECT_ID(N'[dbo].[tblPhoneFlags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhoneFlags];
GO
IF OBJECT_ID(N'[dbo].[tblPhonePropertyHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhonePropertyHistory];
GO
IF OBJECT_ID(N'[dbo].[tblPhones]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhones];
GO
IF OBJECT_ID(N'[dbo].[tblPhonesServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhonesServices];
GO
IF OBJECT_ID(N'[dbo].[tblPhonesStockProperty]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhonesStockProperty];
GO
IF OBJECT_ID(N'[dbo].[tblPhoneStatus]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhoneStatus];
GO
IF OBJECT_ID(N'[dbo].[tblPhoneStock]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPhoneStock];
GO
IF OBJECT_ID(N'[dbo].[tblPlanDetails]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPlanDetails];
GO
IF OBJECT_ID(N'[dbo].[tblPlanDetailType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPlanDetailType];
GO
IF OBJECT_ID(N'[dbo].[tblPlans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPlans];
GO
IF OBJECT_ID(N'[dbo].[tblPlansOptions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPlansOptions];
GO
IF OBJECT_ID(N'[dbo].[tblReferred]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblReferred];
GO
IF OBJECT_ID(N'[dbo].[tblRegisterCustomersLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblRegisterCustomersLog];
GO
IF OBJECT_ID(N'[dbo].[tblResumosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblResumosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblServiceOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServiceOrders];
GO
IF OBJECT_ID(N'[dbo].[tblServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServices];
GO
IF OBJECT_ID(N'[dbo].[tblServicosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServicosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblTotalizadoresFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblTotalizadoresFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblTransactionsCielo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblTransactionsCielo];
GO
IF OBJECT_ID(N'[dbo].[tblWhatsappMessages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblWhatsappMessages];
GO
IF OBJECT_ID(N'[ModelStoreContainer].[CobFullClaro_Sum]', 'U') IS NOT NULL
    DROP TABLE [ModelStoreContainer].[CobFullClaro_Sum];
GO
IF OBJECT_ID(N'[ModelStoreContainer].[CobFullClaroMax_Sum]', 'U') IS NOT NULL
    DROP TABLE [ModelStoreContainer].[CobFullClaroMax_Sum];
GO
IF OBJECT_ID(N'[ModelStoreContainer].[CobFullVivo_Sum]', 'U') IS NOT NULL
    DROP TABLE [ModelStoreContainer].[CobFullVivo_Sum];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'tblCommissions'
CREATE TABLE [dbo].[tblCommissions] (
    [intIdComission] int  NOT NULL,
    [intComissionCost] int  NOT NULL
);
GO

-- Creating table 'tblLog'
CREATE TABLE [dbo].[tblLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdTipo] int  NULL,
    [txtAction] nvarchar(max)  NULL,
    [dteTimeStamp] datetime  NOT NULL
);
GO

-- Creating table 'tblLogTipo'
CREATE TABLE [dbo].[tblLogTipo] (
    [intIdTipo] int IDENTITY(1,1) NOT NULL,
    [txtDescricao] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblOperadoras'
CREATE TABLE [dbo].[tblOperadoras] (
    [intIdOperator] int  NOT NULL,
    [txtName] nvarchar(10)  NOT NULL
);
GO

-- Creating table 'tblPaymentTypes'
CREATE TABLE [dbo].[tblPaymentTypes] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtComment] nvarchar(20)  NOT NULL
);
GO

-- Creating table 'tblPlanDetails'
CREATE TABLE [dbo].[tblPlanDetails] (
    [intIdPlanDetail] int IDENTITY(1,1) NOT NULL,
    [intIdPlan] int  NOT NULL,
    [intDetailType] int  NOT NULL,
    [intDetailValue] bigint  NOT NULL,
    [intDetailCost] bigint  NOT NULL,
    [intExtraCost] bigint  NOT NULL
);
GO

-- Creating table 'tblPlanDetailType'
CREATE TABLE [dbo].[tblPlanDetailType] (
    [intIdDetailType] int IDENTITY(1,1) NOT NULL,
    [txtDescription] varchar(50)  NOT NULL
);
GO

-- Creating table 'tblPlans'
CREATE TABLE [dbo].[tblPlans] (
    [intIdPlan] int IDENTITY(1,1) NOT NULL,
    [intIdOption] int  NULL,
    [intIdPerson] int  NULL
);
GO

-- Creating table 'tblPlansOptions'
CREATE TABLE [dbo].[tblPlansOptions] (
    [intIdPlan] int  NOT NULL,
    [intIdOperator] int  NOT NULL,
    [txtDescription] nvarchar(60)  NOT NULL,
    [intCost] int  NOT NULL,
    [intBitActive] bit  NULL
);
GO

-- Creating table 'tblReferred'
CREATE TABLE [dbo].[tblReferred] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdComission] int  NOT NULL,
    [intIdDad] int  NOT NULL,
    [intIdCurrent] int  NOT NULL
);
GO

-- Creating table 'tblBilhetacoesFebraban'
CREATE TABLE [dbo].[tblBilhetacoesFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtIDUnicoNRC] nvarchar(250)  NOT NULL,
    [txtRecursoCNL] nvarchar(250)  NOT NULL,
    [txtDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefone] nvarchar(250)  NOT NULL,
    [txtCaracteristicaRecurso] nvarchar(250)  NOT NULL,
    [txtDegrauRecurso] nvarchar(250)  NOT NULL,
    [txtDataLigacao] nvarchar(250)  NOT NULL,
    [txtCNLLocalidadeChamada] nvarchar(250)  NOT NULL,
    [txtNomeLocalidadeChamada] nvarchar(250)  NOT NULL,
    [txtUFTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtCODNacionalInternacional] nvarchar(250)  NOT NULL,
    [txtCODOperadora] nvarchar(250)  NOT NULL,
    [txtDescricaoOperadora] nvarchar(250)  NOT NULL,
    [txtCODPais] nvarchar(250)  NOT NULL,
    [txtAreaDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtConjugadoNumeroTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtDuracaoLigacao] nvarchar(250)  NOT NULL,
    [txtCategoria] nvarchar(250)  NOT NULL,
    [txtDescricaoCategoria] nvarchar(250)  NOT NULL,
    [txtHorarioLigacao] nvarchar(250)  NOT NULL,
    [txtTipoChamada] nvarchar(250)  NOT NULL,
    [txtGrupoHorarioTarifario] nvarchar(250)  NOT NULL,
    [txtDescricaoHorarioTarifario] nvarchar(250)  NOT NULL,
    [txtDegrauLigacao] nvarchar(250)  NOT NULL,
    [txtSinalValorLigacao] nvarchar(250)  NOT NULL,
    [txtAliquotaICMS] nvarchar(250)  NOT NULL,
    [txtValorLigacaoComImposto] nvarchar(250)  NOT NULL,
    [txtClasseServico] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblContasFebraban'
CREATE TABLE [dbo].[tblContasFebraban] (
    [intContaId] int IDENTITY(1,1) NOT NULL,
    [dteInsert] datetime  NOT NULL,
    [dteGeracaoConta] nvarchar(250)  NOT NULL,
    [intTipoOperadora] int  NOT NULL,
    [txtIdContaUnica] nvarchar(250)  NOT NULL,
    [intIdContaUnica] bigint  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [intIdEmissao] bigint  NOT NULL
);
GO

-- Creating table 'tblDescontosFebraban'
CREATE TABLE [dbo].[tblDescontosFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtIDUnicoNRC] nvarchar(250)  NOT NULL,
    [txtRecursoCNL] nvarchar(250)  NOT NULL,
    [txtDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefone] nvarchar(250)  NOT NULL,
    [txtGrupoCategoria] nvarchar(250)  NOT NULL,
    [txtDescricaoGrupoCategoria] nvarchar(250)  NOT NULL,
    [txtSinalValorLigacao] nvarchar(250)  NOT NULL,
    [txtBaseCalculoDesconto] nvarchar(250)  NOT NULL,
    [txtPercentualDesconto] nvarchar(250)  NOT NULL,
    [txtValorLigacao] nvarchar(250)  NOT NULL,
    [txtDataInicioAcerto] nvarchar(250)  NOT NULL,
    [txtHoraInicioAcerto] nvarchar(250)  NOT NULL,
    [txtDataFimAcerto] nvarchar(250)  NOT NULL,
    [txtHoraFimAcerto] nvarchar(250)  NOT NULL,
    [txtClasseServico] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblEnderecosFebraban'
CREATE TABLE [dbo].[tblEnderecosFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtIDUnicoNRC] nvarchar(250)  NOT NULL,
    [txtDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefone] nvarchar(250)  NOT NULL,
    [txtCaracteristicaRecurso] nvarchar(250)  NOT NULL,
    [txtCNLRecursoEnderecoPontaA] nvarchar(250)  NOT NULL,
    [txtNomeLocalidadePontaA] nvarchar(250)  NOT NULL,
    [txtUFLocalidadePontaA] nvarchar(250)  NOT NULL,
    [txtEnderecoPontaA] nvarchar(250)  NOT NULL,
    [txtNumeroEnderecoPontaA] nvarchar(250)  NOT NULL,
    [txtComplementoPontaA] nvarchar(250)  NOT NULL,
    [txtBairroPontaA] nvarchar(250)  NOT NULL,
    [txtCNLRecursoEnderecoPontaB] nvarchar(250)  NOT NULL,
    [txtNomeLocalidadePontaB] nvarchar(250)  NOT NULL,
    [txtUFLocalidadePontaB] nvarchar(250)  NOT NULL,
    [txtEnderecoPontaB] nvarchar(250)  NOT NULL,
    [txtNumeroEnderecoPontaB] nvarchar(250)  NOT NULL,
    [txtComplementoPontaB] nvarchar(250)  NOT NULL,
    [txtBairroPontaB] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblHeadsFebraban'
CREATE TABLE [dbo].[tblHeadsFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtIdContaUnica] nvarchar(250)  NOT NULL,
    [txtDataGeracaoArquivo] nvarchar(250)  NOT NULL,
    [txtIdentificadorEmpresa] nvarchar(250)  NOT NULL,
    [txtEmpresaUF] nvarchar(250)  NOT NULL,
    [txtCodigoCliente] nvarchar(250)  NOT NULL,
    [txtNomeCliente] nvarchar(250)  NOT NULL,
    [txtClienteCGC] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblResumosFebraban'
CREATE TABLE [dbo].[tblResumosFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtIDUnicoNRC] nvarchar(250)  NOT NULL,
    [txtRecursoCNL] nvarchar(250)  NOT NULL,
    [txtLocalidade] nvarchar(250)  NOT NULL,
    [txtDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefone] nvarchar(250)  NOT NULL,
    [txtTipoServico] nvarchar(250)  NOT NULL,
    [txtDescricaoServico] nvarchar(250)  NOT NULL,
    [txtCaracteristicaRecurso] nvarchar(250)  NOT NULL,
    [txtDegrauRecurso] nvarchar(250)  NOT NULL,
    [txtVelocidadeRecurso] nvarchar(250)  NOT NULL,
    [txtUnidadeVelocidadeRecurso] nvarchar(250)  NOT NULL,
    [txtInicioAssinatura] nvarchar(250)  NOT NULL,
    [txtFimAssinatura] nvarchar(250)  NOT NULL,
    [txtInicioPeriodoServico] nvarchar(250)  NOT NULL,
    [txtFimPeriodoServico] nvarchar(250)  NOT NULL,
    [txtUnidadeConsumo] nvarchar(250)  NOT NULL,
    [txtQuantidadeConsumo] nvarchar(250)  NOT NULL,
    [txtSinalValorConsumo] nvarchar(250)  NOT NULL,
    [txtValorConsumo] nvarchar(250)  NOT NULL,
    [txtSinalAssinatura] nvarchar(250)  NOT NULL,
    [txtValorAssinatura] nvarchar(250)  NOT NULL,
    [txtAliquota] nvarchar(250)  NOT NULL,
    [txtSinalICMS] nvarchar(250)  NOT NULL,
    [txtValorICMS] nvarchar(250)  NOT NULL,
    [txtSinalTotalOutrosImpostos] nvarchar(250)  NOT NULL,
    [txtValorTotalImpostos] nvarchar(250)  NOT NULL,
    [txtNumeroNotaFiscal] nvarchar(250)  NOT NULL,
    [txtSinalValorConta] nvarchar(250)  NOT NULL,
    [txtValorConta] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblServicosFebraban'
CREATE TABLE [dbo].[tblServicosFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtIDUnicoNRC] nvarchar(250)  NOT NULL,
    [txtRecursoCNL] nvarchar(250)  NOT NULL,
    [txtDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefone] nvarchar(250)  NOT NULL,
    [txtCaracteristicaRecurso] nvarchar(250)  NOT NULL,
    [txtDataServico] nvarchar(250)  NOT NULL,
    [txtCNLLocalidadeChamada] nvarchar(250)  NOT NULL,
    [txtNomeLocalidadeChamada] nvarchar(250)  NOT NULL,
    [txtUFTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtCODNacionalInternacional] nvarchar(250)  NOT NULL,
    [txtCODOperadora] nvarchar(250)  NOT NULL,
    [txtDescricaoOperadora] nvarchar(250)  NOT NULL,
    [txtCODPais] nvarchar(250)  NOT NULL,
    [txtAreaDDD] nvarchar(250)  NOT NULL,
    [txtNumeroTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtConjugadoNumeroTelefoneChamado] nvarchar(250)  NOT NULL,
    [txtDuracaoLigacao] nvarchar(250)  NOT NULL,
    [txtHorarioLigacao] nvarchar(250)  NOT NULL,
    [txtGrupoCategoria] nvarchar(250)  NOT NULL,
    [txtDescricaoGrupoCategoria] nvarchar(250)  NOT NULL,
    [txtCategoria] nvarchar(250)  NOT NULL,
    [txtDescricaoCategoria] nvarchar(250)  NOT NULL,
    [txtSinalValorLigacao] nvarchar(250)  NOT NULL,
    [txtValorLigacao] nvarchar(250)  NOT NULL,
    [txtClasseServico] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblTotalizadoresFebraban'
CREATE TABLE [dbo].[tblTotalizadoresFebraban] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intContaId] int  NULL,
    [txtControleSequencial] nvarchar(250)  NOT NULL,
    [txtCodigoCliente] nvarchar(250)  NOT NULL,
    [txtVencimento] nvarchar(250)  NOT NULL,
    [txtEmissao] nvarchar(250)  NOT NULL,
    [txtQuantidadeRegistros] nvarchar(250)  NOT NULL,
    [txtQuantidadeLinhas] nvarchar(250)  NOT NULL,
    [txtSinalTotal] nvarchar(250)  NOT NULL,
    [txtValorTotal] nvarchar(250)  NOT NULL,
    [txtFiller] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblDiscountPrice'
CREATE TABLE [dbo].[tblDiscountPrice] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtDescription] nvarchar(300)  NULL,
    [intAmmount] bigint  NULL
);
GO

-- Creating table 'tblPersonsImages'
CREATE TABLE [dbo].[tblPersonsImages] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtImage] nvarchar(80)  NOT NULL,
    [intTipo] int  NOT NULL,
    [dteDataCadastro] datetime  NULL
);
GO

-- Creating table 'tblLineRecord'
CREATE TABLE [dbo].[tblLineRecord] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [intIdPhone] int  NULL,
    [dteEntrada] datetime  NULL,
    [dteSaida] datetime  NULL,
    [bitLinhaAssociada] bit  NULL
);
GO

-- Creating table 'tblPersonsParents'
CREATE TABLE [dbo].[tblPersonsParents] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intDDDParent] int  NULL,
    [intPhoneParent] bigint  NULL,
    [txtNameParent] nvarchar(200)  NULL,
    [intIdSon] int  NULL,
    [dteCadastro] datetime  NULL,
    [intIdParent] int  NULL
);
GO

-- Creating table 'tblServiceOrders'
CREATE TABLE [dbo].[tblServiceOrders] (
    [intIdPerson] int  NULL,
    [dteRegister] datetime  NOT NULL,
    [txtDescription] nvarchar(800)  NULL,
    [intIdAgent] int  NULL,
    [txtAgentName] nvarchar(40)  NULL,
    [bitPendingInteraction] bit  NULL,
    [bitPending] bit  NULL,
    [intIdServiceOrder] int IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'CobFullClaro_Sum'
CREATE TABLE [dbo].[CobFullClaro_Sum] (
    [Conta] nvarchar(20)  NULL,
    [Mês] nvarchar(10)  NULL,
    [Telefone] varchar(20)  NULL,
    [Nome] nvarchar(60)  NULL,
    [Nickname] nvarchar(60)  NOT NULL,
    [CPF_Foneclube] varchar(20)  NULL,
    [Plano_Foneclube] varchar(4)  NULL,
    [Plano_Contratado] varchar(20)  NULL,
    [Preço_único] float  NULL,
    [Fração_preço_único_por_linha] float  NULL,
    [Preço_Plano_FC] decimal(36,2)  NULL,
    [Sem_uso] int  NOT NULL,
    [Valor_cobrado_operadora] float  NULL,
    [Valor_total_com_excedentes_FC] float  NULL,
    [Resultado___linha] float  NULL,
    [Franquia_minutos] int  NULL,
    [Total_franquia_minutos] int  NULL,
    [Minutos_locais_excedentes] int  NULL,
    [C_Total_min__lig__Local_] int  NULL,
    [C_Total_min__lig__Local_Tarifa_Zero_] int  NULL,
    [C_Total_minutos_lig__p_números_especiais_] int  NULL,
    [C_Total_ligações_Interurbanas_] int  NULL,
    [C_Total_ligações_no_exterior_] int  NULL,
    [Separador] varchar(1)  NOT NULL,
    [Valor_Ligações_Locais_oper_] float  NULL,
    [Liga__es_para_n_meros_especiais] float  NULL,
    [Servi_os__Torpedos__Hits__Jogos__etc__] float  NULL,
    [Interurbanas_e_Rec__em_viagem_21___Embratel] float  NULL,
    [Interurbanas_e_Rec__em_viagem_31___Telemar] float  NULL,
    [Liga__es_e_Servi_os_no_exterior] float  NULL,
    [Outros] float  NULL,
    [Multa] float  NULL,
    [Erro_valor_internet_operadora] varchar(1)  NOT NULL,
    [Plano_internet_desconhecido] varchar(1)  NOT NULL,
    [Mensalidades_e_Pacotes_Promocionais] float  NULL,
    [Valor_Total_a_Cobrar_do_Cliente] float  NULL,
    [Valor_Total_a_Cobrar_da_Linha_Equalizado] float  NULL,
    [Report_Month] int  NULL,
    [Report_Year] int  NULL,
    [Creation_Date] datetime  NULL,
    [Payment_Date] datetime  NULL,
    [Expiration_Date] datetime  NULL,
    [ContaID] int  NULL
);
GO

-- Creating table 'tblHistoryEvents'
CREATE TABLE [dbo].[tblHistoryEvents] (
    [intIdEvent] int IDENTITY(1,1) NOT NULL,
    [txtEventDescription] nvarchar(400)  NOT NULL
);
GO

-- Creating table 'tblPhoneStatus'
CREATE TABLE [dbo].[tblPhoneStatus] (
    [intIdStatus] int IDENTITY(1,1) NOT NULL,
    [txtDescricao] nvarchar(200)  NULL
);
GO

-- Creating table 'tblPhones'
CREATE TABLE [dbo].[tblPhones] (
    [intIdPhone] int IDENTITY(1,1) NOT NULL,
    [intDDD] int  NOT NULL,
    [intNumber] int  NOT NULL
);
GO

-- Creating table 'tblFoneclubePagarmeTransactions'
CREATE TABLE [dbo].[tblFoneclubePagarmeTransactions] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtOutdadetStatus] nvarchar(200)  NULL,
    [intRefuse_reason] int  NULL,
    [txtStatus_reason] nvarchar(200)  NULL,
    [txtAcquirer_name] nvarchar(200)  NULL,
    [txtAcquirer_id] nvarchar(200)  NULL,
    [intTid] int  NULL,
    [intNsu] int  NULL,
    [dteDate_created] datetime  NULL,
    [dteDate_updated] datetime  NULL,
    [intAmount] int  NULL,
    [intAuthorized_amount] int  NULL,
    [intPaid_amount] int  NULL,
    [intRefunded_amount] int  NULL,
    [intInstallments] int  NULL,
    [intIdTransaction] int  NULL,
    [intCost] int  NULL,
    [txtCard_holder_name] nvarchar(200)  NULL,
    [txtCard_last_digits] nvarchar(200)  NULL,
    [txtCard_first_digits] nvarchar(200)  NULL,
    [txtCard_brand] nvarchar(200)  NULL,
    [txtBoleto_url] nvarchar(200)  NULL,
    [txtBoleto_barcode] nvarchar(200)  NULL,
    [dteBoleto_expiration_date] datetime  NULL,
    [txtIp] nvarchar(200)  NULL,
    [intIdCustomer] int  NULL,
    [txtPayment_method] nvarchar(200)  NULL,
    [bitComissionRevision] bit  NULL,
    [txtPixCode] nvarchar(1000) NULL,
    [dtePix_expiration_date] datetime  NULL
);
GO

-- Creating table 'tblCheckoutPagarMeLog'
CREATE TABLE [dbo].[tblCheckoutPagarMeLog] (
    [intCheckoutLogId] int IDENTITY(1,1) NOT NULL,
    [intAmount] int  NULL,
    [intDaysLimit] int  NULL,
    [txtBoletoInstructions] nvarchar(200)  NULL,
    [txtNome] nvarchar(200)  NULL,
    [txtEmail] nvarchar(200)  NULL,
    [txtDocumentNumber] nvarchar(200)  NULL,
    [txtStreet] nvarchar(200)  NULL,
    [txtStreetNumber] nvarchar(200)  NULL,
    [txtNeighborhood] nvarchar(200)  NULL,
    [txtZipcode] nvarchar(200)  NULL,
    [txtDdd] nvarchar(200)  NULL,
    [txtNumber] nvarchar(200)  NULL,
    [txtCardHolderName] nvarchar(200)  NULL,
    [txtCardNumber] nvarchar(200)  NULL,
    [txtCardExpirationDate] nvarchar(200)  NULL,
    [txtCardCvv] nvarchar(200)  NULL,
    [txtTransactionId] nvarchar(200)  NULL,
    [txtLinkBoleto] nvarchar(200)  NULL,
    [bitCard] bit  NULL,
    [bitBoleto] bit  NULL,
    [intTipoLog] int  NULL,
    [dteRegistro] datetime  NULL
);
GO

-- Creating table 'tblCommisionLevels'
CREATE TABLE [dbo].[tblCommisionLevels] (
    [intIdComission] int IDENTITY(1,1) NOT NULL,
    [intLevel] int  NOT NULL,
    [intAmount] int  NOT NULL,
    [bitActive] bit  NOT NULL,
    [dteUpdate] datetime  NULL
);
GO

-- Creating table 'tblComissionTokens'
CREATE TABLE [dbo].[tblComissionTokens] (
    [intIdToken] int IDENTITY(1,1) NOT NULL,
    [guidToken] uniqueidentifier  NULL,
    [intIdComissionOrder] int  NULL,
    [intIdCustomerReceiver] int  NULL,
    [intIdAgent] int  NULL,
    [bitComissionConceded] bit  NOT NULL,
    [dteLimit] datetime  NOT NULL,
    [dteCreated] datetime  NOT NULL,
    [dteConceded] datetime  NULL
);
GO

-- Creating table 'tblBonusOptions'
CREATE TABLE [dbo].[tblBonusOptions] (
    [intIdBonus] int IDENTITY(1,1) NOT NULL,
    [intAmount] int  NULL,
    [bitActive] bit  NOT NULL,
    [dteUpdate] datetime  NULL,
    [intPercentBonus] int  NULL
);
GO

-- Creating table 'tblEmailTemplates'
CREATE TABLE [dbo].[tblEmailTemplates] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtTipoTemplate] nvarchar(200)  NOT NULL,
    [txtDescription] varchar(max)  NOT NULL,
    [txtSubject] nvarchar(2000)  NOT NULL,
    [toEmail] varchar(500)  NULL,
    [cc] varchar(500)  NULL,
    [bcc] varchar(500)  NULL,
    [fromEmail] varchar(500)  NULL,
    [bitAtivo] bit  NULL
);
GO

-- Creating table 'tblChargingLog'
CREATE TABLE [dbo].[tblChargingLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtChargingLog] varchar(max)  NULL,
    [dteUpdate] datetime  NULL
);
GO

-- Creating table 'tblPhoneFlags'
CREATE TABLE [dbo].[tblPhoneFlags] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [phone] nvarchar(20)  NULL,
    [usoLinha] bit  NULL,
    [estoque] bit  NULL,
    [operadora] int  NULL,
    [plano] nvarchar(50)  NULL,
    [intIdPlan] int  NULL
);
GO

-- Creating table 'tblPhonePropertyHistory'
CREATE TABLE [dbo].[tblPhonePropertyHistory] (
    [intIdHistory] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [dteChange] datetime  NULL,
    [intIdOperator] int  NULL,
    [intIdPlan] int  NULL,
    [intPlanPrice] float  NULL,
    [intIdPhone] int  NULL,
    [intPhoneNumber] int  NULL,
    [intPhoneDDD] int  NULL,
    [intEventType] int  NULL,
    [intIdStatus] int  NULL,
    [dteEntrada] datetime  NULL,
    [dteSaida] datetime  NULL
);
GO

-- Creating table 'tblPersonAssociationHistory'
CREATE TABLE [dbo].[tblPersonAssociationHistory] (
    [intIdHistory] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [dteChange] datetime  NULL,
    [txtDocument] nvarchar(20)  NULL,
    [intEventType] int  NULL,
    [intIdStatus] int  NULL,
    [dteEntrada] datetime  NULL,
    [dteSaida] datetime  NULL
);
GO

-- Creating table 'CobFullVivo_Sum'
CREATE TABLE [dbo].[CobFullVivo_Sum] (
    [Conta] nvarchar(15)  NULL,
    [Data_Emissão] char(10)  NULL,
    [Telefone] nvarchar(501)  NOT NULL,
    [CentroCusto] nvarchar(60)  NOT NULL,
    [Nome] nvarchar(60)  NOT NULL,
    [CPF_foneclube] nvarchar(15)  NOT NULL,
    [Cliente] nvarchar(60)  NOT NULL,
    [Nickname_foneclube] nvarchar(200)  NOT NULL,
    [Resultado___da_linha] varchar(50)  NULL,
    [C__T__Valor_Total_a_Cobrar_cliente] varchar(50)  NULL,
    [Valor_FC_Total_a_cobrar_da_linha] varchar(50)  NULL,
    [Valor_Total_Cobrado_Vivo__com_desconto_] decimal(18,2)  NULL,
    [Preço_único] decimal(21,2)  NULL,
    [C__T__Valor_Excedente_Total_Cliente_] float  NULL,
    [C__T__Valor_Excedente_Servicos_Cliente] float  NULL,
    [Excedente_Por_linha_FC] float  NULL,
    [C_Sem_uso_] varchar(10)  NULL,
    [C__T__Valor_Excedentes_Minutos_Locais_Cliente] float  NULL,
    [C_T__Minutos_Locais_Cliente] float  NULL,
    [C__T__Excedentes_Minutos_Locais_Cliente] float  NULL,
    [C__T__Franquia_minutos_Local_Cliente_] int  NULL,
    [Preço_FC] float  NULL,
    [Plano_FC] varchar(4)  NULL,
    [Planos_divergentes] varchar(3)  NOT NULL,
    [Plano_FC_cadastrado] varchar(3)  NOT NULL,
    [Plano_Contratado_Vivo] varchar(4)  NULL,
    [Preço_único_fração] float  NULL,
    [Valor_Total_Cobrado_Vivo__sem_desconto_] decimal(38,2)  NULL,
    [Valor_Total_Serviços] varchar(6)  NOT NULL,
    [Valor_Líquido_Serviços] varchar(6)  NOT NULL,
    [Valor_Total_Equipamento] decimal(38,2)  NULL,
    [Plano_Duplicado] varchar(200)  NULL,
    [Franquia_Total] float  NULL,
    [Franquia_Local] float  NULL,
    [Duracao_Total_local] decimal(18,2)  NULL,
    [Excedente_franquia_local] float  NULL,
    [Valor_Excedente_franquia_local] float  NULL,
    [Valor_equalizado_franquia_local] float  NULL,
    [Valor_Cobrado_Local] decimal(38,2)  NOT NULL,
    [Duração_Minutos_Local] decimal(18,2)  NULL,
    [Valor_Unitário_Cobrado_Local] decimal(18,2)  NULL,
    [Duraçao_Min__Loc__Per_Anteriores] decimal(18,2)  NULL,
    [Valor_cobrado_Minutos_Loc__Per_Anteriores] float  NULL,
    [Valor_Cobrado_DDD] decimal(38,2)  NOT NULL,
    [Duração_Minutos_DDD] decimal(18,2)  NULL,
    [Valor_Unitário_Cobrado_DDD] decimal(18,2)  NULL,
    [Duraçao_Min__DDD_Per_Anteriores] decimal(18,2)  NULL,
    [Valor_cobrado_Minutos_DDD_Per_Anteriores] float  NULL,
    [Uso_Intra_Grupo] decimal(18,2)  NULL,
    [Valor_Auditoria_Internet] decimal(38,2)  NULL,
    [Valor_Serviço_Internet] decimal(38,2)  NOT NULL,
    [Valor_Desconto_Internet] decimal(38,2)  NOT NULL,
    [Valor_Líquido_Internet] decimal(38,2)  NULL,
    [Valor_Contratado_Internet] decimal(4,2)  NOT NULL,
    [Quantidade_de_SMS] int  NOT NULL,
    [Valor_Total_Cobrado_SMS] decimal(38,2)  NOT NULL,
    [Valor_Unitário_Cobrado_SMS] decimal(18,2)  NULL,
    [Valor_Unitário_Contratado_SMS] decimal(2,2)  NOT NULL,
    [Valor_Unitário_Auditoria_SMS] decimal(19,2)  NULL,
    [Qtd_SMS_Per_Ant] int  NOT NULL,
    [Valor_SMS_Per_Ant_] float  NULL,
    [Resultado___da_Cliente] varchar(50)  NULL,
    [SMS_Amount] decimal(18,2)  NULL,
    [Report_Month] int  NULL,
    [Report_Year] int  NULL,
    [Creation_Date] datetime  NULL,
    [Payment_Date] datetime  NULL,
    [Expiration_Date] datetime  NULL,
    [Active] varchar(50)  NULL,
    [PDF] varchar(50)  NULL,
    [Portability] varchar(20)  NULL,
    [Plano_Claro_Old] varchar(20)  NULL
);
GO

-- Creating table 'tblPastPagarmeUpdate'
CREATE TABLE [dbo].[tblPastPagarmeUpdate] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [dteUpdate] datetime  NULL
);
GO

-- Creating table 'tblLogBackupCharging'
CREATE TABLE [dbo].[tblLogBackupCharging] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [dteRegister] datetime  NULL,
    [txtLog] varchar(max)  NULL
);
GO

-- Creating table 'tblLogComissionOrder'
CREATE TABLE [dbo].[tblLogComissionOrder] (
    [intIdTipo] int IDENTITY(1,1) NOT NULL,
    [dteRegister] datetime  NOT NULL,
    [bitSucesso] bit  NULL,
    [intTotalOrdensLiberadas] int  NULL
);
GO

-- Creating table 'tblBonusOrderLog'
CREATE TABLE [dbo].[tblBonusOrderLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [dteRegister] datetime  NOT NULL,
    [txtLinhasAptasLiberarBonus] nvarchar(max)  NULL,
    [txtLinhasMalComportamento] nvarchar(max)  NULL,
    [txtLinhasPendentePagamento] nvarchar(max)  NULL,
    [txtLlientesSemPaiQueNaoRecemBonus] nvarchar(max)  NULL,
    [txtOrdensBonusLiberadas] nvarchar(max)  NULL,
    [txtLinhasSemEvento] nvarchar(max)  NULL,
    [txtLinhasComErroAoSalvar] nvarchar(max)  NULL
);
GO

-- Creating table 'tblBonusOrderException'
CREATE TABLE [dbo].[tblBonusOrderException] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [bitAtivo] bit  NOT NULL,
    [dteCreated] datetime  NOT NULL
);
GO

-- Creating table 'tblCommissionOrders'
CREATE TABLE [dbo].[tblCommissionOrders] (
    [intIdComissionOrder] int IDENTITY(1,1) NOT NULL,
    [intIdComission] int  NOT NULL,
    [intIdPerson] int  NOT NULL,
    [intBitActive] bit  NOT NULL,
    [dteCreated] datetime  NOT NULL
);
GO

-- Creating table 'tblComissionValidationLog'
CREATE TABLE [dbo].[tblComissionValidationLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdComission] int  NULL,
    [bitComissionConceded] bit  NULL,
    [bitComissionDispatched] bit  NULL,
    [intIdAgent] int  NULL,
    [dteCreated] datetime  NULL
);
GO

-- Creating table 'tblCieloPaymentLog'
CREATE TABLE [dbo].[tblCieloPaymentLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intCustomerId] int  NULL,
    [intCustomerIdERP] int  NULL,
    [txtOrderId] nvarchar(100)  NULL,
    [txtPaymentId] nvarchar(100)  NULL,
    [intPaymentMethod] int  NULL,
    [intAmount] decimal(18,0)  NULL,
    [txtCurrency] nvarchar(60)  NULL,
    [dtePaymentDate] datetime  NULL,
    [txtPaymentGateway] nvarchar(100)  NULL
);
GO

-- Creating table 'tblEmailLog'
CREATE TABLE [dbo].[tblEmailLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [dteRegister] datetime  NOT NULL,
    [txtEmail] nvarchar(400)  NOT NULL,
    [intIdTypeEmail] int  NULL
);
GO

-- Creating table 'tblPhonesServices'
CREATE TABLE [dbo].[tblPhonesServices] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPhone] int  NULL,
    [intIdService] int  NULL,
    [bitAtivo] bit  NULL,
    [dteUpdate] datetime  NULL,
    [dteAtivacao] datetime  NULL,
    [dteDesativacao] datetime  NULL
);
GO

-- Creating table 'tblServices'
CREATE TABLE [dbo].[tblServices] (
    [intIdService] int  NOT NULL,
    [ServiceDesc] nvarchar(400)  NULL,
    [assinaturas] int  NULL,
    [IsExtraOption] bit  NULL,
    [intValorOperadora] int  NULL,
    [intValorFoneclube] int  NULL,
    [bitEditavel] bit  NULL
);
GO

-- Creating table 'tblBonusOrder'
CREATE TABLE [dbo].[tblBonusOrder] (
    [intIdComissionOrder] int IDENTITY(1,1) NOT NULL,
    [intIdChargingHistory] int  NULL,
    [intIdTransaction] int  NULL,
    [intIdCustomerReceiver] int  NULL,
    [intIdPhone] int  NULL,
    [intIdPhonePlan] int  NULL,
    [intPercentBonus] int  NULL,
    [intIdAgent] int  NULL,
    [bitComissionConceded] bit  NOT NULL,
    [dteValidity] datetime  NULL,
    [dteConceded] datetime  NULL,
    [dteCreated] datetime  NOT NULL,
    [intAmount] int  NULL
);
GO

-- Creating table 'tblCieloPaymentStatus'
CREATE TABLE [dbo].[tblCieloPaymentStatus] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtPaymentId] nvarchar(255)  NULL,
    [txtOrderId] nvarchar(255)  NULL,
    [intAmount] int  NULL,
    [txtType] nvarchar(255)  NULL,
    [intCustomerId] int  NULL,
    [intServiceTaxAmount] int  NULL,
    [intInstallments] int  NULL,
    [txtInterest] nvarchar(60)  NULL,
    [bitCapture] bit  NULL,
    [bitAuthenticate] bit  NULL,
    [txtCardNumber] nvarchar(255)  NULL,
    [txtHolder] nvarchar(255)  NULL,
    [txtExpirationDate] nvarchar(255)  NULL,
    [txtBrand] nvarchar(255)  NULL,
    [bitSaveCard] bit  NULL,
    [txtAuthorizationCode] nvarchar(255)  NULL,
    [txtProofOfSale] nvarchar(255)  NULL,
    [txtCurrency] nvarchar(30)  NULL,
    [txtCountry] nvarchar(30)  NULL,
    [intStatus] int  NULL,
    [dteChargingDate] datetime  NULL,
    [txtDemostrative] nvarchar(400)  NULL,
    [txtUrl] nvarchar(400)  NULL,
    [txtBoletoNumber] nvarchar(50)  NULL,
    [txtBarCodeNumber] nvarchar(100)  NULL,
    [txtDigitableLine] nvarchar(400)  NULL,
    [txtAssignor] nvarchar(400)  NULL,
    [txtAddress] nvarchar(400)  NULL,
    [txtIdentification] nvarchar(50)  NULL,
    [txtInstructions] nvarchar(400)  NULL
);
GO

-- Creating table 'tblComissionOrder'
CREATE TABLE [dbo].[tblComissionOrder] (
    [intIdComissionOrder] int IDENTITY(1,1) NOT NULL,
    [intIdComission] int  NULL,
    [intIdBonus] int  NULL,
    [intIdTransaction] int  NOT NULL,
    [intIdCustomerReceiver] int  NULL,
    [intIdAgent] int  NULL,
    [bitComissionConceded] bit  NOT NULL,
    [dteValidity] datetime  NULL,
    [dteConceded] datetime  NULL,
    [dteCreated] datetime  NOT NULL,
    [intAmount] int  NULL,
    [intTotalLinhas] int  NULL,
    [intIdCustomerGiver] int  NULL,
    [txtIdTransaction] nvarchar(200)  NULL,
    [intIdGateway] int  NULL
);
GO

-- Creating table 'tblWhatsappMessages'
CREATE TABLE [dbo].[tblWhatsappMessages] (
    [intIdMessage] bigint  NOT NULL,
    [intIdClient] bigint  NOT NULL,
    [txtText] nvarchar(max)  NOT NULL,
    [txtType] nvarchar(50)  NOT NULL,
    [txtPdf] nvarchar(500)  NULL,
    [dteCreated] datetime  NOT NULL,
    [txtSendBy] nvarchar(500)  NULL,
    [bitRead] bit  NOT NULL
);
GO

-- Creating table 'tblGenericPhoneFlags'
CREATE TABLE [dbo].[tblGenericPhoneFlags] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPhone] int  NULL,
    [dteRegister] datetime  NULL,
    [dteUpdate] datetime  NULL,
    [bitPendingInteraction] bit  NULL,
    [txtDescription] nvarchar(1000)  NULL,
    [intIdFlag] int  NULL,
    [intIdPerson] int  NULL
);
GO

-- Creating table 'tblGenericFlags'
CREATE TABLE [dbo].[tblGenericFlags] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtFlagDescription] nvarchar(200)  NULL,
    [bitExternalFlag] bit  NULL,
    [bitPhoneFlag] bit  NULL,
    [intIdEmail] int  NULL
);
GO

-- Creating table 'CobFullClaroMax_Sum'
CREATE TABLE [dbo].[CobFullClaroMax_Sum] (
    [Conta] nvarchar(20)  NULL,
    [Mês] nvarchar(10)  NULL,
    [Telefone] varchar(20)  NULL,
    [Nome] nvarchar(60)  NULL,
    [Nickname] nvarchar(200)  NULL,
    [CPF_Foneclube] varchar(20)  NULL,
    [Plano_Foneclube] varchar(20)  NULL,
    [Plano_Contratado] varchar(20)  NULL,
    [Preço_único] float  NULL,
    [Fração_preço_único_por_linha] float  NULL,
    [Preço_Plano_FC] decimal(36,2)  NULL,
    [Sem_uso] int  NOT NULL,
    [Valor_cobrado_operadora] float  NULL,
    [Valor_total_com_excedentes_FC] float  NULL,
    [Resultado___linha] float  NULL,
    [Franquia_minutos] int  NULL,
    [Total_franquia_minutos] int  NULL,
    [Minutos_locais_excedentes] int  NULL,
    [C_Total_min__lig__Local_] int  NULL,
    [C_Total_min__lig__Local_Tarifa_Zero_] int  NULL,
    [C_Total_minutos_lig__p_números_especiais_] decimal(18,2)  NULL,
    [C_Total_ligações_Interurbanas_] int  NULL,
    [C_Total_ligações_no_exterior_] int  NULL,
    [Separador] varchar(1)  NOT NULL,
    [Valor_Ligações_Locais_oper_] float  NULL,
    [Liga__es_para_n_meros_especiais] float  NULL,
    [Servi_os__Torpedos__Hits__Jogos__etc__] float  NULL,
    [Interurbanas_e_Rec__em_viagem_21___Embratel] float  NULL,
    [Interurbanas_e_Rec__em_viagem_31___Telemar] float  NULL,
    [Liga__es_e_Servi_os_no_exterior] float  NULL,
    [Outros] float  NULL,
    [Multa] float  NULL,
    [Erro_valor_internet_operadora] varchar(1)  NOT NULL,
    [Plano_internet_desconhecido] varchar(1)  NOT NULL,
    [Mensalidades_e_Pacotes_Promocionais] float  NULL,
    [Valor_Total_a_Cobrar_do_Cliente] float  NULL,
    [Valor_Total_a_Cobrar_da_Linha_Equalizado] float  NULL,
    [Report_Month] int  NULL,
    [Report_Year] int  NULL,
    [Creation_Date] datetime  NULL,
    [Payment_Date] datetime  NULL,
    [Expiration_Date] datetime  NULL,
    [MasterPlan] varchar(50)  NULL,
    [C_TM__DDD] decimal(18,2)  NULL,
    [Franquia_DDD] decimal(18,2)  NULL,
    [Franquia_Local] decimal(18,2)  NULL,
    [C_TM__Excedente_DDD] decimal(18,2)  NULL,
    [C_TM__Locais_Excedentes] decimal(18,2)  NULL,
    [Total_____Correto_Ligaçoes_DDD] decimal(18,2)  NULL,
    [Total_____Correto_Ligaçoes_Locais] decimal(18,2)  NULL,
    [C_T_Excedentes] decimal(18,2)  NULL,
    [ContaID] int  NULL,
    [Active] varchar(50)  NULL,
    [PDF] varchar(50)  NULL,
    [PreçoVip] decimal(18,2)  NULL,
    [C__T__Cobrar_Linha] float  NULL,
    [Assin_Contrato] decimal(18,2)  NULL,
    [Portability] varchar(20)  NULL,
    [Plano_Vivo_Old] varchar(20)  NULL
);
GO

-- Creating table 'tblTransactionsCielo'
CREATE TABLE [dbo].[tblTransactionsCielo] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intHistoryId] int  NOT NULL,
    [intPersonId] int  NULL,
    [txtPaymentId] nvarchar(300)  NOT NULL,
    [intValor] int  NULL,
    [intGatewayId] int  NOT NULL,
    [dteRegister] datetime  NULL,
    [intStatusPayment] int  NULL
);
GO

-- Creating table 'tblMassChargingLog'
CREATE TABLE [dbo].[tblMassChargingLog] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [dteRegistro] datetime  NULL,
    [txtMassCharging] nvarchar(max)  NOT NULL,
    [bitCharged] bit  NULL,
    [intIdMassCharging] int  NULL
);
GO

-- Creating table 'tblPhoneStock'
CREATE TABLE [dbo].[tblPhoneStock] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtNumero] nvarchar(200)  NULL,
    [txtCNPJ] nvarchar(200)  NULL,
    [txtRazaoSocial] nvarchar(200)  NULL,
    [txtCodCliente] nvarchar(200)  NULL,
    [txtICCID] nvarchar(200)  NULL,
    [txtNumeroTemporario] nvarchar(200)  NULL,
    [dteRegistroEntrada] datetime  NULL,
    [dteRegistroSaida] datetime  NULL,
    [dteReferencia] datetime  NULL,
    [txtCodigoFatura] nvarchar(200)  NULL
);
GO

-- Creating table 'tblRegisterCustomersLog'
CREATE TABLE [dbo].[tblRegisterCustomersLog] (
    [intIdCustomer] int IDENTITY(1,1) NOT NULL,
    [dteRegister] datetime  NOT NULL,
    [guidId] uniqueidentifier  NOT NULL,
    [txtPhone] nvarchar(200)  NULL,
    [txtName] nvarchar(500)  NOT NULL,
    [txtEmail] nvarchar(500)  NULL,
    [txtDocument] nvarchar(500)  NULL,
    [intDocumentType] int  NULL,
    [txtPassword] nvarchar(500)  NULL,
    [idPai] int  NULL,
    [txtLog] nvarchar(500)  NULL
);
GO

-- Creating table 'tblPersosAffiliateLinks'
CREATE TABLE [dbo].[tblPersosAffiliateLinks] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtMaskName] nvarchar(500)  NULL,
    [txtOriginalLink] nvarchar(500)  NULL,
    [txtBlinkLink] nvarchar(500)  NULL,
    [dteRegister] datetime  NOT NULL
);
GO

-- Creating table 'tblPersonsAddresses'
CREATE TABLE [dbo].[tblPersonsAddresses] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtStreet] nvarchar(60)  NULL,
    [txtComplement] nvarchar(60)  NULL,
    [intStreetNumber] int  NULL,
    [txtNeighborhood] nvarchar(60)  NULL,
    [txtCity] nvarchar(60)  NULL,
    [txtState] nvarchar(20)  NULL,
    [txtCep] nvarchar(20)  NOT NULL,
    [txtCountry] nvarchar(20)  NULL,
    [intAdressType] int  NULL
);
GO

-- Creating table 'tblChargingHistory'
CREATE TABLE [dbo].[tblChargingHistory] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdCustomer] int  NULL,
    [intIdCollector] int  NULL,
    [txtCollectorName] nvarchar(40)  NULL,
    [intIdPaymentType] int  NULL,
    [txtComment] varchar(8000)  NULL,
    [dtePayment] datetime  NOT NULL,
    [txtAmmountPayment] nvarchar(40)  NOT NULL,
    [txtTokenTransaction] varchar(500)  NULL,
    [intIdBoleto] bigint  NULL,
    [txtAcquireId] nvarchar(40)  NULL,
    [dteCreate] datetime  NULL,
    [dteModify] datetime  NULL,
    [intPaymentStatus] int  NULL,
    [txtTransactionComment] varchar(8000)  NULL,
    [dteDueDate] datetime  NULL,
    [PhoneId] int  NULL,
    [dteChargingDate] datetime  NULL,
    [txtChargingComment] varchar(8000)  NULL,
    [validityMonth] int  NULL,
    [validityYear] int  NULL,
    [dteValidity] datetime  NULL,
    [intChargeStatusId] int  NULL,
    [dteCharge] datetime  NULL,
    [intIdTransaction] int  NULL,
    [bitComissionConceded] bit  NULL,
    [bitCash] bit  NULL,
    [txtCommentEmail] varchar(8000)  NULL,
    [txtCommentBoleto] varchar(8000)  NULL,
    [bitCanceled] bit  NULL,
    [txtPaymentId] nvarchar(100)  NULL,
    [intIdGateway] int  NULL,
    [bitComissionInapt] bit  NULL,
    [txtCartHash] varchar(1000)  NULL,
    [bitPago] bit  NULL,
    [txtStatusPaymentLoja] varchar(400)  NULL,
    [intIdCheckoutLoja] int  NULL
);
GO

-- Creating table 'tblPersons'
CREATE TABLE [dbo].[tblPersons] (
    [intIdPerson] int IDENTITY(1,1) NOT NULL,
    [intContactId] int  NULL,
    [dteRegister] datetime  NOT NULL,
    [txtDocumentNumber] nvarchar(15)  NOT NULL,
    [txtName] nvarchar(60)  NOT NULL,
    [txtNickName] nvarchar(60)  NULL,
    [txtEmail] nvarchar(200)  NULL,
    [dteBorn] datetime  NULL,
    [intGender] int  NULL,
    [intIdPagarme] int  NULL,
    [intIdRole] int  NULL,
    [intIdCurrentOperator] int  NULL,
    [matricula] bigint  NULL,
    [bitManual] bit  NULL,
    [bitDelete] bit  NULL,
    [bitDesativoManual] bit  NULL,
    [intIdLoja] int  NULL,
    [txtPassword] nvarchar(200)  NULL,
    [bitDadosPessoaisCadastrados] bit  NULL,
    [bitSenhaCadastrada] bit  NULL
);
GO

-- Creating table 'tblPersonsPhones'
CREATE TABLE [dbo].[tblPersonsPhones] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [intDDD] int  NULL,
    [intPhone] bigint  NULL,
    [intIdOperator] int  NULL,
    [bitPhoneClube] bit  NULL,
    [intIdPlan] int  NULL,
    [txtNickname] nvarchar(200)  NULL,
    [bitPortability] bit  NULL,
    [bitAtivo] bit  NULL,
    [dteEntradaLinha] datetime  NULL,
    [dteSaidaLinha] datetime  NULL,
    [bitDono] bit  NULL,
    [intIdStatus] int  NULL,
    [intAmmoutPrecoVip] int  NULL,
    [bitPrecoVip] bit  NULL,
    [bitDelete] bit  NULL,
    [bitBonusConceded] bit  NULL,
    [intCountryCode] int  NULL,
    [intContactType] int  NULL,
    [bitAmigos] bit  NULL,
    [bitEsim] bit  NULL
);
GO

-- Creating table 'tblPhonesStockProperty'
CREATE TABLE [dbo].[tblPhonesStockProperty] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [txtPhone] nvarchar(200)  NULL,
    [txtProperty] nvarchar(200)  NULL,
    [intIdProperty] int  NULL,
    [intIdOperadora] int  NULL,
    [txtDescricaoOperadora] nvarchar(200)  NULL,
    [dteRegister] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [intIdComission] in table 'tblCommissions'
ALTER TABLE [dbo].[tblCommissions]
ADD CONSTRAINT [PK_tblCommissions]
    PRIMARY KEY CLUSTERED ([intIdComission] ASC);
GO

-- Creating primary key on [intId] in table 'tblLog'
ALTER TABLE [dbo].[tblLog]
ADD CONSTRAINT [PK_tblLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdTipo] in table 'tblLogTipo'
ALTER TABLE [dbo].[tblLogTipo]
ADD CONSTRAINT [PK_tblLogTipo]
    PRIMARY KEY CLUSTERED ([intIdTipo] ASC);
GO

-- Creating primary key on [intIdOperator] in table 'tblOperadoras'
ALTER TABLE [dbo].[tblOperadoras]
ADD CONSTRAINT [PK_tblOperadoras]
    PRIMARY KEY CLUSTERED ([intIdOperator] ASC);
GO

-- Creating primary key on [intId] in table 'tblPaymentTypes'
ALTER TABLE [dbo].[tblPaymentTypes]
ADD CONSTRAINT [PK_tblPaymentTypes]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdPlanDetail] in table 'tblPlanDetails'
ALTER TABLE [dbo].[tblPlanDetails]
ADD CONSTRAINT [PK_tblPlanDetails]
    PRIMARY KEY CLUSTERED ([intIdPlanDetail] ASC);
GO

-- Creating primary key on [intIdDetailType] in table 'tblPlanDetailType'
ALTER TABLE [dbo].[tblPlanDetailType]
ADD CONSTRAINT [PK_tblPlanDetailType]
    PRIMARY KEY CLUSTERED ([intIdDetailType] ASC);
GO

-- Creating primary key on [intIdPlan] in table 'tblPlans'
ALTER TABLE [dbo].[tblPlans]
ADD CONSTRAINT [PK_tblPlans]
    PRIMARY KEY CLUSTERED ([intIdPlan] ASC);
GO

-- Creating primary key on [intIdPlan] in table 'tblPlansOptions'
ALTER TABLE [dbo].[tblPlansOptions]
ADD CONSTRAINT [PK_tblPlansOptions]
    PRIMARY KEY CLUSTERED ([intIdPlan] ASC);
GO

-- Creating primary key on [intId] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [PK_tblReferred]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblBilhetacoesFebraban'
ALTER TABLE [dbo].[tblBilhetacoesFebraban]
ADD CONSTRAINT [PK_tblBilhetacoesFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intContaId] in table 'tblContasFebraban'
ALTER TABLE [dbo].[tblContasFebraban]
ADD CONSTRAINT [PK_tblContasFebraban]
    PRIMARY KEY CLUSTERED ([intContaId] ASC);
GO

-- Creating primary key on [intId] in table 'tblDescontosFebraban'
ALTER TABLE [dbo].[tblDescontosFebraban]
ADD CONSTRAINT [PK_tblDescontosFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblEnderecosFebraban'
ALTER TABLE [dbo].[tblEnderecosFebraban]
ADD CONSTRAINT [PK_tblEnderecosFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblHeadsFebraban'
ALTER TABLE [dbo].[tblHeadsFebraban]
ADD CONSTRAINT [PK_tblHeadsFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblResumosFebraban'
ALTER TABLE [dbo].[tblResumosFebraban]
ADD CONSTRAINT [PK_tblResumosFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblServicosFebraban'
ALTER TABLE [dbo].[tblServicosFebraban]
ADD CONSTRAINT [PK_tblServicosFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblTotalizadoresFebraban'
ALTER TABLE [dbo].[tblTotalizadoresFebraban]
ADD CONSTRAINT [PK_tblTotalizadoresFebraban]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblDiscountPrice'
ALTER TABLE [dbo].[tblDiscountPrice]
ADD CONSTRAINT [PK_tblDiscountPrice]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsImages'
ALTER TABLE [dbo].[tblPersonsImages]
ADD CONSTRAINT [PK_tblPersonsImages]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblLineRecord'
ALTER TABLE [dbo].[tblLineRecord]
ADD CONSTRAINT [PK_tblLineRecord]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsParents'
ALTER TABLE [dbo].[tblPersonsParents]
ADD CONSTRAINT [PK_tblPersonsParents]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdServiceOrder] in table 'tblServiceOrders'
ALTER TABLE [dbo].[tblServiceOrders]
ADD CONSTRAINT [PK_tblServiceOrders]
    PRIMARY KEY CLUSTERED ([intIdServiceOrder] ASC);
GO

-- Creating primary key on [Nickname], [Sem_uso], [Separador], [Erro_valor_internet_operadora], [Plano_internet_desconhecido] in table 'CobFullClaro_Sum'
ALTER TABLE [dbo].[CobFullClaro_Sum]
ADD CONSTRAINT [PK_CobFullClaro_Sum]
    PRIMARY KEY CLUSTERED ([Nickname], [Sem_uso], [Separador], [Erro_valor_internet_operadora], [Plano_internet_desconhecido] ASC);
GO

-- Creating primary key on [intIdEvent] in table 'tblHistoryEvents'
ALTER TABLE [dbo].[tblHistoryEvents]
ADD CONSTRAINT [PK_tblHistoryEvents]
    PRIMARY KEY CLUSTERED ([intIdEvent] ASC);
GO

-- Creating primary key on [intIdStatus] in table 'tblPhoneStatus'
ALTER TABLE [dbo].[tblPhoneStatus]
ADD CONSTRAINT [PK_tblPhoneStatus]
    PRIMARY KEY CLUSTERED ([intIdStatus] ASC);
GO

-- Creating primary key on [intIdPhone] in table 'tblPhones'
ALTER TABLE [dbo].[tblPhones]
ADD CONSTRAINT [PK_tblPhones]
    PRIMARY KEY CLUSTERED ([intIdPhone] ASC);
GO

-- Creating primary key on [intId] in table 'tblFoneclubePagarmeTransactions'
ALTER TABLE [dbo].[tblFoneclubePagarmeTransactions]
ADD CONSTRAINT [PK_tblFoneclubePagarmeTransactions]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intCheckoutLogId] in table 'tblCheckoutPagarMeLog'
ALTER TABLE [dbo].[tblCheckoutPagarMeLog]
ADD CONSTRAINT [PK_tblCheckoutPagarMeLog]
    PRIMARY KEY CLUSTERED ([intCheckoutLogId] ASC);
GO

-- Creating primary key on [intIdComission] in table 'tblCommisionLevels'
ALTER TABLE [dbo].[tblCommisionLevels]
ADD CONSTRAINT [PK_tblCommisionLevels]
    PRIMARY KEY CLUSTERED ([intIdComission] ASC);
GO

-- Creating primary key on [intIdToken] in table 'tblComissionTokens'
ALTER TABLE [dbo].[tblComissionTokens]
ADD CONSTRAINT [PK_tblComissionTokens]
    PRIMARY KEY CLUSTERED ([intIdToken] ASC);
GO

-- Creating primary key on [intIdBonus] in table 'tblBonusOptions'
ALTER TABLE [dbo].[tblBonusOptions]
ADD CONSTRAINT [PK_tblBonusOptions]
    PRIMARY KEY CLUSTERED ([intIdBonus] ASC);
GO

-- Creating primary key on [intId] in table 'tblEmailTemplates'
ALTER TABLE [dbo].[tblEmailTemplates]
ADD CONSTRAINT [PK_tblEmailTemplates]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblChargingLog'
ALTER TABLE [dbo].[tblChargingLog]
ADD CONSTRAINT [PK_tblChargingLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPhoneFlags'
ALTER TABLE [dbo].[tblPhoneFlags]
ADD CONSTRAINT [PK_tblPhoneFlags]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdHistory] in table 'tblPhonePropertyHistory'
ALTER TABLE [dbo].[tblPhonePropertyHistory]
ADD CONSTRAINT [PK_tblPhonePropertyHistory]
    PRIMARY KEY CLUSTERED ([intIdHistory] ASC);
GO

-- Creating primary key on [intIdHistory] in table 'tblPersonAssociationHistory'
ALTER TABLE [dbo].[tblPersonAssociationHistory]
ADD CONSTRAINT [PK_tblPersonAssociationHistory]
    PRIMARY KEY CLUSTERED ([intIdHistory] ASC);
GO

-- Creating primary key on [Telefone], [CentroCusto], [Nome], [CPF_foneclube], [Cliente], [Nickname_foneclube], [Planos_divergentes], [Plano_FC_cadastrado], [Valor_Total_Serviços], [Valor_Líquido_Serviços], [Valor_Cobrado_Local], [Valor_Cobrado_DDD], [Valor_Serviço_Internet], [Valor_Desconto_Internet], [Valor_Contratado_Internet], [Quantidade_de_SMS], [Valor_Total_Cobrado_SMS], [Valor_Unitário_Contratado_SMS], [Qtd_SMS_Per_Ant] in table 'CobFullVivo_Sum'
ALTER TABLE [dbo].[CobFullVivo_Sum]
ADD CONSTRAINT [PK_CobFullVivo_Sum]
    PRIMARY KEY CLUSTERED ([Telefone], [CentroCusto], [Nome], [CPF_foneclube], [Cliente], [Nickname_foneclube], [Planos_divergentes], [Plano_FC_cadastrado], [Valor_Total_Serviços], [Valor_Líquido_Serviços], [Valor_Cobrado_Local], [Valor_Cobrado_DDD], [Valor_Serviço_Internet], [Valor_Desconto_Internet], [Valor_Contratado_Internet], [Quantidade_de_SMS], [Valor_Total_Cobrado_SMS], [Valor_Unitário_Contratado_SMS], [Qtd_SMS_Per_Ant] ASC);
GO

-- Creating primary key on [intId] in table 'tblPastPagarmeUpdate'
ALTER TABLE [dbo].[tblPastPagarmeUpdate]
ADD CONSTRAINT [PK_tblPastPagarmeUpdate]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblLogBackupCharging'
ALTER TABLE [dbo].[tblLogBackupCharging]
ADD CONSTRAINT [PK_tblLogBackupCharging]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdTipo] in table 'tblLogComissionOrder'
ALTER TABLE [dbo].[tblLogComissionOrder]
ADD CONSTRAINT [PK_tblLogComissionOrder]
    PRIMARY KEY CLUSTERED ([intIdTipo] ASC);
GO

-- Creating primary key on [intId] in table 'tblBonusOrderLog'
ALTER TABLE [dbo].[tblBonusOrderLog]
ADD CONSTRAINT [PK_tblBonusOrderLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblBonusOrderException'
ALTER TABLE [dbo].[tblBonusOrderException]
ADD CONSTRAINT [PK_tblBonusOrderException]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdComissionOrder] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [PK_tblCommissionOrders]
    PRIMARY KEY CLUSTERED ([intIdComissionOrder] ASC);
GO

-- Creating primary key on [intId] in table 'tblComissionValidationLog'
ALTER TABLE [dbo].[tblComissionValidationLog]
ADD CONSTRAINT [PK_tblComissionValidationLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblCieloPaymentLog'
ALTER TABLE [dbo].[tblCieloPaymentLog]
ADD CONSTRAINT [PK_tblCieloPaymentLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblEmailLog'
ALTER TABLE [dbo].[tblEmailLog]
ADD CONSTRAINT [PK_tblEmailLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPhonesServices'
ALTER TABLE [dbo].[tblPhonesServices]
ADD CONSTRAINT [PK_tblPhonesServices]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdService] in table 'tblServices'
ALTER TABLE [dbo].[tblServices]
ADD CONSTRAINT [PK_tblServices]
    PRIMARY KEY CLUSTERED ([intIdService] ASC);
GO

-- Creating primary key on [intIdComissionOrder] in table 'tblBonusOrder'
ALTER TABLE [dbo].[tblBonusOrder]
ADD CONSTRAINT [PK_tblBonusOrder]
    PRIMARY KEY CLUSTERED ([intIdComissionOrder] ASC);
GO

-- Creating primary key on [intId] in table 'tblCieloPaymentStatus'
ALTER TABLE [dbo].[tblCieloPaymentStatus]
ADD CONSTRAINT [PK_tblCieloPaymentStatus]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdComissionOrder] in table 'tblComissionOrder'
ALTER TABLE [dbo].[tblComissionOrder]
ADD CONSTRAINT [PK_tblComissionOrder]
    PRIMARY KEY CLUSTERED ([intIdComissionOrder] ASC);
GO

-- Creating primary key on [intIdMessage] in table 'tblWhatsappMessages'
ALTER TABLE [dbo].[tblWhatsappMessages]
ADD CONSTRAINT [PK_tblWhatsappMessages]
    PRIMARY KEY CLUSTERED ([intIdMessage] ASC);
GO

-- Creating primary key on [intId] in table 'tblGenericPhoneFlags'
ALTER TABLE [dbo].[tblGenericPhoneFlags]
ADD CONSTRAINT [PK_tblGenericPhoneFlags]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblGenericFlags'
ALTER TABLE [dbo].[tblGenericFlags]
ADD CONSTRAINT [PK_tblGenericFlags]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [Sem_uso], [Separador], [Erro_valor_internet_operadora], [Plano_internet_desconhecido] in table 'CobFullClaroMax_Sum'
ALTER TABLE [dbo].[CobFullClaroMax_Sum]
ADD CONSTRAINT [PK_CobFullClaroMax_Sum]
    PRIMARY KEY CLUSTERED ([Sem_uso], [Separador], [Erro_valor_internet_operadora], [Plano_internet_desconhecido] ASC);
GO

-- Creating primary key on [intId] in table 'tblTransactionsCielo'
ALTER TABLE [dbo].[tblTransactionsCielo]
ADD CONSTRAINT [PK_tblTransactionsCielo]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblMassChargingLog'
ALTER TABLE [dbo].[tblMassChargingLog]
ADD CONSTRAINT [PK_tblMassChargingLog]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPhoneStock'
ALTER TABLE [dbo].[tblPhoneStock]
ADD CONSTRAINT [PK_tblPhoneStock]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdCustomer] in table 'tblRegisterCustomersLog'
ALTER TABLE [dbo].[tblRegisterCustomersLog]
ADD CONSTRAINT [PK_tblRegisterCustomersLog]
    PRIMARY KEY CLUSTERED ([intIdCustomer] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersosAffiliateLinks'
ALTER TABLE [dbo].[tblPersosAffiliateLinks]
ADD CONSTRAINT [PK_tblPersosAffiliateLinks]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsAddresses'
ALTER TABLE [dbo].[tblPersonsAddresses]
ADD CONSTRAINT [PK_tblPersonsAddresses]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [PK_tblChargingHistory]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdPerson] in table 'tblPersons'
ALTER TABLE [dbo].[tblPersons]
ADD CONSTRAINT [PK_tblPersons]
    PRIMARY KEY CLUSTERED ([intIdPerson] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsPhones'
ALTER TABLE [dbo].[tblPersonsPhones]
ADD CONSTRAINT [PK_tblPersonsPhones]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPhonesStockProperty'
ALTER TABLE [dbo].[tblPhonesStockProperty]
ADD CONSTRAINT [PK_tblPhonesStockProperty]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [intIdComission] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [FK__tblReferr__intId__50C5FA01]
    FOREIGN KEY ([intIdComission])
    REFERENCES [dbo].[tblCommissions]
        ([intIdComission])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblReferr__intId__50C5FA01'
CREATE INDEX [IX_FK__tblReferr__intId__50C5FA01]
ON [dbo].[tblReferred]
    ([intIdComission]);
GO

-- Creating foreign key on [intIdTipo] in table 'tblLog'
ALTER TABLE [dbo].[tblLog]
ADD CONSTRAINT [FK__tblLog__intIdTip__5165187F]
    FOREIGN KEY ([intIdTipo])
    REFERENCES [dbo].[tblLogTipo]
        ([intIdTipo])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblLog__intIdTip__5165187F'
CREATE INDEX [IX_FK__tblLog__intIdTip__5165187F]
ON [dbo].[tblLog]
    ([intIdTipo]);
GO

-- Creating foreign key on [intIdOperator] in table 'tblPlansOptions'
ALTER TABLE [dbo].[tblPlansOptions]
ADD CONSTRAINT [FK__tblPlansO__intId__361203C5]
    FOREIGN KEY ([intIdOperator])
    REFERENCES [dbo].[tblOperadoras]
        ([intIdOperator])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPlansO__intId__361203C5'
CREATE INDEX [IX_FK__tblPlansO__intId__361203C5]
ON [dbo].[tblPlansOptions]
    ([intIdOperator]);
GO

-- Creating foreign key on [intIdOption] in table 'tblPlans'
ALTER TABLE [dbo].[tblPlans]
ADD CONSTRAINT [FK__tblPlans__intIdO__38EE7070]
    FOREIGN KEY ([intIdOption])
    REFERENCES [dbo].[tblPlansOptions]
        ([intIdPlan])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPlans__intIdO__38EE7070'
CREATE INDEX [IX_FK__tblPlans__intIdO__38EE7070]
ON [dbo].[tblPlans]
    ([intIdOption]);
GO

-- Creating foreign key on [intContaId] in table 'tblBilhetacoesFebraban'
ALTER TABLE [dbo].[tblBilhetacoesFebraban]
ADD CONSTRAINT [FK__tblBilhet__intCo__186C9245]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblBilhet__intCo__186C9245'
CREATE INDEX [IX_FK__tblBilhet__intCo__186C9245]
ON [dbo].[tblBilhetacoesFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblDescontosFebraban'
ALTER TABLE [dbo].[tblDescontosFebraban]
ADD CONSTRAINT [FK__tblDescon__intCo__1E256B9B]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblDescon__intCo__1E256B9B'
CREATE INDEX [IX_FK__tblDescon__intCo__1E256B9B]
ON [dbo].[tblDescontosFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblEnderecosFebraban'
ALTER TABLE [dbo].[tblEnderecosFebraban]
ADD CONSTRAINT [FK__tblEndere__intCo__1590259A]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblEndere__intCo__1590259A'
CREATE INDEX [IX_FK__tblEndere__intCo__1590259A]
ON [dbo].[tblEnderecosFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblHeadsFebraban'
ALTER TABLE [dbo].[tblHeadsFebraban]
ADD CONSTRAINT [FK__tblHeadsF__intCo__0FD74C44]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblHeadsF__intCo__0FD74C44'
CREATE INDEX [IX_FK__tblHeadsF__intCo__0FD74C44]
ON [dbo].[tblHeadsFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblResumosFebraban'
ALTER TABLE [dbo].[tblResumosFebraban]
ADD CONSTRAINT [FK__tblResumo__intCo__12B3B8EF]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblResumo__intCo__12B3B8EF'
CREATE INDEX [IX_FK__tblResumo__intCo__12B3B8EF]
ON [dbo].[tblResumosFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblServicosFebraban'
ALTER TABLE [dbo].[tblServicosFebraban]
ADD CONSTRAINT [FK__tblServic__intCo__1B48FEF0]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblServic__intCo__1B48FEF0'
CREATE INDEX [IX_FK__tblServic__intCo__1B48FEF0]
ON [dbo].[tblServicosFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intContaId] in table 'tblTotalizadoresFebraban'
ALTER TABLE [dbo].[tblTotalizadoresFebraban]
ADD CONSTRAINT [FK__tblTotali__intCo__2101D846]
    FOREIGN KEY ([intContaId])
    REFERENCES [dbo].[tblContasFebraban]
        ([intContaId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblTotali__intCo__2101D846'
CREATE INDEX [IX_FK__tblTotali__intCo__2101D846]
ON [dbo].[tblTotalizadoresFebraban]
    ([intContaId]);
GO

-- Creating foreign key on [intIdComission] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [FK__tblCommis__intId__0A9D95DB]
    FOREIGN KEY ([intIdComission])
    REFERENCES [dbo].[tblCommissions]
        ([intIdComission])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblCommis__intId__0A9D95DB'
CREATE INDEX [IX_FK__tblCommis__intId__0A9D95DB]
ON [dbo].[tblCommissionOrders]
    ([intIdComission]);
GO

-- Creating foreign key on [intIdService] in table 'tblPhonesServices'
ALTER TABLE [dbo].[tblPhonesServices]
ADD CONSTRAINT [FK__tblPhones__intId__3A02903A]
    FOREIGN KEY ([intIdService])
    REFERENCES [dbo].[tblServices]
        ([intIdService])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPhones__intId__3A02903A'
CREATE INDEX [IX_FK__tblPhones__intId__3A02903A]
ON [dbo].[tblPhonesServices]
    ([intIdService]);
GO

-- Creating foreign key on [intIdBonus] in table 'tblComissionOrder'
ALTER TABLE [dbo].[tblComissionOrder]
ADD CONSTRAINT [FK__tblComiss__intId__12C8C788]
    FOREIGN KEY ([intIdBonus])
    REFERENCES [dbo].[tblBonusOptions]
        ([intIdBonus])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblComiss__intId__12C8C788'
CREATE INDEX [IX_FK__tblComiss__intId__12C8C788]
ON [dbo].[tblComissionOrder]
    ([intIdBonus]);
GO

-- Creating foreign key on [intIdComission] in table 'tblComissionOrder'
ALTER TABLE [dbo].[tblComissionOrder]
ADD CONSTRAINT [FK__tblComiss__intId__11D4A34F]
    FOREIGN KEY ([intIdComission])
    REFERENCES [dbo].[tblCommisionLevels]
        ([intIdComission])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblComiss__intId__11D4A34F'
CREATE INDEX [IX_FK__tblComiss__intId__11D4A34F]
ON [dbo].[tblComissionOrder]
    ([intIdComission]);
GO

-- Creating foreign key on [intIdComissionOrder] in table 'tblComissionTokens'
ALTER TABLE [dbo].[tblComissionTokens]
ADD CONSTRAINT [FK__tblComiss__intId__1699586C]
    FOREIGN KEY ([intIdComissionOrder])
    REFERENCES [dbo].[tblComissionOrder]
        ([intIdComissionOrder])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblComiss__intId__1699586C'
CREATE INDEX [IX_FK__tblComiss__intId__1699586C]
ON [dbo].[tblComissionTokens]
    ([intIdComissionOrder]);
GO

-- Creating foreign key on [intIdFlag] in table 'tblGenericPhoneFlags'
ALTER TABLE [dbo].[tblGenericPhoneFlags]
ADD CONSTRAINT [FK__tblGeneri__intId__04659998]
    FOREIGN KEY ([intIdFlag])
    REFERENCES [dbo].[tblGenericFlags]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblGeneri__intId__04659998'
CREATE INDEX [IX_FK__tblGeneri__intId__04659998]
ON [dbo].[tblGenericPhoneFlags]
    ([intIdFlag]);
GO

-- Creating foreign key on [intIdPaymentType] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [FK__tblChargi__intId__08B54D69]
    FOREIGN KEY ([intIdPaymentType])
    REFERENCES [dbo].[tblPaymentTypes]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblChargi__intId__08B54D69'
CREATE INDEX [IX_FK__tblChargi__intId__08B54D69]
ON [dbo].[tblChargingHistory]
    ([intIdPaymentType]);
GO

-- Creating foreign key on [intIdCustomer] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [FK__tblChargi__intId__07C12930]
    FOREIGN KEY ([intIdCustomer])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblChargi__intId__07C12930'
CREATE INDEX [IX_FK__tblChargi__intId__07C12930]
ON [dbo].[tblChargingHistory]
    ([intIdCustomer]);
GO

-- Creating foreign key on [intIdCustomerReceiver] in table 'tblComissionOrder'
ALTER TABLE [dbo].[tblComissionOrder]
ADD CONSTRAINT [FK__tblComiss__intId__13BCEBC1]
    FOREIGN KEY ([intIdCustomerReceiver])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblComiss__intId__13BCEBC1'
CREATE INDEX [IX_FK__tblComiss__intId__13BCEBC1]
ON [dbo].[tblComissionOrder]
    ([intIdCustomerReceiver]);
GO

-- Creating foreign key on [intIdCustomerReceiver] in table 'tblComissionTokens'
ALTER TABLE [dbo].[tblComissionTokens]
ADD CONSTRAINT [FK__tblComiss__intId__178D7CA5]
    FOREIGN KEY ([intIdCustomerReceiver])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblComiss__intId__178D7CA5'
CREATE INDEX [IX_FK__tblComiss__intId__178D7CA5]
ON [dbo].[tblComissionTokens]
    ([intIdCustomerReceiver]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [FK__tblCommis__intId__0B91BA14]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblCommis__intId__0B91BA14'
CREATE INDEX [IX_FK__tblCommis__intId__0B91BA14]
ON [dbo].[tblCommissionOrders]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblDiscountPrice'
ALTER TABLE [dbo].[tblDiscountPrice]
ADD CONSTRAINT [FK__tblDiscou__intId__0F624AF8]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblDiscou__intId__0F624AF8'
CREATE INDEX [IX_FK__tblDiscou__intId__0F624AF8]
ON [dbo].[tblDiscountPrice]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblEmailLog'
ALTER TABLE [dbo].[tblEmailLog]
ADD CONSTRAINT [FK__tblEmailL__intId__12E8C319]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblEmailL__intId__12E8C319'
CREATE INDEX [IX_FK__tblEmailL__intId__12E8C319]
ON [dbo].[tblEmailLog]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblLineRecord'
ALTER TABLE [dbo].[tblLineRecord]
ADD CONSTRAINT [FK__tblLineRe__intId__1332DBDC]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblLineRe__intId__1332DBDC'
CREATE INDEX [IX_FK__tblLineRe__intId__1332DBDC]
ON [dbo].[tblLineRecord]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPersonsAddresses'
ALTER TABLE [dbo].[tblPersonsAddresses]
ADD CONSTRAINT [FK__tblPerson__intId__160F4887]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__160F4887'
CREATE INDEX [IX_FK__tblPerson__intId__160F4887]
ON [dbo].[tblPersonsAddresses]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPersonsImages'
ALTER TABLE [dbo].[tblPersonsImages]
ADD CONSTRAINT [FK__tblPerson__intId__17036CC0]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__17036CC0'
CREATE INDEX [IX_FK__tblPerson__intId__17036CC0]
ON [dbo].[tblPersonsImages]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPersosAffiliateLinks'
ALTER TABLE [dbo].[tblPersosAffiliateLinks]
ADD CONSTRAINT [FK__tblPersos__intId__697C9932]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPersos__intId__697C9932'
CREATE INDEX [IX_FK__tblPersos__intId__697C9932]
ON [dbo].[tblPersosAffiliateLinks]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPlans'
ALTER TABLE [dbo].[tblPlans]
ADD CONSTRAINT [FK__tblPlans__intIdP__19DFD96B]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPlans__intIdP__19DFD96B'
CREATE INDEX [IX_FK__tblPlans__intIdP__19DFD96B]
ON [dbo].[tblPlans]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdDad] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [FK__tblReferr__intId__1CBC4616]
    FOREIGN KEY ([intIdDad])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblReferr__intId__1CBC4616'
CREATE INDEX [IX_FK__tblReferr__intId__1CBC4616]
ON [dbo].[tblReferred]
    ([intIdDad]);
GO

-- Creating foreign key on [intIdCurrent] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [FK__tblReferr__intId__1DB06A4F]
    FOREIGN KEY ([intIdCurrent])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblReferr__intId__1DB06A4F'
CREATE INDEX [IX_FK__tblReferr__intId__1DB06A4F]
ON [dbo].[tblReferred]
    ([intIdCurrent]);
GO

-- Creating foreign key on [PhoneId] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [FK__tblChargi__Phone__09A971A2]
    FOREIGN KEY ([PhoneId])
    REFERENCES [dbo].[tblPersonsPhones]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblChargi__Phone__09A971A2'
CREATE INDEX [IX_FK__tblChargi__Phone__09A971A2]
ON [dbo].[tblChargingHistory]
    ([PhoneId]);
GO

-- Creating foreign key on [intIdPhone] in table 'tblLineRecord'
ALTER TABLE [dbo].[tblLineRecord]
ADD CONSTRAINT [FK__tblLineRe__intId__14270015]
    FOREIGN KEY ([intIdPhone])
    REFERENCES [dbo].[tblPersonsPhones]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblLineRe__intId__14270015'
CREATE INDEX [IX_FK__tblLineRe__intId__14270015]
ON [dbo].[tblLineRecord]
    ([intIdPhone]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPersonsPhones'
ALTER TABLE [dbo].[tblPersonsPhones]
ADD CONSTRAINT [FK__tblPerson__intId__17F790F9]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__17F790F9'
CREATE INDEX [IX_FK__tblPerson__intId__17F790F9]
ON [dbo].[tblPersonsPhones]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPhone] in table 'tblPhonesServices'
ALTER TABLE [dbo].[tblPhonesServices]
ADD CONSTRAINT [FK__tblPhones__intId__390E6C01]
    FOREIGN KEY ([intIdPhone])
    REFERENCES [dbo].[tblPersonsPhones]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPhones__intId__390E6C01'
CREATE INDEX [IX_FK__tblPhones__intId__390E6C01]
ON [dbo].[tblPhonesServices]
    ([intIdPhone]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------