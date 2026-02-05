using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Mvc;
using static Dropbox.Api.TeamLog.AdminAlertSeverityEnum;

namespace Dropbox.WebApi.Controllers
{
    [ApiController]
    [Route("api/dropbox")]
    public class DropboxController : ControllerBase
    {
        private readonly IDropboxServico _IDropboxServico;
        //https://www.dropbox.com/developers/apps

        public DropboxController(IDropboxServico dropboxServico)
        {
            _IDropboxServico = dropboxServico;
        }

        [HttpGet("DadosConta")]
        public async Task<IActionResult> DadosConta(CancellationToken cancellationToken)
        {
            InformacaoContaDto? info = await _IDropboxServico.ObterInformacaoContaAsync(cancellationToken);
            ResultadoOperacao<InformacaoContaDto> resultado = new ResultadoOperacao<InformacaoContaDto>
            {
                Sucesso = true,
                Mensagem = "Informações da conta obtidas com sucesso.",
                Resultado = info
            };
            return Ok(resultado);
        }


        [HttpPost("EnviarArquivo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EnviarArquivo([FromForm] UploadArquivoRequest request, CancellationToken cancellationToken)
        {
            if (request.File == null)
            {
                ResultadoOperacao<object> erro = await ResultadoOperacao<object>.ErroAsync(
                    null,
                    "Não foi possível enviar o arquivo.", 
                    string.Empty, 
                    500);

                return BadRequest(erro);
            }

            try
            {
                ResultadoOperacao<object> sucesso = await ResultadoOperacao<object>.SucessoAsync(
                    data: null,
                    mensagem: "Arquivo enviado com sucesso.",
                    detalhes: string.Empty,
                    statusCode: StatusCodes.Status200OK);

                return Ok(sucesso);
            }
            catch (Exception ex)
            {
                ResultadoOperacao<object> erro = await ResultadoOperacao<object>.ErroAsync(
                    null, 
                    "Erro ao enviar arquivo.", 
                    ex.Message, 
                    500);

                return BadRequest(erro);
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