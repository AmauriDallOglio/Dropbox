using Dropbox.Servicos.Dto;

namespace Dropbox.Aplicacao.Rotas.Command.EnviarArquivo
{
    public class EnviarArquivoResponse
    {
        public string Nome { get; set; } = "";
        public long Tamanho { get; set; }
        public string Caminho { get; set; } = "";
        public string LinkPreview { get; set; } = "";
        public string LinkDownload { get; set; } = "";

        public static EnviarArquivoResponse ConverterUploadArquivoResultadoDto(ArquivoDropboxDto dto)
        {
            return new EnviarArquivoResponse
            {
                Nome = dto.Nome,
                Tamanho = (long)dto.Tamanho,
                Caminho = dto.Caminho,
                LinkPreview = dto.LinkPreview,
                LinkDownload = dto.LinkDownload,
            };
        }
    }
}
