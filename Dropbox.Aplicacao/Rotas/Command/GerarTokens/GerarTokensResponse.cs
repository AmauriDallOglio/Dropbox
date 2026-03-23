namespace Dropbox.Aplicacao.Rotas.Command.InserirCodigoUrl
{
    public class GerarTokensResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
    }
}
