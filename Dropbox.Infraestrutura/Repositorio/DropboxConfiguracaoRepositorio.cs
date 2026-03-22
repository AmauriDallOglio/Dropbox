using Dropbox.Dominio.Entidade;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Dropbox.Infraestrutura.Repositorio
{
    public class DropboxConfiguracaoRepositorio : GenericoRepositorio<DropboxConfiguracao>, IDropboxConfiguracaoRepositorio
    {
        private readonly GenericoContexto _context;

        public DropboxConfiguracaoRepositorio(GenericoContexto contexto) : base(contexto)
        {
            _context = contexto;
 

        }

 
        public async Task<DropboxConfiguracao?> ObterConfiguracaoAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<DropboxConfiguracao>().AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task AtualizarAsync(DropboxConfiguracao entity, CancellationToken cancellationToken)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
