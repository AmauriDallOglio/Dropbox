namespace Dropbox.Servicos.Dto
{
    public class UploadRequest
    {
        public string Pasta { get; set; } = "";
        public string NomeArquivo { get; set; } = "";
        public string Conteudo { get; set; } = "";
        public TipoToken TipoToken { get; set; }
    }

    public enum TipoToken
    {
        OAuth = 0,
        Token = 1
    }
}
