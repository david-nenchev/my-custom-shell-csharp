class Program
{
    static void Main()
    {
        var shellBuiltIn = "is a shell builtin";
        while (true)
        {
            Console.Write("$ ");

            var commandQuene = new Queue<string>(Console.ReadLine().Split([" "], StringSplitOptions.RemoveEmptyEntries));
            //var command = parsedText.FirstOrDefault();
            //var arguments = string.Join(" ", parsedText[1..]);
            string prevCommand = null;


            while (commandQuene.Count > 0)
            {
               
                var command = commandQuene.Dequeue();

                switch (command)
                {
                    case "exit":
                        if (prevCommand == "type")
                        {
                            Console.WriteLine($"{command} {shellBuiltIn}");
                            prevCommand = null;
                            break;
                        }
                        else
                        {
                            return;
                        }
                    case "echo":
                        if (prevCommand == "type")
                        {
                            Console.WriteLine($"{command} {shellBuiltIn}");
                            prevCommand = null;
                            break;
                        }
                        else
                        {
                            Console.WriteLine(string.Join(" ", commandQuene));
                            break;
                        }
                    case "type":
                        if (prevCommand == "type") 
                        {
                            Console.WriteLine($"{command} {shellBuiltIn}");
                            prevCommand = null;
                            break;
                        }
                        else
                        {
                            prevCommand = "type";
                            break;
                        }
                    default:
                        Console.WriteLine($"{command}: command not found");
                        break;
                }
            }

         
        }

    }
}
