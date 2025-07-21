using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Linq;

class Program
{
    private static string current_TimeZone_ID = "Russian Standard Time"; // По умолчанию UTC+3
    private static bool is_Running = true; // Bool для статуса сервера
    private static int[] combined_TimeZones = new int[0]; // Массив для хранения номеров часовых поясов для комбинированного вывода
    private static bool is_Combined_Mode = false; // Bool для режима комбинированного вывода

    // Список часовых поясов
    private static readonly (string City, string Country, string TimeZoneId)[] TimeZones = new[]
    {
        ("Паго-Паго", "Американское Самоа", "Samoa Standard Time"),
        ("Гонолулу", "Гавайи, США", "Hawaiian Standard Time"),
        ("Анкоридж", "Аляска, США", "Alaskan Standard Time"),
        ("Лос-Анджелес", "США", "Pacific Standard Time"),
        ("Феникс", "США", "US Mountain Standard Time"),
        ("Мехико", "Мексика", "Central Standard Time (Mexico)"),
        ("Нью-Йорк", "США", "Eastern Standard Time"),
        ("Санто-Доминго", "Доминиканская Республика", "SA Western Standard Time"),
        ("Сан-Паулу", "Бразилия", "E. South America Standard Time"),
        ("Южная Георгия", "Острова", "UTC-02"),
        ("Прая", "Кабо-Верде", "Cape Verde Standard Time"),
        ("Лондон", "Великобритания", "GMT Standard Time"),
        ("Лагос", "Нигерия", "W. Central Africa Standard Time"),
        ("Каир", "Египет", "Egypt Standard Time"),
        ("Москва", "Россия", "Russian Standard Time"),
        ("Дубай", "ОАЭ", "Arab Standard Time"),
        ("Карачи", "Пакистан", "Pakistan Standard Time"),
        ("Дакка", "Бангладеш", "Bangladesh Standard Time"),
        ("Джакарта", "Индонезия", "SE Asia Standard Time"),
        ("Шанхай", "Китай", "China Standard Time"),
        ("Токио", "Япония", "Tokyo Standard Time"),
        ("Сидней", "Австралия", "AUS Eastern Standard Time"),
        ("Порт-Вила", "Вануату", "Central Pacific Standard Time"),
        ("Окленд", "Новая Зеландия", "New Zealand Standard Time"),
        ("Апиа", "Самоа", "SamoaTime"),
        ("Киритимати", "Кирибати", "KiribatiTime")
    };

    // Кастомные часовые пояса
    private static readonly TimeZoneInfo SamoaTime = TimeZoneInfo.CreateCustomTimeZone("SamoaTime", TimeSpan.FromHours(13), "Апиа (Самоа)", "Апиа (Самоа)");
    private static readonly TimeZoneInfo KiribatiTime = TimeZoneInfo.CreateCustomTimeZone("KiribatiTime", TimeSpan.FromHours(14), "Киритимати (Кирибати)", "Киритимати (Кирибати)");

    static void Main()
    {
        // Запуск потока для вывода времени
        Thread output_Thread = new Thread(OutputTime);
        output_Thread.Start();

        // Запуск сервера
        StartNamedPipeServer();

        // Ожидание завершения потока вывода
        output_Thread.Join();
    }

    // Метод для вывода времени
    static void OutputTime()
    {
        while (is_Running)
        {
            try
            {
                if (is_Combined_Mode && combined_TimeZones.Length > 0)
                {
                    // Вывод времени для всех выбранных часовых поясов
                    string[] times = new string[combined_TimeZones.Length];
                    for (int i = 0; i < combined_TimeZones.Length; i++)
                    {
                        int index = combined_TimeZones[i];
                        string tz_id = TimeZones[index - 1].TimeZoneId;
                        string city = TimeZones[index - 1].City;
                        TimeZoneInfo tz = GetTimeZoneInfo(tz_id);
                        DateTime tz_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                        times[i] = $"{city}: {tz_time:yyyy-MM-dd HH:mm:ss}";
                    }
                    Console.WriteLine($"Текущее время: {string.Join(" | ", times)}");
                }
                else
                {
                    // Вывод времени для одного пояса
                    TimeZoneInfo time_zone = GetTimeZoneInfo(current_TimeZone_ID);
                    DateTime local_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, time_zone);
                    Console.WriteLine($"Текущее время ({current_TimeZone_ID}): {local_time:yyyy-MM-dd HH:mm:ss}");
                }
            }
            // Если часовой пояс не найден
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"Ошибка: Часовой пояс '{current_TimeZone_ID}' не найден. Используется Russian Standard Time.");
                current_TimeZone_ID = "Russian Standard Time";
            }
            Thread.Sleep(10000); // 10 секунд
        }
    }
    // Получение обьекта TimeZoneInfo по id
    static TimeZoneInfo GetTimeZoneInfo(string id)
    {
        // Обработка кастомных часовых поясов
        if (id == "SamoaTime") return SamoaTime;
        if (id == "KiribatiTime") return KiribatiTime;
        try
        {
            // Получение стандартного часового пояса
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            Console.WriteLine($"Ошибка: Часовой пояс '{id}' не найден. Возвращаемся к Russian Standard Time.");
            return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        }
    }

    // Метод для запуска сервера именованных каналов
    static void StartNamedPipeServer()
    {
        Console.WriteLine("Ожидание подключения клиента...");
        while (is_Running)
        {
            // Создание сервера
            using var pipe_Server = new NamedPipeServerStream("TimeZonePipe", PipeDirection.InOut);
            try
            {
                // Ожидание подключения
                pipe_Server.WaitForConnection();

                // Создание потоков для чтения/записи
                using var reader = new StreamReader(pipe_Server);
                using var writer = new StreamWriter(pipe_Server) { AutoFlush = true };

                // Обработка команд от клиента
                while (is_Running && pipe_Server.IsConnected)
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
                // Отключение клиента
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

    // Метод для обработки команд
    static bool ProcessCommand(string command, StreamWriter writer)
    {
        try
        {
            // Обработка команды changetimezone
            if (command == "changetimezone")
            {
                writer.WriteLine("Список доступных часовых поясов:");
                for (int i = 0; i < TimeZones.Length; i++)
                {
                    writer.WriteLine($"{i + 1}. {TimeZones[i].City} ({TimeZones[i].Country}) | {TimeZones[i].TimeZoneId}");
                }
                writer.WriteLine("Введите номер часового пояса:");
                is_Combined_Mode = false;
                combined_TimeZones = new int[0];
                return false;
            }

            // Обработка команды settimezone
            else if (command.StartsWith("settimezone:"))
            {
                string number_str = command.Substring(12).Trim();
                if (int.TryParse(number_str, out int index) && index >= 1 && index <= TimeZones.Length)
                {
                    string new_TimeZone = TimeZones[index - 1].TimeZoneId;
                    current_TimeZone_ID = new_TimeZone;
                    writer.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                    Console.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                    is_Combined_Mode = false;
                    combined_TimeZones = new int[0];
                }
                else
                {
                    writer.WriteLine($"Ошибка: Неверный номер часового пояса. Введите число от 1 до {TimeZones.Length}.");
                    Console.WriteLine($"Ошибка: Неверный номер часового пояса '{number_str}'.");
                }
                return true;
            }

            // Обработка команды timezone
            else if (command.StartsWith("timezone:"))
            {
                string new_TimeZone = command.Substring(9).Trim();
                current_TimeZone_ID = new_TimeZone;
                writer.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                Console.WriteLine($"Часовой пояс изменён на: {new_TimeZone}");
                is_Combined_Mode = false;
                combined_TimeZones = new int[0];
                return true;
            }

            // Обработка команды gettime
            else if (command == "gettime")
            {
                try
                {
                    // Получение текущего времени и выбранного часового пояса
                    TimeZoneInfo time_zone = GetTimeZoneInfo(current_TimeZone_ID);
                    DateTime local_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, time_zone);
                    writer.WriteLine($"Текущее время: {local_time:yyyy-MM-dd HH:mm:ss}, Часовой пояс: {current_TimeZone_ID}");
                    Console.WriteLine($"Клиент запросил время: {local_time:yyyy-MM-dd HH:mm:ss}, Часовой пояс: {current_TimeZone_ID}");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Обработка неверного часового пояса и возврат к UTC+3, который стоит по умолчанию
                    writer.WriteLine($"Ошибка: Часовой пояс '{current_TimeZone_ID}' не найден. Используется Russian Standard Time.");
                    Console.WriteLine($"Ошибка: Часовой пояс '{current_TimeZone_ID}' не найден.");
                    current_TimeZone_ID = "Russian Standard Time";
                }
                return true;
            }

            // Обработка команды combinetimezones
            else if (command.StartsWith("combinetimezones "))
            {
                // Извлечение списка номеров
                string numbers_str = command.Substring(16).Trim();
                // Проверка на наличие
                if (string.IsNullOrEmpty(numbers_str))
                {
                    writer.WriteLine("Ошибка: Укажите до 4 номеров часовых поясов, разделённых пробелами.");
                    Console.WriteLine("Ошибка: Не указаны номера часовых поясов для команды combinetimezones.");
                    return true;

                }

                // Разделение номеров на массив
                string[] number_parts = numbers_str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Проверка на лимит
                if (number_parts.Length > 4)
                {
                    writer.WriteLine("Ошибка: Можно указать не более 4 часовых поясов.");
                    Console.WriteLine("Ошибка: Указано более 4 часовых поясов.");
                    return true;
                }

                // Проверка и сохранение индексов часовых поясов
                int[] new_combined = new int[number_parts.Length];
                bool valid = true;
                for (int i = 0; i < number_parts.Length; i++)
                {
                    if (int.TryParse(number_parts[i], out int index) && index >= 1 && index <= TimeZones.Length)
                    {
                        new_combined[i] = index;
                    }
                    else
                    {
                        valid = false;
                        writer.WriteLine($"Ошибка: Неверный номер часового пояса '{number_parts[i]}'. Введите числа от 1 до {TimeZones.Length}.");
                        Console.WriteLine($"Ошибка: Неверный номер часового пояса '{number_parts[i]}'.");
                        break;
                    }
                }

                // Если данные валидны, включение режима комбинированного вывода
                if (valid)
                {
                    combined_TimeZones = new_combined;
                    is_Combined_Mode = true;
                    writer.WriteLine("Комбинированный вывод времени установлен для указанных часовых поясов.");
                    Console.WriteLine($"Комбинированный вывод времени установлен для: {string.Join(", ", new_combined.Select(i => TimeZones[i - 1].City))}");
                }
                return true;
            }

            // Обработка команды stop
            else if (command == "stop")
            {
                is_Running = false;
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
