namespace Dropbox.Aplicacao.Rotas.Command.InserirCodigoUrl
{
    public class GerarTokensRequest
    {
        public string Code { get; set; } = "";
        public string AppKey { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
    }
}
