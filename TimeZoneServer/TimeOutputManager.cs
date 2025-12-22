using System;
using System.Threading;
/// <summary>
/// Управляет выводом текущего времени для указанного часового пояса или комбинации часовых поясов.
/// </summary>
/// <remarks>Этот класс предоставляет функциональность для отображения текущего времени в указанном часовом поясе или нескольких часовых поясах в комбинированном режиме. 
/// Время периодически обновляется во время работы менеджера. 
/// Часовой пояс по умолчанию - "Russian Standard Time". Поведение вывода времени контролируется следующими статическими полями: <list
/// type="bullet"> 
/// <item> <description><see cref="current_TimeZone_ID"/>: Определяет идентификатор часового пояса для режима одного часового пояса.</description> </item> 
/// <item> <description><see cref="is_Running"/>: Указывает, активен ли процесс вывода времени.</description> </item> 
/// <item> <description><see cref="combined_TimeZones"/>: Определяет индексы часовых поясов для отображения в комбинированном режиме.</description> </item> 
/// <item> <description><see cref="is_Combined_Mode"/>: Определяет, работает ли менеджер в комбинированном режиме.</description> </item> 
/// </list></remarks>
class TimeOutputManager
{
    public static string current_TimeZone_ID = "Russian Standard Time";
    public static bool is_Running = true;
    public static int[] combined_TimeZones = new int[0];
    public static bool is_Combined_Mode = false;

    public static void OutputTime()
    {
        while (is_Running)
        {
            try
            {
                if (is_Combined_Mode && combined_TimeZones.Length > 0)
                {
                    string[] times = new string[combined_TimeZones.Length];
                    for (int i = 0; i < combined_TimeZones.Length; i++)
                    {
                        int index = combined_TimeZones[i];
                        string tz_id = TimeZoneInformer.TimeZones[index - 1].TimeZoneId;
                        string city = TimeZoneInformer.TimeZones[index - 1].City;
                        var tz = TimeZoneInformer.GetTimeZoneInfo(tz_id);
                        DateTime tz_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                        times[i] = $"{city}: {tz_time:yyyy-MM-dd HH:mm:ss}";
                    }
                    Console.WriteLine($"Текущее время: {string.Join(" | ", times)}");
                }
                else
                {
                    var time_zone = TimeZoneInformer.GetTimeZoneInfo(current_TimeZone_ID);
                    DateTime local_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, time_zone);
                    Console.WriteLine($"Текущее время ({current_TimeZone_ID}): {local_time:yyyy-MM-dd HH:mm:ss}");
                }
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"Ошибка: Часовой пояс '{current_TimeZone_ID}' не найден. Используется Russian Standard Time.");
                current_TimeZone_ID = "Russian Standard Time";
            }
            Thread.Sleep(10000);
        }
    }
}