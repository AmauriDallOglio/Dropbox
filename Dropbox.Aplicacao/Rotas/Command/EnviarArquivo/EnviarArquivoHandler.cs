using Dropbox.Api.Files;
using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Command.EnviarArquivo
{
    public class EnviarArquivoHandler
    {
        private readonly IDropboxServico _dropboxServico;

        public EnviarArquivoHandler(IDropboxServico dropboxServico)
        {
            _dropboxServico = dropboxServico;
        }

        public async Task<ResultadoOperacao> Handle(EnviarArquivoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return ResultadoOperacao.GerarErro(   "Arquivo inválido",  400  );
                }

             
                FileMetadata resultado = await _dropboxServico.EnviarArquivoAsync(request.File, "Arquivos", cancellationToken);

                EnviarArquivoResponse response = new EnviarArquivoResponse
                {
                    Nome = resultado.Name,
                    Tamanho = (long)resultado.Size,
                    Caminho = resultado.PathDisplay
                };
                return ResultadoOperacao.GerarSucesso(response,  "Arquivo enviado com sucesso" );
            }
            catch (Exception ex)
            {
                return ResultadoOperacao.GerarErro( "Erro ao enviar arquivo", 500,  ex.Message );
            }
        }
    }
}
