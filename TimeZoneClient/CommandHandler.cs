using System;
/// <summary>
/// Предоставляет функциональность для обработки и выполнения пользовательских команд, связанных с операциями с часовыми поясами.
/// </summary>
/// <remarks>Класс <see cref="CommandHandler"/> читает пользовательский ввод в цикле, обрабатывает различные команды и взаимодействует с клиентом именованного канала для выполнения операций с часовыми поясами.
/// Поддерживаемые команды включают: <list
/// type="bullet"> 
/// <item><description><c>changetimezone</c>: Изменяет текущий часовой пояс.</description></item>
/// <item><description><c>settimezone &lt;номер&gt;</c>: Устанавливает часовой пояс по его числовому идентификатору.</description></item> 
/// <item><description><c>timezone &lt;ID&gt;</c>: Получает информацию о конкретном часовом поясе по его идентификатору.</description></item> 
/// <item><description><c>gettime</c>: Получает текущее время в настроенном часовом поясе.</description></item> 
/// <item><description><c>combinetimezones &lt;номер1&gt; &lt;номер2&gt; ...</c>: Объединяет до четырех часовых поясов по их числовым идентификаторам.</description></item>
/// <item><description><c>stop</c>: Останавливает текущую операцию.</description></item> 
/// <item><description><c>exit</c>: Выходит из командного цикла.</description></item> 
/// </list> Неверные или неподдерживаемые команды перенаправляются клиенту именованного канала для дальнейшей обработки.</remarks>
class CommandHandler
{
    public static void Run()
    {
        Console.WriteLine("Команды: changetimezone, settimezone <номер>, timezone <ID>, gettime, combinetimezones <номер1> <номер2> ... (до 4), stop, exit");
        while (true)
        {
            Console.Write("Введите команду: ");
            string input = Console.ReadLine();

            
            if (input == "exit")
                break;

            
            if (input == "changetimezone")
            {
                NamedPipeClient.HandleChangeTimeZone();
            }
            
            else if (input.StartsWith("settimezone "))
            {
                string number = input.Substring("settimezone ".Length).Trim();
                if (TimeZoneValidator.IsValidTimeZoneNumber(number, out int _))
                {
                    NamedPipeClient.SendCommand("settimezone:" + number);
                }
            }
            
            else if (input.StartsWith("timezone "))
            {
                string id = input.Substring("timezone ".Length).Trim();
                NamedPipeClient.SendCommand("timezone:" + id);
            }
            else if (input == "gettime")
            {
                NamedPipeClient.SendCommand("gettime");
            }
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
                            NamedPipeClient.SendCommand("combinetimezones " + numbers);
                        }
                    }
                }
            }
            else
            {
                NamedPipeClient.SendCommand(input);
            }
        }
    }
}