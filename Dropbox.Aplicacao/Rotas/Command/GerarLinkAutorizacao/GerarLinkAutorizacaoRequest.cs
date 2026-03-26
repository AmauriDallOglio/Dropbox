namespace Dropbox.Aplicacao.Rotas.Command.CriarConta
{
    public class GerarLinkAutorizacaoRequest
    {
        public string AppKey { get; set; } = string.Empty;
        //  public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = string.Empty;
    }
}
