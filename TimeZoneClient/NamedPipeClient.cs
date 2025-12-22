using System;
using System.IO.Pipes;
using System.IO;
/// <summary>
/// Предоставляет функциональность для взаимодействия с сервером именованного канала для выполнения операций, таких как изменение часового пояса или отправка пользовательских команд.
/// </summary>
/// <remarks>Этот класс содержит статические методы для взаимодействия с сервером именованного канала с использованием канала "TimeZonePipe". 
/// Он позволяет отправлять предопределенные или пользовательские команды и обрабатывать ответы сервера.</remarks>
class NamedPipeClient
{
    public static void HandleChangeTimeZone()
    {
        try
        {
            using var pipe_client = new NamedPipeClientStream(".", "TimeZonePipe", PipeDirection.InOut);
            pipe_client.Connect(5000);
            using var writer = new StreamWriter(pipe_client) { AutoFlush = true };
            using var reader = new StreamReader(pipe_client);

            writer.WriteLine("changetimezone");

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                if (line.StartsWith("Введите номер часового пояса:"))
                    break;
            }

            Console.Write("Введите номер: ");
            string number = Console.ReadLine();
            if (TimeZoneValidator.IsValidTimeZoneNumber(number, out int _))
            {
                writer.WriteLine($"settimezone:{number}");
                try
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        Console.WriteLine(line);
                    }
                }
                catch (IOException) { }
            }
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Pipe is broken") && !ex.Message.Contains("Cannot access a closed pipe"))
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
        }
    }

    public static void SendCommand(string command)
    {
        try
        {
            using var pipe_client = new NamedPipeClientStream(".", "TimeZonePipe", PipeDirection.InOut);
            pipe_client.Connect(5000);
            using var writer = new StreamWriter(pipe_client) { AutoFlush = true };
            using var reader = new StreamReader(pipe_client);

            writer.WriteLine(command);

            try
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    Console.WriteLine($"Ответ сервера: {line}");
                }
            }
            catch (IOException) { }
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Pipe is broken") && !ex.Message.Contains("Cannot access a closed pipe"))
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
        }
    }
}