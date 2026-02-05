namespace Dropbox.Aplicacao.EntidadeDto
{
    public class InformacaoContaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public bool TipoBasico { get; set; } 
        public bool TipoBusiness { get; set; }
        public bool TipoPro { get; set; }
    }
}
