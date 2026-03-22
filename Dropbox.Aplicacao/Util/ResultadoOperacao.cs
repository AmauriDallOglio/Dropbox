namespace Dropbox.Aplicacao.Util
{
    public sealed class ResultadoOperacao
    {
        public bool Sucesso { get; init; }
        public string Mensagem { get; init; } = string.Empty;
        public object? Resultado { get; init; }
        public int? StatusCodigo { get; init; }

        public static ResultadoOperacao GerarSucesso(object? dados = null, string? mensagem = null)
        {
            return new ResultadoOperacao
            {
                Sucesso = true,
                Mensagem = mensagem ?? "Operação realizada com sucesso.",
                Resultado = dados
            };
        }

        public static ResultadoOperacao GerarErro(string mensagem, int? codigo = null, object? dados = null)
        {
            return new ResultadoOperacao
            {
                Sucesso = false,
                Mensagem = mensagem,
                StatusCodigo = codigo,
                Resultado = dados
            };
        }
    }


}
