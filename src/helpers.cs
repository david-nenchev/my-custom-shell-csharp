using System.Diagnostics;
using System.Dynamic;
using System.Numerics;
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
        // Order matters: check longer operators first to avoid substring matches

        public static readonly string[] allRedirectOperators = { "2>>", "1>>", ">>", "2>", "1>", ">" };

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
        public static void ToOutput(string message, string? outputRedirect, bool isRedirectAppended)
        {
            // normal messages can be loged in file or console
            if (outputRedirect != null)
            {
                
                    // Write to file - add newline to match console behavior
                    if (isRedirectAppended)
                    {
                        File.AppendAllText(outputRedirect, message + Environment.NewLine);
                    }
                    else
                    {
                        File.WriteAllText(outputRedirect, message + Environment.NewLine);
                    }
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

        public static (string[], string?, string?) ParseInput(string input)
        {
            // Check operators in order (longer first) to avoid substring matches
            // Must check >> before >, 2>> before 2>, 1>> before 1>
            string[] operatorsToCheck = { "2>>", "1>>", ">>", "2>", "1>", ">" };

            foreach(var op in operatorsToCheck)
            {
                var index = input.IndexOf(op);
                if (index != -1)
                {
                    // For plain > or >>, make sure it's not preceded by 1 or 2
                    if ((op == ">" || op == ">>") && index > 0)
                    {
                        char prevChar = input[index - 1];
                        if (prevChar == '1' || prevChar == '2')
                        {
                            // This is actually 1> or 2>, not plain >
                            // Continue to check for next operator
                            continue;
                        }
                    }

                    var restOfCommand = input.Substring(0, index).Trim();
                    var redirect = input.Substring(index + op.Length).Trim();
                    return (ParseShellCommand(restOfCommand), NormalizePath(string.Join(string.Empty, ParseShellCommand(redirect))), op);
                }
            }

            return (ParseShellCommand(input), null, null);
        }

        private static string[] ParseShellCommand(string input)
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
