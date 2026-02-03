using Microsoft.AspNetCore.Http;

namespace Dropbox.Aplicacao.EntidadeDto
{
    public class UploadArquivoRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
