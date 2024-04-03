using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{

    public class PlanoData
    {
        public int id { get; set; }
        public string plano { get; set; }
        public double valor { get; set; }
        public string nome_plano { get; set; }
        public string ligacoes { get; set; }
        public string sms { get; set; }
        public string detalhamento { get; set; }
        public string descricao_detalhamento { get; set; }
        public string cor { get; set; }
    }


    public class PlanosList
    {
        public List<PlanoData> data { get; set; }
    }


    public class ValidateICCIDRequest
    {
        public string iccid { get; set; }
    }

    public class CPFUIResponse
    {
        public int status { get; set; }
        public string nome { get; set; }
    }

    public class CPFRequest
    {
        public string cpf { get; set; }
    }
    public class CNPJRequest
    {
        public string cnpj { get; set; }
    }

    public class CNPJConteRes
    {
        public string cnpj { get; set; }
        public string razao_social { get; set; }
        public string nome_fantasia { get; set; }
    }

    public class CNPJConteResponse
    {
        public bool valido { get; set; }
        public CNPJConteRes data { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CPFConteRes
    {
        public string cpf { get; set; }
        public string titular { get; set; }
        public string nascimento { get; set; }
        public int idade { get; set; }
    }

    public class CPFConteResponse
    {
        public bool valido { get; set; }
        public CPFConteRes data { get; set; }
    }

    public class ValidateICCIDResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public Info info { get; set; }
    }

  
    public class Detalhes1
    {
        public string status { get; set; }
        public object motivo_cancelamento_linha { get; set; }
    }

    public class ValidateNumberByICCIDRes
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public string numero { get; set; }
        public Detalhes1 detalhes { get; set; }
    }




    public class Info
    {
        public string data_solicitacao { get; set; }
        public string data_ativacao { get; set; }
        public string cliente { get; set; }
        public string plano { get; set; }
        public string plano_nome { get; set; }
        public string iccid { get; set; }
        public string numero_ativado { get; set; }
        public string numero_portado { get; set; }
        public string forma_pagamento { get; set; }
        public string esim { get; set; }
    }


    #region ActivatePlanRequest

    public class ActivatePlanRequest
    {
        public string metodo_pagamento { get; set; }
        public string nome { get; set; }
        public string cpf { get; set; }
        public string cnpj { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }
        public string data_nascimento { get; set; }
        public Endereco endereco { get; set; }
        public List<Chip> chips { get; set; }
    }


    public class Chip
    {
        public string iccid { get; set; }
        public int id_plano { get; set; }
        public int ddd { get; set; }
        public string esim { get; set; }
    }


    public class Endereco
    {
        public string rua { get; set; }
        public string numero { get; set; }
        public string complemento { get; set; }
        public string bairro { get; set; }
        public string cep { get; set; }
        public string municipio { get; set; }
        public string uf { get; set; }
    }

    #endregion

    #region ActivatePlanResponse

    public class ActivatePlanResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public ActivatePlanResponseInfo info { get; set; }
        public string link { get; set; }
        public string link_esim { get; set; }
    }


    public class Ativacao
    {
        public string id_plano { get; set; }
        public string ddd { get; set; }
    }


    public class ActivatePlanResponseChip
    {
        public string id { get; set; }
        public Ativacao ativacao { get; set; }
        public string iccid { get; set; }
        public string esim { get; set; }
    }


    public class ActivatePlanResponseInfo
    {
        public int id { get; set; }
        public string id_cliente { get; set; }
        public string data_cadastro { get; set; }
        public List<ActivatePlanResponseChip> chips { get; set; }
    }
    #endregion

    #region ActivatePortRequest


    public class ActivatePortRequest
    {
        public string numero_contel { get; set; }
        public string doador_numero { get; set; }
        public int doador_id_operadora { get; set; }
    }


    public class ActivatePortResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public Portabilidade portabilidade { get; set; }

    }


    public class Portabilidade
    {
        public string doador_numero { get; set; }
        public int operadora { get; set; }
        public int id_chip { get; set; }
        public string data_cadastro { get; set; }
        public int id { get; set; }
    }
    #endregion

    #region PortabilidadeResponse

    public class PortabilidadeResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public List<Datum> data { get; set; }
    }


    public class Datum
    {
        public int id { get; set; }
        public string descricao { get; set; }
    }
    #endregion

    #region LinhasResponse

    public class LinhasDatum
    {
        public string id { get; set; }
        public string linha { get; set; }
        public string iccid { get; set; }
        public string titular { get; set; }
        public string titular_apelido { get; set; }
        public object data_ex { get; set; }
        public object data_portout { get; set; }
        public object nome_identificacao { get; set; }
        public object emoji { get; set; }
        public string data_ativacao { get; set; }
        public string data_inicio_plano { get; set; }
        public string data_fim_plano { get; set; }
        public string data_renovacao { get; set; }
        public string data_cancelamento_linha { get; set; }
        public string data_perda_numero_falta_recarga { get; set; }
        public string plano { get; set; }
        public string documento_titular { get; set; }
        public string status { get; set; }
        public string recorrencia_cartao { get; set; }
        public string portin { get; set; }
        public string esim { get; set; }
        public string esim_pdf { get; set; }
        public string bloqueada { get; set; }
        public string recarga_automatica { get; set; }
        public bool? bitRecAutoFC { get; set; }
    }

    public class LinhasPagingRequest
    {
        public int page { get; set; }
    }
    public class LinhasPagingResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public Pagination pagination { get; set; }
        public List<LinhasDatum> data { get; set; }
    }

    public class Pagination
    {
        public int current_page { get; set; }
        public int quantity_per_page { get; set; }
        public int last_page { get; set; }
    }

    public class LinhasResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public List<LinhasDatum> data { get; set; }
    }


    #endregion

    #region SaldoResponse

    public class SaldoData
    {
        public double restante_dados { get; set; }
        public int restante_minutos { get; set; }
        public int restante_sms { get; set; }
    }

    public class SaldoResponse
    {
        public string line { get; set; }
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public SaldoData data { get; set; }
    }


    public class Detalhes
    {
        public string id { get; set; }
        public string id_linha { get; set; }
        public string linha { get; set; }
        public string linha_apelido { get; set; }
        public string iccid { get; set; }
        public string titular { get; set; }
        public string titular_apelido { get; set; }
        public object nome_identificacao { get; set; }
        public string emoji { get; set; }
        public string data_ativacao { get; set; }
        public string data_inicio_plano { get; set; }
        public string data_fim_plano { get; set; }
        public string data_renovacao { get; set; }
        public string data_perda_numero_falta_recarga { get; set; }
        public string plano { get; set; }
        public string tipo_linha { get; set; }
        public string documento_titular { get; set; }
        public string status { get; set; }
        public string motivo_cancelamento_linha { get; set; }
        public string data_cancelamento_linha { get; set; }
        public string recorrencia_cartao { get; set; }
        public string portin { get; set; }
        public string bloqueada { get; set; }
        public string esim { get; set; }
        public string esim_pdf { get; set; }
        public string recarga_automatica { get; set; }
        public string recarga_automatica_plano { get; set; }
        public string cor_app { get; set; }
        public string portabilidade_linha_doadora { get; set; }
        public string portabilidade_status { get; set; }
        public string portabilidade_data_cadastro { get; set; }
        public string portabilidade_data_agendamento { get; set; }
        public string portabilidade_data_aceite { get; set; }
        public string portabilidade_data_sucesso { get; set; }
        public object data_ex { get; set; }
        public object data_portout { get; set; }
    }


    public class DetalhesRes
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public Detalhes detalhes { get; set; }
    }

    #endregion


    public class ContelPhoneData
    {
        public string plano { get; set; }
        public string esim { get; set; }
        public string recarga_automatica { get; set; }
        public string recarga_automatica_plano { get; set; }
        public string bloqueada { get; set; }
        public string titular { get; set; }
        public string documento_titular { get; set; }
        public string status { get; set; }
        public string recorrencia_cartao { get; set; }
        public string iccid { get; set; }
        public string portin { get; set; }
        public string data_ativacao { get; set; }
        public string linha { get; set; }
        public string Recarregar { get; set; }
        public double restante_dados { get; set; }
        public int restante_minutos { get; set; }
        public int restante_sms { get; set; }
        public string data_fim_plano { get; set; }
        public string data_renovacao { get; set; }
        public string data_cancelamento_linha { get; set; }
        public string data_inicio_plano { get; set; }
        public string LastRecharge { get; set; }
        public string LastPaidDate { get; set; }
        public string LastPaidAmount { get; set; }
        public int LastPaidDays { get; set; }
        public int DaysSinceLastTopup { get; set; }
        public string Pago { get; set; }
        public string PaidPlano { get; set; }

    }

    public class Numero
    {
        public string numero { get; set; }
        public int id_plano { get; set; }
    }

    public class TopUpPlanRequest
    {
        public string metodo_pagamento { get; set; }
        public List<Numero> numeros { get; set; }
    }

    public class TopUpUIPlanRequest
    {
        public string metodo_pagamento { get; set; }
        public bool extra { get; set; }
        public int planGB { get; set; }
        public List<Numero> numeros { get; set; }
    }


    public class Recarga
    {
        public int id { get; set; }
        public string data_cadastro { get; set; }
        public string link_pagamento { get; set; }
        public string boleto_link { get; set; }
        public string boleto_codigo_barras { get; set; }
        public string pix_QRCode { get; set; }
        public string pix_copia_cola { get; set; }
    }


    public class TopUpPlanResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public Recarga recarga { get; set; }
    }


    public class TopUpUIPlanResponse
    {
        public bool bitTopupDone { get; set; }
        public int Status { get; set; }
        public string Linha { get; set; }
        public bool bitWarning { get; set; }
        public bool bitZeroGB { get; set; }
        public string PreFimPlano { get; set; }
        public string PostFimPlano { get; set; }
        public string DataFimPlano { get; set; }
        public string PortIn { get; set; }
        public string StatusPortabilidade { get; set; }
        public string PreSaldo { get; set; }
        public string PostSaldo { get; set; }
        public string SaldoGBAdded { get; set; }
        public string StatusGB { get; set; }
        public DateTime LastTopup { get; set; }
    }


    public class BlockLine
    {
        public string numero { get; set; }
        public string observacoes { get; set; }
        public string motivo { get; set; }
    }

    public class UnBlockLine
    {
        public string numero { get; set; }
    }

    public class BlockLineResponse
    {
        public bool status { get; set; }
        public string mensagem { get; set; }
        public string data { get; set; }
    }


    public class BlockLineResponseUI
    {
        public BlockLineResponse BlockLineResponse { get; set; }
        public string Linha { get; set; }
    }


    public class Historico
    {
        public string data_cadastro { get; set; }
        public string data_recarga { get; set; }
        public string plano { get; set; }
        public string valor { get; set; }
        public string valor_pago { get; set; }
        public string formaPagto { get; set; }
        public string solicitado_por { get; set; }
    }


    public class TopUpHistoryResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public List<Historico> historico { get; set; }
    }


    public class TopupHistoryUIResponse
    {
        public Person Person { get; set; }
        public ContelPhoneData ContelLineData { get; set; }
        public HistoryResponse TopUpHistoryData { get; set; }

    }


    public class HistoryResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public List<HistoricoUI> historico { get; set; }
    }


    public class HistoricoUI
    {
        public string data_cadastro { get; set; }
        public string data_recarga { get; set; }
        public string plano { get; set; }
        public string valor { get; set; }
        public string formaPagto { get; set; }
        public string solicitado_por { get; set; }
    }


    public class RemainingSaldo
    {
        public string saldo { get; set; }
    }

    public class CompraEndereco
    {
        public string rua { get; set; }
        public string numero { get; set; }
        public string complemento { get; set; }
        public string bairro { get; set; }
        public string cep { get; set; }
        public int id_cidade { get; set; }
    }

    public class CompraRequest
    {
        public string metodo_pagamento { get; set; }
        public int id_plano { get; set; }
        public string nome { get; set; }
        public string cpf { get; set; }
        public string cnpj { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }
        public string data_nascimento { get; set; }
        public int quantidade { get; set; }
        public CompraEndereco endereco { get; set; }
    }
    public class CompraResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public string link { get; set; }
    }

    public class CityData
    {
        public int id { get; set; }
        public string cidade { get; set; }
        public string uf { get; set; }
    }

    public class CityResponse
    {
        public List<CityData> data { get; set; }
    }
    public class ApelidoRequest
    {
        public string linha { get; set; }
        public string linha_apelido { get; set; }
    }
    public class ApelidoResponse
    {
        public string mensagem { get; set; }
    }

    public class AuthTokenRequest
    {
        public string chave_acesso { get; set; }
        public string chave_acesso_franquia { get; set; }
    }
    public class AuthTokenDevRequest
    {
        public string chave_acesso { get; set; }
        public string ambiente { get; set; }
        public string chave_acesso_franquia { get; set; }
    }

    public class AuthTokenResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public string token { get; set; }
    }

    public class ImportTopupHistory
    {
        public string txtTipo { get; set; }
        public string DteDateRec { get; set; }
        public string txtValor { get; set; }
        public string txtName { get; set; }
        public string txtApelido { get; set; }
        public string txtLinha { get; set; }
        public string txtPlano { get; set; }
        public string txtValorPlano { get; set; }
        public string txtREALIZADAPOR { get; set; }
    }

    public class ResetLine
    {
        public string linha { get; set; }
        public string novo_iccid { get; set; }
        public string motivo { get; set; }
    }

    public class ResetLineRes
    {
        public string message { get; set; }
        public string esim_pdf { get; set; }
        public string iccid { get; set; }
    }

    
    public class ESIMICCIDRes
    {
        public string id { get; set; }
        public string iccid { get; set; }
        public string tipo { get; set; }
        public string pdf_esim { get; set; }
    }

    public class ESIMICCIDResponse
    {
        public bool retorno { get; set; }
        public string mensagem { get; set; }
        public List<ESIMICCIDRes> data { get; set; }
    }


}
