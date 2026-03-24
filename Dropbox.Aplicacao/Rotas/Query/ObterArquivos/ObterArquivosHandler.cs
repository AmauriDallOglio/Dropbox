using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Query.ObterArquivos
{
    public class ObterArquivosHandler
    {
        private readonly IDropboxServico _dropboxServico;

        public ObterArquivosHandler(IDropboxServico dropboxServico)
        {
            _dropboxServico = dropboxServico;
        }

        public async Task<ResultadoOperacao> Handle( ObterArquivosRequest request,  CancellationToken cancellationToken)
        {
            try
            {
                string pasta = "Arquivos";
                var arquivos = await _dropboxServico.ObterArquivos(pasta,  cancellationToken);
                ObterArquivosResponse response = new ObterArquivosResponse
                {
                    Arquivos = arquivos
                };
                return ResultadoOperacao.GerarSucesso(response, "Arquivos obtidos com sucesso" );
            }
            catch (Exception ex)
            {
                return ResultadoOperacao.GerarErro(  "Erro ao obter arquivos", 500, ex.Message);
            }
        }
    }
}
