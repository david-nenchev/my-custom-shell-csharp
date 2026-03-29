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
        string? outputRedirect = null;
        string? errorRedirect = null;
        var isRedirectAppended = false;

        while (true)
        {
            Console.Write(PROMPT);

            var consoleInput = Console.ReadLine()?.Trim();
            var (parsedInput, redirect, redirectOp) = ParseInput(consoleInput);

            switch (redirectOp)
            {
                case ">":
                case "1>":
                    outputRedirect = redirect;
                    break;
                case "2>":
                    errorRedirect = redirect;
                    break;
                case ">>":
                case "1>>":
                    outputRedirect = redirect;
                    isRedirectAppended = true;
                    break;
                case "2>>":
                    errorRedirect = redirect;
                    isRedirectAppended = true;
                    break;
            }

         
            var command = new Queue<string>(parsedInput);

            // Create redirect files if specified (even if empty) - but only for overwrite mode
            if (!isRedirectAppended)
            {
                if (errorRedirect != null)
                {
                    File.WriteAllText(errorRedirect, string.Empty);
                }
                if (outputRedirect != null)
                {
                    File.WriteAllText(outputRedirect, string.Empty);
                }
            }

            var firstLevelCommand = command.Dequeue();
            var arguments = string.Join(SPACE, command);

            switch (firstLevelCommand)
            {
                case EXIT:
                    return;
                case ECHO:
                    ToOutput(string.Join(SPACE, command), outputRedirect, isRedirectAppended);
                    break;
                case TYPE:
                    if (shellCommands.Contains(arguments))
                    {
                        ToOutput(string.Format(SHELL_BUILTIN_TEMPLATE, arguments), outputRedirect, isRedirectAppended);
                    }
                    else
                    {
                        var typeFileInfo = FindExecutable(arguments);

                        if (typeFileInfo != null)
                        {
                            var fullPath = Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name);
                            ToOutput(string.Format(FILE_LOCATION_TEMPLATE, arguments, fullPath), outputRedirect, isRedirectAppended);
                        }
                        else
                        {
                            ToOutput(string.Format(NOT_FOUND_TEMPLATE, arguments), errorRedirect, isRedirectAppended);
                        }
                    }
                    break;
                case PWD:
                    ToOutput(shellCurrentDirectory, outputRedirect, isRedirectAppended);
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
                        ToOutput(string.Format(CD_NO_SUCH_DIRECTORY_TEMPLATE, CD, newPath), errorRedirect, isRedirectAppended);
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
                            RedirectStandardOutput = true,
                            RedirectStandardError = errorRedirect != null, // redirect stderr if errorRedirect is set
                        };

                        foreach (var arg in command)
                        {
                            startInfo.ArgumentList.Add(arg);
                        }

                        using Process process = Process.Start(startInfo)!;
                        string output = process.StandardOutput.ReadToEnd();
                        string errorOutput = errorRedirect != null ? process.StandardError.ReadToEnd() : string.Empty;
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            // Remove trailing newline since ToOutput adds one
                            output = output.TrimEnd('\n', '\r');
                            ToOutput(output, outputRedirect, isRedirectAppended);
                        }

                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            // Remove trailing newline since ToOutput adds one
                            errorOutput = errorOutput.TrimEnd('\n', '\r');
                            ToOutput(errorOutput, errorRedirect, isRedirectAppended);
                        }

                    }
                    else
                    {
                        ToOutput(string.Format(COMMAND_NOT_FOUND_TEMPLATE, firstLevelCommand), errorRedirect, isRedirectAppended);
                    }
                    break;
            }
        }

    }
}
