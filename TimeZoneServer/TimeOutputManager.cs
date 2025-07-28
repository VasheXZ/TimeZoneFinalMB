using System;
using System.Threading;
/// <summary>
/// Manages the output of the current time for a specified time zone or a combination of time zones.
/// </summary>
/// <remarks>This class provides functionality to display the current time in a specified time zone or multiple
/// time zones in combined mode. The time is updated periodically while the manager is running. The default time zone is
/// "Russian Standard Time".  The behavior of the time output is controlled by the following static fields: <list
/// type="bullet"> <item> <description><see cref="current_TimeZone_ID"/>: Specifies the time zone ID for single time
/// zone mode.</description> </item> <item> <description><see cref="is_Running"/>: Indicates whether the time output
/// process is active.</description> </item> <item> <description><see cref="combined_TimeZones"/>: Specifies the indices
/// of time zones to display in combined mode.</description> </item> <item> <description><see cref="is_Combined_Mode"/>:
/// Determines whether the manager operates in combined mode.</description> </item> </list></remarks>
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