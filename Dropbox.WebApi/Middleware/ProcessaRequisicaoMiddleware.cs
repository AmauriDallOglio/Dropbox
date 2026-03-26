using Dropbox.Api;
using Dropbox.Aplicacao.Util;
using System.Text.Json;

namespace Dropbox.WebApi.Middleware
{
    public class ProcessaRequisicaoMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ProcessaRequisicaoMiddleware> _logger;
        private readonly ICacheSistemaServico _cache;

        public ProcessaRequisicaoMiddleware(
            RequestDelegate next,
            ILogger<ProcessaRequisicaoMiddleware> logger,
            ICacheSistemaServico cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await Handle(context, ex);
            }
        }

        private async Task Handle(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                FormatException => StatusCodes.Status422UnprocessableEntity,

                // SEU DOMÍNIO (melhor prática)
                //   AppException appEx => appEx.StatusCode,

                _ => StatusCodes.Status500InternalServerError
            };

            _logger.LogError(ex, "Erro na aplicação");

            _cache.RegistrarErro(ex, context.Request.Path, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = ResultadoOperacao.GerarErro(
                mensagem: ex.Message,
                codigo: statusCode,
                ex: ex,
                path: context.Request.Path
            );

            await context.Response.WriteAsJsonAsync(response);
        }


        //private readonly RequestDelegate _next;

        //public ProcessaRequisicaoMiddleware(RequestDelegate next)
        //{
        //    _next = next;
        //}

        //public async Task Invoke(HttpContext context)
        //{
        //    ICacheSistemaServico logService = context.RequestServices.GetRequiredService<ICacheSistemaServico>();
        //    try
        //    {

        //        logService.RegistrarAcesso(context.Request.Path, context.Request.Method, context.Response.StatusCode);

        //        await _next(context);
        //    }
        //    catch (Exception ex)
        //    {
        //        // logService.RegistrarErro(ex,  context.Request.Path, "Erro não tratado");

        //        context.Response.ContentType = "application/json";

        //        logService.RegistrarErro(ex, context.Request.Path, ex.Message);

        //        var resultado = ResultadoOperacao.GerarErro("Erro interno do sistema", StatusCodes.Status500InternalServerError, ex.Message, ex, context.Request.Path);
        //        context.Response.StatusCode = resultado.StatusCodigo ?? 500;

        //        var json = JsonSerializer.Serialize(resultado);
        //        await context.Response.WriteAsync(json);



        //        //  await context.Response.WriteAsync(JsonSerializer.Serialize(resultado));

        //    }
        //}
    }
}

