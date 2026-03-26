using Dropbox.Servicos.Dto;

namespace Dropbox.Aplicacao.Rotas.Command.ExcluirArquivo
{
    public class ExcluirArquivoResponse
    {
        public string Caminho { get; set; } = string.Empty;
        public bool Excluido { get; set; }

        public static ExcluirArquivoResponse ConverterExcluirArquivoResultadoDto(ExcluirArquivoResultadoDto dto)
        {
            return new ExcluirArquivoResponse
            {
                Caminho = dto.Caminho,
                Excluido = dto.Excluido
            };
        }
    }
}
