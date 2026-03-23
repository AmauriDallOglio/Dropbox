using Dropbox.Aplicacao.Rotas.Query.DadosConta;
using Dropbox.Aplicacao.Util;
using Dropbox.Dominio.Entidade;
using Dropbox.Dominio.InterfaceRepositorio;
using Dropbox.Servicos.Dto;
using System.Text.Json;
using static Dropbox.Api.TeamLog.AdminAlertSeverityEnum;

namespace Dropbox.Aplicacao.Rotas.Command.InserirCodigoUrl
{
    public class GerarTokensHandler
    {
         
        private readonly IDropboxConfiguracaoRepositorio _configuracaoRepositorio;

        public GerarTokensHandler(IDropboxConfiguracaoRepositorio configuracaoRepositorio)
        {
            _configuracaoRepositorio = configuracaoRepositorio;
        }


        public async Task<ResultadoOperacao> Handle(GerarTokensRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var client = new HttpClient();
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type", "authorization_code"),
                    new KeyValuePair<string,string>("code", request.Code),
                    new KeyValuePair<string,string>("client_id", request.AppKey),
                    new KeyValuePair<string,string>("client_secret", request.AppSecret),
                    new KeyValuePair<string,string>("redirect_uri", request.RedirectUri)
                });


                TokenResponse? tokenResponse;
                try
                {
                    //Http
                    HttpResponseMessage responseHttp = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", content, cancellationToken);
                    if (!responseHttp.IsSuccessStatusCode)
                    {
                        string erro = await responseHttp.Content.ReadAsStringAsync(cancellationToken);
                        return ResultadoOperacao.GerarErro("Erro na requisição ao Dropbox", (int)responseHttp.StatusCode, erro);
                    }


                    //Desserialização
                    string? json = await responseHttp.Content.ReadAsStringAsync(cancellationToken);
                    responseHttp.EnsureSuccessStatusCode();
                    tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
                    if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                        return ResultadoOperacao.GerarErro("Token inválido retornado pelo Dropbox", 500, "AccessToken vazio ou nulo.");

                }
                catch (Exception ex)
                {
                    return ResultadoOperacao.GerarErro("Erro ao processar resposta do Dropbox", 500, ex.Message);
                }


                GerarTokensResponse response = new GerarTokensResponse
                {
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresAt = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                };


                //  Persistência no banco
                try
                {
                    DropboxConfiguracao configuracaoExistente = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken);
                    if (configuracaoExistente == null)
                    {
                        DropboxConfiguracao novaConfig = DropboxConfiguracao.InserirDados( request.AppKey, request.AppSecret,  request.RedirectUri,  response.AccessToken, response.RefreshToken, response.ExpiresAt
                        );
                        await _configuracaoRepositorio.IncluirAsync(novaConfig, cancellationToken);
                    }
                    else
                    {
                        //Grava no banco
                        configuracaoExistente.AtualizarRefreshComCodigo(response.AccessToken, response.RefreshToken, response.ExpiresAt);
                        await _configuracaoRepositorio.EditarAsync(configuracaoExistente, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    return ResultadoOperacao.GerarErro("Erro ao salvar configuração no banco", 500, ex.Message);
                }

                return ResultadoOperacao.GerarSucesso(response, "Token gerado com sucesso");
            }
            catch (Exception ex)
            {
                return ResultadoOperacao.GerarErro("Erro ao trocar código por token", 500, ex.Message);
            }
        }
    }
}
