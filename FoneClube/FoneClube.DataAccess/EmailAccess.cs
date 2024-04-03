using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities.FoneClube;
using FoneClube.Business.Commons.Entities.Generic;
using FoneClube.Business.Commons.Entities.ViewModel.Plano;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess
{
    public class EmailAccess
    {
        public bool SendEmail(Email email)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (email.TemplateType == Convert.ToInt32(Email.TemplateTypes.BoletoCharged)
                        || email.TemplateType == Convert.ToInt32(Email.TemplateTypes.CardCharged)
                        || email.TemplateType == Convert.ToInt32(Email.TemplateTypes.Pix))
                    {
                        try
                        {
                            List<PhonePlanViewModel> planos;
                            if (!String.IsNullOrEmpty(email.Id))
                            {
                                var matricula = Convert.ToInt32(email.Id);
                                planos = new PhoneAccess().GetPlanosCliente(matricula);
                            }
                            else
                            {
                                planos = new PhoneAccess().GetPlanosCliente(email.To);
                            }
                            var complemento = new PhoneAccess().GetCorpoPlanos(planos);
                            email.TargetSecondaryText += complemento;
                        }
                        catch (Exception) { }
                    }

                    if (ctx.Database.Connection.Database == "foneclube-homol")
                        email.To = ConfigurationManager.AppSettings["EmailTestes"];

                    var template = ctx.tblEmailTemplates.FirstOrDefault(e => e.intId == email.TemplateType);

                    var trechoAdicionalDesconto = string.Empty;
                    //var trechoBonus = string.Format(ctx.tblEmailTemplates.FirstOrDefault(e => e.txtTipoTemplate == "adicional-bonus").txtDescription, email.TargetTextBlue);
                    var trechoBeneficio = string.Format(ctx.tblEmailTemplates.FirstOrDefault(e => e.txtTipoTemplate == "adicional-beneficio").txtDescription, email.DiscountPrice);

                    if (!string.IsNullOrEmpty(email.DiscountPrice))
                        trechoAdicionalDesconto = trechoBeneficio;

                    string formatado = string.Empty;
                    if (email.TemplateType == Convert.ToInt32(Email.TemplateTypes.Pix))
                    {
                        email.TargetTextComment = Uri.UnescapeDataString(email.TargetTextComment);
                        email.TargetTextComment = email.TargetTextComment.Replace("\n", "<br/>");
                        email.TargetTextComment = email.TargetTextComment.Replace("*", "").Replace("|", "");
                        formatado = string.Format(template.txtDescription, email.TargetName, email.TargetTextBlue, email.TargetSecondaryText, trechoAdicionalDesconto, email.TargetTextComment);
                    }
                    else
                    {
                        email.TargetSecondaryText = email.TargetSecondaryText.Replace("*", "").Replace("|", "");
                        formatado = string.Format(template.txtDescription, email.TargetName, email.TargetTextBlue, email.TargetSecondaryText, trechoAdicionalDesconto);
                    }

                    var enviaEmail = new Utils().SendEmail(email.To, template.txtSubject, formatado, true);

                    try
                    {
                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = string.Format("Email {0} - {1}", email.To, enviaEmail)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception e) { }




                    return enviaEmail;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public bool SendEmailDinamic(Email email)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    if (ctx.Database.Connection.Database == "foneclube-homol")
                        email.To = ConfigurationManager.AppSettings["EmailTestes"];

                    var template = ctx.tblEmailTemplates.FirstOrDefault(e => e.intId == email.TemplateType);


                    var formatado = string.Format(template.txtDescription, email.TargetName, email.TargetTextBlue, email.TargetSecondaryText, email.DiscountPrice);
                    var enviaEmail = new Utils().SendEmailFoneclube(email.To, template.txtSubject, formatado, true);

                    try
                    {
                        ctx.tblLog.Add(new tblLog
                        {
                            dteTimeStamp = DateTime.Now,
                            intIdTipo = 1,
                            txtAction = string.Format("Email {0} - {1}", email.To, enviaEmail)
                        });

                        ctx.SaveChanges();
                    }
                    catch (Exception e) { }




                    return enviaEmail;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public bool SendEmailShopify(Email email)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var formatado = email.Body;
                    var enviaEmail = new Utils().SendEmailShopify(email.To, email.Subject, formatado, true);

                    return enviaEmail;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public tblEmailTemplates GetEmailDetails(int templateId)
        {
            string templateName = string.Empty;
            switch (templateId)
            {
                case 3:
                    templateName = "Bloqueio por perda_roubo";
                    break;
                case 4:
                    templateName = "Bloqueio por perda_roubo";
                    break;
                case 5:
                    templateName = "Bloqueio por perda_roubo";
                    break;
                case 6:
                    templateName = "Suspensão voluntária 4 meses";
                    break;
                case 7:
                    templateName = "Suspensão voluntária 4 meses";
                    break;
                case 8:
                    templateName = "Upgrade/downgrade de plano";
                    break;
            }
            using (var ctx = new FoneClubeContext())
            {
                return ctx.tblEmailTemplates.Where(x => x.txtTipoTemplate == templateName).FirstOrDefault();
            }
        }

        public bool SendEmailStatus(EmailStatus em, List<Attachments> attachments)
        {
            try
            {
                var enviaEmail = new Utils().SendEmailWithAttachmentStatus(em, attachments);
                return true;
            }
            catch (Exception e)
            {
                return true;
            }

        }

        public List<EmailTemplate> GetTemplates()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    return ctx.tblEmailTemplates.Where(e => e.bitAtivo == true).Select(t => new EmailTemplate
                    {
                        Id = t.intId,
                        Tipo = t.txtTipoTemplate,
                        Subject = t.txtSubject,
                        Description = t.txtDescription,
                        To = string.IsNullOrEmpty(t.toEmail) ? "" : t.toEmail,
                        From = string.IsNullOrEmpty(t.fromEmail) ? "" : t.fromEmail,
                        Cc = string.IsNullOrEmpty(t.cc) ? "" : t.cc,
                        Bcc = string.IsNullOrEmpty(t.bcc) ? "" : t.bcc,
                        ShowInAction = t.bitShowAction.HasValue ? t.bitShowAction.Value : false
                    }).ToList();

                }
            }
            catch (Exception e)
            {
                return new List<EmailTemplate>();
            }
        }

        public bool SaveTemplates(EmailTemplate email)
        {
            try
            {
                if (email != null)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var template = ctx.tblEmailTemplates.FirstOrDefault(e => e.intId == email.Id);
                        if (template != null)
                        {
                            template.txtSubject = email.Subject;
                            template.txtDescription = email.Description;
                            template.toEmail = email.To;
                            template.fromEmail = email.From;
                            template.cc = email.Cc;
                            template.bcc = email.Bcc;
                            template.txtTipoTemplate = email.Tipo;
                            template.bitShowAction = email.ShowInAction;
                            template.bitAtivo = true;
                            ctx.SaveChanges();
                        }
                        else
                        {
                            ctx.tblEmailTemplates.Add(new tblEmailTemplates()
                            {
                                txtSubject = email.Subject,
                                txtDescription = email.Description,
                                toEmail = email.To,
                                fromEmail = email.From,
                                cc = email.Cc,
                                bcc = email.Bcc,
                                txtTipoTemplate = email.Tipo,
                                bitShowAction = email.ShowInAction,
                                bitAtivo = true
                            });
                            ctx.SaveChanges();
                        }
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public bool SendTemplate(EmailTemplate email)
        {
            try
            {
                if (email != null)
                {
                    EmailStatus emailStatus = new EmailStatus();
                    emailStatus.from = email.From;
                    emailStatus.email = email.To;
                    emailStatus.cc = email.Cc;
                    emailStatus.bcc = email.Bcc;
                    emailStatus.subject = email.Subject;
                    emailStatus.body = email.Description;
                    return new Utils().SendEmailWithAttachmentStatus(emailStatus, null);
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteTemplate(int templateId)
        {
            if (templateId > 0)
            {
                using (var ctx = new FoneClubeContext())
                {
                    var deleted = ctx.tblEmailTemplates.FirstOrDefault(x => x.intId == templateId);
                    ctx.tblEmailTemplates.Remove(deleted);
                    ctx.SaveChanges();

                    return true;
                }
            }
            return false;
        }

        public bool Send24HrsLateClientInfo()
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    var pendingClients = ctx.getList24hsLate(null).ToList();
                    if (pendingClients != null && pendingClients.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.AddRange(new DataColumn[6] {
                            new DataColumn("Client Name", typeof(string)),
                            new DataColumn("CPF", typeof(string)),
                            new DataColumn("Phone Number", typeof(string)),
                            new DataColumn("Amount Last Vencimento", typeof(string)),
                            new DataColumn("Date Of Last Vencimento",typeof(string)),
                            new DataColumn("Pending Since Days",typeof(int))});

                        foreach (var pending in pendingClients)
                        {
                            dt.Rows.Add(pending.ClientName, pending.CPF, pending.PhoneNumber, pending.AmountLastVencimento.HasValue ? String.Format("R${0:0.00}", (pending.AmountLastVencimento.Value / 100)) : "0.00", pending.DateOfLastVencimento.HasValue ? pending.DateOfLastVencimento.Value.ToString("dd/MMM/yy hh:mm") : "", pending.PendingDays.Value);
                        }

                        StringBuilder sb = new StringBuilder();
                        //Table start.
                        sb.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");

                        //Adding HeaderRow.
                        sb.Append("<tr>");
                        foreach (DataColumn column in dt.Columns)
                        {
                            sb.Append("<th style='background-color: #B8DBFD;border: 1px solid #ccc'>" + column.ColumnName + "</th>");
                        }
                        sb.Append("</tr>");


                        //Adding DataRow.
                        foreach (DataRow row in dt.Rows)
                        {
                            sb.Append("<tr>");
                            foreach (DataColumn column in dt.Columns)
                            {
                                sb.Append("<td style='width:100px;border: 1px solid #ccc'>" + row[column.ColumnName].ToString() + "</td>");
                            }
                            sb.Append("</tr>");
                        }

                        //Table end.
                        sb.Append("</table>");

                        var template = ctx.tblEmailTemplates.FirstOrDefault(e => e.intId == 31);
                        if (template != null)
                        {
                            string formatado = string.Format(template.txtDescription, sb.ToString());
                            var enviaEmail = new Utils().SendEmail(template.toEmail, template.txtSubject, formatado, true);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblLog.Add(new tblLog
                    {
                        dteTimeStamp = DateTime.Now,
                        intIdTipo = 1,
                        txtAction = "Recreate pix expired charge" + ex.ToString()
                    });
                }
                var exMessage = Utils.ProcessException(ex);
                return false;
            }
            return true;
        }

    }
}
