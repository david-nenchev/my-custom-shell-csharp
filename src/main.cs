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
