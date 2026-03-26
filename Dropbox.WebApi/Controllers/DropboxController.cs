using Dropbox.Aplicacao.Rotas.Command.CriarConta;
using Dropbox.Aplicacao.Rotas.Command.EnviarArquivo;
using Dropbox.Aplicacao.Rotas.Command.ExcluirArquivo;
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
        private readonly EnviarArquivoHandler _enviarArquivoHandler;
        private readonly ExcluirArquivoHandler _excluirArquivoHandler;

        public DropboxController(IDropboxServico dropboxServico, DadosContaHandler handler, GerarLinkAutorizacaoHandler criarContaHandler, GerarTokensHandler inserirCodigoUrlHandler, ObterArquivosHandler obterArquivosHandler, EnviarArquivoHandler enviarArquivoHandler, ExcluirArquivoHandler excluirArquivoHandler)
        {
            _IDropboxServico = dropboxServico;
            _dadosContaHandler = handler;
            _criarContaHandler = criarContaHandler;
            _InserirCodigoUrlHandler = inserirCodigoUrlHandler;
            _obterArquivosHandler = obterArquivosHandler;
            _enviarArquivoHandler = enviarArquivoHandler;
            _excluirArquivoHandler = excluirArquivoHandler;
        }

        [HttpGet("GerarLinkAutorizacao")]
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

            return Ok(resultado);
        }

        [HttpGet("ObterArquivos")]
        public async Task<IActionResult> ObterArquivos([FromQuery] ObterArquivosRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _obterArquivosHandler.Handle(request, cancellationToken);
            return Ok(resultado);
        }


        [HttpPost("EnviarArquivo")]
        public async Task<IActionResult> EnviarArquivo([FromForm] EnviarArquivoRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _enviarArquivoHandler.Handle(request, cancellationToken);
            return Ok(resultado);
        }

        [HttpPost("ExcluirArquivo")]
        public async Task<IActionResult> ExcluirArquivo([FromForm] ExcluirArquivoRequest request, CancellationToken cancellationToken)
        {
            ResultadoOperacao resultado = await _excluirArquivoHandler.Handle(request, cancellationToken);
            return Ok(resultado);
        }

    }
}