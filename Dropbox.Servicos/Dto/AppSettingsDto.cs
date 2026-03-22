namespace Dropbox.Servicos.Dto
{
    public class AppSettingsDto
    {
        public ArquivoConfiguracaoDto ArquivoConfiguracao { get; set; } = new ArquivoConfiguracaoDto();
        public ConnectionStringsDto ConnectionStrings { get; set; } = new ConnectionStringsDto();

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

 