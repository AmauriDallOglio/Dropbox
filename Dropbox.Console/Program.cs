using Dropbox.Console;




try
{
    var conexao = new ConexaoToken();

    var cliente = await conexao.CriaConexaoAsync();

    await conexao.EnviarArquivoAsync( cliente, "/AMAURI_NET/Arquivos/arquivo_teste.txt", "Arquivo de teste enviado via código" );

    await conexao.ListarArquivosAsync( cliente,"/AMAURI_NET/Arquivos" );
}
catch (Exception ex)
{
    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine($"Erro: {ex.Message}");
    Console.WriteLine(ex.InnerException?.Message);
    Console.WriteLine("--------------------------------------------------");
}


