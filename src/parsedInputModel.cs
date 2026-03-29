

namespace codecrafters.models
{
    public class ParsedInputModel
    {
        public ParsedInputModel(string[] parsedInput)
        {
                this.ParsedInput = parsedInput;
        }

        public string[] ParsedInput { get; set; }

        public string? OutputRedirect { get; set; }

        public string? ErrorRedirect { get; set; }

        public bool IsRedirectAppended { get; set; }
    }
}
