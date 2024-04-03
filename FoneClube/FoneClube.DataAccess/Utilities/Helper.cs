using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.DataAccess.affiliates;
using System.Globalization;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoneClube.DataAccess.Utilities
{
    public static class Helper
    {
        public static string SendChargeSummary(tblChargingHistory chargingHistory, string phoneNumbers = null)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var templateList = ctx.tblWhatsAppMessageTemplates.ToList();
                    tblWhatsAppMessageTemplates selectedTemplate = null;
                    foreach (var template in templateList)
                    {
                        if (chargingHistory.intIdPaymentType == 1)
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("cccharged")))
                            {
                                selectedTemplate = template;
                            }
                        }
                        else if (chargingHistory.intIdPaymentType == 2)
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("chargesummaryBoleto")))
                            {
                                selectedTemplate = template;
                            }
                        }
                        else
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("chargesummary")))
                            {
                                selectedTemplate = template;
                            }
                        }
                    }

                    if (selectedTemplate != null)
                    {
                        var transactions = new TransactionAccess().GetAllLastTransactions();
                        var transactionPagarme = transactions.FirstOrDefault(l => l.intIdTransaction == chargingHistory.intIdTransaction);
                        var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == chargingHistory.intIdCustomer);
                        if (!string.IsNullOrEmpty(person.txtDefaultWAPhones) || !string.IsNullOrEmpty(phoneNumbers))
                        {
                            WhatsAppAccess objAccess = new WhatsAppAccess();
                            WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, chargingHistory, person, transactionPagarme, "", phoneNumbers);
                            List<string> replies = null;
                            if (!string.IsNullOrEmpty(selectedTemplate.txtMessageType))
                            {
                                var configsettings = ctx.tblConfigSettings.ToList();
                                switch (selectedTemplate.txtMessageType.ToLower())
                                {
                                    case "button":
                                        {
                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                replies = objAccess.SendMessageWithButtonForChargeSummary(whatsAppMessageSummary, chargingHistory.intId, person.intIdPerson);
                                            else
                                            {
                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                if (stat)
                                                    replies = new List<string>() { "Y" };
                                                else
                                                    replies = new List<string>() { "N" };
                                            }
                                        }
                                        break;
                                    case "url":
                                        {
                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, chargingHistory.intId, person.intIdPerson);
                                            else
                                            {
                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                if (stat)
                                                    replies = new List<string>() { "Y" };
                                                else
                                                    replies = new List<string>() { "N" };
                                            }
                                        }
                                        break;
                                    case "list":
                                        {
                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                            else
                                            {
                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                if (stat)
                                                    replies = new List<string>() { "Y" };
                                                else
                                                    replies = new List<string>() { "N" };
                                            }
                                        }
                                        break;
                                    case "text":
                                        {
                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                            if (stat)
                                                replies = new List<string>() { "Y" };
                                            else
                                                replies = new List<string>() { "N" };
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (replies != null)
                                {
                                    if (replies.All(x => x == replies.First() && x == "Y"))
                                        return "Sent";
                                    else if (replies.All(x => x == replies.First() && x == "N"))
                                        return "Error";
                                    else
                                        return "Partial";
                                }
                                else
                                    return "Error";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
            return "Error";
        }
        public static string SendChargeSummaryText(tblChargingHistory chargingHistory, string phoneNumbers = null)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var templateList = ctx.tblWhatsAppMessageTemplates.ToList();
                    tblWhatsAppMessageTemplates selectedTemplate = null;
                    foreach (var template in templateList)
                    {
                        if (chargingHistory.intIdPaymentType == 1)
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("cccharged")))
                            {
                                selectedTemplate = template;
                            }
                        }
                        else if (chargingHistory.intIdPaymentType == 2)
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("chargesummaryBoleto")))
                            {
                                selectedTemplate = template;
                            }
                        }
                        else
                        {
                            if (template.txtTrigger.Split(',').Any(y => y.Equals("chargesummary.txt")))
                            {
                                selectedTemplate = template;
                            }
                        }
                    }

                    if (selectedTemplate != null)
                    {
                        var transactions = new TransactionAccess().GetAllLastTransactions();
                        var transactionPagarme = transactions.FirstOrDefault(l => l.intIdTransaction == chargingHistory.intIdTransaction);
                        var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == chargingHistory.intIdCustomer);
                        if (!string.IsNullOrEmpty(person.txtDefaultWAPhones) || !string.IsNullOrEmpty(phoneNumbers))
                        {
                            WhatsAppAccess objAccess = new WhatsAppAccess();
                            WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, chargingHistory, person, transactionPagarme, "", phoneNumbers);
                            List<string> replies = null;
                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                            if (stat)
                                replies = new List<string>() { "Y" };
                            else
                                replies = new List<string>() { "N" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
            return "Error";
        }
        public static string SendMarketingMessage(GenericTemplate marketing)
        {
            try
            {

                switch (marketing.TypeId)
                {
                    case 1:
                        {
                            using (var ctx = new FoneClubeContext())
                            {
                                var configsettings = ctx.tblConfigSettings.ToList();

                                var templateList = ctx.tblWhatsAppMessageTemplates.ToList();
                                tblWhatsAppMessageTemplates selectedTemplate = null;
                                foreach (var template in templateList)
                                {
                                    if (template.txtTrigger.Split(',').Any(y => y.Equals("t.marketing1")))
                                    {
                                        selectedTemplate = template;
                                    }
                                }

                                if (selectedTemplate != null)
                                {
                                    var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == marketing.PersonId);

                                    if (!string.IsNullOrEmpty(person.txtDefaultWAPhones) || !string.IsNullOrEmpty(marketing.PhoneNumbers))
                                    {
                                        WhatsAppAccess objAccess = new WhatsAppAccess();
                                        WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, null, person, null, marketing.Invitee, marketing.PhoneNumbers);
                                        if (!string.IsNullOrEmpty(selectedTemplate.txtMessageType))
                                        {
                                            List<string> replies = null;
                                            switch (selectedTemplate.txtMessageType.ToLower())
                                            {
                                                case "button":
                                                    {
                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                            replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                                        else
                                                        {
                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                            if (stat)
                                                                replies = new List<string>() { "Y" };
                                                            else
                                                                replies = new List<string>() { "N" };
                                                        }
                                                    }
                                                    break;
                                                case "url":
                                                    {
                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                            replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, person.intIdPerson);
                                                        else
                                                        {
                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                            if (stat)
                                                                replies = new List<string>() { "Y" };
                                                            else
                                                                replies = new List<string>() { "N" };
                                                        }
                                                    }
                                                    break;
                                                case "list":
                                                    {
                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                            replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                                        else
                                                        {
                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                            if (stat)
                                                                replies = new List<string>() { "Y" };
                                                            else
                                                                replies = new List<string>() { "N" };
                                                        }
                                                    }
                                                    break;
                                                case "text":
                                                    {
                                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                        replies = new List<string>() { "Y" };
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (replies != null)
                                            {
                                                if (replies.All(x => x == replies.First() && x == "Y"))
                                                    return "Sent";
                                                else if (replies.All(x => x == replies.First() && x == "N"))
                                                    return "Error";
                                                else
                                                    return "Partial";
                                            }
                                            else
                                                return "Error";
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            using (var ctx = new FoneClubeContext())
                            {
                                var configsettings = ctx.tblConfigSettings.ToList();

                                var templateList = ctx.tblWhatsAppMessageTemplates.ToList();
                                tblWhatsAppMessageTemplates selectedTemplate = null;
                                foreach (var template in templateList)
                                {
                                    if (template.txtTrigger.Split(',').Any(y => y.Equals("t.marketing2")))
                                    {
                                        selectedTemplate = template;
                                    }
                                }

                                if (selectedTemplate != null)
                                {
                                    var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == marketing.PersonId);
                                    if (!string.IsNullOrEmpty(person.txtDefaultWAPhones) || !string.IsNullOrEmpty(marketing.PhoneNumbers))
                                    {
                                        if (!string.IsNullOrEmpty(marketing.Invitee))
                                        {
                                            var splitInvitees = marketing.Invitee.Split(',');
                                            if (splitInvitees.Length > 0)
                                            {
                                                foreach (var invitee in splitInvitees)
                                                {
                                                    var splitphoneName = invitee.Split('|');
                                                    if (splitInvitees.Length > 0)
                                                    {
                                                        WhatsAppAccess objAccess = new WhatsAppAccess();
                                                        WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, null, person, null, splitInvitees[1], marketing.PhoneNumbers);

                                                        if (!string.IsNullOrEmpty(selectedTemplate.txtMessageType))
                                                        {
                                                            List<string> replies = null;
                                                            switch (selectedTemplate.txtMessageType.ToLower())
                                                            {
                                                                case "button":
                                                                    {
                                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                            replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                                                        else
                                                                        {
                                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                            if (stat)
                                                                                replies = new List<string>() { "Y" };
                                                                            else
                                                                                replies = new List<string>() { "N" };
                                                                        }
                                                                    }
                                                                    break;
                                                                case "url":
                                                                    {
                                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                            replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, person.intIdPerson);
                                                                        else
                                                                        {
                                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                            if (stat)
                                                                                replies = new List<string>() { "Y" };
                                                                            else
                                                                                replies = new List<string>() { "N" };
                                                                        }
                                                                    }
                                                                    break;
                                                                case "list":
                                                                    {
                                                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                            replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                                                        else
                                                                        {
                                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                            if (stat)
                                                                                replies = new List<string>() { "Y" };
                                                                            else
                                                                                replies = new List<string>() { "N" };
                                                                        }
                                                                    }
                                                                    break;
                                                                case "text":
                                                                    {
                                                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                        replies = new List<string>() { "Y" };
                                                                    }
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            if (replies != null)
                                                            {
                                                                if (replies.All(x => x == replies.First() && x == "Y"))
                                                                    return "Sent";
                                                                else if (replies.All(x => x == replies.First() && x == "N"))
                                                                    return "Error";
                                                                else
                                                                    return "Partial";
                                                            }
                                                            else
                                                                return "Error";
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                WhatsAppAccess objAccess = new WhatsAppAccess();
                                                WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, null, person, null, null, marketing.PhoneNumbers);

                                                if (!string.IsNullOrEmpty(selectedTemplate.txtMessageType))
                                                {
                                                    List<string> replies = null;
                                                    switch (selectedTemplate.txtMessageType.ToLower())
                                                    {
                                                        case "button":
                                                            {
                                                                if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                    replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                                                else
                                                                {
                                                                    var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                    if (stat)
                                                                        replies = new List<string>() { "Y" };
                                                                    else
                                                                        replies = new List<string>() { "N" };
                                                                }
                                                            }
                                                            break;
                                                        case "url":
                                                            {
                                                                if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                    replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, person.intIdPerson);
                                                                else
                                                                {
                                                                    var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                    if (stat)
                                                                        replies = new List<string>() { "Y" };
                                                                    else
                                                                        replies = new List<string>() { "N" };
                                                                }
                                                            }
                                                            break;
                                                        case "list":
                                                            {
                                                                if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                    replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                                                else
                                                                {
                                                                    var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                    if (stat)
                                                                        replies = new List<string>() { "Y" };
                                                                    else
                                                                        replies = new List<string>() { "N" };
                                                                }
                                                            }
                                                            break;
                                                        case "text":
                                                            {
                                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                replies = new List<string>() { "Y" };
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                    if (replies != null)
                                                    {
                                                        if (replies.All(x => x == replies.First() && x == "Y"))
                                                            return "Sent";
                                                        else if (replies.All(x => x == replies.First() && x == "N"))
                                                            return "Error";
                                                        else
                                                            return "Partial";
                                                    }
                                                    else
                                                        return "Error";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            WhatsAppAccess objAccess = new WhatsAppAccess();
                                            WhatsAppMessage whatsAppMessageSummary = FormatRequest(selectedTemplate, null, person, null, null, marketing.PhoneNumbers);

                                            if (!string.IsNullOrEmpty(selectedTemplate.txtMessageType))
                                            {
                                                List<string> replies = null;
                                                switch (selectedTemplate.txtMessageType.ToLower())
                                                {
                                                    case "button":
                                                        {
                                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                                            else
                                                            {
                                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                if (stat)
                                                                    replies = new List<string>() { "Y" };
                                                                else
                                                                    replies = new List<string>() { "N" };
                                                            }
                                                        }
                                                        break;
                                                    case "url":
                                                        {
                                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, person.intIdPerson);
                                                            else
                                                            {
                                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                if (stat)
                                                                    replies = new List<string>() { "Y" };
                                                                else
                                                                    replies = new List<string>() { "N" };
                                                            }
                                                        }
                                                        break;
                                                    case "list":
                                                        {
                                                            if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                                replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                                            else
                                                            {
                                                                var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                                if (stat)
                                                                    replies = new List<string>() { "Y" };
                                                                else
                                                                    replies = new List<string>() { "N" };
                                                            }
                                                        }
                                                        break;
                                                    case "text":
                                                        {
                                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                            replies = new List<string>() { "Y" };
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                if (replies != null)
                                                {
                                                    if (replies.All(x => x == replies.First() && x == "Y"))
                                                        return "Sent";
                                                    else if (replies.All(x => x == replies.First() && x == "N"))
                                                        return "Error";
                                                    else
                                                        return "Partial";
                                                }
                                                else
                                                    return "Error";
                                            }
                                        }
                                    }
                                }
                                else
                                    return "Template not available";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
            return "Error";
        }
        public static string SendGenericMessage(GenericTemplate marketing)
        {
            try
            {
                if (marketing.Template != null)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        tblChargingHistory chargeHistory = new tblChargingHistory();

                        if (marketing.Template.TemplateName.ToLower().Contains("creditcard"))
                            chargeHistory = ctx.tblChargingHistory.Where(c => c.intIdCustomer == marketing.PersonId && c.intIdPaymentType == 1).ToList().OrderByDescending(x => x.dteCreate).FirstOrDefault();
                        else
                            chargeHistory = ctx.tblChargingHistory.Where(c => c.intIdCustomer == marketing.PersonId && (c.intIdPaymentType == 2 || c.intIdPaymentType == 3)).ToList().OrderByDescending(x => x.dteCreate).FirstOrDefault();

                        if (chargeHistory != null)
                        {
                            var whatsAppTemplates = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.txtTemplateName.ToLower() == marketing.Template.TemplateName.ToLower());

                            if (whatsAppTemplates != null)
                            {
                                var person = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == marketing.PersonId);
                                if (!string.IsNullOrEmpty(person.txtDefaultWAPhones) || !string.IsNullOrEmpty(marketing.PhoneNumbers))
                                {
                                    WhatsAppAccess objAccess = new WhatsAppAccess();
                                    WhatsAppMessage whatsAppMessageSummary = FormatRequest(marketing, whatsAppTemplates, chargeHistory, person, null);

                                    if (!string.IsNullOrEmpty(whatsAppTemplates.txtMessageType))
                                    {
                                        List<string> replies = null;
                                        var configsettings = ctx.tblConfigSettings.ToList();
                                        switch (whatsAppTemplates.txtMessageType.ToLower())
                                        {
                                            case "button":
                                                {
                                                    if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                        replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                                    else
                                                    {
                                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                        if (stat)
                                                            replies = new List<string>() { "Y" };
                                                        else
                                                            replies = new List<string>() { "N" };
                                                    }
                                                }
                                                break;
                                            case "url":
                                                {
                                                    if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                        replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, person.intIdPerson);
                                                    else
                                                    {
                                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                        if (stat)
                                                            replies = new List<string>() { "Y" };
                                                        else
                                                            replies = new List<string>() { "N" };
                                                    }
                                                }
                                                break;
                                            case "list":
                                                {
                                                    if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                                        replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                                    else
                                                    {
                                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                        if (stat)
                                                            replies = new List<string>() { "Y" };
                                                        else
                                                            replies = new List<string>() { "N" };
                                                    }
                                                }
                                                break;
                                            case "text":
                                                {
                                                    var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                                    if (stat)
                                                        replies = new List<string>() { "Y" };
                                                    else
                                                        replies = new List<string>() { "N" };
                                                }
                                                break;
                                            default:
                                                break;
                                        }

                                        if (replies.All(x => x == replies.First() && x == "Y"))
                                            return "Sent";
                                        else if (replies.All(x => x == replies.First() && x == "N"))
                                            return "Error";
                                        else
                                            return "Partial";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return "Error";
        }
        public static string SendWelcomeMessage(WelcomeTemplate welcomeTemplate)
        {
            if (welcomeTemplate.Template != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    var whatsAppTemplates = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.txtTemplateName.ToLower() == welcomeTemplate.Template.TemplateName.ToLower());
                    if (whatsAppTemplates != null)
                    {
                        if (!string.IsNullOrEmpty(whatsAppTemplates.txtMessageType))
                        {
                            WhatsAppAccess objAccess = new WhatsAppAccess();
                            WhatsAppMessage whatsAppMessageSummary = FormatRequestWelcome(whatsAppTemplates, welcomeTemplate.Name, welcomeTemplate.PhoneNumbers);

                            List<string> replies = null;
                            var configsettings = ctx.tblConfigSettings.ToList();
                            switch (whatsAppTemplates.txtMessageType.ToLower())
                            {
                                case "button":
                                    {
                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseButton" && Convert.ToBoolean(x.txtConfigValue) == true))
                                            replies = objAccess.SendMessageWithButton(whatsAppMessageSummary);
                                        else
                                        {
                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                            if (stat)
                                                replies = new List<string>() { "Y" };
                                            else
                                                replies = new List<string>() { "N" };
                                        }
                                    }
                                    break;
                                case "url":
                                    {
                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseURL" && Convert.ToBoolean(x.txtConfigValue) == true))
                                            replies = objAccess.SendMessageWithButtonUrl(whatsAppMessageSummary, 0, 0);
                                        else
                                        {
                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                            if (stat)
                                                replies = new List<string>() { "Y" };
                                            else
                                                replies = new List<string>() { "N" };
                                        }
                                    }
                                    break;
                                case "list":
                                    {
                                        if (configsettings.Any(x => x.txtConfigName == "WhatsAppTemplateUseList" && Convert.ToBoolean(x.txtConfigValue) == true))
                                            replies = objAccess.SendMessageWithButtonList(whatsAppMessageSummary);
                                        else
                                        {
                                            var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                            if (stat)
                                                replies = new List<string>() { "Y" };
                                            else
                                                replies = new List<string>() { "N" };
                                        }
                                    }
                                    break;
                                case "text":
                                    {
                                        var stat = objAccess.SendMessage(whatsAppMessageSummary);
                                        replies = new List<string>() { "Y" };
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (replies.All(x => x == replies.First() && x == "Y"))
                                return "Sent";
                            else if (replies.All(x => x == replies.First() && x == "N"))
                                return "Error";
                            else
                                return "Partial";
                        }
                    }
                }
            }
            return "Error";
        }
        public static string ReplaceMessage(string message, tblChargingHistory charge, tblPersons client, tblFoneclubePagarmeTransactions transactionPagarme, string invitees = null)
        {
            try
            {
                message = message.Replace("namevariable", client.txtName);
                if (charge != null)
                {
                    var vencimentoday = charge.dteDueDate.HasValue ? charge.dteDueDate.Value.ToString(@"dd", new CultureInfo("PT-br")) : string.Empty;
                    var vencimentomonthTemp = charge.dteDueDate.HasValue ? charge.dteDueDate.Value.ToString(@"MMMM", new CultureInfo("PT-br")) : string.Empty;
                    var vencimentomonth = !string.IsNullOrEmpty(vencimentomonthTemp) ? char.ToUpper(vencimentomonthTemp[0]) + vencimentomonthTemp.Substring(1) : string.Empty;
                    var vencimento = (!string.IsNullOrEmpty(vencimentoday) && !string.IsNullOrEmpty(vencimentomonth)) ? string.Format(@"{0} de {1}", vencimentoday, vencimentomonth) : string.Empty;

                    var vigenciaYear = charge.dteValidity.HasValue ? charge.dteValidity.Value.ToString(@"yyyy", new CultureInfo("PT-br")) : string.Empty;
                    var vigenciamonthTemp = charge.dteValidity.HasValue ? charge.dteValidity.Value.ToString(@"MMMM", new CultureInfo("PT-br")) : string.Empty;
                    var vigenciamonth = !string.IsNullOrEmpty(vigenciamonthTemp) ? char.ToUpper(vigenciamonthTemp[0]) + vigenciamonthTemp.Substring(1) : string.Empty;
                    var vigencia = (!string.IsNullOrEmpty(vigenciamonth) && !string.IsNullOrEmpty(vigenciaYear)) ? string.Format(@"{0} de {1}", vigenciamonth, vigenciaYear) : string.Empty;

                    var amount = "R$" + charge.txtAmmountPayment.Insert(charge.txtAmmountPayment.Length - 2, ".");
                    string tipo = "-";
                    message = message.Replace("commentvariable", string.IsNullOrEmpty(charge.txtChargingComment) ? "" : string.Format("*{0}*", charge.txtChargingComment));
                    message = message.Replace("vencimentovariable", vencimento);
                    message = message.Replace("vigenciavariable", vigencia);
                    message = message.Replace("amountvariable", Convert.ToString(amount));
                    message = message.Replace("summaryurlvariable", string.Format("https://webfc.foneclube.com.br/#/resumocobranca/{0}/{1}", charge.intIdCustomer, charge.intId));
                    message = message.Replace("chargeidvariable", charge.intId.ToString());
                    message = message.Replace("currentdatevariable", DateTime.Now.ToString("dd/MMM/yy hh:mm"));

                    if (charge.intIdPaymentType.Value == 3)
                    {
                        message = message.Replace("pixcodevariable", charge.pixCode);
                        message = message.Replace("qrcodelinkvariable", "http://api.foneclube.com.br/api/pagarme/pix/qrcode/" + charge.intId);
                    }
                    else if (charge.intIdPaymentType.Value == 2)
                    {
                        //Transaction details won't be available until refresh db - Pagerme
                        message = message.Replace("qrcodevariable", charge.txtboletoBarcode);
                        message = message.Replace("linkvariable", charge.txtboletoUrl);
                        message = message.Replace("codigoboletovariable", charge.txtboletoBarcode);
                        message = message.Replace("urlboletovariable", charge.txtboletoUrl);
                    }
                    else
                    {
                        message = message.Replace("pixcodevariable", "");
                        message = message.Replace("qrcodevariable", "");
                        message = message.Replace("qrcodelinkvariable", "");
                        message = message.Replace("linkvariable", "");
                        message = message.Replace("codigoboletovariable", "");
                        message = message.Replace("urlboletovariable", "");
                    }


                    switch (charge.intIdPaymentType.Value)
                    {
                        case 1:
                            {
                                if (transactionPagarme != null)
                                    tipo = "Cartão " + transactionPagarme.txtCard_last_digits;
                                else
                                    tipo = "Cartão";
                            }
                            break;
                        case 2:
                            {
                                tipo = "Boleto";
                            }
                            break;
                        case 3:
                            {
                                tipo = "Pix";
                            }
                            break;
                    }
                    message = message.Replace("tipovariable", tipo);

                }
                message = message.Replace("inviteevariable", string.IsNullOrEmpty(invitees) ? "" : invitees);
                message = message.Replace("invitelinkvariable", new Affiliates().GetReferralLink(client.intIdPerson));
                message = message.Replace("currentdatevariable", DateTime.Now.ToString("dd/MMM/yy hh:mm"));


                using (var ctx = new FoneClubeContext())
                {
                    string parentName = "";
                    var parent = ctx.tblPersonsParents.Where(x => x.intIdSon == client.intIdPerson).FirstOrDefault();
                    if (parent != null)
                    {
                        parentName = ctx.tblPersons.Where(x => x.intIdPerson == parent.intIdParent).FirstOrDefault().txtName;
                    }

                    message = message.Replace("fathervariable", parentName);
                }
            }
            catch (Exception ex)
            {

            }
            return message;
        }
        public static string ReplaceMessageScheduled(string message, tblChargingScheduled charge, tblPersons client, tblFoneclubePagarmeTransactions transactionPagarme, string invitees = null)
        {
            try
            {
                message = message.Replace("namevariable", client.txtName);
                if (charge != null)
                {
                    var vencimentoday = charge.dteDueDate.HasValue ? charge.dteDueDate.Value.ToString(@"dd", new CultureInfo("PT-br")) : string.Empty;
                    var vencimentomonthTemp = charge.dteDueDate.HasValue ? charge.dteDueDate.Value.ToString(@"MMMM", new CultureInfo("PT-br")) : string.Empty;
                    var vencimentomonth = char.ToUpper(vencimentomonthTemp[0]) + vencimentomonthTemp.Substring(1);
                    var vencimento = (!string.IsNullOrEmpty(vencimentoday) && !string.IsNullOrEmpty(vencimentomonth)) ? string.Format(@"{0} de {1}", vencimentoday, vencimentomonth) : string.Empty;

                    var vigenciaYear = charge.dteValidity.HasValue ? charge.dteValidity.Value.ToString(@"yyyy", new CultureInfo("PT-br")) : string.Empty;
                    var vigenciamonthTemp = charge.dteValidity.HasValue ? charge.dteValidity.Value.ToString(@"MMMM", new CultureInfo("PT-br")) : string.Empty;
                    var vigenciamonth = char.ToUpper(vigenciamonthTemp[0]) + vigenciamonthTemp.Substring(1);
                    var vigencia = (!string.IsNullOrEmpty(vigenciamonth) && !string.IsNullOrEmpty(vigenciaYear)) ? string.Format(@"{0} de {1}", vigenciamonth, vigenciaYear) : string.Empty;

                    var amount = "R$" + charge.txtAmmountPayment.Insert(charge.txtAmmountPayment.Length - 2, ".");

                    message = message.Replace("commentvariable", string.IsNullOrEmpty(charge.txtChargingComment) ? "" : string.Format("*{0}*", charge.txtChargingComment));
                    message = message.Replace("vencimentovariable", vencimento);
                    message = message.Replace("vigenciavariable", vigencia);
                    message = message.Replace("amountvariable", Convert.ToString(amount));
                    message = message.Replace("summaryurlvariable", string.Format("https://webfc.foneclube.com.br/#/resumocobranca/{0}/{1}", charge.intIdCustomer, charge.intId));
                    message = message.Replace("chargeidvariable", charge.intId.ToString());
                    message = message.Replace("currentdatevariable", DateTime.Now.ToString("dd/MMM/yy hh:mm"));

                    if (charge.intIdPaymentType.Value == 3)
                    {
                        message = message.Replace("pixcodevariable", charge.pixCode);
                        message = message.Replace("qrcodelinkvariable", "http://api.foneclube.com.br/api/pagarme/pix/qrcode/" + charge.intId);
                    }
                    else
                    {
                        message = message.Replace("pixcodevariable", "");
                        message = message.Replace("qrcodevariable", "");
                        message = message.Replace("qrcodelinkvariable", "");
                        message = message.Replace("linkvariable", "");
                    }
                }
                message = message.Replace("inviteevariable", string.IsNullOrEmpty(invitees) ? "" : invitees);
                message = message.Replace("invitelinkvariable", new Affiliates().GetReferralLink(client.intIdPerson));
                message = message.Replace("currentdatevariable", DateTime.Now.ToString("dd/MMM/yy hh:mm"));


                using (var ctx = new FoneClubeContext())
                {
                    string parentName = "";
                    var parent = ctx.tblPersonsParents.Where(x => x.intIdSon == client.intIdPerson).FirstOrDefault();
                    if (parent != null)
                    {
                        parentName = ctx.tblPersons.Where(x => x.intIdPerson == parent.intIdParent).FirstOrDefault().txtName;
                    }

                    message = message.Replace("fathervariable", parentName);
                }
            }
            catch (Exception ex)
            {

            }
            return message;
        }
        public static string ReplaceFooterHeaderMsg(string message)
        {
            message = message.Replace("currentdatevariable", DateTime.Now.ToString("dd/MMM/yy hh:mm"));
            return message;
        }
        public static WhatsAppMessage FormatRequest(GenericTemplate marketing, tblWhatsAppMessageTemplates watemplate, tblChargingHistory chargeHistory, tblPersons person, string invites = null)
        {
            WhatsAppMessage whatsAppMessage = new WhatsAppMessage();
            if (marketing != null && marketing.Template != null)
            {
                var templateCommentFromUI = marketing.Template.Comment;
                var templateTitleFromUI = marketing.Template.Title;
                var templateFooterFromUI = marketing.Template.Footer;
                var templateButtonsFromUI = marketing.Template.Buttons;
                var templateUrlsFromUI = marketing.Template.Urls;
                var templateListButtonFromUI = marketing.Template.ListButton;
                var templateListSectionsFromUI = marketing.Template.ListSections;
                var templateListSectionRowsFromUI = marketing.Template.ListSectionRows;

                using (var ctx = new FoneClubeContext())
                {
                    tblWhatsAppMessageTemplates whatsAppTemplates = new tblWhatsAppMessageTemplates();

                    if (watemplate != null)
                        whatsAppTemplates = watemplate;
                    else
                        whatsAppTemplates = ctx.tblWhatsAppMessageTemplates.FirstOrDefault(x => x.txtTemplateName.ToLower() == marketing.Template.TemplateName.ToLower());

                    if (whatsAppTemplates != null)
                    {
                        switch (whatsAppTemplates.txtMessageType.ToLower())
                        {
                            case "text":
                                whatsAppMessage.ClientIds = string.IsNullOrEmpty(marketing.PhoneNumbers) ? person.txtDefaultWAPhones : marketing.PhoneNumbers;
                                whatsAppMessage.Message = ReplaceMessage(string.IsNullOrEmpty(templateCommentFromUI) ? whatsAppTemplates.txtComment : templateCommentFromUI, chargeHistory, person, null, invites);
                                break;
                            case "button":
                                if (!string.IsNullOrEmpty(whatsAppTemplates.txtButtons))
                                {
                                    ButtonData objButtonData = new ButtonData();
                                    objButtonData.title = whatsAppTemplates.txtTitle;
                                    objButtonData.footer = whatsAppTemplates.txtFooter;
                                    objButtonData.buttons = new List<ButtonRows>();
                                    foreach (var btn in ReplaceMessage(whatsAppTemplates.txtButtons, chargeHistory, person, null, invites).Split(',').ToList())
                                    {
                                        ButtonRows buttonRows = new ButtonRows();
                                        buttonRows.text = btn;
                                        objButtonData.buttons.Add(buttonRows);
                                    }
                                    whatsAppMessage.ClientIds = string.IsNullOrEmpty(marketing.PhoneNumbers) ? person.txtDefaultWAPhones : marketing.PhoneNumbers;
                                    whatsAppMessage.Title = string.IsNullOrEmpty(templateTitleFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtTitle) : ReplaceFooterHeaderMsg(templateTitleFromUI);
                                    whatsAppMessage.Footer = string.IsNullOrEmpty(templateFooterFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtFooter) : ReplaceFooterHeaderMsg(templateFooterFromUI);
                                    whatsAppMessage.Message = ReplaceMessage(string.IsNullOrEmpty(templateCommentFromUI) ? whatsAppTemplates.txtComment : templateCommentFromUI, chargeHistory, person, null, invites);

                                    whatsAppMessage.ButtonList = objButtonData;
                                }
                                break;
                            case "url":
                                if (!string.IsNullOrEmpty(whatsAppTemplates.txtUrls))
                                {
                                    UrlData objButtonData = new UrlData();
                                    objButtonData.title = whatsAppTemplates.txtTitle;
                                    objButtonData.footer = whatsAppTemplates.txtFooter;
                                    objButtonData.buttonsUrl = new List<ButtonUrlRows>();
                                    foreach (var btn in ReplaceMessage(whatsAppTemplates.txtUrls, chargeHistory, person, null, invites).Split(',').ToList())
                                    {
                                        ButtonUrlRows buttonRows = new ButtonUrlRows();
                                        if (btn.Split('|').Length > 1)
                                        {
                                            buttonRows.text = btn.Split('|')[0];
                                            buttonRows.url = btn.Split('|')[1];
                                        }
                                        else
                                            buttonRows.text = btn.Split('|')[0];
                                        objButtonData.buttonsUrl.Add(buttonRows);
                                    }
                                    whatsAppMessage.ClientIds = string.IsNullOrEmpty(marketing.PhoneNumbers) ? person.txtDefaultWAPhones : marketing.PhoneNumbers;
                                    whatsAppMessage.Title = string.IsNullOrEmpty(templateTitleFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtTitle) : ReplaceFooterHeaderMsg(templateTitleFromUI);
                                    whatsAppMessage.Footer = string.IsNullOrEmpty(templateFooterFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtFooter) : ReplaceFooterHeaderMsg(templateFooterFromUI);
                                    whatsAppMessage.Message = ReplaceMessage(string.IsNullOrEmpty(templateCommentFromUI) ? whatsAppTemplates.txtComment : templateCommentFromUI, chargeHistory, person, null, invites);

                                    whatsAppMessage.UrlList = objButtonData;
                                }
                                break;
                            case "list":
                                if (!string.IsNullOrEmpty(templateListSectionsFromUI))
                                {
                                    List<ListSection> sections = new List<ListSection>();
                                    int loopCount = 0;
                                    var secs = ReplaceMessage(whatsAppTemplates.txtListSections, chargeHistory, person, null, invites);
                                    var secrows = ReplaceMessage(whatsAppTemplates.txtListSectionRows, chargeHistory, person, null, invites);

                                    //Format: Section1|Section2 - Section1Rows|Section2Rows: T1|D1||T2|D2||T3|D3 ||| T1|D1||T2|D2||T3|D3
                                    foreach (var section in secs.Split('|').ToList())
                                    {
                                        var objSection = new ListSection();
                                        objSection.Title = section.Trim();

                                        if (!string.IsNullOrEmpty(secrows))
                                        {
                                            var splitRows = secrows.Split(new string[] { "|||" }, StringSplitOptions.None);
                                            if (splitRows != null && splitRows.Count() > 0 && splitRows[loopCount] != null)
                                            {
                                                var splitSectionRows = splitRows[loopCount].Split(new string[] { "||" }, StringSplitOptions.None);
                                                List<ListRow> rows = new List<ListRow>();
                                                foreach (var srrow in splitSectionRows)
                                                {
                                                    if (srrow != null)
                                                    {
                                                        ListRow objRow = new ListRow();
                                                        var splitTitleDesc = srrow.Trim().Split('|');
                                                        objRow.Title = splitTitleDesc[0].Trim();
                                                        objRow.Description = splitTitleDesc[1].Trim();
                                                        rows.Add(objRow);
                                                    }
                                                }
                                                objSection.Rows = rows;
                                                loopCount++;
                                            }
                                        }
                                        sections.Add(objSection);
                                    }
                                    whatsAppMessage.ClientIds = string.IsNullOrEmpty(marketing.PhoneNumbers) ? person.txtDefaultWAPhones : marketing.PhoneNumbers;
                                    whatsAppMessage.Title = string.IsNullOrEmpty(templateTitleFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtTitle) : ReplaceFooterHeaderMsg(templateTitleFromUI);
                                    whatsAppMessage.Footer = string.IsNullOrEmpty(templateFooterFromUI) ? Helper.ReplaceFooterHeaderMsg(whatsAppTemplates.txtFooter) : ReplaceFooterHeaderMsg(templateFooterFromUI);
                                    whatsAppMessage.Message = ReplaceMessage(string.IsNullOrEmpty(templateCommentFromUI) ? whatsAppTemplates.txtComment : templateCommentFromUI, chargeHistory, person, null, invites);

                                    whatsAppMessage.SendList = new ListData()
                                    {
                                        ButtonText = !string.IsNullOrEmpty(whatsAppTemplates.txtListButton) ? ReplaceMessage(whatsAppTemplates.txtListButton, chargeHistory, person, null, invites) : "",
                                        Description = ReplaceMessage(string.IsNullOrEmpty(templateCommentFromUI) ? whatsAppTemplates.txtComment : templateCommentFromUI, chargeHistory, person, null, invites),
                                        Sections = sections
                                    };
                                }
                                break;
                        }
                    }
                }
            }
            return whatsAppMessage;
        }
        public static WhatsAppMessage FormatRequest(tblWhatsAppMessageTemplates template, tblChargingHistory chargeHistory, tblPersons person, tblFoneclubePagarmeTransactions transactionPagarme, string invites = null, string addPhones = null)
        {
            WhatsAppMessage whatsAppMessage = new WhatsAppMessage();
            if (template != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    switch (template.txtMessageType.ToLower())
                    {
                        case "text":
                            whatsAppMessage.ClientIds = string.IsNullOrEmpty(addPhones) ? person.txtDefaultWAPhones : addPhones;
                            whatsAppMessage.Message = ReplaceMessage(template.txtComment, chargeHistory, person, transactionPagarme, invites);
                            break;
                        case "button":
                            if (!string.IsNullOrEmpty(template.txtButtons))
                            {
                                ButtonData objButtonData = new ButtonData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttons = new List<ButtonRows>();
                                foreach (var btn in ReplaceMessage(template.txtButtons, chargeHistory, person, transactionPagarme, invites).Split(',').ToList())
                                {
                                    ButtonRows buttonRows = new ButtonRows();
                                    buttonRows.text = btn;
                                    objButtonData.buttons.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = string.IsNullOrEmpty(addPhones) ? person.txtDefaultWAPhones : addPhones;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = ReplaceMessage(template.txtComment, chargeHistory, person, transactionPagarme, invites);

                                whatsAppMessage.ButtonList = objButtonData;
                            }
                            break;
                        case "url":
                            if (!string.IsNullOrEmpty(template.txtUrls))
                            {
                                UrlData objButtonData = new UrlData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttonsUrl = new List<ButtonUrlRows>();
                                foreach (var btn in Helper.ReplaceMessage(template.txtUrls, chargeHistory, person, transactionPagarme, invites).Split(',').ToList())
                                {
                                    ButtonUrlRows buttonRows = new ButtonUrlRows();
                                    if (btn.Split('|').Length > 1)
                                    {
                                        buttonRows.text = btn.Split('|')[0];
                                        buttonRows.url = btn.Split('|')[1];
                                    }
                                    else
                                        buttonRows.text = btn.Split('|')[0];
                                    objButtonData.buttonsUrl.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = string.IsNullOrEmpty(addPhones) ? person.txtDefaultWAPhones : addPhones;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = ReplaceMessage(template.txtComment, chargeHistory, person, transactionPagarme, invites);

                                whatsAppMessage.UrlList = objButtonData;
                            }
                            break;
                        case "list":
                            if (!string.IsNullOrEmpty(template.txtListSections))
                            {
                                List<ListSection> sections = new List<ListSection>();
                                int loopCount = 0;
                                var secs = ReplaceMessage(template.txtListSections, chargeHistory, person, transactionPagarme, invites);
                                var secrows = ReplaceMessage(template.txtListSectionRows, chargeHistory, person, transactionPagarme, invites);

                                //Format: Section1|Section2 - Section1Rows|Section2Rows: T1|D1||T2|D2||T3|D3 ||| T1|D1||T2|D2||T3|D3
                                foreach (var section in secs.Split('|').ToList())
                                {
                                    var objSection = new ListSection();
                                    objSection.Title = section.Trim();

                                    if (!string.IsNullOrEmpty(secrows))
                                    {
                                        var splitRows = secrows.Split(new string[] { "|||" }, StringSplitOptions.None);
                                        if (splitRows != null && splitRows.Count() > 0 && splitRows[loopCount] != null)
                                        {
                                            var splitSectionRows = splitRows[loopCount].Split(new string[] { "||" }, StringSplitOptions.None);
                                            List<ListRow> rows = new List<ListRow>();
                                            foreach (var srrow in splitSectionRows)
                                            {
                                                if (srrow != null)
                                                {
                                                    ListRow objRow = new ListRow();
                                                    var splitTitleDesc = srrow.Trim().Split('|');
                                                    objRow.Title = splitTitleDesc[0].Trim();
                                                    objRow.Description = splitTitleDesc[1].Trim();
                                                    rows.Add(objRow);
                                                }
                                            }
                                            objSection.Rows = rows;
                                        }
                                    }
                                    sections.Add(objSection);
                                    loopCount++;
                                }
                                whatsAppMessage.ClientIds = string.IsNullOrEmpty(addPhones) ? person.txtDefaultWAPhones : addPhones;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = ReplaceMessage(template.txtComment, chargeHistory, person, transactionPagarme, invites);

                                whatsAppMessage.SendList = new ListData()
                                {
                                    ButtonText = ReplaceMessage(template.txtListButton, chargeHistory, person, transactionPagarme, invites),
                                    Description = ReplaceMessage(template.txtComment, chargeHistory, person, transactionPagarme, invites),
                                    Sections = sections
                                };
                            }
                            break;
                    }
                }
            }
            return whatsAppMessage;
        }
        public static WhatsAppMessage FormatRequestWelcome(tblWhatsAppMessageTemplates template, string name, string phoneNumbers)
        {
            WhatsAppMessage whatsAppMessage = new WhatsAppMessage();
            if (template != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    switch (template.txtMessageType.ToLower())
                    {
                        case "text":
                            whatsAppMessage.ClientIds = phoneNumbers;
                            whatsAppMessage.Message = template.txtComment.Replace("namevariable", name);
                            break;
                        case "button":
                            if (!string.IsNullOrEmpty(template.txtButtons))
                            {
                                ButtonData objButtonData = new ButtonData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttons = new List<ButtonRows>();
                                foreach (var btn in template.txtButtons.Split(',').ToList())
                                {
                                    ButtonRows buttonRows = new ButtonRows();
                                    buttonRows.text = btn;
                                    objButtonData.buttons.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = phoneNumbers;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment.Replace("namevariable", name);

                                whatsAppMessage.ButtonList = objButtonData;
                            }
                            break;
                        case "url":
                            if (!string.IsNullOrEmpty(template.txtUrls))
                            {
                                UrlData objButtonData = new UrlData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttonsUrl = new List<ButtonUrlRows>();
                                foreach (var btn in template.txtUrls.Split(',').ToList())
                                {
                                    ButtonUrlRows buttonRows = new ButtonUrlRows();
                                    if (btn.Split('|').Length > 1)
                                    {
                                        buttonRows.text = btn.Split('|')[0];
                                        buttonRows.url = btn.Split('|')[1];
                                    }
                                    else
                                        buttonRows.text = btn.Split('|')[0];
                                    objButtonData.buttonsUrl.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = phoneNumbers;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment.Replace("namevariable", name);

                                whatsAppMessage.UrlList = objButtonData;
                            }
                            break;
                        case "list":
                            if (!string.IsNullOrEmpty(template.txtListSections))
                            {
                                List<ListSection> sections = new List<ListSection>();
                                int loopCount = 0;

                                //Format: Section1|Section2 - Section1Rows|Section2Rows: T1|D1||T2|D2||T3|D3 ||| T1|D1||T2|D2||T3|D3
                                foreach (var section in template.txtListSections.Split('|').ToList())
                                {
                                    var objSection = new ListSection();
                                    objSection.Title = section.Trim();

                                    if (!string.IsNullOrEmpty(template.txtListSectionRows))
                                    {
                                        var splitRows = template.txtListSectionRows.Split(new string[] { "|||" }, StringSplitOptions.None);
                                        if (splitRows != null && splitRows.Count() > 0 && splitRows[loopCount] != null)
                                        {
                                            var splitSectionRows = splitRows[loopCount].Split(new string[] { "||" }, StringSplitOptions.None);
                                            List<ListRow> rows = new List<ListRow>();
                                            foreach (var srrow in splitSectionRows)
                                            {
                                                if (srrow != null)
                                                {
                                                    ListRow objRow = new ListRow();
                                                    var splitTitleDesc = srrow.Trim().Split('|');
                                                    objRow.Title = splitTitleDesc[0].Trim();
                                                    objRow.Description = splitTitleDesc[1].Trim();
                                                    rows.Add(objRow);
                                                }
                                            }
                                            objSection.Rows = rows;
                                        }
                                    }
                                    sections.Add(objSection);
                                    loopCount++;
                                }
                                whatsAppMessage.ClientIds = phoneNumbers;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment.Replace("namevariable", name);

                                whatsAppMessage.SendList = new ListData()
                                {
                                    ButtonText = template.txtComment.Replace("namevariable", name),
                                    Description = template.txtComment.Replace("namevariable", name),
                                    Sections = sections
                                };
                            }
                            break;
                    }
                }
            }
            return whatsAppMessage;
        }
        public static WhatsAppMessage FormatRequest(tblWhatsAppMessageTemplates template, string phone)
        {
            WhatsAppMessage whatsAppMessage = new WhatsAppMessage();
            if (template != null)
            {
                using (var ctx = new FoneClubeContext())
                {
                    switch (template.txtMessageType.ToLower())
                    {
                        case "text":
                            whatsAppMessage.ClientIds = phone;
                            whatsAppMessage.Message = template.txtComment;
                            break;
                        case "button":
                            if (!string.IsNullOrEmpty(template.txtButtons))
                            {
                                ButtonData objButtonData = new ButtonData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttons = new List<ButtonRows>();
                                foreach (var btn in template.txtButtons.Split(',').ToList())
                                {
                                    ButtonRows buttonRows = new ButtonRows();
                                    buttonRows.text = btn;
                                    objButtonData.buttons.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = phone;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment;

                                whatsAppMessage.ButtonList = objButtonData;
                            }
                            break;
                        case "url":
                            if (!string.IsNullOrEmpty(template.txtUrls))
                            {
                                UrlData objButtonData = new UrlData();
                                objButtonData.title = template.txtTitle;
                                objButtonData.footer = template.txtFooter;
                                objButtonData.buttonsUrl = new List<ButtonUrlRows>();
                                foreach (var btn in template.txtUrls.Split(',').ToList())
                                {
                                    ButtonUrlRows buttonRows = new ButtonUrlRows();
                                    if (btn.Split('|').Length > 1)
                                    {
                                        buttonRows.text = btn.Split('|')[0];
                                        buttonRows.url = btn.Split('|')[1];
                                    }
                                    else
                                        buttonRows.text = btn.Split('|')[0];
                                    objButtonData.buttonsUrl.Add(buttonRows);
                                }
                                whatsAppMessage.ClientIds = phone;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment;

                                whatsAppMessage.UrlList = objButtonData;
                            }
                            break;
                        case "list":
                            if (!string.IsNullOrEmpty(template.txtListSections))
                            {
                                List<ListSection> sections = new List<ListSection>();
                                int loopCount = 0;

                                //Format: Section1|Section2 - Section1Rows|Section2Rows: T1|D1||T2|D2||T3|D3 ||| T1|D1||T2|D2||T3|D3
                                foreach (var section in template.txtListSections.Split('|').ToList())
                                {
                                    var objSection = new ListSection();
                                    objSection.Title = section.Trim();

                                    if (!string.IsNullOrEmpty(template.txtListSectionRows))
                                    {
                                        var splitRows = template.txtListSectionRows.Split(new string[] { "|||" }, StringSplitOptions.None);
                                        if (splitRows != null && splitRows.Count() > 0 && splitRows[loopCount] != null)
                                        {
                                            var splitSectionRows = splitRows[loopCount].Split(new string[] { "||" }, StringSplitOptions.None);
                                            List<ListRow> rows = new List<ListRow>();
                                            foreach (var srrow in splitSectionRows)
                                            {
                                                if (srrow != null)
                                                {
                                                    ListRow objRow = new ListRow();
                                                    var splitTitleDesc = srrow.Trim().Split('|');
                                                    objRow.Title = splitTitleDesc[0].Trim();
                                                    objRow.Description = splitTitleDesc[1].Trim();
                                                    rows.Add(objRow);
                                                }
                                            }
                                            objSection.Rows = rows;
                                        }
                                    }
                                    sections.Add(objSection);
                                    loopCount++;
                                }
                                whatsAppMessage.ClientIds = phone;
                                whatsAppMessage.Title = ReplaceFooterHeaderMsg(template.txtTitle);
                                whatsAppMessage.Footer = ReplaceFooterHeaderMsg(template.txtFooter);
                                whatsAppMessage.Message = template.txtComment;

                                whatsAppMessage.SendList = new ListData()
                                {
                                    ButtonText = template.txtListButton,
                                    Description = template.txtComment,
                                    Sections = sections
                                };
                            }
                            break;
                    }
                }
            }
            return whatsAppMessage;
        }
    }

    public class LogHelper
    {
        public static void LogMessage(int id, string log, string uniqueId = "")
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.tblExternalLog.Add(new tblExternalLog()
                {
                    dteCreated = DateTime.Now,
                    intIdPerson = id,
                    txtSource = "Facil API",
                    txtLog = log,
                    txtTransactionId = uniqueId
                });
                ctx.SaveChanges();
            }
        }

        public static void LogMessageOld(int id, string log)
        {
            using (var ctx = new FoneClubeContext())
            {
                ctx.tblLog.Add(new tblLog()
                {
                    dteTimeStamp = DateTime.Now,
                    txtAction = log,
                    intIdTipo = 1
                });
                ctx.SaveChanges();
            }
        }
    }

    public class EncryptionHelper
    {
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("b14ca5898a4e4153bbce2ea2315a1916");
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("b14ca5898a4e4153bbce2ea2315a1916");
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
