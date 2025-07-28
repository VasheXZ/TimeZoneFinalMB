using System;
/// <summary>
/// Validates whether a given string represents a valid time zone number.
/// </summary>
/// <remarks>A valid time zone number is an integer between 1 and 26, inclusive. If the validation fails, an error
/// message is written to the console.</remarks>
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