using System;
/// <summary>
/// Provides functionality to handle and process user commands related to time zone operations.
/// </summary>
/// <remarks>The <see cref="CommandHandler"/> class reads user input in a loop, processes various commands,  and
/// interacts with a named pipe client to execute time zone-related operations. Supported commands include: <list
/// type="bullet"> <item><description><c>changetimezone</c>: Changes the current time zone.</description></item>
/// <item><description><c>settimezone &lt;number&gt;</c>: Sets the time zone by its numeric
/// identifier.</description></item> <item><description><c>timezone &lt;ID&gt;</c>: Retrieves information about a
/// specific time zone by its ID.</description></item> <item><description><c>gettime</c>: Retrieves the current time in
/// the configured time zone.</description></item> <item><description><c>combinetimezones &lt;number1&gt;
/// &lt;number2&gt; ...</c>: Combines up to four time zones by their numeric identifiers.</description></item>
/// <item><description><c>stop</c>: Stops the current operation.</description></item> <item><description><c>exit</c>:
/// Exits the command loop.</description></item> </list> Invalid or unsupported commands are forwarded to the named pipe
/// client for further handling.</remarks>
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