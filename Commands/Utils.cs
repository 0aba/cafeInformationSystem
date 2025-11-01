using System;

namespace cafeInformationSystem.Commands;

public static class UtilsCommands
{
    public static string GetInput(string fieldName, bool required)
    {
        while (true)
        {
            Console.Write($"{fieldName}: ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (!required || !string.IsNullOrEmpty(input))
            {
                return input;
            }

            Console.WriteLine($"Warning {fieldName} is required!");
        }
    }

    public static string GetPasswordWithConfirmation()
    {
        while (true)
        {
            Console.Write("Password: ");
            var password = ReadPassword();

            Console.Write("Confirm Password: ");
            var confirmPassword = ReadPassword();

            if (password == confirmPassword)
            {
                if (password.Length >= 8 && password.Length <= 128)
                {
                    return password;
                }
                else
                {
                    Console.WriteLine("Warning password must be between 8 and 128 characters!");
                }

            }
            else
            {
                Console.WriteLine("Warning passwords don't match!");
            }
        }
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}
