using Dropbox.Aplicacao.Rotas.Query.DadosConta;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Dropbox.WebApi.Controllers
{
    [ApiController]
    [Route("api/dropbox")]
    public class DropboxController : ControllerBase
    {
        private readonly IDropboxServico _IDropboxServico;
        //https://www.dropbox.com/developers/apps

        private readonly DadosContaHandler _handler;

        public DropboxController(IDropboxServico dropboxServico, DadosContaHandler handler  )
        {
            _IDropboxServico = dropboxServico;
            _handler = handler;
        }

        [HttpGet("DadosConta")]
        public async Task<IActionResult> DadosConta(CancellationToken cancellationToken)
        {
            var request = new DadosContaRequest();

            var resultado = await _handler.Handle(request, cancellationToken);

            int status = resultado.StatusCodigo ?? StatusCodes.Status200OK;

            return StatusCode(status, resultado);
        }



        //[HttpPost("EnviarArquivo")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> EnviarArquivo([FromForm] UploadArquivoRequest request, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (request.File == null)
        //        {
        //            ResultadoOperacao<object> erro = await ResultadoOperacao<object>.ErroAsync(null, "Não foi possível enviar o arquivo.", string.Empty, 500);
        //            return BadRequest(erro);
        //        }
        //        var resultado = await _IDropboxServico.EnviarArquivoAsync(request, "Arquivos", cancellationToken);
        //        Console.WriteLine($"Arquivo enviado: {resultado.Name}");
        //        Console.WriteLine($"Tamanho: {resultado.Size}");
        //        Console.WriteLine($"Path: {resultado.PathDisplay}");

        //        ResultadoOperacao<object> sucesso = await ResultadoOperacao<object>.SucessoAsync(resultado.PathDisplay, mensagem: "Arquivo enviado com sucesso.", detalhes: string.Empty, statusCode: StatusCodes.Status200OK);
        //        return Ok(sucesso);
        //    }
        //    catch (Exception ex)
        //    {
        //        ResultadoOperacao<object> erro = await ResultadoOperacao<object>.ErroAsync(
        //            null, 
        //            "Erro ao enviar arquivo.", 
        //            ex.Message, 
        //            500);

        //        return BadRequest(erro);
        //    }
        //}



        //[HttpGet("ObterArquivos")]
        //public async Task<IActionResult> ObterArquivos(CancellationToken cancellationToken)
        //{
        //    var arquivos = await _IDropboxServico.ObterArquivos("Arquivos", cancellationToken);
        //    return Ok(arquivos);
        //}


    }
}