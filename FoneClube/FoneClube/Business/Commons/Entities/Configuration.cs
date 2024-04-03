namespace FoneClube.Business.Commons.Entities
{
    public class Configuration
    {
        public string QrCode { get; set; }
        public string Database { get; internal set; }
        public string Version { get; internal set; }
        public string Financeiro { get; internal set; }
        public string Localhost { get; internal set; }
        public string Pagarme { get; internal set; }
    }
}