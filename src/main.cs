using System.Diagnostics;
using static codecrafters.helpers.ShellCommands;
using static codecrafters.helpers.StringHelpers;
using static codecrafters.helpers.UtilityHelpers;
using static codecrafters.helpers.FileHelpers;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write(PROMPT);

            var consoleInput = Console.ReadLine()?.Trim();
            var parsedInput = consoleInput?.Split([SPACE], StringSplitOptions.RemoveEmptyEntries) ?? [];
            var command = new Queue<string>(parsedInput);

            var firstLevelCommand = command.Dequeue();
            var arguments = string.Join(SPACE, command);

            switch (firstLevelCommand)
            {
                case EXIT:
                    return;
                case ECHO:
                    ToOutput(string.Join(SPACE, command));
                    break;
                case TYPE:
                    if (shellCommands.Contains(arguments))
                    {
                        ToOutput(string.Format(SHELL_BUILTIN_TEMPLATE, arguments));
                    }
                    else
                    {
                        var typeFileInfo = FindExecutable(arguments);

                        if (typeFileInfo != null)
                        {
                            var fullPath = Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name);
                            ToOutput(string.Format(FILE_LOCATION_TEMPLATE, arguments, fullPath));
                        }
                        else
                        {
                            ToOutput(string.Format(NOT_FOUND_TEMPLATE, arguments));
                        }
                    }
                    break;
                case PWD:
                    ToOutput(Directory.GetCurrentDirectory());
                    break;
                default:
                    var fileInfo = FindExecutable(firstLevelCommand);

                    if (fileInfo != null)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = fileInfo.Name,
                            UseShellExecute = true,  // needed to open non-exe files too
                            Arguments = arguments
                        };

                        using Process process = Process.Start(startInfo)!;
                        process.WaitForExit();

                    }
                    else
                    {
                        ToOutput(string.Format(COMMAND_NOT_FOUND_TEMPLATE, firstLevelCommand));
                    }
                    break;
            }
        }

    }
}
