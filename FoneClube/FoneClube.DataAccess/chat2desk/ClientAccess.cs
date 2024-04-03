using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities.FoneClube.chat2desk.client;
using FoneClube.Business.Commons.Entities.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Business.Commons.Utils.Utils;

namespace FoneClube.DataAccess.chat2desk
{
    public class ClientWhatsappAccess
    {
        Utils _util;
        public ClientWhatsappAccess()
        {
            _util = new Utils { BaseAddress = ApiConstants.Chat2DeskBaseUrl };
        }
        public ResponseModel<ClientListModel> GetClients(int limit = 1000)
        {
            var result = _util.MakeWebRequest<ClientListModel>($"{ApiConstants.Chat2DeskBaseUrl}/v1/clients?limit={limit}", "GET", ApiConstants.Chat2DeskAPIToken);

            return result;
        }

        public ResponseModel<ClientListModel> GetClient(string phone)
        {
            var result = _util.MakeWebRequest<ClientListModel>($"{ApiConstants.Chat2DeskBaseUrl}/v1/clients?phone={phone}", "GET", ApiConstants.Chat2DeskAPIToken);

            return result;
        }

        public ResponseModel<ClientListModel> GetAllClients()
        {
            int offset = 0;
            List<ClientsDataModel> data = new List<ClientsDataModel>();

            C2DRequest:
            var result = _util.MakeWebRequest<ClientListModel>($"{ApiConstants.Chat2DeskBaseUrl}/v1/clients?limit=200&offset={offset}", "GET", ApiConstants.Chat2DeskAPIToken);

            if (result.Success)
            {
                data.AddRange(result.Data.Data);

                if (data.Count() < result.Data.Meta.total)
                {
                    offset = data.Count;
                    goto C2DRequest;
                }

                result.Data.Data = data;
            }

            return result;
        }

        public ResponseModel<GetClientModel> GetClient(long clientId)
        {
            var result = _util.MakeWebRequest<GetClientModel>($"{ApiConstants.Chat2DeskBaseUrl}/v1/clients/{clientId}", "GET", ApiConstants.Chat2DeskAPIToken);

            return result;
        }
    }
}
