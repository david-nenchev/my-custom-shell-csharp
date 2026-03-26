using System.Diagnostics;

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
            var argument = string.Join(" ", commandQuene);

            switch (command)
            {
                case "exit":
                    return;
                case "echo":
                    Console.WriteLine(string.Join(" ", commandQuene));
                    break;
                case "type":
                    if (shellCommands.Contains(argument))
                    {
                        Console.WriteLine($"{argument} is a shell builtin");
                    }
                    else
                    {
                        var executableMatch = false;
                        string root = Directory.GetCurrentDirectory();
                        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                        {
                            Console.WriteLine(file);
                        }

                        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var path in paths)
                        {
                            var fullPath = root + path;
                            var fileExists = File.Exists(fullPath);
                            if (fileExists)
                            {
                                var canExecute = CanExecute(fullPath);

                                if (canExecute)
                                {
                                    Console.WriteLine($"{argument} is {path}");
                                    executableMatch = true;
                                }
                            }
                        }

                        if (!executableMatch)
                        {
                            Console.WriteLine($"{argument}: not found");
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"{command}: command not found");
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
}
