using FoneClube.BoletoSimples.APIs.Auth;
using FoneClube.BoletoSimples.APIs.BankBillets;
using FoneClube.BoletoSimples.Common;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FoneClube.BoletoSimples
{
    /// <summary>
    /// Cliente de acesso aos recursos da Api do boleto simples
    /// </summary>
    public class BoletoSimplesClient : IDisposable
    {
        public readonly ClientConnection Connection;

        /// <summary>
        /// Api que prove informações do usuário pelo token de acesso
        /// </summary>
        public readonly AuthApi Auth;

        /// <summary>
        /// BankBilletsApi Api de boletos
        /// </summary>
        public readonly BankBilletsApi BankBillets;

        private readonly HttpClient _client;

        /// <summary>
        /// Client default sem personalizar nenuma configuração nesse construtor suas configurações provem do arquivo de configuração
        /// </summary>
        public BoletoSimplesClient() : this(new HttpClient(), new ClientConnection())
        { }

        /// <summary>
        /// Client default com dados da conexão de externos ao seu arquivo de configuração
        /// </summary>
        /// <param name="clientConnection">Dados de conexão com a api</param>
        public BoletoSimplesClient(ClientConnection clientConnection) : this(new HttpClient(), clientConnection)
        { }

        /// <summary>
        /// Client http customizado e dados da conexão de externos ao seu arquivo de configuração
        /// </summary>
        /// <param name="client">Sua versão do HttpClient</param>
        /// <param name="clientConnection">Dados de conexão com a api</param>
        public BoletoSimplesClient(HttpClient client, ClientConnection clientConnection)
        {
            _client = client;
            _client.BaseAddress = clientConnection.GetBaseUri();
            Connection = clientConnection;

            Auth = new AuthApi(this);
            BankBillets = new BankBilletsApi(this);
        }

        internal async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request, default(CancellationToken));
            request.Dispose();

            return new ApiResponse<T>(response);
        }


        internal async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request, default(CancellationToken));
            request.Dispose();

            return response;
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
