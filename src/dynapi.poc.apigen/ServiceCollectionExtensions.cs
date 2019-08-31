﻿using dynapi.poc.apigen.Endpoints;
using Microsoft.AspNetCore.Http;
using wsDSQ;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        private static System.Xml.XmlDocument XmlServiceDocument = new System.Xml.XmlDocument();
        
        //Im the handled endpoint definition change me c:
        /*private static readonly EndpointCollection _registeredEndpoints = new EndpointCollection{
            new Endpoint("asd1", "POST", new PathString("/"),  typeof(EndpointHandler)),
            new Endpoint("asd1", "GET", new PathString("/asd1"),  typeof(EndpointHandler)),
            new Endpoint("asd1", "POST", new PathString("/asd1"),  typeof(EndpointHandler)),
            new Endpoint("asd2", "GET", new PathString("/asd2"),  typeof(EndpointHandler)),
            new Endpoint("genericInstructionExecutor", "POST", new PathString("/genericInstructionExecutor"),  typeof(EndpointHandler))
        };*/
        private static readonly EndpointCollection _registeredEndpoints = new EndpointCollection{
        };



        /// <summary>
        /// Just generates some endpoints
        /// We need to check a way to handle querystrings and route params
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection UseApiGenEndpoints(this IServiceCollection serviceCollection)
        {
            wsDSQ.wsDataServiceQuerySoapClient owsDSQ = new wsDSQ.wsDataServiceQuerySoapClient(wsDataServiceQuerySoapClient.EndpointConfiguration.wsDataServiceQuerySoap12);
            //XmlServiceDocument.Load(appPath + "\\XML\\services.xml");
            XmlServiceDocument.LoadXml (owsDSQ.getParameters());
            string verb = "";
            System.Xml.XmlNode AuxXmlNode;
            foreach (System.Xml.XmlNode oNode in XmlServiceDocument.SelectNodes("//ROOT/*"))
            {

                if ((AuxXmlNode = oNode.SelectSingleNode("@VERB"))!=null){
                    verb = AuxXmlNode.InnerText;
                }
                else {
                    verb = "POST";
                }

                _registeredEndpoints.Add(new Endpoint(oNode.Name , verb, new PathString("/"+ oNode.Name), typeof(EndpointHandler))); 
            }
                //As a singleton so we can add them in runtime
                serviceCollection.AddSingleton(_registeredEndpoints);
                        //Basic endpoint handler
            serviceCollection.AddTransient<EndpointHandler>();

            return serviceCollection;
        }
    }
}
