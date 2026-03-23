namespace Dropbox.Aplicacao.Rotas.Query.ObterArquivos
{
    public class ObterArquivosResponse
    {
        public IEnumerable<object> Arquivos { get; set; } = new List<object>();
    }
}
