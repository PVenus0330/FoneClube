-------------------------------------------
-- febraban contas ---------------------------------
-------------------------------------------


create table tblContasFebraban
(
    intContaId int IDENTITY(1,1) primary key not null,
    dteInsert datetime not null,
    dteGeracaoConta nvarchar(250) not null,
	intTipoOperadora int not null
);

create table tblHeadsFebraban (
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

create table tblResumosFebraban (
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

create table tblEnderecosFebraban(
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

create table tblBilhetacoesFebraban(
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

create table tblServicosFebraban(
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

create table tblDescontosFebraban(
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

create table tblTotalizadoresFebraban(
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

/*

drop table tblHeads
drop table tblResumos
drop table tblEnderecos
drop table tblBilhetacoes
drop table tblServicos
drop table tblDescontos
drop table tblTotalizadores
drop table tblContas

*/