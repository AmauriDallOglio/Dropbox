using Dropbox.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dropbox.Infraestrutura.Mapeamento
{
    public class DropboxConfiguracaoMepeamento : IEntityTypeConfiguration<DropboxConfiguracao>
    {
        public void Configure(EntityTypeBuilder<DropboxConfiguracao> builder)
        {
            builder.ToTable("DropboxConfiguracao");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AccessToken).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.RefreshToken).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.ExpiresAt).HasColumnType("DATETIME2");
            builder.Property(x => x.AppKey).HasMaxLength(200);
            builder.Property(x => x.AppSecret).HasMaxLength(200);
            builder.Property(x => x.RedirectUri).HasMaxLength(500);
            builder.Property(x => x.Pasta).HasMaxLength(500);
            builder.Property(x => x.NomeArquivo).HasMaxLength(200);
            builder.Property(x => x.Tipo).IsRequired();
            builder.HasIndex(x => x.Tipo).IsUnique();
        }
    }
}
