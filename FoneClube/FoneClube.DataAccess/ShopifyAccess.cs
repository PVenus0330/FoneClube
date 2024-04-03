using System;
using FoneClube.DataAccess.security;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using FoneClube.Business.Commons.Entities;
using FoneClube.DataAccess.Utilities;
using FoneClube.Business.Commons.Entities.FoneClube;
using Shopify = FoneClube.Business.Commons.Entities.FoneClube.Shopify;

namespace FoneClube.DataAccess
{
    public class ShopifyAccess
    {
        public Shopify.ShopifyResponse ProcessPaymentWebhook()
        {
            string json = string.Empty;
            FacilAccess facilAccess = new FacilAccess();
            Shopify.ShopifyResponse response = new Shopify.ShopifyResponse();
            try
            {
                string status = string.Empty;
                System.IO.Stream req = HttpContext.Current.Request.InputStream;
                json = new System.IO.StreamReader(req).ReadToEnd();
                LogHelper.LogMessageOld(1, "ProcessPaymentWebhook response:" + json);

                Shopify.ShopifyData data = JsonConvert.DeserializeObject<Shopify.ShopifyData>(json);
                if (data != null && data.customer != null)
                {
                    var personId = RegisterUser(data.customer);
                    if (personId != 0 && data.line_items != null && data.line_items.Count > 0)
                    {
                        response.Activations = new List<Shopify.ShopifyActivationResponse>();
                        foreach (var lineItem in data.line_items)
                        {
                            int planId = GetPlanIdBySku(lineItem.sku);
                            var atRes = ActivateMVNOPlan(personId, planId);
                            if (atRes != null)
                            {
                                SaveShopifyOrderInfo(data, atRes, json, lineItem, planId, personId);
                            }
                            response.Activations.Add(atRes);
                        }

                        response.Status = response.Activations.Count(x => x != null) == response.Activations.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "ProcessPaymentWebhook error:" + ex.ToString());
            }
            return response;
        }

        private static int GetPlanIdBySku(string sku)
        {

            int id = 0;
            switch (sku)
            {
                case "BR_4GB": id = 315; break;
                case "BR_7GB": id = 316; break;
                case "BR_12GB": id = 317; break;
                case "BR_20GB": id = 318; break;
                case "BR_42GB": id = 319; break;
            }
            return id;
        }

        private int RegisterUser(Shopify.Customer cust)
        {
            int personId = 0;
            using (var ctx = new FoneClubeContext())
            {
                string customerId = Convert.ToString(cust.id);
                var ctxPerson = ctx.tblPersons.FirstOrDefault(x => x.txtDocumentNumber == customerId);
                if (ctxPerson is null)
                {
                    var hashedPassword = new Security().EncryptPassword(customerId);
                    var newPerson = new tblPersons
                    {
                        txtName = cust.first_name + " " + cust.last_name,
                        txtDocumentNumber = customerId,
                        dteRegister = DateTime.Now,
                        txtEmail = cust.email,
                        intIdRole = 0,
                        txtPassword = hashedPassword.Password,
                        bitSenhaCadastrada = true,
                        bitDadosPessoaisCadastrados = true,
                        txtDefaultWAPhones = Convert.ToString(cust.phone),
                        bitShopifyUser = true
                    };

                    ctx.tblPersons.Add(newPerson);
                    ctx.SaveChanges();

                    personId = newPerson.intIdPerson;
                }
                else
                {
                    personId = ctxPerson.intIdPerson;
                }
            }

            return personId;
        }

        private Shopify.ShopifyActivationResponse ActivateMVNOPlan(int idPerson, int planId)
        {
            Shopify.ShopifyActivationResponse activationResponse = new Shopify.ShopifyActivationResponse();
            string activationcode = string.Empty;
            try
            {
                if (planId != 0)
                {
                    using (var ctx = new FoneClubeContext())
                    {
                        var tblPerson = ctx.tblPersons.FirstOrDefault(x => x.intIdPerson == idPerson);
                        if (tblPerson != null)
                        {
                            bool fakeActivationenabled = false;
                            try
                            {
                                fakeActivationenabled = Convert.ToBoolean(ctx.tblConfigSettings.Where(x => x.txtConfigName == "UseShopifyFakeActivation").FirstOrDefault().txtConfigValue);
                            }
                            catch (Exception ex)
                            {
                            }

                            if (!fakeActivationenabled)
                            {
                                MVNOAccess mVNOAccess = new MVNOAccess();
                                FacilAccess facilAccess = new FacilAccess();

                                var iccidPool = facilAccess.ValidateNumberByICCIDInLoop(idPerson, "");
                                if (iccidPool != null)
                                {
                                    string newICCID = iccidPool.txtICCID;
                                    activationcode = iccidPool.txtActivationCodeLPA;

                                    ActivatePlanRequest activatePlanRequest = new ActivatePlanRequest();
                                    activatePlanRequest.metodo_pagamento = "SALDO";
                                    activatePlanRequest.nome = tblPerson.txtName;
                                    activatePlanRequest.cnpj = "08453543000176";
                                    activatePlanRequest.email = tblPerson.txtEmail;
                                    activatePlanRequest.telefone = "21981908190";
                                    activatePlanRequest.data_nascimento = "1900-01-01";
                                    activatePlanRequest.endereco = new Business.Commons.Entities.FoneClube.Endereco();
                                    activatePlanRequest.endereco.rua = "Avenida das americas";
                                    activatePlanRequest.endereco.numero = "3434";
                                    activatePlanRequest.endereco.complemento = "305 bloco 2";
                                    activatePlanRequest.endereco.bairro = "Barra da Tijuca";
                                    activatePlanRequest.endereco.cep = "22640102";
                                    activatePlanRequest.endereco.municipio = "Rio de Janeiro";
                                    activatePlanRequest.endereco.uf = "RJ";

                                    activatePlanRequest.chips = new List<Chip>();
                                    var chip = new Chip();
                                    chip.iccid = newICCID;
                                    chip.id_plano = planId;
                                    chip.ddd = 21;
                                    activatePlanRequest.chips.Add(chip);

                                    if (activatePlanRequest != null && activatePlanRequest.chips != null && activatePlanRequest.chips.Count > 0)
                                    {
                                        var response = mVNOAccess.ActivatePlan(activatePlanRequest);

                                        if (response != null && response.retorno && response.info != null && response.info.chips != null && response.info.chips.Count() > 0)
                                        {
                                            foreach (var pho in response.info.chips)
                                            {
                                                System.Threading.Thread.Sleep(10000);

                                                var iccidPhoneData = mVNOAccess.ValidateICCID(pho.iccid);

                                                if (iccidPhoneData != null && iccidPhoneData.retorno && iccidPhoneData.info != null && !string.IsNullOrEmpty(iccidPhoneData.info.numero_ativado))
                                                {
                                                    var activatedPhone = iccidPhoneData.info.numero_ativado;

                                                    string oldFileName = string.Format(@"C:\Temp\Contel\ActivationFiles\OrignalV2\{0}.pdf", pho.iccid);
                                                    string newFileName = string.Format(@"C:\inetroot\FacilActivationPdfs\{0}_{1}.pdf", activatedPhone, pho.iccid);

                                                    string iccid = pho.iccid;
                                                    string qrcode = string.Empty;

                                                    if (System.IO.File.Exists(oldFileName))
                                                    {
                                                        var tbliccid = ctx.tblESimICCIDPool.FirstOrDefault(x => x.txtICCID == pho.iccid);
                                                        if (tbliccid != null)
                                                        {
                                                            activationcode = tbliccid.txtActivationCodeLPA;
                                                            qrcode = QRCodeHelper.GenerateQRCode(activationcode);
                                                        }

                                                        LogHelper.LogMessage(idPerson, string.Format("FacilAccess:ActivateESimNew: Skipping download as file is already present for iccid: {0}", pho.iccid), "");
                                                        try
                                                        {
                                                            System.IO.File.Copy(oldFileName, newFileName);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LogHelper.LogMessage(idPerson, string.Format("FacilAccess:ActivateESimNew: Error occured while copying file for iccid : {0}", ex.ToString()), "");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LogHelper.LogMessage(idPerson, string.Format("FacilAccess:ActivateESimNew: Downloading file again as it not exists: {0}", pho.iccid), "");

                                                        qrcode = facilAccess.DownloadActivationFileContelInloop(idPerson, "", iccidPhoneData.info.esim, activatedPhone, ref iccid, ref activationcode);
                                                    }

                                                    if (!string.IsNullOrEmpty(qrcode) && !string.IsNullOrEmpty(activationcode))
                                                    {
                                                        var tblphones = ctx.tblPersonsPhones.Where(x => x.txtICCID == iccidPhoneData.info.iccid);

                                                        if (tblphones != null && tblphones.Count() > 0)
                                                        {
                                                            foreach (var ph in tblphones)
                                                            {
                                                                ph.intIdPlan = planId;
                                                                ph.bitAtivo = true;
                                                                ph.bitPhoneClube = true;
                                                                ph.intDDD = Convert.ToInt32(activatedPhone.Substring(0, 2));
                                                                ph.intPhone = Convert.ToInt32(activatedPhone.Substring(2));
                                                                ph.bitEsim = true;
                                                                ph.intIdOperator = 4;
                                                                ph.intIdPerson = tblPerson.intIdPerson;
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
                                                                intIdPlan = planId,
                                                                bitAtivo = true,
                                                                bitEsim = true,
                                                                bitPhoneClube = true,
                                                                intIdOperator = 4,
                                                                intIdPerson = tblPerson.intIdPerson,
                                                                txtICCID = iccid,
                                                            });
                                                            ctx.SaveChanges();
                                                        }
                                                    }

                                                    activationResponse.ActivatedNumber = activatedPhone;
                                                    activationResponse.ActivatedPlan = iccidPhoneData.info.plano_nome;
                                                    activationResponse.ActivationDate = iccidPhoneData.info.data_ativacao;
                                                    activationResponse.ICCID = iccidPhoneData.info.iccid;
                                                    activationResponse.ActivationPDFLink = iccidPhoneData.info.esim;
                                                    activationResponse.ActivationCode = activationcode;
                                                    activationResponse.ActivationQrCode = qrcode;

                                                    LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan: Activated successfully");

                                                    try
                                                    {
                                                        string msgAdm = string.Format("*Activation: Shopify User: {0}*\n\n" +
                                                                   "Line: *{1}* \n" +
                                                                   "Plan: {2} \n"
                                                                   , tblPerson.txtName, activatedPhone, iccidPhoneData.info.plano_nome);

                                                        new WhatsAppAccess().SendMessageInfoToAdmin(msgAdm);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }
                                                }
                                                else
                                                {
                                                    LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan:ValidateICCID failed");
                                                    return null;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan:Activation failed");
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan:No chips to activate");
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                activationResponse.ActivatedNumber = "99999999999";
                                activationResponse.ActivatedPlan = "4GB";
                                activationResponse.ActivationDate = "2024-01-08 14:58:22.000";
                                activationResponse.ICCID = "875522222255522256665";
                                activationResponse.ActivationPDFLink = "https://tecnologia.conteltelecom.com.br/Anexos/esim/esim/CONTEL-8955170220117045083.pdf";
                                activationResponse.ActivationCode = "Test LPA:1$sm-v4-064-a-gtm.pr.go-esim.com$4A83417AA38D12BAEAA74ADE8DCE3747";
                                activationResponse.ActivationQrCode = "data:image/png;base64,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6TZ68yTpm+ibTGqjjcu10UaSMt5giD7KpDbauFwbbSStozNfro027E66jd48Sfom+iaT2mjjcm20kaSMNxiijzKpjTYu10YbSevozJdrow27k26jN0+Svom+yaQ22rhcG20kKeMNhuijTGqjjcu10UbSOjrz5dpow+6k2+jNk6Rvom8yqY02LtdGG0nKeIMh+iiT2mjjcm20kbSOzny5NtqwO+k2evMk6Zvom0xqo43LtdFGkjLeYIg+yqQ22rhcG20kraMzX66NNuxOuo3ePEn6Jvomk9po43JttJGkjDcYoo8yqY02LtdGG0nr6MyXa6MNu5NuozdPkr6JvsmkNtq4XBttJCnjDYboo0xqo43LtdFG0jo68+XaaMPupNvozZOkb6JvMqmNNi7XRhtJyniDIfook9po43JttJG0js58uTbasDvpNnrzJOmb6JtMaqONy7XRRpIy3mCIPsqkNtq4XBttJK2jM1+ujTbsTrqN3jxJ+ib6JpPaaONybbSRpIw3GKKPMqmNNi7XRhtJ6+jMl2ujDbuTbqM3T5K+ib7JpDbauFwbbSQp4w2G6KNMaqONy7XRRtI6OvPl2mjD7qTb6M2TpG+ibzKpjTYu10YbScp4gyH6KJPaaONybbSRtI7OfLk22rA76TZ68yTpm+ibTGqjjcu10UaSMt5giD7KpDbauFwbbSStozNfro027E66jd48Sfom+iaT2mjjcm20kaSMNxiijzKpjTYu10YbSevozJdrow27k26jN0+Svom+yaQ22rhcG20kKeMNhuijTGqjjcu10UbSOjrz5dpow+6k2+jNk6Rvom8yqY02LtdGG0nKeIMh+iiT2mjjcm20kbSOzny5NtqwO+k2evMk6Zvom0xqo43LtdFGkjLeYIg+yqQ22rhcG20kraMzX66NNuxOuo3ePEn6Jvomk9po43JttJGkjDcYoo8yqY02LtdGG0mvoTtYqo02llpHZ05qo42k19AdJK2jMy/VRhtLvYbuIKmNNi7XRhtJyniDIfook9po43JttJH0GrqDpdpoY6l1dOakNtpIeg3dQdI6OvNSbbSx1GvoDpLaaONybbSRpIw3GKKPMqmNNi7XRhtJr6E7WKqNNpZaR2dOaqONpNfQHSStozMv1UYbS72G7iCpjTYu10YbScp4gyH6KJPaaONybbSR9Bq6g6XaaGOpdXTmpDbaSHoN3UHSOjrzUm20sdRr6A6S2mjjcm20kaSMNxiijzKpjTYu10YbSa+hO1iqjTaWWkdnTmqjjaTX0B0kraMzL9VGG0u9hu4gqY02LtdGG0nKeIMh+iiT2mjjcm20kfQauoOl2mhjqXV05qQ22kh6Dd1B0jo681JttLHUa+gOktpo43JttJGkjDcYoo8yqY02LtdGG0mvoTtYqo02llpHZ05qo42k19AdJK2jMy/VRhtLvYbuIKmNNi7XRhtJyniDIfook9po43JttJH0GrqDpdpoY6l1dOakNtpIeg3dQdI6OvNSbbSx1GvoDpLaaONybbSRpIw3GKKPMqmNNi7XRhtJr6E7WKqNNpZaR2dOaqONpNfQHSStozMv1UYbS72G7iCpjTYu10YbScp4gyH6KJPaaONybbSR9Bq6g6XaaGOpdXTmpDbaSHoN3UHSOjrzUm20sdRr6A6S2mjjcm20kaSMNxiijzKpjTYu10YbSa+hO1iqjTaWWkdnTmqjjaTX0B0kraMzL9VGG0u9hu4gqY02LtdGG0nKeIMh+iiT2mjjcm20kfQauoOl2mhjqXV05qQ22kh6Dd1B0jo681JttLHUa+gOktpo43JttJGkjDcYoo8yqY02LtdGG0mvoTtYqo02llpHZ05qo42k19AdJK2jMy/VRhtLvYbuIKmNNi7XRhtJyniDIfook9po43JttJH0GrqDpdpoY6l1dOakNtpIeg3dQdI6OvNSbbSx1GvoDpLaaONybbSRpIw3GKKPMqmNNi7XRhtJr6E7WKqNNpZaR2dOaqONpNfQHSStozMv1UYbS72G7iCpjTYu10YbScp4gyH6KJPaaONybbSR9Bq6g6XaaGOpdXTmpDbaSHoN3UHSOjrzUm20sdRr6A6S2mjjcm20kaSMNxiijzKpjTYu10YbSa+hO1iqjTaWWkdnTmqjjaTX0B0kraMzL9VGG0u9hu4gqY02LtdGG0nKeIMh+iiT2mjjcm20kfQauoOl2mhjqXV05qQ22kh6Dd1B0jo681JttLHUa+gOktpo43JttJGkjDcYoo8yqY02LtdGG0mvoTtYqo02llpHZ05qo42k19AdJK2jMy/VRhtLvYbuIKmNNi7XRhtJyniDIfook9po43JttJH0GrqDpdpoY6l1dOakNtpIeg3dQdI6OvNSbbSx1GvoDpLaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8yqY02LtdGG0lttHE5baE3WkoZutOltIXeKKmNNpLW0ZmT2mjjcm20kaSMNxiijzKpjTYu10YbSW20cTltoTdaShm606W0hd4oqY02ktbRmZPaaONybbSRpIw3GKKPMqmNNi7XRhtJbbRxOW2hN1pKGbrTpbSF3iipjTaS1tGZk9po43JttJGkjDcYoo8y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Rhv2eevozEutozMnKUN3mrSOzny5dXTmy0m/4Rczhn7USW20cbl1dOakNtpIaqMN+7x1dOal1tGZk5ShO01aR2e+3Do68+Wk3/CLGUM/6qQ22rjcOjpzUhttJLXRhn3eOjrzUuvozEnK0J0mraMzX24dnfly0m/4xYyhH3VSG21cbh2dOamNNpLaaMM+bx2deal1dOYkZehOk9bRmS+3js58Oek3/GJC9CNMaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaWaqONy62jMy/VRhtJbbRxuXV05sutozMntdFGknSJX3SI/pFIaqONpdbRmZPaaCOpjTaW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";
                            }
                        }

                        else
                        {
                            LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan:Person not found");
                            return null;
                        }
                    }
                }
                else
                {
                    LogHelper.LogMessageOld(1, "ShopifyAccess::ActivateMVNOPlan:PlanId is invalid");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "ActivateMVNOPlan error: " + ex.ToString());
            }
            return activationResponse;
        }

        private void SaveShopifyOrderInfo(Shopify.ShopifyData data, Shopify.ShopifyActivationResponse responsedata, string json, Shopify.LineItem currentItem, int planId, int userId)
        {
            try
            {
                using (var ctx = new FoneClubeContext())
                {
                    ctx.tblShopifySalesInfo.Add(new tblShopifySalesInfo()
                    {
                        intIdPerson = userId,
                        txtShopifyOrderId = Convert.ToString(data.id),
                        txtShopifyOrderLineId = Convert.ToString(currentItem.id),
                        intShopifyCustId = data.customer.id,
                        txtShopifyEmail = data.customer.email,
                        txtComment = json,
                        txtSku = currentItem.sku,
                        intIdPlan = planId,
                        txtPrice = currentItem.price,
                        txtActivationCode = responsedata.ActivationCode,
                        txtActivationQrCode = responsedata.ActivationQrCode,
                        txtActivationPDFLink = responsedata.ActivationPDFLink,
                        txtPhone = responsedata.ActivatedNumber,
                        txtICCID = responsedata.ICCID,
                        txtDateActivation = responsedata.ActivationDate,
                        txtOrderUIPath = "https://webfc.foneclube.com.br/#/tab/shopifyorder/" + Convert.ToString(currentItem.id),
                        dteInsert = DateTime.Now
                    });
                    ctx.SaveChanges();

                    EmailAccess emailAccess = new EmailAccess();
                    Email email = new Email();
                    email.To = data.email;
                    email.Subject = "eSIM Activation and configuration - OrderId:" + Convert.ToString(currentItem.id);
                    email.Body = @"<html><head></head><body><h2 class='bot' style='text-align: center;'>MyEsim.Pro</h2>" +
    "<p class='bold ng-binding'>&nbsp;</p>" +
    "<p class=''><img style = 'display: block; margin-left: auto; margin-right: auto;' src='" + responsedata.ActivationQrCode + "' alt='qrcode' width='250' height='250'></p>" +
    "<p class='m-t-10' style='text-align: center;'><strong>Order#:</strong>" + Convert.ToString(currentItem.id) + "<strong> Date:</strong></p>" +
    "<p class='m-t-10' style='text-align: left;'><strong>Phone #</strong> " + responsedata.ActivatedNumber + "<strong>APN</strong>: surf.br</p>" +
    "<p class='m-t-10'>ICCID: " + responsedata.ICCID + "</p>" +
    "<p class='m-t-10'>LPA: " + responsedata.ActivationCode + "</p>" +
    "<p class='m-t-10'>&nbsp;</p>" +
    "<div class='col-md-6'><button class='btn btn-info' data-clipboard-text='00020101021226820014br.gov.bcb.pix2560pix.stone.com.br/pix/v2/5edfba6f-a326-4bda-891a-e5a367329366520400005303986540540.005802BR5925Chopp Control Auditoria d6014RIO DE JANEIRO62290525paclr33zqkgnxhg1fjzbyuodu6304E4A0'>Codigo PIX</button></div>" +
    "<div class='col-md-6'><a class='btn btn-info' href='https://api.foneclube.com.br/api/pagarme/pix/qrcode/14088' target='_blank' rel='noopener'>QrCode</a></div>" +
    "<p>&nbsp;</p>" +
    "<p class='m-t-10'><span class='bold'>If you have questions please contact WhatsApp&nbsp; +<a href = 'https://wa.me/5521981908190' > 5521 - 98190 - 8190 </ a ></ span ></ p >" +
    "< p class='m-t-10'>or email a shop@myesim.pro</p>" +
    "<p class='m-t-10'>&nbsp;</p></body>";

                    emailAccess.SendEmailShopify(email);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageOld(1, "SaveShopifyOrderInfo error:" + ex.ToString());
            }

        }
    }
}
