namespace Dropbox.Servicos.Dto
{
    public class ExcluirArquivoResultadoDto
    {
        public string Caminho { get; private set; }
        public bool Excluido { get; private set; }

        private ExcluirArquivoResultadoDto(string caminho, bool excluido)
        {
            Caminho = caminho;
            Excluido = excluido;
        }

        public static ExcluirArquivoResultadoDto Criar(string caminho, bool excluido)
        {
            return new ExcluirArquivoResultadoDto(caminho, excluido);
        }
    }
}
