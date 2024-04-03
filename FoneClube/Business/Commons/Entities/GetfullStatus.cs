namespace FoneClube.Business.Commons.Entities
{
    internal class GetfullStatus : Configuration
    {
        public object QrCode { get; set; }
        public string Database { get; set; }
        public object Version { get; set; }
        public object Financeiro { get; set; }
        public object Localhost { get; set; }
        public int Pagarme { get; set; }
    }
}