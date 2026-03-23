using Dropbox.Dominio.Entidade;

namespace Dropbox.Servicos.Dto
{
    public class DropboxConfiguracaoDto
    {
        public string AppKey { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string Pasta { get; set; } = "";
        public string NomeArquivo { get; set; } = "";

        public DropboxConfiguracaoDto ConverterEntidadeParaDto(DropboxConfiguracao entity)
        {
            return new DropboxConfiguracaoDto
            {
                AppKey = entity.AppKey ?? string.Empty,
                AppSecret = entity.AppSecret ?? string.Empty,
                RedirectUri = entity.RedirectUri ?? string.Empty,
                Pasta = entity.Pasta ?? string.Empty,
                NomeArquivo = entity.NomeArquivo ?? string.Empty
            };
        }





    }
}
