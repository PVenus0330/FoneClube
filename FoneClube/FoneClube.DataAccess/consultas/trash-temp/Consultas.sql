select * from tblPersons
select * from tblPersonsPhones
select * from tblPersonsAddresses

select top 10 * from  tblContas
select top 10 * from  tblHeads
select top 10 * from  tblResumos
select top 10 * from  tblEnderecos

select top 500 * from  tblBilhetacoes
SELECT COUNT(*) FROM tblBilhetacoes

select top 10 * from  tblServicos
SELECT COUNT(*) FROM tblServicos

select top 10 * from  tblDescontos
select top 10 * from  tblTotalizadores

select distinct txtNumeroTelefone from tblResumos
where txtNumeroTelefone != ''

select distinct txtIDUnicoNRC from tblResumos
where txtIDUnicoNRC != ''

-- ligações e custos de um número
select  intId, 
intContaId, 
txtEmissao, 
txtVencimento, 
txtIDUnicoNRC, 
txtNumeroTelefone,
txtDataLigacao,
txtNomeLocalidadeChamada,
txtCODNacionalInternacional,
txtAreaDDD,
txtNumeroTelefoneChamado,
txtDuracaoLigacao,
txtCategoria,
txtDescricaoCategoria,
txtHorarioLigacao,
txtTipoChamada,
txtSinalValorLigacao
txtAliquotaICMS,
txtValorLigacaoComImposto 
from tblBilhetacoes 
where txtIDUnicoNRC = '21981395481'


select 
intId, 
intContaId, 
txtIDUnicoNRC, 
txtEmissao, 
txtVencimento,
txtNumeroTelefone 
txtTipoServico, 
txtDescricaoServico, 
txtInicioAssinatura, 
txtFimAssinatura, 
txtInicioPeriodoServico, 
txtFimPeriodoServico,
txtNumeroNotaFiscal,
txtSinalValorConta,
txtValorConta from tblResumos
where txtNumeroTelefone != ''

===============================================

select * from tblOperadoras
select * from tblTaxes
select * from tblPlans







