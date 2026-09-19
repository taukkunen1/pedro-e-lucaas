using System;

namespace Core
{
    public static class AdvancedConsole
    {
        public static void Write(string text, ConsoleColor Color)
        {
            ConsoleColor initialColor = Console.ForegroundColor;
            Console.ForegroundColor = Color;
            Console.Write(text);
            Console.ForegroundColor = initialColor;
        }
        public static void WriteLine(string text, ConsoleColor Color)
        {
            ConsoleColor initialColor = Console.ForegroundColor;
            Console.ForegroundColor = Color;
            Console.WriteLine(text);
            Console.ForegroundColor = initialColor;
        }
    }
}
