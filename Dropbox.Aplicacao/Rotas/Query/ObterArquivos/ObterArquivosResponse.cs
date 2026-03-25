using Dropbox.Servicos.Dto;

namespace Dropbox.Aplicacao.Rotas.Query.ObterArquivos
{
    public class ObterArquivosResponse
    {
        public IEnumerable<ArquivoDropboxDto> Arquivos { get; set; } = new List<ArquivoDropboxDto>();
    }
}
