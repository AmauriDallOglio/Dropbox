using Microsoft.EntityFrameworkCore;

namespace Dropbox.Infraestrutura.Contexto
{
    public class QueryContexto : GenericoContexto
    {
        public QueryContexto(DbContextOptions<GenericoContexto> options) : base(options) { }

    }
}
