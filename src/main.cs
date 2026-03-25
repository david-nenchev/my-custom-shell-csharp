class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var commandQuene = new Queue<string>(Console.ReadLine().Split([" "], StringSplitOptions.RemoveEmptyEntries));
            string[] shellCommands = { "exit", "echo", "type" };
            //var command = parsedText.FirstOrDefault();
            //var arguments = string.Join(" ", parsedText[1..]);

            

            while(commandQuene.Count > 0)
            {
                var command = commandQuene.Dequeue();

                switch (command)
                {
                    case "exit":
                        return;
                    case "echo":
                        Console.WriteLine(string.Join(" ", commandQuene));
                        break;
                    case "type":
                        var argument = string.Join(" ", commandQuene);
                        if (shellCommands.Contains(argument))
                        {
                            Console.WriteLine($"{argument} is a shell builtin");
                        }
                        else
                        {
                            Console.WriteLine($"{argument}: not found");
                        }
                        break;
                    default:
                        Console.WriteLine($"{command}: command not found");
                        break;
                }
            }

         
        }

    }
}
