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
        public const string CD_NO_SUCH_DIRECTORY_TEMPLATE = "{0}: {1}: No such file or directoryy";
        public const string PATH_ENV_VAR = "PATH";
        public const string HOME_ENV_VAR = "HOME";
    }

    static class UtilityHelpers
    {
        public static void ToOutput(string message)
        {
            Console.WriteLine(message);
        }

        public static string NormalizePath(string path)
        {
            if (path.Length > 1 && path.EndsWith(Path.DirectorySeparatorChar))
            {
                return path.TrimEnd(Path.DirectorySeparatorChar);
            }
            return path;
        }

        public static string[] ParseShellCommand(string input)
        {
            var args = new List<string>();
            var current = new StringBuilder();
            bool inSingleQuote = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\'')
                {
                    inSingleQuote = !inSingleQuote; // toggle quote
                    continue; // skip quote itself
                }

                if (char.IsWhiteSpace(c) && !inSingleQuote)
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

            // ✅ Merge adjacent quoted tokens (already concatenated)
            // But also merge tokens separated by empty quotes
            // We can do a single pass since empty quotes were removed
            var mergedArgs = new List<string>();
            var merged = new StringBuilder();

            foreach (var arg in args)
            {
                if (merged.Length > 0)
                    merged.Append(arg); // merge with previous
                else
                    merged.Append(arg);

                // Only split if original had whitespace outside quotes
                mergedArgs.Add(merged.ToString());
                merged.Clear();
            }

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
