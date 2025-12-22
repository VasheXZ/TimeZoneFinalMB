using System;
/// <summary>
/// Проверяет, представляет ли заданная строка допустимый номер часового пояса.
/// </summary>
/// <remarks>Допустимый номер часового пояса - это целое число от 1 до 26 включительно. Если проверка не проходит, сообщение об ошибке выводится в консоль.</remarks>
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