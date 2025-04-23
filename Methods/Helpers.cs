using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Golvi1124.HabitLogger.Methods.Config;


namespace Golvi1124.HabitLogger.Methods;

public static class Helpers
{
    public static int GetNumber(string message)
    {
        Console.WriteLine(message);
        string numberInput = Console.ReadLine();

        if (numberInput == "0") return -1; // Special "go back" case
        int output = 0;

        while (!int.TryParse(numberInput, out output) || output < 0)
        {
            Console.WriteLine("\n\nInvalid number. Please try again!\n\n");
            numberInput = Console.ReadLine();
        }
        return output;

    }

    public static string GetDate(string message)
    {
        Console.WriteLine(message);
        string dateInput = Console.ReadLine();

        if (dateInput == "0") return "0"; // Special "go back" case


        while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Please try again!\n\n");
            dateInput = Console.ReadLine();
        }
        return dateInput;
    }
}
