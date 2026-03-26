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

            var command = new Queue<string>(Console.ReadLine().Split([SPACE], StringSplitOptions.RemoveEmptyEntries));

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
                            ToOutput(string.Format(FILE_LOCATION_TEMPLATE, arguments, Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name)));
                        }
                        else
                        {
                            ToOutput(string.Format(NOT_FOUND_TEMPLATE, arguments));
                        }
                    }
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

                        using (Process process = Process.Start(startInfo)!)
                        {
                            process.WaitForExit();
                        }

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
