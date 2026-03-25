using Dropbox.Aplicacao.Util;
using System.Text.Json;

namespace Dropbox.WebApi.Middleware
{
    public class ProcessaRequisicaoMiddleware
    {
        private readonly RequestDelegate _next;

        public ProcessaRequisicaoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            ICacheSistemaServico logService = context.RequestServices.GetRequiredService<ICacheSistemaServico>();
            try
            {

                logService.RegistrarAcesso(context.Request.Path, context.Request.Method, context.Response.StatusCode);

                await _next(context);
            }
            catch (Exception ex)
            {
                // logService.RegistrarErro(ex,  context.Request.Path, "Erro não tratado");

                context.Response.ContentType = "application/json";

                logService.RegistrarErro(ex, context.Request.Path, ex.Message);

                var resultado = ResultadoOperacao.GerarErro("Erro interno do sistema", StatusCodes.Status500InternalServerError, ex.Message, ex, context.Request.Path);
                context.Response.StatusCode = resultado.StatusCodigo ?? 500;

                var json = JsonSerializer.Serialize(resultado);
                await context.Response.WriteAsync(json);



                //  await context.Response.WriteAsync(JsonSerializer.Serialize(resultado));

            }
        }
    }
}

