using Dropbox.Api;
using Dropbox.Api.Files;
using System.Text;



Console.WriteLine("Carregando token - Dropbox");

var DROPBOX_ACCESS_TOKEN = File.ReadAllText("C:\\Amauri\\GitHub\\DROPBOX_ACCESS_TOKEN.txt").Replace("\\\\", "\\");


if (string.IsNullOrWhiteSpace(DROPBOX_ACCESS_TOKEN))
{
    throw new Exception("Token do Dropbox está vazio.");
}
else
{
    Console.WriteLine($"\n Token: {DROPBOX_ACCESS_TOKEN}");
}

Console.WriteLine("\n Conectando ao Dropbox...");

var dropboxCliente = new DropboxClient(DROPBOX_ACCESS_TOKEN);

try
{
    var conta = await dropboxCliente.Users.GetCurrentAccountAsync();
    if (conta == null)
    {
        throw new Exception("Conta do Dropbox está vazio.");
    }

    Console.WriteLine("\n Informações da conta Dropbox:");
    Console.WriteLine($" - Nome: {conta.Name.DisplayName}");
    Console.WriteLine($" - Email: {conta.Email}");
    Console.WriteLine($" - AccountId: {conta.AccountId}");
    Console.WriteLine($" - País: {conta.Country}");
    Console.WriteLine($" - Tipo Basico: {conta.AccountType.IsBasic}");
    Console.WriteLine($" - Tipo Comercial: {conta.AccountType.IsBusiness}");
    Console.WriteLine($" - Tipo Profissional: {conta.AccountType.IsPro}");
    Console.WriteLine("\n Informações técnicas do cliente:");
    Console.WriteLine($" - Classe: {dropboxCliente.GetType().Name}");
    Console.WriteLine($" - Namespace: {dropboxCliente.GetType().Namespace}");

    // ===== ENVIAR ARQUIVO =====
    var conteudo = Encoding.UTF8.GetBytes("Arquivo de teste enviado via código");

    using var stream = new MemoryStream(conteudo);

    await dropboxCliente.Files.UploadAsync("/AMAURI_NET/Arquivos/arquivo_teste.txt", WriteMode.Overwrite.Instance, body: stream);

    Console.WriteLine("\n Arquivo enviado com sucesso");

    // ===== LISTAR ARQUIVOS =====
    var arquivos = await dropboxCliente.Files.ListFolderAsync("/AMAURI_NET/Arquivos");

    Console.WriteLine($"\n Total de arquivos: {arquivos.Entries.Count}");

    foreach (var item in arquivos.Entries)
    {
        Console.WriteLine($"\n {item.Name} - {item.GetType().Name}");
    }

}
catch (Exception ex)
{
    Console.WriteLine($"\n -------------------------------------------------------------------------");
    Console.WriteLine($" Erro: {ex.Message} / {ex.InnerException?.Message??""}");
    Console.WriteLine($"------------------------------------------------------------------------- \n");
}

