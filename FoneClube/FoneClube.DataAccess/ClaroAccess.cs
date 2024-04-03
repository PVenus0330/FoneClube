using FoneClube.Business.Commons.Entities.Claro;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FoneClube.DataAccess
{
    public class ClaroAccess
    {
        //test
        //private const string Company = "939144068";
        //private const string Username = "crawl";
        //private const string Password = "crawl01";

        //live
        private const string Company = "939144068";
        private const string Username = "9391440";
        private const string Password = "@fone";

        public ClaroAccess()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertificates);
        }

        private string GetPage(string url, CookieContainer c)
        {
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = c;

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0";
            req.Host = "claro-gestoronline.claro.com.br";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            req.Method = "GET";
            req.Referer = "https://claro-gestoronline.claro.com.br/evpn/start.do";

            var resp = req.GetResponse() as HttpWebResponse;

            var reader = new StreamReader(resp.GetResponseStream());
            var html = reader.ReadToEnd();

            return html;
        }

        private static bool AcceptAllCertificates(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private string PostPage(string url, string formParams, CookieContainer c)
        {
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = c;

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0";
            req.Host = "claro-gestoronline.claro.com.br";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Referer = "https://claro-gestoronline.claro.com.br/evpn/start.do";

            byte[] bytes = Encoding.ASCII.GetBytes(formParams);

            req.ContentLength = bytes.Length;

            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            var resp = req.GetResponse() as HttpWebResponse;

            var reader = new StreamReader(resp.GetResponseStream());
            var html = reader.ReadToEnd();

            return html;
        }

        public Dictionary<string, string> ListLines()
        {
            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            formParams = string.Format("costCenter=0&number=&view.admin.management.subscriber.consult.filter.button.label.x=27&view.admin.management.subscriber.consult.filter.button.label.y=8");

            html = PostPage("https://claro-gestoronline.claro.com.br/evpn/admin/subscriberListShow.do", formParams, c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var list = doc.GetElementbyId("tblLinhas").SelectSingleNode("script")
                        .InnerHtml.Split(new string[] { "criaItem" }, StringSplitOptions.RemoveEmptyEntries);

            var ccid = list[1].Split('\'')[3].Substring(2).Replace(")", "");

            var datalist = new Dictionary<string, string>();

            foreach (var item in list.Skip(3))
            {
                var res = item.Split('\'');

                datalist.Add(res[3].Split(' ')[0], ccid);
            }

            return datalist;
        }

        public ClaroLineInfo GetLineDetails(string line)
        {
            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            formParams = string.Format("costCenter=0&number={0}&view.admin.management.subscriber.consult.filter.button.label.x=27&view.admin.management.subscriber.consult.filter.button.label.y=8", line);

            html = PostPage("https://claro-gestoronline.claro.com.br/evpn/admin/subscriberListShow.do", formParams, c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var list = doc.GetElementbyId("tblLinhas").Descendants("td").ToList();
            var ativa = false;
            var bloqueada = false;

            try
            {
                ativa = list[2].InnerText.Trim().Contains("MIN") || list[2].InnerText.Trim().Contains("MAX");
                bloqueada = list[2].InnerText.Trim().Contains("BLOQUEADO");
            }
            catch(Exception){}

            return new ClaroLineInfo
            {
                Line = list[0].InnerText.Trim(),
                Subscriber = list[1].InnerText.Trim(),
                Profile = list[2].InnerText.Trim(),
                Voice = list[3].InnerText.Trim(),
                SMS = list[4].InnerText.Trim(),
                Data = list[5].InnerText.Trim(),
                Ativa = ativa,
                Bloqueada = bloqueada
            };
        }

        public bool IsLineBlocked(string line, string ccId)
        {
            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            var url = string.Format("https://claro-gestoronline.claro.com.br/evpn/admin/subscriberEdit.do?ccid={1}&id={0}", line, ccId);

            html = GetPage(url, c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var token = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["name"].Value == "org.apache.struts.taglib.html.TOKEN").Attributes["value"].Value;

            var temporaryProfile = doc.DocumentNode.Descendants("select").Single(p => p.Attributes["name"].Value == "temporaryProfile").Descendants("option").Single(p => p.Attributes.Contains("selected")).Attributes["value"].Value;

            return temporaryProfile == "1";
        }

        //if you do not want to change values for name and permanentProfile, then pass null as parameter value
        public string UpdateLine(string line, string ccId, string name, string permanentProfile, string temporaryProfile,
            bool? privateNumberNotice, bool? smsGprsBlockNotice, bool? receiveLimitBalanceSMS, string smsNumber, bool? sendLimitBalanceSMS)
        {
            string temporaryProfileBlockStart = "";
            string temporaryProfileBlockEnd = "";

            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            var url = string.Format("https://claro-gestoronline.claro.com.br/evpn/admin/subscriberEdit.do?ccid={1}&id={0}", line, ccId);

            html = GetPage(url, c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var token = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["name"].Value == "org.apache.struts.taglib.html.TOKEN").Attributes["value"].Value;

            if (string.IsNullOrEmpty(name))
            {
                name = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["name"].Value == "nameFmt").Attributes["value"].Value;
            }

            if (string.IsNullOrEmpty(permanentProfile))
            {
                permanentProfile = doc.DocumentNode.Descendants("select").Single(p => p.Attributes["name"].Value == "permanentProfile").Descendants("option").Single(p => p.Attributes.Contains("selected")).Attributes["value"].Value;
            }

            if (string.IsNullOrEmpty(temporaryProfile))
            {
                temporaryProfile = doc.DocumentNode.Descendants("select").Single(p => p.Attributes["name"].Value == "temporaryProfile").Descendants("option").Single(p => p.Attributes.Contains("selected")).Attributes["value"].Value;
            }
            else if (temporaryProfile == "1") //block
            {
                var dateStart = DateTime.Now.Date.AddDays(1);
                var dateEnd = DateTime.Now.Date.AddYears(1);

                temporaryProfileBlockStart = WebUtility.UrlEncode(string.Format("{0:dd'/'MM'/'yyyy}", dateStart));
                temporaryProfileBlockEnd = WebUtility.UrlEncode(string.Format("{0:dd'/'MM'/'yyyy}", dateEnd));
            }

            if (!privateNumberNotice.HasValue)
            {
                privateNumberNotice = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["type"].Value == "checkbox" && p.Attributes["name"].Value == "announcement").Attributes.Contains("checked");
            }

            if (!smsGprsBlockNotice.HasValue)
            {
                smsGprsBlockNotice = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["type"].Value == "checkbox" && p.Attributes["name"].Value == "blockGPRSSMS").Attributes.Contains("checked");
            }

            if (!receiveLimitBalanceSMS.HasValue)
            {
                receiveLimitBalanceSMS = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["type"].Value == "checkbox" && p.Attributes["name"].Value == "receiveSMS").Attributes.Contains("checked");
            }

            if (!sendLimitBalanceSMS.HasValue)
            {
                sendLimitBalanceSMS = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["type"].Value == "checkbox" && p.Attributes["name"].Value == "autoWarningSMS").Attributes.Contains("checked");
            }

            formParams = string.Format(@"org.apache.struts.taglib.html.TOKEN={0}&nameFmt={1}&permanentProfile={2}&temporaryProfile={3}&temporaryProfileStart={9}&temporaryProfileEnd={10}&announcement={4}&announcement=false&blockGPRSSMS={5}&receiveSMS={6}&smsNumber={7}&autoWarningSMS={8}&backButton=&view.admin.management.subscriber.ok.button.label.x=33&view.admin.management.subscriber.ok.button.label.y=8",
                        token, name, permanentProfile, temporaryProfile, privateNumberNotice.ToString().ToLower(), smsGprsBlockNotice.ToString().ToLower(), receiveLimitBalanceSMS.ToString().ToLower(),
                        smsNumber, sendLimitBalanceSMS.ToString().ToLower(), temporaryProfileBlockStart, temporaryProfileBlockEnd.ToString().ToLower());

            html = PostPage("https://claro-gestoronline.claro.com.br/evpn/admin/subscriberEdit.do", formParams, c);

            doc.LoadHtml(html);

            var msg = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "msgErro")
                .SelectSingleNode("label").InnerText.Trim();

            return msg;
        }

        public string BlockLine(string line, string ccId)
        {
            return UpdateLine(line, ccId, null, null, "1", null, null, null, "", null);
        }

        public string UnblockLine(string line, string ccId)
        {
            return UpdateLine(line, ccId, null, null, "0", null, null, null, "", null);
        }

        public string SMSBalanceConfig(int consumptionPercent1, int consumptionPercent2, int consumptionPercent3, int consumptionPercent4)
        {
            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            html = GetPage("https://claro-gestoronline.claro.com.br/evpn/admin/smsBalanceAlertEditShow.do", c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var token = doc.DocumentNode.Descendants("input").Single(p => p.Attributes["name"].Value == "org.apache.struts.taglib.html.TOKEN").Attributes["value"].Value;


            formParams = string.Format("org.apache.struts.taglib.html.TOKEN={0}&consumptionPercent1={1}&consumptionPercent2={2}&consumptionPercent3={3}&consumptionPercent4={4}&view.admin.smsBalanceAlert.ok.button.label.x=33&view.admin.smsBalanceAlert.ok.button.label.y=7",
                token, consumptionPercent1, consumptionPercent2, consumptionPercent3, consumptionPercent4);

            html = PostPage("https://claro-gestoronline.claro.com.br/evpn/admin/smsBalanceAlertEditProcess.do", formParams, c);

            doc.LoadHtml(html);

            var msg = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "msgErro")
                .SelectSingleNode("label").InnerText.Trim();

            return msg;
        }

        public string LongDistanceConfig(int csp, bool useCspDefault)
        {
            var c = new CookieContainer();

            var formParams = string.Format("link=&empresa={0}&usuario={1}&password={2}&x=15&y=2", Company, Username, Password);

            var html = PostPage("https://claro-gestoronline.claro.com.br/evpn/start.do", formParams, c);

            var cspText = string.Empty;

            if (useCspDefault)
            {
                cspText = "&useCSPDefault=on";
            }

            GetPage("https://claro-gestoronline.claro.com.br/evpn/admin/CSPDefaultShow.do", c);

            formParams = string.Format("CSP={0}{1}&useCSPDefault=false&view.admin.management.CSPDefault.update.button.label.x=30&view.admin.management.CSPDefault.update.button.label.y=7", csp, cspText);

            html = PostPage("https://claro-gestoronline.claro.com.br/evpn/admin/CSPDefaultDefine.do", formParams, c);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var msg = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "msgErro")
                .SelectSingleNode("label").InnerText.Trim();

            return msg;
        }
    }
}
