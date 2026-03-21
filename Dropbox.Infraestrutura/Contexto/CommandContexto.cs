using Microsoft.EntityFrameworkCore;

namespace Dropbox.Infraestrutura.Contexto
{
    public class CommandContexto : GenericoContexto
    {
        public CommandContexto(DbContextOptions<GenericoContexto> options) : base(options)
        {
        }
    }
}
