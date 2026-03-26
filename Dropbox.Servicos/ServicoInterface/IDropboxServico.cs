using Dropbox.Servicos.Dto;
using Microsoft.AspNetCore.Http;

namespace Dropbox.Servicos.ServicoInterface
{
    public interface IDropboxServico
    {
        Task<ArquivoDropboxDto> EnviarArquivoAsync(IFormFile file, CancellationToken cancellationToken);
        Task<IEnumerable<ArquivoDropboxDto>> ObterArquivosAsync(CancellationToken cancellationToken);
        Task<ContaDropboxDto> ObterInformacaoContaAsync(CancellationToken cancellationToken);
        Task<ExcluirArquivoResultadoDto> ExcluirArquivoAsync(string nomeArquivo, CancellationToken cancellationToken);

        Task<string> GerarLinkAutorizacaoAsync(string appKey, string redirectUri, CancellationToken cancellationToken);
    }
}
