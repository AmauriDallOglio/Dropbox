using Dropbox.Api;
using Dropbox.Api.Files;
using System.Text;

namespace Dropbox.Console
{
    public class ConexaoToken
    {
        public async Task<DropboxClient> CriaConexao()
        {
            ImprimeLinha("Carregando token - Dropbox");

            var DROPBOX_ACCESS_TOKEN = File.ReadAllText("C:\\Amauri\\GitHub\\DROPBOX_ACCESS_TOKEN.txt").Replace("\\\\", "\\");

            if (string.IsNullOrWhiteSpace(DROPBOX_ACCESS_TOKEN))
                throw new Exception("Token do Dropbox está vazio.");

            ImprimeLinha("\nToken carregado com sucesso");

            ImprimeLinha("\nConectando ao Dropbox...");

            // Criação do cliente (NÃO é async)
            var dropboxCliente = new DropboxClient(DROPBOX_ACCESS_TOKEN);

            // Chamada async REAL para validar o token
            var account = await dropboxCliente.Users.GetCurrentAccountAsync();

            ImprimeLinha($"Conectado como: {account.Email}");

            return dropboxCliente;
        }

        //1️ Cria e valida a conexão
        public async Task<DropboxClient> CriaConexaoAsync()
        {
            ImprimeLinha("Carregando token - Dropbox");

            var token = File.ReadAllText("C:\\Amauri\\GitHub\\DROPBOX_ACCESS_TOKEN.txt").Replace("\\\\", "\\");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token do Dropbox está vazio.");

            ImprimeLinha("Conectando ao Dropbox...");
            var cliente = new DropboxClient(token);
            await ValidarContaAsync(cliente);
            return cliente;
        }

        //2️ Valida se a conta está acessível
        private async Task ValidarContaAsync(DropboxClient cliente)
        {
            var conta = await cliente.Users.GetCurrentAccountAsync();
            if (conta == null)
                throw new Exception("Conta do Dropbox não encontrada.");

            ImprimirInformacoesConta(conta, cliente);
        }

        //3️ Imprime informações da conta
        private void ImprimirInformacoesConta( Dropbox.Api.Users.FullAccount conta,  DropboxClient cliente)
        {
            ImprimeLinha("\nInformações da conta Dropbox:");
            ImprimeLinha($" - Nome: {conta.Name.DisplayName}");
            ImprimeLinha($" - Email: {conta.Email}");
            ImprimeLinha($" - AccountId: {conta.AccountId}");
            ImprimeLinha($" - País: {conta.Country}");
            ImprimeLinha($" - Tipo Básico: {conta.AccountType.IsBasic}");
            ImprimeLinha($" - Tipo Comercial: {conta.AccountType.IsBusiness}");
            ImprimeLinha($" - Tipo Profissional: {conta.AccountType.IsPro}");

            ImprimeLinha("\nInformações técnicas do cliente:");
            ImprimeLinha($" - Classe: {cliente.GetType().Name}");
            ImprimeLinha($" - Namespace: {cliente.GetType().Namespace}");
        }

        //4️ Envia um arquivo para o Dropbox
        public async Task EnviarArquivoAsync( DropboxClient cliente, string caminho, string conteudoTexto)
        {
            var bytes = Encoding.UTF8.GetBytes(conteudoTexto);
            using var stream = new MemoryStream(bytes);
            await cliente.Files.UploadAsync(caminho, WriteMode.Overwrite.Instance, body: stream );
            ImprimeLinha("Arquivo enviado com sucesso.");
        }

        // 5️ Lista arquivos de uma pasta
        public async Task ListarArquivosAsync( DropboxClient cliente, string pasta)
        {
            var arquivos = await cliente.Files.ListFolderAsync(pasta);
            ImprimeLinha($"Total de arquivos: {arquivos.Entries.Count}");
            foreach (var item in arquivos.Entries)
            {
                ImprimeLinha($" - {item.Name} ({item.GetType().Name})");
            }
        }

        private void ImprimeLinha(string mensagem)
        {
            System.Console.WriteLine(mensagem);
        }
    }
}
