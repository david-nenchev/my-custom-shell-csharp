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
            var (preParsedInput, outputRedirect) = ParseOutputRedirect(consoleInput);
            var parsedInput = ParseShellCommand(preParsedInput ?? string.Empty);
            var command = new Queue<string>(parsedInput);

            var firstLevelCommand = command.Dequeue();
            var arguments = string.Join(SPACE, command);

            switch (firstLevelCommand)
            {
                case EXIT:
                    return;
                case ECHO:
                    ToOutput(string.Join(SPACE, command), outputRedirect);
                    break;
                case TYPE:
                    if (shellCommands.Contains(arguments))
                    {
                        ToOutput(string.Format(SHELL_BUILTIN_TEMPLATE, arguments), outputRedirect);
                    }
                    else
                    {
                        var typeFileInfo = FindExecutable(arguments);

                        if (typeFileInfo != null)
                        {
                            var fullPath = Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name);
                            ToOutput(string.Format(FILE_LOCATION_TEMPLATE, arguments, fullPath), outputRedirect);
                        }
                        else
                        {
                            ToOutputError(string.Format(NOT_FOUND_TEMPLATE, arguments));
                        }
                    }
                    break;
                case PWD:
                    ToOutput(shellCurrentDirectory, outputRedirect);
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
                        ToOutputError(string.Format(CD_NO_SUCH_DIRECTORY_TEMPLATE, CD, newPath));
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
                            UseShellExecute = false,
                        };

                        foreach (var arg in command)
                        {
                            startInfo.ArgumentList.Add(arg);
                        }

                        using Process process = Process.Start(startInfo)!;
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            ToOutput(output.TrimEnd('\n', '\r'), outputRedirect);
                        }

                    }
                    else
                    {
                        ToOutputError(string.Format(COMMAND_NOT_FOUND_TEMPLATE, firstLevelCommand));
                    }
                    break;
            }
        }

    }
}
