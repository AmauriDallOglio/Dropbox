using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Command.ExcluirArquivo
{
    public class ExcluirArquivoHandler
    {
        private readonly IDropboxServico _dropboxServico;

        public ExcluirArquivoHandler(IDropboxServico dropboxServico)
        {
            _dropboxServico = dropboxServico;
        }

        public async Task<ResultadoOperacao> Handle(ExcluirArquivoRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NomeArquivo))
                throw new ArgumentException("Nome do arquivo é obrigatório");

            ExcluirArquivoResultadoDto resultado = await _dropboxServico.ExcluirArquivoAsync(request.NomeArquivo, cancellationToken);
            ExcluirArquivoResponse response = ExcluirArquivoResponse.ConverterExcluirArquivoResultadoDto(resultado);

            return ResultadoOperacao.GerarSucesso(response, "Arquivo processado para exclusão");
        }
    }
}
