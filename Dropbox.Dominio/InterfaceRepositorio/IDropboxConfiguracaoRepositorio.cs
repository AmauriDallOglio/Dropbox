using Dropbox.Dominio.Entidade;

namespace Dropbox.Dominio.InterfaceRepositorio
{
    public interface IDropboxConfiguracaoRepositorio : IGenericoRepositorio<DropboxConfiguracao>
    {
        Task<DropboxConfiguracao?> ObterPorTipoAsync(int tipo, CancellationToken cancellationToken);
        Task AtualizarAsync(DropboxConfiguracao dropboxConfiguracao, CancellationToken cancellationToken);
    }
}
