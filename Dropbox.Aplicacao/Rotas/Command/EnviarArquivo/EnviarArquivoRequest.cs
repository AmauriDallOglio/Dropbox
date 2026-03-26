using Microsoft.AspNetCore.Http;

namespace Dropbox.Aplicacao.Rotas.Command.EnviarArquivo
{
    public class EnviarArquivoRequest
    {

        public IFormFile? Arquivo { get; set; } = default!;
    }
}
