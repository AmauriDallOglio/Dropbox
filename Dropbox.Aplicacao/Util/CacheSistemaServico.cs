using Dropbox.Aplicacao.Dto;
using Microsoft.Extensions.Logging;

namespace Dropbox.Aplicacao.Util
{
    public class CacheSistemaServico : ICacheSistemaServico
    {
        private readonly CacheSistemaDto _CacheSistemaDto;
        private readonly ILogger<CacheSistemaServico> _logger;
        private static readonly string _caminhoLog = @"C:\Logs\Dropbox";

        public CacheSistemaServico(CacheSistemaDto cacheSistemaDto, ILogger<CacheSistemaServico> logger)
        {
            _CacheSistemaDto = cacheSistemaDto;
            _logger = logger;

            if (!Directory.Exists(_caminhoLog))
                Directory.CreateDirectory(_caminhoLog);

        }

        public void RegistrarErro(Exception ex, string path, string mensagem)
        {
            // LOG estruturado
            _logger.LogError(null, "LogSistemaService: Erro na aplicação. Path: {Path} Mensagem: {Mensagem}", path, mensagem);

            //Gera o registro em cache
            CacheErroDto erro = CacheErroDto.Criar(ex, path, mensagem);
            _CacheSistemaDto.Erros.Add(erro);
            if (_CacheSistemaDto.Erros.Count > 1000)
                _CacheSistemaDto.Erros.RemoveAt(0);

            //Gera linha no logger manual 
            PrintaConsole.Error($"LogSistemaService: Erro na aplicação. Path: {path} Mensagem: {mensagem}");
            ArquivoLogIncluirLinha(ex, path, mensagem);
        }

        public void RegistrarAcesso(string path, string metodo, int statusCode)
        {
            _logger.LogInformation("Acesso: {Metodo} {Path} Status: {StatusCode}", metodo, path, statusCode);

            CacheAcessoDto acesso = CacheAcessoDto.Criar(path, metodo, statusCode);
            _CacheSistemaDto.Acessos.Add(acesso);

            if (_CacheSistemaDto.Acessos.Count > 1000)
                _CacheSistemaDto.Acessos.RemoveAt(0);
        }


        public static string ArquivoLogIncluirLinha(Exception ex, string requestPath, string mensagemBasica)
        {

            // Nome do arquivo com base na data
            string nomeArquivo = Path.Combine(_caminhoLog, $"error_log_{DateTime.Now:dd-MM-yyyy}.txt");

            string separador = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} -----------------------------------------------------------------------------{Environment.NewLine}";
            string mensagemRetorno = $"TratamentoErroMiddleware | Path: {requestPath} | {mensagemBasica}: {ex.Message}{Environment.NewLine}";

            var mensagemPersonalizada = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensagemRetorno}{Environment.NewLine}";

            File.AppendAllText(nomeArquivo, separador);
            File.AppendAllText(nomeArquivo, mensagemPersonalizada);

            Console.WriteLine($"Erro: {mensagemPersonalizada}");

            PrintaConsole.Error(mensagemPersonalizada);
            return mensagemPersonalizada;
        }


    }
}
