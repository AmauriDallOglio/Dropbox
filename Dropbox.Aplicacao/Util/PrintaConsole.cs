namespace Dropbox.Aplicacao.Util
{
    public static class PrintaConsole
    {

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
