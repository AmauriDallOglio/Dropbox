namespace Dropbox.Aplicacao.Dto
{
    public class CacheSistemaDto
    {
        public List<CacheErroDto> Erros { get; set; } = new();
        public List<CacheAcessoDto> Acessos { get; set; } = new();
    }

    public class CacheErroDto
    {
        public DateTime Data { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public static CacheErroDto Criar(Exception ex, string path, string mensagem)
        {
            return new CacheErroDto
            {
                Data = DateTime.Now,
                Mensagem = mensagem,
                Path = path ?? "Não informado",
                StackTrace = ex?.ToString() ?? "Não informado"
            };
        }
    }

    public class CacheAcessoDto
    {
        public DateTime Data { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public static CacheAcessoDto Criar(string path, string metodo, int statusCode)
        {
            return new CacheAcessoDto
            {
                Data = DateTime.Now,
                Path = path ?? "Não informado",
                Metodo = metodo,
                StatusCode = statusCode
            };
        }
    }
}
