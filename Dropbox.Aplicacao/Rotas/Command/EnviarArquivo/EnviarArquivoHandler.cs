using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Command.EnviarArquivo
{
    public class EnviarArquivoHandler
    {
        private readonly IDropboxServico _dropboxServico;

        private readonly AppSettingsDto _appSettings;

        public EnviarArquivoHandler(IDropboxServico dropboxServico, AppSettingsDto appSettings)
        {
            _dropboxServico = dropboxServico;
            _appSettings = appSettings;
        }

        public async Task<ResultadoOperacao> Handle(EnviarArquivoRequest request, CancellationToken cancellationToken)
        {
            ArquivoDropboxDto resultado = await _dropboxServico.EnviarArquivoAsync(request.Arquivo, cancellationToken);
            EnviarArquivoResponse response = EnviarArquivoResponse.ConverterUploadArquivoResultadoDto(resultado);
            return ResultadoOperacao.GerarSucesso(response, "Arquivo enviado com sucesso");
        }

    }
}
