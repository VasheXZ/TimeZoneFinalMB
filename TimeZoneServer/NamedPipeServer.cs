using System;
using System.IO.Pipes;
using System.IO;
using System.Linq;
/// <summary>
/// Provides functionality to create and manage a named pipe server for handling client commands related to time zone
/// management and time retrieval.
/// </summary>
/// <remarks>The <see cref="NamedPipeServer"/> class listens for client connections via a named pipe and processes
/// commands sent by clients. Supported commands include changing the time zone, retrieving the current time, and
/// managing combined time zone outputs. The server runs continuously until explicitly stopped by a client command or
/// external intervention.  This class is designed to handle multiple client connections sequentially, with each
/// connection being processed in isolation. It uses the "TimeZonePipe" named pipe for communication.</remarks>
class NamedPipeServer
{
    public static void StartNamedPipeServer()
    {
        Console.WriteLine("Ожидание подключения клиента...");
        while (TimeOutputManager.is_Running)
        {
            using var pipe_Server = new NamedPipeServerStream("TimeZonePipe", PipeDirection.InOut);
            try
            {
                pipe_Server.WaitForConnection();

                using var reader = new StreamReader(pipe_Server);
                using var writer = new StreamWriter(pipe_Server) { AutoFlush = true };

                while (TimeOutputManager.is_Running && pipe_Server.IsConnected)
                {
                    string command = reader.ReadLine();
                    if (string.IsNullOrEmpty(command))
                    {
                        break;
                    }
                    if (pipe_Server.IsConnected)
                    {
                        bool should_close = ProcessCommand(command, writer);
                        if (should_close)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сервера: {ex.Message}");
            }
            finally
            {
                if (pipe_Server.IsConnected)
                {
                    try
                    {
                        pipe_Server.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отключении: {ex.Message}");
                    }
                }
            }
        }
    }

    public static bool ProcessCommand(string command, StreamWriter writer)
    {
        try
        {
            // changetimezone
            if (command == "changetimezone")
            {
                writer.WriteLine("Список доступных часовых поясов:");
                for (int i = 0; i < TimeZoneInformer.TimeZones.Length; i++)
                {
                    writer.WriteLine($"{i + 1}. {TimeZoneInformer.TimeZones[i].City} ({TimeZoneInformer.TimeZones[i].Country}) | {TimeZoneInformer.TimeZones[i].TimeZoneId}");
                }
                writer.WriteLine("Введите номер часового пояса:");
                TimeOutputManager.is_Combined_Mode = false;
                TimeOutputManager.combined_TimeZones = new int[0];
                return false;
            }
            // settimezone
            else if (command.StartsWith("settimezone:"))
            {
                string number_str = command.Substring(12).Trim();
                if (int.TryParse(number_str, out int index) && index >= 1 && index <= TimeZoneInformer.TimeZones.Length)
                {
                    string new_TimeZone = TimeZoneInformer.TimeZones[index - 1].TimeZoneId;
                    TimeOutputManager.current_TimeZone_ID = new_TimeZone;
                    writer.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                    Console.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                    TimeOutputManager.is_Combined_Mode = false;
                    TimeOutputManager.combined_TimeZones = new int[0];
                }
                else
                {
                    writer.WriteLine($"Ошибка: Неверный номер часового пояса. Введите число от 1 до {TimeZoneInformer.TimeZones.Length}.");
                    Console.WriteLine($"Ошибка: Неверный номер часового пояса '{number_str}'.");
                }
                return true;
            }
            // timezone
            else if (command.StartsWith("timezone:"))
            {
                string new_TimeZone = command.Substring(9).Trim();
                TimeOutputManager.current_TimeZone_ID = new_TimeZone;
                writer.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                Console.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                TimeOutputManager.is_Combined_Mode = false;
                TimeOutputManager.combined_TimeZones = new int[0];
                return true;
            }
            // gettime
            else if (command == "gettime")
            {
                try
                {
                    var time_zone = TimeZoneInformer.GetTimeZoneInfo(TimeOutputManager.current_TimeZone_ID);
                    DateTime local_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, time_zone);
                    writer.WriteLine($"Текущее время: {local_time:yyyy-MM-dd HH:mm:ss}, Часовой пояс: {TimeOutputManager.current_TimeZone_ID}");
                    Console.WriteLine($"Клиент запросил время: {local_time:yyyy-MM-dd HH:mm:ss}, Часовой пояс: {TimeOutputManager.current_TimeZone_ID}");
                }
                catch (TimeZoneNotFoundException)
                {
                    writer.WriteLine($"Ошибка: Часовой пояс '{TimeOutputManager.current_TimeZone_ID}' не найден. Используется Russian Standard Time.");
                    Console.WriteLine($"Ошибка: Часовой пояс '{TimeOutputManager.current_TimeZone_ID}' не найден.");
                    TimeOutputManager.current_TimeZone_ID = "Russian Standard Time";
                }
                return true;
            }
            // combinetimezones
            else if (command.StartsWith("combinetimezones "))
            {
                string numbers_str = command.Substring(16).Trim();
                if (string.IsNullOrEmpty(numbers_str))
                {
                    writer.WriteLine("Ошибка: Укажите до 4 номеров часовых поясов, разделённых пробелами.");
                    Console.WriteLine("Ошибка: Не указаны номера часовых поясов для команды combinetimezones.");
                    return true;
                }

                string[] number_parts = numbers_str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (number_parts.Length > 4)
                {
                    writer.WriteLine("Ошибка: Можно указать не более 4 часовых поясов.");
                    Console.WriteLine("Ошибка: Указано более 4 часовых поясов.");
                    return true;
                }

                int[] new_combined = new int[number_parts.Length];
                bool valid = true;
                for (int i = 0; i < number_parts.Length; i++)
                {
                    if (int.TryParse(number_parts[i], out int index) && index >= 1 && index <= TimeZoneInformer.TimeZones.Length)
                    {
                        new_combined[i] = index;
                    }
                    else
                    {
                        valid = false;
                        writer.WriteLine($"Ошибка: Неверный номер часового пояса '{number_parts[i]}'. Введите числа от 1 до {TimeZoneInformer.TimeZones.Length}.");
                        Console.WriteLine($"Ошибка: Неверный номер часового пояса '{number_parts[i]}'.");
                        break;
                    }
                }

                if (valid)
                {
                    TimeOutputManager.combined_TimeZones = new_combined;
                    TimeOutputManager.is_Combined_Mode = true;
                    writer.WriteLine("Комбинированный вывод времени установлен для указанных часовых поясов.");
                    Console.WriteLine($"Комбинированный вывод времени установлен для: {string.Join(", ", new_combined.Select(i => TimeZoneInformer.TimeZones[i - 1].City))}");
                }
                return true;
            }
            // stop
            else if (command == "stop")
            {
                TimeOutputManager.is_Running = false;
                writer.WriteLine("Сервер остановлен.");
                Console.WriteLine("Сервер остановлен.");
                return true;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки команды: {ex.Message}");
            writer.WriteLine($"Ошибка обработки команды: {ex.Message}");
            return true;
        }
    }
}