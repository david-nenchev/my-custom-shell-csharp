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
                default:
                    break;
            }

            Console.WriteLine($"{command}: command not found");
        }

    }
}
