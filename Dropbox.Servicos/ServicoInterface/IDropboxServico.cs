using Dropbox.Api.Files;
using Dropbox.Servicos.Dto;
using Microsoft.AspNetCore.Http;

namespace Dropbox.Servicos.ServicoInterface
{
    public interface IDropboxServico
    {
        Task<FileMetadata> EnviarArquivoAsync(IFormFile file, string subFolder, CancellationToken cancellationToken);
        Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken);
        Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken);
    }
}
