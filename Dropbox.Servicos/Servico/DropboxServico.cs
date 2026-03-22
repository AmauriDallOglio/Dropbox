using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Dominio.Entidade;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Dropbox.Servicos.Servico
{
    public class DropboxServico : IDropboxServico
    {
        private readonly AppSettingsDto _AppSettingsDto;

        private readonly IDropboxConfiguracaoRepositorio _repositorio;

        public DropboxServico(IOptions<AppSettingsDto> appSettingsDto, IDropboxConfiguracaoRepositorio repositorio)
        {
            _AppSettingsDto = appSettingsDto.Value;
            _repositorio = repositorio;
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
                        throw new InvalidOperationException("Arquivo OAuth, erro na conversão");

                    var oAuth = (T)(object)LerJsonArquivo<DropboxTokenDto>(conteudo);
                    return oAuth;

                case DropboxParametro.Token:
                    if (typeof(T) != typeof(DropboxTokenDto))
                        throw new InvalidOperationException("Arquivo Token, erro na conversão");

                    var token = (T)(object)LerJsonArquivo<DropboxTokenDto>(conteudo);
                    return token;

                case DropboxParametro.Configuracao:
                    if (typeof(T) != typeof(DropboxConfiguracaoDto))
                        throw new InvalidOperationException("Arquivo Configuracao, erro na conversão");

                    var config = (T)(object)LerJsonArquivo<DropboxConfiguracaoDto>(conteudo);
                    return config;

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
                    return _AppSettingsDto.ArquivoConfiguracao.OAuth ?? throw new FileNotFoundException("OAuth não configurado");

                case DropboxParametro.Token:
                    return _AppSettingsDto.ArquivoConfiguracao.Token ?? throw new FileNotFoundException("Token não configurado");

                case DropboxParametro.Configuracao:
                    return _AppSettingsDto.ArquivoConfiguracao.Configurcao ?? throw new FileNotFoundException("Configuração não configurada");

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


        private async Task<DropboxClient> ObterCliente(DropboxParametro tipo, CancellationToken cancellationToken )
        {
            ////Carregada do arquivo
            //DropboxTokenDto tokenDto = ObterDadosConfiguracao<DropboxTokenDto>(tipo);

            //if (string.IsNullOrWhiteSpace(tokenDto.AccessToken))
            //    throw new Exception("AccessToken inválido.");


            ////Carrega do banco de dados
            DropboxConfiguracao tokenDto = await _repositorio.ObterConfiguracaoAsync(cancellationToken);

            if (tokenDto == null)
                throw new Exception("Configuração do Dropbox não encontrada.");


            var config = await _repositorio.ObterConfiguracaoAsync(cancellationToken);

            if (config == null)
                throw new Exception("Configuração não encontrada.");

            // Token válido
            if (config.ExpiresAt.HasValue && DateTime.UtcNow < config.ExpiresAt.Value.AddMinutes(-2))
                return new DropboxClient(tokenDto.AccessToken.Trim());

            // Token expirado → renovar
            Console.WriteLine("Token expirado. Renovando...");

            tokenDto = await RefreshTokenAsync(config, cancellationToken);





            return new DropboxClient(tokenDto.AccessToken.Trim());
        }



        private async Task<DropboxConfiguracao> RefreshTokenAsync(DropboxConfiguracao dropboxConfiguracao, CancellationToken cancellationToken)
        {
            Dictionary<string, string> request = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = dropboxConfiguracao.RefreshToken,
                ["client_id"] = dropboxConfiguracao.AppKey,
                ["client_secret"] = dropboxConfiguracao.AppSecret
            };

            using var http = new HttpClient();

            //OAuth 2.0 com refresh automático
            var response = await http.PostAsync("https://api.dropboxapi.com/oauth2/token", new FormUrlEncodedContent(request),  cancellationToken);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync(cancellationToken);


            TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json) 
                ?? throw new Exception("Erro ao renovar token");


            //OAuth 2.0 com refresh automático persistido em banco
            dropboxConfiguracao.AccessToken = tokenResponse.AccessToken;
            dropboxConfiguracao.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            await _repositorio.AtualizarAsync(dropboxConfiguracao, cancellationToken);

            Console.WriteLine("Token atualizado no banco");

            return dropboxConfiguracao;
        }




        public async Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken)
        {
            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);



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

            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);

            string caminho = $"{_AppSettingsDto.ArquivoConfiguracao.PastaBase}/{subFolder}/{request.File.FileName}";

            await using var stream = request.File.OpenReadStream();

            return await cliente.Files.UploadAsync(
                caminho,
                WriteMode.Overwrite.Instance,
                body: stream);
        }



        public async Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken)
        {
            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);

            string caminho = $"{_AppSettingsDto.ArquivoConfiguracao.PastaBase}/{subFolder}";

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
