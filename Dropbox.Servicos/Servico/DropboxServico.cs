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
        //private readonly AppSettingsDto _AppSettingsDto;
        private readonly string _pasta = "/AMAURI_NET";

        private readonly IDropboxConfiguracaoRepositorio _repositorio;

        public DropboxServico(IDropboxConfiguracaoRepositorio repositorio)
        {
            // _AppSettingsDto = appSettingsDto;
            _repositorio = repositorio;

        }

        public Task<string> GerarLinkAutorizacaoAsync(string appKey, string redirectUri, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(appKey))
                throw new ArgumentException("AppKey é obrigatório");

            if (string.IsNullOrWhiteSpace(redirectUri))
                throw new ArgumentException("RedirectUri é obrigatório");

            string url = "https://www.dropbox.com/oauth2/authorize" +
                         $"?client_id={appKey}" +
                         "&response_type=code" +
                         "&token_access_type=offline" +
                         $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            return Task.FromResult(url);
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


        }

        public async Task<ArquivoDropboxDto> EnviarArquivoAsync(IFormFile arquivo, CancellationToken cancellationToken)
        {

            if (arquivo == null || arquivo.Length == 0)
            {
                throw new ArgumentException("Arquivo é obrigatório");
            }

            if (arquivo == null)
                throw new ArgumentNullException(nameof(arquivo), "Arquivo não informado");

            if (arquivo.Length == 0)
                throw new ArgumentException("Arquivo vazio");

            if (arquivo.Length > 50 * 1024 * 1024) // 50MB
                throw new ArgumentException("Arquivo excede o limite permitido (50MB)");

            if (string.IsNullOrWhiteSpace(arquivo.FileName))
                throw new ArgumentException("Nome do arquivo inválido");


            string[] extensoesPermitidas = [".jpg", ".png", ".pdf", ".txt"];
            string extensao = Path.GetExtension(arquivo.FileName).ToLower();

            if (!extensoesPermitidas.Contains(extensao))
                throw new ArgumentException($"Tipo de arquivo não permitido: {extensao}");




            using var cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);

            string caminho = $"{_pasta}/{arquivo.FileName}";
            try
            {
                // =========================
                // UPLOAD COM Polly
                // =========================
                var fileMetadata = await PollyInserir().ExecuteAsync(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await using var stream = arquivo.OpenReadStream();
                    return await cliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream);
                });

                if (fileMetadata == null)
                    throw new Exception("Falha ao enviar arquivo para o Dropbox");

                // =========================
                // LINK COMPARTILHADO
                // =========================
                string? url = await ObterOuCriarLinkCompartilhadoAsync(cliente, fileMetadata.PathDisplay);

                if (string.IsNullOrEmpty(url))
                    throw new Exception("Arquivo enviado, mas não foi possível gerar link");

                string preview = AjustarLinkDownload(url, DropboxLinkModo.Preview);
                string download = AjustarLinkDownload(url, DropboxLinkModo.Download);

                // =========================
                // RESULTADO
                // =========================
                return ArquivoDropboxDto.Criar(fileMetadata, preview, download);
            }
            catch (OperationCanceledException)
            {
                throw new TaskCanceledException("Operação cancelada pelo usuário");
            }
            catch (ApiException<UploadError> ex)
            {
                throw new Exception($"Erro no upload Dropbox: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Erro de comunicação com Dropbox", ex);
            }
            catch (Exception)
            {
                throw; // deixa o middleware tratar
            }

        }

        public async Task<IEnumerable<ArquivoDropboxDto>> ObterArquivosAsync(CancellationToken cancellationToken)
        {
            using DropboxClient cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);
            if (cliente == null)
                throw new InvalidOperationException("Falha ao criar cliente do Dropbox");

            try
            {
                // =========================
                // BUSCA ARQUIVOS
                // =========================
                ListFolderResult resultadoDropbox = await cliente.Files.ListFolderAsync(_pasta);


                List<ArquivoDropboxDto> lista = new List<ArquivoDropboxDto>();
                foreach (var arquivo in resultadoDropbox.Entries)
                {
                    if (!arquivo.IsFile)
                        continue;

                    FileMetadata? file = arquivo as FileMetadata;

                    if (file == null)
                        continue; // proteção extra

                    try
                    {
                        // =========================
                        // LINK COMPARTILHADO
                        // =========================
                        string? url = await ObterOuCriarLinkCompartilhadoAsync(cliente, arquivo.PathDisplay);

                        string preview = string.Empty;
                        string download = string.Empty;

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            preview = AjustarLinkDownload(url, DropboxLinkModo.Preview);
                            download = AjustarLinkDownload(url, DropboxLinkModo.Download);
                        }

                        ArquivoDropboxDto entidade = new ArquivoDropboxDto(arquivo.Name, arquivo.PathDisplay, file?.Size, file?.ClientModified, url);
                        lista.Add(entidade);
                    }
                    catch (Exception ex)
                    {
                        // erro genérico por item
                        throw new Exception($"Erro ao processar arquivo {arquivo.Name}", ex);
                    }
                }
                return lista;
            }
            catch (OperationCanceledException)
            {
                throw new TaskCanceledException("Operação cancelada pelo usuário");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Erro de comunicação com o Dropbox", ex);
            }
            catch (Exception)
            {
                throw; // deixa o middleware tratar
            }

        }

        public async Task<ExcluirArquivoResultadoDto> ExcluirArquivoAsync(string nomeArquivo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nomeArquivo))
                throw new ArgumentException("Nome do arquivo é obrigatório");

            using DropboxClient cliente = await ObterCliente(DropboxParametro.OAuth, cancellationToken);

            string caminho = $"{_pasta}/{nomeArquivo}";

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // =========================
                // DELETE COM RETRY (Polly)
                // =========================
                DeleteResult resultado = await PollyDelete().ExecuteAsync(async () =>
                {
                    return await cliente.Files.DeleteV2Async(caminho);
                });

                if (resultado == null)
                    throw new Exception("Falha ao excluir arquivo");

                // =========================
                // SUCESSO
                // =========================
                return ExcluirArquivoResultadoDto.Criar(caminho, true);
            }
            catch (ApiException<DeleteError> ex)
            {
                if (ex.ErrorResponse.IsPathLookup && ex.ErrorResponse.AsPathLookup.Value.IsNotFound)
                {
                    // Arquivo não existe
                    return ExcluirArquivoResultadoDto.Criar(caminho, false);
                }

                throw new Exception($"Erro ao excluir arquivo no Dropbox: {ex.Message}", ex);
            }
            catch (OperationCanceledException)
            {
                throw new TaskCanceledException("Operação cancelada ao excluir arquivo");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Erro de comunicação com Dropbox", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private enum DropboxParametro
        {
            OAuth,
            Token,
            Configuracao
        }

        private async Task<DropboxClient> ObterCliente(DropboxParametro tipo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var dropboxConfiguracao = await _repositorio.ObterConfiguracaoAsync(cancellationToken);

                // =========================
                // VALIDAÇÃO CONFIGURAÇÃO
                // =========================
                if (dropboxConfiguracao == null)
                {
                    throw new InvalidOperationException("Configuração do Dropbox não encontrada");
                }

                if (string.IsNullOrWhiteSpace(dropboxConfiguracao.AppKey))
                    throw new InvalidOperationException("AppKey não configurado");

                if (string.IsNullOrWhiteSpace(dropboxConfiguracao.AppSecret))
                    throw new InvalidOperationException("AppSecret não configurado");

                if (string.IsNullOrWhiteSpace(dropboxConfiguracao.RefreshToken))
                    throw new InvalidOperationException("RefreshToken não configurado");

                // =========================
                // TOKEN VÁLIDO
                // =========================
                if (!string.IsNullOrWhiteSpace(dropboxConfiguracao.AccessToken) && dropboxConfiguracao.ExpiresAt.HasValue && DateTime.UtcNow < dropboxConfiguracao.ExpiresAt.Value.AddMinutes(-2))
                {
                    return new DropboxClient(dropboxConfiguracao.AccessToken.Trim());
                }

                // =========================
                // TOKEN EXPIRADO → REFRESH
                // =========================
                try
                {
                    dropboxConfiguracao = await RefreshTokenAsync(dropboxConfiguracao, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao renovar token do Dropbox", ex);
                }

                if (string.IsNullOrWhiteSpace(dropboxConfiguracao.AccessToken))
                    throw new Exception("Falha ao obter AccessToken após refresh");

                return new DropboxClient(dropboxConfiguracao.AccessToken.Trim());
            }
            catch (OperationCanceledException)
            {
                throw new TaskCanceledException("Operação cancelada ao obter cliente Dropbox");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Erro de comunicação com o Dropbox (token)", ex);
            }
            catch (Exception)
            {
                throw; // middleware trata
            }
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


        private async Task<string?> ObterOuCriarLinkCompartilhadoAsync(DropboxClient cliente, string path)
        {
            ListSharedLinksResult links = await cliente.Sharing.ListSharedLinksAsync(path, directOnly: true);

            SharedLinkMetadata linkExistente = links.Links.FirstOrDefault();
            if (linkExistente != null)
                return linkExistente.Url;

            SharedLinkMetadata novoLink = await cliente.Sharing.CreateSharedLinkWithSettingsAsync(path);
            return novoLink.Url;
        }

        private static string AjustarLinkDownload(string url, DropboxLinkModo modo)
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


        private enum DropboxLinkModo
        {
            Automatico = 0,   // Não altera, só garante que seja válido
            Download = 1,     // Força download (dl=1)
            Streaming = 2,    // Conteúdo bruto (raw=1)
            Preview = 3      // Força visualização (dl=0)
        }


        private AsyncRetryPolicy PollyInserir()
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
                        Console.WriteLine($"[Dropbox Inserir Retry] Tentativa {tentativa} - {ex.Message}");
                    });
            return policy;
        }


        private AsyncRetryPolicy PollyDelete()
        {
            AsyncRetryPolicy policy = Policy
                .Handle<ApiException<DeleteError>>()
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    3,
                    tentativa => TimeSpan.FromSeconds(Math.Pow(2, tentativa)),
                    (ex, tempo, tentativa, context) =>
                    {
                        Console.WriteLine($"[Dropbox Delete Retry] Tentativa {tentativa} - {ex.Message}");
                    });
            return policy;
        }
    }
}
