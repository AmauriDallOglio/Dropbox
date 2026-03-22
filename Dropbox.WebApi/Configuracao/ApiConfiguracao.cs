using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

namespace Dropbox.WebApi.Configuracao
{
    public static class ApiConfiguracao
    {

        public static void Carregar(this IServiceCollection services)
        {

            services.AddControllers();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000;
            });

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dropbox Upload API",
                    Version = "v1",
                    Description = "API para upload e listagem de arquivos no Dropbox"
                });
            });
        }

    }
}
