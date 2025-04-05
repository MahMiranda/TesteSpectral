using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public class Logger
{
    private static Logger instance;
    private static readonly object travaInstancia = new object();

    private readonly Queue<string> filaDeLogs = new Queue<string>();
    private readonly object travaFila = new object();
    private bool processando = false;

    private Logger()
    {
    }

    public static Logger GetInstance()
    {
        if (instance == null)
        {
            lock (travaInstancia)
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
            }
        }

        return instance;
    }

    public void Log(string mensagem, LogLevel nivel)
    {
        string logFormatado = $"[{nivel}] {DateTime.Now}: {mensagem}";

        lock (travaFila)
        {
            filaDeLogs.Enqueue(logFormatado);
        }

        if (!processando)
        {
            processando = true;
            Task.Run(() => ProcessarLogs());
        }
    }

    private async Task ProcessarLogs()
    {
        while (true)
        {
            string mensagem;

            lock (travaFila)
            {
                if (filaDeLogs.Count == 0)
                {
                    processando = false;
                    return;
                }

                mensagem = filaDeLogs.Dequeue();
            }

            Console.WriteLine(mensagem);
        }
    }
}

class Program
{
    static void Main()
    {
        Logger logger = Logger.GetInstance();

        logger.Log("Iniciando...", LogLevel.Info);
        logger.Log("Comportamento inesperado", LogLevel.Warning);
        logger.Log("Erro fatal!", LogLevel.Error);
        Thread.Sleep(1000);
    }
}
