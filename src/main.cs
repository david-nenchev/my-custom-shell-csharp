using System.Diagnostics;
using static codecrafters.helpers.FileHelpers;
using static codecrafters.helpers.ShellCommands;
using static codecrafters.helpers.StringHelpers;
using static codecrafters.helpers.UtilityHelpers;

class Program
{
    static void Main()
    {
        var shellCurrentDirectory = Directory.GetCurrentDirectory();
        var homeDirectory = GetEnvVariableValue(HOME_ENV_VAR);

        while (true)
        {
            Console.Write(PROMPT);

            var consoleInput = Console.ReadLine()?.Trim();
            var parsedInput = ParseShellCommand(consoleInput ?? string.Empty);
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
                    ToOutput(shellCurrentDirectory);
                    break;
                case CD:
                    var newPath = arguments;

                    bool isAbsolute = Path.IsPathRooted(newPath);

                    if (newPath.Contains('~'))
                    {
                        newPath = newPath.Replace("~", homeDirectory);
                    }

                    if (!isAbsolute)
                    {
                        newPath = NormalizePath(Path.GetFullPath(newPath, shellCurrentDirectory));
                    }

                    var exists = Directory.Exists(newPath);
                    
                    if (!exists)
                    {
                        ToOutput(string.Format(CD_NO_SUCH_DIRECTORY_TEMPLATE, CD, newPath));
                    } 
                    else
                    {
                        shellCurrentDirectory = newPath;
                    }
                    break;
                default:
                    var fileInfo = FindExecutable(firstLevelCommand);

                    if (fileInfo != null)
                    {

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = fileInfo.Name,
                            UseShellExecute = false,  // needed to open non-exe files too
                        };

                        foreach (var arg in command)
                        {
                            startInfo.ArgumentList.Add(arg);
                        }

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
