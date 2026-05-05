namespace Dropbox.Servicos.Dto
{
    public class AppSettingsDto
    {
        public ArquivoConfiguracaoDto ArquivoConfiguracao { get; set; } = new ArquivoConfiguracaoDto();
        public ConnectionStringsDto ConnectionStrings { get; set; } = new ConnectionStringsDto();
        public Token Token { get; set; } = new Token();
    }

    public class Token
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }

    public class ConnectionStringsDto
    {
        public string ConexaoServidor { get; set; } = string.Empty;
        public string ConexaoDocker { get; set; } = string.Empty;
    }

    public class ArquivoConfiguracaoDto
    {
        public string OAuth { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Configurcao { get; set; } = string.Empty;
        public string PastaBase { get; set; } = string.Empty;
    }

}

 