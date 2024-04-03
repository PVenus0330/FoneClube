using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class CheckoutPagarMe
    {
        public int Amount { get; set; }
        public int DaysLimit { get; set; }
        public string BoletoInstructions { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string DocumentNumber { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }
        public string Neighborhood { get; set; }
        public string Zipcode { get; set; }
        public string Ddd { get; set; }
        public string Number { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string CardExpirationDate { get; set; }
        public string CardCvv { get; set; }
        public int? IdCustomerPagarme { get; set; }
        public int? IdCustomerFoneclube { get; set; }
        public List<PlanoStore> SelectedPlans { get; set; }
        public int? Frete { get; set; }
    }

    public class PlanoStore
    {
        public int id { get; set; }
        public string descricao { get; set; }
        public string descricaoURL { get; set; }
        public int valor { get; set; }
        public int valorPromocional { get; set; }
        public int totalItensCarrinho { get; set; }
        public int valorFantasia { get; set; }
        public int valorOriginal { get; set; }
        public string apresentacao { get; set; }
        public string tituloClube { get; set; }
        public string textoClube { get; set; }
        public string imagem { get; set; }
        public string planoOptions1 { get; set; }
        public string planoOptions2 { get; set; }
        public string planoOptions3 { get; set; }
        public string planoOptions4 { get; set; }
        public string txtbeneficio { get; set; }
        public bool isEsim { get; set; }
        public bool isPort { get; set; }
        public string txtPortNumber { get; set; }
        public int cartId { get; set; }
    }
}
