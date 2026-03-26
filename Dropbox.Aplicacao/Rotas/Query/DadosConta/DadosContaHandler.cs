using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.Dto;
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

            ContaDropboxDto info = await _dropboxServico.ObterInformacaoContaAsync(cancellationToken);
            DadosContaResponse response = DadosContaResponse.ConverterContaDropboxDto(info);
            return ResultadoOperacao.GerarSucesso(response, "Informações da conta obtidas com sucesso");

        }
    }
}
