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

            builder.Property(x => x.UltimaAtualizacaoToken).HasColumnType("DATETIME2").IsRequired(false);
            builder.Property(x => x.UltimaAtualizacaoRefresh).HasColumnType("DATETIME2").IsRequired(false);
            builder.Property(x => x.UltimaAtualizacaoCodigo).HasColumnType("DATETIME2").IsRequired(false);



        }

        /*
         * 
         * 
         *
          --create database Dropbox

            -- drop table DropboxConfiguracao
            use Dropbox

            CREATE TABLE DropboxConfiguracao (
                Id INT IDENTITY PRIMARY KEY,
 
                AccessToken NVARCHAR(MAX),
                RefreshToken NVARCHAR(MAX),
                ExpiresAt DATETIME2,
                AppKey NVARCHAR(200),
                AppSecret NVARCHAR(200),
                RedirectUri NVARCHAR(500),
                Pasta NVARCHAR(500),
                NomeArquivo NVARCHAR(200)
            );

 
             ALTER TABLE DropboxConfiguracao
            ADD UltimaAtualizacaoToken DATETIME2 NULL,
                UltimaAtualizacaoCodigo DATETIME2 NULL,
                UltimaAtualizacaoRefresh DATETIME2 NULL;

         * 
         * 
         */
    }
}
