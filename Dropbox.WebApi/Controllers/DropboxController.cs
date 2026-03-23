using Dropbox.Aplicacao.Rotas.Command.CriarConta;
using Dropbox.Aplicacao.Rotas.Command.InserirCodigoUrl;
using Dropbox.Aplicacao.Rotas.Query.DadosConta;
using Dropbox.Aplicacao.Rotas.Query.ObterArquivos;
using Dropbox.Aplicacao.Util;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Mvc;

namespace Dropbox.WebApi.Controllers
{
    [ApiController]
    [Route("api/dropbox")]
    public class DropboxController : ControllerBase
    {
        private readonly IDropboxServico _IDropboxServico;
        //https://www.dropbox.com/developers/apps

        private readonly DadosContaHandler _dadosContaHandler;
        private readonly GerarLinkAutorizacaoHandler _criarContaHandler;
        private readonly GerarTokensHandler _InserirCodigoUrlHandler;
        private readonly ObterArquivosHandler _obterArquivosHandler;

        public DropboxController(IDropboxServico dropboxServico, DadosContaHandler handler, GerarLinkAutorizacaoHandler criarContaHandler, GerarTokensHandler inserirCodigoUrlHandler, ObterArquivosHandler obterArquivosHandler)
        {
            _IDropboxServico = dropboxServico;
            _dadosContaHandler = handler;
            _criarContaHandler = criarContaHandler;
            _InserirCodigoUrlHandler = inserirCodigoUrlHandler;
            _obterArquivosHandler = obterArquivosHandler;
        }


        [HttpPost("GerarLinkAutorizacao")]
        public async Task<IActionResult> GerarLinkAutorizacao([FromForm] GerarLinkAutorizacaoRequest request, CancellationToken cancellationToken)
        {
            ResultadoOperacao resultado = await _criarContaHandler.Handle(request, cancellationToken);
            return Ok(resultado);
        }


        [HttpPost("GerarTokens")]
        public async Task<IActionResult> GerarTokens([FromForm] GerarTokensRequest request, CancellationToken cancellationToken)
        {
            ResultadoOperacao resultado = await _InserirCodigoUrlHandler.Handle(request, cancellationToken);
            return Ok(resultado);
        }




        [HttpGet("DadosConta")]
        public async Task<IActionResult> DadosConta(CancellationToken cancellationToken)
        {
            DadosContaRequest request = new DadosContaRequest();

            ResultadoOperacao resultado = await _dadosContaHandler.Handle(request, cancellationToken);

            int status = resultado.StatusCodigo ?? StatusCodes.Status200OK;

            return StatusCode(status, resultado);
        }




        [HttpGet("ObterArquivos")]
        public async Task<IActionResult> ObterArquivos( [FromQuery] ObterArquivosRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _obterArquivosHandler.Handle(request, cancellationToken);

            int status = resultado.StatusCodigo ?? StatusCodes.Status200OK;

            return StatusCode(status, resultado);
        }


        //[HttpPost("enviar-arquivo")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> EnviarArquivo([FromForm] UploadArquivoRequest request, CancellationToken cancellationToken)
        //{
        //    if (request.File == null)
        //    {

        //        var erro = await ResultadoOperacao.GerarErro(null, "Não foi possível enviar o arquivo.", string.Empty, 400);
        //        return BadRequest(erro);
        //    }

        //    var resultado = await _dropboxServico.EnviarArquivoAsync(request, "Arquivos", cancellationToken);
        //    var sucesso = await ResultadoOperacao.GerarSucesso(resultado.PathDisplay, "Arquivo enviado com sucesso.", string.Empty, 200);

        //    return Ok(sucesso);
        //}

        //[HttpGet("obter-arquivos")]
        //public async Task<IActionResult> ObterArquivos(CancellationToken cancellationToken)
        //{
        //    var arquivos = await _dropboxServico.ObterArquivos("Arquivos", cancellationToken);
        //    return Ok(arquivos);
        //}


    }
}