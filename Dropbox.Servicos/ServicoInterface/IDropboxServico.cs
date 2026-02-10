using Dropbox.Api.Files;
using Dropbox.Aplicacao.EntidadeDto;

namespace Dropbox.Servicos.ServicoInterface
{
    public interface IDropboxServico
    {
        Task<FileMetadata> EnviarArquivoAsync(UploadArquivoRequest request, string subFolder, CancellationToken cancellationToken);
        Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken);
        Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken);
    }
}
