using Dropbox.Aplicacao.Util;

namespace Dropbox.WebApi.Middleware
{
    public class ProcessaRequisicaoMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ProcessaRequisicaoMiddleware> _logger;
        private readonly ICacheSistemaServico _cache;

        public ProcessaRequisicaoMiddleware( RequestDelegate next,  ILogger<ProcessaRequisicaoMiddleware> logger,  ICacheSistemaServico cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Prometheus
            if (context.Request.Path.StartsWithSegments("/metrics") || context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }


            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await TratarErroAsync(context, ex);
            }
        }

        private async Task TratarErroAsync(HttpContext context, Exception exception)
        {
            var httpDicionarioCodigoErros = new Dictionary<Type, int>
            {
                { typeof(ArgumentException), StatusCodes.Status400BadRequest },
                { typeof(KeyNotFoundException), StatusCodes.Status404NotFound },
                { typeof(InvalidOperationException), StatusCodes.Status409Conflict },
                { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
                { typeof(FormatException), StatusCodes.Status422UnprocessableEntity },
                { typeof(NullReferenceException), StatusCodes.Status500InternalServerError }
            };

            var statusCode = httpDicionarioCodigoErros.TryGetValue(exception.GetType(), out var code)
                ? code
                : StatusCodes.Status500InternalServerError;

            _logger.LogError(exception, "Erro na aplicação");
            _cache.RegistrarErro(exception, context.Request.Path, exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = ResultadoOperacao.GerarErro(
                mensagem: exception.Message,
                codigo: statusCode,
                ex: exception,
                path: context.Request.Path
            );

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

