using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace Dropbox.Aplicacao.EntidadeDto
{
    public class ResultadoOperacaoDto
    {
        public bool Sucesso { get; set; } = true;
        public string Mensagem { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }


    public enum TipoRetorno : byte
    {
        [Description("Sucesso")]
        Sucesso = 0,

        [Description("Falha")]
        Falha = 1,

        [Description("Parcial")]
        Parcial = 2
    }

    public class ResultadoOperacao<T> : ResultadoOperacaoDto
    {
        public T? Resultado { get; set; }

        public static Task<ResultadoOperacao<T>> SucessoAsync( T? data, string mensagem, string detalhes, int statusCode)
        {
            return Task.FromResult(new ResultadoOperacao<T>
            {
                Sucesso = true,
                Mensagem = mensagem,
                Detalhes = detalhes,
                Resultado = data,
                StatusCode = statusCode
            });
        }

        public static Task<ResultadoOperacao<T>> ErroAsync(T? data, string mensagem, string detalhes, int statusCode)
        {
            return Task.FromResult(new ResultadoOperacao<T>
            {
                Sucesso = false,
                Mensagem = mensagem,
                Detalhes = detalhes,
                Resultado = default, // null
                StatusCode = statusCode
            });
        }

    }

    

}
