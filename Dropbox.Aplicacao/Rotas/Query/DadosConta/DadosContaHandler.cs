using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.ServicoInterface;

namespace Dropbox.Aplicacao.Rotas.Query.DadosConta
{
    public class DadosContaHandler
    {
        private readonly IDropboxServico _dropboxServico;

        public DadosContaHandler(IDropboxServico dropboxServico)
        {
            _dropboxServico = dropboxServico;
        }

        public async Task<ResultadoOperacao> Handle(DadosContaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var info = await _dropboxServico.ObterInformacaoContaAsync(cancellationToken);
                DadosContaResponse response = new DadosContaResponse
                {
                    Conta = info
                };

                return ResultadoOperacao.GerarSucesso( response, "Informações da conta obtidas com sucesso" );
            }
            catch (Exception ex)
            {
                return ResultadoOperacao.GerarErro("Erro ao obter dados da conta", 500, ex.Message);
            }
        }
    }
}
