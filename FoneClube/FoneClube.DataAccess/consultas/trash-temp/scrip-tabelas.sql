/*SET QUOTED_IDENTIFIER OFF;
GO
USE [rodigocp_FoneClube_Homol];
GO
*/
-------------------------------------------
-- febraban contas ---------------------------------
-------------------------------------------

create table tblContas
(
    intContaId int primary key not null,
    dteInsert date not null,
    dteGeracaoConta nvarchar(250) not null
);

create table tblHeads (
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtDataGeracaoArquivo nvarchar(250) not null,
    txtIdentificadorEmpresa nvarchar(250) not null,
    txtEmpresaUF nvarchar(250) not null,
    txtCodigoCliente nvarchar(250) not null,
    txtNomeCliente nvarchar(250) not null,
    txtClienteCGC nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtFiller nvarchar(250) not null
);

create table tblResumos (
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtIDUnicoNRC nvarchar(250) not null,
    txtRecursoCNL nvarchar(250) not null,
    txtLocalidade nvarchar(250) not null,
    txtDDD nvarchar(250) not null,
    txtNumeroTelefone nvarchar(250) not null,
    txtTipoServico nvarchar(250) not null,
    txtDescricaoServico nvarchar(250) not null,
    txtCaracteristicaRecurso nvarchar(250) not null,
    txtDegrauRecurso nvarchar(250) not null,
    txtVelocidadeRecurso nvarchar(250) not null,
    txtUnidadeVelocidadeRecurso nvarchar(250) not null,
    txtInicioAssinatura nvarchar(250) not null,
    txtFimAssinatura nvarchar(250) not null,
    txtInicioPeriodoServico nvarchar(250) not null,
    txtFimPeriodoServico nvarchar(250) not null,
    txtUnidadeConsumo nvarchar(250) not null,
    txtQuantidadeConsumo nvarchar(250) not null,
    txtSinalValorConsumo nvarchar(250) not null,
    txtValorConsumo nvarchar(250) not null,
    txtSinalAssinatura nvarchar(250) not null,
    txtValorAssinatura nvarchar(250) not null,
    txtAliquota nvarchar(250) not null,
    txtSinalICMS nvarchar(250) not null,
    txtValorICMS nvarchar(250) not null,
    txtSinalTotalOutrosImpostos nvarchar(250) not null,
    txtValorTotalImpostos nvarchar(250) not null,
    txtNumeroNotaFiscal nvarchar(250) not null,
    txtSinalValorConta nvarchar(250) not null,
    txtValorConta nvarchar(250) not null,
    txtFiller nvarchar(250) not null
);

create table tblEnderecos(
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtIDUnicoNRC nvarchar(250) not null,
    txtDDD nvarchar(250) not null,
    txtNumeroTelefone nvarchar(250) not null,
    txtCaracteristicaRecurso nvarchar(250) not null,
    txtCNLRecursoEnderecoPontaA nvarchar(250) not null,
    txtNomeLocalidadePontaA nvarchar(250) not null,
    txtUFLocalidadePontaA nvarchar(250) not null,
    txtEnderecoPontaA nvarchar(250) not null,
    txtNumeroEnderecoPontaA nvarchar(250) not null,
    txtComplementoPontaA nvarchar(250) not null,
    txtBairroPontaA nvarchar(250) not null,
    txtCNLRecursoEnderecoPontaB nvarchar(250) not null,
    txtNomeLocalidadePontaB nvarchar(250) not null,
    txtUFLocalidadePontaB nvarchar(250) not null,
    txtEnderecoPontaB nvarchar(250) not null,
    txtNumeroEnderecoPontaB nvarchar(250) not null,
    txtComplementoPontaB nvarchar(250) not null,
    txtBairroPontaB nvarchar(250) not null,
    txtFiller nvarchar(250) not null
)

create table tblBilhetacoes(
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtIDUnicoNRC nvarchar(250) not null,
    txtRecursoCNL nvarchar(250) not null,
    txtDDD nvarchar(250) not null,
    txtNumeroTelefone nvarchar(250) not null,
    txtCaracteristicaRecurso nvarchar(250) not null,
    txtDegrauRecurso nvarchar(250) not null,
    txtDataLigacao nvarchar(250) not null,
    txtCNLLocalidadeChamada nvarchar(250) not null,
    txtNomeLocalidadeChamada nvarchar(250) not null,
    txtUFTelefoneChamado nvarchar(250) not null,
    txtCODNacionalInternacional nvarchar(250) not null,
    txtCODOperadora nvarchar(250) not null,
    txtDescricaoOperadora nvarchar(250) not null,
    txtCODPais nvarchar(250) not null,
    txtAreaDDD nvarchar(250) not null,
    txtNumeroTelefoneChamado nvarchar(250) not null,
    txtConjugadoNumeroTelefoneChamado nvarchar(250) not null,
    txtDuracaoLigacao nvarchar(250) not null,
    txtCategoria nvarchar(250) not null,
    txtDescricaoCategoria nvarchar(250) not null,
    txtHorarioLigacao nvarchar(250) not null,
    txtTipoChamada nvarchar(250) not null,
    txtGrupoHorarioTarifario nvarchar(250) not null,
    txtDescricaoHorarioTarifario nvarchar(250) not null,
    txtDegrauLigacao nvarchar(250) not null,
    txtSinalValorLigacao nvarchar(250) not null,
    txtAliquotaICMS nvarchar(250) not null,
    txtValorLigacaoComImposto nvarchar(250) not null,
    txtClasseServico nvarchar(250) not null,
    txtFiller nvarchar(250) not null
)

create table tblServicos(
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtIDUnicoNRC nvarchar(250) not null,
    txtRecursoCNL nvarchar(250) not null,
    txtDDD nvarchar(250) not null,
    txtNumeroTelefone nvarchar(250) not null,
    txtCaracteristicaRecurso nvarchar(250) not null,
    txtDataServico nvarchar(250) not null,
    txtCNLLocalidadeChamada nvarchar(250) not null,
    txtNomeLocalidadeChamada nvarchar(250) not null,
    txtUFTelefoneChamado nvarchar(250) not null,
    txtCODNacionalInternacional nvarchar(250) not null,
    txtCODOperadora nvarchar(250) not null,
    txtDescricaoOperadora nvarchar(250) not null,
    txtCODPais nvarchar(250) not null,
    txtAreaDDD nvarchar(250) not null,
    txtNumeroTelefoneChamado nvarchar(250) not null,
    txtConjugadoNumeroTelefoneChamado nvarchar(250) not null,
    txtDuracaoLigacao nvarchar(250) not null,
    txtHorarioLigacao nvarchar(250) not null,
    txtGrupoCategoria nvarchar(250) not null,
    txtDescricaoGrupoCategoria nvarchar(250) not null,
    txtCategoria nvarchar(250) not null,
    txtDescricaoCategoria nvarchar(250) not null,
    txtSinalValorLigacao nvarchar(250) not null,
    txtValorLigacao nvarchar(250) not null,
    txtClasseServico nvarchar(250) not null,
    txtFiller nvarchar(250) not null
)

create table tblDescontos(
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtIDUnicoNRC nvarchar(250) not null,
    txtRecursoCNL nvarchar(250) not null,
    txtDDD nvarchar(250) not null,
    txtNumeroTelefone nvarchar(250) not null,
    txtGrupoCategoria nvarchar(250) not null,
    txtDescricaoGrupoCategoria nvarchar(250) not null,
    txtSinalValorLigacao nvarchar(250) not null,
    txtBaseCalculoDesconto nvarchar(250) not null,
    txtPercentualDesconto nvarchar(250) not null,
    txtValorLigacao nvarchar(250) not null,
    txtDataInicioAcerto nvarchar(250) not null,
    txtHoraInicioAcerto nvarchar(250) not null,
    txtDataFimAcerto nvarchar(250) not null,
    txtHoraFimAcerto nvarchar(250) not null,
    txtClasseServico nvarchar(250) not null,
    txtFiller nvarchar(250) not null
)

create table tblTotalizadores(
    intId int  IDENTITY(1,1) primary key not null,
    intContaId int FOREIGN KEY REFERENCES tblContas(intContaId),
    txtControleSequencial nvarchar(250) not null,
    txtCodigoCliente nvarchar(250) not null,
    txtVencimento nvarchar(250) not null,
    txtEmissao nvarchar(250) not null,
    txtQuantidadeRegistros nvarchar(250) not null,
    txtQuantidadeLinhas nvarchar(250) not null,
    txtSinalTotal nvarchar(250) not null,
    txtValorTotal nvarchar(250) not null,
    txtFiller nvarchar(250) not null
)


-----------------------------------
----log----------------------------
-----------------------------------

--select * from tblLog

create table tblLogTipo
(
    intIdTipo int IDENTITY(1,1) primary key not null,
	txtDescricao nvarchar(250) not null
);

create table tblLog
(
    intId int  IDENTITY(1,1) primary key not null,
    intIdTipo int FOREIGN KEY REFERENCES tblLogTipo(intIdTipo),
	txtAction nvarchar(250) not null,
	dteTimeStamp datetime not null
);



insert into tblLogTipo (txtDescricao)
values ('Dev Debug')

insert into tblLogTipo (txtDescricao)
values ('Relatório de Erro')

insert into tblLogTipo (txtDescricao)
values ('Parseamento')


-------------------------------------------
-- persons --------------------------------
-------------------------------------------


create table tblPersons
(
    intIdPerson int IDENTITY(1,1) primary key,
    dteRegister date not null,
    txtDocumentNumber nvarchar(15) not null,
	txtName nvarchar(60) not null,
	txtEmail nvarchar(30) not null,
	dteBorn date,
	intGender int,
	intIdPagarme int not null,
	intIdRole int
);



create table tblPersonsPhones
(
    intId int IDENTITY(1,1) primary key,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
	intDDD int,
	intPhone int,
);

create table tblPersonsAddresses
(
    intId int IDENTITY(1,1) primary key,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
	txtStreet nvarchar(60),
	txtComplement nvarchar(60),
	intStreetNumber int,
	txtNeighborhood nvarchar(60),
	txtCity nvarchar(60),
	txtState nvarchar(20),
	txtCep nvarchar(20)  not null,
	txtCountry nvarchar(20),
);






----------------------------------------
---- operadoras, taxas, planos

create table tblOperadoras
(
    intIdOperator int primary key not null,
    txtName nvarchar(10) not null
);

create table tblTaxes
(
	intIdTax int primary key not null,
	intIdOperator int FOREIGN KEY REFERENCES tblOperadoras(intIdOperator),
	intSimCardCost int not null,
	intFranchiseCost int not null,
	intManagerCost int not null,
	intIntragroupTZCost int not null,
	intSignatureCost int not null,
	intOneGBCost int,
	intThreeGBCost int,
	intFiveGBCost int,
	intTenGBCost int,
	intTwentyGBCost int,
	intFortyGBCost int,
	intLocalCallCost int not null,
	intDDDCallCost int not null,
	intDDDCallOthersCost int not null,
	intSMSCost int
);



create table tblPlansCosts(
	intIdPlan int primary key not null,
	intIdOperator int FOREIGN KEY REFERENCES tblOperadoras(intIdOperator),
	txtDescription nvarchar(60),
	intCost int
)

create table tblPlans(
	intIdPlan int IDENTITY(1,1) primary key,
	intIdOption int FOREIGN KEY REFERENCES tblPlansCosts(intIdPlan),
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson)
)

create table tblCommissions
(
    intIdComission int primary key not null,
    intComissionCost int not null
)

create table tblReferred
(
    intId int primary key not null,
    intIdComission int FOREIGN KEY REFERENCES tblCommissions(intIdComission),
    intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
    intBitActive int
)


--------------------- drop tables

-- foneclube febraban

drop table tblHeads
drop table tblResumos
drop table tblEnderecos
drop table tblBilhetacoes
drop table tblServicos
drop table tblDescontos
drop table tblTotalizadores
drop table tblContas

-- foneclube base
drop table tblReferred
drop table tblPlans -- vai ter relações
drop table tblPlansCosts
drop table tblTaxes
drop table tblCommissions
drop table tblPersonsAddresses
drop table tblPersonsPhones
drop table tblPersons
drop table tblOperadoras

drop table tblLog