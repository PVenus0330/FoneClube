using FoneClube.Business.Commons.Entities.Vivo;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FoneClube.DataAccess
{
    public class VivoAccess
    {
        //test line
        private const string Username = "webcrawler";
        private const string Password = "010203";

        //live line
        //private const string Username = "chopp01";
        //private const string Password = "010203";

        public string Login(CookieContainer c)
        {
            var url = "http://vivogestao.vivoempresas.com.br/Portal/api/datapackcompanyinfo";

            string formParams = "{'user': '" + Username + "','password': '" + Password + "','action': 'login'}";

            var req = WebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = c;

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0";
            req.Host = "vivogestao.vivoempresas.com.br";
            req.Accept = "application/json";
            req.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            req.Method = "POST";
            req.ContentType = "application/json; charset=UTF-8";
            req.Referer = "http://vivogestao.vivoempresas.com.br/Portal/data/login";

            byte[] bytes = Encoding.ASCII.GetBytes(formParams);

            req.ContentLength = bytes.Length;

            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            var resp = req.GetResponse() as HttpWebResponse;

            var reader = new StreamReader(resp.GetResponseStream());
            var html = reader.ReadToEnd();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return html;
        }

        public string GetPage(string url, CookieContainer c, out string vs, string formId = null)
        {
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = c;
            //req.Timeout = int.MaxValue;

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            req.Method = "GET";
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            var resp = req.GetResponse() as HttpWebResponse;

            var reader = new StreamReader(resp.GetResponseStream());
            var html = reader.ReadToEnd();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (string.IsNullOrEmpty(formId))
            {
                vs = doc.GetElementbyId("javax.faces.ViewState").Attributes["value"].Value;
            }
            else
            {
                vs = doc.GetElementbyId(formId).SelectSingleNode("id[javax.faces.ViewState]").Attributes["value"].Value;
            }


            return html;
        }

        public string PostPage(string url, string formParams, CookieContainer c, out string vs, string formId = null)
        {
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = c;

            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0";
            req.Host = "vivogestao.vivoempresas.com.br";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            //req.Referer = "http://vivogestao.vivoempresas.com.br/Portal/pages/home/displayHome.jsf";

            byte[] bytes = Encoding.ASCII.GetBytes(formParams);

            req.ContentLength = bytes.Length;

            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            var resp = req.GetResponse() as HttpWebResponse;

            var reader = new StreamReader(resp.GetResponseStream());
            var html = reader.ReadToEnd();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (string.IsNullOrEmpty(formId))
            {
                vs = doc.GetElementbyId("javax.faces.ViewState").Attributes["value"].Value;
            }
            else
            {
                vs = doc.GetElementbyId(formId).SelectSingleNode("id[javax.faces.ViewState]").Attributes["value"].Value;
            }

            return html;
        }

        //GroupCode is the value of group
        public Dictionary<string, string> ListNumbers(string groupCode)
        {
            var c = new CookieContainer();
            string vs;

            var html = Login(c);

            html = GetPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", c, out vs);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string formId = doc.DocumentNode.Descendants("a").Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "btnGeral")
                .Single().Attributes["onclick"].Value.Split('\'', ':')[4];

            var formParams = string.Format("formSearchMobile:selector={1}&formSearchMobile:modeSearchRadio=msisdn&formSearchMobile:msisdn=&formSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&formSearchMobile:_idcl=formSearchMobile:{2}", vs, groupCode, formId);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out vs);

            var data = ParseNumbers(html);

            var dictionary = new Dictionary<string, string>();

            foreach (var item in data)
            {
                dictionary.Add(item.Key, item.Value);
            }

            //ajax request
            int page = 2;

            while (true)
            {
                formParams = string.Format("AJAXREQUEST=_viewRoot&resultSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&resultSearchMobile:resultSearchMobile_dataTable:resultSearchMobile_scroll={1}&ajaxSingle=resultSearchMobile:resultSearchMobile_dataTable:resultSearchMobile_scroll&AJAX:EVENTS_COUNT=1&", vs, page);

                string v;

                html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out v);

                data = ParseNumbers(html);

                if (dictionary.ContainsKey(data.First().Key))
                {
                    break;
                }

                foreach (var item in data)
                {
                    dictionary.Add(item.Key, item.Value);
                }

                ++page;
            }

            return dictionary;
        }

        private Dictionary<string, string> ParseNumbers(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var table = doc.GetElementbyId("resultSearchMobile:resultSearchMobile_dataTable");

            return table.SelectNodes("tbody/tr/td[1]/a").ToDictionary(p => p.InnerText, p => p.Attributes["onclick"].Value.Split('\'')[7]);
        }

        public VivoLineInfo GetStatusLine(string lineNumber)
        {
            try
            {
                var status = new VivoAccess().GetLineStatus(lineNumber);

                return new VivoLineInfo
                {
                    Status = status,
                    Ativa = "ATIVADO" == status ? true : false
                };
            }
            catch (Exception)
            {
                return new VivoLineInfo { Ativa = false };
            }
        }

        public string GetLineStatus(string lineNumber)
        {
            var c = new CookieContainer();
            string vs;

            var doc = new HtmlDocument();

            var html = Login(c);

            html = GetPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", c, out vs);

            doc.LoadHtml(html);

            var div = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "linhaBotao").InnerHtml;

            var start = div.IndexOf("formSearchMobile:j_id");

            var idcl = div.Substring(start, 23);

            var selector = doc.GetElementbyId("formSearchMobile:selector").Descendants("option").First().Attributes["Value"].Value;

            var formParams = string.Format("formSearchMobile:selector={1}&formSearchMobile:modeSearchRadio=msisdn&formSearchMobile:msisdn={3}&formSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&formSearchMobile:_idcl={2}", vs, selector, idcl, lineNumber);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out vs);

            doc.LoadHtml(html);

            var res = doc.GetElementbyId("resultSearchMobile:resultSearchMobile_dataTable").SelectNodes("tbody/tr[1]/td[1]/a").Single().OuterHtml.Split('\'');

            formParams = string.Format("resultSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&lineSelected={1}&resultSearchMobile:_idcl={2}", vs, res[7], res[3]);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out vs);


            doc.LoadHtml(html);

            var val = doc.DocumentNode.Descendants("div").Where(p => p.InnerHtml.Contains("Situa&#231;&#227;o")
                       && p.Attributes.Contains("class") && p.Attributes["class"].Value == "coluna5 flutuaEsquerda")
                       .Single().Descendants("label").Last().InnerText;

            return val;
        }

        public bool SetLineStatus(string lineNumber, bool isActive)
        {
            int activeVal = isActive ? 51 : 52;

            var c = new CookieContainer();
            string vs;

            var doc = new HtmlDocument();

            var html = Login(c);

            html = GetPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", c, out vs);

            doc.LoadHtml(html);

            var div = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "linhaBotao").InnerHtml;

            var start = div.IndexOf("formSearchMobile:j_id");

            var idcl = div.Substring(start, 23);

            var selector = doc.GetElementbyId("formSearchMobile:selector").Descendants("option").First().Attributes["Value"].Value;

            var formParams = string.Format("formSearchMobile:selector={1}&formSearchMobile:modeSearchRadio=msisdn&formSearchMobile:msisdn={3}&formSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&formSearchMobile:_idcl={2}", vs, selector, idcl, lineNumber);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out vs);

            doc.LoadHtml(html);

            var res = doc.GetElementbyId("resultSearchMobile:resultSearchMobile_dataTable").SelectNodes("tbody/tr[1]/td[1]/a").Single().OuterHtml.Split('\'');

            formParams = string.Format("resultSearchMobile_SUBMIT=1&javax.faces.ViewState={0}&lineSelected={1}&resultSearchMobile:_idcl={2}", vs, res[7], res[3]);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displayMobileUpdate.jsf", formParams, c, out vs);

            doc.LoadHtml(html);

            var displayGroup = doc.DocumentNode.Descendants("input").Single(p => p.Attributes.Contains("value") && p.Attributes["value"].Value == "mobileGroupSituation").Attributes["name"].Value;

            div = doc.DocumentNode.Descendants("div").Single(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "limpaBoth linhaBotao").InnerHtml;

            start = div.IndexOf("displayGroupConsult:j_id");

            var displayGroupConsult = div.Substring(start, 27);

            formParams = string.Format("{1}=mobileGroupSituation&displayGroupConsult_SUBMIT=1&javax.faces.ViewState={0}&displayGroupConsult:_idcl={2}", vs, displayGroup, displayGroupConsult);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/displaySelectedMobile.jsf", formParams, c, out vs);

            doc.LoadHtml(html);

            var select = doc.DocumentNode.Descendants("select").Single(p => p.InnerHtml.Contains(">Ativo<")).Attributes["name"].Value;

            var a = doc.DocumentNode.Descendants("a").Single(p => p.InnerHtml.Contains("<span>Ok</span>")).Attributes["onclick"].Value.Split('\'');

            var group = doc.GetElementbyId("idGroupFlg").Descendants("option").Single(p => p.Attributes.Contains("selected")).Attributes["value"].Value;

            formParams = string.Format("idGroupFlg={4}&{5}={1}&{2}_SUBMIT=1&javax.faces.ViewState={0}=&{2}:_idcl={3}", vs, activeVal, a[1], a[3], group, select);

            html = PostPage("http://vivogestao.vivoempresas.com.br/Portal/pages/administration/mobile/updateMobileGroupSituation.jsf", formParams, c, out vs);

            return html.Contains("sucesso");
        }
    }
}
