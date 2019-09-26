using dynapi.poc.apigen.Endpoints;
using Microsoft.AspNetCore.Http;
using System.IO;
//using wsDSQ;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        //private static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        private static System.Xml.XmlDocument XmlServiceDocument = new System.Xml.XmlDocument();
        private static string AssemblyDirectory = GetApplicationRoot();
        //Im the handled endpoint definition change me c:
        /*private static readonly EndpointCollection _registeredEndpoints = new EndpointCollection{
            new Endpoint("asd1", "POST", new PathString("/"),  typeof(EndpointHandler)),
            new Endpoint("asd1", "GET", new PathString("/asd1"),  typeof(EndpointHandler)),
            new Endpoint("asd1", "POST", new PathString("/asd1"),  typeof(EndpointHandler)),
            new Endpoint("asd2", "GET", new PathString("/asd2"),  typeof(EndpointHandler)),
            new Endpoint("genericInstructionExecutor", "POST", new PathString("/genericInstructionExecutor"),  typeof(EndpointHandler))
        };*/
        static string GetApplicationRoot()
        {
            var appRoot = "";
            //if (System.Runtime.InteropServices.RuntimeInformation
            //                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            //{
            appRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            /*}
            else
            {
                appRoot = System.Reflection.Assembly.GetEntryAssembly().Location;
            }*/


            return appRoot;
        }
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
            //wsDSQ.wsDataServiceQuerySoapClient owsDSQ = new wsDSQ.wsDataServiceQuerySoapClient(wsDataServiceQuerySoapClient.EndpointConfiguration.wsDataServiceQuerySoap12);
            
            //XmlServiceDocument.LoadXml (owsDSQ.getParameters());
            XmlServiceDocument.Load (AssemblyDirectory + Path.DirectorySeparatorChar + "services.xml");
            string verb = "";
            System.Xml.XmlNode AuxXmlNode;
            _registeredEndpoints.Add(new Endpoint("reservedGetApiCatalogEndPoint", "GET", new PathString("/reservedGetApiCatalogEndPoint"), typeof(EndpointHandler)));
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
