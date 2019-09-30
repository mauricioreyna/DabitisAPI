//using System.Xml.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using wsDSQ;
using Dabitis.Sql.DataManager;


namespace Dabitis.Contract.Manager
{
    public class SqlProccesMessage
    {

        //private string AssemblyDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:"+ Path.DirectorySeparatorChar, "");
        private string AssemblyDirectory = GetApplicationRoot();
        //private string AssemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public XmlDocument ProccesMessage(ref XmlDocument doc, ref HttpContext context)
        {
			oSqlSpExecute oSql = null;
			string endPointName = context.Request.Path.ToString().Replace("/", "");
			string sXmlMessage = "";
			XmlDocument AuxDoc = new XmlDocument();

			try
			{

				if (endPointName != "reservedGetApiCatalogEndPoint")
				{
                    sXmlMessage = "<ROOT>" +
                    "<RECORD";
                    foreach (XmlAttribute AuxXmlAttribute in doc.SelectSingleNode("//DATA").Attributes)
                    {
                        sXmlMessage += " " + AuxXmlAttribute.Name + "='" + AuxXmlAttribute.Value + "' ";

                    }
                    sXmlMessage += ">" +
					"<SERVICE_NAME>" +
					endPointName +
					"</SERVICE_NAME>" +
					doc.SelectSingleNode("//DATA").InnerXml.ToString() +
					"</RECORD >" +
					"</ROOT>"; 
					oSqlSpExecute.logLevel ologLevel;
					oSql = new oSqlSpExecute();
					oSql.appServiceFileName = AssemblyDirectory + Path.DirectorySeparatorChar + "services.xml";
					oSql.appLogFileName = AssemblyDirectory  + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "wsDSQ" + System.DateTime.Now.ToString("yyyyMMdd").ToString()  + ".log";
                    ologLevel = oSqlSpExecute.logLevel.high;
                    oSql.appLogLevel = ologLevel;
                    oSql.conectar();
                    var sResult = oSql.getXMLQuery(ref sXmlMessage);

					/*oSqlSpExecute.logLevel ologLevel;
					ologLevel = oSqlSpExecute.logLevel.high;
					oSql.appLogLevel = ologLevel;
					oSql.conectar();*/
					//return (oSql.getXMLQuery(ref sXmlMessage));
					AuxDoc.LoadXml(sResult);
				}
                else
                {
                    AuxDoc.Load(AssemblyDirectory + Path.DirectorySeparatorChar + "services.xml");
                    var sResult = AuxDoc.OuterXml.ToString();
                    AuxDoc.Load(AssemblyDirectory + Path.DirectorySeparatorChar + "XSLParamertersToXmlJsonBase.xslt");
                    AuxDoc.LoadXml(TransformXML(sResult, AuxDoc.OuterXml.ToString()));
                }
			
			}
            catch (System.Exception oExc)
            {
                AuxDoc.LoadXml("<NORESULT resultCode='-1' description='" + System.Security.SecurityElement.Escape(oExc.Message) + "'/>");
            }
            finally
            {
				if (oSql != null)
				{
					oSql.desconectar();
					oSql.Dispose();
				}
            }
            return AuxDoc;
           
            
        }
		public XmlDocument ProccesMessageWS(ref XmlDocument doc, ref HttpContext context)
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
                    AuxDoc.LoadXml(TransformXML(sResult, AuxDoc.OuterXml.ToString()));
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


    }
}
