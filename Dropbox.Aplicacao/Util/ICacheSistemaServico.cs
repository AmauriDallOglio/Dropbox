namespace Dropbox.Aplicacao.Util
{
    public interface ICacheSistemaServico
    {
        void RegistrarErro(Exception ex, string path, string mensagem);
        void RegistrarAcesso(string path, string metodo, int statusCode);
    }
}
