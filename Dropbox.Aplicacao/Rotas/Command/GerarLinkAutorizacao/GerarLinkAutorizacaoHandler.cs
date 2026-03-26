using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Command.CriarConta
{
    public class GerarLinkAutorizacaoHandler
    {
        private readonly IDropboxServico _dropboxServico;

        public GerarLinkAutorizacaoHandler(IDropboxServico dropboxServico)
        {
            _dropboxServico = dropboxServico;
        }


        public async Task<ResultadoOperacao> Handle(GerarLinkAutorizacaoRequest request, CancellationToken cancellationToken)
        {
            string url = await _dropboxServico.GerarLinkAutorizacaoAsync(request.AppKey, request.RedirectUri, cancellationToken);
            GerarLinkAutorizacaoResponse response = new GerarLinkAutorizacaoResponse { UrlAutorizacao = url };
            return ResultadoOperacao.GerarSucesso(response, "Link de autorização gerado com sucesso");
        }
    }
}
