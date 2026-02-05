using Dropbox.Aplicacao.EntidadeDto;
using Microsoft.AspNetCore.Http;

namespace Dropbox.Servicos.ServicoInterface
{
    public interface IDropboxServico
    {
        Task EnviarArquivoAsync(UploadArquivoRequest request, string subFolder, CancellationToken cancellationToken);
        Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken);
        Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken);
    }
}
