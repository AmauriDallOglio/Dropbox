namespace Dropbox.Dominio.Entidade
{
    public class DropboxConfiguracao
    {
        public int Id { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string? AppKey { get; set; }
        public string? AppSecret { get; set; }
        public string? RedirectUri { get; set; }
        public string? Pasta { get; set; }
        public string? NomeArquivo { get; set; }


    }
}
