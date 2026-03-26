using Dropbox.Servicos.Dto;

namespace Dropbox.Aplicacao.Rotas.Query.ObterArquivos
{
    public class ObterArquivosResponse
    {
        public List<ObterArquivosItemResponse> Arquivos { get; set; } = new List<ObterArquivosItemResponse>();
    }


    public class ObterArquivosItemResponse
    {
        public string Nome { get; set; } = string.Empty;
        public string Caminho { get; set; } = string.Empty;
        public long? Tamanho { get; set; }
        public DateTime? DataModificacao { get; set; }

        public string LinkPreview { get; set; } = string.Empty;
        public string LinkDownload { get; set; } = string.Empty;

        public static ObterArquivosItemResponse ConverterArquivoDropboxDto(ArquivoDropboxDto dto)
        {
            return new ObterArquivosItemResponse
            {
                Nome = dto.Nome,
                Caminho = dto.Caminho,
                Tamanho = (long)(dto.Tamanho ?? 0),
                DataModificacao = dto.DataModificacao,
                LinkPreview = dto.LinkPreview,
                LinkDownload = dto.LinkDownload
            };
        }
    }
}
