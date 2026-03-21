namespace Dropbox.Aplicacao.Util
{
    public class ArquivoLog
    {
        private readonly string _caminhoLog;

        public ArquivoLog()
        {

            string basePath = @"C:\"; //AppDomain.CurrentDomain.BaseDirectory; // @"C:\";

            _caminhoLog = Path.Combine(basePath, "Logs");
            if (!Directory.Exists(_caminhoLog))
            {
                Directory.CreateDirectory(_caminhoLog);
            }

            Info(_caminhoLog);

        }

        public async Task<string> IncluirLinha(Exception ex, string requestPath, string mensagemBasica)
        {

            // Nome do arquivo com base na data
            string nomeArquivo = Path.Combine(_caminhoLog, $"error_log_{DateTime.Now:dd-MM-yyyy}.txt");

            string separador = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} -----------------------------------------------------------------------------{Environment.NewLine}";
            string mensagemRetorno = $"TratamentoErroMiddleware | Path: {requestPath} | {mensagemBasica}: {ex.Message}{Environment.NewLine}";

            var mensagemPersonalizada = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensagemRetorno}{Environment.NewLine}";

            File.AppendAllText(nomeArquivo, separador);
            File.AppendAllText(nomeArquivo, mensagemPersonalizada);

            Console.WriteLine($"Erro: {mensagemPersonalizada}");

            Error(mensagemPersonalizada);


            return mensagemRetorno;
        }

        public static void Padrao(string menssagem, ConsoleColor foreground, ConsoleColor background)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            Console.WriteLine(menssagem);
            Console.ResetColor(); // Restaura as cores padrão após a mensagem.
        }


        public static void Error(string menssagem)
        {
            Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.White, ConsoleColor.Red);
        }


        public static void Sucesso(string menssagem)
        {
            Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Black, ConsoleColor.Green);
        }


        public static void Alerta(string menssagem)
        {
            Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Black, ConsoleColor.Yellow);
        }

        public static void Info(string menssagem)
        {
            Padrao($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {menssagem}", ConsoleColor.Yellow, ConsoleColor.Blue);
        }

    }
}
