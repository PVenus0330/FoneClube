using FoneClube.Business.Commons.Entities.Claro;
using EntityFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess
{
    public class ClaroContaAcesso
    {

        public bool SaveConta(ClaroConta conta)
        {
            try
            {
                using (var ctx = new FebrabanContext())
                {

                 
                   
                    if (ctx.tblContasClaro.Any(c => c.txtIdContaUnica == conta.IdUnico))
                         return false;

                    
                    var tblConta = new tblContasClaro
                    {
                        dteInsert = DateTime.Now,
                        txtIdContaUnica = conta.IdUnico,

                        //Todo campos abaixo
                        txtDataGeracaoArquivo = "",
                        txtIdentificadorEmpresa = "",

                        txtNomeCliente = conta.Nome,
                        txtCodigoCliente = conta.IdCliente,
                        
                        txtEnderecoCliente = conta.Endereco,

                        
                        txtVencimento = conta.DataVencimento,
                        txtDataReferenciaInicio = conta.DataReferenciaInicio,
                        txtDataReferenciaFim = conta.DataReferenciaFim,
                        txtValor = conta.Valor,
                        
                        txtCodigoReferenciaDebitoAutomatico = conta.IdReferenciaDebitoAutomatico


                    };

                    ctx.tblContasClaro.Add(tblConta);
                    ctx.SaveChanges();

                    var contaId = tblConta.intContaId;

                   
                    var notas = new List<tblContasClaroRegistroNotaFiscal>();
                    foreach (var nota in conta.Notas)
                        notas.Add(CreateTabelaNotas(nota, contaId));

                    ctx.tblContasClaroRegistroNotaFiscal.AddRange(notas);
                    ctx.SaveChanges();

                   
                    var registros = new List<tblContasClaroLinhaRegistro>();
                    foreach (var linhaRegistro in conta.LinhasRegistro)
                        registros.Add(CreateTabelaLinhaRegistro(linhaRegistro, contaId));

                    EFBatchOperation.For(ctx, ctx.tblContasClaroLinhaRegistro).InsertAll(registros);
                   
                    ctx.SaveChanges();



                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        
        private tblContasClaroRegistroNotaFiscal CreateTabelaNotas(RegistroNotaFiscal nota, int contaId)
        {
            return new tblContasClaroRegistroNotaFiscal {
                intContaId = contaId,
                txtNotaFiscal = nota.NotaFiscal,
                txtCodigo = nota.Codigo,
                txtTipo = nota.Tipo,
                txtAliquota = nota.Aliquota,
                txtBaseCalculo = nota.BaseCalculo,
                txtValorImposto = nota.ValorImposto
            };

        }

        private tblContasClaroLinhaRegistro CreateTabelaLinhaRegistro (LinhaRegistro r, int contaId)
        {
            return new tblContasClaroLinhaRegistro {
                intContaId = contaId,
                txtDDD = r.DDD,
                txtTelefone = r.Telefone,
                txtSecao = r.Secao,
                txtData = r.Data,
                txtHora = r.Hora,
                txtOrigemUFDestino = r.OrigemUFDestino,
                txtNumero = r.Numero,
                txtDuracaoQuantidade = r.DuracaoQuantidade,
                txtTarifa = r.Tarifa,
                txtValor = r.Valor,
                txtValorCobrado = r.ValorCobrado,
                txtNome = r.Nome,
                txtMatricula = r.Matricula,
                txtSubSecao = r.SubSecao,
                txtTipoImposto = r.TipoImposto,
                txtDescricao = r.Descricao,
                txtNomeLocalOrigem = r.NomeLocalOrigem,
                txtNomeLocalDestino = r.NomeLocalDestino,
                txtCodigoLocalOrigem = r.CodigoLocalOrigem,
                txtCodigoLocalDestino = r.CodigoLocalDestino


            };
        }


    }
}
