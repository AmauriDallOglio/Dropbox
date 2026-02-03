using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Mvc;

namespace Dropbox.WebApi.Controllers
{
    [ApiController]
    [Route("api/dropbox")]
    public class DropboxController : ControllerBase
    {
        private readonly IDropboxServico _IDropboxServico;

        public DropboxController(IDropboxServico dropboxServico)
        {
            _IDropboxServico = dropboxServico;
        }

        [HttpGet("DadosConta")]
        public async Task<IActionResult> DadosConta(CancellationToken cancellationToken)
        {
            var info = await _IDropboxServico.ObterInformacaoContaAsync(cancellationToken);
            return Ok(info);
        }


        [HttpPost("EnviarArquivo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EnviarArquivo([FromForm] UploadArquivoRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Arquivo não informado.");

            try
            {
                await _IDropboxServico.EnviarArquivoAsync(request, "Arquivos", cancellationToken);
                return Ok("Arquivo enviado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Mensagem = "Erro ao enviar arquivo",
                    Erro = ex.Message
                });
            }
        }



        [HttpGet("ObterArquivos")]
        public async Task<IActionResult> ObterArquivos(CancellationToken cancellationToken)
        {
            var arquivos = await _IDropboxServico.ObterArquivos("Arquivos", cancellationToken);
            return Ok(arquivos);
        }


    }
}