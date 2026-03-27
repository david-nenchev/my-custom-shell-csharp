using System.Diagnostics;
    namespace codecrafters.helpers
    {
        static class ShellCommands
        {
            public const string EXIT = "exit";
            public const string ECHO = "echo";
            public const string TYPE = "type";
            public const string PWD = "pwd";
            public const string CD = "cd";
            public static readonly string[] shellCommands = { EXIT, ECHO, TYPE, PWD };
        }

        static class StringHelpers
        {
            public const string SPACE = " ";
            public const string PROMPT = "$ ";
            public const string SHELL_BUILTIN_TEMPLATE = "{0} is a shell builtin";
            public const string FILE_LOCATION_TEMPLATE = "{0} is {1}";
            public const string NOT_FOUND_TEMPLATE = "{0}: not found";
            public const string COMMAND_NOT_FOUND_TEMPLATE = "{0}: command not found";
            public const string CD_NO_SUCH_DIRECTORY_TEMPLATE = "{0}: {1}: No such file or directory";
            public const string PATH_ENV_VAR = "PATH";
        }

        static class UtilityHelpers
        {
            public static void ToOutput(string message)
            {
                Console.WriteLine(message);
            }
        }

        static class Patterns
        {
            public const string PATH_VALIDATE_PATTERN = @"^(~(\/[A-Za-z0-9._-]+)*\/?|\/([A-Za-z0-9._-]+(\/[A-Za-z0-9._-]+)*)?\/?|\.{1,2}(\/[A-Za-z0-9._-]+)*\/?)$";
        }

        static class FileHelpers
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
                var paths = GetPathEnvVarValue()?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);


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

            public static string GetPathEnvVarValue()
            {
                return Environment.GetEnvironmentVariable(StringHelpers.PATH_ENV_VAR);
            }
        }
    }
