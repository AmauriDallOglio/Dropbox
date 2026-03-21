using Dropbox.Dominio.Entidade;

namespace Dropbox.Servicos.Dto
{
    public class DropboxTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }

        public static DropboxTokenDto ConverterEntidadeParaDto(DropboxConfiguracao entity)
        {
            return new DropboxTokenDto
            {
                AccessToken = entity.AccessToken ?? string.Empty,
                RefreshToken = entity.RefreshToken ?? string.Empty,
                ExpiresAt = entity.ExpiresAt
            };
        }

        public static DropboxConfiguracao ConverterDtoParaEntidade(DropboxTokenDto dto)
        {
            return new DropboxConfiguracao
            {
                AccessToken = dto.AccessToken ?? string.Empty,
                RefreshToken = dto.RefreshToken ?? string.Empty,
                ExpiresAt = dto.ExpiresAt
            };
        }


    }
}
