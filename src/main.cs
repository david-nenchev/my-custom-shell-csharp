using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using static ShellCommands;
using static StringHelpers;


static class ShellCommands
{
    public const string EXIT = "exit";
    public const string ECHO = "echo";
    public const string TYPE = "type";
}

static class StringHelpers
{
    public const string SPACE = " ";
    public const string PROMPT = "$ ";
    public const string SHELL_BUILTIN_TEMPLATE = "{0} is a shell builtin";
    public const string FILE_LOCATION_TEMPLATE = "{0} is {1}";
    public const string NOT_FOUND_TEMPLATE = "{0}: not found";
    public const string COMMAND_NOT_FOUND_TEMPLATE = "{0}: command not found";
    public const string PATH_ENV_VAR = "PATH";
}

class Program
{
    static void Main()
    {


        while (true)
        {
            Console.Write(PROMPT);

            var commandQuene = new Queue<string>(Console.ReadLine().Split([SPACE], StringSplitOptions.RemoveEmptyEntries));
            string[] shellCommands = { EXIT, ECHO, TYPE };

            var command = commandQuene.Dequeue();
            var arguments = string.Join(SPACE, commandQuene);

            switch (command)
            {
                case EXIT:
                    return;
                case ECHO:
                    Console.WriteLine(string.Join(SPACE, commandQuene));
                    break;
                case TYPE:
                    if (shellCommands.Contains(arguments))
                    {
                        Console.WriteLine(string.Format(SHELL_BUILTIN_TEMPLATE, arguments));
                    }
                    else
                    {
                        var typeFileInfo = FindExecutable(arguments);

                        if (typeFileInfo != null)
                        {
                            Console.WriteLine(string.Format(FILE_LOCATION_TEMPLATE, arguments, Path.Combine(typeFileInfo.Directory.FullName, typeFileInfo.Name)));
                        }
                        else
                        {
                            Console.WriteLine(string.Format(NOT_FOUND_TEMPLATE, arguments));
                        }
                    }
                    break;
                default:

                    var fileInfo = FindExecutable(command);

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
                        Console.WriteLine(string.Format(COMMAND_NOT_FOUND_TEMPLATE, command));
                    }
                    break;
            }
        }

    }

    static bool CanExecute(string path)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.Kill(); // stop immediately
            return true;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false; // no permission or not executable
        }
        catch
        {
            return false;
        }
    }

    static FileInfo FindExecutable(string name)
    {
        var paths = Environment.GetEnvironmentVariable(PATH_ENV_VAR)?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
       

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, name);
            var fileExists = File.Exists(fullPath);
            if (fileExists)
            {
                var canExecute = CanExecute(fullPath);

                if (canExecute)
                {
                    
                    return new FileInfo(fullPath);
                }
            }
        }

        return null;
    }
}
