using Dropbox.Dominio.Entidade;

namespace Dropbox.Dominio.InterfaceRepositorio
{
    public interface IDropboxConfiguracaoRepositorio
    {
        Task<DropboxConfiguracao?> ObterPorTipoAsync(int tipo);
        Task AtualizarAsync(DropboxConfiguracao dropboxConfiguracao);
    }
}
