namespace Dropbox.Aplicacao.Rotas.Command.CriarConta
{
    public class GerarLinkAutorizacaoRequest
    {
        public string AppKey { get; set; } = "";
      //  public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
    }
}
