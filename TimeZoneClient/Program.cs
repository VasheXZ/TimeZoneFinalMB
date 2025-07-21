using System;
using System.IO.Pipes;

// Проверка корректности введенного номера часовго пояса
class TimeZoneValidator
{
    public static bool IsValidTimeZoneNumber(string number, out int index)
    {
        if (int.TryParse(number, out index) && index >= 1 && index <= 26)
        {
            return true;
        }
        Console.WriteLine($"Ошибка: Неверный номер часового пояса '{number}'. Введите число от 1 до 26.");
        return false;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Команды: changetimezone, settimezone <номер>, timezone <ID>, gettime, combinetimezones <номер1> <номер2> ... (до 4), stop, exit");
        while (true)
        {
            Console.Write("Введите команду: ");
            string input = Console.ReadLine();

            // Обработка команды exit
            if (input == "exit")
                break;

            // Обработка команды changetimezone
            if (input == "changetimezone")
            {
                HandleChangeTimeZone();
            }

            // Обработка команды settimezone
            else if (input.StartsWith("settimezone "))
            {
                string number = input.Substring("settimezone ".Length).Trim();
                if (TimeZoneValidator.IsValidTimeZoneNumber(number, out int _))
                {
                    SendCommand("settimezone:" + number);
                }
            }

            // Обработка команды timezone
            else if (input.StartsWith("timezone "))
            {
                string id = input.Substring("timezone ".Length).Trim();
                SendCommand("timezone:" + id);
            }

            // Обработка команды gettime
            else if (input == "gettime")
            {
                SendCommand("gettime");
            }

            // Обработка команды combinetimezones
            else if (input.StartsWith("combinetimezones "))
            {
                string numbers = input.Substring("combinetimezones ".Length).Trim();
                if (string.IsNullOrEmpty(numbers))
                {
                    Console.WriteLine("Ошибка: Укажите до 4 номеров часовых поясов.");
                }
                else
                {
                    string[] number_parts = numbers.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (number_parts.Length > 4)
                    {
                        Console.WriteLine("Ошибка: Можно указать не более 4 часовых поясов.");
                    }
                    else
                    {
                        bool valid = true;
                        foreach (string num in number_parts)
                        {
                            if (!TimeZoneValidator.IsValidTimeZoneNumber(num, out int _))
                            {
                                valid = false;
                                break;
                            }
                        }
                        if (valid)
                        {
                            SendCommand("combinetimezones " + numbers);
                        }
                    }
                }
            }
            else
            {
                SendCommand(input);
            }
        }
    }

    // Метод для обработки команды changetimezone
    static void HandleChangeTimeZone()
    {
        try
        {
            // Создание клиента для именованного канала и подключение
            using var pipe_client = new NamedPipeClientStream(".", "TimeZonePipe", PipeDirection.InOut);
            pipe_client.Connect(5000);
            using var writer = new StreamWriter(pipe_client) { AutoFlush = true };
            using var reader = new StreamReader(pipe_client);

            // Отправка команды changetimezone
            writer.WriteLine("changetimezone");

            // Обработка ответа от сервера
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                if (line.StartsWith("Введите номер часового пояса:"))
                    break;
            }

            // Ввод номера часвого пояса
            Console.Write("Введите номер: ");
            string number = Console.ReadLine();
            if (TimeZoneValidator.IsValidTimeZoneNumber(number, out int _))
            {
                writer.WriteLine($"settimezone:{number}");
                try
                {
                    // Вывод ответа от сервера
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        Console.WriteLine(line);
                    }
                }
                catch (IOException) { }
            }
        }
        // Обработка ошибок
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Pipe is broken") && !ex.Message.Contains("Cannot access a closed pipe"))
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
        }
    }

    // Метод для отправка команд к серверу через именованный канал
    static void SendCommand(string command)
    {
        try
        {
            // Создание клиента для именованного канала и подключение
            using var pipe_client = new NamedPipeClientStream(".", "TimeZonePipe", PipeDirection.InOut);
            pipe_client.Connect(5000);
            using var writer = new StreamWriter(pipe_client) { AutoFlush = true };
            using var reader = new StreamReader(pipe_client);

            // Отправка команды серверу
            writer.WriteLine(command);

            try
            {
                // Обработка и вывод ответа сервера
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    Console.WriteLine($"Ответ сервера: {line}");
                }
            }
            catch (IOException) { }
        }

        // Обработка ошибок
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Pipe is broken") && !ex.Message.Contains("Cannot access a closed pipe"))
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
        }
    }
}