using System.Diagnostics;
    namespace codecrafters.helpers
    {
        static class ShellCommands
        {
            public const string EXIT = "exit";
            public const string ECHO = "echo";
            public const string TYPE = "type";
            public static readonly string[] shellCommands = { EXIT, ECHO, TYPE };
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
        static class UtilityHelpers
        {
            public static void ToOutput(string message)
            {
                Console.WriteLine(message);
            }
        }

        public static class FileHelpers
        {
            public static bool CanExecute(string path)
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

            public static FileInfo FindExecutable(string name)
            {
                var paths = Environment.GetEnvironmentVariable(StringHelpers.PATH_ENV_VAR)?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);


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
    }
