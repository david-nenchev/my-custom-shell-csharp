class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var parsedText = Console.ReadLine().Split([" "], StringSplitOptions.RemoveEmptyEntries);
            var command = parsedText.FirstOrDefault();
            var arguments = string.Join(" ", parsedText[1..]);

            switch (command)
            {
                case "exit":
                    return;
                case "echo":
                    Console.WriteLine(arguments);
                    break;
                default:
                    Console.WriteLine($"{command}: command not found");
                    break;
            }
        }

    }
}
