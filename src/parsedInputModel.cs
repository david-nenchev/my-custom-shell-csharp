

namespace codecrafters.models
{
    public class ParsedInputModel
    {
        public ParsedInputModel(string[] parsedInput, string? redirectLocation  = null, string? op = null)
        {
                this.ParsedInput = parsedInput;
                this.RedirectLocation = redirectLocation;
                this.Operator = op;
        }
        public string[] ParsedInput { get; set; }

        public string? RedirectLocation { get; set; }

        public string? Operator { get; set; }
    }
}
