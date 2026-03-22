using Dropbox.Dominio.Entidade;
using Dropbox.Infraestrutura.Mapeamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Dropbox.Infraestrutura.Contexto
{
    public class GenericoContexto : DbContext
    {

        public GenericoContexto(DbContextOptions<GenericoContexto> options) : base(options)
        {
        }


        public DbSet<DropboxConfiguracao> DropboxConfiguracao { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DropboxConfiguracaoMepeamento());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }
}
