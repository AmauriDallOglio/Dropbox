using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Aplicacao.EntidadeDto;
using Dropbox.Servicos.ServicoInterface;
using Microsoft.Extensions.Options;

namespace Dropbox.Servicos.Servico
{
    public class DropboxServico : IDropboxServico
    {
        private readonly AppSettingsDto _AppSettingsDto;

        public DropboxServico(IOptions<AppSettingsDto> appSettingsDto)
        {
            _AppSettingsDto = appSettingsDto.Value;
        }

        private DropboxClient DropboxCliente()
        {
            if (!File.Exists(_AppSettingsDto.Token))
                throw new FileNotFoundException("Arquivo de token do Dropbox não encontrado.");

            var token = File.ReadAllText(_AppSettingsDto.Token)
                .Replace("\\\\", "\\");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token do Dropbox está vazio.");

            return new DropboxClient(token);
        }

        public async Task<object> ObterInformacaoContaAsync(CancellationToken cancellationToken)
        {
            using var cliente = DropboxCliente();
            var conta = await cliente.Users.GetCurrentAccountAsync();

            cancellationToken.ThrowIfCancellationRequested();

            return new
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

        public async Task EnviarArquivoAsync(UploadArquivoRequest request, string subFolder, CancellationToken cancellationToken)
        {
            if (request == null || request.File.Length == 0)
                throw new ArgumentException("Arquivo inválido.");

            cancellationToken.ThrowIfCancellationRequested();

            using var cliente = DropboxCliente();

            var caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}/{request.File.FileName}";

            await using var stream = request.File.OpenReadStream();

            await cliente.Files.UploadAsync(
                caminho,
                WriteMode.Overwrite.Instance,
                body: stream);
        }

        public async Task<IEnumerable<object>> ObterArquivos(string subFolder, CancellationToken cancellationToken)
        {
            using var cliente = DropboxCliente();

            cancellationToken.ThrowIfCancellationRequested();

            var caminho = $"{_AppSettingsDto.PastaBase}/{subFolder}";

            var resultado = await cliente.Files.ListFolderAsync(caminho);

            return resultado.Entries.Select(e => new
            {
                e.Name,
                Tipo = e.GetType().Name
            });
        }
    }
}
