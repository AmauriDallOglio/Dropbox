using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Text.Json;


namespace Dropbox.Servicos.Servico
{
    public class DropboxServico : IDropboxServico
    {
        private readonly AppSettingsDto _AppSettingsDto;

        public DropboxServico(IOptions<AppSettingsDto> appSettingsDto)
        {
            _AppSettingsDto = appSettingsDto.Value;
        }


        private T ObterDadosConfiguracao<T>(DropboxParametro parametro)
        {
            var caminho = ObterCaminhoParametro(parametro);

            if (!File.Exists(caminho))
                throw new FileNotFoundException($"Arquivo não encontrado: {caminho}");

            string conteudo = File.ReadAllText(caminho);

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new Exception($"Conteúdo do arquivo ({parametro}) está vazio.");

            switch (parametro)
            {
                case DropboxParametro.OAuth:
                    if (typeof(T) != typeof(DropboxTokenDto))
                        throw new InvalidOperationException("Tipo esperado: DropboxTokenDto");

                    var aaaaa = (T)(object)LerJsonArquivo<DropboxTokenDto>(conteudo);
                    return aaaaa;

                case DropboxParametro.Token:
                    if (typeof(T) != typeof(DropboxTokenDto))
                        throw new InvalidOperationException("Tipo esperado: DropboxTokenDto");

                    var aaaa = (T)(object)LerJsonArquivo<DropboxTokenDto>(conteudo);
                    return aaaa;

                case DropboxParametro.Configuracao:
                    if (typeof(T) != typeof(DropboxConfiguracaoDto))
                        throw new InvalidOperationException("Tipo esperado: DropboxConfiguracaoDto");

                    return (T)(object)LerJsonArquivo<DropboxConfiguracaoDto>(conteudo);

                default:
                    throw new ArgumentOutOfRangeException(nameof(parametro), parametro, "Parâmetro inválido");
            }
        }


        private T LerJsonArquivo<T>(string conteudo)
        {
            try
            {
                var obj = JsonSerializer.Deserialize<T>(conteudo,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (obj == null)
                    throw new InvalidOperationException($"Erro ao desserializar {typeof(T).Name}");

                return obj;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"JSON inválido para {typeof(T).Name}", ex);
            }
        }


        private string ObterCaminhoParametro(DropboxParametro parametro)
        {
            switch (parametro)
            {
                case DropboxParametro.OAuth:
                    return _AppSettingsDto.OAuth ?? throw new FileNotFoundException("OAuth não configurado");

                case DropboxParametro.Token:
                    return _AppSettingsDto.Token ?? throw new FileNotFoundException("Token não configurado");

                case DropboxParametro.Configuracao:
                    return _AppSettingsDto.Configuracao ?? throw new FileNotFoundException("Configuração não configurada");

                default:
                    throw new ArgumentOutOfRangeException(nameof(parametro), parametro, "Parâmetro inválido");
            }
        }

        public enum DropboxParametro
        {
            OAuth,
            Token,
            Configuracao
        }


        private DropboxClient ObterCliente()
        {
            var tokenDto = ObterDadosConfiguracao<DropboxTokenDto>(DropboxParametro.OAuth);

            if (string.IsNullOrWhiteSpace(tokenDto.AccessToken))
                throw new Exception("AccessToken inválido.");

            return new DropboxClient(tokenDto.AccessToken.Trim());
        }

        public async Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken)
        {
            using var cliente = ObterCliente();

            var conta = await cliente.Users.GetCurrentAccountAsync();
            cancellationToken.ThrowIfCancellationRequested();
            return new InformacaoContaDto
            {
                Nome = conta.Name.DisplayName,
                Email = conta.Email,
                Pais = conta.Country,
                AccountId = conta.AccountId,
                TipoBasico = conta.AccountType.IsBasic,
                TipoBusiness = conta.AccountType.IsBusiness,
                TipoPro = conta.AccountType.IsPro
            };
        }


        public async Task<FileMetadata> EnviarArquivoAsync(UploadArquivoRequest request, string subFolder, CancellationToken cancellationToken)
        {
            if (request == null || request.File.Length == 0)
                throw new ArgumentException("Arquivo inválido.");

            using var cliente = ObterCliente();

            string caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}/{request.File.FileName}";

            await using var stream = request.File.OpenReadStream();

            return await cliente.Files.UploadAsync(
                caminho,
                WriteMode.Overwrite.Instance,
                body: stream);
        }



        public async Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken)
        {
            using var cliente = ObterCliente();

            string caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}";

            var resultadoDropbox = await cliente.Files.ListFolderAsync(caminho);

            var resultado = new List<object>();

            foreach (var e in resultadoDropbox.Entries)
            {
                string preview = string.Empty;
                string download = string.Empty;

                if (e.IsFile)
                {
                    string? url = await ObterOuCriarLinkCompartilhadoAsync(cliente, e.PathDisplay);

                    if (!string.IsNullOrEmpty(url))
                    {
                        preview = AjustarLinkDownload(url, DropboxLinkModo.Preview);
                        download = AjustarLinkDownload(url, DropboxLinkModo.Download);
                    }
                }

                var file = e as FileMetadata;

                resultado.Add(new
                {
                    e.Name,
                    e.PathDisplay,
                    e.PathLower,
                    Tipo = e.GetType().Name,
                    Tamanho = file?.Size,
                    DataModificacao = file?.ClientModified,
                    Rev = file?.Rev,
                    LinkCompartilhadoPreview = preview,
                    LinkCompartilhadoDownload = download
                });
            }

            return resultado;
        }



        private async Task<string?> ObterOuCriarLinkCompartilhadoAsync(DropboxClient cliente, string path)
        {
            var links = await cliente.Sharing.ListSharedLinksAsync( path,  directOnly: true);

            var linkExistente = links.Links.FirstOrDefault();
            if (linkExistente != null)
                return linkExistente.Url;

            var novoLink = await cliente.Sharing.CreateSharedLinkWithSettingsAsync(path);
            return novoLink.Url;
        }

        public static string AjustarLinkDownload(string url, DropboxLinkModo modo)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            // Remove parâmetros conflitantes
            //url = url.Trim().Trim('"');
            url = Regex.Replace(url, @"([&?])(dl|raw)=\d", string.Empty);

            // Define o parâmetro conforme o modo escolhido
            string parametro = string.Empty;
            switch (modo)
            {
                case DropboxLinkModo.Download:
                    parametro = "dl=1";
                    break;

                case DropboxLinkModo.Streaming:
                    parametro = "raw=1";
                    break;

                case DropboxLinkModo.Preview:
                    parametro = "dl=0";
                    break;

                default:
                    break;
            }


            // Se for automático, retorna o link limpo
            if (parametro == null)
                return url;

            // Adiciona o parâmetro corretamente
            return url.Contains("?") ? $"{url}&{parametro}" : $"{url}?{parametro}";
        }


        public enum DropboxLinkModo
        {
            Automatico = 0,   // Não altera, só garante que seja válido
            Download = 1,     // Força download (dl=1)
            Streaming = 2,    // Conteúdo bruto (raw=1)
            Preview = 3      // Força visualização (dl=0)
        }


    }
}
