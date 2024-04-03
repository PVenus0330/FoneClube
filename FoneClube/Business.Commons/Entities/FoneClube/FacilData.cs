using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class IntlUserDashboard
    {
        public int IdPerson { get; set; }
        public IntlPerson PersonInfo { get; set; }
        public string CurrentBalance { get; set; }
        public string TotalDeposits { get; set; }
        public string TotalPurchases { get; set; }
        public string TotalPurchaseAmount { get; set; }
        public string TodaySale { get; set; }
        public string CurrentMonthSale { get; set; }
        public string OverallSale { get; set; }
        public string FilteredSale { get; set; }
        public List<Purchase> Purchases { get; set; }
        public List<Deposits> Deposits { get; set; }
        public decimal? DebitoSaldo { get; set; }
    }

    public class IntlPerson
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Purchase
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Plan { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Line { get; set; }
        public string Category { get; set; }
        public bool IsRefund { get; set; }
        public string Comment { get; set; }
        public string ICCID { get; set; }
        public decimal? ContelPrice { get; set; }
    }

    public class Deposits
    {
        public string ClientName { get; set; }
        public decimal USDAmount { get; set; }
        public string HandlingCharge { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }
        public string Comment { get; set; }
        public string IsRefund { get; set; }
    }

    public class PurchaseStat
    {
        public string Plan { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }
        public string Line { get; set; }
        public string Category { get; set; }
    }

    public class InitiateRefund
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public bool IsRefund { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
    }

    public class GetIntlDataReq
    {
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Operation { get; set; }
        public string Choice { get; set; }
    }
}
