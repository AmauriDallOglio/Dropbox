using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Dropbox.Servicos.Servico
{
    public class DropboxServico : IDropboxServico
    {
        private readonly AppSettingsDto _AppSettingsDto;

        public DropboxServico(IOptions<AppSettingsDto> appSettingsDto)
        {
            _AppSettingsDto = appSettingsDto.Value;
        }

        private DropboxClient ObterDropboxCliente()
        {
            if (!File.Exists(_AppSettingsDto.Token))
                throw new FileNotFoundException("Arquivo de token do Dropbox não encontrado.");

            var token = File.ReadAllText(_AppSettingsDto.Token).Replace("\\\\", "\\");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token do Dropbox está vazio.");

            return new DropboxClient(token);
        }

        public async Task<InformacaoContaDto> ObterInformacaoContaAsync(CancellationToken cancellationToken)
        {
            using var cliente = ObterDropboxCliente();
            var conta = await cliente.Users.GetCurrentAccountAsync();

            cancellationToken.ThrowIfCancellationRequested();

            return new InformacaoContaDto
            {
                Nome = conta.Name.DisplayName,
                Email = conta.Email,
                Pais = conta.Country,
                AccountId = conta.AccountId,
                TipoBasico = conta.AccountType.IsBasic,
                TipoBusiness = conta.AccountType.IsBusiness,
                TipoPro = conta.AccountType.IsPro
            };
        }

        public async Task<FileMetadata> EnviarArquivoAsync(UploadArquivoRequest request, string subFolder, CancellationToken cancellationToken)
        {
            if (request == null || request.File.Length == 0)
                throw new ArgumentException("Arquivo inválido.");

            cancellationToken.ThrowIfCancellationRequested();
            using var cliente = ObterDropboxCliente();
            var caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}/{request.File.FileName}";
            await using var stream = request.File.OpenReadStream();
            var resultado = await cliente.Files.UploadAsync( caminho,  WriteMode.Overwrite.Instance, body: stream);
            return resultado;
        }

 

        public async Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken)
        {
            DropboxClient cliente = ObterDropboxCliente();
            cancellationToken.ThrowIfCancellationRequested();

            string caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}";
            ListFolderResult? resultadoDropbox = await cliente.Files.ListFolderAsync(caminho);

            List<object> resultado = new List<object>();

            foreach (Metadata e in resultadoDropbox.Entries)
            {
                string linkCompartilhadoPreview = string.Empty;
                string linkCompartilhadoDownload = string.Empty;

                if (e.IsFile)
                {
                    string? urlBase = await ObterOuCriarLinkCompartilhadoAsync( cliente, e.PathDisplay);

                    if (!string.IsNullOrEmpty(urlBase))
                    {
                        linkCompartilhadoPreview = AjustarLinkDownload(urlBase, DropboxLinkModo.Preview);
                        linkCompartilhadoDownload = AjustarLinkDownload(urlBase, DropboxLinkModo.Download);
                    }
                }

                var file = e as Dropbox.Api.Files.FileMetadata;


                resultado.Add(new
                {
                    e.Name,
                    e.PathDisplay,
                    e.PathLower,
                    Tipo = e.GetType().Name,
                    Tamanho = file?.Size,
                    DataModificacao = file?.ClientModified,
                    Rev = file?.Rev,
                    LinkCompartilhadoPreview = linkCompartilhadoPreview,
                    LinkCompartilhadoDownload = linkCompartilhadoDownload
                });
            }

            return resultado;
        }


        private async Task<string?> ObterOuCriarLinkCompartilhadoAsync(DropboxClient cliente, string path)
        {
            var links = await cliente.Sharing.ListSharedLinksAsync( path,  directOnly: true);

            var linkExistente = links.Links.FirstOrDefault();
            if (linkExistente != null)
                return linkExistente.Url;

            var novoLink = await cliente.Sharing.CreateSharedLinkWithSettingsAsync(path);
            return novoLink.Url;
        }

        public static string AjustarLinkDownload(string url, DropboxLinkModo modo)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            // Remove parâmetros conflitantes
            //url = url.Trim().Trim('"');
            url = Regex.Replace(url, @"([&?])(dl|raw)=\d", string.Empty);

            // Define o parâmetro conforme o modo escolhido
            string parametro = string.Empty;
            switch (modo)
            {
                case DropboxLinkModo.Download:
                    parametro = "dl=1";
                    break;

                case DropboxLinkModo.Streaming:
                    parametro = "raw=1";
                    break;

                case DropboxLinkModo.Preview:
                    parametro = "dl=0";
                    break;

                default:
                    break;
            }


            // Se for automático, retorna o link limpo
            if (parametro == null)
                return url;

            // Adiciona o parâmetro corretamente
            return url.Contains("?") ? $"{url}&{parametro}" : $"{url}?{parametro}";
        }


        public enum DropboxLinkModo
        {
            Automatico = 0,   // Não altera, só garante que seja válido
            Download = 1,     // Força download (dl=1)
            Streaming = 2,    // Conteúdo bruto (raw=1)
            Preview = 3      // Força visualização (dl=0)
        }


    }
}
