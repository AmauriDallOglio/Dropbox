using Microsoft.AspNetCore.Http;

namespace Dropbox.Aplicacao.Rotas.Command.EnviarArquivo
{
    public class EnviarArquivoRequest
    {
 
        public IFormFile File { get; set; } = default!;
    }
}
