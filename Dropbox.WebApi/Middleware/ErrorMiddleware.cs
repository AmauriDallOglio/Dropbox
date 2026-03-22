using Dropbox.Aplicacao.Util;
using System.Text.Json;

namespace Dropbox.WebApi.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";

                var resultado = ResultadoOperacao.GerarErro(
                    "Erro interno do sistema",
                    StatusCodes.Status500InternalServerError,
                    ex.Message
                );

                context.Response.StatusCode = resultado.StatusCodigo ?? 500;

                var json = JsonSerializer.Serialize(resultado);

                await context.Response.WriteAsync(json);

                ArquivoLog.Error($"Erro: {ex.Message}");
            }
        }
    }
}
