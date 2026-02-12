using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Users;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Dropbox.Api.Files.SearchMatchType;


// ============================
// CONFIGURAÇÃO
// ============================
string _appKey = "l2wdyrt2f613dty";
string _appSecret = "5zhyyqxacdq11di";
string _redirectUri = "http://localhost";
string _pasta = "/AMAURI_NET/Arquivos";
string _nomeArquivo = $"{DateTime.Now:ddMMyyyy_HHmmss}";


try
{
    EscreverLinha("********************************************************");
    EscreverLinha("************************* Oath 2.0 *********************");
    EscreverLinha("********************************************************");
    EscreverLinha("Iniciando Dropbox...\n");

    DropboxClient dropboxCliente = Inicializacao(TipoToken.OAuth).Result;

    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Oath.txt", "Arquivo enviado via Program.cs");

}
catch (Exception ex)
{
    EscreverLinha($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    EscreverLinha($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    EscreverLinha($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}


try
{
    EscreverLinha("\n********************************************************");
    EscreverLinha("********************* Token ******************************");
    EscreverLinha("**********************************************************");
    EscreverLinha("Iniciando Dropbox...\n");

    DropboxClient dropboxCliente = Inicializacao(TipoToken.Token).Result;

    await EnviarArquivoAsync(dropboxCliente, _pasta, $"{_nomeArquivo}_Token.txt", "Arquivo enviado via Program.cs");

}
catch (Exception ex)
{
    EscreverLinha($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    EscreverLinha($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    EscreverLinha($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}

EscreverLinha("============================");
EscreverLinha("FIM");
EscreverLinha("============================");


// ============================
// MÉTODOS
// ============================


async Task<DropboxClient> Inicializacao(TipoToken tipoToken)
{
    //var DROPBOX_ACCESS_TOKEN = File.ReadAllText(TOKEN_PATH_TOKEN).Replace("\\\\", "\\");



    TokenArquivoDto arquivo = await LerOuCriarTokenAsync(tipoToken);

    string tokenArquivo = arquivo.AccessToken;
    DateTime? vencimento = arquivo.ExpiresAt;

    EscreverLinha($"\n Vencimento: {vencimento}");
    EscreverLinha($"\n Token: {tokenArquivo}");
    if (string.IsNullOrWhiteSpace(tokenArquivo))
    {
        EscreverLinha("Token do Dropbox está vazio.");
    }
    EscreverLinha("\n Conectando ao Dropbox...");


    DropboxClient dropboxCliente = new DropboxClient(tokenArquivo);
    FullAccount conta = await dropboxCliente.Users.GetCurrentAccountAsync();
    if (conta == null)
    {
        //throw new Exception("Conta do Dropbox está vazio.");
    }

    EscreverLinha("============================");
    EscreverLinha("Informações da conta Dropbox:");
    EscreverLinha("============================");
    EscreverLinha($" - Nome: {conta.Name.DisplayName}");
    EscreverLinha($" - Email: {conta.Email}");
    EscreverLinha($" - AccountId: {conta.AccountId}");
    EscreverLinha($" - País: {conta.Country}");
    EscreverLinha($" - Tipo Basico: {conta.AccountType.IsBasic}");
    EscreverLinha($" - Tipo Comercial: {conta.AccountType.IsBusiness}");
    EscreverLinha($" - Tipo Profissional: {conta.AccountType.IsPro}");
    EscreverLinha("Informações técnicas do cliente:");
    EscreverLinha($" - Classe: {conta.GetType().Name}");
    EscreverLinha($" - Namespace: {conta.GetType().Namespace}");

    EscreverLinha("============================");
    EscreverLinha("ENVIAR ARQUIVO ");
    EscreverLinha("============================");

    return dropboxCliente;
}



async Task<TokenArquivoDto> LerOuCriarTokenAsync(TipoToken tipoToken)
{

    EscreverLinha("============================");
    EscreverLinha("LerOuCriarTokenAsync");
    EscreverLinha("============================");

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
                EscreverLinha("Renovando Token...");
                return await RefreskTokenOauthAsync(tokenArquivoDto, caminhoToken);
            }
        }
        else
        {
            EscreverLinha("Gerando arquivo...");
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

            EscreverLinha("Gerando arquivo...");
            return await CriarNovaConfiguracaoRefreshTokenAsync(caminhoToken);
        }
    }
    return tokenArquivoDto;
}


async Task EnviarArquivoAsync(DropboxClient dropboxCliente, string pasta, string nomeArquivo, string conteudo)
{
    EscreverLinha("============================");
    EscreverLinha("EnviarArquivoAsync");
    EscreverLinha("============================");
    string caminho = $"{pasta}/{nomeArquivo}";
    using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(conteudo));
    try
    {
        EscreverLinha("============================");
        EscreverLinha("ENVIAR ARQUIVO ");
        EscreverLinha("============================");
        await dropboxCliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream);
        EscreverLinha($"Arquivo enviado: {nomeArquivo}");


        EscreverLinha("============================");
        EscreverLinha("LISTAR ARQUIVOS");
        EscreverLinha("============================");
        var arquivos = await dropboxCliente.Files.ListFolderAsync(pasta);
        Console.WriteLine($"\n Total de arquivos: {arquivos.Entries.Count}");
        foreach (var item in arquivos.Entries)
        {
            Console.WriteLine($"\n {item.Name} - {item.GetType().Name}");
        }
    }
    catch (ApiException<UploadError> ex)
    {
        EscreverLinha($"Erro Dropbox: {ex.ErrorResponse}");
        if (ex.ErrorResponse.IsPath)
            EscreverLinha($"Path error: {ex.ErrorResponse.AsPath}");

        throw;
    }
}

async Task<TokenArquivoDto> RefreskTokenOauthAsync(TokenArquivoDto tokenArquivoDto, string tOKEN_PATH_OAUTH)
{
    EscreverLinha("============================");
    EscreverLinha("RefreskTokenOauthAsync");
    EscreverLinha("============================");

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
    EscreverLinha("============================");
    EscreverLinha("CriarArquivoOauthAsync");
    EscreverLinha("============================");

    EscreverLinha("\nAbra a URL abaixo no navegador:");
    EscreverLinha(UrlGeracaoCodigo());

    EscreverLinha("\nCole o code:");
    var code = Console.ReadLine();

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
    EscreverLinha("============================");
    EscreverLinha("CriarNovaConfiguracaoRefreshTokenAsync");
    EscreverLinha("============================");

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
    EscreverLinha("============================");
    EscreverLinha("GetAuthorizationUrl");
    EscreverLinha("============================");

    return "https://www.dropbox.com/oauth2/authorize" +
            $"?client_id={_appKey}" +
            "&response_type=code" +
            "&token_access_type=offline" +
            $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";
}

async Task<TokenResponse> ExchangeCodeForTokens(string code)
{
    EscreverLinha("============================");
    EscreverLinha("ExchangeCodeForTokens");
    EscreverLinha("============================");

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
    EscreverLinha("============================");
    EscreverLinha("RefreshAccessToken");
    EscreverLinha("============================");

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

static void EscreverLinha(string msg)
{
    Console.WriteLine(msg);
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

