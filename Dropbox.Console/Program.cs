using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Users;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

DropboxConfiguracaoDto _dropboxConfiguracaoDto = new DropboxConfiguracaoDto();
string _appKey = string.Empty;
string _appSecret = string.Empty;
string _redirectUri = string.Empty;
string _pasta = string.Empty;
string _nomeArquivo = $"{DateTime.Now:ddMMyyyy_HHmmss}";

Alerta("============================");
Alerta("CONFIGURAÇÃO");
Alerta("============================");

try
{
    var caminhoConfig = @"C:\Amauri\GitHub\DROPBOX_CONFIGURACAO.txt";
    if (File.Exists(caminhoConfig))
    {
        var json = await File.ReadAllTextAsync(caminhoConfig);
        _dropboxConfiguracaoDto = JsonSerializer.Deserialize<DropboxConfiguracaoDto>(json);
        if (_dropboxConfiguracaoDto == null)
        {
            Error("Configuração do Dropbox está vazia.");
            return;
        }
    }
    _appKey = _dropboxConfiguracaoDto.AppKey;
    _appSecret = _dropboxConfiguracaoDto.AppSecret;
    _redirectUri = _dropboxConfiguracaoDto.RedirectUri;
    _pasta = _dropboxConfiguracaoDto.Pasta;

    Info($" - AppKey: {_appKey}");
    Info($" - AppSecret: {_appSecret}");
    Info($" - RedirectUri: {_redirectUri}");
    Info($" - Pasta: {_pasta}");
}
catch (Exception ex)
{
    Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    return;
}


try
{
    Info("********************************************************");
    Info("************************* Oath 2.0 *********************");
    Info("********************************************************");
    Info("Iniciando Dropbox...\n");
    DropboxClient dropboxCliente = Inicializacao(TipoToken.OAuth).Result;
    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Oath.txt", "Arquivo enviado via Program.cs");
}
catch (Exception ex)
{
    Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}


try
{
    Info("\n********************************************************");
    Info("********************* Token ******************************");
    Info("**********************************************************");
    Info("Iniciando Dropbox...\n");
    DropboxClient dropboxCliente = Inicializacao(TipoToken.Token).Result;
    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Token.txt", "Arquivo enviado via Program.cs");
}
catch (Exception ex)
{
    Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}

Info("============================");
Info("FIM");
Info("============================");

// ============================
// MÉTODOS
// ============================

async Task<DropboxClient> Inicializacao(TipoToken tipoToken)
{
    //var DROPBOX_ACCESS_TOKEN = File.ReadAllText(TOKEN_PATH_TOKEN).Replace("\\\\", "\\");


    TokenArquivoDto arquivo = await CriarOuCarregarArquivoToken(tipoToken);

    string tokenArquivo = arquivo.AccessToken;
    DateTime? vencimento = arquivo.ExpiresAt;

    Info($" Vencimento: {vencimento}");
    Info($" Token: {tokenArquivo}");
    if (string.IsNullOrWhiteSpace(tokenArquivo))
    {
        Error("Token do Dropbox está vazio.");
    }
    Info("Conectando ao Dropbox...");


    DropboxClient dropboxCliente = new DropboxClient(tokenArquivo);
    try
    {
        FullAccount conta = await dropboxCliente.Users.GetCurrentAccountAsync();
        if (conta == null)
        {
            throw new Exception("Conta do Dropbox está vazio.");
        }

        Alerta("============================");
        Alerta("Informações da conta Dropbox:");
        Alerta("============================");
        Info($" - Nome: {conta.Name.DisplayName}");
        Info($" - Email: {conta.Email}");
        Info($" - AccountId: {conta.AccountId}");
        Info($" - País: {conta.Country}");
        Info($" - Tipo Basico: {conta.AccountType.IsBasic}");
        Info($" - Tipo Comercial: {conta.AccountType.IsBusiness}");
        Info($" - Tipo Profissional: {conta.AccountType.IsPro}");
        Info("Informações técnicas do cliente:");
        Info($" - Classe: {conta.GetType().Name}");
        Info($" - Namespace: {conta.GetType().Namespace}");


    }
    catch (Exception)
    {
        Error("Conta não reconhecida ou sem acesso no Dropbox...");
        throw;
    }

    return dropboxCliente;
}



async Task<TokenArquivoDto> CriarOuCarregarArquivoToken(TipoToken tipoToken)
{
    Alerta("============================");
    Alerta("CriarOuCarregarArquivoToken");
    Alerta("============================");

    string caminhoToken = string.Empty;
    TokenArquivoDto tokenArquivoDto = new TokenArquivoDto();
    switch (tipoToken)
    {
        case TipoToken.OAuth:
            caminhoToken = @"C:\Amauri\GitHub\DROPBOX_ACCESS_OAUTH.txt";
            break;
        case TipoToken.Token:
            caminhoToken = @"C:\Amauri\GitHub\DROPBOX_ACCESS_TOKEN.txt";
            break;
        default:
            break;
    }

    if (tipoToken == TipoToken.OAuth)
    {
        if (File.Exists(caminhoToken))
        {
            var json = await File.ReadAllTextAsync(caminhoToken);
            tokenArquivoDto = JsonSerializer.Deserialize<TokenArquivoDto>(json);

            if (tokenArquivoDto != null && !tokenArquivoDto.IsAccessTokenExpired())
                return tokenArquivoDto;
            else
            {
                Info("Renovando Token...");
                return await RefreshTokenOauthAsync(tokenArquivoDto, caminhoToken);
            }
        }
        else
        {
            Info("Gerando arquivo...");
            return await CriarArquivoOauthAsync(caminhoToken);
        }
    }
    else
    {
        if (File.Exists(caminhoToken))
        {
            var json = File.ReadAllText(caminhoToken).Replace("\\\\", "\\");
            tokenArquivoDto = JsonSerializer.Deserialize<TokenArquivoDto>(json);
        }
        else
        {
            tokenArquivoDto = new TokenArquivoDto
            {
                AccessToken = "",
                RefreshToken = "",
                ExpiresAt = null
            };
            await File.WriteAllTextAsync(caminhoToken, JsonSerializer.Serialize(tokenArquivoDto, new JsonSerializerOptions { WriteIndented = true }));

        }
    }
    return tokenArquivoDto;
}

 


async Task EnviarArquivoAsync(DropboxClient dropboxCliente, string pasta, string nomeArquivo, string conteudo)
{
    Alerta("============================");
    Alerta("EnviarArquivoAsync");
    Alerta("============================");
    string caminho = $"{pasta}/{nomeArquivo}";
    Info($"Caminho: {caminho}");
    using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(conteudo));
    try
    {
        Alerta("============================");
        Alerta("ENVIAR ARQUIVO ");
        Alerta("============================");
        await dropboxCliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream);
        Info($"Arquivo enviado: {nomeArquivo}");


        Alerta("============================");
        Alerta("LISTAR ARQUIVOS");
        Alerta("============================");
        var arquivos = await dropboxCliente.Files.ListFolderAsync(pasta);
        Info($"Total de arquivos: {arquivos.Entries.Count}");
        foreach (var item in arquivos.Entries)
        {
            Info($"{item.Name} - {item.GetType().Name}");
        }
    }
    catch (ApiException<UploadError> ex)
    {
        Info($"Erro Dropbox: {ex.ErrorResponse}");
        if (ex.ErrorResponse.IsPath)
            Info($"Path error: {ex.ErrorResponse.AsPath}");

        throw;
    }
}

async Task<TokenArquivoDto> RefreshTokenOauthAsync(TokenArquivoDto tokenArquivoDto, string tOKEN_PATH_OAUTH)
{
    Alerta("============================");
    Alerta("RefreskTokenOauthAsync");
    Alerta("============================");

    Error($"Vencimento: {tokenArquivoDto.ExpiresAt}");

    var request = new Dictionary<string, string>
    {
        ["grant_type"] = "refresh_token",
        ["refresh_token"] = tokenArquivoDto.RefreshToken,
        ["client_id"] = _appKey,
        ["client_secret"] = _appSecret
    };

    using var client = new HttpClient();
    var response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", new FormUrlEncodedContent(request));

    response.EnsureSuccessStatusCode();

    string json = await response.Content.ReadAsStringAsync();
    TokenResponse token = JsonSerializer.Deserialize<TokenResponse>(json);

    TokenArquivoDto novo = new TokenArquivoDto
    {
        AccessToken = token.AccessToken,
        RefreshToken = tokenArquivoDto.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
    };

    await File.WriteAllTextAsync(tOKEN_PATH_OAUTH, JsonSerializer.Serialize(novo, new JsonSerializerOptions { WriteIndented = true }));

    return novo;
}



// ============================
// OAUTH DO DROPBOX
// ============================

async Task<TokenArquivoDto> CriarArquivoOauthAsync(string tOKEN_PATH_OAUTH)
{
    Alerta("============================");
    Alerta("UrlGeradorDeCodigo");
    Alerta("============================");

    Sucesso("\nAbra a URL abaixo no navegador:");

    string site = "https://www.dropbox.com/oauth2/authorize" +
            $"?client_id={_appKey}" +
            "&response_type=code" +
            "&token_access_type=offline" +
            $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";

    Sucesso(site);

    Sucesso("\nCole o code:");
    var code = Console.ReadLine();
    Sucesso($"Cole o code: {code}");


    if (string.IsNullOrWhiteSpace(code))
        throw new Exception("Code inválido");


    using var client = new HttpClient();
    FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string,string>("grant_type", "authorization_code"),
        new KeyValuePair<string,string>("code", code),
        new KeyValuePair<string,string>("client_id", _appKey),
        new KeyValuePair<string,string>("client_secret", _appSecret),
        new KeyValuePair<string,string>("redirect_uri", _redirectUri)
    });
    var response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", content);
    var json = await response.Content.ReadAsStringAsync();
    TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json)!;



    TokenArquivoDto tokenArquivoDto = new TokenArquivoDto
    {
        AccessToken = tokenResponse.AccessToken,
        RefreshToken = tokenResponse.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
    };

    await File.WriteAllTextAsync(tOKEN_PATH_OAUTH, JsonSerializer.Serialize(tokenArquivoDto, new JsonSerializerOptions { WriteIndented = true }));


    return tokenArquivoDto;

}




void Error(string menssagem)
{
    Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.White, ConsoleColor.Red);
}


void Sucesso(string menssagem)
{
    Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Black, ConsoleColor.Green);
}


void Alerta(string menssagem)
{
    Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Black, ConsoleColor.Yellow);
}

void Info(string menssagem)
{
    Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Yellow, ConsoleColor.Blue);
}

void Padrao(string menssagem, ConsoleColor foreground, ConsoleColor background)
{
    Console.BackgroundColor = background;
    Console.ForegroundColor = foreground;
    Console.WriteLine(menssagem);
    Console.ResetColor(); // Restaura as cores padrão após a mensagem.
}


// ============================
// MODELOS
// ============================

class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

class TokenArquivoDto
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime? ExpiresAt { get; set; }

    public bool IsAccessTokenExpired()
    {
        if (!ExpiresAt.HasValue)
            return false; // sem data definida, assume que não expirou

        return DateTime.UtcNow >= ExpiresAt.Value.AddMinutes(-2);
    }
}


public class DropboxConfiguracaoDto
{
    public string AppKey { get; set; } = "";
    public string AppSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string Pasta { get; set; } = "";
    public string NomeArquivo { get; set; } = "";
}



public enum TipoToken
{
    OAuth = 0,
    Token = 1
}
 