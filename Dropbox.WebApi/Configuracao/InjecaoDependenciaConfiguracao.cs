using Dropbox.Aplicacao.Rotas.Query.DadosConta;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Infraestrutura.Repositorio;
using Dropbox.Servicos.Servico;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.WebApi.Configuracao
{
    public static class InjecaoDependenciaConfiguracao
    {
        public static void Carregar(IServiceCollection services)
        {
            //Aplicação
            services.AddScoped<DadosContaHandler>();

            //Dominio/Infraestrutura
            services.AddScoped<IDropboxConfiguracaoRepositorio, DropboxConfiguracaoRepositorio>();
            services.AddScoped(typeof(IGenericoRepositorio<>), typeof(GenericoRepositorio<>));


            //Servicos
            services.AddScoped<IDropboxServico, DropboxServico>();

        }
    }
}
