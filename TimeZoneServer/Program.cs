using System;
using System.Threading;

class Program
{
    static void Main()
    {
        // Запуск потока для вывода времени
        Thread output_Thread = new Thread(TimeOutputManager.OutputTime);
        output_Thread.Start();

        // Запуск сервера
        NamedPipeServer.StartNamedPipeServer();

        // Ожидание завершения потока вывода
        output_Thread.Join();
    }
}