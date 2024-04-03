using FoneClube.BoletoSimples.APIs.Users.Models;
using FoneClube.BoletoSimples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.BoletoSimples.APIs.Auth
{
    /// <summary>
    /// Api que obtem informações do usuário logado
    /// </summary>
    public sealed class AuthApi
    {
        private readonly BoletoSimplesClient _client;
        private readonly HttpClientRequestBuilder _requestBuilder;

        public AuthApi(BoletoSimplesClient client)
        {
            _client = client;
            _requestBuilder = new HttpClientRequestBuilder(client);
        }

     
        /// <summary>
        /// Obtem informação do usuário pelo token de acesso
        /// </summary>
        /// <returns>Informações gerais do usuário</returns>
        /// <see cref="http://api.boletosimples.com.br/authentication/token/"/>
        public async Task<ApiResponse<UserInfo>> GetUserInfoAsync()
        {
            var request = _requestBuilder.To(_client.Connection.GetBaseUri(), "/userinfo")
                                         .WithMethod(HttpMethod.Get)
                                         .Build();

            return await _client.SendAsync<UserInfo>(request);
        }

    }
}
