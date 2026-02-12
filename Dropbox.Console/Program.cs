using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Users;
using Dropbox.Aplicacao.Util;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


// ============================
// CONFIGURAÇÃO
// ============================
string _appKey = "l2wdyrt2f613dty";
string _appSecret = "5zhyyqxacdq11di";
string _redirectUri = "http://localhost";
string _pasta = "/AMAURI_NET/Arquivos";
string _nomeArquivo = $"{DateTime.Now:ddMMyyyy_HHmmss}";
ArquivoLog _arquivoLog = new ArquivoLog();   

try
{
    _arquivoLog.Info("********************************************************");
    _arquivoLog.Info("************************* Oath 2.0 *********************");
    _arquivoLog.Info("********************************************************");
    _arquivoLog.Info("Iniciando Dropbox...\n");



    DropboxClient dropboxCliente = Inicializacao(TipoToken.OAuth).Result;
    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Oath.txt", "Arquivo enviado via Program.cs");
}
catch (Exception ex)
{
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    _arquivoLog.Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}


try
{
    _arquivoLog.Info("\n********************************************************");
    _arquivoLog.Info("********************* Token ******************************");
    _arquivoLog.Info("**********************************************************");
    _arquivoLog.Info("Iniciando Dropbox...\n");

    DropboxClient dropboxCliente = Inicializacao(TipoToken.OAuth).Result;
    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Token.txt", "Arquivo enviado via Program.cs");
}
catch (Exception ex)
{
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    _arquivoLog.Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}

_arquivoLog.Info("============================");
_arquivoLog.Info("FIM");
_arquivoLog.Info("============================");


// ============================
// MÉTODOS
// ============================


async Task<DropboxClient> Inicializacao(TipoToken tipoToken)
{
    //var DROPBOX_ACCESS_TOKEN = File.ReadAllText(TOKEN_PATH_TOKEN).Replace("\\\\", "\\");



    TokenArquivoDto arquivo = await LerOuCriarTokenAsync(tipoToken);

    string tokenArquivo = arquivo.AccessToken;
    DateTime? vencimento = arquivo.ExpiresAt;

    _arquivoLog.Padrao($" Vencimento: {vencimento}");
    _arquivoLog.Padrao($"\n Token: {tokenArquivo}");
    if (string.IsNullOrWhiteSpace(tokenArquivo))
    {
        _arquivoLog.Error("Token do Dropbox está vazio.");
    }
    _arquivoLog.Padrao("\n Conectando ao Dropbox...");


    DropboxClient dropboxCliente = new DropboxClient(tokenArquivo);
    FullAccount conta = await dropboxCliente.Users.GetCurrentAccountAsync();
    if (conta == null)
    {
        //throw new Exception("Conta do Dropbox está vazio.");
    }

    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("Informações da conta Dropbox:");
    _arquivoLog.Alerta("============================");
    _arquivoLog.Padrao($" - Nome: {conta.Name.DisplayName}");
    _arquivoLog.Padrao($" - Email: {conta.Email}");
    _arquivoLog.Padrao($" - AccountId: {conta.AccountId}");
    _arquivoLog.Padrao($" - País: {conta.Country}");
    _arquivoLog.Padrao($" - Tipo Basico: {conta.AccountType.IsBasic}");
    _arquivoLog.Padrao($" - Tipo Comercial: {conta.AccountType.IsBusiness}");
    _arquivoLog.Padrao($" - Tipo Profissional: {conta.AccountType.IsPro}");
    _arquivoLog.Padrao("Informações técnicas do cliente:");
    _arquivoLog.Padrao($" - Classe: {conta.GetType().Name}");
    _arquivoLog.Padrao($" - Namespace: {conta.GetType().Namespace}");



    return dropboxCliente;
}



async Task<TokenArquivoDto> LerOuCriarTokenAsync(TipoToken tipoToken)
{

    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("LerOuCriarTokenAsync");
    _arquivoLog.Alerta("============================");

    string caminhoToken = string.Empty;
    TokenArquivoDto tokenArquivoDto = new TokenArquivoDto();
    switch (tipoToken)
    {
        case TipoToken.OAuth:
            caminhoToken = @"C:\Amauri\GitHub\DROPBOX_ACCESS_OAuth.txt";
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
                _arquivoLog.Padrao("Renovando Token...");
                return await RefreskTokenOauthAsync(tokenArquivoDto, caminhoToken);
            }
        }
        else
        {
            _arquivoLog.Padrao("Gerando arquivo...");
            return await CriarArquivoOauthAsync(tokenArquivoDto, caminhoToken);
        }
    }
    else
    {
        if (File.Exists(caminhoToken))
        {
            var token = File.ReadAllText(caminhoToken).Replace("\\\\", "\\");


            tokenArquivoDto = new TokenArquivoDto
            {
                AccessToken = token,
                RefreshToken = "",
                ExpiresAt = null
            };

            if (tokenArquivoDto != null && !tokenArquivoDto.IsAccessTokenExpired())
                return tokenArquivoDto;

            _arquivoLog.Padrao("Gerando arquivo...");
            return await CriarNovaConfiguracaoRefreshTokenAsync(caminhoToken);
        }
    }
    return tokenArquivoDto;
}


async Task EnviarArquivoAsync(DropboxClient dropboxCliente, string pasta, string nomeArquivo, string conteudo)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("EnviarArquivoAsync");
    _arquivoLog.Alerta("============================");
    string caminho = $"{pasta}/{nomeArquivo}";
    _arquivoLog.Padrao($"Caminho: {caminho}");
    using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(conteudo));
    try
    {
        _arquivoLog.Alerta("============================");
        _arquivoLog.Alerta("ENVIAR ARQUIVO ");
        _arquivoLog.Alerta("============================");
        await dropboxCliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream);
        _arquivoLog.Padrao($"Arquivo enviado: {nomeArquivo}");


        _arquivoLog.Alerta("============================");
        _arquivoLog.Alerta("LISTAR ARQUIVOS");
        _arquivoLog.Alerta("============================");
        var arquivos = await dropboxCliente.Files.ListFolderAsync(pasta);
        _arquivoLog.Padrao($"\n Total de arquivos: {arquivos.Entries.Count}");
        foreach (var item in arquivos.Entries)
        {
            _arquivoLog.Padrao($"\n {item.Name} - {item.GetType().Name}");
        }
    }
    catch (ApiException<UploadError> ex)
    {
        _arquivoLog.Padrao($"Erro Dropbox: {ex.ErrorResponse}");
        if (ex.ErrorResponse.IsPath)
            _arquivoLog.Padrao($"Path error: {ex.ErrorResponse.AsPath}");

        throw;
    }
}

async Task<TokenArquivoDto> RefreskTokenOauthAsync(TokenArquivoDto tokenArquivoDto, string tOKEN_PATH_OAUTH)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("RefreskTokenOauthAsync");
    _arquivoLog.Alerta("============================");

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

    var json = await response.Content.ReadAsStringAsync();
    TokenResponse token = JsonSerializer.Deserialize<TokenResponse>(json);

    var novo = new TokenArquivoDto
    {
        AccessToken = token.AccessToken,
        RefreshToken = tokenArquivoDto.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
    };

    await File.WriteAllTextAsync(tOKEN_PATH_OAUTH,
        JsonSerializer.Serialize(novo, new JsonSerializerOptions { WriteIndented = true }));

    return novo;
}

async Task<TokenArquivoDto> CriarArquivoOauthAsync(TokenArquivoDto tokenArquivoDto, string tOKEN_PATH_OAUTH)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("CriarArquivoOauthAsync");
    _arquivoLog.Alerta("============================");

    _arquivoLog.Sucesso("\nAbra a URL abaixo no navegador:");
    _arquivoLog.Sucesso(UrlGeracaoCodigo());

    _arquivoLog.Sucesso("\nCole o code:");
    var code = Console.ReadLine();
    _arquivoLog.Sucesso($"Cole o code: {code}");


    if (string.IsNullOrWhiteSpace(code))
        throw new Exception("Code inválido");

    TokenResponse token = await ExchangeCodeForTokens(code);

    TokenArquivoDto store = new TokenArquivoDto
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
    };

    await File.WriteAllTextAsync(tOKEN_PATH_OAUTH, JsonSerializer.Serialize(store, new JsonSerializerOptions { WriteIndented = true }));

    return store;
}

//async Task<TokenResponse> ExchangeRefreshToken(string refreshToken)
//{
//    var request = new Dictionary<string, string>
//    {
//        ["grant_type"] = "refresh_token",
//        ["refresh_token"] = refreshToken,
//        ["client_id"] = _appKey,
//        ["client_secret"] = _appSecret
//    };

//    using var client = new HttpClient();
//    var response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", new FormUrlEncodedContent(request));
 

//    response.EnsureSuccessStatusCode();

//    var json = await response.Content.ReadAsStringAsync();
//    return JsonSerializer.Deserialize<TokenResponse>(json);
//}



async Task<TokenArquivoDto> CriarNovaConfiguracaoRefreshTokenAsync(string tOKEN_PATH_OAUTH)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("CriarNovaConfiguracaoRefreshTokenAsync");
    _arquivoLog.Alerta("============================");

    TokenResponse token = await RefreshAccessToken(tOKEN_PATH_OAUTH);

    TokenArquivoDto store = new TokenArquivoDto
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
    };

    await File.WriteAllTextAsync(tOKEN_PATH_OAUTH, JsonSerializer.Serialize(store, new JsonSerializerOptions { WriteIndented = true }));
    return store;
}



// ============================
// OAUTH DO DROPBOX
// ============================

string UrlGeracaoCodigo()
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("GetAuthorizationUrl");
    _arquivoLog.Alerta("============================");

    return "https://www.dropbox.com/oauth2/authorize" +
            $"?client_id={_appKey}" +
            "&response_type=code" +
            "&token_access_type=offline" +
            $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";
}

async Task<TokenResponse> ExchangeCodeForTokens(string code)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("ExchangeCodeForTokens");
    _arquivoLog.Alerta("============================");

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
    return JsonSerializer.Deserialize<TokenResponse>(json)!;
}

async Task<TokenResponse> RefreshAccessToken(string refreshToken)
{
    _arquivoLog.Alerta("============================");
    _arquivoLog.Alerta("RefreshAccessToken");
    _arquivoLog.Alerta("============================");

    using var client = new HttpClient();
    FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
    {
            new KeyValuePair<string,string>("grant_type", "refresh_token"),
            new KeyValuePair<string,string>("refresh_token", refreshToken),
            new KeyValuePair<string,string>("client_id", _appKey),
            new KeyValuePair<string,string>("client_secret", _appSecret)
    });
    var response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", content);
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<TokenResponse>(json)!;
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


public enum TipoToken
{
    OAuth = 0,
    Token = 1
}

