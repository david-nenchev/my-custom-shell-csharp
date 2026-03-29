using codecrafters.models;
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
        public static readonly string[] shellCommands = { EXIT, ECHO, TYPE, PWD , CD };
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
        public const string BELL = "\x07";
    }

    static class UtilityHelpers
    {
        public static string ReadUserInput()
        {
            StringBuilder sb = new StringBuilder();
            ConsoleKey lastKey = ConsoleKey.NoName;
            string lastTabInput = string.Empty;

            while (true)
            {
                var keyInfo = Console.ReadKey(true); // true = don't echo to console

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(); // Move to next line
                    return sb.ToString();
                }
                else if (keyInfo.Key == ConsoleKey.Tab)
                {
                    // Tab completion for commands
                    var currentInput = sb.ToString();
                    var words = currentInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length <= 1)
                    {
                        // Complete the first word (command)
                        var partialCommand = words.Length == 1 ? words[0] : currentInput;
                        var allMatches = new List<string>();

                        // Collect builtin commands that match
                        foreach (var command in ShellCommands.shellCommands)
                        {
                            if (command.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                allMatches.Add(command);
                            }
                        }

                        // Collect external executables that match
                        var matchingExecutables = FileHelpers.FindMatchingExecutables(partialCommand);
                        allMatches.AddRange(matchingExecutables);

                        if (allMatches.Count == 1)
                        {
                            // Single match: complete it
                            var completedCommand = allMatches[0];
                            Console.Write("\r" + StringHelpers.PROMPT + new string(' ', sb.Length) + "\r" + StringHelpers.PROMPT);
                            sb.Clear();
                            sb.Append(completedCommand);
                            sb.Append(' '); // Add space to buffer
                            Console.Write(completedCommand + ' '); // Write command with trailing space
                        }
                        else if (allMatches.Count > 1)
                        {
                            // Multiple matches
                            if (lastKey == ConsoleKey.Tab && lastTabInput == currentInput)
                            {
                                // Second consecutive tab: show all matches
                                Console.WriteLine(); // Move to new line
                                allMatches.Sort(); // Alphabetical order
                                Console.WriteLine(string.Join("  ", allMatches)); // Two spaces between items
                                Console.Write(StringHelpers.PROMPT + currentInput); // Redisplay prompt with input
                            }
                            else
                            {
                                // First tab: ring bell
                                Console.Write(StringHelpers.BELL);
                                lastTabInput = currentInput;
                            }
                        }
                        else
                        {
                            // No matches: ring bell
                            Console.Write(StringHelpers.BELL);
                        }
                    }

                    lastKey = ConsoleKey.Tab;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    // Handle backspace
                    if (sb.Length > 0)
                    {
                        sb.Length--; // Remove last character
                        Console.Write("\b \b"); // Move back, write space, move back again
                    }
                    lastKey = ConsoleKey.Backspace;
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Regular printable character
                    sb.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                    lastKey = keyInfo.Key;
                }
                // Ignore all other keys (arrows, delete, etc.)
            }
        }

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

        public static ParsedInputModel ParseInput(string input)
        {
            // Check operators in order (longer first) to avoid substring matches
            foreach(var op in ShellCommands.allRedirectOperators)
            {
                if (input.Contains(op))
                {
                    var index = input.IndexOf(op);
                    var restOfCommand = input.Substring(0, index);
                    var redirect = input.Substring(index + op.Length).Trim();
                    var parsedInputModel = new ParsedInputModel(ParseShellCommand(restOfCommand));

                    switch (op)
                    {
                        case ">":
                        case "1>":
                            parsedInputModel.OutputRedirect = redirect;
                            break;
                        case "2>":
                            parsedInputModel.ErrorRedirect = redirect;
                            break;
                        case ">>":
                        case "1>>":
                            parsedInputModel.OutputRedirect = redirect;
                            parsedInputModel.IsRedirectAppended = true;
                            break;
                        case "2>>":
                            parsedInputModel.ErrorRedirect = redirect;
                            parsedInputModel.IsRedirectAppended = true;
                            break;
                    }

                    return parsedInputModel;
                }
            }

            return new ParsedInputModel (ParseShellCommand(input));
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

        public static List<string> FindMatchingExecutables(string partialName)
        {
            var matches = new List<string>();
            var paths = GetEnvVariableValue(StringHelpers.PATH_ENV_VAR)?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (paths == null) return matches;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) continue;

                try
                {
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        if (fileName.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (CanExecute(file))
                            {
                                matches.Add(fileName);
                            }
                        }
                    }
                }
                catch
                {
                    // Skip directories we can't access
                }
            }

            return matches;
        }
    }
}
