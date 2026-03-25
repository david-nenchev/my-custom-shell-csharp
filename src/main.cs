class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var command = Console.ReadLine();

            switch (command)
            {
                case "exit":
                    return;
                case "echo":
                    var restOfCommand = command.Split([" ", "echo"], StringSplitOptions.RemoveEmptyEntries) ?? [];
                    Console.WriteLine(restOfCommand);
                    break;
                default:
                    break;
            }



            Console.WriteLine($"{command}: command not found");
        }

    }
}
