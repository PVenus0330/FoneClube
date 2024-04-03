using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;

namespace FoneClube.DataAccess
{
    public class LojaAccess
    {
        public CheckoutPagarMe GetPersonCheckoutLoja(int id)
        {
            var endereco = GetCustomerAddressLoja(id);
            var checkoutPerson = new CheckoutPagarMe();
            checkoutPerson.Email = endereco.Email;
            checkoutPerson.Street = endereco.Address1;
            checkoutPerson.StreetNumber = GetStreetNumber(endereco.CustomAttributes);
            checkoutPerson.Neighborhood = endereco.Address2;
            checkoutPerson.Zipcode = endereco.ZipPostalCode;
            checkoutPerson.DocumentNumber = GetDocument(id);


            return checkoutPerson;
        }

        private string GetStreetNumber(string customAddress)
        {
            var documentNumber = string.Empty;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(customAddress);

                var nodes = doc.GetElementsByTagName("AddressAttribute");

                var ID_REGISTRO = "17";
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes["ID"].Value == ID_REGISTRO)
                        documentNumber = node.InnerText;
                }

                return documentNumber;
            }
            catch (Exception)
            {

            }


            return documentNumber;
        }

        private string GetDocument(int id)
        {
            try
            {
                using (var ctx = new LojaDBEntities())
                {
                    return ctx.GenericAttribute.FirstOrDefault(c => c.EntityId == id && c.Key == "Customer_CPF").Value;
                }
            }
            catch (Exception e)
            {
                return "00000000000";
            }
        }

        public Address GetCustomerAddressLoja(int id)
        {
            try
            {
                using (var ctx = new LojaDBEntities())
                {
                    var customer = ctx.Customer.FirstOrDefault(c => c.Id == id);
                    return ctx.Address.FirstOrDefault(a => a.Id == customer.BillingAddress_Id);
                }
            }
            catch (Exception)
            {
                return new Address();
            }

        }
    }
}
