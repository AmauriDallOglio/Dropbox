using Dropbox.Aplicacao.Util;
using Dropbox.WebApi.Configuracao;
using Dropbox.WebApi.Middleware;
using Serilog;

namespace Dropbox.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string ambiente = builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json";
            //PrintaConsole.Alerta($"Configuração: {ambiente}");
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ambiente, optional: false, reloadOnChange: true)
                .Build();



            // CONFIGURAÇÃO DO SERILOG
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console() // aparece no terminal e Azure
                .WriteTo.File(@"C:\Logs\Dropbox\error-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();


            // PLUGA O SERILOG NO ASP.NET
            builder.Host.UseSerilog();

            PrintaConsole.Info("Carregando appsettings");
            AppSettingsConfiguracao.Carregar(builder.Services, configuration);

            PrintaConsole.Info("Carregando injeção de dependência");
            InjecaoDependenciaConfiguracao.Carregar(builder.Services);

            PrintaConsole.Info("Carregando configuração da API");
            ConfiguracaoApi.Carregar(builder.Services);


            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/swagger/index.html");
                    return;
                }
                await next();
            });
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.MapControllers();
            app.UseMiddleware<ProcessaRequisicaoMiddleware>();
            app.Run();
        }
    }
}
