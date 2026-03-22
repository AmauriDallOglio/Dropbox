using Dropbox.Aplicacao.Util;
using Dropbox.WebApi.Configuracao;
using Dropbox.WebApi.Middleware;

namespace Dropbox.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


            var ambiente = builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json";
            ArquivoLog.Info($"Configuração: {ambiente}");
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ambiente, optional: false, reloadOnChange: true)
                .Build();



            AppSettingsConfiguracao.Carregar(builder.Services, configuration);
            InjecaoDependenciaConfiguracao.Carregar(builder.Services);
            ApiConfiguracao.Carregar(builder.Services);



            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dropbox Upload API v1");
            });

            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseMiddleware<ErrorMiddleware>();



            app.Run();
        }
    }
}
