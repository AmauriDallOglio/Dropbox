using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Dropbox.Api.Users;
using Dropbox.Dominio.Entidade;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Servicos.Dto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.AspNetCore.Http;
using Polly;
using Polly.Retry;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Dropbox.Servicos.Servico
{
    public class DropboxServico : IDropboxServico
    {
        private readonly AppSettingsDto _AppSettingsDto;

        private readonly IDropboxConfiguracaoRepositorio _repositorio;

        public DropboxServico(AppSettingsDto appSettingsDto, IDropboxConfiguracaoRepositorio repositorio)
        {
            _AppSettingsDto = appSettingsDto;
            _repositorio = repositorio;
        }


        public T ObterDadosConfiguracao<T>(DropboxParametro parametro)
        {
            string caminho = ObterCaminhoParametro(parametro);

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


        private async Task<DropboxClient> ObterCliente(DropboxParametro tipo, CancellationToken cancellationToken)
        {
            ////Carregada do arquivo
            //DropboxTokenDto tokenDto = ObterDadosConfiguracao<DropboxTokenDto>(tipo);

            //if (string.IsNullOrWhiteSpace(tokenDto.AccessToken))
            //    throw new Exception("AccessToken inválido.");

            ////Carrega do banco de dados
            DropboxConfiguracao? dropboxConfiguracao = await _repositorio.ObterConfiguracaoAsync(cancellationToken);
            if (dropboxConfiguracao is null)
            {
                //throw new Exception("Configuração do Dropbox não encontrada.");
                Console.WriteLine("Configuração não encontrada. Criando automaticamente...");
                DropboxConfiguracaoDto dto = ObterDadosConfiguracao<DropboxConfiguracaoDto>(DropboxParametro.Configuracao);
                dropboxConfiguracao.AtualizarConfiguracao(dto.AppKey, dto.AppSecret, dto.RedirectUri, dto.Pasta, dto.NomeArquivo);
                dropboxConfiguracao = await RefreshTokenAsync(dropboxConfiguracao, cancellationToken);
            }
            else
            {
                // Token válido
                if (dropboxConfiguracao.ExpiresAt.HasValue && DateTime.UtcNow < dropboxConfiguracao.ExpiresAt.Value.AddMinutes(-2))
                    return new DropboxClient(dropboxConfiguracao.AccessToken.Trim());

                // Token expirado → renovar
                Console.WriteLine("Token expirado. Renovando...");
                dropboxConfiguracao = await RefreshTokenAsync(dropboxConfiguracao, cancellationToken);
            }
            return new DropboxClient(dropboxConfiguracao.AccessToken.Trim());
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
            HttpResponseMessage response = await http.PostAsync("https://api.dropboxapi.com/oauth2/token", new FormUrlEncodedContent(request), cancellationToken);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync(cancellationToken);


            TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json)
                ?? throw new Exception("Erro ao renovar token");


            //OAuth 2.0 com refresh automático persistido em banco
            dropboxConfiguracao.AtualizarRefreshAutomatico(tokenResponse.AccessToken, DateTime.Now.AddSeconds(tokenResponse.ExpiresIn));

            await _repositorio.EditarAsync(dropboxConfiguracao, cancellationToken);

            Console.WriteLine("Token atualizado no banco");

            return dropboxConfiguracao;
        }




        public async Task<ContaDropboxDto> ObterInformacaoContaAsync(CancellationToken cancellationToken)
        {
            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);
            if (cliente == null)
            {
                throw new Exception("Conta não localizada");
            }

            FullAccount conta = await cliente.Users.GetCurrentAccountAsync();
            if (conta == null)
            {
                throw new Exception("Dados da conta não localizados");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new ContaDropboxDto(conta.Name.DisplayName, conta.Email, conta.Country, conta.AccountId, conta.AccountType.IsBasic, conta.AccountType.IsBusiness, conta.AccountType.IsPro);

            //return new InformacaoContaDto
            //{
            //    Nome = conta.Name.DisplayName,
            //    Email = conta.Email,
            //    Pais = conta.Country,
            //    AccountId = conta.AccountId,
            //    TipoBasico = conta.AccountType.IsBasic,
            //    TipoBusiness = conta.AccountType.IsBusiness,
            //    TipoPro = conta.AccountType.IsPro
            //};
        }


        public async Task<FileMetadata> EnviarArquivoAsync(IFormFile? file, string subFolder, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Arquivo ");

            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);
            string caminho = $"{_AppSettingsDto.ArquivoConfiguracao.PastaBase}/{subFolder}/{file.FileName}";

            await using var stream = file.OpenReadStream();
            AsyncRetryPolicy policy = Polly();
            return await policy.ExecuteAsync(async () =>
            {
                return await cliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream);
            });
        }

        private AsyncRetryPolicy Polly()
        {
            AsyncRetryPolicy policy = Policy
                .Handle<ApiException<UploadError>>() // erro Dropbox
                .Or<HttpRequestException>()          // erro rede
                .Or<TaskCanceledException>()         // timeout
                .WaitAndRetryAsync(
                    3,
                    tentativa => TimeSpan.FromSeconds(Math.Pow(2, tentativa)),
                    (ex, tempo, tentativa, context) =>
                    {
                        Console.WriteLine($"[Dropbox Retry] Tentativa {tentativa} - {ex.Message}");
                    });
            return policy;
        }

        //public async Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken)
        //{
        //    using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);
        //    string caminho = $"{_AppSettingsDto.ArquivoConfiguracao.PastaBase}/{subFolder}";
        //    ListFolderResult resultadoDropbox = await cliente.Files.ListFolderAsync(caminho);
        //    var resultado = new List<object>();
        //    foreach (var e in resultadoDropbox.Entries)
        //    {
        //        string preview = string.Empty;
        //        string download = string.Empty;
        //        if (e.IsFile)
        //        {
        //            string? url = await ObterOuCriarLinkCompartilhadoAsync(cliente, e.PathDisplay);

        //            if (!string.IsNullOrEmpty(url))
        //            {
        //                preview = AjustarLinkDownload(url, DropboxLinkModo.Preview);
        //                download = AjustarLinkDownload(url, DropboxLinkModo.Download);
        //            }
        //        }

        //        FileMetadata file = e as FileMetadata;
        //        resultado.Add(new
        //        {
        //            e.Name,
        //            e.PathDisplay,
        //            e.PathLower,
        //            Tipo = e.GetType().Name,
        //            Tamanho = file?.Size,
        //            DataModificacao = file?.ClientModified,
        //            Rev = file?.Rev,
        //            LinkCompartilhadoPreview = preview,
        //            LinkCompartilhadoDownload = download
        //        });
        //    }

        //    return resultado;
        //}


        public async Task<IEnumerable<ArquivoDropboxDto>> ObterArquivosAsync(string subFolder, CancellationToken cancellationToken)
        {
            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);
            string caminho = $"{_AppSettingsDto.ArquivoConfiguracao.PastaBase}/{subFolder}";
            var resultadoDropbox = await cliente.Files.ListFolderAsync(caminho);
            var lista = new List<ArquivoDropboxDto>();
            foreach (var e in resultadoDropbox.Entries)
            {
                if (!e.IsFile)
                    continue;

                var file = e as FileMetadata;

                string? url = await ObterOuCriarLinkCompartilhadoAsync(cliente, e.PathDisplay);

                string preview = string.Empty;
                string download = string.Empty;

                if (!string.IsNullOrEmpty(url))
                {
                    preview = AjustarLinkDownload(url, DropboxLinkModo.Preview);
                    download = AjustarLinkDownload(url, DropboxLinkModo.Download);
                }

                //lista.Add(new ArquivoDropboxDto
                //{
                //    Nome = e.Name,
                //    Caminho = e.PathDisplay,
                //    Tamanho = file?.Size,
                //    DataModificacao = file?.ClientModified,
                //    LinkPreview = preview,
                //    LinkDownload = download
                //});
                var entidade = new ArquivoDropboxDto(e.Name, e.PathDisplay, file?.Size, file?.ClientModified, url);
                lista.Add(entidade);
            }
            return lista;
        }





        private async Task<string?> ObterOuCriarLinkCompartilhadoAsync(DropboxClient cliente, string path)
        {
            ListSharedLinksResult links = await cliente.Sharing.ListSharedLinksAsync(path, directOnly: true);

            SharedLinkMetadata linkExistente = links.Links.FirstOrDefault();
            if (linkExistente != null)
                return linkExistente.Url;

            SharedLinkMetadata novoLink = await cliente.Sharing.CreateSharedLinkWithSettingsAsync(path);
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
