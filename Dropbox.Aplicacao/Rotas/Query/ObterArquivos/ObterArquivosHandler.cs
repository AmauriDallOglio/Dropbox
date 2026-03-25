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

        public async Task<ResultadoOperacao> Handle(ObterArquivosRequest request, CancellationToken cancellationToken)
        {
            try
            {
                string pasta = "Arquivos";
                var arquivos = await _dropboxServico.ObterArquivosAsync(pasta, cancellationToken);
                return ResultadoOperacao.GerarSucesso(new ObterArquivosResponse { Arquivos = arquivos }, "Arquivos obtidos com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter arquivos: {ex.Message}");
                return ResultadoOperacao.GerarErro("Erro ao obter arquivos", 500, ex.Message);
            }
        }
    }
}
