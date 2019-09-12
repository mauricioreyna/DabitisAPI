using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using wsDSQ;
using System.Xml.Xsl;
using System.IO;
using System.Text.RegularExpressions;

namespace Dabitis.Contract.Manager
{
    public class SqlProccesMessage
    {

        //private string AssemblyDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:"+ Path.DirectorySeparatorChar, "");
        //private string AssemblyDirectory = GetApplicationRoot();
        private string AssemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public XmlDocument ProccesMessage(ref XmlDocument doc, ref HttpContext context)
        {
            XmlDocument AuxDoc = new XmlDocument();
			wsDSQ.wsDataServiceQuerySoapClient owsDSQ = null;
            string endPointName = context.Request.Path.ToString().Replace("/", "");
                                            

            try
			{
				
			    owsDSQ = new wsDSQ.wsDataServiceQuerySoapClient(wsDataServiceQuerySoapClient.EndpointConfiguration.wsDataServiceQuerySoap12);
                if (endPointName != "reservedGetApiCatalogEndPoint")
                {
                    AuxDoc.LoadXml("<ROOT>" +
                        "<RECORD>" +
                        "<SERVICE_NAME>" +
                        endPointName +
                        "</SERVICE_NAME>" +
                        doc.SelectSingleNode("//DATA").InnerXml.ToString() +
                        "</RECORD >" +
                        "</ROOT>"); ;
                    var sResult = owsDSQ.getDataMessage(AuxDoc.InnerXml);

                    /*oSqlSpExecute.logLevel ologLevel;
                    ologLevel = oSqlSpExecute.logLevel.high;
                    oSql.appLogLevel = ologLevel;
                    oSql.conectar();*/
                    //return (oSql.getXMLQuery(ref sXmlMessage));
                    AuxDoc.LoadXml(sResult);
                }
                else
                {
                    var sResult = owsDSQ.getParameters();
                    AuxDoc.Load(AssemblyDirectory + Path.DirectorySeparatorChar + "XSLParamertersToXmlJsonBase.xslt");
                    AuxDoc.LoadXml(TransformXML(sResult, AuxDoc.OuterXml.ToString() ));
                }

            }
			catch (System.Exception oExc)
			{
                AuxDoc.LoadXml("<NORESULT resultCode='-1' description='" + System.Security.SecurityElement.Escape(oExc.Message) + "'/>"); 
			}
			finally
			{
				
			}
            return AuxDoc;
        }
        public static string TransformXML(string inputXml, string xsltString)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
            {
                transform.Load(reader);
            }
            StringWriter results = new StringWriter();
            using (XmlReader reader = XmlReader.Create(new StringReader(inputXml)))
            {
                transform.Transform(reader, null, results);
            }
            return results.ToString();
        }
        
        static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }


    }
}
