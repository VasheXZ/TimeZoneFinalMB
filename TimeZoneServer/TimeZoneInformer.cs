using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZoneServer
{
    internal class TimeZoneInformer
    {
        public static readonly (string City, string Country, string TimeZoneId)[] TimeZones = new[]
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

        public static TimeZoneInfo GetTimeZoneInfo(string id)
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
    }
}
