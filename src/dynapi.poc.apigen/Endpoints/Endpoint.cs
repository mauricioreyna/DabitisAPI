using Microsoft.AspNetCore.Http;
using System;

namespace dynapi.poc.apigen.Endpoints
{
    public class Endpoint
    {
        public Endpoint(string name, string verb, PathString path, Type handler)
        {
            Name = name;
            Verb = verb;
            Path = path;
            Handler = handler;
        }

        public string Name { get; set; }

        public string Verb { get; set; }

        public PathString Path { get; set; }

        public Type Handler { get; set; }
    }
}
