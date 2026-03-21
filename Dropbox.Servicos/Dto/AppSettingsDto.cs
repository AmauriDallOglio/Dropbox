namespace Dropbox.Servicos.Dto
{
    public class AppSettingsDto
    {

        public string OAuth { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Configuracao { get; set; } = string.Empty;
        public string PastaBase { get; set; } = string.Empty;


        public string AppKey { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
      
    }
}
