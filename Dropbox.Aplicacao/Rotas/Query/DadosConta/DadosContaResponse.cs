using Dropbox.Servicos.Dto;

namespace Dropbox.Aplicacao.Rotas.Query.DadosConta
{
    public class DadosContaResponse
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public bool TipoBasico { get; set; }
        public bool TipoBusiness { get; set; }
        public bool TipoPro { get; set; }

        public static DadosContaResponse ConverterContaDropboxDto(ContaDropboxDto dto)
        {
            return new DadosContaResponse
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Pais = dto.Pais,
                AccountId = dto.AccountId,
                TipoBasico = dto.TipoBasico,
                TipoBusiness = dto.TipoBusiness,
                TipoPro = dto.TipoPro
            };
        }
    }
}
