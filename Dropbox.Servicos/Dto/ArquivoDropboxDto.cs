using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dropbox.Servicos.Dto
{
    public class ArquivoDropboxDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Caminho { get; set; } = string.Empty;
        public ulong? Tamanho { get; set; }
        public DateTime? DataModificacao { get; set; }

        public string LinkPreview { get; set; } = string.Empty;
        public string LinkDownload { get; set; } = string.Empty;

        private string UrlCompartilhada { get; set; } = string.Empty;


        private ArquivoDropboxDto() { }

        public ArquivoDropboxDto(string nome, string caminho, ulong? tamanho, DateTime? dataModificacao, string? urlCompartilhada)
        {
            Nome = nome;
            Caminho = caminho;
            Tamanho = tamanho;
            DataModificacao = dataModificacao;
            UrlCompartilhada = urlCompartilhada ?? string.Empty;

            Validar();
        }

        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                throw new Exception("Nome do arquivo inválido");

            if (string.IsNullOrWhiteSpace(Caminho))
                throw new Exception("Caminho do arquivo inválido");
        }


        public string ObterLinkPreview()
        {
            return AjustarLink(UrlCompartilhada, "dl=0");
        }

        public string ObterLinkDownload()
        {
            return AjustarLink(UrlCompartilhada, "dl=1");
        }

        public bool EhArquivoGrande()
        {
            return Tamanho.HasValue && Tamanho.Value > 10_000_000; // 10MB
        }

        private string AjustarLink(string url, string parametro)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            url = System.Text.RegularExpressions.Regex.Replace(url, @"([&?])(dl|raw)=\d", string.Empty);

            return url.Contains("?") ? $"{url}&{parametro}" : $"{url}?{parametro}";
        }
    }
}
