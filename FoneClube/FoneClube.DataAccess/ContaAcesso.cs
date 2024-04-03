using Business.Commons.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess
{
    public class ContaAcesso
    {
        public bool SaveConta(Conta conta)
        {
            try
            {
                using (var ctx = new FebrabanContext())
                {

                    var idContaUnica = conta.Head.ContaUnicaID.Replace(" ", string.Empty);
                    // resumo maior que 0
                    //header maior que zero
                    //totalizador maior que zero

                    //todo retorno de objeto, com false e mensagem
                    var idIntContaUnica = Convert.ToInt64(idContaUnica);
                    var dateEmissao = Convert.ToInt64(conta.Head.Emissao);
                    if (ctx.tblContasFebraban.Any(c => c.intIdContaUnica == idIntContaUnica && c.intIdEmissao == dateEmissao))
                        return false;

                    var tblConta = new tblContasFebraban
                    {
                        dteGeracaoConta = conta.Head.DataGeracaoArquivo,
                        dteInsert = DateTime.Now,
                        intTipoOperadora = conta.TipoOperadora,
                        txtIdContaUnica = idContaUnica,
                        intIdContaUnica = Convert.ToInt64(idContaUnica),
                        txtEmissao = conta.Head.Emissao,
                        intIdEmissao = Convert.ToInt64(conta.Head.Emissao)
                    };

                    ctx.tblContasFebraban.Add(tblConta);
                    ctx.SaveChanges();

                    var contaId = tblConta.intContaId;

                    ctx.tblHeadsFebraban.Add(CreateTabelaHead(conta.Head, contaId));
                    ctx.SaveChanges();

                    var resumos = new List<tblResumosFebraban>();
                    foreach (var resumo in conta.Resumos)
                        resumos.Add(CreateTabelaResumo(resumo, contaId));

                    ctx.tblResumosFebraban.AddRange(resumos);
                    ctx.SaveChanges();

                    var enderecos = new List<tblEnderecosFebraban>();
                    foreach (var endereco in conta.Enderecos)
                        enderecos.Add(CreateTabelaEndereco(endereco, contaId));

                    ctx.tblEnderecosFebraban.AddRange(enderecos);
                    ctx.SaveChanges();

                    var bilhetacoes = new List<tblBilhetacoesFebraban>();
                    foreach (var bilhetacao in conta.Bilhetacoes)
                        bilhetacoes.Add(CreateTabelaBilhetacao(bilhetacao, contaId));

                    ctx.tblBilhetacoesFebraban.AddRange(bilhetacoes);
                    ctx.SaveChanges();

                    var servicos = new List<tblServicosFebraban>();
                    foreach (var servico in conta.Servicos)
                        servicos.Add(CreateTabelaServico(servico, contaId));

                    ctx.tblServicosFebraban.AddRange(servicos);
                    ctx.SaveChanges();

                    var descontos = new List<tblDescontosFebraban>();
                    foreach (var desconto in conta.Descontos)
                        descontos.Add(CreateTabelaDesconto(desconto, contaId));

                    ctx.tblDescontosFebraban.AddRange(descontos);
                    ctx.SaveChanges();

                    var totalizadores = CreateTabelaTotalizadores(conta.Totalizadores, contaId);
                    ctx.tblTotalizadoresFebraban.Add(totalizadores);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private tblTotalizadoresFebraban CreateTabelaTotalizadores(Totalizador totalizadores, int contaId)
        {
            return new tblTotalizadoresFebraban
            {
                intContaId = contaId,
                txtControleSequencial = totalizadores.ControleSequencial,
                txtCodigoCliente = totalizadores.CodigoCliente,
                txtVencimento = totalizadores.Vencimento,
                txtEmissao = totalizadores.Emissao,
                txtQuantidadeRegistros = totalizadores.QuantidadeRegistros,
                txtQuantidadeLinhas = totalizadores.QuantidadeLinhas,
                txtSinalTotal = totalizadores.SinalTotal,
                txtValorTotal = totalizadores.ValorTotal,
                txtFiller = totalizadores.Filler
            };
        }

        private tblDescontosFebraban CreateTabelaDesconto(Desconto desconto, int contaId)
        {
            return new tblDescontosFebraban
            {
                intContaId = contaId,
                txtControleSequencial = desconto.ControleSequencial,
                txtVencimento = desconto.Vencimento,
                txtEmissao = desconto.Emissao,
                txtIDUnicoNRC = desconto.IDUnicoNRC,
                txtRecursoCNL = desconto.RecursoCNL,
                txtDDD = desconto.DDD,
                txtNumeroTelefone = desconto.NumeroTelefone,
                txtGrupoCategoria = desconto.GrupoCategoria,
                txtDescricaoGrupoCategoria = desconto.DescricaoGrupoCategoria,
                txtSinalValorLigacao = desconto.SinalValorLigacao,
                txtBaseCalculoDesconto = desconto.BaseCalculoDesconto,
                txtPercentualDesconto = desconto.PercentualDesconto,
                txtValorLigacao = desconto.ValorLigacao,
                txtDataInicioAcerto = desconto.DataInicioAcerto,
                txtHoraInicioAcerto = desconto.HoraInicioAcerto,
                txtDataFimAcerto = desconto.DataFimAcerto,
                txtHoraFimAcerto = desconto.HoraFimAcerto,
                txtClasseServico = desconto.ClasseServico,
                txtFiller = desconto.Filler
            };
        }

        private tblServicosFebraban CreateTabelaServico(Servico servico, int contaId)
        {
            return new tblServicosFebraban
            {
                intContaId = contaId,
                txtControleSequencial = servico.ControleSequencial,
                txtVencimento = servico.Vencimento,
                txtEmissao = servico.Emissao,
                txtIDUnicoNRC = servico.IDUnicoNRC,
                txtRecursoCNL = servico.RecursoCNL,
                txtDDD = servico.DDD,
                txtNumeroTelefone = servico.NumeroTelefone,
                txtCaracteristicaRecurso = servico.CaracteristicaRecurso,
                txtDataServico = servico.DataServico,
                txtCNLLocalidadeChamada = servico.CNLLocalidadeChamada,
                txtNomeLocalidadeChamada = servico.NomeLocalidadeChamada,
                txtUFTelefoneChamado = servico.UFTelefoneChamado,
                txtCODNacionalInternacional = servico.CODNacionalInternacional,
                txtCODOperadora = servico.CODOperadora,
                txtDescricaoOperadora = servico.DescricaoOperadora,
                txtCODPais = servico.CODPais,
                txtAreaDDD = servico.AreaDDD,
                txtNumeroTelefoneChamado = servico.NumeroTelefoneChamado,
                txtConjugadoNumeroTelefoneChamado = servico.ConjugadoNumeroTelefoneChamado,
                txtDuracaoLigacao = servico.DuracaoLigacao,
                txtHorarioLigacao = servico.HorarioLigacao,
                txtGrupoCategoria = servico.GrupoCategoria,
                txtDescricaoGrupoCategoria = servico.DescricaoGrupoCategoria,
                txtCategoria = servico.Categoria,
                txtDescricaoCategoria = servico.DescricaoCategoria,
                txtSinalValorLigacao = servico.SinalValorLigacao,
                txtValorLigacao = servico.ValorLigacao,
                txtClasseServico = servico.ClasseServico,
                txtFiller = servico.Filler
            };
        }

        private tblBilhetacoesFebraban CreateTabelaBilhetacao(Bilhetacao bilhetacao, int contaId)
        {
            return new tblBilhetacoesFebraban
            {
                intContaId = contaId,
                txtControleSequencial = bilhetacao.ControleSequencial,
                txtVencimento = bilhetacao.Vencimento,
                txtEmissao = bilhetacao.Emissao,
                txtIDUnicoNRC = bilhetacao.IDUnicoNRC,
                txtRecursoCNL = bilhetacao.RecursoCNL,
                txtDDD = bilhetacao.DDD,
                txtNumeroTelefone = bilhetacao.NumeroTelefone,
                txtCaracteristicaRecurso = bilhetacao.CaracteristicaRecurso,
                txtDegrauRecurso = bilhetacao.DegrauRecurso,
                txtDataLigacao = bilhetacao.DataLigacao,
                txtCNLLocalidadeChamada = bilhetacao.CNLLocalidadeChamada,
                txtNomeLocalidadeChamada = bilhetacao.NomeLocalidadeChamada,
                txtUFTelefoneChamado = bilhetacao.UFTelefoneChamado,
                txtCODNacionalInternacional = bilhetacao.CODNacionalInternacional,
                txtCODOperadora = bilhetacao.CODOperadora,
                txtDescricaoOperadora = bilhetacao.DescricaoOperadora,
                txtCODPais = bilhetacao.CODPais,
                txtAreaDDD = bilhetacao.AreaDDD,
                txtNumeroTelefoneChamado = bilhetacao.NumeroTelefoneChamado,
                txtConjugadoNumeroTelefoneChamado = bilhetacao.ConjugadoNumeroTelefoneChamado,
                txtDuracaoLigacao = bilhetacao.DuracaoLigacao,
                txtCategoria = bilhetacao.Categoria,
                txtDescricaoCategoria = bilhetacao.DescricaoCategoria,
                txtHorarioLigacao = bilhetacao.HorarioLigacao,
                txtTipoChamada = bilhetacao.TipoChamada,
                txtGrupoHorarioTarifario = bilhetacao.GrupoHorarioTarifario,
                txtDescricaoHorarioTarifario = bilhetacao.DescricaoHorarioTarifario,
                txtDegrauLigacao = bilhetacao.DegrauLigacao,
                txtSinalValorLigacao = bilhetacao.SinalValorLigacao,
                txtAliquotaICMS = bilhetacao.AliquotaICMS,
                txtValorLigacaoComImposto = bilhetacao.ValorLigacaoComImposto,
                txtClasseServico = bilhetacao.ClasseServico,
                txtFiller = bilhetacao.Filler
            };
        }

        private tblEnderecosFebraban CreateTabelaEndereco(Endereco endereco, int contaId)
        {
            return new tblEnderecosFebraban
            {
                intContaId = contaId,
                txtControleSequencial = endereco.ControleSequencial,
                txtIDUnicoNRC = endereco.IDUnicoNRC,
                txtDDD = endereco.DDD,
                txtNumeroTelefone = endereco.NumeroTelefone,
                txtCaracteristicaRecurso = endereco.CaracteristicaRecurso,
                txtCNLRecursoEnderecoPontaA = endereco.CNLRecursoEnderecoPontaA,
                txtNomeLocalidadePontaA = endereco.NomeLocalidadePontaA,
                txtUFLocalidadePontaA = endereco.UFLocalidadePontaA,
                txtEnderecoPontaA = endereco.EnderecoPontaA,
                txtNumeroEnderecoPontaA = endereco.NumeroEnderecoPontaA,
                txtComplementoPontaA = endereco.ComplementoPontaA,
                txtBairroPontaA = endereco.BairroPontaA,
                txtCNLRecursoEnderecoPontaB = endereco.CNLRecursoEnderecoPontaB,
                txtNomeLocalidadePontaB = endereco.NomeLocalidadePontaB,
                txtUFLocalidadePontaB = endereco.UFLocalidadePontaB,
                txtEnderecoPontaB = endereco.EnderecoPontaB,
                txtNumeroEnderecoPontaB = endereco.NumeroEnderecoPontaB,
                txtComplementoPontaB = endereco.ComplementoPontaB,
                txtBairroPontaB = endereco.BairroPontaB,
                txtFiller = endereco.Filler
            };
        }

        private tblHeadsFebraban CreateTabelaHead(Head head, int contaId)
        {
            return new tblHeadsFebraban
            {
                intContaId = contaId,
                txtControleSequencial = head.ControleSequencial,
                txtIdContaUnica = head.ContaUnicaID,
                txtDataGeracaoArquivo = head.DataGeracaoArquivo,
                txtIdentificadorEmpresa = head.IdentificadorEmpresa,
                txtEmpresaUF = head.EmpresaUF,
                txtCodigoCliente = head.CodigoCliente,
                txtNomeCliente = head.NomeCliente,
                txtClienteCGC = head.ClienteCGC,
                txtVencimento = head.Vencimento,
                txtEmissao = head.Emissao,
                txtFiller = head.Filler
            };
        }

        private tblResumosFebraban CreateTabelaResumo(Resumo resumo, int contaId)
        {
            return new tblResumosFebraban
            {
                intContaId = contaId,
                txtControleSequencial = resumo.ControleSequencial,
                txtVencimento = resumo.Vencimento,
                txtEmissao = resumo.Emissao,
                txtIDUnicoNRC = resumo.IDUnicoNRC,
                txtRecursoCNL = resumo.RecursoCNL,
                txtLocalidade = resumo.Localidade,
                txtDDD = resumo.DDD,
                txtNumeroTelefone = resumo.NumeroTelefone,
                txtTipoServico = resumo.TipoServico,
                txtDescricaoServico = resumo.DescricaoServico,
                txtCaracteristicaRecurso = resumo.CaracteristicaRecurso,
                txtDegrauRecurso = resumo.DegrauRecurso,
                txtVelocidadeRecurso = resumo.VelocidadeRecurso,
                txtUnidadeVelocidadeRecurso = resumo.UnidadeVelocidadeRecurso,
                txtInicioAssinatura = resumo.InicioAssinatura,
                txtFimAssinatura = resumo.FimAssinatura,
                txtInicioPeriodoServico = resumo.InicioPeriodoServico,
                txtFimPeriodoServico = resumo.FimPeriodoServico,
                txtUnidadeConsumo = resumo.UnidadeConsumo,
                txtQuantidadeConsumo = resumo.QuantidadeConsumo,
                txtSinalValorConsumo = resumo.SinalValorConsumo,
                txtValorConsumo = resumo.ValorConsumo,
                txtSinalAssinatura = resumo.SinalAssinatura,
                txtValorAssinatura = resumo.ValorAssinatura,
                txtAliquota = resumo.Aliquota,
                txtSinalICMS = resumo.SinalICMS,
                txtValorICMS = resumo.ValorICMS,
                txtSinalTotalOutrosImpostos = resumo.SinalTotalOutrosImpostos,
                txtValorTotalImpostos = resumo.ValorTotalImpostos,
                txtNumeroNotaFiscal = resumo.NumeroNotaFiscal,
                txtSinalValorConta = resumo.SinalValorConta,
                txtValorConta = resumo.ValorConta,
                txtFiller = resumo.Filler
            };
        }

        public bool ValidaConexaoDB()
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.tblLog.Add(new tblLog
                {
                    txtAction = "Teste conexão DB",
                    dteTimeStamp = DateTime.Now,
                    intIdTipo = 1 //todo colocar enum
                });

                ctx.SaveChanges();
            }

            return true;
        }
    }
}
