using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Business.Commons.Entities;
using FoneClube.Business.Commons.Entities;
using FoneClube.DataAccess;
using FoneClube.Business.Commons.Entities.FoneClube;
using Newtonsoft.Json;
using FoneClube.Business.Commons.Entities.FoneClube.comission;
using FoneClube.Business.Commons.Entities.Cielo;
using FoneClube.Business.Commons.Entities.ViewModel.Comissao;
using FoneClube.Business.Commons.Entities.ViewModel.Amigos;
using Business.Commons.Utils;

namespace FoneClube.DataAccess
{
    public class ComissionAccess
    {
        public bool ReleaseComissions(int matricula)
        {
            try
            {
                //var ordensLiberadas = PrepareCustomerComissionOrders(matricula);
                var comissoes = GetComissoesCustomer(matricula);

                //var ammount = comissoes.Select(c => c.Ammount)
                //            .ToList().Take(comissoes.Count).Sum();

                using (var ctx = new FoneClubeContext())
                {
                    //if (comissoes.Count == 0)
                    //    return true;

                    foreach (var comissao in comissoes)
                    {
                        var comissaoParaLiberar = ctx.tblComissionOrder
                            .FirstOrDefault(c => c.intIdComissionOrder == comissao.Id);

                        comissaoParaLiberar.bitComissionConceded = true;
                        comissaoParaLiberar.dteConceded = DateTime.Now;
                    }

                    var bonus = ctx.tblBonusOrder.Where(b => b.intIdCustomerReceiver == matricula && b.bitComissionConceded == false).ToList();
                    foreach (var b in bonus)
                    {
                        b.bitComissionConceded = true;
                        b.dteConceded = DateTime.Now;
                    }

                    ctx.SaveChanges();

                    if (comissoes.Count > 0)
                        new ComissionAccess().AddComissionLog(comissoes, true);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool HasComission(int matricula)
        {
            try
            {
                return GetComissoesCustomer(matricula).Count > 0;
            }
            catch (Exception)
            {
                throw new Exception();  //tratar com erro customizado
            }
        }

        public Benefit GetComissionAmmount(int matriculaPai)
        {
            try
            {
                //var prepare = PrepareCustomerComissionOrders(matriculaPai);
                var comissoes = GetComissoesCustomer(matriculaPai);
                var ammount = comissoes.Select(c => c.Ammount)
                        .ToList().Take(comissoes.Count).Sum();

                if (ammount > 0)
                    return new Benefit { HasBenefit = true, Ammount = ammount };
                else
                    return new Benefit { HasBenefit = false, Ammount = 0 };
            }
            catch (Exception)
            {
                return new Benefit { HasBenefit = false, Ammount = 0 };
            }
        }

        public List<ComissaoOrdem> GetComissoesCustomer(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblComissionOrder
                        .Where(c => c.bitComissionConceded != true && c.intIdCustomerReceiver == matricula)
                        .Select(c => new ComissaoOrdem
                        {
                            Id = c.intIdComissionOrder,
                            IdRecebedor = c.intIdCustomerReceiver,
                            TransactionId = c.intIdTransaction,
                            Level = c.intIdComission.Value,
                            Ammount = c.intAmount.Value,
                            Concedida = c.bitComissionConceded,
                            TotalLinhas = c.intTotalLinhas
                        })
                        .ToList();


                    return ctx.GetComissoesCustomer(matricula).Select(o => new ComissaoOrdem
                    {
                        Id = o.intIdComissionOrder,
                        IdRecebedor = o.intIdCustomer.Value,
                        TransactionId = o.intIdTransaction,
                        Level = o.intIdComission.Value,
                        Ammount = o.intAmount,
                        Concedida = o.bitComissionConceded
                    }).ToList();

                    //return (from c in ctx.tblChargingHistory
                    //        join t in ctx.tblComissionOrder on c.intIdTransaction equals t.intIdTransaction
                    //        join l in ctx.tblCommisionLevels on t.intIdComissionOrder equals l.intLevel
                    //        where t.bitComissionConceded == false
                    //        && t.intIdCustomerReceiver == matricula
                    //        && t.intIdBonus == null
                    //        select new ComissaoOrdem
                    //        {
                    //            Id = t.intIdComissionOrder,
                    //            IdRecebedor = c.intIdCustomer.Value,
                    //            TransactionId = t.intIdTransaction,
                    //            Level = t.intIdComission.Value,
                    //            Ammount = l.intAmount,
                    //            Concedida = t.bitComissionConceded
                    //        }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<ComissaoOrdem>();
            }

        }

        public int? PrepareCustomerComissionOrders(int matricula)
        {
            using (var ctx = new FoneClubeContext())
            {
                var transacoesPagas = ctx.GetTransacoesPagas().ToList();
                var transacoesAptas = ctx.GetTransacoesAptasComissao().ToList();
                var comissions = ctx.tblCommisionLevels.Where(c => c.bitActive == true).ToList();
                var clienteArvore = GetComissoesCliente(matricula);
                var comissoesOrdem = GetComissoesParaOrdem(clienteArvore, transacoesAptas, comissions);

                if (!comissoesOrdem.Any())
                    return 0;

                var orders = new List<tblComissionOrder>();
                foreach (var comissaoOrdem in comissoesOrdem)
                {
                    orders.Add(new tblComissionOrder
                    {
                        bitComissionConceded = false,
                        dteCreated = DateTime.Now,
                        intIdTransaction = comissaoOrdem.TransactionId.Value,
                        intIdAgent = 1,
                        intIdComission = comissaoOrdem.Level,
                        intIdCustomerReceiver = matricula
                    });
                }

                ctx.tblComissionOrder.AddRange(orders);
                return ctx.SaveChanges();
            }
        }

        public bool SetupComissionOrders()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var primeirasCobrancasIds = ctx.GetPrimeirasCobrancasSemTratamento().ToList().Select(a => a.Value).ToList();
                    var primeirasCobrancas = ctx.tblChargingHistory.Where(c => primeirasCobrancasIds.Contains(c.intId)).ToList();

                    foreach (var cob in primeirasCobrancas)
                    {
                        cob.bitComissionInapt = true;
                    }

                    ctx.SaveChanges();

                    var contagemBase = ctx.GetCountComissionOrder().FirstOrDefault();

                    var transacoesAptas = ctx.GetTransacoesAptasComissao()
                                             .Select(a => new Transaction
                                             {
                                                 Id = a.intIdTransaction.ToString(),
                                                 IdCustomer = a.intIdCustomer,
                                                 TipoGateway = Transaction.Gateway.Pagarme
                                             })
                                             .ToList();

                    transacoesAptas.AddRange(ctx.GetTransacoesAptasComissaoCielo()
                                                  .Select(a => new Transaction
                                                  {
                                                      Id = a.txtPaymentId,
                                                      IdCustomer = a.intIdCustomer,
                                                      TipoGateway = Transaction.Gateway.Cielo
                                                  })
                                                  .ToList());


                    //transacoesAptas.AddRange(ctx.GetTransacoesAptasComissaoNovaLoja()
                    //    .Select(a => new Transaction
                    //    {
                    //        Id = a.intId.ToString(),
                    //        IdCustomer = a.intIdCustomer,
                    //        TipoGateway = Transaction.Gateway.Loja
                    //    }).ToList());

                    var comissionLevels = ctx.tblCommisionLevels.Where(c => c.bitActive == true).ToList();

                    foreach (var cobrancaPaga in transacoesAptas)
                    {
                        int transactionIdPagarme = -1;
                        string transactionIdCielo = string.Empty;

                        var clienteLiberaComission = cobrancaPaga.IdCustomer;

                        if (cobrancaPaga.TipoGateway == Transaction.Gateway.Pagarme)
                            transactionIdPagarme = Convert.ToInt32(cobrancaPaga.Id);

                        if (cobrancaPaga.TipoGateway == Transaction.Gateway.Cielo)
                            transactionIdCielo = cobrancaPaga.Id;

                        if (cobrancaPaga.TipoGateway == Transaction.Gateway.Loja)
                            transactionIdPagarme = Convert.ToInt32(cobrancaPaga.Id);

                        int totalTelefones = 1;
                        var contagemLinhas = ctx.GetLinhasFoneclubeAtivas(clienteLiberaComission).ToList().Count();

                        if (contagemLinhas > 0)
                            totalTelefones = contagemLinhas;

                        var primeiroPai = ctx.tblPersonsParents.FirstOrDefault(o => o.intIdSon == clienteLiberaComission);
                        var paternidade = new List<Comissao>();

                        if (!bool.Equals(primeiroPai, null))
                        {
                            if (!bool.Equals(primeiroPai.intIdParent, null))
                            {
                                foreach (var comissionLevel in comissionLevels)
                                {
                                    if (comissionLevel.intLevel == 1)
                                    {
                                        paternidade.Add(new Comissao
                                        {
                                            ComissionLevel = comissionLevel.intLevel,
                                            Ammount = comissionLevel.intAmount * totalTelefones,
                                            PaiRecebedor = primeiroPai.intIdParent,
                                            TransactionId = transactionIdPagarme,
                                            TransactionIdValue = transactionIdCielo,
                                            TotalLinhas = totalTelefones,
                                            LiberadorComissao = clienteLiberaComission,
                                            TipoGateway = cobrancaPaga.TipoGateway
                                        });
                                    }
                                    else
                                    {
                                        var ultimoPai = paternidade.LastOrDefault().PaiRecebedor;
                                        var pai = ctx.tblPersonsParents.FirstOrDefault(o => o.intIdSon == ultimoPai.Value);

                                        if (!bool.Equals(pai, null))
                                            if (!bool.Equals(pai.intIdParent, null))
                                            {
                                                paternidade.Add(new Comissao
                                                {
                                                    ComissionLevel = comissionLevel.intLevel,
                                                    Ammount = comissionLevel.intAmount * totalTelefones,
                                                    PaiRecebedor = pai.intIdParent,
                                                    TransactionId = transactionIdPagarme,
                                                    TransactionIdValue = transactionIdCielo,
                                                    TotalLinhas = totalTelefones,
                                                    LiberadorComissao = clienteLiberaComission,
                                                    TipoGateway = cobrancaPaga.TipoGateway
                                                });
                                            }
                                    }
                                }

                                foreach (var comission in paternidade)
                                {
                                    ctx.tblComissionOrder.Add(new tblComissionOrder
                                    {
                                        intIdComission = comission.ComissionLevel,
                                        intIdCustomerReceiver = comission.PaiRecebedor,
                                        intIdTransaction = comission.TransactionId.Value,
                                        txtIdTransaction = comission.TransactionIdValue,
                                        intAmount = comission.Ammount,
                                        intTotalLinhas = comission.TotalLinhas,
                                        intIdCustomerGiver = comission.LiberadorComissao,
                                        intIdGateway = Convert.ToInt32(comission.TipoGateway),
                                        dteCreated = DateTime.Now
                                    });
                                }

                                ctx.SaveChanges();
                            }
                        }
                    }

                    try
                    {
                        var total = ctx.GetCountComissionOrder().FirstOrDefault() - contagemBase;
                        ctx.tblLogComissionOrder.Add(new tblLogComissionOrder
                        {
                            bitSucesso = true,
                            dteRegister = DateTime.Now,
                            intTotalOrdensLiberadas = total
                        });
                        ctx.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ctx.tblLogComissionOrder.Add(new tblLogComissionOrder
                        {
                            bitSucesso = false,
                            dteRegister = DateTime.Now
                        });
                        ctx.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLogComissionOrder.Add(new tblLogComissionOrder
                    {
                        bitSucesso = false,
                        dteRegister = DateTime.Now
                    });
                    ctx.SaveChanges();
                }
                return false;
            }
        }

        public bool SetupArvoreComissions()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    //TODO: algoritmo arvore
                }
                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public bool UpdateBonusAmount()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.UpdateAmountBonus();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<ComissaoOrdem> GetComissoesParaOrdem(
            List<Comissao> clienteArvore,
            List<GetTransacoesAptasComissao_Result> transacoesAptas,
            List<tblCommisionLevels> comissions)
        {
            var comissoesParaOrdem = new List<ComissaoOrdem>();
            // . Suelen filha do nick entrou e pagou
            // . nick ganha level 1
            // . cardozo ganha level 2
            // . marcio ganha level 3

            var listaPaternidade = new List<Person>();


            foreach (var nivelComission in clienteArvore)
            {
                foreach (var filho in nivelComission.Filhos)
                {
                    var teste = filho;
                }
                //comissoesParaOrdem
                //    .AddRange((from c in comissao.Filhos
                //               join t in transacoesAptas
                //               on c.Id equals t.intIdCustomer
                //               select new ComissaoOrdem
                //               {
                //                   IdRecebedor = t.intIdCustomer,
                //                   TransactionId = t.intIdTransaction,
                //                   Level = comissao.ComissionLevel,
                //                   Ammount = comissao.Ammount
                //               }).ToList());
            }

            return comissoesParaOrdem;
        }

        public bool InconsistenciaPaternidade(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    int coberturaIntegridade = 30;
                    var paternidade = ctx.tblPersonsParents.ToList();

                    var pai = paternidade.FirstOrDefault(p => p.intIdSon == matricula);

                    for (int i = 0; i < coberturaIntegridade; i++)
                    {
                        var ultimoPaiId = pai.intIdParent;

                        if (ultimoPaiId == matricula)
                            return true;

                        pai = paternidade.FirstOrDefault(p => p.intIdSon == ultimoPaiId);

                        if (bool.Equals(pai, null))
                            return false;

                        if (bool.Equals(pai.intIdParent, null))
                            return false;
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InconsistenciaPaternidade(int matricula, List<tblPersonsParents> paternidade)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    int coberturaIntegridade = 30;

                    var pai = paternidade.FirstOrDefault(p => p.intIdSon == matricula);

                    for (int i = 0; i < coberturaIntegridade; i++)
                    {
                        var ultimoPaiId = pai.intIdParent;

                        if (ultimoPaiId == matricula)
                            return true;

                        pai = paternidade.FirstOrDefault(p => p.intIdSon == ultimoPaiId);

                        if (bool.Equals(pai, null))
                            return false;

                        if (bool.Equals(pai.intIdParent, null))
                            return false;
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Comissao> GetComissoesCliente(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var comissoes = new List<Comissao>();
                    var comissions = ctx.tblCommisionLevels
                        .Where(c => c.bitActive == true).ToList();

                    foreach (var comissionLevel in comissions)
                    {
                        if (comissoes.Count == 0)
                        {
                            comissoes.Add(new Comissao
                            {
                                Ammount = comissionLevel.intAmount,
                                ComissionLevel = comissionLevel.intLevel,
                                Filhos = GetFilhos(matricula)
                            });
                        }

                        if (comissoes.Count > 0 && comissionLevel.intLevel > 1)
                        {
                            comissoes.Add(new Comissao
                            {
                                Ammount = comissionLevel.intAmount,
                                ComissionLevel = comissionLevel.intLevel,
                                Filhos = GetFilhosMultiplos(comissoes.LastOrDefault().Filhos)
                            });
                        }
                    }

                    return comissoes;
                }
            }
            catch (Exception)
            {
                return new List<Comissao>();
            }
        }

        public List<Comissao> GetHierarquiaCliente(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var nonPayingCustomers = ctx.GetClientesSemPagamento().ToList();
                    var comissoes = new List<Comissao>();
                    var comissions = ctx.tblCommisionLevels
                        .Where(c => c.bitActive == true).ToList();

                    var ordens = ctx.tblComissionOrder.Where(c => c.intIdCustomerReceiver == matricula).ToList();

                    foreach (var comissionLevel in comissions)
                    {
                        if (comissoes.Count == 0)
                        {
                            comissoes.Add(new Comissao
                            {
                                Filhos = GetFilhosHierarquia(matricula, nonPayingCustomers, ordens),
                                ComissionLevel = 1
                            });
                        }

                        if (comissoes.Count > 0 && comissionLevel.intLevel > 1)
                        {
                            comissoes.Add(new Comissao
                            {
                                Filhos = GetFilhosMultiplosHierarquia(comissoes.LastOrDefault().Filhos, nonPayingCustomers, ordens),
                                ComissionLevel = comissoes.LastOrDefault().ComissionLevel + 1
                            });
                        }
                    }

                    return comissoes;
                }
            }
            catch (Exception e)
            {
                return new List<Comissao>();
            }
        }

        public List<Comissao> GetHierarquiaClienteDocument(string document)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var pessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == document);
                    if (pessoa == null)
                        return new List<Comissao>();
                    else
                        return GetHierarquiaCliente(pessoa.intIdPerson);

                }
            }
            catch (Exception e)
            {
                return new List<Comissao>();
            }
        }



        public List<Person> GetHierarquiaPagante(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetFilhosHierarquiaPagante(matricula).ToList()
                    .Select(p => new Person
                    {
                        Id = p.intIdSon.Value,
                        IdPagarme = p.intIdPagarme.Value,
                        DocumentNumber = p.txtDocumentNumber,
                        Name = p.txtName,
                        Email = p.txtEmail
                    }).ToList();

                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        public List<Person> GetHierarquiaNaoPagante(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetFilhosHierarquiaNaoPagante(matricula).ToList()
                    .Select(p => new Person
                    {
                        Id = p.intIdSon.Value,
                        IdPagarme = p.intIdPagarme,
                        DocumentNumber = p.txtDocumentNumber,
                        Name = p.txtName,
                        Email = p.txtEmail
                    }).ToList();

                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }


        public string GetListaPais(List<Person> pais)
        {
            var listaPais = string.Empty;
            foreach (var pai in pais)
            {
                listaPais += pai.Id + ",";
            }

            return listaPais;
        }

        public List<Person> GetFilhosMultiplos(List<Person> pais)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetMutiplosFilhos(GetListaPais(pais)).Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme
                    }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        public List<Person> GetFilhos(int matriculaPai)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetFilhos(matriculaPai).Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme
                    }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        private List<Person> GetFilhosMultiplosHierarquia(List<Person> pais, List<GetClientesSemPagamento_Result> nonPayingCustomers, List<tblComissionOrder> ordens)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var liberadas = ordens.Where(c => c.bitComissionConceded == true).ToList();
                    var pendentes = ordens.Where(c => c.bitComissionConceded == false).ToList();

                    return ctx.GetMutiplosFilhosHierarquia(GetListaPais(pais))
                    .Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme,
                        Name = f.txtName,
                        Email = f.txtEmail,
                        DocumentNumber = f.txtDocumentNumber,
                        SemPagamento = nonPayingCustomers.Any(n => n.intIdPerson == f.intIdSon.Value),
                        Pai = new Pai { Id = f.intIdParent.Value, Name = f.txtNomePai },
                        ComissionDetails = new ComissionDetails
                        {
                            ValorTotalPago = liberadas.Where(c => c.intIdCustomerGiver == f.intIdSon.Value).ToList().Select(c => c.intAmount).Take(ordens.Count).Sum().Value,
                            ValorTotalLiberadoParaPagar = pendentes.Where(c => c.intIdCustomerGiver == f.intIdSon.Value).ToList().Select(c => c.intAmount).Take(ordens.Count).Sum().Value
                        }
                        //,
                        //DataUltimaCobranca = nonPayingCustomers.FirstOrDefault(n => n.intIdPerson == f.intIdSon.Value).dataUltimaCobranca
                    }).Where(f => !string.IsNullOrEmpty(f.Name)).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        public List<Person> GetFilhosMultiplosHierarquia(List<Person> pais)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetMutiplosFilhosHierarquia(GetListaPais(pais))
                    .Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme,
                        Name = f.txtName,
                        Email = f.txtEmail,
                        DocumentNumber = f.txtDocumentNumber
                    }).Where(f => !string.IsNullOrEmpty(f.Name)).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        public List<Person> GetFilhosHierarquia(int matriculaPai)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetFilhosHierarquia(matriculaPai)
                    .Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme,
                        DocumentNumber = f.txtDocumentNumber,
                        Name = f.txtName,
                        Email = f.txtEmail
                    }).Where(f => !string.IsNullOrEmpty(f.Name)).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        private List<Person> GetFilhosHierarquia(int matriculaPai, List<GetClientesSemPagamento_Result> nonPayingCustomers, List<tblComissionOrder> ordens)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var liberadas = ordens.Where(c => c.bitComissionConceded == true).ToList();
                    var pendentes = ordens.Where(c => c.bitComissionConceded == false).ToList();

                    return ctx.GetFilhosHierarquia(matriculaPai)
                    .Select(f => new Person
                    {
                        Id = f.intIdSon.Value,
                        IdPagarme = f.intIdPagarme,
                        DocumentNumber = f.txtDocumentNumber,
                        Name = f.txtName,
                        Email = f.txtEmail,
                        SemPagamento = nonPayingCustomers.Any(n => n.intIdPerson == f.intIdSon.Value),
                        IdParent = matriculaPai,
                        ComissionDetails = new ComissionDetails
                        {
                            ValorTotalPago = liberadas.Where(c => c.intIdCustomerGiver == f.intIdSon.Value).ToList().Select(c => c.intAmount).Take(ordens.Count).Sum().Value,
                            ValorTotalLiberadoParaPagar = pendentes.Where(c => c.intIdCustomerGiver == f.intIdSon.Value).ToList().Select(c => c.intAmount).Take(ordens.Count).Sum().Value
                        }
                    }).Where(f => !string.IsNullOrEmpty(f.Name)).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Person>();
            }
        }

        public bool InsertBonusComission(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblComissionOrder.Add(new tblComissionOrder
                    {
                        bitComissionConceded = false,
                        intIdBonus = 1,
                        intIdCustomerReceiver = person.Id,
                        dteCreated = DateTime.Now
                    });

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }


        }


        public bool AddComissionLog(List<ComissaoOrdem> comissions, bool statusDispatched)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    foreach (var comission in comissions)
                    {
                        var comissionLog = ctx.tblComissionValidationLog.FirstOrDefault(c => c.intIdComission == comission.Id);

                        if (bool.Equals(comissionLog, null))
                        {
                            ctx.tblComissionValidationLog.Add(new tblComissionValidationLog
                            {
                                bitComissionConceded = true,
                                bitComissionDispatched = statusDispatched,
                                dteCreated = DateTime.Now,
                                intIdComission = comission.Id
                            });
                        }
                        else
                        {
                            comissionLog.bitComissionDispatched = statusDispatched;
                            //comissionLog.dteUpdate = DateTime.Now;
                        }
                    }
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<GetFilhosSemPai_Result> GetFilhosSemPai()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetFilhosSemPai().ToList();
                }
            }
            catch (Exception)
            {
                return new List<GetFilhosSemPai_Result>();
            }
        }

        public List<CustomerComission> GetClientesListadosPaternidadeInfo()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var clientes = ctx.tblPersons.Where(p => p.bitDesativoManual != true && p.bitDelete != true)
                                    .Select(p => new CustomerComission
                                    {
                                        Id = p.intIdPerson,
                                        Name = p.txtName,
                                        Email = p.txtEmail
                                    }).ToList();

                    var paternidade = ctx.tblPersonsParents.ToList();
                    var filhosSemPai = GetFilhosSemPai();

                    foreach (var cliente in clientes)
                    {
                        cliente.Listado = paternidade.Any(p => p.intIdSon == cliente.Id);
                        cliente.SemPai = filhosSemPai.Any(p => p.intIdPerson == cliente.Id);
                        cliente.InconsistenciaPaternidade = InconsistenciaPaternidade(cliente.Id, paternidade);
                    }

                    return clientes;
                }
            }
            catch (Exception)
            {
                return new List<CustomerComission>();
            }
        }

        public bool SetupDesconto()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var customerPhonesCobrancasPagas = ctx.GetCustomerPhonesUltimasCobrancasPagasGeneric().ToList();
                    var customerPhonesCobrancasPagasSemDescontoConcedido = customerPhonesCobrancasPagas.Where(a => a.bitDescontoConcedido != true);
                    var ownersComPagamento = customerPhonesCobrancasPagas.Select(a => a.intIdPerson).ToList();
                    var paternidade = ctx.tblPersonsParents.ToList();
                    var paternidadeStatusFilhos = ctx.GetPaternidadeStatusFilhos().ToList();
                    var historicoComOrdensFeitas = ctx.tblDescontoOrder.Select(a => a.intIdChargingHistory).ToList();

                    //testar com dado mock
                    var historicoComOrdensNaoFeitas = customerPhonesCobrancasPagasSemDescontoConcedido
                        .Select(o => o.intIdCharging).ToList()
                        .Where(a => !historicoComOrdensFeitas.Contains(a)).ToList();

                    var linhasComPagamentoPendenteDeOrdemHistorico = customerPhonesCobrancasPagasSemDescontoConcedido
                                                                     .Where(a => historicoComOrdensNaoFeitas.Contains(a.intIdCharging));

                    var paisDistintos = paternidadeStatusFilhos
                        .Where(p => p.intIdParent != null && p.intIdParent != 0)
                        .Select(a => a.intIdParent).Distinct().ToList();

                    var ordensFeitas = ctx.tblDescontoOrder.Where(a => paisDistintos.Contains(a.intIdCustomerReceiver)).ToList();

                    var listaPai = new List<PaiTotalizador>();
                    foreach (var pai in paisDistintos)
                    {
                        var contagemFilhos = paternidadeStatusFilhos.Where(a => a.intIdParent == pai && a.bitDelete != true && a.bitDesativoManual != true)
                                               .ToList().Count();

                        if (pai != null)
                        {
                            listaPai.Add(new PaiTotalizador
                            {
                                Matricula = pai.Value,
                                TotalFihos = contagemFilhos
                            });
                        }

                    }

                    foreach (var linhaFilho in linhasComPagamentoPendenteDeOrdemHistorico)
                    {
                        var paiSelecionado = paternidade.FirstOrDefault(a => a.intIdSon == linhaFilho.intIdPerson);


                        if (paiSelecionado != null)
                        {
                            var statusPai = listaPai.FirstOrDefault(a => a.Matricula == paiSelecionado.intIdParent);
                            var descontoMetade = .5;

                            ctx.tblDescontoOrder.Add(new tblDescontoOrder
                            {
                                intIdChargingHistory = linhaFilho.intIdCharging,
                                intIdCustomerReceiver = statusPai.Matricula,
                                intIdPhone = Convert.ToInt32(linhaFilho.intPhone),
                                intIdPhonePlan = linhaFilho.intIdPlan,
                                bitDescontoConceded = false,
                                bitDescontoExtra = false,
                                dteCreated = DateTime.Now,
                                intAmount = Convert.ToInt32(linhaFilho.intCost * descontoMetade)
                            });

                            //seção bônus de desconto
                            if (statusPai.TotalFihos > 2)
                            {
                                var ordensParaCliente = ordensFeitas.Where(a => a.intIdCustomerReceiver == paiSelecionado.intIdParent).ToList();


                                if (statusPai.TotalFihos == 3)
                                {
                                    var hadDescontoExtraTerceiro = ordensParaCliente.Any(a => a.bitDescontoTerceiro == true);
                                    if (!hadDescontoExtraTerceiro)
                                    {
                                        for (int i = 0; i < statusPai.TotalFihos; i++)
                                        {
                                            ctx.tblDescontoOrder.Add(new tblDescontoOrder
                                            {
                                                intIdChargingHistory = linhaFilho.intIdCharging,
                                                intIdCustomerReceiver = statusPai.Matricula,
                                                intIdPhone = Convert.ToInt32(linhaFilho.intPhone),
                                                intIdPhonePlan = linhaFilho.intIdPlan,
                                                bitDescontoConceded = false,
                                                bitDescontoExtra = false,
                                                dteCreated = DateTime.Now,
                                                intAmount = Convert.ToInt32(linhaFilho.intCost * descontoMetade),
                                                bitDescontoTerceiro = true
                                            });
                                        }
                                    }
                                }

                                if (statusPai.TotalFihos == 6)
                                {
                                    var hadDescontoExtraSexto = ordensParaCliente.Any(a => a.bitDescontoSexto == true);
                                    if (!hadDescontoExtraSexto)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            ctx.tblDescontoOrder.Add(new tblDescontoOrder
                                            {
                                                intIdChargingHistory = linhaFilho.intIdCharging,
                                                intIdCustomerReceiver = statusPai.Matricula,
                                                intIdPhone = Convert.ToInt32(linhaFilho.intPhone),
                                                intIdPhonePlan = linhaFilho.intIdPlan,
                                                bitDescontoConceded = false,
                                                bitDescontoExtra = false,
                                                dteCreated = DateTime.Now,
                                                intAmount = Convert.ToInt32(linhaFilho.intCost * descontoMetade),
                                                bitDescontoSexto = true
                                            });
                                        }
                                    }
                                }

                                if (statusPai.TotalFihos == 10)
                                {
                                    var hadDescontoExtraDecimo = ordensParaCliente.Any(a => a.bitDescontoDecimo == true);
                                    if (!hadDescontoExtraDecimo)
                                    {
                                        //definir
                                    }
                                }
                            }


                        }
                        else
                        {
                            //sem pai
                        }
                    }

                    //no historico com ordem não feita, fazer odem


                    //pegar pagamentos confirmados pagarme para liberar benefícios
                    //utilizar informações e contagens pra definir benefícios


                    //saiu filho cortamos os extras
                    //limitar ao sair, marcados como extra
                    //bonus 3 meses precisa ter 3 filhos a cada liberação
                    //bonus 6 meses precisa ter 6 filhos a cada liberação

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public class PaiTotalizador
        {
            public int Matricula { get; set; }
            public int TotalFihos { get; set; }
        }


        public bool SetupBonus()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        var update = new ComissionAccess().UpdateProperties();
                    }
                    catch (Exception e) { }

                    var linhasCobrancasAptasLiberarBonus = new List<Charging>();
                    var linhasAptasLiberarBonus = new List<FoneClube.Business.Commons.Entities.FoneClube.Phone>();
                    var linhasMalComportamento = new List<FoneClube.Business.Commons.Entities.FoneClube.Phone>();
                    var linhasPendentePagamento = new List<FoneClube.Business.Commons.Entities.FoneClube.Phone>();
                    var clientesSemPaiQueNaoRecemBonus = new List<Person>();
                    var ordensBonusLiberadas = new List<tblBonusOrder>();
                    var linhasSemEvento = new List<FoneClube.Business.Commons.Entities.FoneClube.Phone>();
                    var linhasComErroAoSalvar = new List<FoneClube.Business.Commons.Entities.FoneClube.Phone>();


                    var customerPhonesCobrancasPagas = ctx.GetCustomerPhonesUltimasCobrancasPagas()
                                      .Where(a => a.bitBonusConceded != true && (a.dataUltimaCobrancaPaga != null || a.dataUltimaCobrancaPagaCielo != null)).ToList();

                    var customers = customerPhonesCobrancasPagas
                        .Where(p => p.intIdPerson != null)
                        .Select(p => new Person
                        {
                            Id = p.intIdPerson.Value,
                            Charging = new Charging
                            {
                                Id = p.dataUltimaCobrancaPagaCielo == null ? Convert.ToInt32(p.intIdCharging) : Convert.ToInt32(p.intIdChargingCielo),
                                CreateDate = p.dataUltimaCobrancaPagaCielo == null ? p.dataUltimaCobrancaPaga : p.dataUltimaCobrancaPagaCielo,
                                GatewayId = p.dataUltimaCobrancaPagaCielo == null ? 1 : 2
                            }
                        })
                        .GroupBy(customer => customer.Id)
                        .Select(group => group.First())
                        .ToList();


                    foreach (var customer in customers)
                    {
                        var phones = customerPhonesCobrancasPagas
                            .Where(c => c.intIdPerson == customer.Id)
                            .Select(d => new Phone { Id = d.intIdCustomerPhone, DataEntrada = d.dteEntradaLinha })
                            .ToList();

                        var properties = ctx.tblPhonePropertyHistory
                            .Where(p => p.intIdPerson == customer.Id)
                            .ToList();

                        var cobrancasPagasPagarme = (from c in ctx.tblChargingHistory
                                                     join t in ctx.tblFoneclubePagarmeTransactions on c.intIdTransaction equals t.intIdTransaction
                                                     where c.intIdCustomer == customer.Id
                                                     && t.txtOutdadetStatus == "Paid"
                                                     select new Charging
                                                     {
                                                         Id = c.intId,
                                                         GatewayId = c.intIdTransaction,
                                                         CreateDate = c.dteCreate,
                                                         Comment = c.txtComment
                                                     }).ToList();

                        var cobrancasPagasCielo = customerPhonesCobrancasPagas
                                                    .Where(c => c.dataUltimaCobrancaPagaCielo != null && c.intIdPerson == customer.Id)
                                                    .Select(a => new Charging
                                                    {
                                                        Id = Convert.ToInt32(a.intIdChargingCielo),
                                                        CreateDate = a.dataUltimaCobrancaPagaCielo
                                                    }).ToList();

                        cobrancasPagasPagarme.AddRange((from c in ctx.tblChargingHistory
                                                        where c.intIdCustomer == customer.Id
                                                        && c.bitPago == true
                                                        select new Charging
                                                        {
                                                            Id = c.intId,
                                                            GatewayId = c.intIdTransaction,
                                                            CreateDate = c.dteCreate,
                                                            Comment = c.txtComment
                                                        }).ToList());

                        foreach (var cob in cobrancasPagasPagarme)
                            cob.CreateDate = Convert.ToDateTime(cob.CreateDate).Date;

                        foreach (var cob in cobrancasPagasCielo)
                            cob.CreateDate = Convert.ToDateTime(cob.CreateDate).Date;

                        foreach (var phone in phones)
                        {
                            var ultimoEventoDaLinha = properties.Where(p => p.intIdPhone == phone.Id && p.intIdStatus != null)
                                .OrderByDescending(o => o.dteChange).FirstOrDefault();

                            if (ultimoEventoDaLinha == null)
                            {
                                ultimoEventoDaLinha = new tblPhonePropertyHistory
                                {
                                    intIdStatus = Convert.ToInt32(Phone.PhoneStatus.Ativa),
                                    dteEntrada = phone.DataEntrada
                                };
                            }


                            if (!bool.Equals(ultimoEventoDaLinha, null))
                            {
                                if (ultimoEventoDaLinha.intIdStatus == Convert.ToInt32(Phone.PhoneStatus.Ativa))
                                {
                                    var dataAtivacao = Convert.ToDateTime(ultimoEventoDaLinha.dteEntrada).Date;

                                    var cobrancaPagarme = cobrancasPagasPagarme.FirstOrDefault(c => c.CreateDate >= dataAtivacao);
                                    var cobrancaCielo = cobrancasPagasCielo.FirstOrDefault(c => c.CreateDate >= dataAtivacao);

                                    if (bool.Equals(cobrancaPagarme, null))
                                        cobrancaPagarme = cobrancasPagasPagarme.FirstOrDefault(c => c.Comment == "Checkout Loja Pagarme" || c.Comment == "Coletado em restauração automática");


                                    if (!bool.Equals(cobrancaPagarme, null))
                                        SegmentarLinhaApta(cobrancaPagarme, linhasAptasLiberarBonus, phone, linhasCobrancasAptasLiberarBonus);
                                    else if (!bool.Equals(cobrancaCielo, null))
                                        SegmentarLinhaApta(cobrancaCielo, linhasAptasLiberarBonus, phone, linhasCobrancasAptasLiberarBonus);
                                    else
                                        linhasPendentePagamento.Add(phone);

                                }
                                else
                                {
                                    linhasMalComportamento.Add(phone);
                                }
                            }
                            else
                            {
                                linhasSemEvento.Add(phone);
                            }
                        }
                    }

                    var listaIds = linhasCobrancasAptasLiberarBonus.Select(a => a.Phones.FirstOrDefault().Id).ToList();
                    var personsPhonesAptos = ctx.tblPersonsPhones.Where(pp => listaIds.Contains(pp.intId)).ToList();
                    var ownersLiberadores = personsPhonesAptos.Select(a => a.intIdPerson).ToList();
                    var pais = ctx.tblPersonsParents.Where(pp => ownersLiberadores.Contains(pp.intIdSon)).ToList();
                    var plans = ctx.tblPlansOptions.ToList();

                    foreach (var cobrancaLinha in linhasCobrancasAptasLiberarBonus)
                    {
                        var detalhesLinha = personsPhonesAptos.FirstOrDefault(a => a.intId == cobrancaLinha.Phones.FirstOrDefault().Id);
                        var pai = pais.FirstOrDefault(p => p.intIdSon == detalhesLinha.intIdPerson);

                        if (!bool.Equals(pai, null))
                        {
                            if (!bool.Equals(pai.intIdParent, null))
                            {
                                var ordemBonus = new tblBonusOrder
                                {
                                    intIdChargingHistory = cobrancaLinha.Id,
                                    dteCreated = DateTime.Now,
                                    intIdCustomerReceiver = pai.intIdParent,
                                    intIdPhone = detalhesLinha.intId,
                                    intPercentBonus = 50,
                                    intIdPhonePlan = detalhesLinha.intIdPlan,
                                    bitComissionConceded = false
                                };
                                try
                                {
                                    ordemBonus.intAmount = plans.FirstOrDefault(p => p.intIdPlan == detalhesLinha.intIdPlan).intCost / 2;
                                }
                                catch (Exception)
                                {
                                    UpdateBonusAmount();
                                }

                                var linhaParaBaixa = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == detalhesLinha.intId);

                                try
                                {

                                    linhaParaBaixa.bitBonusConceded = true;
                                    ctx.tblBonusOrder.Add(ordemBonus);
                                    ctx.SaveChanges();
                                    ordensBonusLiberadas.Add(ordemBonus);
                                }
                                catch (Exception e)
                                {
                                    linhasComErroAoSalvar.Add(new Phone { Id = linhaParaBaixa.intId });
                                }
                            }
                            else
                            {
                                clientesSemPaiQueNaoRecemBonus.Add(new Person { Id = detalhesLinha.intIdPerson.Value });
                            }
                        }
                        else
                        {
                            clientesSemPaiQueNaoRecemBonus.Add(new Person { Id = detalhesLinha.intIdPerson.Value });
                        }

                    }

                    try
                    {
                        var clientesNaoPagantes = (from o in linhasPendentePagamento
                                                   join p in ctx.tblPersonsPhones on o.Id equals p.intId
                                                   select new
                                                   {
                                                       Value = p.intIdPerson.Value,
                                                       Name = string.Empty
                                                   }).Distinct()
                                                .Select(p => new tblBonusOrderException
                                                {
                                                    bitAtivo = false,
                                                    dteCreated = DateTime.Now,
                                                    intIdPerson = p.Value
                                                }).ToList();

                        ctx.Database.ExecuteSqlCommand("TRUNCATE TABLE tblBonusOrderException");
                        ctx.tblBonusOrderException.AddRange(clientesNaoPagantes);
                        ctx.SaveChanges();
                    }
                    catch (Exception) { }


                    try
                    {
                        ctx.tblBonusOrderLog.Add(new tblBonusOrderLog
                        {
                            dteRegister = DateTime.Now,
                            txtLinhasAptasLiberarBonus = linhasAptasLiberarBonus.Count() > 0 ? linhasAptasLiberarBonus.Count().ToString() + "_" + JsonConvert.SerializeObject(linhasAptasLiberarBonus) : string.Empty,
                            txtLinhasComErroAoSalvar = linhasComErroAoSalvar.Count() > 0 ? linhasComErroAoSalvar.Count().ToString() + "_" + JsonConvert.SerializeObject(linhasComErroAoSalvar) : string.Empty,
                            txtLinhasMalComportamento = linhasMalComportamento.Count() > 0 ? linhasMalComportamento.Count().ToString() + "_" + JsonConvert.SerializeObject(linhasMalComportamento) : string.Empty,
                            txtLinhasPendentePagamento = linhasPendentePagamento.Count() > 0 ? linhasPendentePagamento.Count().ToString() + "_" + JsonConvert.SerializeObject(linhasPendentePagamento) : string.Empty,
                            txtLinhasSemEvento = linhasSemEvento.Count() > 0 ? linhasSemEvento.Count().ToString() + "_" + JsonConvert.SerializeObject(linhasSemEvento) : string.Empty,
                            txtLlientesSemPaiQueNaoRecemBonus = clientesSemPaiQueNaoRecemBonus.Count() > 0 ? clientesSemPaiQueNaoRecemBonus.Count().ToString() + "_" + JsonConvert.SerializeObject(clientesSemPaiQueNaoRecemBonus) : string.Empty,
                            txtOrdensBonusLiberadas = ordensBonusLiberadas.Count() > 0 ? ordensBonusLiberadas.Count().ToString() + "_" + JsonConvert.SerializeObject(ordensBonusLiberadas) : string.Empty
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }

                    UpdateBonusAmount();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void SegmentarLinhaApta(Charging cobranca, List<Phone> linhasAptasLiberarBonus, Phone phone, List<Charging> linhasCobrancasAptasLiberarBonus)
        {
            linhasAptasLiberarBonus.Add(phone);

            linhasCobrancasAptasLiberarBonus.Add(new Charging
            {
                Id = cobranca.Id,
                Phones = new List<Phone> { phone }
            });
        }

        public ListaLogBonus GetBonusLog()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    //refact
                    var phones = ctx.tblPersonsPhones.ToList();
                    var persons = ctx.tblPersons.ToList();
                    //var teste = (from pp in ctx.tblPersonsPhones
                    // join t in listaLog.LinhasAptasBonus on pp.intId equals t.Id
                    // join p in ctx.tblPersons on pp.intIdPerson equals p.intIdPerson
                    // select new Phone {
                    //     Id = t.Id,
                    //     Owner = pp.intIdPerson,
                    //     OwnerName = p.txtName,
                    //     OwnerDocument = p.txtDocumentNumber
                    // }).ToList();

                    var log = ctx.tblBonusOrderLog.Where(p => p.intId > 0)
                                .OrderByDescending(o => o.dteRegister).FirstOrDefault();

                    var listaLog = new ListaLogBonus();

                    listaLog.DataRegistroLog = log.dteRegister;

                    var linhasAptasBonus = log.txtLinhasAptasLiberarBonus.Replace(log.txtLinhasAptasLiberarBonus.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasAptasBonus = JsonConvert.DeserializeObject<List<Phone>>(linhasAptasBonus);

                    if (listaLog.LinhasAptasBonus != null)
                        PopulaLinhaLog(listaLog.LinhasAptasBonus, phones, persons);

                    var linhasComErroAoSalvar = log.txtLinhasComErroAoSalvar.Replace(log.txtLinhasComErroAoSalvar.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasComErroAoSalvar = JsonConvert.DeserializeObject<List<Phone>>(linhasComErroAoSalvar);

                    if (listaLog.LinhasComErroAoSalvar != null)
                        PopulaLinhaLog(listaLog.LinhasComErroAoSalvar, phones, persons);

                    var linhasMalComportamento = log.txtLinhasMalComportamento.Replace(log.txtLinhasMalComportamento.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasMalComportamento = JsonConvert.DeserializeObject<List<Phone>>(linhasMalComportamento);

                    if (listaLog.LinhasMalComportamento != null)
                        PopulaLinhaLog(listaLog.LinhasMalComportamento, phones, persons);

                    var linhasPendentes = log.txtLinhasPendentePagamento.Replace(log.txtLinhasPendentePagamento.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasPendentePagamento = JsonConvert.DeserializeObject<List<Phone>>(linhasPendentes);

                    if (listaLog.LinhasPendentePagamento != null)
                        PopulaLinhaLog(listaLog.LinhasPendentePagamento, phones, persons);

                    var linhasSemEvento = log.txtLinhasSemEvento.Replace(log.txtLinhasSemEvento.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasSemEvento = JsonConvert.DeserializeObject<List<Phone>>(linhasSemEvento);

                    if (listaLog.LinhasSemEvento != null)
                        PopulaLinhaLog(listaLog.LinhasSemEvento, phones, persons);

                    var linhasSemPaiQueNaoRecebemBonus = log.txtLlientesSemPaiQueNaoRecemBonus.Replace(log.txtLlientesSemPaiQueNaoRecemBonus.Split('_')[0] + "_", string.Empty);
                    listaLog.LinhasSemPaiQueNaoRecebemBonus = JsonConvert.DeserializeObject<List<Phone>>(linhasSemPaiQueNaoRecebemBonus);

                    if (listaLog.LinhasSemPaiQueNaoRecebemBonus != null)
                        PopulaLinhaLog(listaLog.LinhasSemPaiQueNaoRecebemBonus, phones, persons);

                    if (log.txtOrdensBonusLiberadas.Length > 0)
                    {
                        var ordensBonusLiberadas = log.txtOrdensBonusLiberadas
                            .Replace(log.txtOrdensBonusLiberadas.Split('_')[0] + "_", string.Empty)
                            .Replace("intIdPhone", "Id");
                        var ordensBonusLiberadasDeserializadas = JsonConvert.DeserializeObject<List<Phone>>(ordensBonusLiberadas);


                        listaLog.LinhasOrdemBonusLiberada = JsonConvert.DeserializeObject<List<Phone>>(ordensBonusLiberadas);

                        if (listaLog.LinhasOrdemBonusLiberada != null)
                            PopulaLinhaLog(listaLog.LinhasOrdemBonusLiberada, phones, persons);
                    }


                    return listaLog;
                }
            }
            catch (Exception e)
            {
                return new ListaLogBonus();
            }
        }

        public void PopulaLinhaLog(List<Phone> phones, List<tblPersonsPhones> personsPhones, List<tblPersons> tblPersons)
        {
            foreach (var linha in phones)
            {
                var phone = personsPhones.FirstOrDefault(l => l.intId == linha.Id);
                if (phone != null)
                {
                    linha.Owner = phone.intIdPerson;
                    var person = tblPersons.FirstOrDefault(p => p.intIdPerson == linha.Owner);
                    if (person != null)
                    {
                        linha.OwnerDocument = person.txtDocumentNumber;
                        linha.OwnerName = person.txtName;
                    }
                }
            }
        }

        public List<BonusOrderReport> GetHistoryBonusOrder(int? total)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var planos = ctx.tblPlansOptions.ToList();

                    int totalComission = Convert.ToInt32(total);

                    var bonus = ctx.tblBonusOrder
                        .Where(c => c.intIdComissionOrder > 0)
                        .Select(b => new BonusOrder
                        {
                            intIdComissionOrder = b.intIdComissionOrder,
                            intIdAgent = b.intIdAgent,
                            intIdChargingHistory = b.intIdChargingHistory,
                            intIdCustomerReceiver = b.intIdCustomerReceiver,
                            intIdPhone = b.intIdPhone,
                            intIdPhonePlan = b.intIdPhonePlan,
                            intIdTransaction = b.intIdTransaction,
                            intPercentBonus = b.intPercentBonus,
                            bitComissionConceded = b.bitComissionConceded,
                            dteConceded = b.dteConceded,
                            dteCreated = b.dteCreated,
                            dteValidity = b.dteValidity
                        })
                        .Take(totalComission).ToList().OrderByDescending(r => r.dteCreated);

                    foreach (var b in bonus)
                    {
                        var plan = planos.FirstOrDefault(p => p.intIdPlan == b.intIdPhonePlan);

                        if (plan == null)
                            b.intPlanValue = 0;
                        else
                        {
                            b.intPlanValue = plan.intCost;
                            b.intBonusValue = plan.intCost * (b.intPercentBonus / 100.00);
                        }

                    }

                    var liberadores = bonus.Select(c => c.intIdChargingHistory).ToList();

                    var detalhesLiberador = (from a in bonus
                                             join t in ctx.tblChargingHistory on a.intIdChargingHistory equals t.intId
                                             join p in ctx.tblPersons on t.intIdCustomer equals p.intIdPerson
                                             select new Person
                                             {
                                                 Id = p.intIdPerson,
                                                 DocumentNumber = p.txtDocumentNumber,
                                                 Name = p.txtName,
                                                 Charging = new Charging
                                                 {
                                                     Id = t.intId
                                                 }
                                             }).ToList();

                    var detalhes = (from a in bonus
                                    join p in ctx.tblPersons on a.intIdCustomerReceiver equals p.intIdPerson
                                    join t in ctx.tblChargingHistory on a.intIdChargingHistory equals t.intId
                                    select new BonusOrderReport
                                    {
                                        Recebedor = new Person
                                        {
                                            Id = p.intIdPerson,
                                            DocumentNumber = p.txtDocumentNumber,
                                            Name = p.txtName
                                        },
                                        Liberador = new Person
                                        {
                                            Id = detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory) == null ? 0 : detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory).Id,
                                            Name = detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory) == null ? string.Empty : detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory).Name,
                                            DocumentNumber = detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory) == null ? string.Empty : detalhesLiberador.FirstOrDefault(c => c.Charging.Id == a.intIdChargingHistory).DocumentNumber,

                                            Charging = new Charging
                                            {
                                                Id = t.intId,
                                                ChargingDate = t.dteCreate,
                                                Ammount = t.txtAmmountPayment,
                                                Comment = t.txtComment,
                                                TransactionId = a.intIdTransaction
                                            }
                                        },
                                        Order = a

                                    }).ToList();


                    return detalhes;
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public List<ComissionOrderReport> GetHistoryComissionOrder(int? total)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    int totalComission = Convert.ToInt32(total);

                    var comissoes = ctx.tblComissionOrder
                        .Where(c => c.intIdComission > 0)
                        .Select(a => new ComissionOrder
                        {
                            intIdComissionOrder = a.intIdComissionOrder,
                            intIdAgent = a.intIdAgent,
                            intIdBonus = a.intIdBonus,
                            intIdComission = a.intIdComission,
                            intIdCustomerGiver = a.intIdCustomerGiver,
                            intIdCustomerReceiver = a.intIdCustomerReceiver,
                            intIdTransaction = a.intIdTransaction,
                            intTotalLinhas = a.intTotalLinhas,
                            bitComissionConceded = a.bitComissionConceded,
                            dteConceded = a.dteConceded,
                            dteCreated = a.dteCreated,
                            dteValidity = a.dteValidity,
                            intAmount = a.intAmount
                        })
                        .Take(totalComission)
                        .ToList().OrderByDescending(r => r.dteCreated);

                    var liberadores = comissoes.Select(c => c.intIdTransaction).ToList();

                    var detalhesLiberador = (from a in comissoes
                                             join p in ctx.tblPersons on a.intIdCustomerGiver equals p.intIdPerson
                                             join t in ctx.tblChargingHistory on a.intIdTransaction equals t.intIdTransaction
                                             select new Person
                                             {
                                                 Id = p.intIdPerson,
                                                 DocumentNumber = p.txtDocumentNumber,
                                                 Name = p.txtName
                                             }).ToList();


                    var detalhes = (from a in comissoes
                                    join p in ctx.tblPersons on a.intIdCustomerReceiver equals p.intIdPerson
                                    join t in ctx.tblChargingHistory on a.intIdTransaction equals t.intIdTransaction
                                    select new ComissionOrderReport
                                    {
                                        Recebedor = new Person
                                        {
                                            Id = p.intIdPerson,
                                            DocumentNumber = p.txtDocumentNumber,
                                            Name = p.txtName
                                        },
                                        Liberador = new Person
                                        {
                                            Id = detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver) == null ? 0 : detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver).Id,
                                            Name = detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver) == null ? string.Empty : detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver).Name,
                                            DocumentNumber = detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver) == null ? string.Empty : detalhesLiberador.FirstOrDefault(c => c.Id == a.intIdCustomerGiver).DocumentNumber,

                                            Charging = new Charging
                                            {
                                                Id = t.intId,
                                                ChargingDate = t.dteCreate,
                                                Ammount = t.txtAmmountPayment,
                                                Comment = t.txtComment,
                                                TransactionId = a.intIdTransaction
                                            }
                                        },
                                        Order = a

                                    }).ToList();

                    return detalhes;
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public struct ListaLogBonus
        {
            public DateTime DataRegistroLog { get; set; }
            public List<Phone> LinhasAptasBonus { get; set; }
            public List<Phone> LinhasMalComportamento { get; set; }
            public List<Phone> LinhasPendentePagamento { get; set; }
            public List<Phone> LinhasSemPaiQueNaoRecebemBonus { get; set; }
            public List<Phone> LinhasOrdemBonusLiberada { get; set; }
            public List<Phone> LinhasSemEvento { get; set; }
            public List<Phone> LinhasComErroAoSalvar { get; set; }
        }



        public bool UpdateProperties()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var properties = ctx.tblPhonePropertyHistory
                                        .Where(p => p.intIdPhone == 0).ToList();

                    foreach (var property in properties)
                    {
                        var phone = ctx.tblPersonsPhones
                                       .FirstOrDefault(p => p.intDDD == property.intPhoneDDD
                                       && p.intPhone == property.intPhoneNumber
                                       && p.bitAtivo == true
                                       && p.intIdPerson == property.intIdPerson);

                        if (!bool.Equals(phone, null))
                        {
                            property.intIdPhone = phone.intId;
                            ctx.SaveChanges();
                        }

                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public decimal GetCustomerBonusPlanPrice(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var plansOptions = ctx.tblPlansOptions.ToList();
                    var bonus = ctx.tblBonusOrder.Where(b => b.intIdCustomerReceiver == matricula && b.bitComissionConceded == false).ToList();
                    decimal amount = 0;

                    foreach (var ordem in bonus)
                    {
                        decimal amountPlan;
                        var planOption = plansOptions.FirstOrDefault(p => p.intIdPlan == ordem.intIdPhonePlan);
                        if (!bool.Equals(planOption, null))
                        {
                            if (!bool.Equals(planOption.intCost, null))
                            {
                                amountPlan = Convert.ToDecimal(planOption.intCost);
                                var percent = Convert.ToDecimal(Convert.ToInt32(ordem.intPercentBonus) / 100.00);
                                amount += amountPlan * percent;
                            }
                            else
                            {
                                var teste = 1;
                            }
                        }
                        else
                        {
                            var teste2 = 1;
                        }
                    }

                    return amount;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public decimal GetCustomerBonus(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var bonus = ctx.tblBonusOrder.Where(b => b.intIdCustomerReceiver == matricula && b.bitComissionConceded == false).ToList();
                    decimal amount = 0;

                    foreach (var ordem in bonus)
                        amount += ordem.intAmount.Value;

                    return amount;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public bool SetDispatchBonus(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var plansOptions = ctx.tblPlansOptions.ToList();
                    var bonus = ctx.tblBonusOrder.Where(b => b.intIdCustomerReceiver == matricula && b.bitComissionConceded == false).ToList();

                    foreach (var item in bonus)
                    {
                        item.bitComissionConceded = true;
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TotalizadoresComissao GetTotalizadoresComissao(int customerID)
        {
            try
            {
                var bonus = Convert.ToInt32(GetCustomerBonus(customerID));
                var comission = GetComissionAmmount(customerID).Ammount;

                return new TotalizadoresComissao
                {
                    ValorTotalLiberadoParaPagarBonus = bonus,
                    ValorTotalLiberadoParaPagarComissao = comission,
                    ValorTotalLiberadoParaPagarCliente = bonus + comission
                };
            }
            catch (Exception)
            {
                return new TotalizadoresComissao
                {
                    ValorTotalLiberadoParaPagarBonus = 0,
                    ValorTotalLiberadoParaPagarComissao = 0,
                    ValorTotalLiberadoParaPagarCliente = 0
                };
            }
        }

        public DetalhesComissao GetDetalhesComissao(int customerId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var detalhesComissao = new DetalhesComissao();

                    var detalhesBonus = ctx.GetCustomerBonusDetails(customerId).ToList();

                    var filhos = (from p in ctx.tblPersons
                                  join pp in ctx.tblPersonsParents on p.intIdPerson equals pp.intIdSon
                                  join pf in ctx.tblPersonsPhones on p.intIdPerson equals pf.intIdPerson
                                  where pp.intIdParent == customerId
                                  && pf.bitPhoneClube == false
                                  select new CustomerComissao
                                  {
                                      id = p.intIdPerson,
                                      telefone = pf.intDDD.ToString() + pf.intPhone.ToString(),
                                      nome = p.txtName,
                                      dataEntrada = p.dteRegister
                                  }).Distinct()
                                    .OrderByDescending(a => a.dataEntrada)
                                    .ToList();

                    foreach (var filho in filhos)
                    {
                        filho.nome = new ProfileAccess().GetName(filho.nome).Name;
                        filho.status = ctx.IsCustomerPayd(filho.id).FirstOrDefault() > 0 ? "Compra realizada" : "Cadastrado";

                        var detalhesFilho = detalhesBonus.Where(d => d.idFilho == filho.id).ToList();

                        int bonusFilho = 0;

                        foreach (var detalheFilho in detalhesFilho)
                        {
                            bonusFilho += detalheFilho.intAmount.Value;
                        }

                        var detalhe = detalhesBonus.Where(d => d.idFilho == filho.id).FirstOrDefault();

                        if (detalhe != null)
                            filho.statusConcedido = detalhe.bonusConcedido;

                        filho.bonus = bonusFilho.ToString();
                    }

                    detalhesComissao.customersComissao = filhos;
                    return detalhesComissao;
                }
            }
            catch (Exception)
            {
                return new DetalhesComissao();
            }
        }

        public BenefitResult ReleaseSplitedBenefit(int customerId, int amount)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (amount < 1)
                        return new BenefitResult
                        {
                            Success = false,
                            Message = "Não é possível liberar benefício a baixo de 1 Real."
                        };

                    if (customerId <= 0 || ctx.tblPersons.Any(a => a.intContactId == customerId))
                        return new BenefitResult
                        {
                            Success = false,
                            Message = "Cliente inválido"
                        };

                    var totalizadores = GetTotalizadoresComissao(customerId);

                    if (totalizadores.ValorTotalLiberadoParaPagarCliente < amount)
                        return new BenefitResult
                        {
                            Success = false,
                            Message = "Não é possível liberar benefício maior que o cliente possuí"
                        };

                    double valorBaixa = amount;
                    var ordens = new List<Order>();
                    var options = ctx.tblPlansOptions.ToList();

                    ordens.AddRange(ctx.tblComissionOrder
                        .Where(c => c.intIdCustomerReceiver == customerId && c.bitComissionConceded != true)
                        .Select(a => new Order
                        {
                            IdOrder = a.intIdComissionOrder,
                            Amount = a.intAmount.Value,
                            tipo = Order.TipoOrdem.Comissao
                        })
                        .ToList());

                    ordens.AddRange((from a in ctx.tblBonusOrder
                                     join b in ctx.tblPlansOptions on a.intIdPhonePlan equals b.intIdPlan
                                     where a.intIdCustomerReceiver == customerId && a.bitComissionConceded != true
                                     select new Order
                                     {
                                         IdOrder = a.intIdComissionOrder,
                                         Amount = (b.intCost * (a.intPercentBonus / 100.00)).Value,
                                         tipo = Order.TipoOrdem.Bonus
                                     }).ToList());

                    ordens = ordens.Select(a => new Order
                    {
                        Amount = a.Amount,
                        IdOrder = a.IdOrder,
                        tipo = a.tipo
                    }).OrderBy(a => a.Amount).ToList();

                    ReleaseOrders(ordens, valorBaixa);

                    UpdateOrders(ordens.Where(a => a.Baixa == true).ToList(), valorBaixa, ctx);

                    var ordemReduzida = ordens.Where(a => a.Reduzido == true).FirstOrDefault();

                    if (ordemReduzida != null)
                        UpdateFractionOrder(ordemReduzida, valorBaixa, ctx);

                    ctx.SaveChanges();

                    SaveLogComission(amount + "_" + customerId + "_" + JsonConvert.SerializeObject(ordens.Where(a => a.Baixa == true || a.Reduzido == true).ToList()), ctx);

                    var totalizadoresResultantes = GetTotalizadoresComissao(customerId);
                    return new BenefitResult
                    {
                        Success = true,
                        Message = string.Format("Montante liberado para o cliente, o cliente linha um saldo de {0}R$ e agora tem {1}R$.", (totalizadores.ValorTotalLiberadoParaPagarCliente / 100.00), (totalizadoresResultantes.ValorTotalLiberadoParaPagarCliente / 100.00)),
                        Value = totalizadoresResultantes.ValorTotalLiberadoParaPagarCliente
                    };
                }
            }
            catch (Exception e)
            {
                return new BenefitResult
                {
                    Success = false,
                    Message = "Ocorreu um erro inesperado"
                };
            }
        }

        public bool SaveLogComission(string comissionLog, FoneClubeContext ctx)
        {
            try
            {
                ctx.tblLog.Add(new tblLog
                {
                    dteTimeStamp = DateTime.Now,
                    intIdTipo = 5,
                    txtAction = comissionLog
                });

                ctx.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public object GetComissionLog()
        {
            using (var ctx = new FoneClubeContext())
            {
                var log = ctx.tblLog.LastOrDefault();
                var lista = log.txtAction.ToString().Split('_');

                return new
                {
                    Amount = lista[0],
                    ClientId = lista[1],
                    log = lista[2]
                };
            }
        }

        private void UpdateFractionOrder(Order ordem, double valorBaixa, FoneClubeContext ctx)
        {
            if (ordem.tipo == Order.TipoOrdem.Bonus)
            {
                var bonus = ctx.tblBonusOrder.FirstOrDefault(a => a.intIdComissionOrder == ordem.IdOrder);
                bonus.intAmount = Convert.ToInt32(Math.Floor(ordem.Amount));
            }
            else
            {
                var comissao = ctx.tblComissionOrder.FirstOrDefault(a => a.intIdComissionOrder == ordem.IdOrder);
                comissao.intAmount = Convert.ToInt32(Math.Floor(ordem.Amount));
                //risco minimizado mas testar // RISCO pois 101.1 não pode virar 1011 e sim 10110
            }
            ctx.SaveChanges();
        }

        private void UpdateOrders(List<Order> ordens, double valorBaixa, FoneClubeContext ctx)
        {
            var comissions = ordens.Where(a => a.tipo == Order.TipoOrdem.Comissao).Select(a => a.IdOrder).ToList();
            var bonus = ordens.Where(a => a.tipo == Order.TipoOrdem.Bonus).Select(a => a.IdOrder).ToList();

            var tblBonus = ctx.tblBonusOrder.Where(a => bonus.Contains(a.intIdComissionOrder)).ToList();
            var tblComissionOrder = ctx.tblComissionOrder.Where(a => comissions.Contains(a.intIdComissionOrder)).ToList();

            foreach (var b in tblComissionOrder)
                b.bitComissionConceded = true;

            foreach (var b in tblBonus)
                b.bitComissionConceded = true;

            ctx.SaveChanges();
        }

        private void ReleaseOrders(List<Order> ordens, double valorBaixa)
        {
            foreach (var ordem in ordens)
            {
                if (valorBaixa >= ordem.Amount)
                {
                    valorBaixa = valorBaixa - ordem.Amount;
                    ordem.Baixa = true;
                }
                else if (valorBaixa < ordem.Amount)
                {
                    ordem.Amount = ordem.Amount - valorBaixa;
                    valorBaixa = 0;
                    ordem.Reduzido = true;
                }

                if (valorBaixa == 0)
                    break;


            }
        }

        public List<CustomerViewModel> GetCustomerAmigosQuatroFilhos()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    // confirmar se contaremos linhas claro
                    var dezGB = 31;
                    var vinteGB = 32;
                    var cinquentaGB = 33;
                    var dezGBAmigos = 42;

                    var dezGBClaro = 22;
                    var vinteGBClaro = 23;
                    var quarentaGBClaro = 24;
                    var cemGBClaro = 25;

                    var persons = ctx.tblPersons
                                     .Where(p => p.bitDesativoManual != true && p.bitDelete != true)
                                     .ToList();

                    var phones = ctx.tblPersonsPhones
                                    .Where(p => p.bitPhoneClube == true
                                                && (p.intIdPlan == dezGB ||
                                                    p.intIdPlan == vinteGB ||
                                                    p.intIdPlan == cinquentaGB ||
                                                    p.intIdPlan == dezGBAmigos))
                                    .ToList();

                    var tblFoneclubePagarmeTransactions = ctx.tblFoneclubePagarmeTransactions.ToList();


                    var clientes = ctx.GetClientesComPagamento()
                                        .Select(a => new CustomerViewModel { Id = a.intIdPerson, DocumentNumber = a.txtDocumentNumber, Name = a.txtName })
                                        .ToList();

                    var paternidade = ctx.tblPersonsParents.ToList();

                    foreach (var cliente in clientes)
                    {
                        cliente.Filhos = paternidade.Where(p => p.intIdParent == cliente.Id)
                                   .Select(a => new Filho
                                   {
                                       Id = a.intIdSon.Value
                                   })
                                   .ToList();

                        foreach (var filho in cliente.Filhos)
                        {
                            var personFilho = persons.FirstOrDefault(p => p.intIdPerson == filho.Id);
                            if (personFilho != null)
                            {
                                filho.Name = personFilho.txtName;
                                filho.DocumentNumber = personFilho.txtDocumentNumber;
                                filho.IdPagarme = Convert.ToInt32(personFilho.intIdPagarme);

                                filho.Phones = phones.Where(p => p.intIdPerson == filho.Id)
                                      .Select(p => new PhoneFilho { Id = p.intId, IdPlan = p.intIdPlan.Value })
                                      .ToList();

                                if (filho.IdPagarme > 0)
                                {
                                    var ultimoPagamento = new TransactionAccess().GetCustomerLastTransactionPaid(Convert.ToInt32(filho.IdPagarme), tblFoneclubePagarmeTransactions).FirstOrDefault();

                                    if (ultimoPagamento != null)
                                    {
                                        filho.UltimoPagamento = Convert.ToDateTime(ultimoPagamento.dteDate_created).ToString();
                                        filho.DiasDoPagamento = (DateTime.Now - Convert.ToDateTime(ultimoPagamento.dteDate_created)).Days;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var cliente in clientes)
                    {
                        cliente.Filhos = cliente.Filhos.Where(f => f.DiasDoPagamento < 35 && f.Name != null && f.Phones.Count > 0 && f.UltimoPagamento != "0001-01-01T00:00:00").ToList();
                    }


                    var clientesQuatroFilhos = clientes.Where(c => c.Filhos.Count > 3).ToList();
                    return clientesQuatroFilhos;
                }
            }
            catch (Exception e)
            {
                return new List<CustomerViewModel>();
            }
        }

        public List<CustomerViewModel> GetCustomerAmigosDoisFilhos()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    // confirmar se contaremos linhas claro
                    var dezGB = 31;
                    var vinteGB = 32;
                    var cinquentaGB = 33;
                    var dezGBAmigos = 42;

                    var dezGBClaro = 22;
                    var vinteGBClaro = 23;
                    var quarentaGBClaro = 24;
                    var cemGBClaro = 25;

                    var persons = ctx.tblPersons
                                     .Where(p => p.bitDesativoManual != true && p.bitDelete != true)
                                     .ToList();

                    var phones = ctx.tblPersonsPhones
                                    .Where(p => p.bitPhoneClube == true
                                                && (p.intIdPlan == dezGB ||
                                                    p.intIdPlan == vinteGB ||
                                                    p.intIdPlan == cinquentaGB ||
                                                    p.intIdPlan == dezGBAmigos))
                                    .ToList();

                    var tblFoneclubePagarmeTransactions = ctx.tblFoneclubePagarmeTransactions.ToList();


                    var clientes = ctx.GetClientesComPagamento()
                                        .Select(a => new CustomerViewModel { Id = a.intIdPerson, DocumentNumber = a.txtDocumentNumber, Name = a.txtName })
                                        .ToList();

                    var paternidade = ctx.tblPersonsParents.ToList();

                    foreach (var cliente in clientes)
                    {
                        cliente.Filhos = paternidade.Where(p => p.intIdParent == cliente.Id)
                                   .Select(a => new Filho
                                   {
                                       Id = a.intIdSon.Value
                                   })
                                   .ToList();

                        foreach (var filho in cliente.Filhos)
                        {
                            var personFilho = persons.FirstOrDefault(p => p.intIdPerson == filho.Id);
                            if (personFilho != null)
                            {
                                filho.Name = personFilho.txtName;
                                filho.DocumentNumber = personFilho.txtDocumentNumber;
                                filho.IdPagarme = Convert.ToInt32(personFilho.intIdPagarme);

                                filho.Phones = phones.Where(p => p.intIdPerson == filho.Id)
                                      .Select(p => new PhoneFilho { Id = p.intId, IdPlan = p.intIdPlan.Value })
                                      .ToList();

                                if (filho.IdPagarme > 0)
                                {
                                    var ultimoPagamento = new TransactionAccess().GetCustomerLastTransactionPaid(Convert.ToInt32(filho.IdPagarme), tblFoneclubePagarmeTransactions).FirstOrDefault();

                                    if (ultimoPagamento != null)
                                    {
                                        filho.UltimoPagamento = Convert.ToDateTime(ultimoPagamento.dteDate_created).ToString();
                                        filho.DiasDoPagamento = (DateTime.Now - Convert.ToDateTime(ultimoPagamento.dteDate_created)).Days;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var cliente in clientes)
                    {
                        cliente.Filhos = cliente.Filhos.Where(f => f.DiasDoPagamento < 35 && f.Name != null && f.Phones.Count > 0 && f.UltimoPagamento != "0001-01-01T00:00:00").ToList();
                    }


                    var clientesQuatroFilhos = clientes.Where(c => c.Filhos.Count > 1 && c.Filhos.Count < 4).ToList();
                    return clientesQuatroFilhos;
                }
            }
            catch (Exception e)
            {
                return new List<CustomerViewModel>();
            }
        }

        //4289, 4075
        private bool IsChildrenMedgrupoOrRM(int idChild, List<tblPersonsParents> data)
        {
            bool isExists = false;
            var tblChild = data.Where(x => x.intIdSon == idChild).FirstOrDefault();
            if (tblChild != null && tblChild.intIdParent > 0)
            {
                if (tblChild.intIdParent == 4289 || tblChild.intIdParent == 6268)
                {
                    isExists = true;
                }
                else if (tblChild.intIdParent == 4555)
                {
                    isExists = false;
                }
                else
                {
                    isExists = IsChildrenMedgrupoOrRM(tblChild.intIdParent.Value, data);
                }
            }
            return isExists;
        }

        public Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel GetPromoCodeOwner(string customer, string code)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    string promoDecoded = string.Empty;

                    try
                    {
                        promoDecoded = code;
                    }
                    catch (Exception)
                    {

                    }

                    if (!string.IsNullOrEmpty(promoDecoded))
                    {
                        var matriculaFilho = Convert.ToInt32(customer);

                        var paternidade = ctx.tblPersonsParents.FirstOrDefault(a => a.intIdSon == matriculaFilho);

                        if (paternidade == null)
                        {
                            ctx.tblPersonsParents.Add(new tblPersonsParents { dteCadastro = DateTime.Now, intIdParent = 4555, intIdSon = matriculaFilho });
                            ctx.SaveChanges();

                            return new Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel { id = -1 };
                        }
                        else
                        {
                            var result = IsChildrenMedgrupoOrRM(matriculaFilho, ctx.tblPersonsParents.ToList());

                            if (result && promoDecoded.ToUpper() == "MED50")
                            {
                                return new Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel { id = matriculaFilho };
                            }
                            else
                                return new Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel { id = -1 };
                        }
                    }
                    else
                    {
                        return new Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel { id = -1 };
                    }
                }
            }
            catch (Exception e)
            {
                return new Business.Commons.Entities.ViewModel.CustomerMinhaContaViewModel { id = -1 };
            }
        }

    }
}
