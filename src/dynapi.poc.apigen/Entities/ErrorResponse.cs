namespace dynapi.poc.apigen.Entities
{
    public class ErrorResponse
    {
        public ErrorResponse(string name, string message)
        {
            Name = name;
            Message = message;
        }

        public string Name { get; }
        public string Message { get; }
    }
}
