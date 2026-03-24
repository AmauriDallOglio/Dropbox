using Dropbox.Aplicacao.Rotas.Command.CriarConta;
using Dropbox.Aplicacao.Rotas.Command.EnviarArquivo;
using Dropbox.Aplicacao.Rotas.Command.InserirCodigoUrl;
using Dropbox.Aplicacao.Rotas.Query.DadosConta;
using Dropbox.Aplicacao.Rotas.Query.ObterArquivos;
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
            services.AddScoped<GerarLinkAutorizacaoHandler>();
            services.AddScoped<GerarTokensHandler>();
            services.AddScoped<ObterArquivosHandler>();
            services.AddScoped<EnviarArquivoHandler>();

            //Dominio/Infraestrutura
            services.AddScoped<IDropboxConfiguracaoRepositorio, DropboxConfiguracaoRepositorio>();
            services.AddScoped(typeof(IGenericoRepositorio<>), typeof(GenericoRepositorio<>));


            //Servicos
            services.AddScoped<IDropboxServico, DropboxServico>();

        }
    }
}
