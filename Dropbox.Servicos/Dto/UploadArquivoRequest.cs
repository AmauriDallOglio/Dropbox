using Microsoft.AspNetCore.Http;

namespace Dropbox.Servicos.Dto
{
    public class UploadArquivoRequest
    {
        public IFormFile? File { get; set; } = default!;
    }
}
