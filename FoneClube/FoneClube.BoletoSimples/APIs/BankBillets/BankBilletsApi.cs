using FoneClube.BoletoSimples.APIs.BankBillets.Models;
using FoneClube.BoletoSimples.APIs.BankBillets.RequestMessages;
using FoneClube.BoletoSimples.Common;
using System.Net.Http;
using System.Threading.Tasks;

namespace FoneClube.BoletoSimples.APIs.BankBillets
{
    /// <summary>
    /// Api de boletos
    /// </summary>
    public sealed class BankBilletsApi
    {
        private readonly BoletoSimplesClient _client;
        private readonly HttpClientRequestBuilder _requestBuilder;
        private const string BANK_BILLET_API = "/bank_billets";

        public BankBilletsApi(BoletoSimplesClient client)
        {
            _client = client;
            _requestBuilder = new HttpClientRequestBuilder(client);
        }


        /// <summary>
        /// Cria um boleto
        /// </summary>
        /// <param name="bankBillet">dados do boleto</param>
        /// <returns>Boleto criado com sucesso</returns>
        /// <see cref="http://api.boletosimples.com.br/reference/v1/bank_billets/#criar-boleto"/>
        public async Task<ApiResponse<BankBillet>> PostAsync(BankBillet bankBillet)
        {
            var request = _requestBuilder.To(_client.Connection.GetBaseUri(), BANK_BILLET_API)
                                         .WithMethod(HttpMethod.Post)
                                         .AndOptionalContent(bankBillet)
                                         .Build();

            return await _client.SendAsync<BankBillet>(request);
        }

        /// <summary>
        /// Obter informações de um boleto
        /// </summary>
        /// <param name="id">Identificador do boleto</param>
        /// <returns>Boleto criado com sucesso</returns>
        /// <see cref="http://api.boletosimples.com.br/reference/v1/bank_billets/#informaes-do-boleto"/>
        public async Task<ApiResponse<BankBillet>> GetAsync(int id)
        {
            var request = _requestBuilder.To(_client.Connection.GetBaseUri(), $"{BANK_BILLET_API}/{id}")
                                         .WithMethod(HttpMethod.Get)
                                         .Build();

            return await _client.SendAsync<BankBillet>(request);
        }

       
        /// <summary>
        /// Cancelar um boleto
        /// Você pode cancelar boletos nos status de Aberto(opened) ou Vencido(overdue)
        /// </summary>
        /// <param name="id">Identificador do boleto</param>
        /// <returns>HttpResponseMessage with HttpStatusCode 204 (NO Content)</returns>
        /// <see cref="http://api.boletosimples.com.br/reference/v1/bank_billets/#cancelar-boleto"/>
        public async Task<HttpResponseMessage> CancelAsync(int id)
        {
            var request = _requestBuilder.To(_client.Connection.GetBaseUri(), $"{BANK_BILLET_API}/{id}/cancel")
                                         .WithMethod(HttpMethod.Put)
                                         .Build();

            return await _client.SendAsync(request);
        }

        /// <summary>
        /// Duplicar um boleto
        /// No momento não há cálculo de juros automáticos que atualizem o valor do boleto
        /// </summary>
        /// <param name="id">Identificador do boleto</param>
        /// <param name="requestMessage">Parâmetros de duplicação</param>
        /// <returns>Boleto criado com sucesso</returns>
        /// <see cref="http://api.boletosimples.com.br/reference/v1/bank_billets/#gerar-segunda-via-do-boleto"/>
        public async Task<ApiResponse<BankBillet>> DuplicateAsync(int id, Duplicate requestMessage)
        {
            var request = _requestBuilder.To(_client.Connection.GetBaseUri(), $"{BANK_BILLET_API}/{id}/duplicate")
                                         .WithMethod(HttpMethod.Post)
                                         .AndOptionalContent(requestMessage)
                                         .Build();

            return await _client.SendAsync<BankBillet>(request);
        }


    }
}
