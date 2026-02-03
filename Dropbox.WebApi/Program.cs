
using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.Servico;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

namespace Dropbox.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<AppSettingsDto>(builder.Configuration.GetSection("Dropbox"));

            builder.Services.AddScoped<IDropboxServico, DropboxServico>();

            builder.Services.AddControllers();

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100_000_000;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dropbox Upload API",
                    Version = "v1",
                    Description = "API para upload e listagem de arquivos no Dropbox"
                });
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dropbox Upload API v1");
            });

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
