using Dropbox.Dominio.Entidade;

namespace Dropbox.Dominio.InterfaceRepositorio
{
    public interface IDropboxConfiguracaoRepositorio : IGenericoRepositorio<DropboxConfiguracao>
    {
        Task<DropboxConfiguracao?> ObterConfiguracaoAsync(CancellationToken cancellationToken);
       // Task AtualizarAsync(DropboxConfiguracao dropboxConfiguracao, CancellationToken cancellationToken);
    }
}
