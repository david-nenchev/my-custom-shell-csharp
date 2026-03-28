using System.Diagnostics;
using System.Text;
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
        public const string HOME_ENV_VAR = "HOME";
    }

    static class UtilityHelpers
    {
        public static void ToOutputError(string message)
        {
            // error messages are loged to console
            Console.WriteLine(message);
        }

        public static void ToOutput(string message, string? outputLocation)
        {
            // normal messages can be loged in file or console
            if (outputLocation != null)
            {
                File.WriteAllText(outputLocation, message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        public static string NormalizePath(string path)
        {
            if (path.Length > 1 && path.EndsWith(Path.DirectorySeparatorChar))
            {
                return path.TrimEnd(Path.DirectorySeparatorChar);
            }
            return path;
        }

        public static (string, string?) ParseOutputRedirect(string input)
        {
            if (input.Contains('>'))
            {
                string[] splittedInput = input.Split([">", "1>"], StringSplitOptions.None);
                var outputRedirect = string.Join(string.Empty, ParseShellCommand(splittedInput[1]));
                outputRedirect = NormalizePath(outputRedirect);
                return (splittedInput[0], outputRedirect);
            }

            return (input, null);
        }

        public static string[] ParseShellCommand(string input)
        {
            var args = new List<string>();
            var current = new StringBuilder();
            bool inSingleQuote = false;
            bool inDoubleQuote = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // Handle backslash escape (not inside single quotes)
                if (c == '\\' && !inSingleQuote && i + 1 < input.Length)
                {
                    i++; // skip the backslash
                    current.Append(input[i]); // add the next character as literal
                    continue;
                }

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote; // toggle single quote
                    continue; // skip quote itself
                }

                if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote; // toggle double quote
                    continue; // skip quote itself
                }

                if (char.IsWhiteSpace(c) && !inSingleQuote && !inDoubleQuote)
                {
                    // space outside quotes → finish current token if not empty
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Clear();
                    }
                    // else skip consecutive spaces
                }
                else
                {
                    current.Append(c);
                }
            }

            // Add last token
            if (current.Length > 0)
                args.Add(current.ToString());

            return args.ToArray();
        }
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
            var paths = GetEnvVariableValue(StringHelpers.PATH_ENV_VAR)?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

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

        public static string GetEnvVariableValue(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
