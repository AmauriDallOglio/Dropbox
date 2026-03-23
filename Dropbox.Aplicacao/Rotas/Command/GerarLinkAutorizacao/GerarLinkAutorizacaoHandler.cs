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
            try
            {
                Console.WriteLine("Configuração não encontrada. Criando automaticamente...");


                string url = "https://www.dropbox.com/oauth2/authorize" +
                                   $"?client_id={request.AppKey}" +
                                   "&response_type=code" +
                                   "&token_access_type=offline" +
                                   $"&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}";


                Console.WriteLine(url);

                GerarLinkAutorizacaoResponse response = new GerarLinkAutorizacaoResponse
                {
                    UrlAutorizacao = url
                };

                return ResultadoOperacao.GerarSucesso( response, "Informações da conta obtidas com sucesso" );

            }
            catch (Exception ex)
            {
                return ResultadoOperacao.GerarErro("Erro ao obter dados da conta",  500, ex.Message );
            }
        }
    }
}
