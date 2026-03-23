namespace Dropbox.Dominio.Entidade
{
    public class DropboxConfiguracao
    {
        public int Id { get; private set; }

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        public string? AppKey { get; private set; }
        public string? AppSecret { get; private set; }
        public string? RedirectUri { get; private set; }
        public string? Pasta { get; private set; }
        public string? NomeArquivo { get; private set; }


        public DateTime? UltimaAtualizacaoToken { get; private set; }
        public DateTime? UltimaAtualizacaoRefresh { get; private set; }
        public DateTime? UltimaAtualizacaoCodigo { get; private set; }
 


        // Construtor protegido para EF
        protected DropboxConfiguracao() { }

        // Fábrica para criar nova configuração
        public static DropboxConfiguracao InserirDados( string appKey,  string appSecret,  string redirectUri, string accessToken, string refreshToken,  DateTime expiresAt, string? pasta = null, string? nomeArquivo = null)
        {
            return new DropboxConfiguracao
            {
                AppKey = appKey,
                AppSecret = appSecret,
                RedirectUri = redirectUri,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                Pasta = pasta,
                NomeArquivo = nomeArquivo
            };
        }

        // Método de domínio para atualizar tokens
        public void AtualizarTokens(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            UltimaAtualizacaoToken = DateTime.Now;
        }

        public void AtualizarRefreshComCodigo(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            UltimaAtualizacaoCodigo = DateTime.Now;
        }


        public void AtualizarConfiguracao(string appKey, string appSecret, string redirectUri, string pasta, string nomeArquivo)
        {
            AppKey = appKey;
            AppSecret = appSecret;
            RedirectUri = redirectUri;
            Pasta = pasta;
            NomeArquivo = nomeArquivo;

                //AccessToken = "",
                //RefreshToken = "",
                //ExpiresAt = null
        }

        public void AtualizarRefreshAutomatico(string accessToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            ExpiresAt = expiresAt;
            UltimaAtualizacaoRefresh = DateTime.Now;
        }

 

    }
}
