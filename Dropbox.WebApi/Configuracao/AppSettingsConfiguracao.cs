using Dropbox.Aplicacao.Util;
using Dropbox.Infraestrutura.Contexto;
using Dropbox.Servicos.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dropbox.WebApi.Configuracao
{
    public static class AppSettingsConfiguracao
    {
        public static void Carregar(this IServiceCollection services, IConfigurationRoot configuration)
        {

            AppSettingsDto appSettingsDto = configuration?.Get<AppSettingsDto>() ?? new AppSettingsDto();
            appSettingsDto = CarregaBancoDeDados(services, configuration, appSettingsDto);
            ImprimeAppSettingsDto(appSettingsDto);
            services.AddSingleton(appSettingsDto);
        }

        private static AppSettingsDto CarregaBancoDeDados(this IServiceCollection services, IConfigurationRoot configuration, AppSettingsDto appSettingsDto)
        {
            string conexao = "";
            string azureDbCommand = Environment.GetEnvironmentVariable("AZURE_DB") ?? string.Empty;
            if (!string.IsNullOrEmpty(azureDbCommand))
            {
                appSettingsDto.ConnectionStrings.ConexaoServidor = azureDbCommand;
                conexao = azureDbCommand;
            }
            else
            {

                conexao = appSettingsDto.ConnectionStrings.ConexaoServidor;
            }

            services.AddSqlServer<GenericoContexto>(conexao);
            services.AddDbContext<CommandContexto>(opt => opt.UseSqlServer(conexao));
            services.AddDbContext<QueryContexto>(opt => opt.UseSqlServer(conexao));
            return appSettingsDto;
        }

        private static void ImprimeAppSettingsDto(AppSettingsDto appSettings)
        {
            string json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            ArquivoLog.Alerta($"AppSettingsDto carregado: {json}");
        }


    }
}
