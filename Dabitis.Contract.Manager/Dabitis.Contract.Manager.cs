using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Xml;
using Microsoft.AspNetCore.Http;
//using SqlDataManager;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text;
using wsDSQ;


namespace Dabitis.Contract.Manager
{
    public class SqlProccesMessage
    {
        public XmlDocument ProccesMessage(ref XmlDocument doc, ref HttpContext context)
        {
            //SqlDataManager.oSqlSpExecute oSql = null;
            
            //String sPath = Server.MapPath(".");
            XmlDocument AuxDoc = new XmlDocument();
            //AuxDoc.Load(appServiceFileName);
            //oSql = new oSqlSpExecute();
            //oSql.appServiceFileName = appServiceFileName;
            //oSql.appLogFileName = AuxDoc.SelectSingleNode("//@logPath").InnerText + "\\Dabitis" + DateTime.Now.ToString("YYYYMMdd")+ ".log";
            //wsDataServiceQuerySoapClient.EndpointConfiguration epc = new wsDataServiceQuerySoapClient.EndpointConfiguration();

            wsDSQ.wsDataServiceQuerySoapClient owsDSQ = new wsDSQ.wsDataServiceQuerySoapClient(wsDataServiceQuerySoapClient.EndpointConfiguration.wsDataServiceQuerySoap12);
            
            AuxDoc.LoadXml("<ROOT>" +
                "<RECORD>" +
                "<SERVICE_NAME>" + 
                context.Request.Path.ToString().Replace("/", "") +
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
            return AuxDoc; 
        }

         
    }
}
