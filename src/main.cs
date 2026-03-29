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

            var consoleInput = ReadUserInput()?.Trim();
            var parsedInputModel = ParseInput(consoleInput);
         
            var command = new Queue<string>(parsedInputModel.ParsedInput);

            // Create/handle redirect files
            if (!parsedInputModel.IsRedirectAppended)
            {
                // Overwrite mode: create empty file (truncate if exists)
                if (parsedInputModel.ErrorRedirect != null)
                {
                    File.WriteAllText(parsedInputModel.ErrorRedirect, string.Empty);
                }
                if (parsedInputModel.OutputRedirect != null)
                {
                    File.WriteAllText(parsedInputModel.OutputRedirect, string.Empty);
                }
            }
            else
            {
                // Append mode: create empty file only if it doesn't exist
                if (parsedInputModel.ErrorRedirect != null && !File.Exists(parsedInputModel.ErrorRedirect))
                {
                    File.WriteAllText(parsedInputModel.ErrorRedirect, string.Empty);
                }
                if (parsedInputModel.OutputRedirect != null && !File.Exists(parsedInputModel.OutputRedirect))
                {
                    File.WriteAllText(parsedInputModel.OutputRedirect, string.Empty);
                }
            }

            var firstLevelCommand = command.Dequeue();
            var arguments = string.Join(SPACE, command);

            switch (firstLevelCommand)
            {
                case EXIT:
                    return;
                case ECHO:
                    ToOutput(string.Join(SPACE, command), parsedInputModel.OutputRedirect, parsedInputModel.IsRedirectAppended);
                    break;
                case TYPE:
                    if (shellCommands.Contains(arguments))
                    {
                        ToOutput(string.Format(SHELL_BUILTIN_TEMPLATE, arguments), parsedInputModel.OutputRedirect, parsedInputModel.IsRedirectAppended);
                    }
                    else
                    {
                        var typeFileInfo = FindExecutable(arguments);

                        if (typeFileInfo != null)
                        {
                            var fullPath = Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name);
                            ToOutput(string.Format(FILE_LOCATION_TEMPLATE, arguments, fullPath), parsedInputModel.OutputRedirect, parsedInputModel.IsRedirectAppended);
                        }
                        else
                        {
                            ToOutput(string.Format(NOT_FOUND_TEMPLATE, arguments), parsedInputModel.ErrorRedirect, parsedInputModel.IsRedirectAppended);
                        }
                    }
                    break;
                case PWD:
                    ToOutput(shellCurrentDirectory, parsedInputModel.OutputRedirect, parsedInputModel.IsRedirectAppended);
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
                        ToOutput(string.Format(CD_NO_SUCH_DIRECTORY_TEMPLATE, CD, newPath), parsedInputModel.ErrorRedirect, parsedInputModel.IsRedirectAppended);
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
                            RedirectStandardError = parsedInputModel.ErrorRedirect != null, // redirect stderr if errorRedirect is set
                        };

                        foreach (var arg in command)
                        {
                            startInfo.ArgumentList.Add(arg);
                        }

                        using Process process = Process.Start(startInfo)!;
                        string output = process.StandardOutput.ReadToEnd();
                        string errorOutput = parsedInputModel.ErrorRedirect != null ? process.StandardError.ReadToEnd() : string.Empty;
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            // Remove trailing newline since ToOutput adds one
                            output = output.TrimEnd('\n', '\r');
                            ToOutput(output, parsedInputModel.OutputRedirect, parsedInputModel.IsRedirectAppended);
                        }

                        if (!string.IsNullOrEmpty(errorOutput)) 
                        {
                            // Remove trailing newline since ToOutput adds one
                            errorOutput = errorOutput.TrimEnd('\n', '\r');
                            ToOutput(errorOutput, parsedInputModel.ErrorRedirect, parsedInputModel.IsRedirectAppended);
                        }

                    }
                    else
                    {
                        ToOutput(string.Format(COMMAND_NOT_FOUND_TEMPLATE, firstLevelCommand), parsedInputModel.ErrorRedirect, parsedInputModel.IsRedirectAppended);
                    }
                    break;
            }
        }

    }
}
