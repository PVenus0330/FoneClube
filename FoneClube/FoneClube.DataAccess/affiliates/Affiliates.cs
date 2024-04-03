using Business.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess.affiliates
{
    public class Affiliates
    {
        public string GetReferralLink(int idPerson)
        {
            var encodeId = new Utils().Base64Encode(idPerson.ToString());
            var linkCadastro = "https://foneclube.com.br/convite/{0}";
            return string.Format(linkCadastro, encodeId);
        }
    }
}
