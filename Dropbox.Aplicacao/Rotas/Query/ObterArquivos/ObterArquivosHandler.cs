using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
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
            IEnumerable<ArquivoDropboxDto> arquivos = await _dropboxServico.ObterArquivosAsync(cancellationToken);
            List<ObterArquivosItemResponse> response = arquivos.Select(ObterArquivosItemResponse.ConverterArquivoDropboxDto).ToList();
            return ResultadoOperacao.GerarSucesso(new ObterArquivosResponse { Arquivos = response }, "Arquivos obtidos com sucesso");
        }
    }
}
