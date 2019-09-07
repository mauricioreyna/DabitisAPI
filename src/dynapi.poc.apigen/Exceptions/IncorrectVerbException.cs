namespace dynapi.poc.apigen.Exceptions
{
    public class IncorrectVerbException : DomainException
    {
        public IncorrectVerbException(string verb, string uri)
            : base($"Url '{uri}' does not accept '{verb}' as a verb", 405)
        {
        }
    }
}
