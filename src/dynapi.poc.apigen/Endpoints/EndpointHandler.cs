using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Dabitis.Contract;



namespace dynapi.poc.apigen.Endpoints
{
    /// <summary>
    /// A default handler, just returns what is recieved by the request if it is a json req
    /// </summary>
    internal class EndpointHandler : IEndpointHandler
    {
        #region Constants

        private const string JsonContent = "application/json";
        private Dabitis.Contract.Manager.SqlProccesMessage _SqlProccesMessage = new Dabitis.Contract.Manager.SqlProccesMessage();
        //private static string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        #endregion

        #region Worker process

        public async Task<object> ProcessAsync(HttpContext context)
        {
            object parsedContent = null;
           

            if (context.Request.ContentType?.ToLowerInvariant() == JsonContent)
            {
                var content = string.Empty;
                using (var streamReader = new StreamReader(context.Request.Body))
                {
                    content = await streamReader.ReadToEndAsync();
                }

                /**********************************/

                XmlDocument doc = JsonConvert.DeserializeXmlNode(content);
                var result = JsonConvert.SerializeXmlNode(_SqlProccesMessage.ProccesMessage(ref doc, ref context));

                /**********************************/

                //Some examples
                parsedContent = JsonConvert.DeserializeObject(result);
                //parsedContent = JsonConvert.DeserializeObject(content);
                //var parsedContent2 = JsonConvert.DeserializeObject<dynamic>(content);
                //var parsedContent3 = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            }

            return AssambleResponse(context, parsedContent);
        }

        private Task AssambleResponse(HttpContext context, object result)
        {
            if (result is null)
            {
                // 204 : No content
                context.Response.StatusCode = 204;
                return Task.CompletedTask;
            }
            else
            {
                // 200 : Ok
                context.Response.StatusCode = 200;
                context.Response.ContentType = JsonContent;

                return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
        } 

        #endregion
    }
}
