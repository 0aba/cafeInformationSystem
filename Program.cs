using Avalonia;
using System;
using dotenv.net;
using System.Linq;
using cafeInformationSystem.Commands;

namespace cafeInformationSystem;

sealed class Program
{
    private static readonly string[] _COMMANDS = { "createadmin" };
    // TODO! перенести в .env
    private const bool _COMMANDS_ENABLE = true;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        DotEnv.Load();

        if (args.Length == 0) { }
        else if (args.Length > 0 && IsManagementCommand(args))
        {
            if (!_COMMANDS_ENABLE)
            {
                throw new Exception("Command disabled");
            }
            RunManagementCommand(args);
            return;
        } else
        {
            throw new Exception("Unknown arguments");
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static bool IsManagementCommand(string[] args)
    {
        if (args.Length < 2)
        {
            return false;
        }
        
        var option = args[0].ToLower();
        var command = args[1].ToLower();
        return option.Equals("-c") && _COMMANDS.Contains(command);
    }

    private static void RunManagementCommand(string[] args)
    {
        Console.WriteLine($"Executing command: {string.Join(" ", args)}");

        var command = args[1].ToLower();
        switch (command)
        {
            case "createadmin":
                new CreateAdministratorCommand().Execute();
                break;
            default:
                throw new Exception("Command not found");
        }
        Console.WriteLine("Command executed successfully");
    }
}
