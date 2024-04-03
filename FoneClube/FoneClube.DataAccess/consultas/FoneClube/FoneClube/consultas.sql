DECLARE @MyCounter int;
SET @intIdPerson = 1;

select * from tblPersons
where intIdPerson = 1

select * from tblPersonsPhones
where intIdPerson = 1

select * from tblPersonsAddresses
where intIdPerson = 1

SELECT * from tblPersonsImages
where intIdPerson = 1

select * from tblPlans
where intIdPerson = 1



--ALTER TABLE tblPersons ALTER COLUMN intIdPagarme int NULL -- ( Alterar coluna pra nullable )

/*
update tblContasFebraban
set dteInsert = '2017-04-28 09:54:24.254'
where intContaId = 11
*/

select * from  tblContasFebraban --100387217
WHERE intContaId in (12,13)

select top 10 * from  tblHeadsFebraban
WHERE intContaId in (12,13)
select top 10 * from  tblResumosFebraban
WHERE intContaId in (12,13)
select top 10 * from  tblEnderecosFebraban
WHERE intContaId in (12,13)
select top 500 * from  tblBilhetacoesFebraban
WHERE intContaId = 4
SELECT COUNT(*) FROM tblBilhetacoes

select top 10 * from  tblServicosFebraban
WHERE intContaId = 7
SELECT COUNT(*) FROM tblServicos

select top 10 * from  tblDescontosFebraban
WHERE intContaId = 4
select top 10 * from  tblTotalizadoresFebraban

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

--===============================================

select * from tblOperadoras
select * from tblTaxes
select * from tblPlans

select * from tblPersons
select * from tblReferred
select * from tblCommissions

delete from tblReferred
where intId = 9









