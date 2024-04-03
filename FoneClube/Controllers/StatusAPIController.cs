using Microsoft.AspNetCore.Mvc;
using FoneClube.DataAccess;
using FoneClube.Business.Commons.Entities.FoneClube;
using Microsoft.Extensions.Configuration;

namespace FoneClube.WebAPI.Controllers
{
    public class StatusAPIController : Controller
    {
        private readonly IConfiguration _configuration;

        public StatusAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("database/name")]
        public string GetCliente()
        {
            return new StatusAPIAccess().GetDatabaseName();
        }

        [Route("version")]
        public string GetVersion()
        {
            return _configuration["VersaoAPI"];
        }

        [Route("pagarme")]
        public string GetStatusPagarme()
        {
            return _configuration["APIKEY"].Substring(0, 8) + " - " + _configuration["ENCRYPTIONKEY"].Substring(0, 8);
        }

        [Route("financeiro")]
        public string GetEmailFinanceiro()
        {
            return _configuration["EmailFinanceiro"];
        }

        [Route("localhost")]
        public string GetLocalHost()
        {
            return _configuration["ExecutandoLocalHost"];
        }

        [Route("full")]
        public FoneClube.Business.Commons.Entities.Configuration GetfullStatus()
        {
            return new FoneClube.Business.Commons.Entities.Configuration
            {
                QrCode = _configuration["qrcodelink"],
                Database = new StatusAPIAccess().GetDatabaseName(),
                Version = _configuration["VersaoAPI"], // Specify the correct FoneClube.Business.Commons.Entities.Configuration.Version property here
                Financeiro = _configuration["EmailFinanceiro"],
                Localhost = _configuration["ExecutandoLocalHost"],
                Pagarme = _configuration["APIKEY"].Substring(0, 8) + " - " + _configuration["ENCRYPTIONKEY"].Substring(0, 8)
            };
        }
    }
}
