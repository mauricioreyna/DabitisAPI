using System;

namespace dynapi.poc.apigen.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }
    }
}
