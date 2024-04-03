using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using FoneClube.Business.Commons;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.ViewModel;

namespace FoneClube.DataAccess.Utilities
{
    public static class WhatsAppUtil
    {
        const string CB_InviteCheck = "InviteCheck";
        const string CB_UserPhoneCheck = "UserPhoneCheck";
        const string CB_CPFCheck = "CPFCheck";
        const string CB_Registration = "Registration";

        const string US_Unregistered = "Unregistered";
        const string US_Registered = "Registered";
        const string US_Client = "Client";

        const string TMP_RegisteredUser = "RegisteredUser";
        const string TMP_ActiveClient = "ActiveClient";
        const string TMP_CPFAPICallSuccessWithNoName = "CPFAPICallSuccessWithNoName";
        const string TMP_InvalidCPFNumber = "InvalidCPFNumber";
        const string TMP_RegisteredAlready = "RegisteredAlready";
        const string TMP_UnregisteredNewUser = "UnregisteredNewUser";

        public static void ProcessWhatsAppResponse(WhatsAppWebHookResponse response)
        {
            if (response != null)
            {
                switch (response.@event)
                {
                    case "onmessage":
                        {
                            switch (response.ack)
                            {
                                case 1:
                                    {
                                        ProcessReply(response.body, response.from);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private static void ProcessReply(string message, string from)
        {
            if (!string.IsNullOrEmpty(message))
            {
                #region Incoming message
                long isCpfNumber = 0;
                try
                {
                    long.TryParse(message, out isCpfNumber);
                }
                catch { }

                try
                {
                    string tempMessage = message;
                    isCpfNumber = Convert.ToInt64(tempMessage.Replace(".", "").Replace("-", ""));
                }
                catch { }

                int parentId = 0;
                string cpfNumber = string.Empty;
                string operation = string.Empty;

                if (message.Contains("https://foneclube.com.br/convite/"))
                {
                    var url = Regex.Match(message, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
                    if (url != null && !string.IsNullOrEmpty(url.Value))
                    {
                        var match = url.Value.Split('/').LastOrDefault();
                        if (!string.IsNullOrEmpty(match))
                        {
                            var intId = System.Text.Encoding.GetEncoding(28591).GetString(Convert.FromBase64String(match));
                            parentId = Convert.ToInt32(intId);
                            operation = "Invite";

                            UpdateWhatsAppBotUserData(from);
                        }
                    }
                }
                else if (isCpfNumber != 0 && message.Length > 10 && message.Length < 20)
                {
                    if (ValidaCNPJOrCPF.IsCpf(message) || (ValidaCNPJOrCPF.IsCnpj(message)))
                    {
                        cpfNumber = Regex.Replace(message, "[^0-9]+", "");
                        operation = "ValidCPFNumber";
                        UpdateWhatsAppBotUserData(from, cpfNumber);
                    }
                    else
                    {
                        operation = "InvalidCPFNumber";
                    }
                }
                else
                {
                    operation = message;
                }

                #endregion

                var template = GetTemplateByTrigger(operation);

                if (template != null)
                {
                    PerformCallBackAction(template, from, parentId, cpfNumber);
                }
            }
        }

        public static tblWhatsAppMessageTemplates GetTemplateByTrigger(string trigger)
        {
            using (var ctx = new FoneClubeContext())
            {
                tblWhatsAppMessageTemplates selectedTemplate = null;
                foreach (var template in ctx.tblWhatsAppMessageTemplates.ToList())
                {
                    if (template.txtTrigger.Split(',').Any(y => y.Trim().ToLower().Equals(trigger.Trim().ToLower())) && template.bitInternal.HasValue && template.bitInternal.Value)
                    {
                        selectedTemplate = template;
                        break;
                    }
                }
                if (selectedTemplate != null)
                {
                    return selectedTemplate;
                }
            }
            return null;
        }

        public static void PerformCallBackAction(tblWhatsAppMessageTemplates template, string from, int parentId, string cpfNumber)
        {
            ProfileAccess profileAccess = new ProfileAccess();
            WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
            WhatsAppMessage whatsAppMessage = new WhatsAppMessage();

            if (!string.IsNullOrEmpty(template.txtCallBackAction))
            {
                switch (template.txtCallBackAction)
                {
                    case CB_InviteCheck:
                        {
                            string fatherVariable = string.Empty;

                            var personName = profileAccess.GetPersonMinimal(parentId);
                            if (personName != null)
                            {
                                fatherVariable = personName.Name;
                                var phoneNum = Regex.Replace(from, "[^0-9]+", string.Empty);
                                var userStatus = whatsAppAccess.CheckUserStatus(phoneNum);
                                if (!string.IsNullOrEmpty(userStatus))
                                {
                                    string userType = userStatus.Split('|')[0];
                                    string userName = userStatus.Split('|')[1];

                                    UpdateWhatsAppBotUserData(from, string.Empty, userName);

                                    switch (userType)
                                    {
                                        case US_Unregistered:
                                            {
                                                template.txtComment = template.txtComment.Replace("fathervariable", fatherVariable);
                                                whatsAppMessage = Helper.FormatRequest(template, from);
                                            }
                                            break;
                                        case US_Registered:
                                            {
                                                template = GetTemplateByTrigger(TMP_RegisteredUser);
                                                template.txtComment = template.txtComment.Replace("namevariable", userName);
                                                whatsAppMessage = Helper.FormatRequest(template, from);
                                            }
                                            break;
                                        case US_Client:
                                            {
                                                template = GetTemplateByTrigger(TMP_ActiveClient);
                                                template.txtComment = template.txtComment.Replace("namevariable", userName);
                                                whatsAppMessage = Helper.FormatRequest(template, from);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                    case CB_UserPhoneCheck:
                        {
                            var phoneNum = Regex.Replace(from, "[^0-9]+", string.Empty);
                            var userStatus = whatsAppAccess.CheckUserStatus(phoneNum);
                            if (!string.IsNullOrEmpty(userStatus))
                            {
                                string userType = userStatus.Split('|')[0];
                                string userName = userStatus.Split('|')[1];

                                UpdateWhatsAppBotUserData(from, string.Empty, userName);

                                switch (userType)
                                {
                                    case US_Unregistered:
                                        {
                                            template = GetTemplateByTrigger(TMP_UnregisteredNewUser);
                                            whatsAppMessage = Helper.FormatRequest(template, from);
                                        }
                                        break;
                                    case US_Registered:
                                        {
                                            template = GetTemplateByTrigger(TMP_RegisteredUser);
                                            template.txtComment = template.txtComment.Replace("namevariable", userName);
                                            whatsAppMessage = Helper.FormatRequest(template, from);
                                        }
                                        break;
                                    case US_Client:
                                        {
                                            template = GetTemplateByTrigger(TMP_ActiveClient);
                                            template.txtComment = template.txtComment.Replace("namevariable", userName);
                                            whatsAppMessage = Helper.FormatRequest(template, from);
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case CB_CPFCheck:
                        {
                            var cpfResponse = ValidateCPF(cpfNumber);
                            if (cpfResponse != null && cpfResponse.status == 1)
                            {
                                string cpfName = "Test " + cpfNumber;
                                if (!string.IsNullOrEmpty(cpfResponse.nome))
                                {
                                    cpfName = cpfResponse.nome;
                                    UpdateWhatsAppBotUserData(from, cpfNumber, cpfResponse.nome);
                                    template.txtComment = template.txtComment.Replace("namevariable", cpfName);
                                    whatsAppMessage = Helper.FormatRequest(template, from);
                                }
                                else
                                {
                                    template = GetTemplateByTrigger(TMP_CPFAPICallSuccessWithNoName);
                                    whatsAppMessage = Helper.FormatRequest(template, from);
                                }
                            }
                            else
                            {
                                template = GetTemplateByTrigger(TMP_InvalidCPFNumber);
                                whatsAppMessage = Helper.FormatRequest(template, from);
                            }
                        }
                        break;
                    case CB_Registration:
                        {
                            string name = string.Empty;
                            var data = RegisterUser(from, out name);
                            if (data == 0)
                            {
                                template.txtComment = template.txtComment.Replace("namevariable", name);
                                whatsAppMessage = Helper.FormatRequest(template, from);
                            }
                            else if (data == 1)
                            {
                                template = GetTemplateByTrigger(TMP_RegisteredAlready);
                                template.txtComment = template.txtComment.Replace("namevariable", name);
                                whatsAppMessage = Helper.FormatRequest(template, from);
                            }
                        }
                        break;
                    default:
                        {
                            var phoneNum = Regex.Replace(from, "[^0-9]+", string.Empty);
                            var tblPerson = profileAccess.GetPersonyPhone(phoneNum);
                            if (tblPerson != null)
                            {
                                var tblchargingHistory = profileAccess.GetlastChargingHistory(tblPerson.intIdPerson);
                                var tblPagarme = profileAccess.GetLastPaymentByTransId(tblchargingHistory.intIdTransaction.Value);
                                whatsAppMessage = Helper.FormatRequest(template, tblchargingHistory, tblPerson, tblPagarme);
                            }
                            else
                            {
                                whatsAppMessage = Helper.FormatRequest(template, from);
                            }
                        }
                        break;
                }
            }
            else
            {
                var phoneNum = Regex.Replace(from, "[^0-9]+", string.Empty);
                var tblPerson = profileAccess.GetPersonyPhone(phoneNum);
                if (tblPerson != null)
                {
                    var tblchargingHistory = profileAccess.GetlastChargingHistory(tblPerson.intIdPerson);
                    if (tblchargingHistory != null)
                    {
                        var tblPagarme = profileAccess.GetLastPaymentByTransId(tblchargingHistory.intIdTransaction.Value);
                        whatsAppMessage = Helper.FormatRequest(template, tblchargingHistory, tblPerson, tblPagarme);
                    }
                    else
                    {
                        whatsAppMessage = Helper.FormatRequest(template, from);
                    }
                }
                else
                {
                    whatsAppMessage = Helper.FormatRequest(template, from);
                }
            }
            if (!string.IsNullOrEmpty(template.txtMessageType))
            {
                switch (template.txtMessageType.ToLower())
                {
                    case "button":
                        whatsAppAccess.SendMessageWithButton(whatsAppMessage);
                        break;
                    case "url":
                        whatsAppAccess.SendMessageWithButtonUrl(whatsAppMessage, 0, 0);
                        break;
                    case "list":
                        whatsAppAccess.SendMessageWithButtonList(whatsAppMessage);
                        break;
                    case "text":
                        whatsAppAccess.SendMessage(whatsAppMessage);
                        break;
                    default:
                        break;
                }
            }

        }

        public static CPFResponse ValidateCPF(string number)
        {
            CPFResponse cpfResponse = new CPFResponse();
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.cpfcnpj.com.br");

                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("/74817fbeb42c87d0a61f20684d3309e3/1/" + number).Result;
                if (response.IsSuccessStatusCode)
                {
                    cpfResponse = response.Content.ReadAsAsync<CPFResponse>().Result;
                }

                client.Dispose();
            }
            catch
            { }
            return cpfResponse;
        }

        public static void UpdateWhatsAppBotUserData(string from, string cpfNumber = "", string userName = "")
        {
            using (var ctx = new FoneClubeContext())
            {
                var userData = ctx.tblWhatsAppBotUserData.Where(x => x.txtPhone == from).FirstOrDefault();
                if (userData == null)
                {
                    ctx.tblWhatsAppBotUserData.Add(new tblWhatsAppBotUserData()
                    {
                        txtPhone = from,
                        txtCPFNumber = cpfNumber,
                        txtName = userName
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(cpfNumber))
                        userData.txtCPFNumber = cpfNumber;
                    if (!string.IsNullOrEmpty(userName))
                        userData.txtName = userName;
                }
                ctx.SaveChanges();
            }
        }

        public static int RegisterUser(string from, out string name)
        {
            WhatsAppAccess whatsAppAccess = new WhatsAppAccess();
            name = string.Empty;
            int status = -1;
            try
            {
                var phoneNum = Regex.Replace(from, "[^0-9]+", string.Empty);
                var userStatus = whatsAppAccess.CheckUserStatus(phoneNum);
                if (!string.IsNullOrEmpty(userStatus))
                {
                    string userType = userStatus.Split('|')[0];
                    string userName = userStatus.Split('|')[1];
                    if (US_Unregistered == userType)
                    {
                        using (var ctx = new FoneClubeContext())
                        {
                            var userData = ctx.tblWhatsAppBotUserData.Where(x => x.txtPhone == from).FirstOrDefault();
                            if (userData != null)
                            {
                                CustomerCrossRegisterViewModel model = new CustomerCrossRegisterViewModel();
                                model.phone = Regex.Replace(from, "[^0-9]+", string.Empty);
                                model.documento = userData.txtCPFNumber;
                                model.documentType = "CPF";
                                model.name = userData.txtName;
                                model.email = userData.txtCPFNumber + "@foneclube.com.br";
                                model.password = model.documento;
                                model.confirmPassword = model.documento;

                                name = userData.txtName;

                                ProfileAccess profile = new ProfileAccess();
                                var data = profile.InsertNewCustomerRegisterCross(model);
                                if (data != null)
                                {
                                    if (data.Status == HttpStatusCode.OK)
                                    {
                                        status = 0;
                                    }
                                    if (data.Status == HttpStatusCode.MethodNotAllowed)
                                    {
                                        status = 1;
                                    }
                                }
                            }
                        }
                    }
                    else if (US_Registered == userType || US_Client == userType)
                    {
                        name = userName;
                        status = 1;
                    }
                }
            }
            catch (Exception ex) { }

            return status;
        }
    }



    public static class ValidaCNPJOrCPF
    {
        public static bool IsCnpj(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito = string.Empty;
            string tempCnpj;
            try
            {
                cnpj = cnpj.Trim();
                cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
                if (cnpj.Length != 14)
                    return false;
                tempCnpj = cnpj.Substring(0, 12);
                soma = 0;
                for (int i = 0; i < 12; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = resto.ToString();
                tempCnpj = tempCnpj + digito;
                soma = 0;
                for (int i = 0; i < 13; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = digito + resto.ToString();
            }
            catch (Exception ex)
            {
            }
            return cnpj.EndsWith(digito);
        }

        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito = string.Empty;
            int soma;
            int resto;
            try
            {
                cpf = cpf.Trim();
                cpf = cpf.Replace(".", "").Replace("-", "");
                if (cpf.Length != 11)
                    return false;
                tempCpf = cpf.Substring(0, 9);
                soma = 0;

                for (int i = 0; i < 9; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = resto.ToString();
                tempCpf = tempCpf + digito;
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = digito + resto.ToString();
            }
            catch (Exception ex)
            {
            }
            return cpf.EndsWith(digito);
        }
    }
}
