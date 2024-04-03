using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.FoneClube.phone;
using FoneClube.Business.Commons.Entities.ViewModel;
using FoneClube.Business.Commons.Entities.ViewModel.MinhaConta;
using FoneClube.Business.Commons.Entities.woocommerce;
using FoneClube.Business.Commons.Entities.woocommerce.order;
using FoneClube.Business.Commons.Utils;
using FoneClube.DataAccess.affiliates;
using FoneClube.DataAccess.blink;
using FoneClube.DataAccess.chat2desk;
using FoneClube.DataAccess.security;
using FoneClube.DataAccess.Utilities;
using FoneClube.DataAccess.pagarme;
using FoneClube.DataAccess.woocommerce;
//using FoneClube.DataAccess.woocommerce;
using Newtonsoft.Json;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace FoneClube.DataAccess
{
    public class ProfileAccess
    {
        const string pattern = @"[0-9]+GB|MB";
        Regex rg = new Regex(pattern);
        public HttpStatusCode SavePerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                if (InsertPerson(person) != HttpStatusCode.OK)
                    return HttpStatusCode.BadRequest;

                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim());
                if (!bool.Equals(tblpessoa, null))
                    person.Id = tblpessoa.intIdPerson;
                else
                    throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Não foi possível coleta ID"));

                if (!SetCustomerParentPhone(person))
                    throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Não foi possível salvar telefone do Pai"));

                ctx.tblPersonsAddresses.RemoveRange(ctx.tblPersonsAddresses.Where(p => p.intIdPerson == person.Id).ToList());
                if (!SavePersonAddresses(person, ctx))
                    throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Não foi possível update em endereço"));

            }
            return HttpStatusCode.OK;
        }



        //refatorar
        public Person GetPerson(int id)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == id);

                if (bool.Equals(tblpessoa, null))
                {
                    return new Person();
                }
                else
                {
                    return GetPerson(tblpessoa.txtDocumentNumber.Trim());
                }
            }
        }

        public List<GetAllCustomers_Result> GetAllCustomersNew()
        {
            using (var ctx = new FoneClubeContext())
            {
                return ctx.GetAllCustomers().ToList();
            }
        }

        public List<CustomersListViewModel> GetAllCustomers()
        {
            var persons = new List<Person>();
            var customersViewModel = new List<CustomersListViewModel>();

            using (var ctx = new FoneClubeContext())
            {
                List<Person> customers;
                var todosClientes = ctx.GetTodosClientes().ToList();
                var usersettings = ctx.tblUserSettings.ToList();

                customers = todosClientes.Select(p => new Person
                {
                    Id = p.intIdPerson,
                    Email = p.txtEmail,
                    Name = p.txtName,
                    DocumentNumber = p.txtDocumentNumber,
                    Register = p.dteRegister,
                    IdPagarme = p.intIdPagarme,
                    SoftDelete = p.bitDelete,
                    Desativo = p.bitDesativoManual,
                    DefaultWAPhones = p.txtDefaultWAPhones,
                    IsVIP = usersettings.Any(x => x.intPerson == p.intIdPerson) ? usersettings.Where(x => x.intPerson == p.intIdPerson).FirstOrDefault().bitVIP : false,
                    Use2Prices = usersettings.Any(x => x.intPerson == p.intIdPerson) ? usersettings.Where(x => x.intPerson == p.intIdPerson).FirstOrDefault().bitUse2Prices : false
                }).Where(p => p.Desativo != true).ToList();

                persons.AddRange(customers);

                var pendingFlagInteraction = ctx.GetCustomersPendingFlags().ToList();
                var idCustomers = customers.Select(c => c.Id).ToList();

                foreach (var person in persons)
                {
                    var pendingInteraction = pendingFlagInteraction.FirstOrDefault(p => p.intIdPerson == person.Id);

                    if (pendingInteraction != null)
                        person.PendingFlagInteraction = pendingInteraction.hasPendingPersonFlag > 0 || pendingInteraction.hasPendingPhoneFlag > 0;
                }

                var tblPhonesList = ctx.tblPersonsPhones.Where(a => idCustomers.Contains(a.intIdPerson.Value) && a.bitPhoneClube == false)
                        .Select(p => new
                        {
                            CustomerId = p.intIdPerson.Value,
                            Phones = new Phone
                            {
                                DDD = p.intDDD.ToString(),
                                Number = p.intPhone.ToString(),
                                CountryCode = p.intCountryCode.HasValue ? p.intCountryCode.Value.ToString() : "55"
                            }
                        }).ToList();

                var tblAdress = ctx.tblPersonsAddresses.Where(a => idCustomers.Contains(a.intIdPerson.Value))
                        .Select(a => new
                        {
                            CustomerId = a.intIdPerson.Value,
                            Adress = new Adress
                            {
                                Cep = a.txtCep,
                                City = a.txtCity,
                                Country = a.txtCountry,
                                State = a.txtState,
                                Street = a.txtStreet,
                                StreetNumber = a.intStreetNumber.ToString(),
                                Complement = a.txtComplement,
                                Neighborhood = a.txtNeighborhood
                            }
                        }).ToList();

                var tblChargings = ctx.tblChargingHistory.OrderByDescending(x => x.dteCreate).Select(c => new
                {
                    CustomerId = c.intIdCustomer,
                    LastPayment = c.dteCreate
                }).ToList();

                foreach (var customer in persons)
                {
                    TimeSpan? diasSemCobrar = null;
                    var charge = tblChargings.FirstOrDefault(x => x.CustomerId == customer.Id);
                    if (charge != null)
                        diasSemCobrar = DateTime.Now - charge.LastPayment;

                    var telefone = "pendente";
                    var telefoneCustomer = tblPhonesList.FirstOrDefault(p => p.CustomerId == customer.Id);
                    var addressCustomer = tblAdress.FirstOrDefault(p => p.CustomerId == customer.Id);

                    var tblPhonesListNew = ctx.tblPersonsPhones.Where(a => a.intIdPerson == customer.Id)
                       .Select(p => new Phone()
                       {
                           DDD = p.intDDD.ToString(),
                           Number = p.intPhone.ToString(),
                           IsFoneclube = p.bitPhoneClube,
                           CountryCode = p.intCountryCode.HasValue ? p.intCountryCode.Value.ToString() : "55"
                       }).ToList();

                    if (telefoneCustomer != null)
                        if (telefoneCustomer.Phones != null)
                            telefone = "(" + telefoneCustomer.Phones.DDD.ToString() + ") " + telefoneCustomer.Phones.Number.ToString();

                    var afiliateLink = new Affiliates().GetReferralLink(customer.Id);

                    var parent = GetCustomerParent(customer);

                    customersViewModel.Add(new CustomersListViewModel
                    {
                        Id = customer.Id,
                        Email = customer.Email,
                        Name = customer.Name,
                        DocumentNumber = customer.DocumentNumber,
                        Telefone = telefone == "pendente" ? customer.DefaultWAPhones : telefone,
                        DiasSemCobrar = diasSemCobrar != null ? diasSemCobrar.Value.Days.ToString() : "s/c",
                        Address = addressCustomer != null && addressCustomer.Adress != null ? addressCustomer.Adress : null,
                        IsVIP = customer.IsVIP,
                        Use2Prices = customer.Use2Prices,
                        Referral = afiliateLink,
                        Phones = tblPhonesListNew,
                        Pai = parent.Pai,
                        IdPagarme = customer.IdPagarme
                    });
                }
            }

            return customersViewModel;
        }


        public class CustomersListViewModel
        {
            public bool? Desativo { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Telefone { get; set; }
            public string Email { get; set; }
            public string DocumentNumber { get; set; }
            public string DiasSemCobrar { get; set; }
            public Adress Address { get; set; }
            public int ParentId { get; set; }
            public bool IsVIP { get; set; }
            public bool Use2Prices { get; set; }
            public string Referral { get; set; }
            public List<Phone> Phones { get; set; }
            public Pai Pai { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public IntlPhone IntlPhone { get; set; }

            public int? IdPagarme { get; set; }
        }

        public Person GetPersonMinimal(int id)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == id);

                if (bool.Equals(tblpessoa, null))
                {
                    return new Person();
                }
                else
                {
                    return new Person
                    {
                        DocumentNumber = tblpessoa.txtDocumentNumber,
                        Name = tblpessoa.txtName
                    };
                }
            }
        }

        public tblPersons GetPersonyPhone(string phoneNum)
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.SaveChanges();
                string countryCode = phoneNum.Substring(0, 2);
                string areaCode = phoneNum.Substring(2, 2);
                string phoneNumber = phoneNum.Substring(4);
                var phone = ctx.tblPersonsPhones.Where(x => x.intCountryCode.Value.ToString() == countryCode
                && x.intDDD.Value.ToString() == areaCode && x.intPhone.Value.ToString() == phoneNumber && x.bitPhoneClube == false).FirstOrDefault();

                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == phone.intIdPerson);

                if (bool.Equals(tblpessoa, null))
                {
                    return null;
                }
                else
                {
                    return tblpessoa;
                }
            }
        }

        public HttpStatusCode HardDeletePerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                if (bool.Equals(pessoa, null))
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados")));

                if (Convert.ToBoolean(ctx.sp_DeletePerson(person.Id).FirstOrDefault()))
                    return HttpStatusCode.OK;
                else
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados ou o dado não foi deletado")));

            }

        }

        public HttpStatusCode SoftDeletePerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                if (bool.Equals(pessoa, null))
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados")));

                SetDisasociationPerson(person);
                DesativaLinhas(ctx, person);

                try
                {
                    pessoa.bitDelete = true;
                    ctx.SaveChanges();
                    return HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados ou Não foi atualizado")));
                }

            }

        }

        public HttpStatusCode UnDeletePerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                if (bool.Equals(pessoa, null))
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados")));

                SetAssociationPerson(person);

                AtivarLinhas(ctx, person);

                try
                {
                    pessoa.bitDelete = false;
                    ctx.SaveChanges();
                    return HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados ou Não foi atualizado")));
                }

            }

        }

        private static int GetPlanIdByGB(string gb)
        {
            int id = 315;
            switch (gb)
            {
                case "4 GB": id = 315; break;
                case "7 GB": id = 316; break;
                case "12 GB": id = 317; break;
                case "20 GB": id = 318; break;
                case "42 GB": id = 319; break;
            }
            return id;
        }

        private void AddPhoneLinesWithICCID(CustomerInstaChargeViewModel model, int idPerson)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == idPerson).FirstOrDefault();
                var address = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == idPerson).FirstOrDefault();
                var plans = ctx.tblPlansOptions.Where(x => x.intIdOperator == 4).ToList();
                var phoneCom = ctx.tblPersonsPhones.Where(x => x.bitPhoneClube == false).FirstOrDefault();

                if (address is null)
                {
                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        txtCep = "22793081",
                        intIdPerson = tblPerson.intIdPerson,
                        intStreetNumber = 3434,
                        txtCity = "Rio de Janeiro",
                        txtCountry = "Brasil",
                        txtNeighborhood = "Barra da Tijuca",
                        txtState = "RJ",
                        txtStreet = "Avenida das americas",
                        txtComplement = "305 bloco 2",
                        intAdressType = Constants.EnderecoCobranca
                    });
                    ctx.SaveChanges();

                    address = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == idPerson).FirstOrDefault();
                }

                //Format - ICCID|Plano|CPF|Phone|DDD|Port|Operado

                ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                activatePlanRequest.metodo_pagamento = "SALDO";
                activatePlanRequest.nome = tblPerson.txtName;
                if (tblPerson.txtDocumentNumber.Length == 11)
                    activatePlanRequest.cpf = tblPerson.txtDocumentNumber;
                else
                    activatePlanRequest.cnpj = tblPerson.txtDocumentNumber;
                activatePlanRequest.email = string.IsNullOrEmpty(tblPerson.txtEmail) ? "oi@facil.tel" : tblPerson.txtEmail;
                activatePlanRequest.telefone = phoneCom != null ? string.Concat(phoneCom.intDDD, phoneCom.intPhone) : "21999999999";
                activatePlanRequest.data_nascimento = "1900-01-01";
                activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();
                activatePlanRequest.endereco.rua = address.txtStreet;
                activatePlanRequest.endereco.numero = address.intStreetNumber.ToString();
                activatePlanRequest.endereco.complemento = address.txtComplement;
                activatePlanRequest.endereco.bairro = address.txtNeighborhood;
                activatePlanRequest.endereco.cep = address.txtCep.Replace("-", "").Replace(".", "");
                activatePlanRequest.endereco.municipio = address.txtCity;
                activatePlanRequest.endereco.uf = address.txtState;

                activatePlanRequest.chips = new List<Chip>();

                var selectedPlans = model.SelectedPlans.Split(new string[] { "#" }, StringSplitOptions.None);
                if (selectedPlans != null && selectedPlans.Length > 0)
                {
                    foreach (var splan in selectedPlans)
                    {
                        var planDetails = splan.Split('|');
                        if (planDetails != null && planDetails.Length > 0)
                        {
                            var iccid = planDetails[0];
                            var plan = !string.IsNullOrEmpty(planDetails[1]) ? Convert.ToInt32(planDetails[1]) : -1;
                            var vip = !string.IsNullOrEmpty(planDetails[2]) ? Convert.ToInt32(planDetails[2]) : 0;
                            var phone = !string.IsNullOrEmpty(planDetails[3]) ? planDetails[3].Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "") : "";
                            var ddd = !string.IsNullOrEmpty(planDetails[4]) ? Convert.ToInt32(planDetails[4]) : 0;
                            var port = !string.IsNullOrEmpty(planDetails[5]) ? Convert.ToInt64(planDetails[5]) : 0;
                            var operado = planDetails[6];
                            var activate = planDetails[7];

                            var nuvoDDD = phone != "" ? Convert.ToInt32(phone.Substring(0, 2)) : 0;
                            var nuvoPhone = phone != "" ? Convert.ToInt64(phone.Substring(2)) : 0;

                            tblPersonsPhones tblphones1 = null;
                            if (nuvoDDD == 0 || nuvoPhone == 0)
                            {
                                tblphones1 = ctx.tblPersonsPhones.FirstOrDefault(x => x.intDDD == ddd && x.intPhone == port && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value);
                            }
                            else
                            {
                                tblphones1 = ctx.tblPersonsPhones.FirstOrDefault(x => x.intDDD == nuvoDDD && x.intPhone == nuvoDDD && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value);
                            }

                            if (tblphones1 == null)
                            {
                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                {
                                    txtICCID = iccid,
                                    intDDD = nuvoDDD != 0 ? nuvoDDD : ddd == 0 ? 99 : ddd,
                                    intPhone = nuvoPhone != 0 ? nuvoPhone : port == 0 ? 999999999 : port,
                                    intIdPlan = plan,
                                    intIdPerson = idPerson,
                                    txtPortNumber = ddd + "" + port,
                                    bitDelete = false,
                                    bitPhoneClube = true,
                                    bitAtivo = true,
                                    intAmmoutPrecoVip = vip,
                                    bitEsim = model.Esim
                                });
                                ctx.SaveChanges();
                            }
                            else
                            {
                                tblphones1.txtICCID = iccid;
                                tblphones1.intIdPlan = plan;
                                tblphones1.intIdPerson = idPerson;
                                tblphones1.txtPortNumber = ddd + "" + port;
                                tblphones1.bitDelete = false;
                                tblphones1.bitPhoneClube = true;
                                tblphones1.bitAtivo = true;
                                tblphones1.intAmmoutPrecoVip = vip;
                                tblphones1.bitEsim = model.Esim;

                                ctx.SaveChanges();
                            }

                            if ((activate != null && activate == "Agora") || model.Bonus)
                            {
                                var chip = new Chip();
                                chip.iccid = iccid;
                                chip.id_plano = 316;
                                //chip.id_plano = plans.Where(x => x.intIdPlan == Convert.ToInt32(plan)).FirstOrDefault().intOperatorPlanId.Value;
                                chip.ddd = ddd == 0 ? ddd : 21;
                                chip.esim = model.Esim == true ? "SIM" : "N";
                                activatePlanRequest.chips.Add(chip);
                            }

                        }
                    }


                    if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                    {
                        MVNOAccess mVNOAccess = new MVNOAccess();
                        var response = mVNOAccess.ActivatePlan(activatePlanRequest);
                        if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                        {
                            foreach (var pho in response.info.chips)
                            {
                                var iccidPhoneData = mVNOAccess.ValidateICCID(pho.iccid);

                                if (iccidPhoneData != null && iccidPhoneData.retorno)
                                {

                                    if (iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                    {
                                        var activatedPhone = iccidPhoneData.info.numero_ativado;

                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                        //Update plans
                                        if (tblphones != null && tblphones.Count() > 0)
                                        {
                                            foreach (var ph in tblphones)
                                            {
                                                //ph.intIdPlan = plans.Where(x => x.intOperatorPlanId == Convert.ToInt32(pho.ativacao.id_plano)).FirstOrDefault().intIdPlan;
                                                ph.intIdPlan = 46;
                                                ph.bitAtivo = true;
                                                ph.bitPhoneClube = true;
                                                ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                            }
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                            {
                                                intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                intCountryCode = 55,
                                                intIdPlan = 46,
                                                bitAtivo = true,
                                                bitPhoneClube = true,
                                                intIdOperator = 4,
                                                intIdPerson = tblPerson.intIdPerson,
                                            });
                                            ctx.SaveChanges();
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan No chips to activate" });
                        ctx.SaveChanges();
                    }
                }
            }
        }

        private bool DesativaLinhas(FoneClubeContext ctx, Person person)
        {
            try
            {
                var phones = ctx.tblPersonsPhones
                                .Where(p => p.intIdPerson == person.Id)
                                .ToList();

                foreach (var phone in phones)
                {
                    phone.bitAtivo = false;
                }

                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AtivarLinhas(FoneClubeContext ctx, Person person)
        {
            try
            {
                var phones = ctx.tblPersonsPhones
                                .Where(p => p.intIdPerson == person.Id)
                                .ToList();

                foreach (var phone in phones)
                {
                    phone.bitAtivo = true;
                }

                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Person GetPerson(string documentNumber)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == documentNumber.Trim());


                if (bool.Equals(tblpessoa, null))
                    return new Person();


                var link = ctx.tblPersosAffiliateLinks.FirstOrDefault(p => p.intIdPerson == tblpessoa.intIdPerson);
                var settingexists = ctx.tblUserSettings.Where(x => x.intPerson == tblpessoa.intIdPerson).FirstOrDefault();

                var pessoa = new Person
                {
                    Id = tblpessoa.intIdPerson,
                    Email = tblpessoa.txtEmail,
                    Name = tblpessoa.txtName,
                    NickName = tblpessoa.txtNickName,
                    Born = tblpessoa.dteBorn.ToString(),
                    DocumentNumber = tblpessoa.txtDocumentNumber,
                    Gender = Convert.ToInt32(tblpessoa.intGender),
                    IdRole = Convert.ToInt32(tblpessoa.intIdRole),
                    Register = tblpessoa.dteRegister,
                    IdPagarme = tblpessoa.intIdPagarme,
                    SoftDelete = tblpessoa.bitDelete,
                    Desativo = tblpessoa.bitDesativoManual,
                    Use2Prices = settingexists != null ? settingexists.bitUse2Prices : false,
                    IsPrecoFCSum = settingexists != null ? settingexists.bitPrecoFCSum : false,
                    IsPrecoPromoSum = settingexists != null ? settingexists.bitPrecoPromoSum : false,
                    IsVIP = settingexists != null ? settingexists.bitVIP : false,
                    IsLinhaAtiva = settingexists != null ? settingexists.bitLinhaAtiva : false,
                    IsShowICCID = settingexists != null ? settingexists.bitShowICCID : false,
                    IsShowPort = settingexists != null ? settingexists.bitShowPort : false,
                    DefaultWAPhones = tblpessoa.txtDefaultWAPhones
                };

                try
                {
                    pessoa.AffiliateLink = link.txtBlinkLink;
                    pessoa.OriginalAffiliateLink = link.txtOriginalLink;
                    pessoa.Referral = new Affiliates().GetReferralLink(tblpessoa.intIdPerson);
                }
                catch (Exception) { }

                var parent = GetCustomerParent(pessoa);
                pessoa.NameParent = parent.NameParent;
                pessoa.PhoneDDDParent = parent.PhoneDDDParent;
                pessoa.PhoneNumberParent = parent.PhoneNumberParent;
                pessoa.LastChargeDate = GetLastChargeDate(pessoa);
                pessoa.Pai = parent.Pai;

                pessoa.Adresses = new List<Adress>();
                var addresses = ctx.tblPersonsAddresses.Where(a => a.intIdPerson == tblpessoa.intIdPerson).ToList();

                foreach (var address in addresses)
                {
                    pessoa.Adresses.Add(new Adress
                    {
                        Cep = address.txtCep,
                        City = address.txtCity,
                        Country = address.txtCountry,
                        State = address.txtState,
                        Street = address.txtStreet,
                        StreetNumber = address.intStreetNumber.ToString(),
                        Complement = address.txtComplement,
                        Neighborhood = address.txtNeighborhood

                    });
                }



                pessoa.Photos = new List<Photo>();
                var images = ctx.tblPersonsImages.Where(a => a.intIdPerson == tblpessoa.intIdPerson)
                    .Select(y => new Photo { Id = y.intId, Name = y.txtImage, Tipo = y.intTipo })
                    .ToList();
                pessoa.Photos = images;


                pessoa.Phones = new List<Phone>();
                var phones = ctx.tblPersonsPhones.Where(a => a.intIdPerson == tblpessoa.intIdPerson).ToList();
                var tblDrCelular = ctx.tblDrCelularTemp.ToList();
                var tblContelLinhas = ctx.tblContelLinhasList.ToList();
                var listComparePhones = ctx.GetPlansForComparison().Where(x => x.CPF_FC == pessoa.DocumentNumber).ToList();

                foreach (var phone in phones)
                {
                    int operadora = -1;
                    var idPlanOption = Convert.ToInt32(phone.intIdPlan);
                    var tblPlanOption = ctx.tblPlansOptions.FirstOrDefault(o => o.intIdPlan == idPlanOption);
                    if (tblPlanOption != null)
                        operadora = tblPlanOption.intIdOperator;

                    var celularData = tblDrCelular.Where(x => x.txtLinha.Replace("-", "") == string.Concat(phone.intDDD, phone.intPhone)).OrderByDescending(x => x.ano).ThenByDescending(x => x.mes).FirstOrDefault();
                    var ContelIccid = tblContelLinhas.Where(x => x.txticcid == phone.txtICCID).FirstOrDefault();
                    var ContelLinha = tblContelLinhas.Where(x => x.txtlinha == string.Concat(phone.intDDD, phone.intPhone)).FirstOrDefault();
                    var planDivergente = listComparePhones.Where(x => x.Linha_FC == string.Concat(phone.intDDD, phone.intPhone)).FirstOrDefault();
                    bool isSame = false;

                    if (planDivergente != null)
                    {
                        isSame = planDivergente.Divergente.ToLower() == "yes" ? false : true;
                    }

                    //Contel
                    var planSel = ctx.tblPlansOptions.FirstOrDefault(x => x.intIdPlan == phone.intIdPlan.Value);
                    Match match1 = rg.Match(planSel != null ? planSel.txtDescription : "");
                    Match match2 = rg.Match(ContelIccid != null ? ContelIccid.txtplano.Replace(" ", "") : ContelLinha != null ? ContelLinha.txtplano.Replace(" ", "") : "");
                    if (!string.IsNullOrEmpty(match1.Value) && !string.IsNullOrEmpty(match2.Value) && match1.Value.ToLower() == match2.Value.ToLower())
                        isSame = true;

                    var tel = new Phone
                    {
                        DDD = phone.intDDD.ToString(),
                        Number = phone.intPhone.ToString(),
                        IsFoneclube = phone.bitPhoneClube,
                        Id = phone.intId,
                        IdPlanOption = Convert.ToInt32(phone.intIdPlan),
                        NickName = phone.txtNickname,
                        CountryCode = Convert.ToString(phone.intCountryCode),
                        Portability = phone.bitPortability,
                        IdOperator = operadora,
                        LinhaAtiva = phone.bitAtivo,
                        Status = phone.intIdStatus,
                        AmmountPrecoVip = phone.intAmmoutPrecoVip,
                        OperatorChargedPrice = tblPlanOption != null ? tblPlanOption.intCost : 0,
                        PrecoVipStatus = phone.bitPrecoVip,
                        Delete = phone.bitDelete,
                        PluginDR = celularData != null ? celularData.txtFuncionalidade : "",
                        PlanoDR = celularData != null ? celularData.txtPlanoDeServico : "",
                        PlanoDivergente = isSame,
                        ESim = phone.bitEsim.HasValue ? phone.bitEsim.Value : false,
                        UsoLinha = celularData != null ? celularData.txtUsoLinha : "",
                        ICCID = !string.IsNullOrEmpty(phone.txtICCID) ? phone.txtICCID : ContelLinha != null ? ContelLinha.txticcid : phone.txtICCID,
                        PortNumber = !string.IsNullOrEmpty(phone.txtPortNumber) ? phone.txtPortNumber : ContelIccid == null ? phone.txtPortNumber : !string.IsNullOrEmpty(ContelIccid.txtlinha) ? ContelIccid.txtlinha : ContelLinha != null ? ContelLinha.txtlinha : "inativo",
                        IsContelLine = (ContelIccid != null || ContelLinha != null) ? true : false,
                        PlanoContel = ContelIccid != null ? ContelIccid.txtplano : ContelLinha != null ? ContelLinha.txtplano : "",
                        FimPlano = ContelLinha != null ? ContelLinha.txtdata_fim_plano : "",
                        AutoContel = ContelLinha != null ? ContelLinha.txtrecarga_automatica : "",
                        AutoRecFC = ContelLinha != null ? ContelLinha.dteAutoTopup : "",
                        ContelStatus = ContelLinha != null ? ContelLinha.txtstatus : "",
                        Cancela = ContelLinha != null ? ContelLinha.txtdata_cancelamento_linha : "",
                    };

                    tel.Servicos = new PhoneAccess().GetPhoneServices(tel).Servicos;

                    try
                    {
                        ////refact
                        //var claro = 1;
                        //var vivo = 2;

                        //if (tel.IdOperator == claro)
                        //{
                        //    var line = new ClaroAccess().GetLineDetails(tel.DDD + tel.Number);
                        //    tel.OperatorBlockedLineStatus = line.Profile.Contains("BLOQUEADO");
                        //}
                    }
                    catch (Exception e)
                    {

                    }


                    pessoa.Phones.Add(tel);
                    //todocode gere status claro
                }

                var discount = ctx.tblDiscountPrice.FirstOrDefault(d => d.intIdPerson == tblpessoa.intIdPerson);
                if (!bool.Equals(discount, null))
                {
                    pessoa.SinglePrice = Convert.ToInt64(discount.intAmmount);
                    pessoa.DescriptionSinglePrice = discount.txtDescription;

                }


                return pessoa;
            }

        }

        public Person GetPersonWithStatusPhone(string documentNumber)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblpessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == documentNumber.Trim());

                if (bool.Equals(tblpessoa, null))
                    return new Person();

                var pessoa = new Person
                {
                    Id = tblpessoa.intIdPerson,
                    Email = tblpessoa.txtEmail,
                    Name = tblpessoa.txtName,
                    NickName = tblpessoa.txtNickName,
                    Born = tblpessoa.dteBorn.ToString(),
                    DocumentNumber = tblpessoa.txtDocumentNumber,
                    Gender = Convert.ToInt32(tblpessoa.intGender),
                    IdRole = Convert.ToInt32(tblpessoa.intIdRole),
                    Register = tblpessoa.dteRegister,
                    IdPagarme = tblpessoa.intIdPagarme
                };

                var parent = GetCustomerParent(pessoa);
                pessoa.NameParent = parent.NameParent;
                pessoa.PhoneDDDParent = parent.PhoneDDDParent;
                pessoa.PhoneNumberParent = parent.PhoneNumberParent;
                pessoa.LastChargeDate = GetLastChargeDate(pessoa);
                pessoa.Pai = parent.Pai;

                //--------------------------- quando existia duas tabelas, remover o trecho apos dev feito
                //var whoInvite = tblpessoa.tblReferred1.FirstOrDefault(a => a.intIdCurrent == tblpessoa.intIdPerson);
                //if (whoInvite != null)
                //{
                //    pessoa.IdParent = whoInvite.intIdDad;
                //    //tlvz migrar tudo pra nova tabela
                //    try
                //    {
                //        var parent = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == pessoa.IdParent);
                //        var phoneParent = ctx.tblPersonsPhones.FirstOrDefault(x => x.bitPhoneClube == true && x.bitAtivo == true && x.intIdPerson == parent.intIdPerson);
                //        pessoa.NameParent = parent.txtName;
                //        pessoa.PhoneNumberParent = Convert.ToInt64(string.Format("{0}{1}", phoneParent.intDDD.ToString(), phoneParent.intPhone.ToString()));  
                //    }
                //    catch (Exception e) {

                //    }
                //}
                //else
                //{
                //    var parent = GetCustomerParent(pessoa);
                //    pessoa.NameParent = parent.Name;
                //    pessoa.PhoneNumberParent = Convert.ToInt64(parent.Phones.FirstOrDefault().DDD + parent.Phones.FirstOrDefault().Number);
                //}

                pessoa.Adresses = new List<Adress>();
                var addresses = ctx.tblPersonsAddresses.Where(a => a.intIdPerson == tblpessoa.intIdPerson).ToList();

                foreach (var address in addresses)
                {
                    pessoa.Adresses.Add(new Adress
                    {
                        Cep = address.txtCep,
                        City = address.txtCity,
                        Country = address.txtCountry,
                        State = address.txtState,
                        Street = address.txtStreet,
                        StreetNumber = address.intStreetNumber.ToString(),
                        Complement = address.txtComplement,
                        Neighborhood = address.txtNeighborhood

                    });
                }



                pessoa.Photos = new List<Photo>();
                var images = ctx.tblPersonsImages.Where(a => a.intIdPerson == tblpessoa.intIdPerson)
                    .Select(y => new Photo { Id = y.intId, Name = y.txtImage, Tipo = y.intTipo })
                    .ToList();
                pessoa.Photos = images;


                pessoa.Phones = new List<Phone>();
                var phones = ctx.tblPersonsPhones.Where(a => a.intIdPerson == tblpessoa.intIdPerson).ToList();
                foreach (var phone in phones)
                {
                    var tel = new Phone
                    {
                        DDD = phone.intDDD.ToString(),
                        Number = phone.intPhone.ToString(),
                        IsFoneclube = phone.bitPhoneClube,
                        Id = phone.intId,
                        IdPlanOption = Convert.ToInt32(phone.intIdPlan),
                        NickName = phone.txtNickname,
                        Portability = phone.bitPortability,
                        IdOperator = Convert.ToInt32(phone.intIdOperator),
                        LinhaAtiva = phone.bitAtivo,
                        Status = phone.intIdStatus,
                        AmmountPrecoVip = phone.intAmmoutPrecoVip,
                        PrecoVipStatus = phone.bitPrecoVip
                    };

                    try
                    {
                        var claro = 1;

                        if (tel.IdOperator == claro)
                        {
                            tel.StatusClaro = new ClaroAccess().GetLineDetails(tel.DDD + tel.Number);
                        }
                        else
                        {
                            tel.StatusVivo = new VivoAccess().GetStatusLine(tel.DDD + tel.Number);
                        }
                    }
                    catch (Exception e)
                    {

                    }


                    pessoa.Phones.Add(tel);
                    //todocode gere status claro
                }

                var discount = ctx.tblDiscountPrice.FirstOrDefault(d => d.intIdPerson == tblpessoa.intIdPerson);
                if (!bool.Equals(discount, null))
                {
                    pessoa.SinglePrice = Convert.ToInt64(discount.intAmmount);
                    pessoa.DescriptionSinglePrice = discount.txtDescription;

                }

                return pessoa;
            }

        }
        public List<PhoneLines> GetCustomersPhoneByCPF(string documnetNumber)
        {
            List<PhoneLines> phoneLines = new List<PhoneLines>();
            using (var ctx = new FoneClubeContext())
            {
                var personId = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == documnetNumber);
                var lstPhones = ctx.GetPhoneLinesByPerson(personId.intIdPerson);

                foreach (var res in lstPhones)
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
                        Delete = res.DeletedLine
                    });
                }
            }

            return phoneLines;
        }

        public List<Person> GetPersons()
        {
            var persons = new List<Person>();

            using (var ctx = new FoneClubeContext())
            {
                //definir role de cliente normal, e regra final para retorno clientes, por enquanto deixei temp
                var customers = ctx.tblPersons.Where(p => p.intIdRole == null || p.intIdRole == 0).Distinct().ToList();
                var filhosSemPai = new ComissionAccess().GetFilhosSemPai();

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["ExecutandoLocalHost"]))
                    customers = ctx.tblPersons.Where(p => p.txtDocumentNumber == "43388638209" || p.txtDocumentNumber == "10667103767" || p.txtDocumentNumber == "90647491753" || p.txtDocumentNumber == "84288060710" || p.intIdPerson == 4184 || p.intIdPerson == 1 || p.intIdPerson == 2).Distinct().ToList();

                foreach (var customer in customers)
                {
                    var pessoa = GetPerson(customer.txtDocumentNumber);
                    var filhosSemPaiObject = filhosSemPai.FirstOrDefault(c => c.intIdPerson == pessoa.Id);

                    pessoa.Orphan = !bool.Equals(filhosSemPaiObject, null);


                    persons.Add(pessoa);
                }
            }

            return persons;
        }

        public List<Person> GetIntlPersons()
        {
            var persons = new List<Person>();
            using (var ctx = new FoneClubeContext())
            {
                var ctxPerson = ctx.tblPersons.Where(x => x.bitIntl == true).ToList();
                if (ctxPerson != null && ctxPerson.Count > 0)
                {
                    persons.Add(new Person()
                    {
                        Name = "All",
                        Id = -1,
                        Email = "",
                        DocumentNumber = ""
                    });

                    foreach (var person in ctxPerson)
                    {
                        persons.Add(new Person()
                        {
                            Name = person.txtName,
                            Id = person.intIdPerson,
                            Email = person.txtEmail,
                            DocumentNumber = person.txtDocumentNumber
                        });
                    }
                }
            }
            return persons;
        }

        public IntlUserDashboard GetIntlUserData(GetIntlDataReq request)
        {
            IntlUserDashboard intlUser = new IntlUserDashboard();
            int id = Convert.ToInt32(request.Id);
            int operation = request.Operation == "TODOS" ? 0 : request.Operation == "ATIVAÇÃO" ? 1 : request.Operation == "RECARGA" ? 2 : 3;

            try
            {
                request.StartDate = request.StartDate.Length > 20 ? request.StartDate.Substring(0, 23) : request.StartDate;
                request.EndDate = request.EndDate.Length > 20 ? request.EndDate.Substring(0, 23) : request.EndDate;
            }
            catch (Exception ex) { }
            using (var ctx = new FoneClubeContext())
            {
                var persons = ctx.tblPersons.ToList();
                var ctxPerson = ctx.tblPersons.Where(x => x.intIdPerson == id).FirstOrDefault();
                if (ctxPerson != null)
                {
                    intlUser.IdPerson = id;
                    intlUser.PersonInfo = new IntlPerson()
                    {
                        Email = ctxPerson.txtEmail,
                        Name = ctxPerson.txtName
                    };
                }
                else
                {
                    intlUser.IdPerson = id;
                    intlUser.PersonInfo = new IntlPerson()
                    {
                        Email = "",
                        Name = "All"
                    };
                }

                #region Today's Count
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                var listToday = ctx.GetFacilSaleStats(1).ToList();
                var listMonth = ctx.GetFacilSaleStats(2).ToList();
                var listOverall = ctx.GetFacilSaleStats(3).ToList();
                var listFilter = ctx.GetFacilSaleStatsByFilter(request.Id, request.StartDate, request.EndDate, request.Operation, request.Choice).ToList();

                if (listToday != null && listToday.Count > 0)
                {
                    sb.Append("Sales : <strong>Today - " + DateTime.Now.ToString("dd/MMM").ToUpper() + "</strong><br/><br/>");

                    if (id == -1)
                    {
                        var results = listToday.GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.Append("Nome: " + user.Name);
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.Append(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.Append(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.AppendLine("<br/><br/><div class='newline'></div>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.TodaySale = sb.ToString();
                        }
                    }
                    else
                    {
                        var results = listToday.Where(x => x.Name == ctxPerson.txtName).GroupBy(
                                p => p.Name).Select(grp => new
                                {
                                    Name = grp.Key,
                                    Plans = grp.ToList()
                                });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.Append(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.Append(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.Append("<br/>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.TodaySale = sb.ToString();
                        }
                    }
                }
                #endregion

                #region Month's Count
                sb.Clear();
                if (listMonth != null && listMonth.Count > 0)
                {
                    sb.AppendLine("Sales : <strong>" + DateTime.Now.ToString("MMM-yyyy").ToUpper() + "</strong> <br/><br/>");
                    if (id == -1)
                    {
                        var results = listMonth.GroupBy(
                        p => p.Name).Select(grp => new
                        {
                            Name = grp.Key,
                            Plans = grp.ToList()
                        });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.Append("Nome: " + user.Name);
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} :   {1},    Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.AppendLine("<br/><br/><div class='newline'></div>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.CurrentMonthSale = sb.ToString();
                        }
                    }
                    else
                    {
                        var results = listMonth.Where(x => x.Name == ctxPerson.txtName).GroupBy(
                           p => p.Name).Select(grp => new
                           {
                               Name = grp.Key,
                               Plans = grp.ToList()
                           });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} :   {1},    Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.Append("<br/>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.CurrentMonthSale = sb.ToString();
                        }
                    }
                }
                #endregion

                #region Overall Count
                sb.Clear();
                if (listOverall != null && listOverall.Count > 0)
                {
                    sb.AppendLine("Sales : <strong>Overall</strong><br/><br/>");
                    if (id == -1)
                    {
                        var results = listOverall.GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.Append("Nome: " + user.Name);
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.AppendLine("<br/><br/><div class='newline'></div>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.OverallSale = sb.ToString();
                        }
                    }
                    else
                    {
                        var results = listOverall.Where(x => x.Name == ctxPerson.txtName).GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.Append("<br/>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.OverallSale = sb.ToString();
                        }
                    }
                }
                sb.Clear();
                #endregion

                #region Filtered Count
                sb.Clear();
                if (listFilter != null && listFilter.Count > 0)
                {
                    sb.AppendLine("Sales : <strong>Filter</strong><br/><br/>");
                    if (id == -1)
                    {
                        var results = listFilter.GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                sb.Append("Nome: " + user.Name);
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.AppendLine("<br/><br/><div class='newline'></div>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.FilteredSale = sb.ToString();
                        }
                    }
                    else
                    {
                        var results = listFilter.Where(x => x.Name == ctxPerson.txtName).GroupBy(
                            p => p.Name).Select(grp => new
                            {
                                Name = grp.Key,
                                Plans = grp.ToList()
                            });

                        if (results != null)
                        {
                            foreach (var user in results)
                            {
                                if (user.Plans != null && user.Plans.Count > 0)
                                {
                                    foreach (var plan in user.Plans)
                                    {
                                        sb.AppendLine(string.Format("<div class='fontsmall'>{0} : {1},  Amount : U$ {2} </div>", plan.Plan, plan.TotalCount, plan.TotalAmount));
                                    }
                                    sb.AppendLine(string.Format("<div class='totalPlans'>Planos: <strong>{0}</strong>,  Amount: U$ <strong>{1}</strong></div>", user.Plans.Sum(x => x.TotalCount), user.Plans.Sum(x => x.TotalAmount)));
                                    sb.Append("<br/>");
                                }
                                else
                                    sb.AppendLine("<strong>No plans sold</strong>");
                            }
                            intlUser.FilteredSale = sb.ToString();
                        }
                    }
                }
                sb.Clear();
                #endregion

                List<tblInternationalUserPurchases> tblPhistory = new List<tblInternationalUserPurchases>();

                DateTime startDate = DateTime.Now.AddDays(-1);
                try
                {
                    startDate = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    startDate = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                DateTime endDate = DateTime.Now;
                try
                {
                    endDate = DateTime.ParseExact(request.EndDate, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    endDate = DateTime.ParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                }

                if (id == -1)
                {
                    if (operation == 0)
                        tblPhistory = ctx.tblInternationalUserPurchases.Where(x => (!x.bitTest.HasValue || !x.bitTest.Value) && x.dteDeducted >= startDate && x.dteDeducted <= endDate).ToList();
                    else
                        tblPhistory = ctx.tblInternationalUserPurchases.Where(x => (!x.bitTest.HasValue || !x.bitTest.Value) && x.intPurchaseType == operation && x.dteDeducted >= startDate && x.dteDeducted <= endDate).ToList();
                }
                else
                {
                    if (operation == 0)
                        tblPhistory = ctx.tblInternationalUserPurchases.Where(x => x.intIdPerson == id && (!x.bitTest.HasValue || !x.bitTest.Value) && x.dteDeducted >= startDate && x.dteDeducted <= endDate).ToList();
                    else
                        tblPhistory = ctx.tblInternationalUserPurchases.Where(x => x.intIdPerson == id && (!x.bitTest.HasValue || !x.bitTest.Value) && x.intPurchaseType == operation && x.dteDeducted >= startDate && x.dteDeducted <= endDate).ToList();
                }
                if (tblPhistory != null && tblPhistory.Count() > 0)
                {
                    intlUser.Purchases = new List<Purchase>();
                    intlUser.TotalPurchaseAmount = Convert.ToString(Convert.ToDecimal(tblPhistory.Sum(y => y.intAmountDeducted), CultureInfo.InvariantCulture));
                    intlUser.TotalPurchases = Convert.ToString(tblPhistory.Count());

                    foreach (var hist in tblPhistory)
                    {
                        var person = persons.Where(x => x.intIdPerson == hist.intIdPerson).FirstOrDefault().txtName;
                        intlUser.Purchases.Add(new Purchase()
                        {
                            Id = hist.intId,
                            Amount = hist.intAmountDeducted,
                            Category = hist.intPurchaseType == 1 ? "Activation" : "Topup",
                            Date = hist.dteDeducted,
                            Line = hist.txtPhone,
                            Plan = hist.txtPlan,
                            ClientName = person,
                            IsRefund = hist.bitRefund.HasValue ? hist.bitRefund.Value : false,
                            Comment = hist.txtComments,
                            ICCID = hist.txtICCID,
                            ContelPrice = hist.intContelPrice,
                        });
                    }
                }

                List<tblInternationalDeposits> tblDeposits = new List<tblInternationalDeposits>();
                if (id == -1)
                    tblDeposits = ctx.tblInternationalDeposits.OrderByDescending(y => y.dteDateAdded).ToList();
                else
                    tblDeposits = ctx.tblInternationalDeposits.Where(x => x.intIdPerson == id).OrderByDescending(y => y.dteDateAdded).ToList();

                if (tblDeposits != null && tblDeposits.Count > 0)
                {
                    intlUser.TotalDeposits = Convert.ToString(tblDeposits.Sum(y => y.intFinalValue));

                    intlUser.Deposits = new List<Deposits>();
                    foreach (var hist in tblDeposits)
                    {
                        var person = persons.Where(x => x.intIdPerson == hist.intIdPerson).FirstOrDefault().txtName;
                        intlUser.Deposits.Add(new Deposits()
                        {
                            USDAmount = hist.intUSDAmount,
                            Comment = hist.txtComment,
                            HandlingCharge = hist.txtHandlingCharges,
                            IsRefund = hist.bitRefund.HasValue && hist.bitRefund.Value ? "Y" : "N",
                            FinalAmount = hist.intFinalValue.HasValue ? hist.intFinalValue.Value : 0,
                            Source = hist.intIdPaymentType,
                            Date = hist.dteDateAdded,
                            ClientName = person
                        });
                    }
                }

                if (ctx.tblInternationalUserBalance.Any(x => x.intIdPerson == id))
                    intlUser.CurrentBalance = Convert.ToString(Convert.ToDecimal(ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == id).intAmountBalance, CultureInfo.InvariantCulture));
                else if (id == -1)
                    intlUser.CurrentBalance = Convert.ToString(Convert.ToDecimal(ctx.tblInternationalUserBalance.ToList().Sum(x => x.intAmountBalance), CultureInfo.InvariantCulture));

                if (id == -1)
                    intlUser.DebitoSaldo = ctx.tblInternationalUserPurchases.Where(x => x.bitTest.HasValue == false || x.bitTest.Value == false && x.dteDeducted >= startDate && x.dteDeducted <= endDate).Sum(x => x.intContelPrice);
                else
                    intlUser.DebitoSaldo = ctx.tblInternationalUserPurchases.Where(x => x.bitTest.HasValue == false || x.bitTest.Value == false && x.intIdPerson == id && x.dteDeducted >= startDate && x.dteDeducted <= endDate).Sum(x => x.intContelPrice);
            }
            return intlUser;
        }

        public string RegisterInternationCustomer(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var isPersonExists = ctx.tblPersons.Any(x => x.txtDocumentNumber == person.DocumentNumber);
                    if (isPersonExists)
                        return "CPF already exists";

                    var jisPersonExists = ctx.tblPersons.Any(x => x.txtEmail == person.Email);
                    if (jisPersonExists)
                        return "Email already exists";

                    var hashedPassword = new Security().EncryptPassword(person.Password);

                    ctx.tblPersons.Add(new tblPersons()
                    {
                        txtName = person.Name,
                        txtDefaultWAPhones = person.DefaultWAPhones,
                        txtEmail = person.Email,
                        txtPassword = hashedPassword.Password,
                        txtDocumentNumber = person.DocumentNumber,
                        bitIntl = true,
                        dteRegister = DateTime.Now
                    });
                    ctx.SaveChanges();

                    return "User registered successfully";
                }
            }
            catch (Exception erro)
            {
                return "Error: " + erro.ToString();
            }
        }

        public List<Person> GetAllPersons(bool minimal)
        {
            //colocar proc para consultas simples cruas
            // atualizar abordagem de last charging para procpra ganhar melhoria

            var persons = new List<Person>();

            using (var ctx = new FoneClubeContext())
            {
                List<Person> customers;
                var userSettings = ctx.tblUserSettings.ToList();
                var contelLinesList = ctx.tblContelLinhasList.ToList();

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["ExecutandoLocalHost"]))
                    customers = ctx.tblPersons.Where(p => p.txtDocumentNumber == "43388638209" || p.txtDocumentNumber == "90647491753" || p.txtDocumentNumber == "84288060710" || p.intIdPerson == 4184 || p.intIdPerson == 1)
                    .Distinct().Select(p => new Person
                    {
                        Id = p.intIdPerson,
                        Email = p.txtEmail,
                        Name = p.txtName,
                        NickName = p.txtNickName,
                        Born = p.dteBorn.ToString(),
                        DocumentNumber = p.txtDocumentNumber,
                        Gender = p.intGender.Value,
                        IdRole = p.intIdRole.Value,
                        Register = p.dteRegister,
                        IdPagarme = p.intIdPagarme,
                        SoftDelete = p.bitDelete,
                        Desativo = p.bitDesativoManual,
                        DefaultPaymentDay = p.intDftBillPaymentDay,
                        DefaultVerificar = p.intDftVerificar,
                        DefaultWAPhones = p.txtDefaultWAPhones,
                        Use2Prices = userSettings.Any(x => x.intPerson == p.intIdPerson) ? userSettings.Where(x => x.intPerson == p.intIdPerson).FirstOrDefault().bitUse2Prices : false
                    }).ToList();
                else
                {
                    var todosClientes = ctx.GetTodosClientes().ToList();

                    customers = todosClientes.Select(p => new Person
                    {
                        Id = p.intIdPerson,
                        Email = p.txtEmail,
                        Name = p.txtName,
                        NickName = p.txtNickName,
                        DocumentNumber = p.txtDocumentNumber,
                        //IdRole = p.intIdRole,
                        Register = p.dteRegister,
                        IdPagarme = p.intIdPagarme,
                        SoftDelete = p.bitDelete,
                        Desativo = p.bitDesativoManual,
                        DefaultPaymentDay = p.intDftBillPaymentDay,
                        DefaultVerificar = p.intDftVerificar,
                        DefaultWAPhones = p.txtDefaultWAPhones,
                        Use2Prices = userSettings.Any(x => x.intPerson == p.intIdPerson) ? userSettings.Where(x => x.intPerson == p.intIdPerson).FirstOrDefault().bitUse2Prices : false
                    }).ToList();
                }


                persons.AddRange(customers);


                var pendingFlagInteraction = ctx.GetCustomersPendingFlags().ToList();
                var idCustomers = customers.Select(c => c.Id).ToList();

                foreach (var person in persons)
                {
                    var pendingInteraction = pendingFlagInteraction.FirstOrDefault(p => p.intIdPerson == person.Id);

                    if (pendingInteraction != null)
                        person.PendingFlagInteraction = pendingInteraction.hasPendingPersonFlag > 0 || pendingInteraction.hasPendingPhoneFlag > 0;
                }

                var tblPhones = ctx.tblPersonsPhones.Where(a => idCustomers.Contains(a.intIdPerson.Value) & a.bitPhoneClube.HasValue && !a.bitPhoneClube.Value)
                        .Select(p => new
                        {
                            CustomerId = p.intIdPerson.Value,
                            Phones = new Phone
                            {
                                DDD = p.intDDD.ToString(),
                                Number = p.intPhone.ToString(),
                                IsFoneclube = p.bitPhoneClube,
                                Id = p.intId,
                                IdPlanOption = p.intIdPlan.Value,
                                NickName = p.txtNickname,
                                Portability = p.bitPortability,
                                LinhaAtiva = p.bitAtivo,
                                Status = p.intIdStatus,
                                AmmountPrecoVip = p.intAmmoutPrecoVip,
                                PrecoVipStatus = p.bitPrecoVip,
                                Delete = p.bitDelete,
                                CountryCode = p.intCountryCode.HasValue ? p.intCountryCode.Value.ToString() : "55",
                                ICCID = p.txtICCID,
                                PortNumber = p.txtPortNumber
                            }
                        }).ToList();



                if (!minimal)
                {
                    //var clients = new ClientWhatsappAccess().GetAllClients();
                    //var unreadMessages = ctx.tblWhatsappMessages.Where(x => x.txtType == "from_client" && !x.bitRead).Select(x => x.intIdClient).ToList();
                    //if (clients.Success)
                    //{
                    //    customers.ForEach(x =>
                    //    {
                    //        x.Phones = new List<Phone>();
                    //        x.Phones.Add(tblPhones.Where(a => a.CustomerId == x.Id).Select(a => a.Phones).FirstOrDefault());
                    //        var actualPhone = x.Phones.FirstOrDefault();
                    //        if (actualPhone != null)
                    //        {
                    //            string customerPhone = $"{actualPhone.DDD}{actualPhone.Number}";
                    //            x.WClient = new WhatsappClient { PhoneNumber = customerPhone };
                    //            var matchedClient = clients.Data.Data.FirstOrDefault(y => y.Phone.Contains("55" + customerPhone) || y.Phone.Contains(customerPhone));
                    //            if (matchedClient != null)
                    //            {
                    //                x.WClient.IsRegisteredWithChat2Desk = true;
                    //                x.WClient.ProfilePicUrl = matchedClient.Avatar;
                    //                x.WClient.ClientId = matchedClient.Id;
                    //                x.WClient.UnreadMessages = unreadMessages.Where(y => y == matchedClient.Id).Count();
                    //            }
                    //        }
                    //    });
                    //}

                    var planOptions = ctx.tblPlansOptions.ToList();

                    var tblParents = ctx.tblPersonsParents.Where(p => idCustomers.Contains(p.intIdSon.Value))
                        .Select(p => new Person
                        {
                            Id = p.intIdSon.Value,
                            NameParent = p.txtNameParent,
                            PhoneDDDParent = p.intDDDParent.ToString(),
                            PhoneNumberParent = p.intPhoneParent.ToString(),
                            IdParent = p.intIdParent
                        }).ToList();

                    var tblAdress = ctx.tblPersonsAddresses.Where(a => idCustomers.Contains(a.intIdPerson.Value))
                        .Select(a => new
                        {
                            CustomerId = a.intIdPerson.Value,
                            Adress = new Adress
                            {
                                Cep = a.txtCep,
                                City = a.txtCity,
                                Country = a.txtCountry,
                                State = a.txtState,
                                Street = a.txtStreet,
                                StreetNumber = a.intStreetNumber.ToString(),
                                Complement = a.txtComplement,
                                Neighborhood = a.txtNeighborhood
                            }
                        }).ToList();

                    tblPhones = ctx.tblPersonsPhones.Where(a => idCustomers.Contains(a.intIdPerson.Value))
                        .Select(p => new
                        {
                            CustomerId = p.intIdPerson.Value,
                            Phones = new Phone
                            {
                                DDD = p.intDDD.ToString(),
                                Number = p.intPhone.ToString(),
                                IsFoneclube = p.bitPhoneClube,
                                Id = p.intId,
                                IdPlanOption = p.intIdPlan.Value,
                                NickName = p.txtNickname,
                                Portability = p.bitPortability,
                                LinhaAtiva = p.bitAtivo,
                                Status = p.intIdStatus,
                                AmmountPrecoVip = p.intAmmoutPrecoVip,
                                PrecoVipStatus = p.bitPrecoVip,
                                Delete = p.bitDelete,
                                CountryCode = p.intCountryCode.HasValue ? p.intCountryCode.Value.ToString() : "55",
                                ICCID = p.txtICCID,
                                IdOperator = p.intIdOperator.HasValue ? p.intIdOperator.Value : -1
                            }
                        }).ToList();

                    var tblDiscounts = ctx.tblDiscountPrice.Where(d => idCustomers.Contains(d.intIdPerson.Value))
                        .Select(d => new Person
                        {
                            Id = d.intIdPerson.Value,
                            SinglePrice = d.intAmmount.Value,
                            DescriptionSinglePrice = d.txtDescription
                        });

                    var tblChargings = ctx.tblChargingHistory.OrderByDescending(x => x.dteCreate).Select(c => new
                    {
                        CustomerId = c.intIdCustomer,
                        LastPayment = c.dteCreate
                    }).ToList();

                    var filhosSemPai = new ComissionAccess().GetFilhosSemPai();
                    var links = ctx.tblPersosAffiliateLinks.ToList();
                    foreach (var customer in customers)
                    {
                        var parent = tblParents.FirstOrDefault(x => x.Id == customer.Id);
                        var link = links.FirstOrDefault(p => p.intIdPerson == customer.Id);


                        if (!bool.Equals(parent, null))
                        {
                            customer.IdParent = parent.IdParent;
                            customer.NameParent = parent.NameParent;
                            customer.PhoneDDDParent = parent.PhoneDDDParent;
                            customer.PhoneNumberParent = parent.PhoneNumberParent;

                            try
                            {
                                customer.OriginalAffiliateLink = link.txtOriginalLink;
                                customer.AffiliateLink = link.txtBlinkLink;
                            }
                            catch (Exception) { }


                            customer.Pai = new Pai
                            {
                                Id = parent.IdParent,
                                Name = parent.NameParent
                            };
                            try
                            {
                                var pai = customers.FirstOrDefault(p => p.Id == parent.IdParent);

                                if (pai != null)
                                {
                                    customer.Pai.Name = pai.Name;
                                    customer.NameParent = pai.Name;
                                }
                            }
                            catch (Exception) { }


                        }

                        var personParent = tblChargings.FirstOrDefault(x => x.CustomerId == customer.Id);
                        if (personParent != null)
                            customer.LastChargeDate = personParent.LastPayment;

                        customer.Adresses = new List<Adress>();
                        customer.Adresses.AddRange(tblAdress.Where(a => a.CustomerId == customer.Id).Select(a => a.Adress).ToList());

                        customer.Phones = new List<Phone>();
                        customer.Phones.AddRange(tblPhones.Where(a => a.CustomerId == customer.Id).Select(a => a.Phones).ToList());

                        foreach (var phone in customer.Phones)
                            phone.IdOperator = planOptions.FirstOrDefault(o => o.intIdPlan == phone.IdPlanOption) == null ? -1 : planOptions.FirstOrDefault(o => o.intIdPlan == phone.IdPlanOption).intIdOperator;

                        customer.Phones = new PhoneAccess().GetGenericPhoneFlags(customer.Phones, ctx);

                        var discount = tblDiscounts.FirstOrDefault(d => d.Id == customer.Id);
                        if (!bool.Equals(discount, null))
                        {
                            customer.SinglePrice = discount.SinglePrice;
                            customer.DescriptionSinglePrice = discount.DescriptionSinglePrice;
                        }

                        customer.Orphan = !bool.Equals(filhosSemPai.FirstOrDefault(c => c.intIdPerson == customer.Id), null);

                        var vipSum = ctx.GetSumpVIPByCustomer(customer.Id).FirstOrDefault();
                        if (vipSum.HasValue)
                        {
                            customer.VIPSum = "R$" + vipSum.Value;
                        }
                        else
                        {
                            customer.VIPSum = "OFF";
                        }

                        if (customer.Phones != null)
                        {
                            var foneclubePhones = customer.Phones.Where(x => x.IdOperator == 4 && x.LinhaAtiva.HasValue && x.LinhaAtiva.Value && x.IsFoneclube.HasValue && x.IsFoneclube.Value).ToList();
                            if (foneclubePhones != null && foneclubePhones.Count() > 0)
                            {
                                var blockedphones = (from ctl in contelLinesList
                                                     join ph in foneclubePhones
                                                              on ctl.txtlinha equals string.Concat(ph.DDD, ph.Number)
                                                     where !string.IsNullOrEmpty(ctl.txtbloqueada) && ctl.txtbloqueada != "NÃO"
                                                     select ph).ToList();
                                var cancelled = (from ctl in contelLinesList
                                                 join ph in foneclubePhones
                                                          on ctl.txtlinha equals string.Concat(ph.DDD, ph.Number)
                                                 where !string.IsNullOrEmpty(ctl.txtbloqueada) && ctl.txtstatus == "CANCELADO"
                                                 select ph).ToList();

                                if (blockedphones != null && blockedphones.Count() > 0)
                                {
                                    customer.LineStatus = "B";
                                }
                                else if (cancelled != null && cancelled.Count() > 0)
                                {
                                    customer.LineStatus = "C";
                                }
                                else
                                {
                                    customer.LineStatus = "A";
                                }
                            }
                            else
                            {
                                customer.LineStatus = "O";
                            }
                        }
                        else
                        {
                            customer.LineStatus = "O";
                        }
                    }
                }
            }

            return persons;
        }

        public bool IsNewDocument(string document)
        {
            using (var ctx = new FoneClubeContext())
            {
                return !ctx.tblPersons.Any(p => p.txtDocumentNumber.Trim() == document.Trim());
            }
        }

        public HttpStatusCode UpdatePerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                    if (!bool.Equals(tblPerson, null))
                    {

                        if (tblPerson.txtDocumentNumber.Trim() != person.DocumentNumber.Trim())
                            UpdateDocumentNumber(person, tblPerson, ctx);

                        if (ctx.tblPersons.Any(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim() && p.intIdPerson != person.Id))
                            throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ExistentDocument));

                        if (!UpdatePersonData(person, tblPerson))
                            throw new HttpResponseException(
                             new Utils().GetErrorPostMessage("Não foi possível update em dados básicos de pessoa."));


                        if (!string.IsNullOrEmpty(person.PhoneDDDParent) && !string.IsNullOrEmpty(person.PhoneNumberParent))
                            if (!SetCustomerParentPhone(person))
                                throw new HttpResponseException(
                                      new Utils().GetErrorPostMessage("Não foi possível update em quem convidou."));

                        //temporariamente desativado então fica opcional mesmo
                        //try
                        //{
                        //    if (!bool.Equals(person.Photos, null) && person.Photos.Any())
                        //        SavePersonImages(person, ctx);
                        //}
                        //catch (Exception e) {

                        //}


                        if (!bool.Equals(person.Images, null) && person.Images.Any())
                            if (!SavePersonImagesLegacy(person, ctx))
                                throw new HttpResponseException(
                                      new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ImagensError));

                        if (!UpdatePersonPhones(person, ctx))
                            throw new HttpResponseException(
                             new Utils().GetErrorPostMessage("Não foi possíve update em telefones"));

                        ctx.tblPersonsAddresses.RemoveRange(ctx.tblPersonsAddresses.Where(p => p.intIdPerson == person.Id).ToList());
                        if (!SavePersonAddresses(person, ctx))
                            throw new HttpResponseException(
                             new Utils().GetErrorPostMessage("Não foi possível update em endereço"));


                        if (!string.IsNullOrEmpty(person.SinglePrice.ToString()))
                            if (!InsertSinglePrice(person, ctx))
                                throw new HttpResponseException(
                                    new Utils().GetErrorPostMessage("Não foi possível update Single price"));


                        ctx.SaveChanges();


                        try
                        {
                            var name = GetName(person.Name);
                            var customer = new woocommerce.Customer();
                            var billing = new woocommerce.CustomerBilling();
                            customer.email = person.Email;
                            customer.first_name = name.Name;
                            customer.last_name = name.LastName;

                            var cpf = false;
                            cpf = person.DocumentNumber.Length <= 11;


                            if (cpf)
                                billing.cpf = person.DocumentNumber;
                            else
                                billing.cnpj = person.DocumentNumber;

                            billing.city = person.Adresses.FirstOrDefault().City;
                            billing.postcode = person.Adresses.FirstOrDefault().Cep;
                            billing.address_1 = person.Adresses.FirstOrDefault().Street + " " + person.Adresses.FirstOrDefault().StreetNumber;
                            billing.address_2 = person.Adresses.FirstOrDefault().Neighborhood + " " + person.Adresses.FirstOrDefault().Complement;
                            billing.state = person.Adresses.FirstOrDefault().State;
                            billing.phone = person.Phones.FirstOrDefault(f => f.IsFoneclube == true).DDD.ToString() + person.Phones.FirstOrDefault(f => f.IsFoneclube == true).Number;

                            customer.billing = billing;
                            var saveCustomer = new WooAPI().UpdateCustomer(Convert.ToInt32(tblPerson.intIdLoja), customer);
                        }
                        catch (Exception e)
                        {
                            var teste = e;
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(
                             new Utils().GetErrorPostMessage("Id de pessoa inexistente"));
                    }

                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                             new Utils().GetErrorPostMessage(e.InnerException.Message));

                }

            }

            return HttpStatusCode.OK;
        }

        private void UpdateDocumentNumber(Person person, tblPersons tblPerson, FoneClubeContext ctx)
        {
            tblPerson.txtDocumentNumber = person.DocumentNumber;
            ctx.SaveChanges();
        }


        public bool UpdatePersonPhones(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return UpdatePersonPhones(person, ctx);
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public bool UpdatePersonPhones(Person person, FoneClubeContext ctx)
        {
            try
            {
                if (!bool.Equals(person.Phones, null))
                {
                    foreach (var phone in person.Phones)
                    {
                        var newPhone = !ctx.tblPersonsPhones.Any(p => p.intId == phone.Id && p.intIdPerson == person.Id);
                        int? dddNumber;
                        long? phoneNumber;

                        try
                        {
                            dddNumber = Convert.ToInt32(phone.DDD);
                            phoneNumber = Convert.ToInt64(phone.Number);
                        }
                        catch (Exception e)
                        {
                            dddNumber = null;
                            phoneNumber = null;
                        }

                        if (newPhone)
                        {
                            var telefone = GetPhoneOwner(new Phone { DDD = phone.DDD.Trim(), Number = phone.Number.Trim() });
                            if (!bool.Equals(telefone.DocumentNumber, null))
                                throw new HttpResponseException(
                                new Utils().GetErrorPostMessage("Linha ativa em outra conta, use o Get Phone Number pra descobrir qual"));

                            var planOption = ctx.tblPlansOptions.Where(p => p.intIdPlan == phone.IdPlanOption).FirstOrDefault();

                            if (planOption != null)
                            {
                                phone.IdOperator = planOption.intIdOperator;
                            }

                            ctx.tblPersonsPhones.Add(new tblPersonsPhones
                            {
                                intIdPerson = person.Id,
                                intDDD = dddNumber,
                                intPhone = phoneNumber,
                                intIdOperator = phone.IdOperator,
                                bitPhoneClube = phone.IsFoneclube,
                                bitPortability = phone.Portability,
                                intIdPlan = phone.IdPlanOption,
                                txtNickname = phone.NickName,
                                bitAtivo = phone.LinhaAtiva,
                                intIdStatus = phone.Status,
                                bitPrecoVip = phone.PrecoVipStatus,
                                intAmmoutPrecoVip = phone.AmmountPrecoVip,
                                dteEntradaLinha = DateTime.Now,
                                txtICCID = phone.ICCID,
                                txtPortNumber = phone.PortNumber
                            });

                            var phoneAccess = new PhoneAccess();
                            var propertyPhone = new Phone
                            {
                                DDD = phone.DDD,
                                Number = phone.Number,
                                IdPlanOption = phone.IdPlanOption
                            };

                            if (Convert.ToBoolean(phone.IsFoneclube))
                            {
                                var statusPhone = phoneAccess.GetStatusLinha(person.Id, propertyPhone);

                                if (Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Ativa)
                                    phoneAccess.InsertHistoricoAtivarLinha(person.Id, propertyPhone);
                                else if (!Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Desativa)
                                    phoneAccess.InsertHistoricoDesativarLinha(person.Id, propertyPhone);
                            }
                        }
                        else
                        {
                            var personPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == phone.Id && p.intIdPerson == person.Id);

                            if (Convert.ToBoolean(phone.Delete))
                            {
                                //Se era bitDelete pra soft não tem motivo de fazer remove
                                //ctx.tblPersonsPhones.Remove(personPhone);
                            }
                            else
                            {
                                personPhone.intIdPerson = person.Id;
                                personPhone.intDDD = dddNumber;
                                personPhone.intPhone = phoneNumber;
                                personPhone.intIdOperator = phone.IdOperator;
                                personPhone.bitPhoneClube = phone.IsFoneclube;
                                personPhone.bitPortability = phone.Portability;
                                personPhone.intIdPlan = phone.IdPlanOption;
                                personPhone.txtNickname = phone.NickName;
                                personPhone.bitAtivo = phone.LinhaAtiva;
                                personPhone.intIdStatus = phone.Status;
                                personPhone.bitPrecoVip = phone.PrecoVipStatus;
                                personPhone.intAmmoutPrecoVip = phone.AmmountPrecoVip;
                                personPhone.txtICCID = phone.ICCID;
                                personPhone.txtPortNumber = phone.PortNumber;
                            }

                            var phoneAccess = new PhoneAccess();
                            var propertyPhone = new Phone
                            {
                                DDD = phone.DDD,
                                Number = phone.Number,
                                IdPlanOption = phone.IdPlanOption,
                                Id = phone.Id
                            };

                            if (Convert.ToBoolean(phone.IsFoneclube) && !Convert.ToBoolean(phone.Delete))
                            {
                                var statusPhone = phoneAccess.GetStatusLinha(person.Id, propertyPhone);

                                if (personPhone.intIdPlan != phone.IdPlanOption)
                                {
                                    phoneAccess.InsertHistoricoUpdateLinha(person.Id, propertyPhone);
                                }
                                else
                                {
                                    if (Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Ativa)
                                        phoneAccess.InsertHistoricoAtivarLinha(person.Id, propertyPhone);
                                    else if (!Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Desativa)
                                        phoneAccess.InsertHistoricoDesativarLinha(person.Id, propertyPhone);
                                }
                            }
                        }
                    }

                    ctx.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool CheckoutPersonLojaRegister(Order order)
        {
            using (var ctx = new FoneClubeContext())
            {
                var cliente = new Person();
                var customer = ctx.tblPersons.FirstOrDefault(p => p.intIdLoja == order.customer_id);

                if (customer == null)
                {
                    customer = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == order.billing.cpf);

                    try
                    {
                        customer.intIdLoja = order.customer_id;
                        ctx.SaveChanges();
                    }
                    catch (Exception) { }
                }

                cliente.Id = customer.intIdPerson;

                var isBoleto = order.payment_method_title.Contains("Boleto");

                var charging = new Charging();
                charging.AnoVingencia = DateTime.Now.Year.ToString();
                charging.MesVingencia = DateTime.Now.Month.ToString();
                charging.CollectorName = "woo";
                charging.Ammount = order.total.Replace(".", string.Empty);
                charging.Comment = order.payment_method_title;
                charging.CartHash = order.cart_hash;
                charging.IdLoja = order.id;
                charging.StatusDescription = order.status;
                charging.Payd = order.status == "completed";

                cliente.Charging = charging;


                var saveCharging = new ChargingAcess().SaveChargingHistoryLoja(cliente);
            }


            return true;
        }

        public bool CheckoutUpdatePersonLojaRegister(Order order)
        {
            using (var ctx = new FoneClubeContext())
            {
                var cliente = new Person();
                var customer = ctx.tblPersons.FirstOrDefault(p => p.intIdLoja == order.customer_id);


                if (customer == null)
                {
                    customer = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == order.billing.cpf);

                    try
                    {
                        customer.intIdLoja = order.customer_id;
                        ctx.SaveChanges();
                    }
                    catch (Exception) { }
                }

                cliente.Id = customer.intIdPerson;

                var isBoleto = order.payment_method_title.Contains("Boleto");

                var charging = new Charging();
                charging.AnoVingencia = DateTime.Now.Year.ToString();
                charging.MesVingencia = DateTime.Now.Month.ToString();
                charging.CollectorName = "woo";
                charging.Ammount = order.total.Replace(".", string.Empty);
                charging.Comment = order.payment_method_title;
                charging.CartHash = order.cart_hash;
                charging.IdLoja = order.id;
                charging.StatusDescription = order.status;
                charging.Payd = order.status == "completed";

                cliente.Charging = charging;


                var saveCharging = new ChargingAcess().UpdateChargingHistoryLoja(cliente, order.id);
            }


            return true;
        }

        public List<ChargeAndServiceOrderHistory> GetChargingServiceOrdersHistory(int personID)
        {
            using (var ctx = new FoneClubeContext())
            {
                List<tblChargingHistory> tblCharges = new List<tblChargingHistory>();
                var chargesAndServiceOrders = new List<ChargeAndServiceOrderHistory>();
                if (personID == 1)
                {
                    var currentYear = new DateTime(DateTime.Now.Year, 1, 1);
                    tblCharges = ctx.tblChargingHistory
                        .Where(c => c.intIdCustomer == personID && DbFunctions.TruncateTime(c.dteCreate) >= DbFunctions.TruncateTime(currentYear)).ToList().OrderByDescending(x => x.dteCreate).ToList();
                }
                else
                {
                    tblCharges = ctx.tblChargingHistory
                        .Where(c => c.intIdCustomer == personID).ToList().OrderByDescending(x => x.dteCreate).ToList();
                }
                var transactions = new TransactionAccess().GetLastTransactions(tblCharges);

                foreach (var charge in tblCharges)
                {
                    string paymentStatus = string.Empty;
                    string transactionLastUpdate = string.Empty;
                    string transactionComment = string.Empty;
                    var transaction = charge.intIdTransaction;
                    tblFoneclubePagarmeTransactions pagarmeTransaction = null;
                    if (transaction != null)
                    {
                        pagarmeTransaction = ctx.tblFoneclubePagarmeTransactions.FirstOrDefault(t => t.intIdTransaction == transaction);
                        if (pagarmeTransaction != null)
                        {
                            paymentStatus = pagarmeTransaction.txtOutdadetStatus;
                            transactionLastUpdate = Convert.ToDateTime(pagarmeTransaction.dteDate_updated).ToString("yyyy-MM-dd h:mm tt");
                            transactionComment = pagarmeTransaction.txtCard_last_digits;
                        }
                        else if (pagarmeTransaction == null && transaction.Value == 999999999)
                        {
                            paymentStatus = "Paid";
                            transactionLastUpdate = charge.dtePayment.ToString("yyyy-MM-dd h:mm tt");
                        }
                    }
                    var person = ctx.tblPersons.Where(x => x.intIdPerson == personID).FirstOrDefault();

                    var newCharge = new ChargeAndServiceOrderHistory
                    {
                        CreatedDate = charge.dteCreate.HasValue ? charge.dteCreate.Value : DateTime.MinValue,
                        IsCharge = true,
                        PersonId = charge.intIdCustomer.Value,
                        Charges = new Charging
                        {
                            Id = charge.intId,
                            CreationDate = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd h:mm tt"),
                            CreationDateFormatted = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd"),
                            CollectorName = charge.txtCollectorName,
                            Comment = Helper.ReplaceMessage(charge.txtComment, charge, person, pagarmeTransaction),
                            CommentEmail = Helper.ReplaceMessage(charge.txtCommentEmail, charge, person, pagarmeTransaction),
                            CommentBoleto = Helper.ReplaceMessage(charge.txtCommentBoleto, charge, person, pagarmeTransaction),
                            IdCollector = charge.intIdCollector,
                            PaymentType = charge.intIdPaymentType,
                            Ammount = charge.txtAmmountPayment,
                            Token = charge.txtTokenTransaction,
                            BoletoId = Convert.ToInt64(charge.intIdBoleto),
                            AcquireId = charge.txtAcquireId,
                            PaymentStatusDescription = paymentStatus,
                            TransactionLastUpdate = transactionLastUpdate,
                            TransactionId = charge.intIdTransaction,
                            TransactionComment = charge.intIdPaymentType == 1 ? transactionComment : charge.txtTransactionComment,
                            AnoVingencia = Convert.ToDateTime(charge.dteValidity).Year.ToString(),
                            MesVingencia = Convert.ToDateTime(charge.dteValidity).Month.ToString(),
                            Pago = charge.bitPago,
                            PixCode = charge.pixCode,
                            DueDate = charge.dteDueDate,
                            IsActive = charge.bitActive.HasValue ? charge.bitActive.Value : false,
                            TxtWAPhones = charge.txtWAPhones,
                            ChargingComment = charge.txtChargingComment,
                            SendWAText = charge.bitSendWAText,
                            SendMarketing1 = charge.bitSendMarketing1,
                            SendMarketing2 = charge.bitSendMarketing2,
                            BoletoUrl = charge.txtboletoUrl,
                            BoletoBarcode = charge.txtboletoBarcode,
                            Installments = charge.intInstallments.HasValue ? charge.intInstallments.Value : 0
                        }
                    };

                    if (Convert.ToBoolean(charge.bitPago))
                    {
                        newCharge.Charges.PaymentStatusDescription = "Paid";
                    }

                    try
                    {
                        if (charge.txtCollectorName == "woo")
                        {
                            int BOLETO = 2;
                            int CARTAO = 1;

                            var isBoletoLoja = newCharge.Charges.Comment.ToLower().Contains("boleto");
                            newCharge.Charges.PaymentType = isBoletoLoja ? BOLETO : CARTAO;
                        }
                    }
                    catch (Exception) { }


                    try
                    {
                        newCharge.Charges.BoletoExpires = transactions.FirstOrDefault(t => t.intIdTransaction == charge.intIdTransaction).dteBoleto_expiration_date;
                    }
                    catch (Exception) { }

                    chargesAndServiceOrders.Add(newCharge);
                }

                var orders = ctx.tblServiceOrders.Where(p => p.intIdPerson == personID).ToList();
                foreach (var order in orders)
                {
                    chargesAndServiceOrders.Add(new ChargeAndServiceOrderHistory
                    {
                        CreatedDate = order.dteRegister,
                        IsServiceOrder = true,
                        ServiceOrders = new ServiceOrder
                        {
                            RegisterDate = order.dteRegister,
                            Description = order.txtDescription,
                            PendingInteraction = Convert.ToBoolean(order.bitPendingInteraction),
                            AgentName = order.txtAgentName,
                            AgentId = Convert.ToInt32(order.intIdAgent)
                        }
                    });
                }
                //return chargesAndServiceOrders.OrderByDescending(x => x.CreatedDate).ToList();
                return chargesAndServiceOrders.ToList();
            }
        }

        public List<ChargeAndServiceOrderHistory> GetChargingServiceOrdersHistory(List<int> personIDs)
        {
            using (var ctx = new FoneClubeContext())
            {
                var chargesAndServiceOrders = new List<ChargeAndServiceOrderHistory>();
                var tblCharges = ctx.tblChargingHistory
                    .Where(c => c.intIdCustomer.HasValue && personIDs.Contains(c.intIdCustomer.Value)).ToList().OrderByDescending(x => x.dteCreate).ToList();

                var transactions = new TransactionAccess().GetLastTransactions(tblCharges);

                personIDs.ForEach(x =>
                {
                    var chargeHistory = tblCharges.Where(y => y.intIdCustomer == x).OrderByDescending(y => y.dteCreate)
                    .Select(charge => new ChargeAndServiceOrderHistory
                    {
                        PersonId = x,
                        CreatedDate = charge.dteCreate.HasValue ? charge.dteCreate.Value : DateTime.MinValue,
                        IsCharge = true,
                        Charges = new Charging
                        {
                            Id = charge.intId,
                            CreationDate = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd h:mm tt"),
                            CreationDateFormatted = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd"),
                            CollectorName = charge.txtCollectorName,
                            Comment = charge.txtComment,
                            CommentEmail = charge.txtCommentEmail,
                            CommentBoleto = charge.txtCommentBoleto,
                            IdCollector = charge.intIdCollector,
                            PaymentType = charge.intIdPaymentType,
                            Ammount = charge.txtAmmountPayment,
                            Token = charge.txtTokenTransaction,
                            BoletoId = Convert.ToInt64(charge.intIdBoleto),
                            AcquireId = charge.txtAcquireId,
                            TransactionId = charge.intIdTransaction,
                            AnoVingencia = Convert.ToDateTime(charge.dteValidity).Year.ToString(),
                            MesVingencia = Convert.ToDateTime(charge.dteValidity).Month.ToString(),
                            DueDate = charge.dteDueDate,
                            BoletoBarcode = charge.txtboletoBarcode,
                            BoletoUrl = charge.txtboletoUrl
                        }
                    }).FirstOrDefault();

                    chargesAndServiceOrders.Add(chargeHistory);
                });

                List<long> transactionIds = chargesAndServiceOrders.Where(x => x != null && x.Charges != null && x.Charges.TransactionId.HasValue).ToList().Select(x => x.Charges.TransactionId.Value).ToList();
                var pagarmeTransactions = ctx.tblFoneclubePagarmeTransactions.Where(t => t.intIdTransaction.HasValue && transactionIds.Contains(t.intIdTransaction.Value)).ToList();

                chargesAndServiceOrders.ForEach(x =>
                {
                    if (x != null && x.Charges != null && x.Charges.TransactionId.HasValue)
                    {
                        var pagarmeTrans = pagarmeTransactions.OrderByDescending(z => z.intIdTransaction.Value).FirstOrDefault(y => y.intIdTransaction == x.Charges.TransactionId);
                        if (pagarmeTrans != null)
                        {
                            x.Charges.PaymentStatusDescription = pagarmeTrans.txtOutdadetStatus;
                            x.Charges.TransactionLastUpdate = Convert.ToDateTime(pagarmeTrans.dteDate_updated).ToString("yyyy-MM-dd h:mm tt");
                        }

                        var transaction = transactions.FirstOrDefault(t => t.intIdTransaction == x.Charges.TransactionId);
                        if (transaction != null)
                        {
                            x.Charges.BoletoExpires = transaction.dteBoleto_expiration_date;
                        }
                    }
                });

                var orders = ctx.tblServiceOrders.Where(p => p.intIdPerson.HasValue && personIDs.Contains(p.intIdPerson.Value))
                    .Select(order => new ChargeAndServiceOrderHistory
                    {
                        PersonId = order.intIdPerson.Value,
                        CreatedDate = order.dteRegister,
                        IsServiceOrder = true,
                        ServiceOrders = new ServiceOrder
                        {
                            RegisterDate = order.dteRegister,
                            Description = order.txtDescription,
                            PendingInteraction = order.bitPendingInteraction ?? false,
                            AgentName = order.txtAgentName,
                            AgentId = order.intIdAgent ?? 0
                        }
                    }).ToList();

                chargesAndServiceOrders.AddRange(orders);
                return chargesAndServiceOrders;
            }
        }
        public List<ChargeAndServiceOrderHistory> GetChargingAndServiceOrderHistoryDocument(string document)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var pessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == document);
                    if (pessoa == null)
                        return new List<ChargeAndServiceOrderHistory>();
                    else
                        return GetChargingServiceOrdersHistory(pessoa.intIdPerson);

                }
            }
            catch (Exception e)
            {
                return new List<ChargeAndServiceOrderHistory>();
            }

        }

        private bool UpdatePersonData(Person person, tblPersons tblPerson)
        {
            try
            {
                if (!string.IsNullOrEmpty(person.DocumentNumber))
                    tblPerson.txtDocumentNumber = person.DocumentNumber.Trim();

                if (!string.IsNullOrEmpty(person.Name))
                    tblPerson.txtName = person.Name.Trim();

                if (!string.IsNullOrEmpty(person.NickName))
                    tblPerson.txtNickName = person.NickName;

                if (!string.IsNullOrEmpty(person.IdPagarme.ToString()))
                    tblPerson.intIdPagarme = person.IdPagarme;

                if (!string.IsNullOrEmpty(person.Email))
                    tblPerson.txtEmail = person.Email;

                if (!string.IsNullOrEmpty(person.Born))
                    tblPerson.dteBorn = DateTime.Parse(person.Born);

                if (!bool.Equals(person.Gender, null))
                    tblPerson.intGender = person.Gender;

                if (!bool.Equals(person.IdRole, null))
                    tblPerson.intIdRole = person.IdRole;




                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public HttpStatusCode InsertPerson(Person person)
        {

            using (var ctx = new FoneClubeContext())
            {

                if (ctx.tblPersons.Any(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim()))
                {
                    throw new HttpResponseException(
                        new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ExistentDocument));
                }
                else
                {

                    try
                    {
                        var newPerson = new tblPersons
                        {
                            txtName = person.Name.Trim(),
                            txtDocumentNumber = person.DocumentNumber.Trim(),
                            dteBorn = DateTime.Parse(person.Born, new CultureInfo("pt-BR")), //DateTime.Parse(person.Born),
                            dteRegister = DateTime.Now,
                            txtEmail = person.Email,
                            intIdCurrentOperator = person.IdCurrentOperator
                        };
                        ctx.tblPersons.Add(newPerson);
                        ctx.SaveChanges();

                        var personId = newPerson.intIdPerson;
                        SetAssociationPerson(new Person { Id = personId });

                        try
                        {
                            foreach (var phone in person.Phones)
                            {
                                ctx.tblPersonsPhones.Add(new tblPersonsPhones
                                {
                                    intIdPerson = personId,
                                    intDDD = Convert.ToInt32(phone.DDD),
                                    intPhone = Convert.ToInt64(phone.Number),
                                    intIdOperator = phone.IdOperator,
                                    bitPhoneClube = phone.IsFoneclube,
                                    bitPortability = phone.Portability,
                                    intIdPlan = phone.IdPlanOption,
                                    txtNickname = phone.NickName,
                                    bitAtivo = phone.LinhaAtiva,
                                    intIdStatus = phone.Status,
                                    dteEntradaLinha = DateTime.Now
                                });

                                var phoneAccess = new PhoneAccess();
                                var propertyPhone = new Phone
                                {
                                    DDD = phone.DDD,
                                    Number = phone.Number,
                                    IdPlanOption = phone.IdPlanOption,
                                    Id = phone.Id
                                };

                                if (Convert.ToBoolean(phone.IsFoneclube))
                                {
                                    var statusPhone = phoneAccess.GetStatusLinha(person.Id, propertyPhone);

                                    if (Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Ativa)
                                        phoneAccess.InsertHistoricoAtivarLinha(person.Id, propertyPhone);
                                    else if (!Convert.ToBoolean(phone.LinhaAtiva) && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Desativa)
                                        phoneAccess.InsertHistoricoDesativarLinha(person.Id, propertyPhone);
                                }
                            }

                            ctx.SaveChanges();
                        }
                        catch (Exception)
                        {
                            var personToRemove = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == personId);
                            ctx.tblPersons.Remove(personToRemove);
                            ctx.SaveChanges();
                        }



                    }
                    catch (Exception)
                    {
                        throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não foi possível adicionar Cliente, campos básicos de contato indevidos.")));
                    }

                    ctx.SaveChanges();

                }
                return HttpStatusCode.OK;
            }
        }

        public HttpStatusCode InstaInsertPerson(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                if (ctx.tblPersons.Any(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim()))
                {
                    throw new HttpResponseException(
                        new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ExistentDocument));
                }
                else
                {
                    try
                    {
                        var newPerson = new tblPersons
                        {
                            txtName = person.Name.Trim(),
                            txtDocumentNumber = person.DocumentNumber.Trim(),
                            dteRegister = DateTime.Now,
                            txtEmail = person.Email,
                            txtDefaultWAPhones = person.IntlPhone.CountryCode + person.IntlPhone.Phone
                        };
                        ctx.tblPersons.Add(newPerson);
                        ctx.SaveChanges();

                        ctx.tblPersonsParents.Add(new tblPersonsParents
                        {
                            dteCadastro = DateTime.Now,
                            intIdSon = newPerson.intIdPerson,
                            intIdParent = 4555
                        });
                        ctx.SaveChanges();

                        var ddd = person.IntlPhone.Phone.Substring(0, 2);
                        var phone = person.IntlPhone.Phone.Substring(2);
                        //if (ctx.tblPersonsPhones.Any(p => p.intDDD + "" + p.intPhone == person.IntlPhone.Phone && p.bitPhoneClube == false))
                        //{
                        //    throw new HttpResponseException(
                        //        new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ExistentPhone));
                        //}
                        //else
                        //{
                        try
                        {
                            var newPhone = new tblPersonsPhones
                            {
                                intDDD = Convert.ToInt32(ddd),
                                intPhone = Convert.ToInt64(phone),
                                bitPhoneClube = false,
                                intIdOperator = -1,
                                intIdPerson = newPerson.intIdPerson,
                                bitAtivo = true,
                                bitDelete = false,
                                intCountryCode = Convert.ToInt32(person.IntlPhone.CountryCode),
                                intIdPlan = -1,
                            };
                            ctx.tblPersonsPhones.Add(newPhone);
                            ctx.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogMessageOld(1, "ProfileAccess:InstaInsertPerson Inner: Error" + ex.ToString());

                            throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(string.Format("Não foi possível adicionar Cliente, campos básicos de contato indevidos.")));
                        }

                        WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                        var wmessage = new WhatsAppMessage()
                        {
                            Message = "New Insta registration - Name : " + person.Name + " CPF: " + person.DocumentNumber,
                            ClientIds = "5521982008200,5521981908190,919004453881"
                        };
                        whatsAppAccess.SendMessage(wmessage);
                        //}
                    }
                    catch (HttpResponseException ex)
                    {
                        LogHelper.LogMessageOld(1, "ProfileAccess:InstaInsertPerson: HttpResponseException : " + ex.Response.ToString());

                        LogHelper.LogMessageOld(1, "ProfileAccess:InstaInsertPerson: HttpResponseException : " + ex.Response.ReasonPhrase);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessageOld(1, "ProfileAccess:InstaInsertPerson: Error : " + ex.ToString());
                        throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não foi possível adicionar Cliente, campos básicos de contato indevidos.")));
                    }
                }
                return HttpStatusCode.OK;
            }
        }

        public bool InsertNewPhoneToCustomer(List<tblPersonsPhones> phones)
        {
            using (var ctx = new FoneClubeContext())
            {
                foreach (var phone in phones)
                {
                    var existing = ctx.tblPersonsPhones.Any(p => p.intDDD + "" + p.intPhone == phone.intDDD + "" + phone.intPhone && p.intIdPerson == phone.intIdPerson && p.bitPhoneClube == true);
                    if (!existing)
                    {
                        var newPhone = new tblPersonsPhones
                        {
                            intDDD = Convert.ToInt32(phone.intDDD),
                            intPhone = Convert.ToInt64(phone.intPhone),
                            bitPhoneClube = true,
                            intIdOperator = phone.intIdOperator,
                            intIdPerson = phone.intIdPerson,
                            bitAtivo = true,
                            bitDelete = false,
                            intCountryCode = 55,
                            intIdPlan = phone.intIdPlan,
                            txtPortNumber = phone.txtPortNumber,
                            txtICCID = phone.txtICCID,
                            txtNickname = phone.txtNickname,
                            intAmmoutPrecoVip = phone.intAmmoutPrecoVip
                        };
                        ctx.tblPersonsPhones.Add(newPhone);
                        ctx.SaveChanges();
                    }
                }

                return true;
            }
        }
        public HttpStatusCode InsertPersonData(Person person)
        {
            //update da pessoa dos valores enviados
            //o que não for enviado não tem alteração
            using (var ctx = new FoneClubeContext())
            {
                var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim());
                person.Id = tblPerson.intIdPerson;

                if (!bool.Equals(tblPerson, null))
                {
                    if (!string.IsNullOrEmpty(person.Name))
                        tblPerson.txtName = person.Name.Trim();

                    if (!string.IsNullOrEmpty(person.NickName))
                        tblPerson.txtNickName = person.NickName;

                    if (!string.IsNullOrEmpty(person.IdPagarme.ToString()))
                        tblPerson.intIdPagarme = person.IdPagarme;

                    if (!string.IsNullOrEmpty(person.Email))
                        tblPerson.txtEmail = person.Email;

                    if (!string.IsNullOrEmpty(person.Born))
                        tblPerson.dteBorn = DateTime.Parse(person.Born);

                    if (!bool.Equals(person.Gender, null))
                        tblPerson.intGender = person.Gender;

                    if (!bool.Equals(person.IdRole, null))
                        tblPerson.intIdRole = person.IdRole;

                    if (!bool.Equals(person.Phones, null))
                        if (person.Phones.Count > 0)
                            if (!SaveContactInfo(person, ctx))
                                throw new HttpResponseException(new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ContactError));

                    if (!bool.Equals(person.Photos, null))
                        if (person.Photos.Count > 0)
                            if (!SavePersonImages(person, ctx))
                                throw new HttpResponseException(new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ImagensError));


                    if (!bool.Equals(person.Images, null) && person.Images.Any())
                        if (!SavePersonImagesLegacy(person, ctx))
                            throw new HttpResponseException(
                                  new Utils().GetErrorPostMessage(Utils.ReasonPhrase.ImagensError));


                    if (!string.IsNullOrEmpty(person.PhoneDDDParent) && !string.IsNullOrEmpty(person.PhoneNumberParent))
                        if (!SetCustomerParentPhone(person))
                            throw new HttpResponseException(
                                  new Utils().GetErrorPostMessage("Não foi possível update em quem convidou."));

                    var cadastraPlano = false;//person.Plans.Count > 0;

                    if (!bool.Equals(person.Id, null) && person.Phones != null)
                        cadastraPlano = person.Phones.Any(p => p.IdPlanOption > 0);


                    if (cadastraPlano)
                        SavePersonPlans(person, ctx);

                    if (!string.IsNullOrEmpty(person.PhoneDDDParent) && !string.IsNullOrEmpty(person.PhoneNumberParent))
                        if (!SetCustomerParentPhone(person))
                            throw new HttpResponseException(
                                  new Utils().GetErrorPostMessage("Não foi possível update em quem convidou."));


                    if (!string.IsNullOrEmpty(person.SinglePrice.ToString()))
                        if (!InsertSinglePrice(person, ctx))
                            throw new HttpResponseException(
                                new Utils().GetErrorPostMessage("Não foi possível update Single price"));


                    ctx.SaveChanges();
                    return HttpStatusCode.OK;

                }
                else
                {
                    throw new HttpResponseException(
                         new Utils().GetErrorPostMessage(Utils.ReasonPhrase.UpdateError));
                }
            }
        }

        public HttpStatusCode InsertPersonAdress(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                if (person.Adresses.Count > 0)
                {
                    person.Id = GetPersonId(person);
                    if (SavePersonAddresses(person, ctx))
                        return HttpStatusCode.OK;
                    else
                        throw new HttpResponseException(
                         new Utils().GetErrorPostMessage(Utils.ReasonPhrase.InsertAddressError));
                }
                else
                {
                    throw new HttpResponseException(
                         new Utils().GetErrorPostMessage(Utils.ReasonPhrase.InsertAddressError));
                }
            }
        }

        private void RemoveNewPerson(tblPersons newPerson, FoneClubeContext ctx)
        {
            //remover dependências
            ctx.tblPersons.Attach(newPerson);
            ctx.tblPersons.Remove(newPerson);
            ctx.SaveChanges();
        }

        public int GetPersonId(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == person.DocumentNumber.Trim()).intIdPerson;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public bool SaveContactInfo(Person person, FoneClubeContext ctx)
        {
            try
            {
                foreach (var phone in person.Phones)
                {

                    int? dddNumber;
                    long? phoneNumber;

                    try
                    {
                        dddNumber = string.IsNullOrEmpty(phone.DDD.ToString()) ? (int?)null : Convert.ToInt32(phone.DDD);
                        phoneNumber = string.IsNullOrEmpty(phone.Number.ToString()) ? (long?)null : Convert.ToInt64(phone.Number);
                    }
                    catch (Exception e)
                    {
                        dddNumber = null;
                        phoneNumber = null;
                    }

                    ctx.tblPersonsPhones.Add(new tblPersonsPhones
                    {
                        intIdPerson = person.Id,
                        intDDD = dddNumber,
                        intPhone = phoneNumber,
                        intIdOperator = phone.IdOperator,
                        bitPhoneClube = phone.IsFoneclube,
                        bitPortability = phone.Portability,
                        intIdPlan = phone.IdPlanOption,
                        txtNickname = phone.NickName,
                        bitAtivo = phone.LinhaAtiva,
                        intIdStatus = phone.Status,
                        dteEntradaLinha = DateTime.Now
                    });
                }

                ctx.SaveChanges();
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SavePersonAddresses(Person person, FoneClubeContext ctx)
        {
            try
            {
                foreach (var adress in person.Adresses)
                {
                    if (string.IsNullOrEmpty(adress.Cep)
                        || string.IsNullOrEmpty(adress.State)
                        || string.IsNullOrEmpty(adress.City)
                        || string.IsNullOrEmpty(adress.Street)
                        || string.IsNullOrEmpty(adress.Neighborhood)
                        || string.IsNullOrEmpty(adress.State)
                        || string.IsNullOrEmpty(adress.StreetNumber)
                        || string.IsNullOrEmpty(person.Id.ToString()))
                        return false;


                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        intIdPerson = person.Id,
                        txtCep = adress.Cep,
                        txtCity = adress.City,
                        txtStreet = adress.Street,
                        txtCountry = adress.Country, //opção: string.IsNullOrEmpty(adress.Country) ? null : adress.Country.Trim(), 
                        txtComplement = adress.Complement,
                        txtNeighborhood = adress.Neighborhood,
                        txtState = adress.State,
                        intStreetNumber = Convert.ToInt32(adress.StreetNumber)
                    });
                }

                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SavePersonAddress(Person person, FoneClubeContext ctx)
        {
            try
            {
                var hasAddress = ctx.tblPersonsAddresses.Any(a => a.intIdPerson == person.Id);

                if (!hasAddress)
                {
                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        intIdPerson = person.Id,
                        txtCep = person.Adresses.FirstOrDefault().Cep,
                        txtCity = person.Adresses.FirstOrDefault().City,
                        txtStreet = person.Adresses.FirstOrDefault().Street,
                        txtCountry = person.Adresses.FirstOrDefault().Country,
                        txtComplement = person.Adresses.FirstOrDefault().Complement,
                        txtNeighborhood = person.Adresses.FirstOrDefault().Neighborhood,
                        txtState = person.Adresses.FirstOrDefault().State,
                        intStreetNumber = Convert.ToInt32(person.Adresses.FirstOrDefault().StreetNumber)
                    });
                }
                else
                {
                    var address = ctx.tblPersonsAddresses.FirstOrDefault(a => a.intIdPerson == person.Id);
                    address.txtCep = person.Adresses.FirstOrDefault().Cep;
                    address.txtCity = person.Adresses.FirstOrDefault().City;
                    address.txtStreet = person.Adresses.FirstOrDefault().Street;
                    address.txtCountry = person.Adresses.FirstOrDefault().Country;
                    address.txtComplement = person.Adresses.FirstOrDefault().Complement;
                    address.txtNeighborhood = person.Adresses.FirstOrDefault().Neighborhood;
                    address.txtState = person.Adresses.FirstOrDefault().State;
                    address.intStreetNumber = Convert.ToInt32(person.Adresses.FirstOrDefault().StreetNumber);
                }

                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SavePersonImages(Person person, FoneClubeContext ctx)
        {
            try
            {

                var images = new List<tblPersonsImages>();


                int[] ids = person.Photos.Select(i => i.Id).ToArray();
                var date = DateTime.Now;

                var personImages = ctx.tblPersonsImages.Where(x => x.intIdPerson == person.Id).ToList();

                if (personImages.Any())
                {
                    var removed = personImages.Where(y => !ids.Contains(y.intId)).ToList();
                    if (removed.Any())
                        ctx.tblPersonsImages.RemoveRange(removed);
                }

                var inserted = person.Photos.Where(i => i.Id == 0).ToList();

                foreach (var image in inserted)
                {

                    images.Add(new tblPersonsImages
                    {
                        intIdPerson = person.Id,
                        txtImage = image.Name,
                        intTipo = image.Tipo,
                        dteDataCadastro = date

                    });

                }

                ctx.tblPersonsImages.AddRange(images);
                ctx.SaveChanges();


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public bool SavePersonImagesLegacy(Person person, FoneClubeContext ctx)
        {
            try
            {
                var images = new List<tblPersonsImages>();
                var date = DateTime.Now;

                foreach (var image in person.Images)
                {
                    images.Add(new tblPersonsImages
                    {
                        intIdPerson = person.Id,
                        txtImage = image,
                        dteDataCadastro = date
                    });
                }

                ctx.tblPersonsImages.AddRange(images);
                ctx.SaveChanges();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool SavePersonPlans(Person person, FoneClubeContext ctx)
        {
            try
            {
                var plans = new List<tblPlans>();
                foreach (var phone in person.Phones)
                {
                    if (!string.IsNullOrEmpty(phone.IdPlanOption.ToString()))
                    {
                        plans.Add(new tblPlans
                        {
                            intIdPerson = person.Id,
                            intIdOption = phone.IdPlanOption
                        });
                    }
                }

                ctx.tblPlans.AddRange(plans);
                ctx.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        #region comments
        //Get my indications to reward
        // I can have many childs (now 3 is the limit) but only one father
        //momento que pagou e precisa liberar ordem de pagamento
        //public List<Person> GetParents(Person person)
        //{
        //    try
        //    {
        //        using (var ctx = new FoneClubeContext())
        //        {
        //            var parents = new List<Person>();

        //            var firstLevelReference = ctx.tblReferred.FirstOrDefault(p => p.intIdCurrent == person.Id && p.intIdComission == Utils.Commission.FirstLevel);

        //            if (!bool.Equals(firstLevelReference, null))
        //                parents.Add(new Person { Id = firstLevelReference.intIdDad, IdCommissionLevel = Utils.Commission.FirstLevel });
        //            else
        //                return parents;

        //            var secondLevelReference = ctx.tblReferred.FirstOrDefault(p => p.intIdCurrent == person.Id && p.intIdComission == Utils.Commission.SecondLevel);

        //            if (!bool.Equals(secondLevelReference, null))
        //                parents.Add(new Person { Id = secondLevelReference.intIdDad, IdCommissionLevel = Utils.Commission.SecondLevel });
        //            else
        //                return parents;

        //            var thirdLevelReference = ctx.tblReferred.FirstOrDefault(p => p.intIdCurrent == person.Id && p.intIdComission == Utils.Commission.ThirdLevel);

        //            if (!bool.Equals(thirdLevelReference, null))
        //                parents.Add(new Person { Id = thirdLevelReference.intIdDad, IdCommissionLevel = Utils.Commission.ThirdLevel });
        //            else
        //                return parents;


        //            return parents;
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}


        //public int? GetParentId(int idCurrentPerson)
        //{

        //    using (var ctx = new FoneClubeContext())
        //    {
        //        var firstLevelReference = ctx.tblReferred.FirstOrDefault(p => p.intIdCurrent == idCurrentPerson && p.intIdComission == Utils.Commission.FirstLevel);
        //        if (!bool.Equals(firstLevelReference, null))
        //            return firstLevelReference.intIdDad;
        //        else
        //            return null;
        //    }
        //}

        //momento de cadastro direcionando indicações
        //public bool SavePersonIndicator(Person person, FoneClubeContext ctx)
        //{
        //    try
        //    {
        //        // acrescentar para referencia id da opção do plano mais o intcontact ( ddd mais telefone )
        //        //TODO separar entidade Person de Indicator ( ter uma classe de indicador com ID, e Parent )

        //        //todo criar enum, planos que não tem comissão
        //        if (person.IdPlanOption == 1 || person.IdPlanOption == 8)
        //            return true;

        //        //adds first level commission
        //        ctx.tblReferred.Add(new tblReferred
        //        {
        //            intIdComission = Utils.Commission.FirstLevel,
        //            intIdDad = Convert.ToInt32(person.IdParent),
        //            intIdCurrent = person.Id
        //        });

        //        //posso mover pra um foreach esse objeto posteriormente se escalar
        //        var indicatorParents = new ProfileAccess().GetParents(new Person { Id = Convert.ToInt32(person.IdParent) });

        //        //adds second level comission if is possible ( to indicator parent ) 
        //        var secondLevelRelation = indicatorParents.FirstOrDefault(o => o.IdCommissionLevel == Utils.Commission.FirstLevel);
        //        if (!bool.Equals(secondLevelRelation, null))
        //        {
        //            ctx.tblReferred.Add(new tblReferred
        //            {
        //                intIdComission = Utils.Commission.SecondLevel,
        //                intIdDad = secondLevelRelation.Id, //current
        //                intIdCurrent = person.Id
        //            });
        //        }

        //        //adds third level comission if is possible ( to indicator parent.parent ) 
        //        var thirdLevelRelation = indicatorParents.FirstOrDefault(o => o.IdCommissionLevel == Utils.Commission.SecondLevel);
        //        if (!bool.Equals(thirdLevelRelation, null))
        //        {
        //            ctx.tblReferred.Add(new tblReferred
        //            {
        //                intIdComission = Utils.Commission.ThirdLevel,
        //                intIdDad = thirdLevelRelation.Id,
        //                intIdCurrent = person.Id
        //            });
        //        }

        //        ctx.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        //remover códgo a baixo

        #endregion

        public Person GetPersonName(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                var phoneNumber = 0;
                if (!bool.Equals(person.Phones.FirstOrDefault(), null))
                    phoneNumber = Convert.ToInt32(person.Phones.FirstOrDefault().Number);

                var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intContactId == phoneNumber);
                if (!bool.Equals(tblPerson, null))
                {
                    return new Person
                    {
                        Id = tblPerson.intIdPerson,
                        Name = tblPerson.txtName
                    };
                }
                else
                {
                    throw new HttpResponseException(
                    new Utils().GetErrorPostMessage(string.Format("Indicação feita é inválida: {0}", person.Id)));
                }
            }

        }

        public tblChargingHistory GetlastChargingHistory(int personId)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblCH = ctx.tblChargingHistory.Where(x => x.intIdCustomer == personId).OrderByDescending(x => x.dteCreate).FirstOrDefault();
                return tblCH;
            }
        }

        public tblFoneclubePagarmeTransactions GetLastPaymentByTransId(long transactionId)
        {
            using (var ctx = new FoneClubeContext())
            {
                var tblCH = ctx.tblFoneclubePagarmeTransactions.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                return tblCH;
            }
        }

        public List<Charging> GetChargingHistory(int personID)
        {
            using (var ctx = new FoneClubeContext())
            {
                var charges = new List<Charging>();
                var tblCharges = ctx.tblChargingHistory.Where(c => c.intIdCustomer == personID).ToList();
                var tblChargesScheduled = ctx.tblChargingScheduled.Where(c => c.intIdCustomer == personID && c.bitExecuted == false).ToList();
                var person = ctx.tblPersons.Where(x => x.intIdPerson == personID).FirstOrDefault();
                foreach (var charge in tblCharges)
                {
                    charges.Add(new Charging
                    {
                        CreateDate = charge.dteCreate,
                        CreationDate = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd h:mm tt"),
                        CollectorName = charge.txtCollectorName,
                        Comment = charge.txtComment,
                        IdCollector = charge.intIdCollector,
                        PaymentType = charge.intIdPaymentType,
                        Ammount = charge.txtAmmountPayment,
                        Token = charge.txtTokenTransaction,
                        BoletoId = Convert.ToInt64(charge.intIdBoleto),
                        AcquireId = charge.txtAcquireId,
                        CommentEmail = charge.txtCommentEmail,
                        AnoVingencia = Convert.ToDateTime(charge.dteValidity).Year.ToString(),
                        MesVingencia = Convert.ToDateTime(charge.dteValidity).Month.ToString(),
                        ExpireDate = charge.dteDueDate.HasValue ? charge.dteDueDate.Value : Convert.ToDateTime(charge.dteValidity),
                        DueDate = charge.dteDueDate.HasValue ? charge.dteDueDate.Value : Convert.ToDateTime(charge.dteValidity),
                        TxtWAPhones = person.txtDefaultWAPhones,
                        ChargingComment = charge.txtChargingComment,
                        SendWAText = charge.bitSendWAText,
                        SendMarketing1 = charge.bitSendMarketing1,
                        SendMarketing2 = charge.bitSendMarketing2,
                        BoletoUrl = charge.txtboletoUrl,
                        BoletoBarcode = charge.txtboletoBarcode,
                        DefaultPaymentDay = !person.intDftBillPaymentDay.HasValue ? 25 : person.intDftBillPaymentDay.Value,
                        VerficarDay = !person.intDftVerificar.HasValue ? 5 : person.intDftVerificar.Value,
                        Installments = charge.intInstallments.HasValue ? charge.intInstallments.Value : 0
                    });
                }

                foreach (var charge in tblChargesScheduled)
                {
                    charges.Add(new Charging
                    {
                        CreateDate = charge.dteCreate,
                        CreationDate = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd h:mm tt"),
                        CollectorName = charge.txtCollectorName,
                        Comment = charge.txtComment,
                        IdCollector = charge.intIdCollector,
                        PaymentType = charge.intIdPaymentType,
                        Ammount = charge.txtAmmountPayment,
                        Token = charge.txtTokenTransaction,
                        BoletoId = Convert.ToInt64(charge.intIdBoleto),
                        AcquireId = charge.txtAcquireId,
                        CommentEmail = charge.txtCommentEmail,
                        AnoVingencia = Convert.ToDateTime(charge.dteValidity).Year.ToString(),
                        MesVingencia = Convert.ToDateTime(charge.dteValidity).Month.ToString(),
                        ExpireDate = charge.dteDueDate.HasValue ? charge.dteDueDate.Value : Convert.ToDateTime(charge.dteValidity),
                        DueDate = charge.dteExecution,
                        TxtWAPhones = person.txtDefaultWAPhones,
                        ChargingComment = charge.txtChargingComment,
                        SendWAText = charge.bitSendWAText,
                        SendMarketing1 = charge.bitSendMarketing1,
                        SendMarketing2 = charge.bitSendMarketing2,
                        DefaultPaymentDay = !person.intDftBillPaymentDay.HasValue ? 25 : person.intDftBillPaymentDay.Value,
                        VerficarDay = !person.intDftVerificar.HasValue ? 5 : person.intDftVerificar.Value,
                        Installments = charge.intInstallments.HasValue ? charge.intInstallments.Value : 0
                    });
                }

                var listCharges = charges.OrderByDescending(x => x.CreateDate).ToList();

                return listCharges;
            }

        }

        public List<Charging> GetChargingHistory(string documentNumber)
        {
            using (var ctx = new FoneClubeContext())
            {
                var charges = new List<Charging>();
                var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == documentNumber);

                if (tblPerson == null)
                    return charges;

                var personID = tblPerson.intIdPerson;

                var tblCharges = ctx.tblChargingHistory.Where(c => c.intIdCustomer == personID).ToList();
                foreach (var charge in tblCharges)
                {
                    charges.Add(new Charging
                    {
                        CreationDate = Convert.ToDateTime(charge.dteCreate).ToString("yyyy-MM-dd h:mm tt"),
                        CollectorName = charge.txtCollectorName,
                        Comment = charge.txtComment,
                        IdCollector = charge.intIdCollector,
                        PaymentType = charge.intIdPaymentType,
                        Ammount = charge.txtAmmountPayment,
                        Token = charge.txtTokenTransaction,
                        BoletoId = Convert.ToInt64(charge.intIdBoleto),
                        AcquireId = charge.txtAcquireId
                    });
                }

                charges.OrderBy(c => c.CreationDate);

                charges.Reverse();

                return charges;
            }

        }

        public HttpStatusCode SaveChargingHistory(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (person.DefaultPaymentDay.HasValue)
                    {
                        var personInfo = ctx.tblPersons.Where(x => x.intIdPerson == person.Id).FirstOrDefault();
                        personInfo.intDftBillPaymentDay = person.DefaultPaymentDay.Value;
                        ctx.SaveChanges();
                    }
                }
                using (var ctx = new FoneClubeContext())
                {
                    DateTime vingencia;
                    DateTime pixExpiryDate;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                        pixExpiryDate = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                        pixExpiryDate = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    if (!string.IsNullOrEmpty(person.Charging.PixCode))
                    {
                        person.Charging.PaymentType = (int)pagarme.PagarmeAccess.PaymentTypes.pix;
                        pixExpiryDate = person.Charging.ExpireDate;
                    }

                    var chargingHistory = new tblChargingHistory
                    {
                        txtAmmountPayment = person.Charging.Ammount,
                        intIdCollector = person.Charging.IdCollector,
                        intIdCustomer = person.Id,
                        intIdPaymentType = person.Charging.PaymentType,
                        txtCollectorName = person.Charging.CollectorName,
                        txtComment = person.Charging.Comment,
                        txtCommentBoleto = person.Charging.CommentBoleto,
                        txtCommentEmail = person.Charging.CommentEmail,
                        txtTokenTransaction = person.Charging.Token,
                        intIdBoleto = person.Charging.BoletoId,
                        txtAcquireId = person.Charging.AcquireId,
                        dteValidity = vingencia,
                        intChargeStatusId = person.Charging.ChargeStatus,
                        dteCreate = DateTime.Now,
                        bitCash = person.Charging.CacheTransaction,
                        dtePayment = DateTime.Now,
                        pixCode = person.Charging.PixCode,
                        dteDueDate = person.Charging.DueDate,
                        dteExpiryDate = pixExpiryDate,
                        bitActive = true,
                        txtWAPhones = person.Charging.TxtWAPhones,
                        txtChargingComment = person.Charging.ChargingComment,
                        bitSendWAText = person.Charging.SendWAText,
                        bitSendMarketing1 = person.Charging.SendMarketing1,
                        bitSendMarketing2 = person.Charging.SendMarketing2,
                        txtboletoBarcode = person.Charging.BoletoBarcode,
                        txtboletoUrl = person.Charging.BoletoUrl,
                        intInstallments = person.Charging.Installments,
                        txtInstaRegsiterData = person.Charging.InstaRegsiterData
                    };

                    try
                    {
                        chargingHistory.intIdTransaction = Convert.ToInt64(person.Charging.TransactionId);
                        chargingHistory.txtTransactionComment = person.Charging.TransactionComment;
                    }
                    catch (Exception)
                    {
                        chargingHistory.intIdTransaction = -1;
                    }

                    try
                    {
                        ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                        {
                            dteRegister = DateTime.Now,
                            txtLog = JsonConvert.SerializeObject(chargingHistory)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }

                    ctx.tblChargingHistory.Add(chargingHistory);
                    ctx.SaveChanges();

                    try
                    {
                        chargingHistory.intIdFrete = person.Charging.Frete;
                        ctx.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }

                    if (chargingHistory.intIdTransaction > 0)
                    {
                        Helper.SendChargeSummary(chargingHistory);
                        if (chargingHistory.bitSendWAText)
                        {
                            Helper.SendChargeSummaryText(chargingHistory);
                        }
                        if (chargingHistory.bitSendMarketing1.HasValue && chargingHistory.bitSendMarketing1.Value)
                        {
                            var template = new GenericTemplate() { PersonId = chargingHistory.intIdCustomer.Value, PhoneNumbers = chargingHistory.txtWAPhones, TypeId = 1 };
                            Helper.SendMarketingMessage(template);
                        }
                        if (chargingHistory.bitSendMarketing2.HasValue && chargingHistory.bitSendMarketing2.Value)
                        {
                            var template = new GenericTemplate() { PersonId = chargingHistory.intIdCustomer.Value, PhoneNumbers = chargingHistory.txtWAPhones, TypeId = 2 };
                            Helper.SendMarketingMessage(template);
                        }

                        //todo envio de email quando é schedule
                        try
                        {

                            if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.pix && person.Charging.SendEmail || (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.pix && person.Charging.SendEmail && person.Charging.Scheduled))
                            {
                                var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                var linkImg = ConfigurationManager.AppSettings["qrcodelink"] + chargingHistory.intId;
                                var post = new EmailAccess().SendEmail(new Email
                                {
                                    To = cliente.txtEmail,
                                    TargetName = cliente.txtName.Split(' ')[0],
                                    TargetTextBlue = ConfigurationManager.AppSettings["qrcodelink"] + chargingHistory.intId,
                                    TargetSecondaryText = @"<b>Total da sua conta: R$ " + (Convert.ToDouble(person.Charging.Ammount) / 100).ToString("F").Replace(".", ",") + "</b>",
                                    TemplateType = Convert.ToInt32(Email.TemplateTypes.Pix),
                                    TargetTextComment = person.Charging.CommentEmail
                                });
                            }
                            else if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.boleto && person.Charging.SendEmail && person.Charging.Scheduled)
                            {
                                var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                var post = new EmailAccess().SendEmail(new Email
                                {
                                    To = cliente.txtEmail,
                                    TargetName = cliente.txtName.Split(' ')[0],
                                    TargetTextBlue = person.Charging.BoletoLink,
                                    TargetSecondaryText = person.Charging.CommentBoleto,
                                    TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                                });
                            }
                            else if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.card && person.Charging.SendEmail && person.Charging.Scheduled)
                            {
                                /*
                                 * //card
                                 var emailObject = {
                                        'Id':vm.customer.Id,
                                        'To': vm.newCustomer.email, //vm.newCustomer.email
                                        'TargetName' : vm.newCustomer.name,
                                        'TargetTextBlue' : $filter('currency')(vm.amount / 100, ""),
                                        // 'CustomerComment':vm.customerComment,
                                        'TargetSecondaryText' : vm.customerComment,
                                        // 'TargetSecondaryText' : vm.commentBoleto,
                                        'TemplateType' : 1
                                    }
                                 */

                                //var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                //var post = new EmailAccess().SendEmail(new Email
                                //{
                                //    To = cliente.txtEmail,
                                //    TargetName = cliente.txtName.Split(' ')[0],
                                //    TargetTextBlue = (Convert.ToDouble(person.Charging.Ammount) / 100).ToString("F").Replace(".", ","),
                                //    TargetSecondaryText = "",
                                //    TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                                //});
                            }

                        }
                        catch (Exception)
                        {

                        }
                    }
                    var tipo = (Convert.ToInt32(person.Charging.PaymentType) == 1) ? "cartão de crédito" : "boleto";
                    //new Utils().SendEmail(Utils.EmailTo.Vendas, "Cobrança realizada.", string.Format(" Foi realizado uma cobrança no valor de {0} Reais para o cliente: {1}\n Id:{2} \n Observação:{3} \n Tipo de cobrança:{4} \n Agente:{4}", (Convert.ToDouble(person.Charging.Ammount) / 100), ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id).txtName, person.Id, person.Charging.Comment, tipo, person.Charging.CollectorName));

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public HttpStatusCode SaveChargingHistoryNew(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (person.DefaultPaymentDay.HasValue)
                    {
                        var personInfo = ctx.tblPersons.Where(x => x.intIdPerson == person.Id).FirstOrDefault();
                        personInfo.intDftBillPaymentDay = person.DefaultPaymentDay.Value;
                        ctx.SaveChanges();
                    }
                }
                using (var ctx = new FoneClubeContext())
                {
                    if (person.Charging.MutliVigencias != null && person.Charging.MutliVigencias.Length > 0)
                    {
                        person.Charging.MutliVigencias = person.Charging.MutliVigencias.OrderBy(d => d).ToArray();
                        int count = person.Charging.MutliVigencias.Length;
                        for (int icount = 0; icount < count; icount++)
                        {
                            DateTime vingencia;
                            DateTime pixExpiryDate;
                            try
                            {
                                vingencia = new DateTime(Convert.ToInt32(person.Charging.MutliVigencias[icount].Split(' ')[0]), Convert.ToInt32(person.Charging.MutliVigencias[icount].Split(' ')[1]), 1, 0, 0, 0);
                                pixExpiryDate = new DateTime(Convert.ToInt32(person.Charging.MutliVigencias[icount].Split(' ')[0]), Convert.ToInt32(person.Charging.MutliVigencias[icount].Split(' ')[1]), 1, 0, 0, 0);
                            }
                            catch (Exception)
                            {
                                vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                                pixExpiryDate = new DateTime(2000, 1, 1, 0, 0, 0);
                            }

                            if (!string.IsNullOrEmpty(person.Charging.PixCode))
                            {
                                person.Charging.PaymentType = (int)pagarme.PagarmeAccess.PaymentTypes.pix;
                                pixExpiryDate = person.Charging.ExpireDate;
                            }

                            var chargingHistory = new tblChargingHistory
                            {
                                txtAmmountPayment = (icount == count - 1) ? person.Charging.Ammount : "000",
                                intIdCollector = person.Charging.IdCollector,
                                intIdCustomer = person.Id,
                                intIdPaymentType = person.Charging.PaymentType,
                                txtCollectorName = person.Charging.CollectorName,
                                txtComment = person.Charging.Comment,
                                txtCommentBoleto = person.Charging.CommentBoleto,
                                txtCommentEmail = person.Charging.CommentEmail,
                                txtTokenTransaction = person.Charging.Token,
                                intIdBoleto = person.Charging.BoletoId,
                                txtAcquireId = person.Charging.AcquireId,
                                dteValidity = vingencia,
                                intChargeStatusId = person.Charging.ChargeStatus,
                                dteCreate = DateTime.Now,
                                bitCash = person.Charging.CacheTransaction,
                                dtePayment = DateTime.Now,
                                pixCode = person.Charging.PixCode,
                                dteDueDate = person.Charging.DueDate,
                                dteExpiryDate = pixExpiryDate,
                                bitActive = true,
                                txtWAPhones = person.Charging.TxtWAPhones,
                                txtChargingComment = person.Charging.ChargingComment,
                                bitSendWAText = person.Charging.SendWAText,
                                bitSendMarketing1 = person.Charging.SendMarketing1,
                                bitSendMarketing2 = person.Charging.SendMarketing2,
                                txtboletoBarcode = person.Charging.BoletoBarcode,
                                txtboletoUrl = person.Charging.BoletoUrl,
                                intInstallments = person.Charging.Installments,
                                txtInstaRegsiterData = person.Charging.InstaRegsiterData
                            };

                            try
                            {
                                chargingHistory.intIdTransaction = Convert.ToInt64(person.Charging.TransactionId);
                                chargingHistory.txtTransactionComment = person.Charging.TransactionComment;
                            }
                            catch (Exception)
                            {
                                chargingHistory.intIdTransaction = -1;
                            }

                            try
                            {
                                ctx.tblLogBackupCharging.Add(new tblLogBackupCharging
                                {
                                    dteRegister = DateTime.Now,
                                    txtLog = JsonConvert.SerializeObject(chargingHistory)
                                });

                                ctx.SaveChanges();
                            }
                            catch (Exception) { }

                            ctx.tblChargingHistory.Add(chargingHistory);
                            ctx.SaveChanges();

                            try
                            {
                                chargingHistory.intIdFrete = person.Charging.Frete;
                                ctx.SaveChanges();
                            }
                            catch (Exception)
                            {

                            }

                            if (chargingHistory.intIdTransaction > 0)
                            {
                                if (icount == count - 1)
                                {
                                    Helper.SendChargeSummary(chargingHistory);
                                    if (chargingHistory.bitSendWAText)
                                    {
                                        Helper.SendChargeSummaryText(chargingHistory);
                                    }
                                    if (chargingHistory.bitSendMarketing1.HasValue && chargingHistory.bitSendMarketing1.Value)
                                    {
                                        var template = new GenericTemplate() { PersonId = chargingHistory.intIdCustomer.Value, PhoneNumbers = chargingHistory.txtWAPhones, TypeId = 1 };
                                        Helper.SendMarketingMessage(template);
                                    }
                                    if (chargingHistory.bitSendMarketing2.HasValue && chargingHistory.bitSendMarketing2.Value)
                                    {
                                        var template = new GenericTemplate() { PersonId = chargingHistory.intIdCustomer.Value, PhoneNumbers = chargingHistory.txtWAPhones, TypeId = 2 };
                                        Helper.SendMarketingMessage(template);
                                    }

                                    //todo envio de email quando é schedule
                                    try
                                    {

                                        if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.pix && person.Charging.SendEmail || (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.pix && person.Charging.SendEmail && person.Charging.Scheduled))
                                        {
                                            var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                            var linkImg = ConfigurationManager.AppSettings["qrcodelink"] + chargingHistory.intId;
                                            var post = new EmailAccess().SendEmail(new Email
                                            {
                                                To = cliente.txtEmail,
                                                TargetName = cliente.txtName.Split(' ')[0],
                                                TargetTextBlue = ConfigurationManager.AppSettings["qrcodelink"] + chargingHistory.intId,
                                                TargetSecondaryText = @"<b>Total da sua conta: R$ " + (Convert.ToDouble(person.Charging.Ammount) / 100).ToString("F").Replace(".", ",") + "</b>",
                                                TemplateType = Convert.ToInt32(Email.TemplateTypes.Pix),
                                                TargetTextComment = person.Charging.CommentEmail
                                            });
                                        }
                                        else if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.boleto && person.Charging.SendEmail && person.Charging.Scheduled)
                                        {
                                            var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                            var post = new EmailAccess().SendEmail(new Email
                                            {
                                                To = cliente.txtEmail,
                                                TargetName = cliente.txtName.Split(' ')[0],
                                                TargetTextBlue = person.Charging.BoletoLink,
                                                TargetSecondaryText = person.Charging.CommentBoleto,
                                                TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                                            });
                                        }
                                        else if (person.Charging.PaymentType == (int)pagarme.PagarmeAccess.PaymentTypes.card && person.Charging.SendEmail && person.Charging.Scheduled)
                                        {
                                            /*
                                             * //card
                                             var emailObject = {
                                                    'Id':vm.customer.Id,
                                                    'To': vm.newCustomer.email, //vm.newCustomer.email
                                                    'TargetName' : vm.newCustomer.name,
                                                    'TargetTextBlue' : $filter('currency')(vm.amount / 100, ""),
                                                    // 'CustomerComment':vm.customerComment,
                                                    'TargetSecondaryText' : vm.customerComment,
                                                    // 'TargetSecondaryText' : vm.commentBoleto,
                                                    'TemplateType' : 1
                                                }
                                             */

                                            //var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                                            //var post = new EmailAccess().SendEmail(new Email
                                            //{
                                            //    To = cliente.txtEmail,
                                            //    TargetName = cliente.txtName.Split(' ')[0],
                                            //    TargetTextBlue = (Convert.ToDouble(person.Charging.Ammount) / 100).ToString("F").Replace(".", ","),
                                            //    TargetSecondaryText = "",
                                            //    TemplateType = Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                                            //});
                                        }

                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                            var tipo = (Convert.ToInt32(person.Charging.PaymentType) == 1) ? "cartão de crédito" : "boleto";
                            //new Utils().SendEmail(Utils.EmailTo.Vendas, "Cobrança realizada.", string.Format(" Foi realizado uma cobrança no valor de {0} Reais para o cliente: {1}\n Id:{2} \n Observação:{3} \n Tipo de cobrança:{4} \n Agente:{4}", (Convert.ToDouble(person.Charging.Ammount) / 100), ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id).txtName, person.Id, person.Charging.Comment, tipo, person.Charging.CollectorName));
                        }
                    }
                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public HttpStatusCode SaveScheduleHistory(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    DateTime vingencia;
                    try
                    {
                        vingencia = new DateTime(Convert.ToInt32(person.Charging.AnoVingencia), Convert.ToInt32(person.Charging.MesVingencia), 1, 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        vingencia = new DateTime(2000, 1, 1, 0, 0, 0);
                    }

                    DateTime schedule;
                    try
                    {
                        schedule = new DateTime(Convert.ToInt32(person.Charging.ScheduledYear), Convert.ToInt32(person.Charging.ScheduledMonth), Convert.ToInt32(person.Charging.ScheduledDay), 0, 0, 0);
                    }
                    catch (Exception)
                    {
                        throw new Exception();
                    }

                    var chargingSchedule = new tblChargingScheduled
                    {
                        txtAmmountPayment = person.Charging.Ammount,
                        intIdCollector = person.Charging.IdCollector,
                        intIdCustomer = person.Id,
                        intIdPaymentType = person.Charging.PaymentType,
                        txtCollectorName = person.Charging.CollectorName,
                        txtComment = person.Charging.Comment,
                        txtCommentBoleto = person.Charging.CommentBoleto,
                        txtCommentEmail = person.Charging.CommentEmail,
                        txtTokenTransaction = string.Empty,
                        intIdBoleto = 0,
                        txtAcquireId = string.Empty,
                        dteValidity = vingencia,
                        intChargeStatusId = person.Charging.ChargeStatus,
                        dteCreate = DateTime.Now,
                        bitCash = person.Charging.CacheTransaction,
                        dtePayment = DateTime.Now,
                        pixCode = string.Empty,
                        dteExecution = schedule,
                        bitExecuted = false,
                        txtTransactionComment = person.Charging.TransactionComment,
                        txtWAPhones = person.Charging.TxtWAPhones,
                        txtChargingComment = person.Charging.ChargingComment,
                        bitSendWAText = person.Charging.SendWAText,
                        bitSendMarketing1 = person.Charging.SendMarketing1,
                        bitSendMarketing2 = person.Charging.SendMarketing2,
                        intInstallments = person.Charging.Installments,
                    };

                    ctx.tblChargingScheduled.Add(chargingSchedule);
                    ctx.SaveChanges();

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public HttpStatusCode DeactivePhoneNumber(Phone phone)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var tblPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == phone.Id);
                    tblPhone.bitDelete = true;
                    tblPhone.bitAtivo = false;

                    phone.DDD = tblPhone.intDDD.ToString();
                    phone.Number = tblPhone.intPhone.ToString();
                    phone.IdPlanOption = tblPhone.intIdPlan.Value;

                    var phoneAccess = new PhoneAccess();
                    phoneAccess.InsertHistoricoDesativarLinha(tblPhone.intIdPerson.Value, phone);
                    phoneAccess.InsertHistoricoDesligarLinha(tblPhone.intIdPerson.Value, phone);

                    ctx.SaveChanges();

                    return HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
        }

        public HttpStatusCode DeletePhoneNumber(Phone phone)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var tblPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == phone.Id);
                    ctx.tblPersonsPhones.Remove(tblPhone);
                    ctx.SaveChanges();

                    return HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
        }

        public List<Plan> GetCustomerPlan(string documentNumber)
        {
            var plans = new List<Plan>();
            using (var ctx = new FoneClubeContext())
            {
                var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber.Trim() == documentNumber.Trim());

                if (string.IsNullOrEmpty(tblPerson.intIdPerson.ToString()))
                    throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(string.Format("Não existe esse registro na base de dados")));


                var phones = ctx.tblPersonsPhones.Where(p => p.intIdPerson == tblPerson.intIdPerson && p.bitPhoneClube == true).ToList();

                foreach (var phone in phones)
                {
                    var planDetail = ctx.tblPlansOptions.FirstOrDefault(p => p.intIdPlan == phone.intIdPlan);

                    if (!bool.Equals(planDetail, null))
                    {
                        plans.Add(new Plan
                        {
                            Id = Convert.ToInt32(phone.intIdPlan),
                            IdOperator = planDetail.intIdOperator,
                            Value = planDetail.intCost,
                            Description = planDetail.txtDescription
                        });
                    }
                    else
                    {
                        plans.Add(new Plan());
                    }


                }
            }

            return plans;
        }

        public HttpStatusCode InsertServiceOrder(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblServiceOrders.Add(new tblServiceOrders
                    {
                        dteRegister = DateTime.Now,
                        bitPendingInteraction = person.ServiceOrder.PendingInteraction,
                        txtAgentName = person.ServiceOrder.AgentName,
                        intIdAgent = person.ServiceOrder.AgentId,
                        intIdPerson = person.Id,
                        txtDescription = person.ServiceOrder.Description
                    });

                    ctx.SaveChanges();

                    return HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public List<ServiceOrder> GetServiceOrders(int personId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var serviceOrders = new List<ServiceOrder>();

                    var orders = ctx.tblServiceOrders.Where(p => p.intIdPerson == personId).ToList();
                    foreach (var order in orders)
                    {
                        serviceOrders.Add(new ServiceOrder
                        {
                            Description = order.txtDescription,
                            PendingInteraction = Convert.ToBoolean(order.bitPendingInteraction),
                            AgentName = order.txtAgentName,
                            AgentId = Convert.ToInt32(order.intIdAgent)
                        });
                    }

                    return serviceOrders;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool InsertSinglePrice(Person person, FoneClubeContext ctx)
        {
            try
            {

                var tblDiscountPrices = ctx.tblDiscountPrice.FirstOrDefault(p => p.intIdPerson == person.Id);

                if (bool.Equals(tblDiscountPrices, null))
                {
                    ctx.tblDiscountPrice.Add(new tblDiscountPrice
                    {
                        intIdPerson = person.Id,
                        intAmmount = person.SinglePrice,
                        txtDescription = person.DescriptionSinglePrice
                    });
                }
                else
                {
                    tblDiscountPrices.intAmmount = person.SinglePrice;
                    tblDiscountPrices.txtDescription = person.DescriptionSinglePrice;
                }

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Person GetPhoneOwner(Phone telefone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var ddd = Convert.ToInt32(telefone.DDD);
                    var numero = Convert.ToInt64(telefone.Number);
                    var tblTelefone = ctx.tblPersonsPhones
                        .FirstOrDefault(p => p.intDDD == ddd && p.intPhone == numero && p.bitAtivo == true && p.bitPhoneClube == true);

                    if (!bool.Equals(tblTelefone, null))
                    {
                        var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == tblTelefone.intIdPerson);

                        return new Person
                        {
                            Id = pessoa.intIdPerson,
                            Name = pessoa.txtName,
                            NickName = pessoa.txtNickName,
                            Email = pessoa.txtEmail,
                            DocumentNumber = pessoa.txtDocumentNumber
                        };
                    }
                    {
                        return new Person { Id = -1 };
                    }

                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public Person GetPhoneOwner(Phone telefone, bool onlyFoneclube)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var ddd = Convert.ToInt32(telefone.DDD);
                    var numero = Convert.ToInt64(telefone.Number);
                    tblPersonsPhones tblTelefone;

                    if (onlyFoneclube)
                        tblTelefone = ctx.tblPersonsPhones
                        .FirstOrDefault(p => p.intDDD == ddd && p.intPhone == numero && p.bitAtivo == true && p.bitPhoneClube == true);
                    else
                        tblTelefone = ctx.tblPersonsPhones
                            .FirstOrDefault(p => p.intDDD == ddd && p.intPhone == numero && p.bitAtivo == true && p.bitPhoneClube == false);

                    if (!bool.Equals(tblTelefone, null))
                    {
                        var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == tblTelefone.intIdPerson);
                        if (!bool.Equals(pessoa, null))
                        {
                            return new Person
                            {
                                Id = pessoa.intIdPerson,
                                Name = pessoa.txtName,
                                NickName = pessoa.txtNickName,
                                Email = pessoa.txtEmail,
                                DocumentNumber = pessoa.txtDocumentNumber
                            };
                        }
                        else
                        {
                            return new Person { Id = -1 };
                        }

                    }
                    {
                        return new Person { Id = -1 };
                    }
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool SetCustomerParentPhone(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var existeRegistro = ctx.tblPersonsParents.Any(x => x.intIdSon == person.Id);
                    int? ddd;
                    int? phone;
                    if (string.IsNullOrEmpty(person.PhoneDDDParent))
                    {
                        ddd = null;
                        phone = null;
                    }
                    else
                    {
                        ddd = Convert.ToInt32(person.PhoneDDDParent);
                        phone = Convert.ToInt32(person.PhoneNumberParent);
                    }

                    if (!existeRegistro)
                    {
                        ctx.tblPersonsParents.Add(new tblPersonsParents
                        {
                            intIdSon = person.Id,
                            intDDDParent = ddd,
                            intPhoneParent = phone,
                            txtNameParent = person.NameParent,
                            dteCadastro = DateTime.Now
                        });
                    }
                    else
                    {
                        var pessoa = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == person.Id);
                        pessoa.intDDDParent = ddd;
                        pessoa.intPhoneParent = phone;
                        pessoa.txtNameParent = person.NameParent;
                        pessoa.dteCadastro = DateTime.Now;
                    }

                    ctx.SaveChanges();
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Person GetCustomerParent(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var personParent = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == person.Id);
                    if (personParent != null)
                    {
                        var parentId = personParent.intIdParent;
                        var tblPersonParent = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == parentId);

                        if (parentId == null || person.Id == 0 || tblPersonParent == null)
                            return new Person();

                        var pai = new Person
                        {
                            NameParent = personParent.txtNameParent,
                            PhoneDDDParent = personParent.intDDDParent.ToString(),
                            PhoneNumberParent = personParent.intPhoneParent.ToString(),
                            Pai = new Pai
                            {
                                Id = tblPersonParent.intIdPerson,
                                Name = tblPersonParent.txtName
                            }
                        };

                        try
                        {
                            var contatoPai = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == pai.Pai.Id && p.bitPhoneClube != true);
                            pai.Pai.ContatoPai = contatoPai.intDDD.ToString() + contatoPai.intPhone.ToString();
                        }
                        catch (Exception)
                        {
                            pai.Pai.ContatoPai = string.Empty;
                        }


                        return pai;
                    }
                    else
                        return new Person();

                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }



        public List<Phone> GetNickNameResults(string nickName)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var result = new List<Phone>();

                    var phones = ctx.tblPersonsPhones.Where(p => p.txtNickname.ToLower().Contains(nickName.ToLower())).ToList();

                    foreach (var phone in phones)
                    {

                        result.Add(new Phone { DDD = phone.intDDD.ToString(), Number = phone.intPhone.ToString(), NickName = phone.txtNickname });
                    }

                    return result;
                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public bool SetPagarmeID(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                    tblPerson.intIdPagarme = person.IdPagarme;
                    ctx.SaveChanges();
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        //Save comment to tblServiceOrders table by SKG
        public HttpStatusCode SaveServiceOrder(tblServiceOrders order)
        {
            using (var ctx = new FoneClubeContext())
            {
                if (order.bitPendingInteraction == null)
                {
                    order.bitPendingInteraction = false;
                }
                tblServiceOrders newOrder;
                try
                {
                    newOrder = new tblServiceOrders
                    {
                        //intIdAgent = order.intIdPerson,
                        intIdPerson = order.intIdPerson,
                        dteRegister = DateTime.Now,
                        txtDescription = order.txtDescription,
                        bitPendingInteraction = order.bitPendingInteraction
                    };
                    ctx.tblServiceOrders.Add(newOrder);
                    ctx.SaveChanges();
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(
                        new Utils().GetErrorPostMessage(string.Format("Não foi possível adicionar Cliente, campos básicos de contato indevidos.")));
                }
            }
        }
        //Get comments from tblServiceOrders table by SKG
        public List<tblServiceOrders> GetTblServiceOrders(int personID)
        {
            using (var ctx = new FoneClubeContext())
            {
                List<tblServiceOrders> orders = new List<tblServiceOrders>();
                try
                {
                    orders = ctx.tblServiceOrders.Where(o => o.intIdPerson == personID).OrderByDescending(o => o.dteRegister).ToList();
                }
                catch (Exception ex)
                {
                }
                return orders;
            }
        }

        //deprecated
        public tblChargingHistory GetLastPaymentMethodById(int personID)
        {
            var _tblChargingHistory = new tblChargingHistory();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var history = ctx.tblChargingHistory
                        .Where(h => h.intIdCustomer == personID)
                        .OrderByDescending(h => h.dteCreate).ToList();

                    if (history != null && history.Count != 0)
                        _tblChargingHistory = history.First();

                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
            return _tblChargingHistory;
        }

        public PersonParent GetParentByPhone(long? phoneparent, int personid)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var personParent = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == personid && x.intPhoneParent == phoneparent);
                    if (personParent != null)
                    {
                        return new PersonParent
                        {
                            DDDParent = personParent.intDDDParent,
                            PhoneParent = personParent.intPhoneParent,
                            NameParent = personParent.txtNameParent,
                            IdPerson = personParent.intIdSon


                        };
                    }
                    else
                        return new PersonParent();

                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public List<PersonParent> GetParentAll()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ////List<PersonParent> lstPersonParent = new List<PersonParent>();
                    ////var personParent = ctx.tblPersonsParents.Distinct().ToList();
                    ////foreach (var PP in personParent)
                    ////{
                    ////    lstPersonParent.Add(new PersonParent
                    ////    {
                    ////        DDDParent = PP.intDDDParent,
                    ////        PhoneParent = PP.intPhoneParent,
                    ////        NameParent= PP.txtNameParent,
                    ////        IdPerson=PP.intIdSon
                    ////    });
                    ////}

                    ////return lstPersonParent;

                    //List<string> lstPersonParent = new List<string>();
                    var tblpersons = ctx.tblPersons.ToList();
                    List<PersonParent> lstPersonParent = new List<PersonParent>();
                    var personParent = ctx.tblPersonsParents
                                        .Where(p => string.IsNullOrEmpty(p.txtNameParent) == false)
                                        .GroupBy(p => new { p.txtNameParent })
                                        //.GroupBy(p => new { p.txtNameParent, p.intPhoneParent})
                                        .Select(g => g.FirstOrDefault())
                                        .ToList();
                    foreach (var PP in personParent)
                    {
                        var doc = tblpersons.FirstOrDefault(x => x.intIdPerson == PP.intIdSon);
                        lstPersonParent.Add(new PersonParent
                        {
                            DDDParent = PP.intDDDParent,
                            PhoneParent = PP.intPhoneParent,
                            NameParent = PP.txtNameParent,
                            IdPerson = PP.intIdSon,
                            CPF = doc != null ? doc.txtDocumentNumber : ""
                        });
                    }

                    return lstPersonParent;

                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public PersonParent GetParentByParentName(string parentname, int personid)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var personParent = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == personid && x.txtNameParent == parentname);
                    if (personParent != null)
                    {
                        return new PersonParent
                        {
                            DDDParent = personParent.intDDDParent,
                            PhoneParent = personParent.intPhoneParent,
                            NameParent = personParent.txtNameParent,
                            IdPerson = personParent.intIdSon


                        };
                    }
                    else
                        return new PersonParent();

                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public Person GetEmailFromLineNumber(int ddd, int lineNumber)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var personPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intDDD == ddd && p.intPhone == lineNumber && p.bitAtivo == true);
                    return new Person
                    {
                        Email = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == personPhone.intIdPerson).txtEmail,
                        Name = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == personPhone.intIdPerson).txtName
                    };
                }
            }
            catch (Exception ex)
            {
                return new Person();
            }
        }

        public DateTime? GetLastChargeDate(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    if (person != null)
                    {
                        var personParent = ctx.tblChargingHistory
                            .OrderByDescending(x => x.dtePayment)
                            .FirstOrDefault(x => x.intIdCustomer == person.Id);

                        if (personParent != null)
                            return personParent.dtePayment;
                        else
                            return DateTime.MinValue;
                    }
                }

            }
            catch (Exception e)
            {
                return null;
            }
            return null;
        }

        public int GetCustomerId(string document)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {

                    var person = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == document);

                    if (person == null)
                        return 0;
                    else
                        return person.intIdPerson;

                }

            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                            new Utils().GetErrorPostMessage(e.InnerException.ToString()));
            }
        }

        public int SetCrossCustomer(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var documento = person.DocumentNumber.Trim();

                    var tblPerson = ctx.tblPersons
                                        .FirstOrDefault(p => p.txtDocumentNumber == documento && p.bitDelete != true);

                    var phone = person.Phones.FirstOrDefault();

                    if (bool.Equals(tblPerson, null))
                    {
                        var newPerson = new tblPersons
                        {
                            txtName = person.Name.Trim(),
                            txtDocumentNumber = documento,
                            dteRegister = DateTime.Now,
                            txtEmail = person.Email,
                            intIdRole = 0,
                            txtPassword = person.Password,
                            bitSenhaCadastrada = true,
                            bitDadosPessoaisCadastrados = true,
                            txtDefaultWAPhones = phone.CountryCode + phone.DDD + phone.Number
                        };

                        ctx.tblPersons.Add(newPerson);
                        ctx.SaveChanges();

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            intIdPerson = newPerson.intIdPerson,
                            intCountryCode = Convert.ToInt32(phone.CountryCode),
                            intDDD = Convert.ToInt32(phone.DDD),
                            intPhone = Convert.ToInt64(phone.Number),
                            bitPhoneClube = false,
                            bitAtivo = true,
                            intIdPlan = 0
                        });

                        ctx.SaveChanges();

                        ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                        {
                            txtCep = "22793081",
                            intIdPerson = newPerson.intIdPerson,
                            intStreetNumber = 3434,
                            txtCity = "Rio de Janeiro",
                            txtCountry = "Brasil",
                            txtNeighborhood = "Barra da Tijuca",
                            txtState = "RJ",
                            txtStreet = "Avenida das americas",
                            txtComplement = "305 bloco 2",
                            intAdressType = Constants.EnderecoCobranca
                        });
                        ctx.SaveChanges();

                        return newPerson.intIdPerson;
                    }
                    else
                    {
                        throw new HttpResponseException(
                                                    new Utils().GetErrorPostMessage("Cliente já cadastrado no Foneclube"));
                    }

                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int SetCrossCustomer(Person person, bool cadastroRapido)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var documento = person.DocumentNumber.Trim();

                    var tblPerson = ctx.tblPersons
                                        .FirstOrDefault(p => p.txtDocumentNumber == documento && p.bitDelete != true);


                    if (bool.Equals(tblPerson, null))
                    {
                        var newPerson = new tblPersons
                        {
                            txtName = person.Name.Trim(),
                            txtDocumentNumber = documento,
                            dteRegister = DateTime.Now,
                            txtEmail = person.Email,
                            intIdRole = 0,
                            txtPassword = person.Password,
                            bitDadosPessoaisCadastrados = true,
                            bitSenhaCadastrada = true
                        };

                        ctx.tblPersons.Add(newPerson);
                        ctx.SaveChanges();

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            intIdPerson = newPerson.intIdPerson,
                            intDDD = Convert.ToInt32(person.Phones.FirstOrDefault().DDD),
                            intPhone = Convert.ToInt32(person.Phones.FirstOrDefault().Number),
                            bitPhoneClube = false,
                            bitAtivo = true,
                            intIdPlan = 0
                        });

                        ctx.SaveChanges();
                        return newPerson.intIdPerson;
                    }
                    else
                    {
                        throw new HttpResponseException(
                                                    new Utils().GetErrorPostMessage("Cliente já cadastrado no Foneclube"));
                    }

                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int SetPartialCustomer(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var documento = person.DocumentNumber.Trim();
                    var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == documento);
                    //var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == documento && p.bitDelete != true);



                    if (bool.Equals(tblPerson, null))
                    {
                        var newPerson = new tblPersons
                        {
                            txtName = person.Name.Trim(),
                            txtDocumentNumber = documento,
                            dteRegister = DateTime.Now,
                            txtEmail = person.Email,
                            intIdRole = 0
                        };

                        ctx.tblPersons.Add(newPerson);
                        ctx.SaveChanges();

                        if (person.IdParent != -1)
                            SetPersonParent(newPerson.intIdPerson, person.IdParent, ctx);

                        SetAssociationPerson(new Person { Id = newPerson.intIdPerson });


                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            intIdPerson = newPerson.intIdPerson,
                            intDDD = Convert.ToInt32(person.Phones.FirstOrDefault().DDD),
                            intPhone = Convert.ToInt32(person.Phones.FirstOrDefault().Number),
                            bitPhoneClube = false,
                            bitAtivo = true,
                            intIdPlan = 0
                        });





                        ctx.SaveChanges();


                        try
                        {
                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                            {
                                txtCep = person.Adresses.FirstOrDefault().Cep,
                                intIdPerson = newPerson.intIdPerson,
                                intStreetNumber = Convert.ToInt32(person.Adresses.FirstOrDefault().StreetNumber),
                                txtCity = person.Adresses.FirstOrDefault().City,
                                txtCountry = person.Adresses.FirstOrDefault().Country,
                                txtNeighborhood = person.Adresses.FirstOrDefault().Neighborhood,
                                txtState = person.Adresses.FirstOrDefault().State,
                                txtStreet = person.Adresses.FirstOrDefault().Street
                            });

                            ctx.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //faz ação de log e aviso
                        }

                        return newPerson.intIdPerson;
                    }
                    else
                    {
                        tblPerson.bitDelete = false;
                        tblPerson.bitDesativoManual = false;
                        tblPerson.txtName = person.Name.Trim();
                        tblPerson.txtEmail = person.Email;

                        SetAssociationPerson(new Person { Id = tblPerson.intIdPerson });

                        var contactPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == tblPerson.intIdPerson && p.bitPhoneClube == false);
                        var address = ctx.tblPersonsAddresses.FirstOrDefault(p => p.intIdPerson == tblPerson.intIdPerson);

                        if (bool.Equals(contactPhone, null))
                        {
                            ctx.tblPersonsPhones.Add(new tblPersonsPhones
                            {
                                intIdPerson = tblPerson.intIdPerson,
                                intDDD = Convert.ToInt32(person.Phones.FirstOrDefault().DDD),
                                intPhone = Convert.ToInt32(person.Phones.FirstOrDefault().Number),
                                bitPhoneClube = false,
                                bitAtivo = true,
                                intIdPlan = 0
                            });
                        }
                        else
                        {
                            contactPhone.intDDD = Convert.ToInt32(person.Phones.FirstOrDefault().DDD);
                            contactPhone.intPhone = Convert.ToInt32(person.Phones.FirstOrDefault().Number);
                            contactPhone.bitPhoneClube = false;
                            contactPhone.bitAtivo = true;
                        }




                        ctx.SaveChanges();

                        if (bool.Equals(address, null))
                        {
                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                            {
                                txtCep = person.Adresses.FirstOrDefault().Cep,
                                intIdPerson = tblPerson.intIdPerson,
                                intStreetNumber = Convert.ToInt32(person.Adresses.FirstOrDefault().StreetNumber),
                                txtCity = person.Adresses.FirstOrDefault().City,
                                txtCountry = person.Adresses.FirstOrDefault().Country,
                                txtNeighborhood = person.Adresses.FirstOrDefault().Neighborhood,
                                txtState = person.Adresses.FirstOrDefault().State,
                                txtStreet = person.Adresses.FirstOrDefault().Street
                            });
                        }
                        else
                        {
                            address.txtCep = person.Adresses.FirstOrDefault().Cep;
                            address.intIdPerson = tblPerson.intIdPerson;
                            address.intStreetNumber = Convert.ToInt32(person.Adresses.FirstOrDefault().StreetNumber);
                            address.txtCity = person.Adresses.FirstOrDefault().City;
                            address.txtCountry = person.Adresses.FirstOrDefault().Country;
                            address.txtNeighborhood = person.Adresses.FirstOrDefault().Neighborhood;
                            address.txtState = person.Adresses.FirstOrDefault().State;
                            address.txtStreet = person.Adresses.FirstOrDefault().Street;
                        }

                        ctx.SaveChanges();

                        return tblPerson.intIdPerson;
                    }
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int UpdateClientDetails(CustomersListViewModel person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (person.Id > 0)
                    {
                        var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                        int ddd = 0;
                        long phone = 0;
                        var txtDefaultWAPhones = person.IntlPhone.CountryCode + person.IntlPhone.Phone;

                        ddd = Convert.ToInt32(person.IntlPhone.Phone.Substring(0, 2));
                        phone = Convert.ToInt64(person.IntlPhone.Phone.Substring(2));

                        if (bool.Equals(tblPerson, null))
                        {
                            var newPerson = new tblPersons
                            {
                                txtName = person.Name.Trim(),
                                txtDocumentNumber = person.DocumentNumber,
                                dteRegister = DateTime.Now,
                                txtEmail = person.Email,
                                txtDefaultWAPhones = person.Telefone,
                                intIdRole = 0
                            };

                            if (person.Desativo.HasValue)
                                newPerson.bitDesativoManual = person.Desativo.Value;

                            ctx.tblPersons.Add(newPerson);
                            ctx.SaveChanges();

                            if (person.ParentId != -1)
                                SetPersonParent(newPerson.intIdPerson, person.ParentId, ctx);

                            SetAssociationPerson(new Person { Id = newPerson.intIdPerson });

                            ctx.tblPersonsPhones.Add(new tblPersonsPhones
                            {
                                intIdPerson = newPerson.intIdPerson,
                                intDDD = ddd,
                                intPhone = phone,
                                bitPhoneClube = false,
                                bitAtivo = true,
                                intIdPlan = 0
                            });
                            ctx.SaveChanges();

                            try
                            {
                                ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                {
                                    txtCep = person.Address.Cep,
                                    intIdPerson = newPerson.intIdPerson,
                                    intStreetNumber = Convert.ToInt32(person.Address.StreetNumber),
                                    txtCity = person.Address.City,
                                    txtCountry = "Brasil",
                                    txtNeighborhood = person.Address.Neighborhood,
                                    txtState = person.Address.State,
                                    txtStreet = person.Address.Street,
                                    intAdressType = Constants.EnderecoEntrega
                                });

                                ctx.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                LogHelper.LogMessageOld(1, "Update Client Details error : {0}" + e.ToString());
                            }

                            return newPerson.intIdPerson;
                        }
                        else
                        {
                            if (person.Desativo.HasValue)
                                tblPerson.bitDesativoManual = person.Desativo.Value;
                            tblPerson.txtName = person.Name.Trim();
                            tblPerson.txtEmail = person.Email;
                            tblPerson.txtDefaultWAPhones = txtDefaultWAPhones;
                            tblPerson.txtDocumentNumber = person.DocumentNumber;
                            ctx.SaveChanges();

                            if (person.ParentId != -1)
                                SetPersonParent(tblPerson.intIdPerson, person.ParentId, ctx);

                            SetAssociationPerson(new Person { Id = tblPerson.intIdPerson });

                            var contactPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == tblPerson.intIdPerson && p.bitPhoneClube == false);
                            var address = ctx.tblPersonsAddresses.FirstOrDefault(p => p.intIdPerson == tblPerson.intIdPerson);

                            if (bool.Equals(contactPhone, null))
                            {
                                ctx.tblPersonsPhones.Add(new tblPersonsPhones
                                {
                                    intIdPerson = tblPerson.intIdPerson,
                                    intDDD = ddd,
                                    intPhone = phone,
                                    bitPhoneClube = false,
                                    bitAtivo = true,
                                    intIdPlan = 0
                                });
                            }
                            else
                            {
                                contactPhone.intDDD = ddd;
                                contactPhone.intPhone = phone;
                                contactPhone.bitPhoneClube = false;
                                contactPhone.bitAtivo = true;
                            }
                            ctx.SaveChanges();

                            if (bool.Equals(address, null))
                            {
                                ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                {
                                    txtCep = person.Address.Cep,
                                    intIdPerson = tblPerson.intIdPerson,
                                    intStreetNumber = Convert.ToInt32(person.Address.StreetNumber),
                                    txtCity = person.Address.City,
                                    txtCountry = "Brasil",
                                    txtNeighborhood = person.Address.Neighborhood,
                                    txtState = person.Address.State,
                                    txtStreet = person.Address.Street,
                                    txtComplement = person.Address.Complement,
                                    intAdressType = Constants.EnderecoEntrega
                                });
                            }
                            else
                            {
                                address.txtCep = person.Address.Cep;
                                address.txtComplement = person.Address.Complement;
                                address.intIdPerson = tblPerson.intIdPerson;
                                address.intStreetNumber = Convert.ToInt32(person.Address.StreetNumber);
                                address.txtCity = person.Address.City;
                                address.txtCountry = "Brasil";
                                address.txtNeighborhood = person.Address.Neighborhood;
                                address.txtState = person.Address.State;
                                address.txtStreet = person.Address.Street;
                                address.intAdressType = Constants.EnderecoEntrega;
                            }

                            ctx.SaveChanges();

                            return tblPerson.intIdPerson;
                        }
                    }
                    else
                    {
                        LogHelper.LogMessageOld(1, "PersonId is 0");
                        return 0;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.LogMessageOld(1, "Update Client Details error : {0}" + e.ToString());
                return 0;
            }
        }

        public int UpdateClientLineDetails(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var contactPhone = ctx.tblPersonsPhones.FirstOrDefault(x => x.intId == phone.Id);
                    if (contactPhone != null)
                    {
                        contactPhone.intDDD = Convert.ToInt32(phone.DDD);
                        contactPhone.intPhone = Convert.ToInt64(phone.Number);
                        contactPhone.txtPortNumber = phone.PortNumber;
                        contactPhone.bitAtivo = phone.LinhaAtiva;
                        contactPhone.txtNickname = phone.NickName;
                        contactPhone.txtICCID = phone.ICCID;
                        contactPhone.intIdPlan = phone.IdPlanOption;
                        contactPhone.intAmmoutPrecoVip = phone.AmmountPrecoVip;
                        ctx.SaveChanges();
                    }
                    return 1;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int SoftDeleteLine(Phone phone)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var contactPhone = ctx.tblPersonsPhones.FirstOrDefault(x => x.intId == phone.Id);
                    if (contactPhone != null)
                    {
                        contactPhone.bitDelete = phone.Delete;
                        contactPhone.bitAtivo = false;
                        ctx.SaveChanges();
                    }
                    return 1;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public bool SetPersonParent(int intIdPerson, int? idParent, FoneClubeContext ctx)
        {
            try
            {
                var tblPersonsParents = new tblPersonsParents
                {
                    intIdSon = intIdPerson,
                    intIdParent = Convert.ToInt32(idParent),
                    dteCadastro = DateTime.Now
                };
                try
                {
                    var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == idParent);
                    var phone = ctx.tblPersonsPhones
                        .FirstOrDefault(p => p.intIdPerson == idParent && p.bitPhoneClube == true && p.bitAtivo == true);

                    tblPersonsParents.txtNameParent = pessoa.txtName;
                    tblPersonsParents.intDDDParent = phone.intDDD;
                    tblPersonsParents.intPhoneParent = phone.intPhone;
                }
                catch (Exception) { }

                //ctx.AtualizaDadosPais();

                ctx.tblPersonsParents.Add(tblPersonsParents);

                ctx.SaveChanges();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SetPersonAtivity(Person person)
        {
            try
            {
                if (person == null)
                    return false;

                using (var ctx = new FoneClubeContext())
                {
                    var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);
                    tblPerson.bitDesativoManual = person.Desativo;

                    if (Convert.ToBoolean(person.Desativo))
                        SetDisasociationPerson(person);
                    else
                        SetAssociationPerson(person);

                    ctx.SaveChanges();
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SetCustomerNextAction(Person person)
        {
            try
            {
                if (person == null)
                    return false;

                using (var ctx = new FoneClubeContext())
                {
                    var tblPerson = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == person.Id);

                    if (person.NextActionDate.HasValue)
                        tblPerson.dteNextActionDate = person.NextActionDate.Value;

                    if (!string.IsNullOrEmpty(person.NextActionText))
                        tblPerson.txtNextAction = person.NextActionText;

                    ctx.SaveChanges();
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SetAssociationPerson(Person person)
        {
            try
            {
                if (person == null)
                    return false;

                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblPersonAssociationHistory.Add(new tblPersonAssociationHistory
                    {
                        txtDocument = person.DocumentNumber,
                        intIdStatus = 2,
                        intEventType = 2,
                        intIdPerson = person.Id,
                        dteChange = DateTime.Now,
                        dteEntrada = DateTime.Now
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

        public bool SetDisasociationPerson(Person person)
        {
            try
            {
                if (person == null)
                    return false;

                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblPersonAssociationHistory.Add(new tblPersonAssociationHistory
                    {
                        txtDocument = person.DocumentNumber,
                        intIdStatus = 1,
                        intEventType = 1,
                        intIdPerson = person.Id,
                        dteChange = DateTime.Now,
                        dteSaida = DateTime.Now
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

        public bool CustomerActiveExists(string document)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var pessoa = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == document);
                    if (bool.Equals(pessoa, null))
                        return false;
                    else
                    {
                        if (Convert.ToBoolean(pessoa.bitDelete) || Convert.ToBoolean(pessoa.bitDesativoManual))
                        {
                            return false;
                        }
                        else
                            return true;

                    }

                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SyncCustomers()
        {
            try
            {
                using (var ctxLoja = new LojaDBEntities())
                {
                    var nopCustomers = ctxLoja.Customer.Where(c => c.Active == true && c.Deleted == false)
                        .ToList();

                    var emailsLoja = nopCustomers.Select(c => c.Email).ToList();

                    using (var ctx = new FoneClubeContext())
                    {
                        foreach (var email in emailsLoja)
                        {
                            var pessoa = ctx.tblPersons.FirstOrDefault(p => p.txtEmail == email);
                            if (!bool.Equals(pessoa, null))
                            {
                                var customerLoja = nopCustomers.FirstOrDefault(c => c.Email == email);
                                var atributosCustomer = ctxLoja.GenericAttribute.Where(g => g.EntityId == customerLoja.Id).ToList();
                                if (atributosCustomer.Count > 0)
                                {
                                    var atributoId = atributosCustomer.FirstOrDefault(a => a.Key == "IdErp");
                                    if (!bool.Equals(atributoId, null))
                                    {
                                        var idFoneclube = atributoId.Value;
                                    }
                                    else
                                    {
                                        ctxLoja.GenericAttribute.Add(new GenericAttribute
                                        {
                                            EntityId = customerLoja.Id,
                                            KeyGroup = "Customer",
                                            Key = "IdErp",
                                            StoreId = 0,
                                            Value = pessoa.intIdPerson.ToString()
                                        });

                                        ctxLoja.SaveChanges();
                                    }
                                }
                                else
                                {
                                    ctxLoja.GenericAttribute.Add(new GenericAttribute
                                    {
                                        EntityId = customerLoja.Id,
                                        KeyGroup = "Customer",
                                        Key = "IdErp",
                                        StoreId = 0,
                                        Value = pessoa.intIdPerson.ToString()
                                    });

                                    ctxLoja.SaveChanges();
                                }



                            }
                        }



                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public CheckoutPagarMe GetCheckoutPerson(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var pessoa = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == matricula);

                    Adress address;
                    Phone phone;

                    var modeloAddress = new Adress
                    {
                        Cep = "22793081",
                        Street = "Avenida das Américas",
                        StreetNumber = "7837",
                        Neighborhood = "Barra da Tijuca"
                    };

                    var modeloPhone = new Phone
                    {
                        DDD = "21",
                        Number = "982008200"
                    };

                    try
                    {
                        address = ctx.tblPersonsAddresses.Where(p => p.intIdPerson == matricula).Select(a => new Adress
                        {
                            Street = a.txtStreet,
                            StreetNumber = a.intStreetNumber.ToString(),
                            Neighborhood = a.txtNeighborhood,
                            Cep = a.txtCep
                        }).FirstOrDefault();
                    }
                    catch (Exception)
                    {
                        address = modeloAddress;
                    }

                    if (address is null)
                        address = modeloAddress;

                    try
                    {
                        phone = ctx.tblPersonsPhones.Where(p => p.intIdPerson == matricula && p.bitPhoneClube == false).Select(a => new Phone
                        {
                            DDD = a.intDDD.ToString(),
                            Number = a.intPhone.ToString()
                        }).FirstOrDefault();

                        if (phone is null)
                            phone = modeloPhone;
                    }
                    catch (Exception)
                    {
                        phone = modeloPhone;
                    }


                    if (string.IsNullOrEmpty(address.Street) ||
                        string.IsNullOrEmpty(address.Neighborhood) ||
                        string.IsNullOrEmpty(address.StreetNumber) ||
                        string.IsNullOrEmpty(address.Cep))
                        address = modeloAddress;

                    if (string.IsNullOrEmpty(phone.Number) ||
                        string.IsNullOrEmpty(phone.DDD))
                        phone = modeloPhone;

                    var checkout = new CheckoutPagarMe
                    {
                        Nome = pessoa.txtName,
                        DocumentNumber = pessoa.txtDocumentNumber,
                        Email = pessoa.txtEmail,
                        Street = address.Street,
                        StreetNumber = address.StreetNumber,
                        Neighborhood = address.Neighborhood,
                        Zipcode = address.Cep,
                        Ddd = phone.DDD,
                        Number = phone.Number,
                        IdCustomerPagarme = pessoa.intIdPagarme
                    };

                    return checkout;
                }
            }
            catch (Exception e)
            {
                return new CheckoutPagarMe();
            }
        }

        public List<Person> GetActiveCustomers()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblPersons.Where(p => p.bitDesativoManual != true && p.bitDelete != true)
                           .Select(p => new Person
                           {
                               Id = p.intIdPerson,
                               Name = p.txtName,
                               DocumentNumber = p.txtDocumentNumber
                           }).OrderBy(x => x.Name).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<Person>();
            }
        }

        public List<Person> GetActiveCustomersParentList()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var allCustomers = ctx.tblPersons.ToList();

                    var customers = allCustomers.Where(p => p.bitDesativoManual != true && p.bitDelete != true)
                           .Select(p => new Person
                           {
                               Id = p.intIdPerson,
                               Name = p.txtName,
                               DocumentNumber = p.txtDocumentNumber
                           }).OrderBy(x => x.Name).ToList();

                    var parents = ctx.tblPersonsParents.Where(p => p.intIdParent != null).ToList();

                    foreach (var customer in customers)
                    {
                        var parent = parents.FirstOrDefault(p => p.intIdSon == customer.Id);

                        if (!bool.Equals(parent, null))
                        {
                            string name;
                            var person = allCustomers.FirstOrDefault(p => p.intIdPerson == parent.intIdParent.Value);

                            if (bool.Equals(person, null))
                            {
                                name = string.Empty;
                            }
                            else
                            {
                                name = person.txtName;
                            }

                            customer.Pai = new Pai
                            {
                                Id = parent.intIdParent.Value,
                                Name = name
                            };
                        }
                    }

                    return customers;
                }
            }
            catch (Exception e)
            {
                return new List<Person>();
            }
        }

        public bool SetCustomerParent(Person person)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var existeRegistro = ctx.tblPersonsParents.Any(x => x.intIdSon == person.Id);
                    var contactPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == person.Pai.Id && p.bitPhoneClube == false);
                    int? ddd;
                    int? phone;

                    if (bool.Equals(contactPhone, null))
                    {
                        ddd = 21;
                        phone = 222222222;
                    }
                    else
                    {
                        ddd = contactPhone.intDDD;
                        phone = Convert.ToInt32(contactPhone.intPhone);
                    }

                    if (!existeRegistro)
                    {
                        ctx.tblPersonsParents.Add(new tblPersonsParents
                        {
                            intIdSon = person.Id,
                            intDDDParent = ddd,
                            intPhoneParent = phone,
                            txtNameParent = person.Pai.Name,
                            intIdParent = person.Pai.Id,
                            dteCadastro = DateTime.Now
                        });
                    }
                    else
                    {
                        var pessoa = ctx.tblPersonsParents.FirstOrDefault(x => x.intIdSon == person.Id);
                        pessoa.intDDDParent = ddd;
                        pessoa.intPhoneParent = phone;
                        pessoa.txtNameParent = person.NameParent;
                        pessoa.intIdParent = person.Pai.Id;
                        pessoa.dteCadastro = DateTime.Now;
                    }

                    ctx.SaveChanges();
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public int InstaSetParentInfo(PersonParentModel person)
        {
            int idPerson = 0;
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var tblPersons = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == person.CPF);

                    if (tblPersons != null)
                    {
                        tblPersons.txtName = person.Name;
                        tblPersons.txtDefaultWAPhones = "55" + person.WhatsAppNumber;
                        idPerson = tblPersons.intIdPerson;
                        ctx.SaveChanges();

                        var tblphones1 = ctx.tblPersonsPhones.FirstOrDefault(x => x.intDDD == Convert.ToInt32(person.WhatsAppNumber.Substring(0, 2)) && x.intPhone == Convert.ToInt64(person.WhatsAppNumber.Substring(2)));

                        if (tblphones1 == null)
                        {
                            ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                            {
                                intDDD = Convert.ToInt32(person.WhatsAppNumber.Substring(0, 2)) == 0 ? 99 : Convert.ToInt32(person.WhatsAppNumber.Substring(0, 2)),
                                intPhone = Convert.ToInt64(person.WhatsAppNumber.Substring(2)) == 0 ? 999999999 : Convert.ToInt64(person.WhatsAppNumber.Substring(2)),
                                intIdPlan = -1,
                                intIdPerson = idPerson,
                                bitDelete = false,
                                bitPhoneClube = false,
                                bitAtivo = true
                            });
                            ctx.SaveChanges();
                        }
                    }
                    else
                    {
                        CustomerCrossRegisterViewModel objModel = new CustomerCrossRegisterViewModel();
                        objModel.phone = person.WhatsAppNumber;
                        objModel.documento = person.CPF;
                        objModel.documentType = person.CPF.Length > 11 ? "CPF" : "CNPJ";
                        objModel.name = person.Name;
                        objModel.email = person.CPF + "@foneclube.com.br";
                        objModel.password = "000000";
                        objModel.confirmPassword = "000000";

                        var result = InsertNewCustomerRegisterCross(objModel);
                        idPerson = result.IdCliente;

                        ctx.SaveChanges();
                    }

                }
            }
            catch (Exception ex) { }
            return idPerson;
        }
        public bool UpdatePersonBasicInfo(PhoneViewModel person)
        {
            try
            {
                using (var context = new FoneClubeContext())
                {
                    var personData = context.tblPersons
                                            .FirstOrDefault(p => p.intIdPerson == person.PersonId);

                    if (personData != null)
                    {
                        personData.txtName = person.PersonName;
                        personData.txtNickName = person.NickName;
                        personData.txtEmail = person.Email;
                        personData.txtDocumentNumber = person.DocumentNumber;

                        context.SaveChanges();
                        return true;
                    }
                    else
                        return false;

                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool SaveSinglePrice(Person person)
        {
            using (var context = new FoneClubeContext())
            {
                return InsertSinglePrice(person, context);
            }
        }

        public bool SavePersonPhoneNumber(PhoneViewModel phoneModel)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var newPhone = !ctx.tblPersonsPhones.Any(p => p.intId == phoneModel.PersonPhoneId
                    && p.intIdPerson == phoneModel.PersonId);

                    //to fix deprecated attribute, PlanOptionId will be removed
                    phoneModel.PlanOptionId = phoneModel.PlanId;

                    if (newPhone)
                    {
                        var telefone = GetPhoneOwner(new Phone
                        {
                            DDD = phoneModel.DDNumber.ToString(),
                            Number = phoneModel.PhoneNumber.ToString()
                        });

                        if (!bool.Equals(telefone.DocumentNumber, null))
                            throw new HttpResponseException(
                            new Utils().GetErrorPostMessage("Linha ativa em outra conta, use o Get Phone Number pra descobrir qual"));

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            intIdPerson = phoneModel.PersonId,
                            intIdOperator = phoneModel.OperatorId,
                            intIdStatus = phoneModel.StatusId,
                            intIdPlan = phoneModel.PlanId,

                            intDDD = phoneModel.DDNumber,
                            intPhone = phoneModel.PhoneNumber,
                            txtNickname = phoneModel.NickName,

                            bitAtivo = phoneModel.IsActive,
                            bitPhoneClube = phoneModel.IsPhoneClube,
                            bitPortability = phoneModel.IsPortability,
                            bitPrecoVip = phoneModel.IsPrecoVip,

                            intAmmoutPrecoVip = phoneModel.AmoutPrecoVip,
                            dteEntradaLinha = DateTime.Now
                        });

                        var phoneAccess = new PhoneAccess();
                        var propertyPhone = new Phone
                        {
                            DDD = phoneModel.DDNumber.ToString(),
                            Number = phoneModel.PhoneNumber.ToString(),
                            IdPlanOption = phoneModel.PlanOptionId
                        };

                        if (Convert.ToBoolean(phoneModel.IsPhoneClube))
                        {
                            var statusPhone = phoneAccess.GetStatusLinha(phoneModel.PersonId, propertyPhone);

                            if (phoneModel.IsActive && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Ativa)
                            {
                                phoneAccess.InsertHistoricoAtivarLinha(phoneModel.PersonId, propertyPhone);
                            }
                            else if (!phoneModel.IsActive && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Desativa)
                            {
                                phoneAccess.InsertHistoricoDesativarLinha(phoneModel.PersonId, propertyPhone);
                            }
                        }
                    }
                    else
                    {
                        var personPhone = ctx.tblPersonsPhones
                            .FirstOrDefault(p => p.intId == phoneModel.PersonPhoneId
                            && p.intIdPerson == phoneModel.PersonId);


                        personPhone.intDDD = phoneModel.DDNumber;
                        personPhone.intPhone = phoneModel.PhoneNumber;
                        personPhone.intIdOperator = phoneModel.OperatorId;
                        personPhone.intIdPlan = phoneModel.PlanId;
                        personPhone.intAmmoutPrecoVip = phoneModel.AmoutPrecoVip;
                        personPhone.txtNickname = phoneModel.NickName;

                        var phoneAccess = new PhoneAccess();
                        var propertyPhone = new Phone
                        {
                            DDD = phoneModel.DDNumber.ToString(),
                            Number = phoneModel.PhoneNumber.ToString(),
                            IdPlanOption = phoneModel.PlanOptionId,
                            Id = phoneModel.PersonPhoneId
                        };

                        if (Convert.ToBoolean(phoneModel.IsPhoneClube))
                        {
                            var statusPhone = phoneAccess.GetStatusLinha(phoneModel.PersonId, propertyPhone);

                            if (personPhone.intIdPlan != phoneModel.PlanOptionId)
                            {
                                phoneAccess.InsertHistoricoUpdateLinha(phoneModel.PersonId, propertyPhone);
                            }
                            else
                            {
                                if (phoneModel.IsActive && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Ativa)
                                {
                                    phoneAccess.InsertHistoricoAtivarLinha(phoneModel.PersonId, propertyPhone);
                                }

                                else if (!phoneModel.IsActive && (Phone.PhoneStatus)statusPhone != Phone.PhoneStatus.Desativa)
                                {
                                    phoneAccess.InsertHistoricoDesativarLinha(phoneModel.PersonId, propertyPhone);
                                }

                            }
                        }
                    }

                    ctx.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ActivationChangeStatus(int personPhoneId, bool activate)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var personPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intId == personPhoneId);
                    var phone = new Phone
                    {
                        Id = personPhone.intId,
                        DDD = personPhone.intDDD.ToString(),
                        Number = personPhone.intPhone.ToString(),
                        IdPlanOption = personPhone.intIdPlan
                    };

                    if (activate)
                    {
                        personPhone.bitDelete = false;
                        personPhone.bitAtivo = true;

                        new PhoneAccess().InsertHistoricoAtivarLinha(personPhone.intIdPerson.GetValueOrDefault(), phone);
                    }
                    else
                    {
                        personPhone.bitDelete = true;
                        personPhone.bitAtivo = false;
                        new PhoneAccess().InsertHistoricoDesativarLinha(personPhone.intIdPerson.GetValueOrDefault(), phone);
                        //new PhoneAccess().InsertHistoricoDesligarLinha(personPhone.intIdPerson.GetValueOrDefault(), phone);
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

        public class RegisterResponse
        {
            public HttpStatusCode Status { get; set; }
            public int IdCliente { get; set; }
            public String Message { get; set; }
        }

        public class InstaChargeResponse
        {
            public HttpStatusCode Status { get; set; }
            public int IdCliente { get; set; }
            public String Message { get; set; }
        }



        public bool SetCustomerLink(int idPerson, string link)
        {

            using (var ctx = new FoneClubeContext())
            {
                var persons = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == idPerson);
                return true;
            }
        }

        public bool SetAllCustomersLinks()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var persons = ctx.tblPersons.Where(p => p.bitDelete != true).ToList();
                    var tblLinks = ctx.tblPersosAffiliateLinks.ToList();
                    var tblLinksNull = ctx.tblPersosAffiliateLinks.Where(l => l.txtBlinkLink == null).ToList();

                    foreach (var person in persons)
                    {
                        //var hasLink = tblLinks.Any(l => l.intIdPerson == person.intIdPerson);
                        var link = tblLinks.FirstOrDefault(l => l.intIdPerson == person.intIdPerson);

                        if (link == null)
                        {
                            var customerNames = GetName(person.txtName);
                            var afiliateLink = new Affiliates().GetReferralLink(person.intIdPerson);
                            var maskName = customerNames.Name.ToLower() + "-" + customerNames.LastWord.ToLower();
                            var blinkService = new BlinkIntegrationManager().CreateLinkIndication(afiliateLink, maskName);

                            ctx.tblPersosAffiliateLinks.Add(new tblPersosAffiliateLinks
                            {
                                dteRegister = DateTime.Now,
                                intIdPerson = person.intIdPerson,
                                txtMaskName = maskName,
                                txtBlinkLink = blinkService,
                                txtOriginalLink = afiliateLink
                            });

                            ctx.SaveChanges();
                        }


                    }

                    foreach (var linkNull in tblLinksNull)
                    {
                        if (linkNull.intIdPerson == 5026)
                        {
                            var person = persons.FirstOrDefault(p => p.intIdPerson == linkNull.intIdPerson);

                            var customerNames = GetName(person.txtName);
                            var afiliateLink = new Affiliates().GetReferralLink(person.intIdPerson);
                            var maskName = customerNames.Name.ToLower() + "-" + customerNames.LastWord.ToLower();
                            var blinkService = new BlinkIntegrationManager().CreateLinkIndication(afiliateLink, maskName);

                            if (blinkService != null)
                            {
                                try
                                {
                                    ctx.tblPersosAffiliateLinks.Add(new tblPersosAffiliateLinks
                                    {
                                        dteRegister = DateTime.Now,
                                        intIdPerson = person.intIdPerson,
                                        txtMaskName = maskName,
                                        txtBlinkLink = blinkService,
                                        txtOriginalLink = afiliateLink
                                    });

                                    ctx.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                    var teste = e;
                                }

                            }
                        }



                    }




                    return true;
                }
            }
            catch (Exception)
            {
                throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Ocorreu um erro na tentativa de cadastro, entre em contato no número: +55 21 97218-7932. ( Celular e Whatsapp ) "));
            }
        }

        public string GetMaskName(string name, string previousName)
        {
            var verification = previousName.Replace(name, string.Empty);

            if (string.IsNullOrEmpty(verification))
                name = name + 1;
            else
                name = name + (Convert.ToInt32(verification) + 1);

            return name;
        }



        private bool SaveGenericAttributes(LojaDBEntities ctx, CustomerCrossRegisterViewModel customeRegisterViewModel, int newStoreCustomerId)
        {
            try
            {
                var customerNames = GetName(customeRegisterViewModel.name);

                ctx.GenericAttribute.Add(new GenericAttribute
                {
                    EntityId = newStoreCustomerId,
                    KeyGroup = "Customer",
                    StoreId = 0,
                    Key = "FirstName",
                    Value = customerNames.Name
                });

                ctx.GenericAttribute.Add(new GenericAttribute
                {
                    EntityId = newStoreCustomerId,
                    KeyGroup = "Customer",
                    StoreId = 0,
                    Key = "LastName",
                    Value = customerNames.LastName
                });

                ctx.GenericAttribute.Add(new GenericAttribute
                {
                    EntityId = newStoreCustomerId,
                    KeyGroup = "Customer",
                    StoreId = 0,
                    Key = "Customer_CPF",
                    Value = customeRegisterViewModel.documento
                });

                ctx.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public CustomerViewModel GetName(string name)
        {
            try
            {
                var count = 0;
                var customer = new CustomerViewModel();

                foreach (var palavra in name.Split(' '))
                {
                    count++;

                    if (count == 1)
                        customer.Name = palavra;
                    else
                        customer.LastName += palavra + " ";

                    customer.LastWord = palavra;
                }

                return customer;
            }
            catch (Exception)
            {
                return new CustomerViewModel { Name = name };
            }

        }

        private bool SaveGenericAtibruteIdERP(LojaDBEntities ctx, int idNovoClienteLoja, string idNovoClienteFC)
        {
            try
            {
                ctx.GenericAttribute.Add(new GenericAttribute
                {
                    EntityId = idNovoClienteLoja,
                    KeyGroup = "Customer",
                    Key = "IdErp",
                    StoreId = 0,
                    Value = idNovoClienteFC
                });

                ctx.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SavePersonLojaRegister(CustomerWoocommerce customeRegisterViewModel)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var document = string.IsNullOrEmpty(customeRegisterViewModel.billing.cpf) ? customeRegisterViewModel.billing.cnpj : customeRegisterViewModel.billing.cpf;
                    var existentCustomer = ctx.tblPersons.Any(p => p.txtDocumentNumber == document && p.bitDelete != true && p.txtDocumentNumber != "");

                    if (existentCustomer)
                        return UpdatePersonLojaRegister(customeRegisterViewModel);

                    var cliente = new tblPersons
                    {
                        dteRegister = DateTime.Now,
                        txtEmail = customeRegisterViewModel.email,
                        txtName = customeRegisterViewModel.first_name + " " + customeRegisterViewModel.last_name,
                        txtDocumentNumber = document,
                        intIdLoja = customeRegisterViewModel.id
                    };

                    ctx.tblPersons.Add(cliente);
                    ctx.SaveChanges();

                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        txtCep = customeRegisterViewModel.billing.postcode.Replace("-", ""),
                        intIdPerson = cliente.intIdPerson,
                        intStreetNumber = string.IsNullOrEmpty(customeRegisterViewModel.billing.number) ? 0 : Convert.ToInt32(customeRegisterViewModel.billing.number),
                        txtCity = customeRegisterViewModel.billing.city,
                        txtCountry = customeRegisterViewModel.billing.country,
                        txtNeighborhood = customeRegisterViewModel.billing.neighborhood,
                        txtState = customeRegisterViewModel.billing.state,
                        txtStreet = customeRegisterViewModel.billing.address_1,
                        txtComplement = customeRegisterViewModel.billing.address_2,
                        intAdressType = Constants.EnderecoCobranca
                    });

                    ctx.SaveChanges();

                    //ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    //{
                    //    txtCep = customeRegisterViewModel.shipping.postcode.Replace("-", ""),
                    //    intIdPerson = cliente.intIdPerson,
                    //    intStreetNumber = string.IsNullOrEmpty(customeRegisterViewModel.shipping.number) ? 0 : Convert.ToInt32(customeRegisterViewModel.shipping.number),
                    //    txtCity = customeRegisterViewModel.shipping.city,
                    //    txtCountry = customeRegisterViewModel.shipping.country,
                    //    txtNeighborhood = customeRegisterViewModel.shipping.neighborhood,
                    //    txtState = customeRegisterViewModel.shipping.state,
                    //    txtStreet = customeRegisterViewModel.shipping.address_1,
                    //    txtComplement = customeRegisterViewModel.shipping.address_2,
                    //    intAdressType = Constants.EnderecoEntrega
                    //});

                    try
                    {
                        var cellphone = customeRegisterViewModel.billing.cellphone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            bitPhoneClube = false,
                            bitAtivo = true,
                            intDDD = Convert.ToInt32(cellphone.Substring(0, 2)),
                            intPhone = Convert.ToInt32(cellphone.Substring(2)),
                            intIdPerson = cliente.intIdPerson,
                            intContactType = Constants.TelefoneContatoCelular
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception) { }


                    try
                    {
                        var phone = customeRegisterViewModel.billing.phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

                        ctx.tblPersonsPhones.Add(new tblPersonsPhones
                        {
                            bitPhoneClube = false,
                            bitAtivo = true,
                            intDDD = Convert.ToInt32(phone.Substring(0, 2)),
                            intPhone = Convert.ToInt32(phone.Substring(2)),
                            intIdPerson = cliente.intIdPerson,
                            intContactType = Constants.TelefoneContatoFixo
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception e)
                    {

                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool UpdatePersonLojaRegister(CustomerWoocommerce customeRegisterViewModel)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var document = string.IsNullOrEmpty(customeRegisterViewModel.billing.cpf) ? customeRegisterViewModel.billing.cnpj : customeRegisterViewModel.billing.cpf;

                    var existentCustomer = ctx.tblPersons.Any(p => p.txtDocumentNumber == document && p.bitDelete != true);
                    var customer = ctx.tblPersons.FirstOrDefault(p => p.intIdLoja == customeRegisterViewModel.id);

                    if (existentCustomer && customer == null)
                    {
                        customer = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == document && p.bitDelete != true);
                    }

                    customer.txtEmail = customeRegisterViewModel.email;
                    customer.txtName = customeRegisterViewModel.first_name + " " + customeRegisterViewModel.last_name;
                    customer.txtDocumentNumber = document;
                    customer.intIdLoja = customeRegisterViewModel.id;


                    ctx.SaveChanges();

                    var address = ctx.tblPersonsAddresses.FirstOrDefault(pa => pa.intIdPerson == customer.intIdPerson);

                    address.txtCep = customeRegisterViewModel.billing.postcode.Replace("-", "");
                    address.intStreetNumber = string.IsNullOrEmpty(customeRegisterViewModel.billing.number) ? 0 : Convert.ToInt32(customeRegisterViewModel.billing.number);
                    address.txtCity = customeRegisterViewModel.billing.city;
                    address.txtCountry = customeRegisterViewModel.billing.country;
                    address.txtNeighborhood = customeRegisterViewModel.billing.neighborhood;
                    address.txtState = customeRegisterViewModel.billing.state;
                    address.txtStreet = customeRegisterViewModel.billing.address_1;
                    address.txtComplement = customeRegisterViewModel.billing.address_2;


                    var hasContactPhoneWithoutType = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == customer.intIdPerson && p.bitPhoneClube == false && p.intContactType == null);

                    try
                    {
                        var cellphone = customeRegisterViewModel.billing.cellphone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
                        var tblCellphone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == customer.intIdPerson && p.bitPhoneClube == false && p.intContactType == Constants.TelefoneContatoCelular);

                        if (tblCellphone != null)
                        {
                            tblCellphone.intDDD = Convert.ToInt32(cellphone.Substring(0, 2));
                            tblCellphone.intPhone = Convert.ToInt32(cellphone.Substring(2));
                        }
                        else
                        {
                            hasContactPhoneWithoutType.intDDD = Convert.ToInt32(cellphone.Substring(0, 2));
                            hasContactPhoneWithoutType.intPhone = Convert.ToInt32(cellphone.Substring(2));
                            hasContactPhoneWithoutType.intContactType = Constants.TelefoneContatoCelular;
                        }

                        ctx.SaveChanges();
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        var phone = customeRegisterViewModel.billing.phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
                        var tblPhone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == customer.intIdPerson && p.bitPhoneClube == false && p.intContactType == Constants.TelefoneContatoFixo); ;

                        if (tblPhone != null)
                        {
                            tblPhone.intDDD = Convert.ToInt32(phone.Substring(0, 2));
                            tblPhone.intPhone = Convert.ToInt32(phone.Substring(2));
                        }
                        else
                        {
                            //nunca cai aqui
                        }

                        ctx.SaveChanges();
                    }
                    catch (Exception)
                    {
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public RegisterResponse InsertNewCustomerCross(CustomerCrossRegisterViewModel customeRegisterViewModel)
        {
            //return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Email já cadastrado no Foneclube." };

            var idNovoCliente = 0;
            var idPai = 4555; //foneclube
            var intIdCustomerLog = 0;
            bool saveGenericIdERP = true;
            bool saveGenericAttributes = true;
            bool saveStoreBasic = true;
            bool sendEmail;
            string blinkService = string.Empty;
            string afiliateLink = string.Empty;

            try
            {

                var hashedPassword = new Security().EncryptPassword(customeRegisterViewModel.documento);

                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        if (customeRegisterViewModel.idPai != null)
                            idPai = Convert.ToInt32(customeRegisterViewModel.idPai);
                    }
                    catch (Exception) { }


                    var log = new tblRegisterCustomersLog
                    {
                        dteRegister = DateTime.Now,
                        txtDocument = customeRegisterViewModel.documento,
                        txtEmail = customeRegisterViewModel.email,
                        txtName = customeRegisterViewModel.name,
                        txtPassword = hashedPassword.Password,
                        txtPhone = customeRegisterViewModel.phone,
                        intDocumentType = customeRegisterViewModel.documentType == "CPF" ? 1 : 2,
                        idPai = idPai,
                        guidId = Guid.NewGuid()
                    };

                    ctx.tblRegisterCustomersLog.Add(log);
                    ctx.SaveChanges();

                    var hasPerson = ctx.tblPersons
                        .Any(p => p.txtDocumentNumber == customeRegisterViewModel.documento && p.bitDelete != true);


                    var hasEmail = ctx.tblPersons
                        .Any(p => p.txtEmail == customeRegisterViewModel.email.Trim() && p.bitDelete != true);

                    if (hasEmail)
                        return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Email já cadastrado no Foneclube." };

                    if (hasPerson)
                        return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Cliente já cadastrado." };

                    var phones = new List<Phone>();
                    phones.Add(new Phone
                    {
                        DDD = customeRegisterViewModel.phone.Substring(0, 2),
                        Number = customeRegisterViewModel.phone.Substring(2)
                    });

                    idNovoCliente = SetCrossCustomer(new Person
                    {
                        DocumentNumber = customeRegisterViewModel.documento,
                        Email = customeRegisterViewModel.email,
                        Name = customeRegisterViewModel.name,
                        Phones = phones
                    });

                    ctx.SaveChanges();

                    intIdCustomerLog = log.intIdCustomer;

                    ctx.tblPersonsParents.Add(new tblPersonsParents
                    {
                        dteCadastro = DateTime.Now,
                        intIdSon = idNovoCliente,
                        intIdParent = idPai
                    });

                    ctx.SaveChanges();


                    WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                    var wmessage = new WhatsAppMessage()
                    {
                        Message = "New web registration - Name : " + customeRegisterViewModel.name + " CPF: " + customeRegisterViewModel.documento,
                        ClientIds = "5521982008200,5521981908190"
                    };
                    whatsAppAccess.SendMessage(wmessage);
                }

                using (var ctx = new LojaDBEntities())
                {
                    Customer newStoreCustomer = new Customer();

                    try
                    {
                        newStoreCustomer = new Customer
                        {
                            CustomerGuid = Guid.NewGuid(),
                            Username = customeRegisterViewModel.documento,
                            Email = customeRegisterViewModel.email,
                            IsTaxExempt = false,
                            AffiliateId = 100000,
                            VendorId = 0,
                            HasShoppingCartItems = false,
                            RequireReLogin = false,
                            FailedLoginAttempts = 0,
                            Active = true,
                            IsSystemAccount = false,
                            Deleted = false,
                            CreatedOnUtc = DateTime.Now,
                            LastActivityDateUtc = DateTime.Now,
                            RegisteredInStoreId = 1,
                            BillingAddress_Id = null,
                            ShippingAddress_Id = null
                        };

                        ctx.Customer.Add(newStoreCustomer);
                        ctx.SaveChanges();

                        ctx.CustomerPassword.Add(new CustomerPassword
                        {
                            CustomerId = newStoreCustomer.Id,
                            PasswordFormatId = 1,
                            Password = hashedPassword.Password,
                            PasswordSalt = hashedPassword.SaltKey,
                            CreatedOnUtc = DateTime.Now
                        });

                        var customerId = 3;
                        ctx.Customer_CustomerRole_Mapping.Add(new Customer_CustomerRole_Mapping
                        {
                            Customer_Id = newStoreCustomer.Id,
                            CustomerRole_Id = customerId
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception)
                    {
                        saveStoreBasic = false;
                    }

                    saveGenericIdERP = SaveGenericAtibruteIdERP(ctx, newStoreCustomer.Id, idNovoCliente.ToString());
                    saveGenericAttributes = SaveGenericAttributes(ctx, customeRegisterViewModel, newStoreCustomer.Id);
                }


                if (saveStoreBasic != true || saveGenericAttributes != true || saveGenericIdERP != true)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var log = ctx.tblRegisterCustomersLog.FirstOrDefault(r => r.intIdCustomer == intIdCustomerLog);
                        log.txtLog = saveStoreBasic.ToString() + " " + saveGenericAttributes.ToString() + " " + saveGenericIdERP.ToString();
                        ctx.SaveChanges();
                    }
                }

                try
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var customerNames = GetName(customeRegisterViewModel.name);
                        afiliateLink = new Affiliates().GetReferralLink(idNovoCliente);

                        var maskName = customerNames.Name.ToLower() + "-" + customerNames.LastWord.ToLower();
                        var previousMaskName = ctx.tblPersosAffiliateLinks.FirstOrDefault(p => p.txtMaskName == maskName);

                        if (previousMaskName != null)
                            maskName = new ProfileAccess().GetMaskName(maskName, previousMaskName.txtMaskName);

                        blinkService = new BlinkIntegrationManager().CreateLinkIndication(afiliateLink, maskName);

                        var email = new Email
                        {
                            TemplateType = 24,
                            To = customeRegisterViewModel.email,
                            TargetName = customerNames.Name,
                            TargetTextBlue = "<a href='" + blinkService + "'>" + blinkService.Replace("http://", string.Empty) + "</a>",
                            TargetSecondaryText = string.Empty,
                            DiscountPrice = "<a href='https://loja.foneclube.com.br/login'>loja.foneclube.com.br/login</a>"
                        };

                        sendEmail = new EmailAccess().SendEmailDinamic(email);

                        try
                        {
                            ctx.tblPersosAffiliateLinks.Add(new tblPersosAffiliateLinks
                            {
                                dteRegister = DateTime.Now,
                                intIdPerson = idNovoCliente,
                                txtMaskName = maskName,
                                txtBlinkLink = blinkService,
                                txtOriginalLink = afiliateLink
                            });

                            ctx.SaveChanges();
                        }
                        catch (Exception)
                        {

                        }
                    }

                    try
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var log = ctx.tblRegisterCustomersLog.FirstOrDefault(r => r.intIdCustomer == intIdCustomerLog);

                            if (log.txtLog != null)
                                log.txtLog = log.txtLog.ToString() + sendEmail.ToString() + "_" + afiliateLink + "_" + idNovoCliente;
                            else
                                log.txtLog = sendEmail.ToString() + "_" + afiliateLink + "_" + idNovoCliente;

                            ctx.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception)
                {

                }


                return new RegisterResponse { Status = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                     new Utils().GetErrorPostMessage("Ocorreu um erro na tentativa de cadastro, entre em contato no número: +55 21 97218-7932. ( Celular e Whatsapp ) "));
            }

        }

        public RegisterResponse InsertNewCustomerRegisterCross(CustomerCrossRegisterViewModel customeRegisterViewModel)
        {
            //return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Email já cadastrado no Foneclube." };

            var idNovoCliente = 0;
            var idPai = 4555; //foneclube
            var intIdCustomerLog = 0;
            bool sendEmail;
            string blinkService = string.Empty;
            string afiliateLink = string.Empty;

            try
            {
                var hashedPassword = new Security().EncryptPassword(customeRegisterViewModel.documento);

                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        if (customeRegisterViewModel.idPai != null)
                            idPai = Convert.ToInt32(customeRegisterViewModel.idPai);
                    }
                    catch (Exception) { }

                    string documentoDuplicado = string.Empty;
                    var hasDocument = ctx.tblPersons.Any(p => p.txtDocumentNumber == customeRegisterViewModel.documento);

                    if (hasDocument)
                        documentoDuplicado = "duplicado";

                    var log = new tblRegisterCustomersLog
                    {
                        dteRegister = DateTime.Now,
                        txtDocument = customeRegisterViewModel.documento + documentoDuplicado,
                        txtEmail = customeRegisterViewModel.email,
                        txtName = customeRegisterViewModel.name,
                        txtPassword = hashedPassword.Password,
                        txtPhone = customeRegisterViewModel.country + customeRegisterViewModel.phone,
                        intDocumentType = customeRegisterViewModel.documentType == "CPF" ? 1 : 2,
                        idPai = idPai,
                        guidId = Guid.NewGuid()
                    };

                    ctx.tblRegisterCustomersLog.Add(log);
                    ctx.SaveChanges();

                    if (hasDocument)
                    {
                        return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Cliente já cadastrado." };
                        //throw new HttpResponseException(
                        //                     new Utils().GetErrorPostMessage("Esse cliente já existe no foneclube, tente fazer login ou entre em contato pelo nosso Whatsapp +55 (21) 97338-9882"));
                    }

                    var hasPerson = ctx.tblPersons
                        .Any(p => p.txtDocumentNumber == customeRegisterViewModel.documento && p.bitDelete != true);


                    //var hasEmail = ctx.tblPersons
                    //    .Any(p => p.txtEmail == customeRegisterViewModel.email.Trim() && p.bitDelete != true);

                    //if (hasEmail)
                    //    return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Email de Cliente já cadastrado." };

                    if (hasPerson)
                        return new RegisterResponse { Status = HttpStatusCode.MethodNotAllowed, Message = "Cliente já cadastrado." };

                    var phones = new List<Phone>();

                    phones.Add(new Phone
                    {
                        CountryCode = customeRegisterViewModel.country,
                        DDD = customeRegisterViewModel.phone.Substring(0, 2),
                        Number = customeRegisterViewModel.phone.Substring(2)
                    });

                    idNovoCliente = SetCrossCustomer(new Person
                    {
                        DocumentNumber = customeRegisterViewModel.documento,
                        Email = customeRegisterViewModel.email,
                        Name = customeRegisterViewModel.name,
                        Phones = phones,
                        Password = hashedPassword.Password
                    });

                    ctx.SaveChanges();

                    intIdCustomerLog = log.intIdCustomer;

                    ctx.tblPersonsParents.Add(new tblPersonsParents
                    {
                        dteCadastro = DateTime.Now,
                        intIdSon = idNovoCliente,
                        intIdParent = idPai
                    });

                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        txtCep = "22793081",
                        intIdPerson = idNovoCliente,
                        intStreetNumber = 3434,
                        txtCity = "Rio de Janeiro",
                        txtCountry = "Brasil",
                        txtNeighborhood = "Barra da Tijuca",
                        txtState = "RJ",
                        txtStreet = "Avenida das americas",
                        txtComplement = "305 bloco 2",
                        intAdressType = Constants.EnderecoCobranca
                    });

                    ctx.SaveChanges();

                    var parentName = ctx.tblPersons.Where(x => x.intIdPerson == idPai).FirstOrDefault();
                    string parentNome = string.Empty;
                    string parentPhone = string.Empty;
                    if (parentName != null)
                    {
                        parentNome = parentName.txtName;
                        var pphone = ctx.tblPersonsPhones.FirstOrDefault(x => x.intIdPerson == parentName.intIdPerson && x.bitPhoneClube == true);
                        if (pphone != null)
                            parentPhone = pphone.intDDD.ToString() + pphone.intPhone.ToString();
                    }

                    WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                    SendMessageToAdminAndParent objSend = new SendMessageToAdminAndParent()
                    {
                        Type = "RegistrationSuccess",
                        Name = customeRegisterViewModel.name,
                        CPF = customeRegisterViewModel.documento,
                        ParentName = parentNome,
                        WhatsAppNumber = customeRegisterViewModel.phone,
                        ParentWhatsAppNumber = parentPhone
                    };
                    whatsAppAccess.SendMessageInfoToAdminAndParent(objSend);

                    var confirmation = new GenericTemplate()
                    {
                        Template = new WhatsAppMessageTemplates() { TemplateName = "Confirmar" },
                        PhoneNumbers = customeRegisterViewModel.phone,
                        PersonId = idNovoCliente
                    };
                    whatsAppAccess.SendGenericMessage(confirmation);
                }

                try
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var customerNames = GetName(customeRegisterViewModel.name);
                        afiliateLink = new Affiliates().GetReferralLink(idNovoCliente);

                        var maskName = customerNames.Name.ToLower() + "-" + customerNames.LastWord.ToLower();
                        var previousMaskName = ctx.tblPersosAffiliateLinks.FirstOrDefault(p => p.txtMaskName == maskName);

                        if (previousMaskName != null)
                            maskName = new ProfileAccess().GetMaskName(maskName, previousMaskName.txtMaskName);

                        try
                        {
                            blinkService = new BlinkIntegrationManager().CreateLinkIndication(afiliateLink, maskName);
                        }
                        catch (Exception e) { }

                        var email = new Email();
                        if (string.IsNullOrEmpty(blinkService))
                        {
                            email = new Email
                            {
                                TemplateType = 24,
                                To = customeRegisterViewModel.email,
                                TargetName = customerNames.Name,
                                TargetTextBlue = "<a href='" + afiliateLink + "'>" + afiliateLink.Replace("http://", string.Empty) + "</a>",
                                TargetSecondaryText = string.Empty,
                                DiscountPrice = ""
                            };
                        }
                        else
                        {
                            email = new Email
                            {
                                TemplateType = 24,
                                To = customeRegisterViewModel.email,
                                TargetName = customerNames.Name,
                                TargetTextBlue = "<a href='" + blinkService + "'>" + blinkService.Replace("http://", string.Empty) + "</a>",
                                TargetSecondaryText = string.Empty,
                                DiscountPrice = ""
                            };
                        }



                        sendEmail = new EmailAccess().SendEmailDinamic(email);

                        try
                        {
                            ctx.tblPersosAffiliateLinks.Add(new tblPersosAffiliateLinks
                            {
                                dteRegister = DateTime.Now,
                                intIdPerson = idNovoCliente,
                                txtMaskName = maskName,
                                txtBlinkLink = blinkService,
                                txtOriginalLink = afiliateLink
                            });

                            ctx.SaveChanges();
                        }
                        catch (Exception)
                        {

                        }
                    }

                    try
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var log = ctx.tblRegisterCustomersLog.FirstOrDefault(r => r.intIdCustomer == intIdCustomerLog);

                            if (log.txtLog != null)
                                log.txtLog = log.txtLog.ToString() + sendEmail.ToString() + "_" + afiliateLink + "_" + idNovoCliente;
                            else
                                log.txtLog = sendEmail.ToString() + "_" + afiliateLink + "_" + idNovoCliente;

                            ctx.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception)
                {

                }

                return new RegisterResponse { Status = HttpStatusCode.OK, IdCliente = idNovoCliente };
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void AddEditPhone(Phone phoneIn, int IdPerson)
        {
            #region Save Customer Phone
            if (!string.IsNullOrEmpty(phoneIn.DDD) && !string.IsNullOrEmpty(phoneIn.DDD))
            {
                using (var ctx = new FoneClubeContext())
                {
                    var ddd = Convert.ToInt32(phoneIn.DDD);
                    var phone = Convert.ToInt64(phoneIn.Number);

                    var isPhoneExists = ctx.tblPersonsPhones.Any(x => x.intDDD == ddd && x.intPhone == phone && x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value);
                    if (!isPhoneExists)
                    {
                        ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                        {
                            intDDD = ddd,
                            intPhone = phone,
                            intIdPlan = 0,
                            intIdPerson = IdPerson,
                            bitDelete = false,
                            bitPhoneClube = false,
                            bitAtivo = phoneIn.LinhaAtiva,
                            bitEsim = phoneIn.ESim
                        });
                        ctx.SaveChanges();
                    }
                }
            }

            #endregion
        }

        public void AddEditAddress(Adress addressIn, int IdPerson)
        {
            if (addressIn != null)
            {
                #region Save Customer Phone
                using (var ctx = new FoneClubeContext())
                {
                    var addressExist = ctx.tblPersonsAddresses.FirstOrDefault(x => x.intIdPerson == IdPerson);
                    if (addressExist == null)
                    {
                        if (addressIn != null)
                        {
                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                            {
                                txtCep = addressIn.Cep,
                                intIdPerson = IdPerson,
                                intStreetNumber = !string.IsNullOrEmpty(addressIn.StreetNumber) ? Convert.ToInt32(addressIn.StreetNumber) : 3434,
                                txtCity = addressIn.City,
                                txtCountry = "Brasil",
                                txtNeighborhood = addressIn.Neighborhood,
                                txtState = addressIn.State,
                                txtStreet = addressIn.Street,
                                txtComplement = addressIn.Complement,
                                intAdressType = Constants.EnderecoEntrega
                            }); ;
                        }
                        else
                        {
                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                            {
                                txtCep = "22793081",
                                intIdPerson = IdPerson,
                                intStreetNumber = 3434,
                                txtCity = "Rio de Janeiro",
                                txtCountry = "Brasil",
                                txtNeighborhood = "Barra da Tijuca",
                                txtState = "RJ",
                                txtStreet = "Avenida das americas",
                                txtComplement = "305 bloco 2",
                                intAdressType = Constants.EnderecoEntrega
                            });
                        }
                    }
                    else
                    {
                        addressExist.txtCep = addressIn.Cep;
                        addressExist.intIdPerson = IdPerson;
                        addressExist.intStreetNumber = !string.IsNullOrEmpty(addressIn.StreetNumber) ? Convert.ToInt32(addressIn.StreetNumber) : 3434;
                        addressExist.txtCity = addressIn.City;
                        addressExist.txtCountry = "Brasil";
                        addressExist.txtNeighborhood = addressIn.Neighborhood;
                        addressExist.txtState = addressIn.State;
                        addressExist.txtStreet = addressIn.Street;
                        addressExist.txtComplement = addressIn.Complement;
                        addressExist.intAdressType = Constants.EnderecoEntrega;
                    }
                    ctx.SaveChanges();

                }

                #endregion
            }
        }

        public string AddFoneClubePhones(List<Phone> phones, int IdPerson, string password, string actCPF, bool isActivateBeforePayment)
        {
            #region Save Customer Foneclube Phone
            string status = "sucesso";
            string Password = ConfigurationManager.AppSettings["ActivationPassword"];
            int intActDDD = 0;
            if (string.IsNullOrEmpty(Password))
                Password = "@Luana010203";

            using (var ctx = new FoneClubeContext())
            {
                var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == IdPerson).FirstOrDefault();
                var plans = ctx.tblPlansOptions.Where(x => x.intIdOperator == 4).ToList();
                var phoneCom = ctx.tblPersonsPhones.Where(x => x.bitPhoneClube == false).FirstOrDefault();
                var address = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == IdPerson).FirstOrDefault();

                //Format - ICCID|Plano|CPF|Phone|DDD|Port|Operado

                ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                activatePlanRequest.metodo_pagamento = "SALDO";
                activatePlanRequest.nome = tblPerson.txtName;

                if (tblPerson.txtDocumentNumber.Length == 11 || actCPF.Length == 11)
                {
                    if (!string.IsNullOrEmpty(actCPF))
                        activatePlanRequest.cpf = actCPF;
                    else
                        activatePlanRequest.cpf = tblPerson.txtDocumentNumber;
                }
                else
                {
                    if (!string.IsNullOrEmpty(actCPF))
                        activatePlanRequest.cnpj = actCPF;
                    else
                        activatePlanRequest.cnpj = tblPerson.txtDocumentNumber;
                }
                activatePlanRequest.email = string.IsNullOrEmpty(tblPerson.txtEmail) ? "oi@facil.tel" : tblPerson.txtEmail;
                activatePlanRequest.telefone = phoneCom != null ? string.Concat(phoneCom.intDDD, phoneCom.intPhone) : "21999999999";
                activatePlanRequest.data_nascimento = "1900-01-01";
                activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();
                activatePlanRequest.endereco.rua = address.txtStreet;
                activatePlanRequest.endereco.numero = address.intStreetNumber.ToString();
                activatePlanRequest.endereco.complemento = address.txtComplement;
                activatePlanRequest.endereco.bairro = address.txtNeighborhood;
                activatePlanRequest.endereco.cep = address.txtCep.Replace("-", "").Replace(".", "");
                activatePlanRequest.endereco.municipio = address.txtCity;
                activatePlanRequest.endereco.uf = address.txtState;

                activatePlanRequest.chips = new List<Chip>();

                if (phones != null && phones.Count > 0)
                {
                    foreach (var phone in phones)
                    {
                        if (phone != null)
                        {
                            intActDDD = Convert.ToInt32(phone.DDD);

                            if (string.IsNullOrEmpty(phone.Number) && string.IsNullOrEmpty(phone.PortNumber) && !string.IsNullOrEmpty(phone.ICCID))
                            {
                                var isIccidAlreadyExist = ctx.tblPersonsPhones.FirstOrDefault(x => x.txtICCID == phone.ICCID && x.intDDD == 0 && x.intPhone == 0);
                                if (isIccidAlreadyExist is null)
                                {
                                    ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                    {
                                        txtICCID = phone.ICCID,
                                        intDDD = 99,
                                        intPhone = 999999999,
                                        intIdPlan = phone.IdPlanOption,
                                        intIdPerson = IdPerson,
                                        bitDelete = false,
                                        bitPhoneClube = true,
                                        bitAtivo = true,
                                        intAmmoutPrecoVip = phone.AmmountPrecoVip,
                                        bitEsim = phone.ESim,
                                        intIdOperator = 4,
                                    });
                                }
                                else
                                {
                                    isIccidAlreadyExist.txtICCID = phone.ICCID;
                                    isIccidAlreadyExist.intDDD = 99;
                                    isIccidAlreadyExist.intPhone = 999999999;
                                    isIccidAlreadyExist.intIdPlan = phone.IdPlanOption;
                                    isIccidAlreadyExist.intIdPerson = IdPerson;
                                    isIccidAlreadyExist.bitDelete = false;
                                    isIccidAlreadyExist.bitPhoneClube = true;
                                    isIccidAlreadyExist.bitAtivo = true;
                                    isIccidAlreadyExist.intAmmoutPrecoVip = phone.AmmountPrecoVip;
                                    isIccidAlreadyExist.bitEsim = phone.ESim;
                                    isIccidAlreadyExist.intIdOperator = 4;
                                }
                                ctx.SaveChanges();

                                if (isActivateBeforePayment && string.Equals(password, Password))
                                {
                                    var chip = new Chip();

                                    if (phone.ESim)
                                    {
                                        chip.esim = "SIM";
                                    }
                                    else
                                    {
                                        chip.iccid = phone.ICCID;
                                        //chip.esim = "N";
                                    }
                                    chip.id_plano = plans.Where(x => x.intIdPlan == Convert.ToInt32(phone.IdPlanOption)).FirstOrDefault().intOperatorPlanId.Value;
                                    chip.ddd = intActDDD;

                                    activatePlanRequest.chips.Add(chip);
                                }
                                //else
                                //{
                                //    status = "A senha está incorreta para ativação";
                                //    return status;
                                //}
                            }
                            else
                            {
                                phone.Number = phone.Number.Replace("-", "");
                                phone.PortNumber = phone.PortNumber.Replace("-", "");

                                var nuvoDDD = string.IsNullOrEmpty(phone.Number) ? 0 : Convert.ToInt32(phone.Number.Substring(0, 2));
                                var nuvoPhone = string.IsNullOrEmpty(phone.Number) ? 0 : Convert.ToInt64(phone.Number.Substring(2));

                                var ddd = string.IsNullOrEmpty(phone.PortDDD) ? 0 : Convert.ToInt32(phone.PortDDD);
                                var port = string.IsNullOrEmpty(phone.PortNumber) ? 0 : Convert.ToInt64(phone.PortNumber);

                                tblPersonsPhones tblphones1 = null;
                                if (nuvoDDD == 0 || nuvoPhone == 0)
                                {
                                    tblphones1 = ctx.tblPersonsPhones.FirstOrDefault(x => x.intDDD == ddd && x.intPhone == port && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value);
                                }
                                else
                                {
                                    tblphones1 = ctx.tblPersonsPhones.FirstOrDefault(x => x.intDDD == nuvoDDD && x.intPhone == nuvoPhone && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value);
                                }

                                if (tblphones1 == null || !phone.Activate)
                                {
                                    ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                    {
                                        txtICCID = phone.ICCID,
                                        intDDD = nuvoDDD != 0 ? nuvoDDD : ddd == 0 ? 99 : ddd,
                                        intPhone = nuvoPhone != 0 ? nuvoPhone : port == 0 ? 999999999 : port,
                                        intIdPlan = phone.IdPlanOption,
                                        intIdPerson = IdPerson,
                                        txtPortNumber = ddd != 0 && port != 0 ? ddd + "" + port : null,
                                        bitDelete = false,
                                        bitPhoneClube = true,
                                        bitAtivo = true,
                                        intAmmoutPrecoVip = phone.AmmountPrecoVip,
                                        bitEsim = phone.ESim,
                                        intIdOperator = 4,
                                    });
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    tblphones1.txtICCID = phone.ICCID;
                                    tblphones1.intIdPlan = phone.IdPlanOption;
                                    tblphones1.intIdPerson = IdPerson;
                                    tblphones1.txtPortNumber = ddd + "" + port;
                                    tblphones1.bitDelete = false;
                                    tblphones1.bitPhoneClube = true;
                                    tblphones1.bitAtivo = true;
                                    tblphones1.intAmmoutPrecoVip = phone.AmmountPrecoVip;
                                    tblphones1.bitEsim = phone.ESim;
                                    tblphones1.intIdOperator = 4;

                                    ctx.SaveChanges();
                                }

                                if (isActivateBeforePayment && string.Equals(password, Password))
                                {
                                    var chip = new Chip();
                                    if (phone.ESim)
                                    {
                                        chip.esim = "SIM";
                                    }
                                    else
                                    {
                                        chip.iccid = phone.ICCID;
                                        //chip.esim = "N";
                                    }
                                    chip.id_plano = plans.Where(x => x.intIdPlan == Convert.ToInt32(phone.IdPlanOption)).FirstOrDefault().intOperatorPlanId.Value;
                                    chip.ddd = intActDDD;
                                    activatePlanRequest.chips.Add(chip);
                                }
                                //else
                                //{
                                //    status = "A senha está incorreta para ativação";
                                //    return status;
                                //}
                            }
                        }

                        if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                        {
                            MVNOAccess mVNOAccess = new MVNOAccess();
                            var response = mVNOAccess.ActivatePlan(activatePlanRequest);
                            if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                            {
                                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
                                foreach (var pho in response.info.chips)
                                {
                                    System.Threading.Thread.Sleep(10000);
                                    var iccidPhoneData = mVNOAccess.ValidateICCID(pho.iccid);

                                    if (iccidPhoneData != null && iccidPhoneData.retorno)
                                    {
                                        if (iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                        {
                                            var activatedPhone = iccidPhoneData.info.numero_ativado;

                                            var planActivated = GetPlanIdByGB(iccidPhoneData.info.plano_nome);

                                            var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                            //Update plans
                                            if (tblphones != null && tblphones.Count() > 0)
                                            {
                                                foreach (var ph in tblphones)
                                                {
                                                    ph.intIdPlan = planActivated;
                                                    ph.bitAtivo = true;
                                                    ph.bitPhoneClube = true;
                                                    ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                    ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                }
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                {
                                                    intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                    intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                    intCountryCode = 55,
                                                    intIdPlan = planActivated,
                                                    bitAtivo = true,
                                                    bitPhoneClube = true,
                                                    intIdOperator = 4,

                                                    intIdPerson = tblPerson.intIdPerson,
                                                });
                                                ctx.SaveChanges();
                                            }

                                            string msgAdm = "New Activation post payment for *" + tblPerson.txtName + "*" +
                                                    "\n\nPlan : " + iccidPhoneData.info.plano_nome +
                                                    "\n Phone : " + iccidPhoneData.info.numero_ativado +
                                                    "\nICCID : " + iccidPhoneData.info.iccid;

                                            WhatsAppAccess whatsApp = new WhatsAppAccess();
                                            whatsApp.SendMessageInfoToAdmin(msgAdm);

                                            if (!string.IsNullOrEmpty(phone.PortNumber))
                                            {
                                                ActivatePortRequest portRequest = new ActivatePortRequest();
                                                portRequest.numero_contel = activatedPhone;
                                                portRequest.doador_numero = phone.PortDDD + phone.PortNumber;
                                                portRequest.doador_id_operadora = Convert.ToInt32(phone.PortDDD);

                                                var portresponse = mVNOAccess.PortNumber(portRequest);

                                                string mssg1 = string.Empty;
                                                if (portresponse != null && portresponse.retorno)
                                                {
                                                    mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Solicitada com successo para o numero {3}. LINK: {4} {5}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, phone.PortNumber, response.link, response.link_esim);
                                                }
                                                else
                                                {
                                                    mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Portabilidade falhada para o numero {3}. LINK: {4} {5}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, phone.PortNumber, response.link, response.link_esim);
                                                }
                                                whatsApp.SendMessageInfoToAdmin(mssg1);

                                                ctx.tblLog.Add(new tblLog()
                                                {
                                                    dteTimeStamp = DateTime.Now,
                                                    txtAction = "PortNumber completed"
                                                });
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {
                                                var msg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* \n Portabilidade  Não Solicitada. LINK: {3} {4}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, response.link, response.link_esim);
                                                whatsApp.SendMessageInfoToAdmin(msg1);
                                            }
                                        }
                                        else
                                        {
                                            status = "Plano ativado com sucesso, mas não conseguiu obter o número ativado";
                                        }
                                    }

                                }
                            }
                            else
                            {
                                status = "Ocorreu um erro na ativação:" + response.mensagem;
                            }
                        }
                        else
                        {
                            ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan No chips to activate" });
                            ctx.SaveChanges();
                            //status = "Nenhum chip disponível para ativação";
                        }
                    }
                }
            }

            return status;
            #endregion
        }

        public string InstaInsertCustomerOrLine(InstaRegisterClientOrLineViewModel model, out tblPersons person1)
        {
            person1 = null;
            string error = "sucesso";
            try
            {
                if (model != null && model.Person != null && model.CustomerPhone != null && model.Phones != null)
                {
                    int idPai = 0;
                    if (model.Person != null && model.Person.Parent != null)
                    {
                        idPai = InstaSetParentInfo(model.Person.Parent);
                    }

                    using (var ctx = new FoneClubeContext())
                    {
                        var tblPerson = ctx.tblPersons.Where(x => x.txtDocumentNumber.Trim() == model.Person.CPF.Trim()).FirstOrDefault();
                        if (tblPerson != null)
                        {
                            person1 = tblPerson;
                            #region Edit Customer

                            if (!string.IsNullOrEmpty(model.Person.Nome))
                                tblPerson.txtName = model.Person.Nome;
                            if (!string.IsNullOrEmpty(model.Person.Email))
                                tblPerson.txtEmail = model.Person.Email;
                            ctx.SaveChanges();

                            #endregion

                            AddEditPhone(model.CustomerPhone, tblPerson.intIdPerson);

                            AddEditAddress(model.Address, tblPerson.intIdPerson);

                            error = AddFoneClubePhones(model.Phones, tblPerson.intIdPerson, model.ActivationPwd, model.ActivationCPF, model.IsActivateBeforePayment);
                        }
                        else
                        {
                            var ddd = Convert.ToInt32(model.CustomerPhone.DDD);
                            var phone = Convert.ToInt64(model.CustomerPhone.Number);

                            CustomerCrossRegisterViewModel objModel = new CustomerCrossRegisterViewModel();
                            objModel.phone = model.Person.WhatsAppNumber;
                            objModel.documento = model.Person.CPF;
                            objModel.documentType = model.Person.CPFType == 0 ? "CPF" : "CNPJ";
                            objModel.name = model.Person.Nome;
                            objModel.email = !string.IsNullOrEmpty(model.Person.Email) ? model.Person.Email : string.Format("{0}{1}@foneclube.com.br", ddd, phone);
                            objModel.password = "000000";
                            objModel.confirmPassword = "000000";
                            if (idPai != 0)
                                objModel.idPai = idPai.ToString();

                            var result = InsertNewCustomerRegisterCross(objModel);
                            if (result != null && result.Status == HttpStatusCode.OK)
                            {
                                var person = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == model.Person.CPF);

                                if (person != null)
                                {
                                    person1 = person;
                                    if (result != null && result.Status == HttpStatusCode.OK)
                                    {
                                        AddEditAddress(model.Address, tblPerson.intIdPerson);

                                        AddEditPhone(model.CustomerPhone, tblPerson.intIdPerson);

                                        error = AddFoneClubePhones(model.Phones, tblPerson.intIdPerson, model.ActivationPwd, model.ActivationCPF, model.IsActivateBeforePayment);
                                    }
                                }
                            }
                            else
                            {
                                error = "Falha ao registrar usuário";
                            }
                        }
                    }
                }
                else
                {
                    error = "Detalhes necessários não disponíveis, como telefone e linhas do cliente";
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = ex.ToString()
                    });
                    ctx.SaveChanges();
                }
                error = "Ocorreu um erro, tente novamente mais tarde";
            }
            return error;
        }

        public InstaChargeResponse InstaRegisterAndCharge(CustomerInstaChargeViewModel model)
        {
            try
            {
                if (model != null)
                {
                    int ddd = 0;
                    long phone = 0;
                    int idPai = 0;

                    if (model.Parent != null)
                    {
                        idPai = InstaSetParentInfo(model.Parent);
                    }

                    if (model.WhatsAppNumber.Length >= 11)
                    {
                        ddd = Convert.ToInt32(model.WhatsAppNumber.Substring(0, 2));
                        phone = Convert.ToInt64(model.WhatsAppNumber.Substring(2));
                    }

                    using (var ctx = new FoneClubeContext())
                    {
                        var tblPerson = ctx.tblPersons.Where(x => x.txtDocumentNumber.Trim() == model.CPF.Trim()).FirstOrDefault();
                        if (tblPerson != null)
                        {
                            // Save incoming fields
                            if (!string.IsNullOrEmpty(model.Nome))
                                tblPerson.txtName = model.Nome;
                            if (!string.IsNullOrEmpty(model.Email))
                                tblPerson.txtEmail = model.Email;
                            ctx.SaveChanges();

                            var isPhoneExists = ctx.tblPersonsPhones.Any(x => x.intDDD == ddd && x.intPhone == phone && x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value);
                            if (!isPhoneExists)
                            {
                                ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                {
                                    intDDD = ddd,
                                    intPhone = phone,
                                    intIdPlan = 0,
                                    intIdPerson = tblPerson.intIdPerson,
                                    bitDelete = false,
                                    bitPhoneClube = false,
                                    bitAtivo = true,
                                    bitEsim = model.Esim
                                });
                                ctx.SaveChanges();
                            }

                            var addressExist = ctx.tblPersonsAddresses.FirstOrDefault(x => x.intIdPerson == tblPerson.intIdPerson);
                            if (addressExist == null)
                            {
                                //Add dummy address
                                if (model.ShipmentType == 2)
                                {
                                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                    {
                                        txtCep = "22793081",
                                        intIdPerson = tblPerson.intIdPerson,
                                        intStreetNumber = 3434,
                                        txtCity = "Rio de Janeiro",
                                        txtCountry = "Brasil",
                                        txtNeighborhood = "Barra da Tijuca",
                                        txtState = "RJ",
                                        txtStreet = "Avenida das americas",
                                        txtComplement = "305 bloco 2",
                                        intAdressType = Constants.EnderecoCobranca
                                    });
                                }
                                else
                                {
                                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                    {
                                        txtCep = model.ShipmentAddress.CEP,
                                        intIdPerson = tblPerson.intIdPerson,
                                        intStreetNumber = Convert.ToInt32(model.ShipmentAddress.Numero),
                                        txtCity = model.ShipmentAddress.Cidade,
                                        txtCountry = "Brasil",
                                        txtNeighborhood = model.ShipmentAddress.Bairro,
                                        txtState = model.ShipmentAddress.Estado,
                                        txtStreet = model.ShipmentAddress.Rua,
                                        txtComplement = model.ShipmentAddress.complemento,
                                        intAdressType = Constants.EnderecoEntrega
                                    });
                                }

                                ctx.SaveChanges();
                            }
                            else
                            {
                                addressExist.txtCep = model.ShipmentAddress.CEP;
                                addressExist.intIdPerson = tblPerson.intIdPerson;
                                addressExist.intStreetNumber = Convert.ToInt32(model.ShipmentAddress.Numero);
                                addressExist.txtCity = model.ShipmentAddress.Cidade;
                                addressExist.txtCountry = "Brasil";
                                addressExist.txtNeighborhood = model.ShipmentAddress.Bairro;
                                addressExist.txtState = model.ShipmentAddress.Estado;
                                addressExist.txtStreet = model.ShipmentAddress.Rua;
                                addressExist.txtComplement = model.ShipmentAddress.complemento;
                                addressExist.intAdressType = Constants.EnderecoEntrega;
                            }

                            AddPhoneLinesWithICCID(model, tblPerson.intIdPerson);

                            if (model.IsChargeReq)
                            {
                                var charging = new Charging();
                                charging.SendEmail = true;
                                charging.PaymentType = 3;
                                charging.Ammount = model.Amount.ToString();
                                charging.AnoVingencia = model.Vigencia.Split(' ')[0];
                                charging.MesVingencia = model.Vigencia.Split(' ')[1];
                                charging.CommentEmail = model.Comment;
                                charging.Comment = model.Comment;
                                charging.TransactionComment = model.Comment;
                                charging.DueDate = Convert.ToDateTime(model.Vencimento);
                                charging.TxtWAPhones = tblPerson.txtDefaultWAPhones;
                                charging.SendMarketing1 = false;
                                charging.SendMarketing2 = false;
                                charging.Installments = 0;
                                charging.InstaRegsiterData = model.SelectedPlans;

                                var personCharge = new Person { Id = tblPerson.intIdPerson, Charging = charging };

                                PagarmeAccess pagarmeAccess = new PagarmeAccess();
                                if (model.PaymentType == 3)
                                {
                                    var paymentResult = pagarmeAccess.GeraCobrancaIntegrada(personCharge);
                                    if (paymentResult != null && paymentResult.StatusPaid)
                                    {
                                        return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Cliente  " + tblPerson.txtName + " cobrado com sucesso." };
                                    }
                                    else
                                    {
                                        return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Falha na Cobrança! Dados do cliente " + tblPerson.txtName + " atualizados com sucesso." };
                                    }
                                }
                            }
                            else
                            {
                                return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Cliente " + tblPerson.txtName + " atualizado com sucesso." };
                            }
                        }
                        else
                        {
                            CustomerCrossRegisterViewModel objModel = new CustomerCrossRegisterViewModel();
                            objModel.phone = model.WhatsAppNumber;
                            objModel.documento = model.CPF;
                            objModel.documentType = model.CpfType == 0 ? "CPF" : "CNPJ";
                            objModel.name = model.Nome;
                            objModel.email = !string.IsNullOrEmpty(model.Email) ? model.Email : string.Format("{0}{1}@foneclube.com.br", ddd, phone);
                            objModel.password = "000000";
                            objModel.confirmPassword = "000000";
                            if (idPai != 0)
                                objModel.idPai = idPai.ToString();

                            var result = InsertNewCustomerRegisterCross(objModel);

                            var person = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == model.CPF);

                            if (person != null)
                            {
                                if (result != null && result.Status == HttpStatusCode.OK)
                                {
                                    if (!ctx.tblPersonsAddresses.Any(x => x.intIdPerson == person.intIdPerson))
                                    {
                                        //Add dummy address
                                        if (model.ShipmentType == 2)
                                        {
                                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                            {
                                                txtCep = "22793081",
                                                intIdPerson = person.intIdPerson,
                                                intStreetNumber = 3434,
                                                txtCity = "Rio de Janeiro",
                                                txtCountry = "Brasil",
                                                txtNeighborhood = "Barra da Tijuca",
                                                txtState = "RJ",
                                                txtStreet = "Avenida das americas",
                                                txtComplement = "305 bloco 2",
                                                intAdressType = Constants.EnderecoCobranca
                                            });
                                        }
                                        else
                                        {
                                            ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                                            {
                                                txtCep = model.ShipmentAddress.CEP,
                                                intIdPerson = person.intIdPerson,
                                                intStreetNumber = Convert.ToInt32(model.ShipmentAddress.Numero),
                                                txtCity = model.ShipmentAddress.Cidade,
                                                txtCountry = "Brasil",
                                                txtNeighborhood = model.ShipmentAddress.Bairro,
                                                txtState = model.ShipmentAddress.Estado,
                                                txtStreet = model.ShipmentAddress.Rua,
                                                txtComplement = model.ShipmentAddress.complemento,
                                                intAdressType = Constants.EnderecoEntrega
                                            });
                                        }

                                        ctx.SaveChanges();
                                    }

                                    var isPhoneExists = ctx.tblPersonsPhones.Any(x => x.intDDD == ddd && x.intPhone == phone && x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value);
                                    if (!isPhoneExists)
                                    {
                                        ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                        {
                                            intDDD = ddd,
                                            intPhone = phone,
                                            intIdPlan = 0,
                                            intIdPerson = tblPerson.intIdPerson,
                                            bitDelete = false,
                                            bitPhoneClube = false,
                                            bitAtivo = true,
                                            bitEsim = model.Esim
                                        });
                                        ctx.SaveChanges();
                                    }

                                    AddPhoneLinesWithICCID(model, person.intIdPerson);

                                    if (model.IsChargeReq)
                                    {
                                        var charging = new Charging();
                                        charging.SendEmail = true;
                                        charging.PaymentType = 3;
                                        charging.Ammount = model.Amount.ToString();
                                        charging.AnoVingencia = model.Vigencia.Split(' ')[0];
                                        charging.MesVingencia = model.Vigencia.Split(' ')[1];
                                        charging.CommentEmail = model.Comment;
                                        charging.Comment = model.Comment;
                                        charging.TransactionComment = model.Comment;
                                        charging.DueDate = Convert.ToDateTime(model.Vencimento);
                                        charging.TxtWAPhones = person.txtDefaultWAPhones;
                                        charging.SendMarketing1 = false;
                                        charging.SendMarketing2 = false;
                                        charging.Installments = 0;
                                        charging.InstaRegsiterData = model.SelectedPlans;

                                        var personCharge = new Person { Id = person.intIdPerson, Charging = charging };

                                        PagarmeAccess pagarmeAccess = new PagarmeAccess();
                                        if (model.PaymentType == 3)
                                        {
                                            var paymentResult = pagarmeAccess.GeraCobrancaIntegrada(personCharge);
                                            if (paymentResult != null && paymentResult.StatusPaid)
                                            {
                                                return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Novo cliente " + person.txtName + " cadastrado e cobrado com sucesso." };
                                            }
                                            else
                                            {
                                                return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Falha ao cobrar novo cliente " + person.txtName + "!!!  cadastro registro realizado com sucesso." };
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = "Novo cliente  " + person.txtName + " registrado com sucesso!" };
                                    }
                                }
                            }
                            return new InstaChargeResponse { Status = HttpStatusCode.OK, Message = result.Message };
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
            return new InstaChargeResponse
            {
                Status = HttpStatusCode.OK
            };
        }

        public int InstaChargeClient(tblPersons person, Charging model, string cardId)
        {
            try
            {
                var charging = new Charging();
                charging.SendEmail = true;
                charging.PaymentType = model.PaymentType;
                charging.Ammount = model.Ammount.ToString();
                charging.AnoVingencia = model.AnoVingencia;
                charging.MesVingencia = model.MesVingencia;
                charging.CommentEmail = model.Comment;
                charging.Comment = model.Comment;
                charging.TransactionComment = model.Comment;
                charging.DueDate = model.DueDate;
                charging.TxtWAPhones = person.txtDefaultWAPhones;
                charging.SendMarketing1 = false;
                charging.SendMarketing2 = false;
                charging.Installments = 0;
                charging.InstaRegsiterData = model.InstaRegsiterData;

                var personCharge = new Person { Id = person.intIdPerson, Charging = charging };

                PagarmeAccess pagarmeAccess = new PagarmeAccess();
                if (model.PaymentType == 3)
                {
                    var paymentResult = pagarmeAccess.GeraCobrancaIntegrada(personCharge);
                    if (paymentResult != null && paymentResult.StatusPaid)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (model.PaymentType == 1)
                {
                    var paymentResult = pagarmeAccess.GeraCobrancaCard(personCharge, cardId);
                    if (paymentResult != null && paymentResult.StatusPaid)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }

                }

            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
            return 0;
        }

        public void ActivateMVNOPlansPostPayment(long transactionId)
        {
            MVNOAccess mVNOAccess = new MVNOAccess();
            int shipmentType = 1;
            using (var ctx = new FoneClubeContext())
            {
                var charge = ctx.tblChargingHistory.Where(x => x.intIdTransaction == transactionId).FirstOrDefault();
                if (charge != null && !string.IsNullOrEmpty(charge.txtInstaRegsiterData))
                {
                    var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                    var address = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                    var plans = ctx.tblPlansOptions.Where(x => x.intIdOperator == 4).ToList();
                    var selectedPlans = charge.txtInstaRegsiterData.Split(new string[] { "#" }, StringSplitOptions.None);
                    var phoneCom = ctx.tblPersonsPhones.Where(x => x.bitPhoneClube == false).FirstOrDefault();

                    if (address is null)
                    {
                        ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                        {
                            txtCep = "22793081",
                            intIdPerson = tblPerson.intIdPerson,
                            intStreetNumber = 3434,
                            txtCity = "Rio de Janeiro",
                            txtCountry = "Brasil",
                            txtNeighborhood = "Barra da Tijuca",
                            txtState = "RJ",
                            txtStreet = "Avenida das americas",
                            txtComplement = "305 bloco 2",
                            intAdressType = Constants.EnderecoCobranca
                        });
                        ctx.SaveChanges();

                        address = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == charge.intIdCustomer).FirstOrDefault();
                    }

                    //Format - ICCID|Plano|Amount|ActivationDDD|Phone|PortDDD|Port|IsActivate|IsCharge|ESim
                    if (selectedPlans != null && selectedPlans.Length > 0)
                    {
                        ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                        activatePlanRequest.metodo_pagamento = "SALDO";
                        activatePlanRequest.nome = tblPerson.txtName;
                        if (tblPerson.txtDocumentNumber.Length == 11)
                            activatePlanRequest.cpf = tblPerson.txtDocumentNumber;
                        else
                            activatePlanRequest.cnpj = tblPerson.txtDocumentNumber;
                        activatePlanRequest.email = string.IsNullOrEmpty(tblPerson.txtEmail) ? "oi@facil.tel" : tblPerson.txtEmail;
                        activatePlanRequest.telefone = phoneCom != null ? string.Concat(phoneCom.intDDD, phoneCom.intPhone) : "21999999999";
                        activatePlanRequest.data_nascimento = "1900-01-01";
                        activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();
                        activatePlanRequest.endereco.rua = address.txtStreet;
                        activatePlanRequest.endereco.numero = address.intStreetNumber.ToString();
                        activatePlanRequest.endereco.complemento = address.txtComplement;
                        activatePlanRequest.endereco.bairro = address.txtNeighborhood;
                        activatePlanRequest.endereco.cep = address.txtCep.Replace("-", "").Replace(".", "");
                        activatePlanRequest.endereco.municipio = address.txtCity;
                        activatePlanRequest.endereco.uf = address.txtState;

                        activatePlanRequest.chips = new List<Chip>();

                        foreach (var splan in selectedPlans)
                        {
                            var planDetails = splan.Split('|');
                            if (planDetails != null && planDetails.Length > 0)
                            {
                                var iccid = planDetails[0];
                                var plan = !string.IsNullOrEmpty(planDetails[1]) ? Convert.ToInt32(planDetails[1]) : -1;
                                var amount = !string.IsNullOrEmpty(planDetails[2]) ? Convert.ToInt32(planDetails[2]) : 0;
                                var actDDD = !string.IsNullOrEmpty(planDetails[3]) ? Convert.ToInt32(planDetails[3]) : 0;
                                var phone = !string.IsNullOrEmpty(planDetails[4]) ? Convert.ToInt32(planDetails[4]) : 0;
                                var portDDD = !string.IsNullOrEmpty(planDetails[5]) ? Convert.ToInt32(planDetails[5]) : 0;
                                var port = !string.IsNullOrEmpty(planDetails[6]) ? Convert.ToInt64(planDetails[6]) : 0;
                                var activate = Convert.ToBoolean(planDetails[7]);
                                var esim = Convert.ToBoolean(planDetails[9]);
                                shipmentType = Convert.ToInt32(planDetails[10]);

                                if (activate)
                                {
                                    var chip = new Chip();
                                    if (esim)
                                    {
                                        chip.esim = "SIM";
                                    }
                                    else
                                    {
                                        chip.iccid = iccid;
                                        //chip.esim = "N";
                                    }
                                    chip.id_plano = plans.Where(x => x.intIdPlan == Convert.ToInt32(plan)).FirstOrDefault().intOperatorPlanId.Value;
                                    chip.ddd = actDDD == 0 ? actDDD : 21;
                                    activatePlanRequest.chips.Add(chip);
                                }

                                if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                                {
                                    var response = mVNOAccess.ActivatePlan(activatePlanRequest);

                                    if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                                    {
                                        foreach (var pho in response.info.chips)
                                        {
                                            System.Threading.Thread.Sleep(10000);

                                            var iccidPhoneData = mVNOAccess.ValidateICCID(pho.iccid);

                                            if (iccidPhoneData != null && iccidPhoneData.retorno)
                                            {
                                                if (iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                                {
                                                    var activatedPhone = iccidPhoneData.info.numero_ativado;

                                                    var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                                    var planActivated = GetPlanIdByGB(iccidPhoneData.info.plano_nome);

                                                    //Update plans
                                                    if (tblphones != null && tblphones.Count() > 0)
                                                    {
                                                        foreach (var ph in tblphones)
                                                        {
                                                            //ph.intIdPlan = plans.Where(x => x.intOperatorPlanId == Convert.ToInt32(pho.ativacao.id_plano)).FirstOrDefault().intIdPlan;
                                                            ph.intIdPlan = planActivated;
                                                            ph.bitAtivo = true;
                                                            ph.bitPhoneClube = true;
                                                            ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                            ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                        }
                                                        ctx.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        ctx.tblPersonsPhones.Add(new tblPersonsPhones()
                                                        {
                                                            intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2)),
                                                            intPhone = Convert.ToInt32(activatedPhone.Substring(2)),
                                                            intCountryCode = 55,
                                                            intIdPlan = planActivated,
                                                            bitAtivo = true,
                                                            bitPhoneClube = true,
                                                            intIdOperator = 4,
                                                            intIdPerson = tblPerson.intIdPerson,
                                                        });
                                                        ctx.SaveChanges();
                                                    }
                                                    string msgAdm = "New Activation post payment for *" + tblPerson.txtName + "*" +
                                                        "\n\nPlan : " + iccidPhoneData.info.plano_nome +
                                                        "\n Phone : " + iccidPhoneData.info.numero_ativado +
                                                        "\nICCID : " + iccidPhoneData.info.iccid;

                                                    WhatsAppAccess whatsApp = new WhatsAppAccess();
                                                    whatsApp.SendMessageInfoToAdmin(msgAdm);

                                                    if (port != 0)
                                                    {
                                                        ActivatePortRequest portRequest = new ActivatePortRequest();
                                                        portRequest.numero_contel = activatedPhone;
                                                        portRequest.doador_numero = portDDD + Convert.ToString(port);
                                                        portRequest.doador_id_operadora = portDDD;

                                                        var portresponse = mVNOAccess.PortNumber(portRequest);

                                                        string mssg1 = string.Empty;
                                                        if (portresponse != null && portresponse.retorno)
                                                        {
                                                            mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Solicitada com successo para o numero {3}. LINK: {4} {5}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, port, response.link, response.link_esim);
                                                        }
                                                        else
                                                        {
                                                            mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Portabilidade falhada para o numero {3}. LINK: {4} {5}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, port, response.link, response.link_esim);
                                                        }
                                                        whatsApp.SendMessageInfoToAdmin(mssg1);

                                                        ctx.tblLog.Add(new tblLog()
                                                        {
                                                            dteTimeStamp = DateTime.Now,
                                                            txtAction = "PortNumber completed"
                                                        });
                                                        ctx.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        var msg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* \n Portabilidade  Não Solicitada. LINK: {3} {4}", tblPerson.txtName, iccidPhoneData.info.plano, activatedPhone, response.link, response.link_esim);
                                                        whatsApp.SendMessageInfoToAdmin(msg1);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            else
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan No chips to activate" });
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        public void ActivateStoreEsim(long transactionId)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var getPlans = ctx.tblStoreCustomerPlans.Where(x => x.intIdTransaction == transactionId && x.bitESim && !x.bitActivated).ToList();
                    if (getPlans != null && getPlans.Count > 0)
                    {
                        foreach (var plan in getPlans)
                        {
                            var txtport = !string.IsNullOrEmpty(plan.txtPortNumber) ? plan.txtPortNumber.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "") : "";
                            var planoOption = ctx.tblPlansOptions.Where(x => x.intIdPlan == plan.intIdPlan).FirstOrDefault();
                            var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == plan.intIdPerson).FirstOrDefault();
                            var tblPersonPhone = ctx.tblPersonsPhones.Where(x => x.intIdPerson == plan.intIdPerson && (x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value)).FirstOrDefault();
                            var tblPersonAddress = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == plan.intIdPerson).FirstOrDefault();

                            ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                            activatePlanRequest.metodo_pagamento = "SALDO";
                            activatePlanRequest.nome = tblPerson.txtName;
                            if (tblPerson.txtDocumentNumber.Length == 11)
                                activatePlanRequest.cpf = tblPerson.txtDocumentNumber;
                            else
                                activatePlanRequest.cnpj = tblPerson.txtDocumentNumber;
                            activatePlanRequest.email = string.IsNullOrEmpty(tblPerson.txtEmail) ? "oi@facil.tel" : tblPerson.txtEmail;
                            activatePlanRequest.telefone = tblPersonPhone != null ? string.Concat(tblPersonPhone.intDDD, tblPersonPhone.intPhone).Length == 11 ? string.Concat(tblPersonPhone.intDDD, tblPersonPhone.intPhone) : "21999999999" : "21999999999";
                            activatePlanRequest.data_nascimento = "1900-01-01";
                            activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();
                            activatePlanRequest.endereco.rua = tblPersonAddress.txtStreet;
                            activatePlanRequest.endereco.numero = tblPersonAddress.intStreetNumber.ToString();
                            activatePlanRequest.endereco.complemento = tblPersonAddress.txtComplement;
                            activatePlanRequest.endereco.bairro = tblPersonAddress.txtNeighborhood;
                            activatePlanRequest.endereco.cep = tblPersonAddress.txtCep.Replace("-", "").Replace(".", "");
                            activatePlanRequest.endereco.municipio = tblPersonAddress.txtCity;
                            activatePlanRequest.endereco.uf = tblPersonAddress.txtState;

                            activatePlanRequest.chips = new List<Chip>();

                            if (plan.bitESim)
                            {
                                var chip = new Chip();
                                chip.id_plano = planoOption.intOperatorPlanId.HasValue ? planoOption.intOperatorPlanId.Value : 315;
                                chip.ddd = txtport == "" ? Convert.ToInt32(txtport.Substring(0, 2)) : 21;
                                chip.esim = "SIM";
                                activatePlanRequest.chips.Add(chip);
                            }

                            if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                            {
                                WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
                                MVNOAccess mVNOAccess = new MVNOAccess();
                                var response = mVNOAccess.ActivatePlan(activatePlanRequest);
                                if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                                {
                                    try
                                    {
                                        var updatePlan = ctx.tblStoreCustomerPlans.Where(x => x.intId == plan.intId).FirstOrDefault();
                                        if (updatePlan != null)
                                        {
                                            updatePlan.bitActivated = true;
                                            ctx.SaveChanges();
                                        }
                                    }
                                    catch (Exception ex) { }

                                    System.Threading.Thread.Sleep(10000);
                                    foreach (var pho in response.info.chips)
                                    {
                                        var iccidPhoneData = mVNOAccess.ValidateICCID(pho.iccid);

                                        ctx.tblLog.Add(new tblLog()
                                        {
                                            dteTimeStamp = DateTime.Now,
                                            txtAction = "ActivateStoreEsim: Activated iccid: " + pho.iccid
                                        });
                                        ctx.SaveChanges();

                                        if (iccidPhoneData != null && iccidPhoneData.retorno)
                                        {
                                            if (iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                            {
                                                var activatedPhone = iccidPhoneData.info.numero_ativado;

                                                var msg = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* ativada com sucesso. LINK: {3} {4}", tblPerson.txtName, plan.txtPlanDescription, activatedPhone, response.link, response.link_esim);
                                                whatsAppAccess.SendMessageInfoToAdmin(msg);

                                                ctx.tblLog.Add(new tblLog()
                                                {
                                                    dteTimeStamp = DateTime.Now,
                                                    txtAction = "ActivateStoreEsim: Activated phone: " + activatedPhone
                                                });
                                                ctx.SaveChanges();

                                                var tblphones = ctx.tblPersonsPhones.FirstOrDefault(x => x.intIdPerson == plan.intIdPerson && x.bitPhoneClube == true && x.intIdPlan == plan.intIdPlan && string.IsNullOrEmpty(x.txtICCID));
                                                tblphones.txtICCID = pho.iccid;
                                                tblphones.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                tblphones.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                tblphones.bitAtivo = true;

                                                ctx.SaveChanges();

                                                if (!string.IsNullOrEmpty(txtport))
                                                {
                                                    ActivatePortRequest portRequest = new ActivatePortRequest();
                                                    portRequest.numero_contel = activatedPhone;
                                                    portRequest.doador_numero = txtport;
                                                    portRequest.doador_id_operadora = 52;

                                                    var portresponse = mVNOAccess.PortNumber(portRequest);

                                                    string mssg1 = string.Empty;
                                                    if (portresponse != null && portresponse.retorno)
                                                    {
                                                        mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Solicitada com successo para o numero {3}. LINK: {4} {5}", tblPerson.txtName, plan.txtPlanDescription, activatedPhone, txtport, response.link, response.link_esim);
                                                    }
                                                    else
                                                    {
                                                        mssg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* Portabilidade falhada para o numero {3}. LINK: {4} {5}", tblPerson.txtName, plan.txtPlanDescription, activatedPhone, txtport, response.link, response.link_esim);
                                                    }
                                                    whatsAppAccess.SendMessageInfoToAdmin(mssg1);

                                                    ctx.tblLog.Add(new tblLog()
                                                    {
                                                        dteTimeStamp = DateTime.Now,
                                                        txtAction = "PortNumber completed"
                                                    });
                                                    ctx.SaveChanges();
                                                }
                                                else
                                                {
                                                    var msg1 = string.Format("*Cliente*: {0}, \n*Plano ativado*: {1} \nLinha *{2}* \n Portabilidade  Não Solicitada. LINK: {3} {4}", tblPerson.txtName, plan.txtPlanDescription, activatedPhone, response.link, response.link_esim);
                                                    whatsAppAccess.SendMessageInfoToAdmin(msg1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var foneClubePhones = ctx.tblPersonsPhones.FirstOrDefault(x => x.intIdPerson == plan.intIdPerson && x.bitPhoneClube == true && x.intIdPlan == plan.intIdPlan && string.IsNullOrEmpty(x.txtICCID));
                                            foneClubePhones.txtICCID = pho.iccid;
                                            ctx.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        var updatePlan = ctx.tblStoreCustomerPlans.Where(x => x.intId == plan.intId).FirstOrDefault();
                                        if (updatePlan != null)
                                        {
                                            updatePlan.bitActivated = true;
                                            ctx.SaveChanges();
                                        }
                                    }
                                    catch (Exception ex) { }

                                    var msg = string.Format("*Cliente*: {0}, \n *Plano ativado*: {1} \n Erro de ativação, a linha não foi ativada.", tblPerson.txtName, plan.txtPlanDescription);
                                    whatsAppAccess.SendMessageInfoToAdmin(msg);
                                }
                            }
                            else
                            {
                                ctx.tblLog.Add(new tblLog() { dteTimeStamp = DateTime.Now, txtAction = "ActivatePlan No chips to activate" });
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public void DeliverStoreSim(long transactionId)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    MVNOAccess mVNOAccess = new MVNOAccess();
                    var getPlans = ctx.tblStoreCustomerPlans.Where(x => x.intIdTransaction == transactionId && !x.bitESim && !x.bitActivated).ToList();
                    if (getPlans != null && getPlans.Count > 0)
                    {
                        var getPlan = getPlans[0];
                        var tblPerson = ctx.tblPersons.Where(x => x.intIdPerson == getPlan.intIdPerson).FirstOrDefault();
                        var tblPersonPhone = ctx.tblPersonsPhones.Where(x => x.intIdPerson == getPlan.intIdPerson && (x.bitPhoneClube.HasValue && !x.bitPhoneClube.Value)).FirstOrDefault();
                        var tblPersonAddress = ctx.tblPersonsAddresses.Where(x => x.intIdPerson == getPlan.intIdPerson).FirstOrDefault();

                        if (tblPersonAddress != null)
                        {
                            int cityId = 0;
                            try
                            {
                                var cityResponse = mVNOAccess.GetCityByState(tblPersonAddress.txtState);
                                if (cityResponse != null)
                                {
                                    cityId = cityResponse.data.Where(x => x.cidade.ToLower() == tblPersonAddress.txtCity.ToLower()).FirstOrDefault().id;
                                }
                            }
                            catch (Exception ex)
                            {
                                cityId = 0;
                            }

                            CompraRequest compraRequest = new CompraRequest();
                            compraRequest.metodo_pagamento = "SALDO";
                            compraRequest.nome = tblPerson.txtName;
                            if (tblPerson.txtDocumentNumber.Length == 11)
                                compraRequest.cpf = tblPerson.txtDocumentNumber;
                            else
                                compraRequest.cnpj = tblPerson.txtDocumentNumber;
                            compraRequest.nome = tblPerson.txtName;
                            compraRequest.email = string.IsNullOrEmpty(tblPerson.txtEmail) ? "oi@facil.tel" : tblPerson.txtEmail;
                            compraRequest.telefone = tblPersonPhone != null ? string.Concat(tblPersonPhone.intDDD, tblPersonPhone.intPhone).Length == 11 ? string.Concat(tblPersonPhone.intDDD, tblPersonPhone.intPhone) : "21999999999" : "21999999999";
                            compraRequest.data_nascimento = "1900-01-01";
                            compraRequest.endereco = new CompraEndereco();
                            compraRequest.endereco.rua = tblPersonAddress.txtStreet;
                            compraRequest.endereco.numero = tblPersonAddress.intStreetNumber.ToString();
                            compraRequest.endereco.complemento = tblPersonAddress.txtComplement;
                            compraRequest.endereco.bairro = tblPersonAddress.txtNeighborhood;
                            compraRequest.endereco.cep = tblPersonAddress.txtCep.Replace("-", "").Replace(".", "");
                            compraRequest.endereco.id_cidade = cityId;

                            compraRequest.quantidade = getPlans.Count;
                            compraRequest.id_plano = 88888;

                            var response = mVNOAccess.DeliverSimToCustomer(compraRequest);
                            if (response != null && response.retorno)
                            {
                                try
                                {
                                    foreach (var updatePlan in getPlans)
                                    {
                                        var rr = ctx.tblStoreCustomerPlans.FirstOrDefault(x => x.intId == updatePlan.intId);
                                        rr.bitActivated = true;
                                        ctx.SaveChanges();
                                    }
                                }
                                catch (Exception ex) { }

                                string msg = string.Format("Entrega do chip via logística iniciada pela API para o " +
                                    "\n*Cliente: {0}*" +
                                    "\n*ID da transação: {1}* " +
                                    "\n*Quantidade: {2}*" +
                                    "\n*Endereço: {3}*" +
                                    "\n*Plano vendido: {4}*" +
                                    "\n*Status : {5}*",
                                    tblPerson.txtName, transactionId, getPlans.Count,
                                    tblPersonAddress.txtStreet + ", " + tblPersonAddress.intStreetNumber + ", " + tblPersonAddress.txtComplement + ", " + tblPersonAddress.txtNeighborhood + ", " + tblPersonAddress.txtCep,
                                    getPlan.txtPlanDescription,
                                    "Não ativado"
                                    );
                                new WhatsAppAccess().SendMessageInfoToAdmin(msg);
                            }
                            else
                            {
                                string msg = string.Format("Entrega do chip via logística falhou devido a um erro na API" +
                                     "\n*Cliente: {0}*" +
                                     "\n*ID da transação: {1}* " +
                                     "\n*Quantidade: {2}*" +
                                     "\n*Endereço: {3}*" +
                                     "\n*Plano vendido: {4}*" +
                                     "\n*Status : {5}*" +
                                     "\n Error: {6}",
                                     tblPerson.txtName, transactionId, getPlans.Count,
                                     tblPersonAddress.txtStreet + ", " + tblPersonAddress.intStreetNumber + ", " + tblPersonAddress.txtComplement + ", " + tblPersonAddress.txtNeighborhood + ", " + tblPersonAddress.txtCep,
                                     getPlan.txtPlanDescription,
                                     "Não ativado",
                                     response.mensagem
                                     );
                                new WhatsAppAccess().SendMessageInfoToAdmin(msg);
                            }
                        }
                        //else
                        //{
                        //    string msg = string.Format("Entrega do chip via logística gerenciada pela API não envio devido ao estado como RJ para \n*Cliente: {0}* \n*ID da transação: {1}* \n*Quantidade: {2}*", tblPerson.txtName, transactionId, getPlans.Count);
                        //    new WhatsAppAccess().SendMessageInfoToAdmin(msg);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    ctx.tblLog.Add(new tblLog()
                    {
                        dteTimeStamp = DateTime.Now,
                        txtAction = "DeliverStoreSim error:" + ex.ToString()
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public void UpdateOrderStatus(long transactionId)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var order = ctx.tblStoreOrders.FirstOrDefault(x => x.intIdTransaction == transactionId);
                    if (order != null)
                    {
                        order.txtPaymentStatus = "Pago";
                        order.txtStatus = "Concluída";
                        ctx.SaveChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }

        public bool InsertPhonesPendingToPort(PhonesPendingToPortViewModel phonesToPort)
        {
            if (phonesToPort != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    try
                    {
                        ctx.tblPhonesPendingToPort.RemoveRange(ctx.tblPhonesPendingToPort.Where(p => p.intIdCustomer == phonesToPort.userId).ToList());
                        ctx.SaveChanges();
                    }
                    catch { }

                    ctx.tblPhonesPendingToPort.Add(new tblPhonesPendingToPort()
                    {
                        intIdCustomer = phonesToPort.userId,
                        txtNumberOfLines = phonesToPort.numPorts,
                        txtPhoneNumbers = phonesToPort.phones
                    });
                    ctx.SaveChanges();
                }
            }
            return true;
        }
        public bool SetCadastroSenha(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (customerMinhaConta != null && customerMinhaConta.senha != null && !string.IsNullOrEmpty(customerMinhaConta.senha.senha))
                    {
                        var hashedPassword = new Security().EncryptPassword(customerMinhaConta.senha.senha);
                        var customer = ctx.tblPersons.FirstOrDefault(a => a.intIdPerson == customerMinhaConta.id);
                        customer.txtPassword = hashedPassword.Password;
                        customer.bitSenhaCadastrada = true;
                    }
                    ctx.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SetCadastroPessoal(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var donoDocumento = ctx.tblPersons.FirstOrDefault(a => a.txtDocumentNumber == customerMinhaConta.dadosPessoais.documento);

                    if (donoDocumento != null)
                    {
                        if (donoDocumento.intIdPerson != customerMinhaConta.id)
                        {
                            //todo mensagem usada no client
                            throw new HttpResponseException(
                                new Utils().GetErrorPostMessage("O CPF inserido já é utilizado no Foneclube por um cliente registrado."));
                        }
                    }

                    var customer = ctx.tblPersons.FirstOrDefault(a => a.intIdPerson == customerMinhaConta.id);
                    customer.txtName = customerMinhaConta.dadosPessoais.nome;
                    customer.txtEmail = customerMinhaConta.dadosPessoais.email;
                    customer.txtDocumentNumber = customerMinhaConta.dadosPessoais.documento;

                    var phone = ctx.tblPersonsPhones.FirstOrDefault(a => a.bitPhoneClube == false && a.intIdPerson == customerMinhaConta.id);

                    phone.intDDD = Convert.ToInt32(customerMinhaConta.dadosPessoais.telefone.Substring(0, 2));
                    phone.intPhone = Convert.ToInt32(customerMinhaConta.dadosPessoais.telefone.Substring(2));

                    customer.bitDadosPessoaisCadastrados = true;

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool SetCadastroEndereco(CustomerMinhaContaViewModel customerMinhaConta)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var address = ctx.tblPersonsAddresses.FirstOrDefault(a => a.intIdPerson == customerMinhaConta.id);
                    var hasAnyAddress = address != null;

                    if (hasAnyAddress)
                    {
                        address.txtCep = customerMinhaConta.endereco.cep.Replace("-", "");
                        address.intStreetNumber = Convert.ToInt32(customerMinhaConta.endereco.numero);
                        address.txtCity = customerMinhaConta.endereco.cidade;
                        address.txtNeighborhood = customerMinhaConta.endereco.bairro;
                        address.txtState = customerMinhaConta.endereco.estado;
                        address.txtStreet = customerMinhaConta.endereco.rua;
                        address.txtComplement = customerMinhaConta.endereco.complemento;
                    }
                    else
                    {
                        ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                        {
                            txtCep = customerMinhaConta.endereco.cep.Replace("-", ""),
                            intIdPerson = customerMinhaConta.id,
                            intStreetNumber = Convert.ToInt32(customerMinhaConta.endereco.numero),
                            txtCity = customerMinhaConta.endereco.cidade,
                            txtCountry = "Brasil",
                            txtNeighborhood = customerMinhaConta.endereco.bairro,
                            txtState = customerMinhaConta.endereco.estado,
                            txtStreet = customerMinhaConta.endereco.rua,
                            txtComplement = customerMinhaConta.endereco.complemento,
                            intAdressType = Constants.EnderecoCobranca
                        });
                    }

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SavePersonAddressFC(CustomerAddressViewModel customeAddressViewModel)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (customeAddressViewModel.idCliente == 0)
                    {
                        customeAddressViewModel.idCliente = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == customeAddressViewModel.documento).intIdPerson;
                    }

                    ctx.tblPersonsAddresses.Add(new tblPersonsAddresses
                    {
                        txtCep = customeAddressViewModel.cep.Replace("-", ""),
                        intIdPerson = customeAddressViewModel.idCliente,
                        intStreetNumber = customeAddressViewModel.numero,
                        txtCity = customeAddressViewModel.cidade,
                        txtCountry = "Brasil",
                        txtNeighborhood = customeAddressViewModel.bairro,
                        txtState = customeAddressViewModel.estado,
                        txtStreet = customeAddressViewModel.rua,
                        txtComplement = customeAddressViewModel.complemento,
                        intAdressType = Constants.EnderecoCobranca
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

        public bool GetLoginAuthentication(string user, string password)
        {
            try
            {
                var isMail = user.Contains("@");

                using (var ctx = new FoneClubeContext())
                {
                    tblPersons cliente;
                    if (isMail)
                        cliente = ctx.tblPersons.FirstOrDefault(p => p.txtEmail == user);
                    else
                        cliente = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == user);

                    var hashedPassword = new Security().EncryptPassword(password);

                    return hashedPassword.Password == cliente.txtPassword;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public UserLogin GetLoginAuthenticationUser(string user, string password)
        {
            try
            {

                var isMail = user.Contains("@");

                using (var ctx = new FoneClubeContext())
                {
                    tblPersons cliente = new tblPersons();
                    bool semPaiDeclarado = false;

                    try
                    {
                        user = user.Replace("-", "").Replace(".", "").Replace("(", "").Replace(")", "");
                        password = password.Replace("-", "").Replace(".", "").Replace("(", "").Replace(")", "");

                        //var ddd = Convert.ToInt32(user.Substring(0, 2));
                        //var number = Convert.ToInt32(user.Substring(2));
                        //var phone = ctx.tblPersonsPhones.Where(p => p.bitPhoneClube == false && p.intDDD == ddd && p.intPhone == number).FirstOrDefault();

                        //if (phone != null)
                        //    cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == phone.intIdPerson);

                        //if (cliente.intIdPerson == 0)
                        cliente = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == user);
                    }
                    catch (Exception)
                    {
                        return new UserLogin();
                    }

                    try
                    {
                        var paternidade = ctx.tblPersonsParents.FirstOrDefault(a => a.intIdSon == cliente.intIdPerson);
                        var foneclube = 4555;
                        semPaiDeclarado = paternidade.intIdParent == foneclube;
                    }
                    catch (Exception) { }

                    try
                    {
                        if (cliente.intIdPerson == 0)
                        {
                            if (isMail)
                                cliente = ctx.tblPersons.FirstOrDefault(p => p.txtEmail == user);
                            else
                                cliente = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == user);
                        }
                    }
                    catch (Exception)
                    {
                        return new UserLogin();
                    }


                    var hashedPassword = new Security().EncryptPassword(password);

                    if (cliente == null)
                        return new UserLogin();

                    if (hashedPassword.Password == cliente.txtPassword)
                    {
                        var customerNames = GetName(cliente.txtName);

                        return new UserLogin { id = cliente.intIdPerson, email = cliente.txtEmail, username = customerNames.Name, cadastroPendente = TemPendencia(cliente), clienteMultinivel = Convert.ToBoolean(cliente.bitMultinivel), indicado = true };
                    }
                    else
                    {
                        return new UserLogin();
                    };
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    new Utils().GetErrorPostMessage("Ocorreu um erro na tentativa de login"));
            }
        }

        public bool GetCustomerIndicado(int matricula)
        {
            try
            {
                bool semPaiDeclarado;

                using (var ctx = new FoneClubeContext())
                {
                    var paternidade = ctx.tblPersonsParents.FirstOrDefault(a => a.intIdSon == matricula);
                    var foneclube = 4555;
                    semPaiDeclarado = paternidade.intIdParent == foneclube || paternidade == null;

                    return !semPaiDeclarado;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    new Utils().GetErrorPostMessage("Ocorreu erro ao tentar coletar cliente"));
            }
        }

        public CustomerMinhaContaViewModel GetCadastrosRealizados(int matricula)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var customer = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == matricula);
                    var phone = ctx.tblPersonsPhones.FirstOrDefault(p => p.intIdPerson == matricula && p.bitPhoneClube == false);
                    var endereco = ctx.tblPersonsAddresses.FirstOrDefault(p => p.intIdPerson == matricula);

                    //var clienteCadastroPendente = customer.txtEmail.Contains("@pendente.com");

                    bool semPaiDeclarado = false;
                    try
                    {
                        var paternidade = ctx.tblPersonsParents.FirstOrDefault(a => a.intIdSon == matricula);
                        var foneclube = 4555;
                        semPaiDeclarado = paternidade.intIdParent == foneclube;
                    }
                    catch (Exception) { }

                    //validar pois ta chamando 2 vezes
                    //todo
                    //colocar registro na tabela pra clientes retroativos, todos sem senha,validar dados por email, e endereco na tabela

                    string contactPhone = string.Empty;

                    if (phone != null)
                        contactPhone = phone.intDDD.ToString() + phone.intPhone.ToString();

                    var enderecoCadastrado = new FoneClube.Business.Commons.Entities.ViewModel.MinhaConta.Endereco();
                    if (endereco != null)
                    {
                        enderecoCadastrado.numero = endereco.intStreetNumber.ToString();
                        enderecoCadastrado.rua = endereco.txtStreet.ToString();
                        enderecoCadastrado.cep = endereco.txtCep.ToString();
                        enderecoCadastrado.bairro = endereco.txtNeighborhood.ToString();
                        enderecoCadastrado.cidade = endereco.txtCity.ToString();
                        enderecoCadastrado.estado = endereco.txtState.ToString();
                        enderecoCadastrado.complemento = endereco.txtComplement.ToString();
                    }

                    //if (clienteCadastroPendente)
                    //{
                    //commented based on this comment from Marcio - The system is enforcing that there must be an email, please remove this and put a fake email whatsappPhone@foneclube.com.br so that the charge will be created.

                    //return new CustomerMinhaContaViewModel
                    //{
                    //    dadosPessoasCadastrados = false,
                    //    enderecoCadastrado = endereco != null,
                    //    senhaCadastrada = Convert.ToBoolean(customer.bitSenhaCadastrada),
                    //    dadosPessoais = new DadosPessoais
                    //    {
                    //        telefone = contactPhone,
                    //        nome = customer.txtName != null ? customer.txtName : string.Empty,
                    //        documento = customer.txtDocumentNumber
                    //    },
                    //    endereco = enderecoCadastrado,
                    //    clienteMultinivel = Convert.ToBoolean(customer.bitMultinivel),
                    //    indicado = !semPaiDeclarado
                    //};
                    //}

                    return new CustomerMinhaContaViewModel
                    {
                        dadosPessoasCadastrados = Convert.ToBoolean(customer.bitDadosPessoaisCadastrados),
                        enderecoCadastrado = endereco != null,
                        senhaCadastrada = Convert.ToBoolean(customer.bitSenhaCadastrada),
                        dadosPessoais = new DadosPessoais
                        {
                            documento = customer.txtDocumentNumber != null ? customer.txtDocumentNumber : string.Empty,
                            email = customer.txtEmail,
                            telefone = contactPhone,
                            nome = customer.txtName != null ? customer.txtName : string.Empty
                        },
                        endereco = enderecoCadastrado,
                        clienteMultinivel = Convert.ToBoolean(customer.bitMultinivel),
                        indicado = !semPaiDeclarado
                    };
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    new Utils().GetErrorPostMessage("Ocorreu um erro na tentativa de login"));
            }
        }

        public bool TemPendencia(tblPersons cliente)
        {
            try
            {
                if (!Convert.ToBoolean(cliente.bitDadosPessoaisCadastrados) || !Convert.ToBoolean(cliente.bitSenhaCadastrada))
                {
                    return true;
                }
                else
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        return !ctx.tblPersonsAddresses.Any(p => p.intIdPerson == cliente.intIdPerson);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool GetRecoverPassword(string recover)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var customer = ctx.tblPersons.FirstOrDefault(p => p.txtEmail == recover);
                    if (customer == null)
                    {
                        customer = ctx.tblPersons.FirstOrDefault(p => p.txtDocumentNumber == recover);
                    }

                    if (customer != null)
                    {
                        var cpf = customer.txtDocumentNumber;
                        var limite = DateTime.Now.AddHours(2).ToString();
                        var format = string.Format("{0}_{1}", cpf, limite);
                        var comando = new Utils().Base64Encode(format);
                        var guid = Guid.NewGuid();
                        var site = @"https://foneclube.com.br/recuperar-senha/" + guid;
                        var mensagem = "Oi prezado cliente, para resetarmos sua senha <strong style='color:#508fe3;font-weight:600'> <a href='" + site + "' target='_blank'>Clique Aqui</a></strong> <br> Caso você não tenha solicitado recuperação de senha ignore este email. <br>  Obrigado";

                        ctx.tblRenovaSenha.Add(new tblRenovaSenha
                        {
                            dteLimite = DateTime.Now.AddHours(3),
                            guidReset = guid,
                            txtDocumento = cpf,
                            txtEmail = customer.txtEmail,
                            intIdPerson = customer.intIdPerson
                        });

                        ctx.SaveChanges();

                        return new Utils().SendEmail(customer.txtEmail, "Recuperação de senha FONECLUBE", mensagem, true);
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public TrocaSenhaViewModel GetMensagemTrocaSenha(string guid)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var guidAtual = Guid.Parse(guid);
                    var registroGuid = ctx.tblRenovaSenha.FirstOrDefault(f => f.guidReset == guidAtual);

                    bool isValidGuid;

                    if (registroGuid == null)
                        isValidGuid = false;
                    else
                        isValidGuid = registroGuid.dteLimite > DateTime.Now;

                    if (isValidGuid)
                    {
                        var senha = new Utils().CreatePassword(6).ToLower();
                        var hashedPassword = new Security().EncryptPassword(senha);
                        var cliente = ctx.tblPersons.FirstOrDefault(p => p.intIdPerson == registroGuid.intIdPerson);
                        cliente.txtPassword = hashedPassword.Password;
                        registroGuid.guidReset = null;
                        ctx.SaveChanges();

                        return new TrocaSenhaViewModel { Sucesso = true, Mensagem = "Sua senha foi alterada, após logar, você pode redefinir a senha na área do cliente em 'Minha Conta'.", Senha = senha };
                    }
                    else
                    {
                        return new TrocaSenhaViewModel { Sucesso = false, Mensagem = "Seu Token de troca de senha expirou ou é inválido, por favor faça novamente ou entre em contato pelo Whatsapp." };
                    }
                }
            }
            catch (Exception e)
            {
                return new TrocaSenhaViewModel { Sucesso = false, Mensagem = "Sua troca de senha falhou por motivos de segurança, faça contato pelo nosso Whatsapp." };
            }
        }
        public HttpStatusCode SaveDefaultPaymentInfo(Person person)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var personInfo = ctx.tblPersons.Where(x => x.intIdPerson == person.Id).FirstOrDefault();
                    if (person.DefaultPaymentDay.HasValue)
                        personInfo.intDftBillPaymentDay = person.DefaultPaymentDay.Value;
                    if (person.DefaultVerificar.HasValue)
                        personInfo.intDftVerificar = person.DefaultVerificar.Value;
                    if (!string.IsNullOrEmpty(person.DefaultWAPhones))
                        personInfo.txtDefaultWAPhones = person.DefaultWAPhones;
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public HttpStatusCode SaveUsar2Preços(int personId, bool status)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var personInfo = ctx.tblPersons.Where(x => x.intIdPerson == personId).FirstOrDefault();
                    personInfo.bitUsar2Preços = status;
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public HttpStatusCode SaveUserSettings(UserSettings userSettings)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var personInfo = ctx.tblUserSettings.Where(x => x.intPerson == userSettings.IntIdPerson).FirstOrDefault();
                    if (personInfo != null)
                    {
                        personInfo.bitPrecoFCSum = userSettings.IsPrecoFCSum;
                        personInfo.bitPrecoPromoSum = userSettings.IsPrecoPromoSum;
                        personInfo.bitUse2Prices = userSettings.IsUse2Prices;
                        personInfo.bitVIP = userSettings.IsVIP;
                        personInfo.bitLinhaAtiva = userSettings.IsLinhaAtiva;
                        personInfo.bitShowICCID = userSettings.IsShowICCID;
                        personInfo.bitShowPort = userSettings.IsShowPort;
                    }
                    else
                    {
                        ctx.tblUserSettings.Add(new tblUserSettings()
                        {
                            intPerson = userSettings.IntIdPerson,
                            bitVIP = userSettings.IsVIP,
                            bitPrecoFCSum = userSettings.IsPrecoFCSum,
                            bitPrecoPromoSum = userSettings.IsPrecoPromoSum,
                            bitUse2Prices = userSettings.IsUse2Prices,
                            bitLinhaAtiva = userSettings.IsLinhaAtiva,
                            bitShowICCID = userSettings.IsShowICCID,
                            bitShowPort = userSettings.IsShowPort
                        });
                    }
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public List<tblAgGridState> GetGridStates(string gridName)
        {
            using (var ctx = new FoneClubeContext())
            {
                return ctx.tblAgGridState.Where(x => x.txtAgidName == gridName).OrderBy(x => x.txtStateName).ToList();
            }
        }

        public HttpStatusCode SaveGridState(tblAgGridState gridState)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    if (gridState.intId != -1)
                    {
                        var state = ctx.tblAgGridState.FirstOrDefault(x => x.intId == gridState.intId);
                        if (state != null)
                        {
                            state.txtAgidName = gridState.txtAgidName;
                            state.txtStateName = gridState.txtStateName;
                            state.txtFilterModel = gridState.txtFilterModel;
                            state.txtSortModel = gridState.txtSortModel;
                            state.txtColumnState = gridState.txtColumnState;
                            state.IsDefault = false;
                            state.IsActive = true;
                        }
                        else
                        {
                            ctx.tblAgGridState.Add(new tblAgGridState()
                            {
                                txtAgidName = gridState.txtAgidName,
                                txtStateName = gridState.txtStateName,
                                txtFilterModel = gridState.txtFilterModel,
                                txtSortModel = gridState.txtSortModel,
                                txtColumnState = gridState.txtColumnState,
                                IsActive = true,
                                IsDefault = false
                            });
                        }
                    }
                    else
                    {
                        ctx.tblAgGridState.Add(new tblAgGridState()
                        {
                            txtAgidName = gridState.txtAgidName,
                            txtStateName = gridState.txtStateName,
                            txtFilterModel = gridState.txtFilterModel,
                            txtSortModel = gridState.txtSortModel,
                            txtColumnState = gridState.txtColumnState,
                            IsActive = true,
                            IsDefault = false
                        });
                    }
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public HttpStatusCode DeleteGridState(tblAgGridState gridState)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var state = ctx.tblAgGridState.Where(x => x.txtAgidName == gridState.txtAgidName && x.intId == gridState.intId).FirstOrDefault();
                    if (state != null)
                    {
                        ctx.tblAgGridState.Remove(state);
                        ctx.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public HttpStatusCode UpdateDefaultState(tblAgGridState gridState)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var up = ctx.tblAgGridState.Where(x => x.txtAgidName == gridState.txtAgidName).ToList();
                    up.ForEach(m => m.IsDefault = false);
                    ctx.SaveChanges();

                    var state = ctx.tblAgGridState.FirstOrDefault(x => x.txtAgidName == gridState.txtAgidName && x.intId == gridState.intId);
                    if (state != null)
                    {
                        state.IsDefault = true;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        var first = ctx.tblAgGridState.FirstOrDefault(x => x.txtAgidName == gridState.txtAgidName);
                        first.IsDefault = true;
                        ctx.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }

        public HttpStatusCode UpdateUnplacedCart(tblUnPlacedCartItems cartItems)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var cart = ctx.tblUnPlacedCartItems.FirstOrDefault(x => x.intIdPerson == cartItems.intIdPerson);
                    if (cart != null)
                    {
                        cart.txtCartItems = cartItems.txtCartItems;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        ctx.tblUnPlacedCartItems.Add(new tblUnPlacedCartItems()
                        {
                            intIdPerson = cartItems.intIdPerson,
                            txtCartItems = cartItems.txtCartItems
                        });
                        ctx.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(
                                new Utils().GetErrorPostMessage(e.InnerException.ToString()));
                }
            }
            return HttpStatusCode.OK;
        }
        public tblUnPlacedCartItems GetUnplacedCart(int person)
        {
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    return ctx.tblUnPlacedCartItems.FirstOrDefault(x => x.intIdPerson == person);

                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public List<StoreOrder> GetCartOrders(int person)
        {
            List<StoreOrder> storeOrders = new List<StoreOrder>();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var orders = ctx.tblStoreOrders.Where(x => x.intIdPerson == person).ToList();
                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            var storeOrder = new StoreOrder();
                            storeOrder.OrderDate = order.dteOrderDate;
                            storeOrder.IdCharge = order.intIdCharge;
                            storeOrder.IdPerson = order.intIdPerson;
                            storeOrder.IdTransaction = order.intIdTransaction.HasValue ? order.intIdTransaction.Value : 0;
                            storeOrder.NumberOfPlans = order.intNumberOfPlans;
                            storeOrder.OrderId = order.intOrderId;
                            storeOrder.PaymentStatus = order.txtPaymentStatus;
                            storeOrder.Status = order.txtStatus;
                            storeOrder.Total = order.txtTotal;
                            storeOrder.Plans = new List<StorePlans>();

                            var plans = ctx.tblStoreCustomerPlans.Where(x => x.intIdPerson == person && x.intOrderId == order.intOrderId);
                            if (plans != null)
                            {
                                foreach (var plan in plans)
                                {
                                    var storePlan = new StorePlans();
                                    storePlan.Id = plan.intId;
                                    storePlan.IdPerson = plan.intIdPerson;
                                    storePlan.ESim = plan.bitESim;
                                    storePlan.Port = plan.bitPort;
                                    storePlan.Activated = plan.bitActivated;
                                    storePlan.PortNumber = plan.txtPortNumber;
                                    storePlan.IdPlan = plan.intIdPlan;
                                    storePlan.PlanDescription = plan.txtPlanDescription;
                                    storePlan.PlanAmount = plan.txtPlanAmount;
                                    storePlan.ChipAmount = plan.txtChipAmount;
                                    storePlan.ShippingAmount = plan.txtShippingAmount;
                                    storePlan.OrderId = plan.intOrderId.Value;

                                    storeOrder.Total += (Convert.ToInt32(plan.txtChipAmount) + Convert.ToInt32(plan.txtShippingAmount));
                                    storeOrder.Plans.Add(storePlan);
                                }
                            }
                            storeOrders.Add(storeOrder);
                        }
                    }

                }
                catch (Exception e)
                {
                }
                return storeOrders;
            }
        }

        private string GetPaymentStatusTransalted(string payStatus)
        {
            string status = string.Empty;
            if (payStatus == "Paid")
            {
                status = "Pago";
            }
            else if (payStatus == "Refunded" || payStatus == "Refused")
            {
                status = "Estornado";
            }
            else
            {
                status = "Aguardando";
            }
            return status;
        }

        public List<StoreCharges> GetChargeHistoryForStore(int personId)
        {
            List<StoreCharges> storeCharges = new List<StoreCharges>();
            using (var ctx = new FoneClubeContext())
            {
                try
                {
                    var charges = ctx.tblChargingHistory.Where(x => x.intIdCustomer == personId && x.bitActive.HasValue && x.bitActive.Value).ToList();
                    if (charges != null && charges.Count > 0)
                    {
                        foreach (var charge in charges)
                        {
                            var paymentStatus = ctx.tblFoneclubePagarmeTransactions.FirstOrDefault(x => x.intIdTransaction == charge.intIdTransaction.Value);
                            StoreCharges storeCharge = new StoreCharges();
                            storeCharge.Id = charge.intId;
                            storeCharge.TransactionId = charge.intIdTransaction.HasValue ? charge.intIdTransaction.Value : 0;
                            storeCharge.Vigencia = charge.dteValidity.HasValue ? charge.dteValidity.Value : (DateTime?)null;
                            storeCharge.Vencimento = charge.dteDueDate.HasValue ? charge.dteDueDate.Value : (DateTime?)null;
                            storeCharge.Total = Convert.ToInt32(charge.txtAmmountPayment);
                            storeCharge.Source = new ChargingAcess().GetTipoLabel(charge.intIdPaymentType.Value);
                            storeCharge.Comment = charge.txtComment;
                            storeCharge.Status = paymentStatus != null ? GetPaymentStatusTransalted(paymentStatus.txtOutdadetStatus) : "Aguardando";
                            storeCharge.PaymentDate = paymentStatus != null && paymentStatus.txtOutdadetStatus == "Paid" ? paymentStatus.dteDate_updated.Value.ToString(@"dd MMM", new CultureInfo("PT-br")) : "";
                            storeCharges.Add(storeCharge);
                        }
                    }

                    var phones = ctx.tblPersonsPhones.Where(x => x.intIdPerson == personId && x.bitPhoneClube.HasValue && x.bitPhoneClube.Value && x.intIdOperator == 4 && x.bitAtivo.HasValue && x.bitAtivo.Value).Select(x => x.intDDD + "" + x.intPhone).ToList();
                    var paymentsFromContel = (from hist in ctx.tblContelTopupHistory
                                              join phone in phones on hist.txtLinha equals phone
                                              where hist.txtFormaPagto == "Boleto" || hist.txtFormaPagto == "Pix Recebimento" || hist.txtFormaPagto == "Cartão de crédito" || hist.txtFormaPagto == "Cartão de crédito | recorrência cartão"
                                              select hist).ToList();
                    if (paymentsFromContel != null && paymentsFromContel.Count > 0)
                    {
                        foreach (var payment in paymentsFromContel)
                        {
                            var topupDate = DateTime.ParseExact(payment.txtDataRecarga, "yyyy-MM-dd HH:mm:ss.fff",
                                                              System.Globalization.CultureInfo.InvariantCulture);

                            StoreCharges storeCharge = new StoreCharges();
                            storeCharge.Id = 0;
                            storeCharge.TransactionId = 0;
                            storeCharge.Vigencia = (DateTime?)null;
                            storeCharge.Vencimento = (DateTime?)null;
                            storeCharge.Total = Convert.ToInt32(payment.txtValor.Replace(".", ""));
                            storeCharge.Source = "CONTEL";
                            storeCharge.Comment = "";
                            storeCharge.Status = "Pago";
                            storeCharge.PaymentDate = topupDate.ToString(@"dd MMM", new CultureInfo("PT-br"));
                            storeCharges.Add(storeCharge);
                        }
                    }
                }
                catch (HttpResponseException erro)
                {
                }
                return storeCharges;
            }
        }

        public string InitiateRefund(InitiateRefund refund)
        {
            string status = string.Empty;
            int idPerson = 0;
            MVNOAccess mVNOAccess = new MVNOAccess();
            try
            {
                if (refund != null && refund.IsRefund)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var purchases = ctx.tblInternationalUserPurchases.FirstOrDefault(x => x.intId == refund.Id && x.bitRefund.HasValue && !x.bitRefund.Value);
                        if (purchases != null)
                        {
                            idPerson = purchases.intIdPerson;
                            purchases.bitRefund = true;
                            //purchases.txtComments = refund.Comment;
                            ctx.SaveChanges();

                            var client = ctx.tblPersons.Where(x => x.intIdPerson == purchases.intIdPerson).FirstOrDefault();
                            tblContelPlanMapping plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == purchases.intIdPerson && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                            if (plan is null)
                                plan = ctx.tblContelPlanMapping.Where(x => x.intIdPerson == 1 && x.txtPlanName == purchases.txtPlan).FirstOrDefault();

                            if (refund.Action == "Refund")
                            {
                                var saldo = mVNOAccess.GetSaldo(refund.Phone);

                                // Saldo less than plan - User consumed some MB - No refund
                                if (saldo != null && saldo.data != null && saldo.data.restante_dados < plan.intDataMB.Value)
                                {
                                    var poolData = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtPhoneNumber == refund.Phone && x.txtICCID == purchases.txtICCID);
                                    if (poolData is null)
                                    {
                                        ctx.tblInternationActivationPool.Add(new tblInternationActivationPool()
                                        {
                                            intIdPerson = client.intIdPerson,
                                            bitFailedPostActivation = false,
                                            bitReadyForReActivation = false,
                                            intIdPlan = plan.intIdPlan,
                                            txtICCID = purchases.txtICCID,
                                            txtPhoneNumber = purchases.txtPhone,
                                            txtResetStatus = "Not Applicable",
                                            txtStatus = "Refuse Refund: Data Used",
                                            dteActivation = purchases.dteDeducted
                                        });
                                        ctx.SaveChanges();
                                    }
                                    return string.Format("Refuse Refund: Data Used : {0} MB out of {1} is used by Customer", (plan.intDataMB.Value / 1024), (saldo.data.restante_dados / 1024));
                                }
                                else
                                {
                                    //// 2 days old
                                    ctx.tblInternationalDeposits.Add(new tblInternationalDeposits()
                                    {
                                        bitRefund = true,
                                        dteDateAdded = DateTime.Now,
                                        intFinalValue = purchases.intAmountDeducted,
                                        intIdPaymentType = "Refund",
                                        intIdPerson = purchases.intIdPerson,
                                        txtComment = !string.IsNullOrEmpty(refund.Comment) ? refund.Comment : string.Format("Refunded by FACIL for Phone:{0} on {1}", refund.Phone, DateTime.Now.ToShortDateString()),
                                        intUSDAmount = purchases.intAmountDeducted
                                    });
                                    ctx.SaveChanges();

                                    var user = ctx.tblInternationalUserBalance.FirstOrDefault(x => x.intIdPerson == client.intIdPerson);
                                    if (user != null)
                                    {
                                        user.intAmountBalance = user.intAmountBalance + purchases.intAmountDeducted;
                                        ctx.SaveChanges();
                                    }

                                    var pool = ctx.tblInternationActivationPool.FirstOrDefault(x => x.txtPhoneNumber == refund.Phone && x.txtICCID == purchases.txtICCID);
                                    if (pool is null)
                                    {
                                        var isLessThan2Days = (DateTime.Now - purchases.dteDeducted).TotalDays <= 2;
                                        if (isLessThan2Days)
                                        {

                                            try
                                            {
                                                var dataPool = new tblInternationActivationPool()
                                                {
                                                    intIdPerson = client.intIdPerson,
                                                    bitFailedPostActivation = true,
                                                    bitReadyForReActivation = true,
                                                    intIdPlan = plan.intIdPlan,
                                                    txtICCID = purchases.txtICCID,
                                                    txtResetICCID = purchases.txtICCID,
                                                    txtPhoneNumber = purchases.txtPhone,
                                                    dteReset = DateTime.Now,
                                                    intIdPersonReset = 1,
                                                    txtResetStatus = "Pending",
                                                    txtStatus = "Resell Pool + Blocked",
                                                    dteActivation = purchases.dteDeducted
                                                };
                                                ctx.tblInternationActivationPool.Add(dataPool);
                                                ctx.SaveChanges();

                                                int poolId = dataPool.intId;

                                                ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                                {
                                                    intActivationPoolId = poolId,
                                                    txtICCID = purchases.txtICCID,
                                                    txtPhone = purchases.txtPhone,
                                                    txtStatus = "Resell Pool + Blocked",
                                                    dteAction = DateTime.Now,
                                                    txtDoneBy = "System"
                                                });
                                                ctx.SaveChanges();
                                            }
                                            catch (Exception ex)
                                            {
                                                LogHelper.LogMessage(purchases.intIdPerson, string.Format("Error adding to Pool: {0}", ex.ToString()));
                                            }

                                            //LogHelper.LogMessage(purchases.intIdPerson, string.Format("ProfileAccess:InitiateRefund :: Resetting iccid for number : " + purchases.txtPhone));

                                            //ResetLine resetReq = new ResetLine()
                                            //{
                                            //    linha = purchases.txtPhone,
                                            //    motivo = "Troca para eSIM",
                                            //    novo_iccid = ""
                                            //};
                                            //var result = new MVNOAccess().ResetLine(resetReq);

                                            //if (result != null && !string.IsNullOrEmpty(result.message))
                                            //{
                                            //    if (string.Equals(result.message, "Troca de chip realizada com sucesso.", StringComparison.OrdinalIgnoreCase))
                                            //    {
                                            //        LogHelper.LogMessage(purchases.intIdPerson, string.Format("ProfileAccess:InitiateRefund :: Downloading PDF : {0}" , purchases.txtPhone));

                                            //        string iccid = string.Empty, activationcode =  string.Empty;
                                            //        var qrcode = new MVNOAccess().DownloadActivationFileContel(result.esim_pdf, purchases.txtPhone, ref iccid, ref activationcode);
                                            //        LogHelper.LogMessage(purchases.intIdPerson, string.Format("ProfileAccess:InitiateRefund :: ESim Resetted successfully for number : {0}, old iccid:{1}, new iccid:{2} ", purchases.txtPhone, purchases.txtICCID, iccid));

                                            //        var tblEsim = ctx.tbleSimActivationInfo.FirstOrDefault(x => x.txtLinha == purchases.txtPhone && x.txtICCID == iccid);
                                            //        if (tblEsim != null)
                                            //        {
                                            //            tblEsim.txtActivationCode = activationcode;
                                            //            tblEsim.txtActivationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                            //            tblEsim.txtActivationImage = qrcode;
                                            //            tblEsim.txtActivationPdfUrl = result.esim_pdf;
                                            //            tblEsim.txtICCID = iccid;
                                            //            tblEsim.txtLinha = purchases.txtPhone;
                                            //            tblEsim.dteInsert = DateTime.Now;
                                            //        }
                                            //        else
                                            //        {
                                            //            ctx.tbleSimActivationInfo.Add(new tbleSimActivationInfo()
                                            //            {
                                            //                txtActivationCode = activationcode,
                                            //                txtActivationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                            //                txtActivationImage = qrcode,
                                            //                txtActivationPdfUrl = result.esim_pdf,
                                            //                txtICCID = iccid,
                                            //                txtLinha = purchases.txtPhone,
                                            //                dteInsert = DateTime.Now
                                            //            });
                                            //        }
                                            //        ctx.SaveChanges();


                                            //        LogHelper.LogMessage(purchases.intIdPerson, string.Format("Reset request for line: {0} Resetted Successfully", purchases.txtPhone));

                                            //        try
                                            //        {
                                            //            var dataPool = new tblInternationActivationPool()
                                            //            {
                                            //                intIdPerson = client.intIdPerson,
                                            //                bitFailedPostActivation = true,
                                            //                bitReadyForReActivation = true,
                                            //                intIdPlan = plan.intIdPlan,
                                            //                txtICCID = iccid,
                                            //                txtResetICCID = purchases.txtICCID,
                                            //                txtPhoneNumber = purchases.txtPhone,
                                            //                dteReset = DateTime.Now,
                                            //                intIdPersonReset = 1,
                                            //                txtResetStatus = "Pending",
                                            //                txtStatus = "Resell Pool + Blocked",
                                            //                dteActivation = purchases.dteDeducted
                                            //            };
                                            //            ctx.tblInternationActivationPool.Add(dataPool);
                                            //            ctx.SaveChanges();

                                            //            int poolId = dataPool.intId;

                                            //            ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                            //            {
                                            //                intActivationPoolId = poolId,
                                            //                txtICCID = iccid,
                                            //                txtPhone = purchases.txtPhone,
                                            //                txtStatus = "Resell Pool + Blocked",
                                            //                dteAction = DateTime.Now,
                                            //                txtDoneBy = "System"
                                            //            });
                                            //            ctx.SaveChanges();
                                            //        }
                                            //        catch (Exception ex)
                                            //        {
                                            //            LogHelper.LogMessage(purchases.intIdPerson, string.Format("Error adding to Pool: {0}", ex.ToString()));
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        LogHelper.LogMessage(purchases.intIdPerson, string.Format("Reset API throws error: {0}", result.message));
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    LogHelper.LogMessage(purchases.intIdPerson, string.Format("Reset API throws error result or result.message is null"));
                                            //}
                                        }
                                        else
                                        {
                                            tblInternationActivationPool dataPool = new tblInternationActivationPool()
                                            {
                                                intIdPerson = client.intIdPerson,
                                                bitFailedPostActivation = false,
                                                bitReadyForReActivation = false,
                                                intIdPlan = plan.intIdPlan,
                                                txtICCID = purchases.txtICCID,
                                                txtPhoneNumber = purchases.txtPhone,
                                                txtResetStatus = "Not Applicable",
                                                txtStatus = "Request Refund to Contel",
                                                dteActivation = purchases.dteDeducted,
                                                txtResetICCID = purchases.txtICCID
                                            };
                                            ctx.tblInternationActivationPool.Add(dataPool);
                                            ctx.SaveChanges();

                                            int poolId = dataPool.intId;

                                            ctx.tblInternationActivationPoolLog.Add(new tblInternationActivationPoolLog()
                                            {
                                                intActivationPoolId = poolId,
                                                txtICCID = purchases.txtICCID,
                                                txtPhone = purchases.txtPhone,
                                                txtStatus = "Request Refund to Contel",
                                                dteAction = DateTime.Now,
                                                txtDoneBy = "System"
                                            });
                                            ctx.SaveChanges();
                                        }

                                        BlockLine blockLine = new BlockLine()
                                        {
                                            numero = refund.Phone,
                                            motivo = "BLOQUEIO DE IMEI",
                                            observacoes = ""
                                        };
                                        mVNOAccess.BlockLine(blockLine);
                                    }
                                    else
                                    {
                                        pool.txtStatus = "Resell Pool + Blocked";
                                        pool.txtResetStatus = "Pending";
                                        pool.intIdPlan = plan.intIdPlan;
                                        pool.bitFailedPostActivation = true;
                                        pool.bitReadyForReActivation = true;
                                        ctx.SaveChanges();
                                    }

                                }
                            }
                            status = "Refunded successfully and balance added to Client :" + client.txtName;
                        }
                        else
                        {
                            status = "Invalid data";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage(idPerson, string.Format("ProfileAccess:InitiateRefund::Error: {0}", ex.ToString()));
            }
            return status;
        }
    }
}
