using Business.Commons.Utils;
using Data.Service;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.email;
using FoneClube.Business.Commons.Entities.FoneClube.estoque;
using FoneClube.Business.Commons.Entities.FoneClube.flag;
using FoneClube.Business.Commons.Entities.FoneClube.linhas;
using FoneClube.Business.Commons.Entities.FoneClube.phone;
using FoneClube.Business.Commons.Entities.ViewModel.Plano;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FoneClube.DataAccess
{
    public class PhoneAccess
    {
        public List<Evento> GetEventos()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblHistoryEvents.Where(e => e.txtEventDescription.Length > 0)
                        .Select(o => new Evento
                        {
                            Id = o.intIdEvent,
                            Descricao = o.txtEventDescription
                        }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Evento>();
            }

        }

        public Phone.PhoneStatus GetStatusLinha(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var phoneProperty = ctx.tblPhonePropertyHistory.FirstOrDefault(p => p.intPhoneDDD == phoneDDD && p.intPhoneNumber == phoneNumber && p.intIdPerson == personId);

                    if (phoneProperty != null)
                        if (phoneProperty.intIdStatus != null)
                            return (Phone.PhoneStatus)phoneProperty.intIdStatus;


                    return Phone.PhoneStatus.SemStatus;
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public List<PropertyHistoryViewModel> GetPersonPropertyHistory(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPhonePropertyHistory.Where(p => p.intIdPerson == person.Id)
                                        .Select(o => new PropertyHistoryViewModel
                                        {
                                            IdHistory = o.intIdHistory,
                                            IdOperator = o.intIdOperator,
                                            IdPerson = o.intIdPerson,
                                            IdPlan = o.intIdPlan,
                                            IdStatus = o.intIdStatus,
                                            PlanPrice = o.intPlanPrice * 100, //cents
                                            IdPhone = o.intIdPhone,
                                            PhoneDDD = o.intPhoneDDD,
                                            PhoneNumber = o.intPhoneNumber,
                                            Change = o.dteChange,
                                            Entrada = o.dteEntrada,
                                            Saida = o.dteSaida,
                                            EventType = o.intEventType
                                        }).ToList();
                }
            }
            catch (Exception)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não foi possível coletar histórico, ocorreu um problema na API")));
            }
        }

        public bool GetStatusLinhaAptaFoneclube(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var tblPersonPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intDDD == phoneDDD && p.intPhone == phoneNumber && p.intIdPerson == personId && p.bitPhoneClube == true);
                    if (tblPersonPhone == null)
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public bool InsertHistoricoDesativarLinha(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var tblPlanOption = ctx.tblPlansOptions.FirstOrDefault(p => p.intIdPlan == phone.IdPlanOption);

                    var historyProperty = new tblPhonePropertyHistory
                    {
                        intIdPerson = personId,
                        dteChange = DateTime.Now,
                        intIdOperator = tblPlanOption.intIdOperator,
                        intIdPlan = tblPlanOption.intIdPlan,
                        intPhoneDDD = phoneDDD,
                        intPhoneNumber = phoneNumber,
                        intEventType = Convert.ToInt32(Phone.PhoneEvents.DesativarLinha),
                        intIdStatus = Convert.ToInt32(Phone.PhoneStatus.Desativa),
                        intPlanPrice = Convert.ToDouble(tblPlanOption.intCost) / 100,
                        dteSaida = DateTime.Now,
                        intIdPhone = phone.Id
                    };

                    ctx.tblPhonePropertyHistory.Add(historyProperty);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InsertHistoricoAtivarLinha(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var tblPlanOption = ctx.tblPlansOptions.FirstOrDefault(p => p.intIdPlan == phone.IdPlanOption);

                    var historyProperty = new tblPhonePropertyHistory
                    {
                        intIdPerson = personId,
                        dteChange = DateTime.Now,
                        intIdOperator = tblPlanOption.intIdOperator,
                        intIdPlan = tblPlanOption.intIdPlan,
                        intPhoneDDD = phoneDDD,
                        intPhoneNumber = phoneNumber,
                        intEventType = Convert.ToInt32(Phone.PhoneEvents.AtivarLinha),
                        intIdStatus = Convert.ToInt32(Phone.PhoneStatus.Ativa),
                        intPlanPrice = Convert.ToDouble(tblPlanOption.intCost) / 100,
                        dteEntrada = DateTime.Now,
                        intIdPhone = phone.Id
                    };

                    ctx.tblPhonePropertyHistory.Add(historyProperty);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InsertHistoricoDesligarLinha(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var tblPlanOption = ctx.tblPlansOptions.FirstOrDefault(p => p.intIdPlan == phone.IdPlanOption);


                    var historyProperty = new tblPhonePropertyHistory
                    {
                        intIdPerson = personId,
                        dteChange = DateTime.Now,
                        intIdOperator = tblPlanOption.intIdOperator,
                        intIdPlan = tblPlanOption.intIdPlan,
                        intPhoneDDD = phoneDDD,
                        intPhoneNumber = phoneNumber,
                        intEventType = Convert.ToInt32(Phone.PhoneEvents.DesligarLinha),
                        intIdStatus = Convert.ToInt32(Phone.PhoneStatus.Desligada),
                        intPlanPrice = Convert.ToDouble(tblPlanOption.intCost) / 100,
                        dteSaida = DateTime.Now,
                        intIdPhone = phone.Id
                    };

                    ctx.tblPhonePropertyHistory.Add(historyProperty);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InsertHistoricoUpdateLinha(int personId, Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneDDD = Convert.ToInt32(phone.DDD);
                    var phoneNumber = Convert.ToInt32(phone.Number);
                    var tblPlanOption = ctx.tblPlansOptions.FirstOrDefault(p => p.intIdPlan == phone.IdPlanOption);


                    var historyProperty = new tblPhonePropertyHistory
                    {
                        intIdPerson = personId,
                        dteChange = DateTime.Now,
                        intIdOperator = tblPlanOption.intIdOperator,
                        intIdPlan = tblPlanOption.intIdPlan,
                        intPhoneDDD = phoneDDD,
                        intPhoneNumber = phoneNumber,
                        intEventType = Convert.ToInt32(Phone.PhoneEvents.AtualizarPlano),
                        intPlanPrice = Convert.ToDouble(tblPlanOption.intCost) / 100,
                        dteSaida = DateTime.Now,
                        intIdPhone = phone.Id
                    };

                    ctx.tblPhonePropertyHistory.Add(historyProperty);
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Estoque> GetLinhasFoneclubeEstoque()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var titulos = ctx.tblOperadoras.ToList();

                    var estoque = ctx.Database.SqlQuery<ResultEstoqueTIM>("GetEstoqueTIM")
                        .ToList().Where(a => a.ocupada == 0).Select(a => new Estoque
                        {
                            operadora = 3,
                            linhaLivreOperadora = a.phones,
                            descricao = "TIM"
                        }).ToList();

                    estoque.AddRange(ctx.GetLinhasFoneclubeEstoque().Select(p => new Estoque
                    {
                        linhaLivreOperadora = p.linhaLivreOperadora,
                        operadora = p.operadora,
                        descricao = titulos.FirstOrDefault(a => a.intIdOperator == p.operadora).txtName
                    }).ToList());

                    var listaLinhas = estoque.OrderBy(a => a.operadora).Where(a => a.descricao != "VIVO").ToList();
                    var tblStock = ctx.tblPhonesStockProperty.ToList();

                    foreach (var linha in listaLinhas)
                    {
                        if (!tblStock.Any(x => x.txtPhone == linha.linhaLivreOperadora))
                        {
                            ctx.tblPhonesStockProperty.Add(new tblPhonesStockProperty
                            {
                                txtPhone = linha.linhaLivreOperadora,
                                dteRegister = DateTime.Now,
                                intIdOperadora = linha.operadora,
                                txtDescricaoOperadora = linha.descricao
                            });
                        }
                    }

                    ctx.SaveChanges();

                    var listaFinal = ctx.tblPhonesStockProperty.ToList().Select(a => new Estoque
                    {
                        descricao = a.txtDescricaoOperadora,
                        linhaLivreOperadora = a.txtPhone,
                        operadora = a.intIdOperadora,
                        IdLinha = a.intId,
                        propriedadeInterna = a.txtProperty
                        //propriedadeInterna = "FC"
                    }).ToList();


                    return listaFinal;
                }
            }
            catch (Exception)
            {
                return new List<Estoque>();
            }
        }

        public bool SetPropriedadeIterna(Estoque phoneEstoque)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phone = ctx.tblPhonesStockProperty.FirstOrDefault(a => a.intId == phoneEstoque.IdLinha);

                    if (phone != null)
                    {
                        phone.txtProperty = phoneEstoque.propriedadeInterna;
                        ctx.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public class ResultEstoqueTIM
        {
            public string phones { get; set; }
            public int ocupada { get; set; }
        }

        public List<LinhaDetalhesMinimos> GetLinhasFoneclubeMinimal()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var stockDetails = ctx.tblPhoneStock.ToList();

                    var listaTIM =
                    (from pp in ctx.tblPersonsPhones
                     join p in ctx.tblPersons on pp.intIdPerson equals p.intIdPerson
                     join pl in ctx.tblPlansOptions on pp.intIdPlan equals pl.intIdPlan
                     where pp.bitPhoneClube == true && pp.bitAtivo == true && pp.intIdOperator == 3
                     select new LinhaDetalhesMinimos
                     {
                         txtName = !string.IsNullOrEmpty(p.txtName) ? p.txtName : "~",
                         txtNickname = !string.IsNullOrEmpty(p.txtNickName) ? p.txtNickName : "~",
                         //Email = !string.IsNullOrEmpty(p.txtEmail) ? p.txtEmail : "~",
                         //Register = p.dteRegister != null ? p.dteRegister.ToString() : string.Empty,
                         //Born = Convert.ToDateTime(p.dteBorn).ToLongDateString(), 
                         //Phone = pp.intPhone.ToString(),
                         numeroTelefoneCompleto = pp.intDDD.ToString() + pp.intPhone.ToString(),
                         //DisplayPhone = string.Format("({0}) {1}-{2}", pp.intDDD, pp.intPhone.ToString().Substring(0,5), pp.intPhone.ToString().Substring(5, 9)),
                         //OperatorName = pl.txtDescription,
                         //PlanId = pp.intIdPlan.Value,
                         //PlanDescription = pl.txtDescription,
                         //PlanCost = pl.intCost,
                         idPhone = pp.intId,
                         //AmoutPrecoVip = pp.intAmmoutPrecoVip == null ? 0 : pp.intAmmoutPrecoVip.Value,
                         intIdPerson = p.intIdPerson,
                         operadora = pp.intIdOperator.Value,
                         bitLinhaAtiva = pp.bitAtivo,
                         bitPrecoVip = pp.bitPrecoVip,
                         intPrecoVip = pp.intAmmoutPrecoVip

                     }).ToList();


                    var lista = ctx.GetLinhasFoneclubeMinimal().Select(a => new LinhaDetalhesMinimos
                    {
                        idPhone = a.idPhone,
                        intIdPerson = a.intIdPerson,
                        bitLinhaAtiva = a.bitLinhaAtiva,
                        bitPrecoVip = a.bitPrecoVip,
                        intPrecoFoneclube = 0,
                        intPrecoVip = a.intPrecoVip,
                        linhaLivreOperadora = a.linhaLivreOperadora,
                        operadora = a.operadora,
                        txtName = a.txtName,
                        txtNickname = a.txtNickname,
                        txtPlanoFoneclube = a.txtPlanoFoneclube,
                        numeroTelefoneCompleto = a.linhaLivreOperadora,
                        intIdPlan = a.intIdPlan
                    }).ToList();

                    var servicos = (from a in lista
                                    join p in ctx.tblPhonesServices on a.idPhone equals p.intIdPhone
                                    join s in ctx.tblServices on p.intIdService equals s.intIdService
                                    where p.bitAtivo == true
                                    select new Service
                                    {
                                        IdPhone = p.intIdPhone,
                                        IdService = p.intIdService,
                                        Descricao = s.ServiceDesc
                                    }).ToList();

                    var flags = ctx.tblPhoneFlags.ToList();

                    foreach (var linha in lista)
                    {
                        linha.Servicos = servicos.Where(s => s.IdPhone == linha.idPhone).ToList();

                        var flag = flags.FirstOrDefault(a => a.phone == linha.numeroTelefoneCompleto);

                        if (flag != null)
                        {
                            linha.divergencia = !(linha.intIdPlan == flag.intIdPlan);
                        }

                        linha.divergencia = false;
                    }

                    foreach (var phone in lista)
                    {
                        var detailsTim = stockDetails.FirstOrDefault(p => p.txtNumero == phone.numeroTelefoneCompleto);
                        if (detailsTim != null)
                        {
                            phone.CodigoCliente = detailsTim.txtCodCliente;
                            phone.RazaoSocial = detailsTim.txtRazaoSocial;
                            phone.CCID = detailsTim.txtICCID;
                        }
                    }

                    return lista;
                }
            }
            catch (Exception e)
            {
                return new List<LinhaDetalhesMinimos>();
            }
        }

        public List<tblPhoneFlags> GetStatusLinhasOperadora()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    Task.Factory.StartNew(() => UpdatePhoneFlags());
                    return ctx.tblPhoneFlags.ToList();
                }
            }
            catch (Exception)
            {
                return new List<tblPhoneFlags>();
            }
        }

        public async void UpdatePhoneFlags()
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.UpdatePhoneFlags();
            }
        }

        public bool UpdatePhoneDesassociation(int phoneId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == phoneId);
                    phone.bitAtivo = false;
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InsertPhoneAssociation(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return new ProfileAccess().SaveContactInfo(person, ctx);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<NovosClientes> GetRegisteredPersonsLog()
        {
            using (var ctx = new FoneClubeContext())
            {
                var novosClientes = new List<NovosClientes>();
                var dataLimite = DateTime.Now.AddDays(-60);
                var allPersons = ctx.tblPersons.ToList();
                var persons = allPersons.Where(c => c.dteRegister > dataLimite).Select(a =>
                    new Person
                    {
                        Id = a.intIdPerson,
                        Name = a.txtName,
                        DocumentNumber = a.txtDocumentNumber,
                        Register = a.dteRegister,
                        Email = a.txtEmail
                    })
                    .OrderByDescending(d => d.Register).ToList();

                var personsIds = persons.Select(p => p.Id).ToList();
                var pais = ctx.tblPersonsParents.Where(p => personsIds.Contains(p.intIdSon.Value));
                var telefones = ctx.tblPersonsPhones.Where(p => personsIds.Contains(p.intIdPerson.Value) && p.bitPhoneClube == false);
                var chargings = ctx.tblChargingHistory.Where(a => personsIds.Contains(a.intIdCustomer.Value)).ToList();
                var chargingsIs = chargings.Select(a => a.intIdTransaction).ToList();
                var statusPagarme = ctx.tblFoneclubePagarmeTransactions.Where(a => chargingsIs.Contains(a.intIdTransaction.Value)).ToList();

                foreach (var cliente in persons)
                {
                    var pai = pais.FirstOrDefault(p => p.intIdSon == cliente.Id);
                    var fullPai = allPersons.FirstOrDefault(a => a.intIdPerson == pai.intIdParent);

                    if (pai != null)
                        cliente.Pai = new Pai { Id = pai.intIdParent, Name = fullPai.txtName, Documento = fullPai.txtDocumentNumber };

                    bool pago;
                    var compraInicial = chargings.FirstOrDefault(a => a.intIdCustomer == cliente.Id && a.txtCollectorName == "Checkout Pagarme");

                    if (compraInicial != null)
                    {
                        var status = statusPagarme.FirstOrDefault(a => a.intIdTransaction == compraInicial.intIdTransaction);
                        if (status == null)
                            pago = false;
                        else
                            pago = status.txtOutdadetStatus == "Paid";

                        cliente.Charging = new Charging { Id = compraInicial.intId, Token = compraInicial.txtTokenTransaction, Ammount = compraInicial.txtAmmountPayment, Frete = compraInicial.intIdFrete, Pago = pago, Comment = compraInicial.txtComment };
                        cliente.Charging.Planos = GetCheckoutPlans(compraInicial.txtTokenTransaction);
                    }
                    else
                        cliente.Charging = null;
                }

                foreach (var customer in persons)
                {
                    if (customer.Charging == null)
                    {
                        customer.Charging = new Charging();
                    }

                    var telefone = telefones.FirstOrDefault(a => a.intIdPerson == customer.Id);
                    novosClientes.Add(new NovosClientes
                    {
                        Id = customer.Id,
                        Name = customer.Name == null ? string.Empty : customer.Name,
                        DocumentNumber = customer.DocumentNumber == null ? string.Empty : customer.DocumentNumber,
                        RegisterDate = customer.Register,
                        Email = customer.Email == null ? string.Empty : customer.Email,
                        Pago = customer.Charging.Pago,
                        Planos = customer.Charging.Token == null ? string.Empty : customer.Charging.Token,
                        IdParent = customer.Pai.Id,
                        DocumentParent = customer.Pai.Documento == null ? string.Empty : customer.Pai.Documento,
                        NameParent = customer.Pai.Name == null ? string.Empty : customer.Pai.Name,
                        Custo = customer.Charging.Ammount,
                        Telefone = telefone != null ? telefone.intDDD.ToString() + "" + telefone.intPhone.ToString() : null,
                        Frete = customer.Charging.Frete
                    });
                }

                return novosClientes;
            }
        }

        public class NovosClientes
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string DocumentNumber { get; set; }
            public DateTime? RegisterDate { get; set; }
            public string Email { get; set; }
            public int? IdParent { get; set; }
            public string NameParent { get; set; }
            public string DocumentParent { get; set; }
            public int? Frete { get; set; }
            public bool? Pago { get; set; }
            public string Planos { get; set; }
            public string Custo { get; internal set; }
            public string Telefone { get; internal set; }
        }

        public List<Plan> GetCheckoutPlans(string planos)
        {
            using (var ctx = new FoneClubeContext())
            {
                var checkoutPlans = new List<Plan>();

                try
                {
                    var plansParams = planos.Split(',');
                    var plansOptions = ctx.tblPlansOptions.ToList();

                    foreach (var option in plansParams)
                    {
                        var planOption = Convert.ToInt32(option);
                        var current = plansOptions.FirstOrDefault(p => p.intIdPlan == planOption);
                        if (current != null)
                        {
                            checkoutPlans.Add(new Plan { Id = planOption, Description = current.txtDescription });
                        }
                    }

                    return checkoutPlans;
                }
                catch (Exception)
                {
                    return checkoutPlans;
                }
            }
        }

        public List<Plan> GetPlansOptions()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPlansOptions
                        .Where(p => p.intBitActive == true)
                        .Select(p => new Business.Commons.Entities.FoneClube.Plan
                        {
                            Id = p.intIdPlan,
                            Description = p.txtDescription,
                            IdOperator = p.intIdOperator,
                            Value = p.intCost
                        }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Plan>();
            }
        }

        public bool UpdatePhonePlan(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var tblPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == phone.Id);
                    tblPhone.intIdPlan = phone.IdPlanOption;
                    ctx.SaveChanges();

                    var detailPhone = new Phone
                    {
                        Id = tblPhone.intId,
                        DDD = tblPhone.intDDD.ToString(),
                        Number = tblPhone.intPhone.ToString(),
                        IdPlanOption = tblPhone.intIdPlan
                    };

                    InsertHistoricoUpdateLinha(Convert.ToInt32(tblPhone.intIdPerson), detailPhone);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<GetDivergencias_Result> GetStatusDivergencia()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.GetDivergencias().ToList();
                }
            }
            catch (Exception e)
            {
                return new List<GetDivergencias_Result>();
            }
        }

        public bool InsertPhoneService(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (phone == null)
                        return false;

                    if (phone.Id > 0 || phone.Servicos.FirstOrDefault() != null)
                    {
                        foreach (var servico in phone.Servicos)
                        {
                            var tblServicePhone = ctx.tblPhonesServices.Where(p => p.intIdPhone == phone.Id && p.intIdService == servico.Id && p.bitAtivo == true)
                                .FirstOrDefault();

                            if (tblServicePhone == null)
                            {
                                ctx.tblPhonesServices.Add(new tblPhonesServices
                                {
                                    intIdPhone = phone.Id,
                                    intIdService = servico.Id,
                                    bitAtivo = true,
                                    dteAtivacao = DateTime.Now,
                                    dteUpdate = DateTime.Now
                                });
                            }
                            else
                            {
                                tblServicePhone.dteUpdate = DateTime.Now;
                            }

                        }

                        ctx.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InsertDeactivePhoneService(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (phone == null)
                        return false;

                    if (phone.Id > 0 || phone.Servicos.FirstOrDefault() != null)
                    {
                        foreach (var servico in phone.Servicos)
                        {
                            var tblServicePhone = ctx.tblPhonesServices.Where(p => p.intIdPhone == phone.Id && p.intIdService == servico.Id && p.bitAtivo == true)
                                .FirstOrDefault();

                            tblServicePhone.bitAtivo = false;
                            tblServicePhone.dteDesativacao = DateTime.Now;
                            tblServicePhone.dteUpdate = DateTime.Now;
                        }

                        ctx.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<PhoneService> GetServices()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblServices.Where(a => a.bitEditavel == true).Select(s => new PhoneService
                    {
                        Id = s.intIdService,
                        Descricao = s.ServiceDesc,
                        ExtraOption = s.IsExtraOption,
                        AmountFoneclube = s.intValorFoneclube.Value,
                        AmountOperadora = s.intValorOperadora.Value,
                        Assinatura = s.assinaturas == 1 ? true : false
                    }).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<PhoneService>();
            }
        }

        public Phone GetPhoneServices(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var services = ctx.tblServices
                                      .Where(a => a.bitEditavel == true)
                                      .Select(s => new PhoneService
                                      {
                                          Id = s.intIdService,
                                          Descricao = s.ServiceDesc,
                                          ExtraOption = s.IsExtraOption,
                                          AmountFoneclube = s.intValorFoneclube.Value,
                                          AmountOperadora = s.intValorOperadora.Value,
                                          Assinatura = s.assinaturas == 1 ? true : false
                                      }).ToList();

                    phone.Servicos = new List<PhoneService>();
                    var servicosAtivos = ctx.tblPhonesServices.Where(p => p.intIdPhone == phone.Id && p.bitAtivo == true).ToList();
                    foreach (var servico in servicosAtivos)
                    {
                        phone.Servicos.Add(new PhoneService
                        {
                            Id = servico.intIdService.Value,
                            Descricao = services.FirstOrDefault(q => q.Id == servico.intIdService.Value) != null ? services.FirstOrDefault(q => q.Id == servico.intIdService.Value).Descricao : string.Empty,
                            AmountFoneclube = services.FirstOrDefault(q => q.Id == servico.intIdService.Value) != null ? services.FirstOrDefault(q => q.Id == servico.intIdService.Value).AmountFoneclube : 0,
                            AmountOperadora = services.FirstOrDefault(q => q.Id == servico.intIdService.Value) != null ? services.FirstOrDefault(q => q.Id == servico.intIdService.Value).AmountOperadora : 0
                        });
                    }

                    return phone;
                }
            }
            catch (Exception e)
            {
                return new Phone();
            }
        }

        public List<Plan> GetAllPlansOptions()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPlansOptions
                        .Select(p => new Business.Commons.Entities.FoneClube.Plan
                        {
                            Id = p.intIdPlan,
                            Description = p.txtDescription,
                            IdOperator = p.intIdOperator,
                            Value = p.intCost,
                            Active = p.intBitActive
                        }).ToList();
                }
            }
            catch (Exception)
            {
                return new List<Plan>();
            }
        }

        public List<PhoneService> GetAllPhoneServices()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblServices.Select(a => new PhoneService
                    {
                        Id = a.intIdService,
                        Descricao = a.ServiceDesc,
                        AmountFoneclube = a.intValorFoneclube.Value,
                        AmountOperadora = a.intValorOperadora.Value,
                        Assinatura = a.assinaturas == 1 ? true : false,
                        ExtraOption = a.IsExtraOption,
                        Editavel = a.bitEditavel
                    }).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<PhoneService>();
            }
        }

        public bool InsertNewPlan(Plan plan)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var newPlanId = ctx.tblPlansOptions.OrderByDescending(a => a.intIdPlan).First().intIdPlan + 1;
                    ctx.tblPlansOptions.Add(new tblPlansOptions
                    {
                        intIdPlan = newPlanId,
                        intIdOperator = plan.IdOperator,
                        txtDescription = plan.Description,
                        intBitActive = plan.Active,
                        intCost = plan.Cost
                    });

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool InsertNewService(PhoneService service)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var newServiceId = ctx.tblServices.OrderByDescending(a => a.intIdService).First().intIdService + 1;
                    ctx.tblServices.Add(new tblServices
                    {
                        intIdService = newServiceId,
                        ServiceDesc = service.Descricao,
                        assinaturas = Convert.ToBoolean(service.Assinatura) ? 1 : 0,
                        IsExtraOption = service.ExtraOption,
                        intValorFoneclube = service.AmountFoneclube,
                        intValorOperadora = service.AmountOperadora,
                        bitEditavel = service.Editavel
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


        public bool UpdatePlan(Plan plan)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var tblPlan = ctx.tblPlansOptions.FirstOrDefault(a => a.intIdPlan == plan.Id);
                    tblPlan.intIdOperator = plan.IdOperator;
                    tblPlan.txtDescription = plan.Description;
                    tblPlan.intBitActive = plan.Active;
                    tblPlan.intCost = plan.Value;

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool UpdateService(PhoneService service)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var tblService = ctx.tblServices.FirstOrDefault(a => a.intIdService == service.Id);
                    tblService.ServiceDesc = service.Descricao;
                    tblService.assinaturas = Convert.ToBoolean(service.Assinatura) ? 1 : 0;
                    tblService.IsExtraOption = service.ExtraOption;
                    tblService.intValorFoneclube = service.AmountFoneclube;
                    tblService.intValorOperadora = service.AmountOperadora;
                    tblService.bitEditavel = service.Editavel;

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InsertPhoneFlag(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    ctx.tblGenericPhoneFlags.Add(new tblGenericPhoneFlags
                    {
                        dteRegister = DateTime.Now,
                        bitPendingInteraction = phone.Flags.FirstOrDefault().PendingInteraction,
                        intIdFlag = phone.Flags.FirstOrDefault().IdType,
                        intIdPhone = phone.Id,
                        txtDescription = phone.Flags.FirstOrDefault().InteractionDescription
                    });

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool AllPhoneLinesEdit(EditParam param)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var phoneLine = ctx.tblPersonsPhones.Where(x => x.intId == param.Id).FirstOrDefault();
                    if (phoneLine != null)
                    {
                        switch (param.Key)
                        {
                            case "PrecoVIP":
                                var value1 = param.Value.Contains("R$") ? Convert.ToInt32(param.Value.Replace("R$", "").Replace(".", "")) : Convert.ToInt32(param.Value.Replace(".", ""));
                                phoneLine.intAmmoutPrecoVip = value1;
                                break;
                            case "Apelido":
                                phoneLine.txtNickname = param.Value;
                                break;
                            case "Plano_FC":
                                var value = param.Value.Remove(param.Value.LastIndexOf("-")).Trim();
                                var val = ctx.tblPlansOptions.Where(x => x.txtDescription == value).FirstOrDefault();
                                phoneLine.intIdPlan = val.intIdPlan;
                                break;
                            case "Ativa":
                                phoneLine.bitAtivo = Convert.ToBoolean(param.Value);
                                break;
                            case "RecAutFCFlag":
                                {
                                    var contel = ctx.tblContelLinhasList.FirstOrDefault(x => x.txtlinha == phoneLine.intDDD + "" + phoneLine.intPhone);
                                    contel.bitRecAutoFC = Convert.ToBoolean(param.Value);
                                    ctx.SaveChanges();
                                }
                                break;
                            case "ClienteAtivo_FC":
                                {
                                    var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == phoneLine.intIdPerson).FirstOrDefault();
                                    tblPerson.bitDesativoManual = Convert.ToBoolean(param.Value);
                                    tblPerson.bitDelete = Convert.ToBoolean(param.Value);
                                    ctx.SaveChanges();
                                }
                                break;
                            case "Nome_FC":
                                {
                                    var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == phoneLine.intIdPerson).FirstOrDefault();
                                    tblPerson.txtName = param.Value;
                                    ctx.SaveChanges();
                                }
                                break;

                        }
                        ctx.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public FlagResponse InsertGenericFlag(GenericFlag flag)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var response = new FlagResponse();

                    var tblGenericPhoneFlags = new tblGenericPhoneFlags
                    {
                        dteRegister = DateTime.Now,
                        bitPendingInteraction = flag.PendingInteraction,
                        intIdFlag = flag.IdFlagType,
                        intIdPhone = flag.IdPhone,
                        txtDescription = flag.Description,
                        intIdPerson = flag.IdPerson
                    };

                    ctx.tblGenericPhoneFlags.Add(tblGenericPhoneFlags);
                    ctx.SaveChanges();
                    response.FlagSuccess = true;

                    var flagType = ctx.tblGenericFlags.FirstOrDefault(c => c.intId == flag.IdFlagType);

                    if (flagType != null)
                    {
                        try
                        {
                            if (flagType.intIdEmail.Value > 0)
                                response.EmailSuccess = SendFlagEmail(ctx, flag, tblGenericPhoneFlags, flagType);
                        }
                        catch (Exception)
                        {
                            response.EmailSuccess = false;
                        }
                    }



                    return response;
                }
            }
            catch (Exception e)
            {
                return new FlagResponse { EmailSuccess = false, FlagSuccess = false };
            }
        }

        private bool SendFlagEmail(FoneClubeContext ctx, GenericFlag flag, tblGenericPhoneFlags tblGenericPhoneFlags, tblGenericFlags flagType)
        {
            try
            {
                var phone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == flag.IdPhone);
                var plan = new AccountAccess().GetPlans().FirstOrDefault(p => p.Id == flag.PlanId);

                if (plan == null)
                    plan = new Plan { Description = string.Empty };

                if (plan != null && phone != null)
                {
                    var data = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                    var descriptionAbreviada = flagType.txtFlagDescription.Substring(0, 3);
                    var footer = string.Format("{0}-{1}-{2}", tblGenericPhoneFlags.intId, data, descriptionAbreviada);

                    var template = ctx.tblEmailTemplates.FirstOrDefault(e => e.intId == flagType.intIdEmail.Value);

                    var friendlyPhone = phone.intDDD.ToString() + phone.intPhone.ToString();
                    var dynamicText = string.Format("{0} / {1}", friendlyPhone, plan.Description);
                    if (plan.Description == string.Empty)
                        dynamicText = dynamicText.Replace("/", "");

                    var emailBody = string.Format(template.txtDescription, dynamicText, footer);

                    if (flag.FullEmail != null)
                    {
                        return new Utils().SendEmailMultiple(flag.FullEmail.To, string.Format(flag.FullEmail.Title, string.Empty), emailBody, flag.FullEmail.Cc, flag.FullEmail.Bcc);
                    }
                    else
                    {
                        return new Utils().SendEmailOperadora(template.toEmail, string.Format(template.txtSubject, string.Empty), emailBody);
                    }
                }
                else
                    return false;

            }
            catch (Exception e)
            {
                return false;
            }

        }


        public List<Phone> GetGenericPhoneFlags(List<Phone> phones, FoneClubeContext ctx)
        {
            try
            {
                var flagsTypes = ctx.tblGenericFlags.ToList();

                var flagsPhoneIds = ctx.tblGenericPhoneFlags
                    .Select(a => a.intIdPhone).ToList();

                var flags = ctx.tblGenericPhoneFlags
                        .Where(p => flagsPhoneIds.Contains(p.intIdPhone)).ToList();

                foreach (var phone in phones)
                {
                    if (flags.Count > 0)
                    {
                        phone.Flags = flags.Where(p => p.intIdPhone == phone.Id).Select(f => new Flag
                        {
                            Id = f.intId,
                            IdType = f.intIdFlag.Value,
                            InteractionDescription = f.txtDescription,
                            RegisterDate = Convert.ToDateTime(f.dteRegister),
                            PendingInteraction = Convert.ToBoolean(f.bitPendingInteraction),
                            TypeDescription = flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value) != null ?
                            flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value).txtFlagDescription
                            : string.Empty
                        }).ToList();
                    }
                }

                return phones;
            }
            catch (Exception e)
            {
                return phones;
            }
        }

        public List<Flag> GetGenericPersonFlags(int idPerson, FoneClubeContext ctx)
        {
            try
            {

                var flagsTypes = ctx.tblGenericFlags.ToList();

                return ctx.tblGenericPhoneFlags
                        .Where(p => p.intIdPerson == idPerson).Select(f => new Flag
                        {
                            Id = f.intId,
                            IdType = f.intIdFlag.Value,
                            InteractionDescription = f.txtDescription,
                            RegisterDate = Convert.ToDateTime(f.dteRegister),
                            PendingInteraction = Convert.ToBoolean(f.bitPendingInteraction),
                            TypeDescription = flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value) != null ?
                            flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value).txtFlagDescription
                            : string.Empty
                        }).ToList();
            }
            catch (Exception e)
            {
                return new List<Flag>();
            }
        }

        public bool UpdatePendingFlagStatus(GenericFlag genericFlag)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var flag = ctx.tblGenericPhoneFlags.FirstOrDefault(f => f.intId == genericFlag.Id);
                    flag.bitPendingInteraction = genericFlag.PendingInteraction;
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Flag> GetGenericFlagsTypes(bool phoneFlag)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    if (phoneFlag)
                    {
                        var flags = ctx.tblGenericFlags.Where(c => c.bitPhoneFlag == true).ToList();
                        var flagEmailsIds = flags.Select(f => f.intIdEmail).ToList();
                        var templates = ctx.tblEmailTemplates.Where(e => flagEmailsIds.Contains(e.intId)).ToList();

                        return flags.Select(f => new Flag
                        {

                            IdType = f.intId,
                            TypeDescription = f.txtFlagDescription,
                            PhoneFlag = Convert.ToBoolean(f.bitPhoneFlag),
                            FullEmail = new FullEmail
                            {
                                Title = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).txtSubject : string.Empty,
                                Body = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).txtDescription : string.Empty,
                                To = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).toEmail : string.Empty,
                                Cc = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).cc : string.Empty,
                                Bcc = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).bcc : string.Empty
                            }
                        }).ToList();
                    }
                    else
                    {
                        var flags = ctx.tblGenericFlags.ToList();
                        var flagEmailsIds = flags.Select(f => f.intIdEmail).ToList();
                        var templates = ctx.tblEmailTemplates.Where(e => flagEmailsIds.Contains(e.intId)).ToList();

                        return ctx.tblGenericFlags.ToList().Select(f => new Flag
                        {

                            IdType = f.intId,
                            TypeDescription = f.txtFlagDescription,
                            PhoneFlag = Convert.ToBoolean(f.bitPhoneFlag),
                            FullEmail = new FullEmail
                            {
                                Title = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).txtSubject : string.Empty,
                                Body = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).txtDescription : string.Empty,
                                To = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).toEmail : string.Empty,
                                Cc = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).cc : string.Empty,
                                Bcc = templates.FirstOrDefault(t => t.intId == f.intIdEmail) != null ? templates.FirstOrDefault(t => t.intId == f.intIdEmail).bcc : string.Empty
                            }
                        }).ToList();
                    }

                }
                catch (Exception)
                {
                    return new List<Flag>();
                }
            }
        }

        //todo definir local de flags
        public List<Flag> GetAllGenericFlags(int idPerson)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var flags = new List<Flag>();
                    var flagsTypes = ctx.tblGenericFlags.ToList();

                    var flagsPhoneIds = ctx.tblPersonsPhones
                    .Where(p => p.intIdPerson == idPerson && p.bitPhoneClube == true)
                    .Select(pp => pp.intId).ToList();

                    var genericFlags = ctx.tblGenericPhoneFlags
                            .Where(p => flagsPhoneIds.Contains(p.intIdPhone.Value))
                            .ToList();

                    genericFlags.AddRange(ctx.tblGenericPhoneFlags
                        .Where(p => p.intIdPerson == idPerson).ToList());

                    foreach (var f in genericFlags)
                    {
                        flags.Add(new Flag
                        {
                            Id = f.intId,
                            IdType = f.intIdFlag.Value,
                            InteractionDescription = f.txtDescription,
                            RegisterDate = Convert.ToDateTime(f.dteRegister),
                            PendingInteraction = Convert.ToBoolean(f.bitPendingInteraction),
                            TypeDescription = flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value) != null ?
                                flagsTypes.FirstOrDefault(o => o.intId == f.intIdFlag.Value).txtFlagDescription
                                : string.Empty
                        });
                    }



                    return flags;
                    //método primário
                    //var person = new ProfileAccess().GetPerson(intIdPerson);
                    //person.Phones = GetGenericPhoneFlags(person.Phones, ctx);
                    //person.Flags = GetGenericPersonFlags(intIdPerson, ctx);
                    //return person;
                }
                catch (Exception e)
                {
                    throw new Exception();
                }

            }
        }

        public List<PhonePlanViewModel> GetPlanosCliente(int matricula)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var phonesPlans = new List<PhonePlanViewModel>();
                    var planosOptions = ctx.tblPlansOptions.Where(p => p.intBitActive == true).ToList();
                    var planos = ctx.tblPersonsPhones.Where(p => p.intIdPerson == matricula && p.bitPhoneClube.HasValue && p.bitPhoneClube == true && (!p.bitDelete.HasValue || !p.bitDelete.Value) && p.bitAtivo.HasValue && p.bitAtivo.Value).ToList();
                    foreach (var plano in planos)
                    {
                        var detalhe = planosOptions.FirstOrDefault(a => a.intIdPlan == plano.intIdPlan);
                        phonesPlans.Add(new PhonePlanViewModel
                        {
                            Telefone = plano.intDDD.ToString() + "" + plano.intPhone.ToString(),
                            TelefoneFormatado = "(" + plano.intDDD.ToString() + ") " + plano.intPhone.ToString(),
                            DescricaoAbreviadaPlano = detalhe.txtDescriptionResumo,
                            DescricaoPlano = detalhe.txtDescriptionAmigavel
                        });
                    }

                    return phonesPlans;
                }
                catch (Exception e)
                {
                    return new List<PhonePlanViewModel>();
                }

            }
        }

        public List<PhonePlanViewModel> GetPlanosCliente(string email)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var phonesPlans = new List<PhonePlanViewModel>();
                    var planosOptions = ctx.tblPlansOptions.Where(p => p.intBitActive == true).ToList();
                    var cliente = ctx.tblPersons.FirstOrDefault(p => p.txtEmail == email);
                    if (cliente == null)
                    {
                        return new List<PhonePlanViewModel>();
                    }

                    var planos = ctx.tblPersonsPhones.Where(p => p.intIdPerson == cliente.intIdPerson && p.bitPhoneClube == true && p.bitDelete != true).ToList();
                    foreach (var plano in planos)
                    {
                        var detalhe = planosOptions.FirstOrDefault(a => a.intIdPlan == plano.intIdPlan);
                        phonesPlans.Add(new PhonePlanViewModel
                        {
                            Telefone = plano.intDDD.ToString() + "" + plano.intPhone.ToString(),
                            TelefoneFormatado = "(" + plano.intDDD.ToString() + ") " + plano.intPhone.ToString(),
                            DescricaoAbreviadaPlano = detalhe.txtDescriptionResumo,
                            DescricaoPlano = detalhe.txtDescriptionAmigavel
                        });
                    }

                    return phonesPlans;
                }
                catch (Exception e)
                {
                    return new List<PhonePlanViewModel>();
                }

            }
        }

        public string GetCorpoPlanos(List<PhonePlanViewModel> planos)
        {
            if (planos.Count == 0)
                return string.Empty;

            var complemento = "<br>" + "<span style='font-weight: 600'>Sua linha FONECLUBE:</ span > <br>";

            if (planos.Count > 1)
                complemento = "<br>" + "<span style='font-weight: 600'>Suas linhas FONECLUBE:</ span > <br>";

            foreach (var plano in planos)
            {
                complemento += "<span>" + "<span style='font-weight: 600'>" + plano.DescricaoPlano + "</ span >" + "  " + plano.TelefoneFormatado + "</span ><br>";
            }

            return complemento;
        }



        #region New Calls 

        public List<PhoneServiceViewModel> GetAllActivePhoneServices()
        {
            using (var context = new FoneClubeContext())
            {
                try
                {
                    return (from service in context.tblServices
                            join phoneService in context.tblPhonesServices on service.intIdService equals phoneService.intIdService
                            where service.assinaturas == 1 && service.IsExtraOption == true && phoneService.bitAtivo == true
                            select new PhoneServiceViewModel
                            {
                                ServiceId = service.intIdService,
                                ServiceName = service.ServiceDesc,
                                PersonPhoneId = phoneService.intIdPhone,
                                ActiveDate = phoneService.dteAtivacao,
                                DeActiveDate = phoneService.dteDesativacao,
                            }).ToList();
                }
                catch (Exception e)
                {
                    throw new Exception();
                }

            }
        }

        public List<PhoneServiceViewModel> GetActivePhoneServices(int personId)
        {
            using (var context = new FoneClubeContext())
            {
                try
                {
                    return (
                    from service in context.tblServices
                    join phoneService in context.tblPhonesServices on service.intIdService equals phoneService.intIdService
                    where service.assinaturas == 1
                    && service.IsExtraOption == true
                    && phoneService.bitAtivo == true
                    && phoneService.intIdPhone == personId
                    select new PhoneServiceViewModel
                    {
                        ServiceId = service.intIdService,
                        ServiceName = service.ServiceDesc,
                        PersonPhoneId = phoneService.intIdPhone,
                        ActiveDate = phoneService.dteAtivacao,
                        DeActiveDate = phoneService.dteDesativacao,
                    }).ToList();
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
        }

        public List<PhoneViewModel> GetAllPhonesNumbers()
        {
            using (var ctx = new FoneClubeContext())
            {
                var activePhoneServices = GetAllActivePhoneServices();
                var allphones = ctx.GetAllPhoneNumbers().ToList();
                var stockDetails = ctx.tblPhoneStock.ToList();

                var listaTIM =
                (from pp in ctx.tblPersonsPhones
                 join p in ctx.tblPersons on pp.intIdPerson equals p.intIdPerson
                 join pl in ctx.tblPlansOptions on pp.intIdPlan equals pl.intIdPlan
                 where pp.bitPhoneClube == true && pp.bitAtivo == true && pp.intIdOperator == 3
                 select new PhoneViewModel
                 {
                     PersonName = !string.IsNullOrEmpty(p.txtName) ? p.txtName : "~",
                     NickName = !string.IsNullOrEmpty(p.txtNickName) ? p.txtNickName : "~",
                     Email = !string.IsNullOrEmpty(p.txtEmail) ? p.txtEmail : "~",
                     Register = p.dteRegister != null ? p.dteRegister.ToString() : string.Empty,
                     //Born = Convert.ToDateTime(p.dteBorn).ToLongDateString(), 
                     Phone = pp.intPhone.ToString(),
                     CompletePhone = pp.intDDD.ToString() + pp.intPhone.ToString(),
                     DisplayPhone = pp.intDDD.ToString() + pp.intPhone.ToString(),
                     OperatorName = pl.txtDescription,
                     PlanId = pp.intIdPlan.Value,
                     PlanDescription = pl.txtDescription,
                     PlanCost = pl.intCost,
                     PersonPhoneId = pp.intId,
                     AmoutPrecoVip = pp.intAmmoutPrecoVip == null ? 0 : pp.intAmmoutPrecoVip.Value,
                     PersonId = p.intIdPerson,
                     OperatorId = pp.intIdOperator.Value,
                     UsoLinha = -1

                 }).ToList();

                foreach (var linhaTIM in listaTIM)
                {
                    linhaTIM.PhoneServices = activePhoneServices.Where(f => f.PersonPhoneId == linhaTIM.PersonPhoneId).ToList();
                }


                var listaRetorno = allphones.Select(p => new PhoneViewModel
                {
                    PersonName = !string.IsNullOrEmpty(p.PersonName) ? p.PersonName : "~",
                    NickName = !string.IsNullOrEmpty(p.NickName) ? p.NickName : string.Empty,
                    Email = !string.IsNullOrEmpty(p.Email) ? p.Email : string.Empty,
                    Register = p.Register != null ? Convert.ToDateTime(p.Register).ToLongDateString() : string.Empty,
                    Born = p.Born != null ? Convert.ToDateTime(p.Born).ToLongTimeString() : string.Empty,
                    Gender = p.Gender != null ? p.Gender.ToString() : string.Empty,
                    Phone = p.Phone != null ? p.Phone.ToString() : string.Empty,
                    CompletePhone = p.CompletePhone != null ? p.CompletePhone.ToString() : string.Empty,
                    DisplayPhone = p.DisplayPhone != null ? p.DisplayPhone.ToString() : string.Empty,
                    OperatorName = p.OperatorName != null ? p.OperatorName.ToString() : string.Empty,
                    MasterOperatorName = p.MasterOperatorName != null ? p.MasterOperatorName : string.Empty,
                    PlanDescription = p.PlanDescription != null ? p.PlanDescription : string.Empty,
                    EntradaLinha = p.EntradaLinha != null ? Convert.ToDateTime(p.EntradaLinha).ToLongTimeString() : string.Empty,
                    SaidaLinha = p.SaidaLinha != null ? Convert.ToDateTime(p.SaidaLinha).ToLongTimeString() : string.Empty,
                    StatusName = p.StatusName != null ? p.StatusName : string.Empty,
                    PhoneNumber = Convert.ToInt32(p.PhoneId),
                    PersonPhoneId = Convert.ToInt32(p.PersonPhoneId),
                    DDNumber = Convert.ToInt32(p.DDD),
                    PlanCost = Convert.ToInt32(p.PlanCost),
                    AmoutPrecoVip = Convert.ToInt32(p.AmmoutPrecoVip),
                    //AdditionServiceCost = Convert.ToInt32(p.AdditionServiceCost),
                    IsPrecoVip = Convert.ToBoolean(p.PrecoVip),
                    BonusConceded = Convert.ToBoolean(p.BonusConceded),
                    Dono = Convert.ToBoolean(p.Dono),
                    IsActive = Convert.ToBoolean(p.Ativo),
                    IsPortability = Convert.ToBoolean(p.Portability),
                    PlanId = Convert.ToInt32(p.PlanId),
                    PersonId = Convert.ToInt32(p.PersonId),
                    OperatorId = Convert.ToInt32(p.OperatorId),
                    StatusId = Convert.ToInt32(p.StatusId),
                    PhoneServices = activePhoneServices.Where(f => f.PersonPhoneId == p.PersonPhoneId).ToList(),
                    UsoLinha = Convert.ToInt32(p.UsoLinha)
                }).ToList();

                listaRetorno.AddRange(listaTIM);

                foreach (var phone in listaRetorno)
                {
                    phone.CCID = string.Empty;
                    phone.RazaoSocial = string.Empty;
                    phone.CodigoCliente = string.Empty;
                    phone.Register = string.Empty;

                    var detailsTim = stockDetails.FirstOrDefault(p => p.txtNumero == phone.CompletePhone);
                    if (detailsTim != null)
                    {
                        phone.CodigoCliente = detailsTim.txtCodCliente;
                        phone.RazaoSocial = detailsTim.txtRazaoSocial;
                        phone.CCID = detailsTim.txtICCID;

                        var dono = "FC";
                        if (detailsTim.txtRazaoSocial.Contains("RM"))
                            dono = "RM";

                        phone.Register = dono + " : " + detailsTim.txtICCID + " : " + detailsTim.txtCodCliente;
                    }

                    if (phone.DocumentNumber == null && phone.Register == string.Empty && phone.NickName == string.Empty)
                    {
                        phone.DocumentNumber = "08.453.543/0001-76";
                        phone.PersonName = "FONECLUBE Estoque";
                    }
                }

                return listaRetorno;

            }
        }

        public List<PhoneViewModel> GetAvailablePhoneNumbers(string phoneNumber)
        {
            using (var context = new FoneClubeContext())
            {
                var result = context.PrAvailablePhoneNumbers().ToList();
                var allPhones = new List<PhoneViewModel>();

                if (result != null)
                {
                    foreach (var resultPhone in result)
                    {
                        allPhones.Add(new PhoneViewModel
                        {
                            CompletePhone = resultPhone.CompletePhone,
                            DisplayPhone = resultPhone.DisplayPhone
                        });
                    }
                }

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    allPhones.Add(new PhoneViewModel
                    {
                        CompletePhone = phoneNumber,
                        DisplayPhone = String.Format("({0}) {1}-{2}"
                        , phoneNumber.Substring(0, 2)
                        , phoneNumber.Substring(2, 5)
                        , phoneNumber.Substring(7, 4))
                    });
                }

                return allPhones;
            }
        }


        public List<PhoneViewModel> GetAllCustomerPhones(int personId)
        {
            using (var ctx = new FoneClubeContext())
            {

                var activePhoneServices = GetAllActivePhoneServices();

                return ctx.GetAllPhoneNumbersByPerson(personId).ToList()
                    .Select(p => new PhoneViewModel
                    {
                        PersonName = !string.IsNullOrEmpty(p.PersonName) ? p.PersonName : "~",
                        NickName = !string.IsNullOrEmpty(p.NickName) ? p.NickName : string.Empty,
                        Email = !string.IsNullOrEmpty(p.Email) ? p.Email : string.Empty,
                        Register = p.Register != null ? p.Register.ToLongTimeString() : string.Empty,
                        Born = p.Born != null ? Convert.ToDateTime(p.Born).ToLongTimeString() : string.Empty,
                        Gender = p.Gender != null ? p.Gender.ToString() : string.Empty,
                        Phone = p.Phone != null ? p.Phone.ToString() : string.Empty,
                        CompletePhone = p.CompletePhone != null ? p.CompletePhone.ToString() : string.Empty,
                        DisplayPhone = p.DisplayPhone != null ? p.DisplayPhone.ToString() : string.Empty,
                        OperatorName = p.OperatorName != null ? p.OperatorName.ToString() : string.Empty,
                        MasterOperatorName = p.MasterOperatorName != null ? p.MasterOperatorName : string.Empty,
                        PlanDescription = p.PlanDescription != null ? p.PlanDescription : string.Empty,
                        EntradaLinha = p.EntradaLinha != null ? Convert.ToDateTime(p.EntradaLinha).ToLongTimeString() : string.Empty,
                        SaidaLinha = p.SaidaLinha != null ? Convert.ToDateTime(p.SaidaLinha).ToLongTimeString() : string.Empty,
                        StatusName = p.StatusName != null ? p.StatusName : string.Empty,
                        PhoneNumber = !string.IsNullOrEmpty(p.PhoneId.ToString()) ? p.PhoneId : 0,
                        PersonPhoneId = !string.IsNullOrEmpty(p.PersonPhoneId.ToString()) ? p.PersonPhoneId : 0,
                        DDNumber = !string.IsNullOrEmpty(p.DDD.ToString()) ? p.DDD.Value : 0,
                        PlanCost = !string.IsNullOrEmpty(p.PlanCost.ToString()) ? p.PlanCost : 0,
                        AmoutPrecoVip = !string.IsNullOrEmpty(p.AmmoutPrecoVip.ToString()) ? p.AmmoutPrecoVip : 0,
                        AdditionServiceCost = !string.IsNullOrEmpty(p.AdditionServiceCost.ToString()) ? p.AdditionServiceCost.Value : 0,
                        IsPrecoVip = Convert.ToBoolean(p.PrecoVip),
                        BonusConceded = Convert.ToBoolean(p.BonusConceded),
                        Dono = Convert.ToBoolean(p.Dono),
                        IsActive = Convert.ToBoolean(p.Ativo),
                        IsPortability = Convert.ToBoolean(p.Portability),
                        PlanId = Convert.ToInt32(p.PlanId),
                        PersonId = Convert.ToInt32(p.PersonId),
                        OperatorId = Convert.ToInt32(p.OperatorId),
                        StatusId = Convert.ToInt32(p.StatusId),
                        PhoneServices = activePhoneServices.Where(f => f.PersonPhoneId == p.PersonPhoneId).ToList()
                    }).ToList();

            }
        }

        public decimal GetMonthlyAmount(int personId)
        {
            using (var context = new FoneClubeContext())
            {
                var phoneNumbers = GetAllCustomerPhones(personId);

                decimal monthlyAmount = 0;
                foreach (var item in phoneNumbers)
                {
                    monthlyAmount += item.AmoutPrecoVip == 0 ? item.PlanCost : item.AmoutPrecoVip;
                }

                return monthlyAmount != 0 ? monthlyAmount / 100 : 0;

            }
        }

        #endregion


        public List<PhoneLines> GetAllPhonesLines()
        {
            List<PhoneLines> phoneLines = new List<PhoneLines>();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var tblresult = ctx.GetAllPhoneLines().ToList();
                    foreach (var res in tblresult)
                    {
                        phoneLines.Add(new PhoneLines()
                        {
                            Id = res.Id,
                            TopUp = "Y",
                            TopUpHistory = true,
                            IdPerson = res.IdPerson.HasValue ? res.IdPerson.Value : 0,
                            Ativa = res.Ativa,
                            ClienteAtivo_FC = res.ClienteAtivo_FC,
                            CPF_DR = res.CPF_DR,
                            CPF_FC = res.CPF_FC,
                            ICCID = res.ICCID,
                            LinhaSemUso = res.LinhaSemUso.HasValue && res.LinhaSemUso.Value ? true : false,
                            Linha_DR = res.Linha_DR,
                            Linha_FC = res.Linha_FC,
                            Nome_DR = res.Nome_DR,
                            Nome_FC = res.Nome_FC,
                            PhoneNumber = res.PhoneNumber,
                            Plano_DR = res.Plano_DR,
                            Plano_FC = res.Plano_FC,
                            Plugin_DR = res.Plugin_DR,
                            PrecoUnico = res.PrecoUnico,
                            PrecoVIP = res.PrecoVIP,
                            Preco_FC = res.Preco_FC,
                            Propriedade = res.Propriedade,
                            Roaming = res.Roaming,
                            Total_DR = res.Total_DR,
                            Total_FC = res.Total_FC,
                            Apelido = res.Apelido,
                            Plano_Contel = res.Plano_Contel,
                            Saldo = res.Saldo,
                            Recarga_Automatica = !string.IsNullOrEmpty(res.Plano_Contel) ? res.Recarga_Automatica : (bool?)null,
                            Cancelation_Date = res.Cancelation_Date,
                            FimPlano = res.FimPlano,
                            PortNumber = res.PortNumber,
                            Ativacao = res.Ativacao,
                            InicioPlano = res.InicioPlano,
                            Bloqueada = res.Bloqueada,
                            Esim = res.Esim,
                            VIPSum = res.VIPSum,
                            FCSum = res.FCSum,
                            StatusCob = res.StatusCob,
                            UltPagDias = res.UltPagDias.HasValue ? res.UltPagDias.Value : 0,
                            IsContelLine = res.IsContelLine.Value,
                            ContelStatus = res.ContelStatus,
                            AutoRec = res.AutoRec,
                            PortIn = res.PortIn,
                            DocContel = res.DocContel,
                            LastPaidAmount = res.LastPaidAmount,
                            ContelBlockStatus = res.ContelStatus == "CANCELADO" ? "C" : res.IsContelLine.HasValue && res.IsContelLine.Value == true ? (res.Bloqueada == "NÃO" || string.IsNullOrEmpty(res.Bloqueada)) ? "A" : "B" : "O",
                            RecAutFC = res.RecAutFC,
                            ValorPago = res.ValorPago,
                            RecAutFCFlag = res.RecAutFCFlag.HasValue ? res.RecAutFCFlag.Value : false,
                            DaysSinceLastTopup = res.DaysSinceLastTopup.HasValue ? res.DaysSinceLastTopup.Value : 0,
                            Agendado = res.Agendado.HasValue ? res.Agendado.Value : 0,
                            DteRegistered = res.DteRegistered
                        });
                    }
                }
                catch (Exception ex)
                {
                    ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "GetAllPhonelines error:" + ex.ToString() });
                    ctx.SaveChanges();
                }
            }
            return phoneLines;
        }

        public bool SavePhonesStore(List<PlanoStore> plans, int personId)
        {
            if (plans != null && plans.Count() > 0)
            {
                using (var ctx = new FoneClubeContext())
                {
                    foreach (var plan in plans)
                    {
                        var phone = plan.txtPortNumber.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                        {
                            intIdPerson = personId,
                            intDDD = 21,
                            intPhone = 988887777,
                            intCountryCode = 55,
                            bitPortability = plan.isPort && !string.IsNullOrEmpty(phone) ? plan.isPort : false,
                            txtPortNumber = plan.isPort && !string.IsNullOrEmpty(phone) ? plan.txtPortNumber : "",
                            bitAtivo = false,
                            bitDelete = false,
                            bitEsim = plan.isEsim,
                            bitPhoneClube = true,
                            intIdOperator = 4,
                            intIdPlan = plan.id,
                            intAmmoutPrecoVip = plan.valor,
                        });
                        ctx.SaveChanges();
                    }

                    var delCart = ctx.tblUnPlacedCartItems.FirstOrDefault(x => x.intIdPerson == personId);
                    if (delCart != null)
                    {
                        ctx.tblUnPlacedCartItems.Remove(delCart);
                        ctx.SaveChanges();
                    }
                }


            }
            return true;
        }
        public bool SaveStorePlanDetails(List<PlanoStore> plans, int shipmentType, int personId)
        {
            if (plans != null && plans.Count() > 0)
            {
                using (var ctx = new FoneClubeContext())
                {
                    var charge = ctx.tblChargingHistory.Where(x => x.intIdCustomer == personId).OrderByDescending(x => x.intId).FirstOrDefault();
                    var planTotal = plans.Sum(x => x.valor);
                    var newOrder = new tblStoreOrders()
                    {
                        intIdPerson = personId,
                        dteOrderDate = DateTime.Now,
                        intNumberOfPlans = plans.Count,
                        txtTotal = planTotal,
                        intIdCharge = charge != null ? charge.intId : 0,
                        intIdTransaction = charge != null ? charge.intIdTransaction.Value : 0,
                        txtPaymentStatus = "Aguardando Pagamento",
                        txtStatus = "Pendente",
                    };

                    ctx.tblStoreOrders.Add(newOrder);
                    ctx.SaveChanges();

                    long orderId = newOrder.intOrderId;

                    foreach (var plan in plans)
                    {
                        ctx.tblStoreCustomerPlans.Add(new tblStoreCustomerPlans()
                        {
                            intOrderId = orderId,
                            intIdPerson = personId,
                            bitESim = plan.isEsim,
                            bitPort = plan.isPort,
                            txtPortNumber = plan.txtPortNumber,
                            intIdCharge = charge != null ? charge.intId : 0,
                            intIdPlan = plan.id,
                            intIdTransaction = charge != null ? charge.intIdTransaction.Value : 0,
                            txtPlanDescription = plan.descricao,
                            txtPlanAmount = plan.valor.ToString(),
                            txtChipAmount = plan.isEsim ? "0" : "1000",
                            txtShippingAmount = shipmentType == 2 ? "1000" : "0"
                        });
                        ctx.SaveChanges();
                    }
                }
            }
            return true;
        }
    }
}
