using Dropbox.Dominio.Entidade;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Dropbox.Infraestrutura.Repositorio
{
    public class DropboxConfiguracaoRepositorio : GenericoRepositorio<DropboxConfiguracao>, IDropboxConfiguracaoRepositorio
    {
        private readonly GenericoContexto _context;

        public DropboxConfiguracaoRepositorio(GenericoContexto dbContext) : base(dbContext)
        {
            _context = dbContext;
 

        }

 
        public async Task<DropboxConfiguracao?> ObterPorTipoAsync(int tipo, CancellationToken cancellationToken)
        {
            return await _context.Set<DropboxConfiguracao>()
                .FirstOrDefaultAsync(x => x.Tipo == tipo);
        }

        public async Task AtualizarAsync(DropboxConfiguracao entity, CancellationToken cancellationToken)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
