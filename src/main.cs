using System;
using System.Diagnostics;
using System.Reflection;

class Program
{
    static void Main()
    {


        while (true)
        {
            Console.Write("$ ");

            var commandQuene = new Queue<string>(Console.ReadLine().Split([" "], StringSplitOptions.RemoveEmptyEntries));
            string[] shellCommands = { "exit", "echo", "type" };



            var command = commandQuene.Dequeue();
            var arguments = string.Join(" ", commandQuene);

            switch (command)
            {
                case "exit":
                    return;
                case "echo":
                    Console.WriteLine(string.Join(" ", commandQuene));
                    break;
                case "type":
                    if (shellCommands.Contains(arguments))
                    {
                        Console.WriteLine($"{arguments} is a shell builtin");
                    }
                    else
                    {
                        var executableTypePath = FindExecutable(arguments);

                        if (executableTypePath != null)
                        {
                            Console.WriteLine($"{arguments} is {executableTypePath}");
                        }
                        else
                        {
                            Console.WriteLine($"{arguments}: not found");
                        }
                    }
                    break;
                default:

                    var executablePath = FindExecutable(command);
                    Console.WriteLine(executablePath);

                    if (executablePath != null)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = command,
                            Arguments = command + arguments,  // pass the arguments here
                            UseShellExecute = true,  // needed to open non-exe files too
                            WorkingDirectory = executablePath
                        };

                        Process process = Process.Start(startInfo);
                    }
                    else
                    {
                        Console.WriteLine($"{command}: command not found");
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

    static string? FindExecutable(string name)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
       

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, name);
            var fileExists = File.Exists(fullPath);
            if (fileExists)
            {
                var canExecute = CanExecute(fullPath);

                if (canExecute)
                {
                    return path;
                }
            }
        }

        return null;
    }
}
