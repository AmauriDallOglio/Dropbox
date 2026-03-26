using Dropbox.Api.Files;

namespace Dropbox.Servicos.Dto
{
    public class ArquivoDropboxDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Caminho { get; set; } = string.Empty;
        public ulong? Tamanho { get; set; }
        public DateTime? DataModificacao { get; set; }

        public string LinkPreview { get; set; } = string.Empty;
        public string LinkDownload { get; set; } = string.Empty;

        private string UrlCompartilhada { get; set; } = string.Empty;


        private ArquivoDropboxDto() { }

        public ArquivoDropboxDto(string nome, string caminho, ulong? tamanho, DateTime? dataModificacao, string? urlCompartilhada)
        {
            Nome = nome;
            Caminho = caminho;
            Tamanho = tamanho;
            DataModificacao = dataModificacao;
            UrlCompartilhada = urlCompartilhada ?? string.Empty;

            Validar();
        }


        public static ArquivoDropboxDto Criar(FileMetadata fileMetadata, string preview, string download)
        {
            return new ArquivoDropboxDto
            {
                Nome = fileMetadata.Name,
                Caminho = fileMetadata.PathDisplay,
                Tamanho = fileMetadata.Size,
                DataModificacao = fileMetadata.ClientModified,
                LinkPreview = preview,
                LinkDownload = download
            };
        }

        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new Exception("Nome do arquivo inválido");

            if (string.IsNullOrWhiteSpace(Caminho))
                throw new Exception("Caminho do arquivo inválido");
        }




    }
}
