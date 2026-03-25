class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var command = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries) ?? [];

            foreach (var commandItem in command)
            {
                switch (commandItem)
                {
                    case "exit":
                        return;
                    default:
                        break;
                }
            }

      

            Console.WriteLine($"{command}: command not found");
        }

    }
}
