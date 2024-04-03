
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 06/29/2017 23:55:31
-- Generated from EDMX file: C:\GitProjects\FoneClube.NET\FoneClube.DataAccess\temp.edmx
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

IF OBJECT_ID(N'[dbo].[FK__tblBilhet__intCo__186C9245]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblBilhetacoesFebraban] DROP CONSTRAINT [FK__tblBilhet__intCo__186C9245];
GO
IF OBJECT_ID(N'[dbo].[FK__tblChargi__intId__1D66518C]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblChargingHistory] DROP CONSTRAINT [FK__tblChargi__intId__1D66518C];
GO
IF OBJECT_ID(N'[dbo].[FK__tblChargi__intId__1E5A75C5]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblChargingHistory] DROP CONSTRAINT [FK__tblChargi__intId__1E5A75C5];
GO
IF OBJECT_ID(N'[dbo].[FK__tblCommis__intId__1995C0A8]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblCommissionOrders] DROP CONSTRAINT [FK__tblCommis__intId__1995C0A8];
GO
IF OBJECT_ID(N'[dbo].[FK__tblCommis__intId__1A89E4E1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblCommissionOrders] DROP CONSTRAINT [FK__tblCommis__intId__1A89E4E1];
GO
IF OBJECT_ID(N'[dbo].[FK__tblDescon__intCo__1E256B9B]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDescontosFebraban] DROP CONSTRAINT [FK__tblDescon__intCo__1E256B9B];
GO
IF OBJECT_ID(N'[dbo].[FK__tblEndere__intCo__1590259A]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblEnderecosFebraban] DROP CONSTRAINT [FK__tblEndere__intCo__1590259A];
GO
IF OBJECT_ID(N'[dbo].[FK__tblHeadsF__intCo__0FD74C44]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblHeadsFebraban] DROP CONSTRAINT [FK__tblHeadsF__intCo__0FD74C44];
GO
IF OBJECT_ID(N'[dbo].[FK__tblLog__intIdTip__5165187F]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLog] DROP CONSTRAINT [FK__tblLog__intIdTip__5165187F];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__01BE3717]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsPhones] DROP CONSTRAINT [FK__tblPerson__intId__01BE3717];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__049AA3C2]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsAddresses] DROP CONSTRAINT [FK__tblPerson__intId__049AA3C2];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPerson__intId__0777106D]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPersonsImages] DROP CONSTRAINT [FK__tblPerson__intId__0777106D];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlans__intIdO__0F183235]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlans] DROP CONSTRAINT [FK__tblPlans__intIdO__0F183235];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlans__intIdP__100C566E]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlans] DROP CONSTRAINT [FK__tblPlans__intIdP__100C566E];
GO
IF OBJECT_ID(N'[dbo].[FK__tblPlansO__intId__0C3BC58A]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblPlansOptions] DROP CONSTRAINT [FK__tblPlansO__intId__0C3BC58A];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__14D10B8B]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__14D10B8B];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__15C52FC4]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__15C52FC4];
GO
IF OBJECT_ID(N'[dbo].[FK__tblReferr__intId__16B953FD]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblReferred] DROP CONSTRAINT [FK__tblReferr__intId__16B953FD];
GO
IF OBJECT_ID(N'[dbo].[FK__tblResumo__intCo__12B3B8EF]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblResumosFebraban] DROP CONSTRAINT [FK__tblResumo__intCo__12B3B8EF];
GO
IF OBJECT_ID(N'[dbo].[FK__tblServic__intCo__1B48FEF0]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblServicosFebraban] DROP CONSTRAINT [FK__tblServic__intCo__1B48FEF0];
GO
IF OBJECT_ID(N'[dbo].[FK__tblTotali__intCo__2101D846]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblTotalizadoresFebraban] DROP CONSTRAINT [FK__tblTotali__intCo__2101D846];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[tblBilhetacoesFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblBilhetacoesFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblChargingHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblChargingHistory];
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
IF OBJECT_ID(N'[dbo].[tblEnderecosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblEnderecosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblHeadsFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblHeadsFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLog];
GO
IF OBJECT_ID(N'[dbo].[tblLogTipo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLogTipo];
GO
IF OBJECT_ID(N'[dbo].[tblOperadoras]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOperadoras];
GO
IF OBJECT_ID(N'[dbo].[tblPaymentTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPaymentTypes];
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
IF OBJECT_ID(N'[dbo].[tblPersonsPhones]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPersonsPhones];
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
IF OBJECT_ID(N'[dbo].[tblResumosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblResumosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblServicosFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServicosFebraban];
GO
IF OBJECT_ID(N'[dbo].[tblTotalizadoresFebraban]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblTotalizadoresFebraban];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'tblChargingHistory'
CREATE TABLE [dbo].[tblChargingHistory] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdCustomer] int  NULL,
    [intIdCollector] int  NULL,
    [txtCollectorName] nvarchar(40)  NULL,
    [intIdPaymentType] int  NULL,
    [txtComment] nvarchar(80)  NULL,
    [dtePayment] datetime  NOT NULL,
    [txtAmmountPayment] nvarchar(40)  NOT NULL,
    [txtTokenTransaction] nvarchar(20)  NULL,
    [intIdBoleto] bigint  NULL,
    [txtAcquireId] nvarchar(40)  NULL
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
    [txtAction] nvarchar(250)  NOT NULL,
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

-- Creating table 'tblPersons'
CREATE TABLE [dbo].[tblPersons] (
    [intIdPerson] int IDENTITY(1,1) NOT NULL,
    [dteRegister] datetime  NOT NULL,
    [txtDocumentNumber] nvarchar(15)  NOT NULL,
    [txtName] nvarchar(60)  NOT NULL,
    [txtEmail] nvarchar(200)  NULL,
    [dteBorn] datetime  NULL,
    [intGender] int  NULL,
    [intIdPagarme] int  NULL,
    [intIdRole] int  NULL,
    [intContactId] int  NULL,
    [txtNickName] nvarchar(60)  NULL,
    [matricula] bigint  NULL,
    [bitManual] bit  NULL,
    [intIdCurrentOperator] int  NULL
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
    [txtCountry] nvarchar(20)  NULL
);
GO

-- Creating table 'tblPersonsImages'
CREATE TABLE [dbo].[tblPersonsImages] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [txtImage] nvarchar(80)  NOT NULL
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

-- Creating table 'tblPersonsPhones'
CREATE TABLE [dbo].[tblPersonsPhones] (
    [intId] int IDENTITY(1,1) NOT NULL,
    [intIdPerson] int  NULL,
    [intDDD] int  NULL,
    [intPhone] bigint  NULL,
    [bitPhoneClube] bit  NULL,
    [intIdOperator] int  NULL,
    [intIdPlan] int  NULL,
    [txtNickname] nvarchar(200)  NULL,
    [bitPortability] bit  NULL
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
    [intIdContaUnica] bigint  NULL
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

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [intId] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [PK_tblChargingHistory]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intIdComissionOrder] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [PK_tblCommissionOrders]
    PRIMARY KEY CLUSTERED ([intIdComissionOrder] ASC);
GO

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

-- Creating primary key on [intIdPerson] in table 'tblPersons'
ALTER TABLE [dbo].[tblPersons]
ADD CONSTRAINT [PK_tblPersons]
    PRIMARY KEY CLUSTERED ([intIdPerson] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsAddresses'
ALTER TABLE [dbo].[tblPersonsAddresses]
ADD CONSTRAINT [PK_tblPersonsAddresses]
    PRIMARY KEY CLUSTERED ([intId] ASC);
GO

-- Creating primary key on [intId] in table 'tblPersonsImages'
ALTER TABLE [dbo].[tblPersonsImages]
ADD CONSTRAINT [PK_tblPersonsImages]
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

-- Creating primary key on [intId] in table 'tblPersonsPhones'
ALTER TABLE [dbo].[tblPersonsPhones]
ADD CONSTRAINT [PK_tblPersonsPhones]
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

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [intIdCustomer] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [FK__tblChargi__intId__61BB7BD9]
    FOREIGN KEY ([intIdCustomer])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblChargi__intId__61BB7BD9'
CREATE INDEX [IX_FK__tblChargi__intId__61BB7BD9]
ON [dbo].[tblChargingHistory]
    ([intIdCustomer]);
GO

-- Creating foreign key on [intIdPaymentType] in table 'tblChargingHistory'
ALTER TABLE [dbo].[tblChargingHistory]
ADD CONSTRAINT [FK__tblChargi__intId__62AFA012]
    FOREIGN KEY ([intIdPaymentType])
    REFERENCES [dbo].[tblPaymentTypes]
        ([intId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblChargi__intId__62AFA012'
CREATE INDEX [IX_FK__tblChargi__intId__62AFA012]
ON [dbo].[tblChargingHistory]
    ([intIdPaymentType]);
GO

-- Creating foreign key on [intIdComission] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [FK__tblCommis__intId__436BFEE3]
    FOREIGN KEY ([intIdComission])
    REFERENCES [dbo].[tblCommissions]
        ([intIdComission])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblCommis__intId__436BFEE3'
CREATE INDEX [IX_FK__tblCommis__intId__436BFEE3]
ON [dbo].[tblCommissionOrders]
    ([intIdComission]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblCommissionOrders'
ALTER TABLE [dbo].[tblCommissionOrders]
ADD CONSTRAINT [FK__tblCommis__intId__4460231C]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblCommis__intId__4460231C'
CREATE INDEX [IX_FK__tblCommis__intId__4460231C]
ON [dbo].[tblCommissionOrders]
    ([intIdPerson]);
GO

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

-- Creating foreign key on [intIdPerson] in table 'tblPersonsAddresses'
ALTER TABLE [dbo].[tblPersonsAddresses]
ADD CONSTRAINT [FK__tblPerson__intId__2C88998B]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__2C88998B'
CREATE INDEX [IX_FK__tblPerson__intId__2C88998B]
ON [dbo].[tblPersonsAddresses]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPersonsImages'
ALTER TABLE [dbo].[tblPersonsImages]
ADD CONSTRAINT [FK__tblPerson__intId__2F650636]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__2F650636'
CREATE INDEX [IX_FK__tblPerson__intId__2F650636]
ON [dbo].[tblPersonsImages]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdPerson] in table 'tblPlans'
ALTER TABLE [dbo].[tblPlans]
ADD CONSTRAINT [FK__tblPlans__intIdP__39E294A9]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPlans__intIdP__39E294A9'
CREATE INDEX [IX_FK__tblPlans__intIdP__39E294A9]
ON [dbo].[tblPlans]
    ([intIdPerson]);
GO

-- Creating foreign key on [intIdDad] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [FK__tblReferr__intId__51BA1E3A]
    FOREIGN KEY ([intIdDad])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblReferr__intId__51BA1E3A'
CREATE INDEX [IX_FK__tblReferr__intId__51BA1E3A]
ON [dbo].[tblReferred]
    ([intIdDad]);
GO

-- Creating foreign key on [intIdCurrent] in table 'tblReferred'
ALTER TABLE [dbo].[tblReferred]
ADD CONSTRAINT [FK__tblReferr__intId__52AE4273]
    FOREIGN KEY ([intIdCurrent])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblReferr__intId__52AE4273'
CREATE INDEX [IX_FK__tblReferr__intId__52AE4273]
ON [dbo].[tblReferred]
    ([intIdCurrent]);
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

-- Creating foreign key on [intIdPerson] in table 'tblPersonsPhones'
ALTER TABLE [dbo].[tblPersonsPhones]
ADD CONSTRAINT [FK__tblPerson__intId__29AC2CE0]
    FOREIGN KEY ([intIdPerson])
    REFERENCES [dbo].[tblPersons]
        ([intIdPerson])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__tblPerson__intId__29AC2CE0'
CREATE INDEX [IX_FK__tblPerson__intId__29AC2CE0]
ON [dbo].[tblPersonsPhones]
    ([intIdPerson]);
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------