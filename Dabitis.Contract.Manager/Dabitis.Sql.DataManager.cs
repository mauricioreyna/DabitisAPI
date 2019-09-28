using HandlerException;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
namespace Dabitis.Sql.DataManager
{
    /// <summary>
    /// Dabitis.Sql.DataManager es una librería de mensajería que realiza operaciones sobre base de datos MSSQL y Sybase a través de llamados a procedimietos almacenados(Stored Procedures)
    /// </summary>
    /// <remarks>
	/// Dabitis.Sql.DataManager utiliza archivos de configuración donde cada mensaje tiene asociado un procedimiento almancenado.
	/// <para>
    /// Dabitis.Sql.DataManager utiliza llamados a procedimientos almacenados para obtener los datos del modelo, por lo cual posee acceso directo a los datos que necesita manipular y solo necesita enviar sus resultados de regreso al usuario, deshaciéndose de la sobrecarga resultante de comunicar grandes cantidades de datos salientes y entrantes..
	/// </para>
    /// <para>Cada mensaje posee cuerpo y estructura propia donde su nombre determinará el mensaje a consumir. Se encuentra definido por una estructura XML de árbol con diversos parámetros que determinarán su comportamiento.</para>
	/// <list type="table">Servicio
	///     <listheader>
	///         <term>Atributos</term><description>Descripción</description>
	///     </listheader>
	///     <item><term>REPORT_TEMPLATE (Opcional)</term><description>Determina si el mensaje posee una plantilla de reporte asociada. La misma debe estar contenida en la carpeta XSL ubicada en la raíz del sitio.[Este atributo está asociado a la solución "Data Service Query"] </description></item>
	///     <item><term>SP_NAME </term><description>Determina el procedimiento almacenado asociado al mensaje. Ej: Nombre de base..[nombre del procedimiento almacenado].</description></item>
    ///     <item><term>PARAMETER\VALUES(Opcional)</term><description>Es una colección que contiene elementos de tipo VALUE. Dichos elementos conforman en la capa de presentación los datos presentados como un comboBox [Este atributo está asociado a la solución "Data Service Query"]</description></item>
    ///     <item><term>PARAMETER\VALUES\VALUE(Opcional)</term><description>Representa cada uno de los elementos a presentar en un comboBox. Sus atributos INNERTEXT y OUTERTEXT representan el valor y la descripción a visualizar respectivamente[Este atributo está asociado a la solución "Data Service Query"]</description></item>
	/// </list>
	/// <para>A través de un conjunto de nodos denominado PARAMETERS_LIST/PARAMETER establece una relación uno a uno entre cada PARAMETER y cada parámetro del procedimiento almancenado. Cada PARAMETER posee los siguientes atributos:</para>
	/// <list type="table">Servicio
	///     <listheader>
	///         <term>Atributos</term><description>Descripción</description>
	///     </listheader>
	///     <item><term>TYPE</term>
	///			<description>Determina el tipo de dato asociado al parámetro. Sus valores posibles son:
	///			<list  type="bullet">
	///				<item><term>INT     </term></item>
	///				<item><term>NUMERIC </term></item>
	///				<item><term>MONEY   </term></item>
	///				<item><term>CHAR    </term></item>
	///				<item><term>VARCHAR </term></item>
	///				<item><term>DATE    </term></item>
	///				<item><term>DATETIME</term></item>
	///			</list>
	///			</description>
	///		</item>
	///     <item><term>FIELD</term><description>Determina el campo del procedimiento almacenado asociado.</description></item>
	///     <item><term>DEFAULT (Opcional)</term><description>Determina el valor por default en caso de que el nodo no se enviado.</description></item>
	///     <item><term>TYPEMODE (Opcional)</term><description>Determina si el nodo corresponde a un parámetro de salida o entrada en el procedimiento almacenado. Su valores posibles son 1 y 0 (Verdadero o Falso), siendo su valor por defecto 0.</description></item>
	///     <item><term>PRESICION (Opcional)</term><description>En el caso de una variable de tipo (TYPE) CHAR o VARCHAR determinará el largo de la cadena. Si la variable es de tipo (TYPE) NUMERIC especificará el máximo número de dígitos que el motor de base de datos podrá trabajar o almacenar. Si bien es opcional se recomienda utilizar este parámetro cuando se trabaje con variables de salida (TYPEMODE="1") de los procedimientos.</description></item>
	///     <item><term>NOTNULLABLE o NOTNULL (Opcional) </term><description>Determina,  cuando el nodo sea de tipo (TYPE) VARCHAR, si éste puede contener un valor vacío (valor=""), sus valores posibles son 1 y 0 (Verdadero o Falso) siendo su valor por defecto 0.</description></item>
	/// </list>
	/// <para>El nodo COMMENT determina los cometarios asociados al mensaje. Este nodo permite la introducción de código HTML puro en caso de considerarlo necesario. [Este nodo está asociado a la solución "Data Service Query"]</para>
    /// <example>Mensaje
    /// <code>
	///<![CDATA[
	///<EMPLOYEE_SALES_BY_COUNTRY SP_NAME="Northwind..[employee sales by country]" REPORT_TEMPLATE="service_tabla.xsl">
	///	<PARAMETERS_LIST>
	///		<PARAMETER TYPE="DATE" FIELD="Beginning_Date" DEFAULT="28/08/1996">FECHA_DE_COMIENZO</PARAMETER>
	///		<PARAMETER TYPE="DATE" FIELD="Ending_Date"    DEFAULT="16/04/1998">FECHA_DE_FIN</PARAMETER>
	///		<PARAMETER TYPE="VARCHAR" TYPEMODE="1" FIELD="DateReport" PRESICION="16">DATEREPORT</PARAMETER>
	///	</PARAMETERS_LIST>
	///	<COMMENT>
	///		Este servicio provee información referente a las ventas de los empleados por país.<br/>
	///		Posee dos parámetros de entrada:<br/>
	///		<img src="imagenes/param.gif"/><b>FECHA_DE_COMIENZO</b><br/>
	///		<img src="imagenes/param.gif"/><b>FECHA_DE_FIN</b><br/>
	///		FECHA_DE_FIN nunca debe superior a cuatros meses respecto a FECHA_DE_COMIENZO.<br/>
	///	</COMMENT>
	///</EMPLOYEE_SALES_BY_COUNTRY>
	///]]>
    /// </code>
    /// </example>
    /// </remarks>
    /// <example>Construcción
    /// <code>
    /// Dabitis.Sql.DataManager.oSqlSpExecute oSql = null;
    /// oSql = new oSqlSpExecute();
    /// oSql.appServiceFileName = "miArchivoDeConfiguracion.xml";
    /// oSql.conectar();
    /// </code>
    /// </example>
	/// <seealso cref="oSqlSpExecute.getXMLQuery(ref string)"/>
    /// 
    public class oSqlSpExecute : IDisposable
    {
        #region property and members
        /// <summary>
        //private OleDbTransaction _OleDbTransaction = null;
        //private stateTransaction _stateTransaction = stateTransaction.none;
        private bool Disposed = false;
        private Dictionary<string, string> _oDataTypeDictionary = new Dictionary<string, string>();
        private IOLog _IOLog = null;
        private logLevel _llevel = logLevel.high;
        private oException _oException = null;
        private SqlCommand _SqlCommand = null;
        private SqlConnection _SqlConnection = null;
        private SqlDataAdapter _SqlDataAdapter = null;
        private string _appDirectory = "";
        private string _appDirectoryTemplate = "";
        private string _appLogFileName = "";
        private string _appServiceFileName = "";
        private string _LastSQLStatement = "";
        private string _sStep = "";
        private string appDir = System.Environment.SystemDirectory.ToString();
        private string AssemblyDirectory = GetApplicationRoot();
        private XmlDocument _XmlDocument = null;
        private XmlDocument _XmlServiceDocument = null;
        /// Obtiene el estado de la transacción actual.
        /// </summary>
        /*public stateTransaction stateTransactionDb
        {
            get
            {
                return (this._stateTransaction);
|
            }
        }*/

        /// <summary>
        /// Determina si la conexión esta abierta.
        /// </summary>
		/// <value>Analiza el estado de la propiedad ConnectionState</value>
        public bool isConnected
        {
            get
            {
                return (this._SqlConnection != null && this._SqlConnection.State == ConnectionState.Open);

            }
        }
        /// <summary>
        /// Obtiene o asigna la carpeta de plantillas XSLT. Por defecto toma la ruta Application Path + "\xsl"
        /// </summary>*/
        public string appDirectoryTemplate
        {
            get
            {
                if (this._appDirectoryTemplate == "")
                {
                    this._appDirectoryTemplate = this.appDirectory;// + Path.DirectorySeparatorChar + "xsl";
                }

                return (this._appDirectoryTemplate);

            }
            set
            {
                this._appDirectoryTemplate = value;
            }
        }
        /// <summary>
        /// Obtiene la carpeta de Aplicación
        /// </summary>
        public string appDirectory
        {
            get
            {
                DirectoryInfo di = null;
                if (this._appDirectory == "")
                {
                    di = new DirectoryInfo(this._appServiceFileName);
                    this._appDirectory = di.Parent.FullName;
                    di = null;
                }

                return (this._appDirectory);

            }
        }
        /// <summary>
        /// Obtiene o asigna el archivo de log.
		/// <para>El archivo de log es una archivo donde se escribe y lista las acciones que van ocurriendo dentro de los procesos del aplicativo</para>
        /// </summary>
        /// <seealso cref="oSqlSpExecute.getXMLQuery(ref string)"/>
        public string appLogFileName
        {
            get
            {
                if (this._appLogFileName == "")
                    this._appLogFileName = appDir + Path.DirectorySeparatorChar + "sdm" + getDataFormatDate() + ".log";
                return this._appLogFileName;
            }
            set
            {
                this._appLogFileName = value;
            }
        }
        /// <summary>Define el nivel de log de escritura en disco</summary>
        /// <seealso cref="oSqlSpExecute.getXMLQuery(ref string)"/>
        public logLevel appLogLevel
        {
            get
            {
                return this._llevel;
            }
            set
            {
                this._llevel = value;
            }
        }
        /// <summary>
        /// Asigna u obtiene el valor del archivo de servicios.
        /// Si se crea el objeto sin parámetro busca por default en el directorio de sistema del SO el archivo de parametrización %system%\sdmServices.xml.
        /// </summary>
        public string appServiceFileName
        {
            get
            {
                if (this._appServiceFileName == "")
                    this._appServiceFileName = appDir + Path.DirectorySeparatorChar + "sdmServices.xml";
                return this._appServiceFileName;
            }
            set
            {
                if (isConnected)
                    throw new oSqlSpExecuteException("No se puede cambiar origen de servicios. Conexión ya abierta");
                _appServiceFileName = value;
            }
        }
        private string sStep
        {
            get
            {
                if (this._sStep == "")
                    this._sStep = "0";
                else
                    this._sStep = Convert.ToString(Convert.ToDouble(this._sStep) + 1);

                return this._sStep;
            }
        }
        /// <summary>Representa los estados posibles de una transacción</summary>
        /*public enum stateTransaction
        {
            /// <summary>
            /// No existe ninguna transacción abierta
            /// </summary>
            none = 0,
            /// <summary>
            /// Existe una transacción abierta
            /// </summary>
            open = 1
        }*/
        /// <summary>La enumeración logLevel define el nivel de escritura en el archivo de log.</summary>
        public enum logLevel
        {
            /// <summary>
            /// No escribe en log.
            /// </summary>
            none = 0,
            /// <summary>
            /// Escribe acciones tales como archivo de conexión utilizado, directorio de plantillas XSLT, Conexión establecida.
            /// </summary>
            low = 1,
            /// <summary>
            /// Suma al nivel low el servicio determinado.
            /// </summary>
            medium = 2,
            /// <summary>
            /// Escribe toda la sucesión de asiganciones en el mensaje y la llamada al motor.
            /// </summary>
            high = 3
        }
        #endregion//property and members
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #region constructors
        /// <summary>
        /// Crea una nueva instancia de la clase
        /// </summary>
        /// <param name="fileNameServiceParameters">Ruta y nombre del archivo de conexión</param>
        public oSqlSpExecute(string fileNameServiceParameters)
        {
            appServiceFileName = fileNameServiceParameters;
            loadDataType();
        }
        /// <summary>
        /// Crea una nueva instancia de la clase
        /// </summary>
        public oSqlSpExecute()
        {
            loadDataType();
        }
        #endregion//constructors
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #region dispose
        /// <summary>
        /// Liberar todos los recursos de su propiedad. También libera todos los recursos que posean sus tipos base; para ello, llama al método Dispose de su tipo primario.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    if (this._IOLog != null)
                    {
                        if (this._llevel > logLevel.medium)
                            this._IOLog.recordLogLine("Finalizando instancia [~]");
                        this._IOLog.Dispose();
                    }
                    if (this.isConnected)
                        this._SqlConnection.Close();
                    if (this._XmlServiceDocument != null)
                        this._XmlServiceDocument = null;
                    if (this._oException != null)
                        this._oException.Dispose();
                    //free any managed resources
                }

                //free unmanaged resources
            }

            Disposed = true;
        }
        /// <summary>
        /// Destructor de la instancia
        /// </summary>
        ~oSqlSpExecute()
        {
            Dispose(false);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #endregion//dispose
        #region getData
        /// <summary>
        /// Recibe un mensaje XML de tipo Dabitis.Sql.DataManager y ejecuta su instrucción.
        /// </summary>
        /// <param name="strXML">Mensaje Dabitis.Sql.DataManager
        /// <example>Mensaje tipo Dabitis.Sql.DataManager
        /// <code>
        /// <![CDATA[
        /// <?xml version="1.0" encoding="iso-8859-1"?>                   
        /// <ROOT>                                                        
        ///	<RECORD>                                                  
        ///		<SERVICE_NAME>EMPLOYEE_SALES_BY_COUNTRY</SERVICE_NAME>
        ///		<REPORT_TEMPLATE>service_tabla.xsl</REPORT_TEMPLATE>  
        ///		<FECHA_DE_COMIENZO>28/08/1996</FECHA_DE_COMIENZO>     
        ///		<FECHA_DE_FIN>16/04/1998</FECHA_DE_FIN>               
        ///		<DATEREPORT/>                                         
        ///	</RECORD>                                                 
        /// </ROOT>                                                       
        /// ]]>
        /// </code>
        /// </example>        
        /// <example>
        /// <para>En el siguiente ejemplo de código se produce un ciclo completo de conexión y obtención de datos. Utiliza los siguientes métodos y propiedades:
        ///			<list  type="bullet">
        ///				<item><term>oSqlSpExecute.Dispose()</term></item>
        ///				<item><term>oSqlSpExecute.appLogLevel   </term></item>
        ///				<item><term>oSqlSpExecute.appServiceFileName </term></item>
        ///				<item><term>oSqlSpExecute.conectar()    </term></item>
        ///				<item><term>oSqlSpExecute.desconectar()    </term></item>
        ///				<item><term>oSqlSpExecute.getXMLQuery()</term></item>
        ///				<item><term>oSqlSpExecute.oSqlSpExecute()</term></item>
        ///			</list>		
        /// </para>
        /// <code>
        /// public string getDataMessage(string sXmlMessage)                                                                                          
        /// {                                                                                                                                         
        ///     Dabitis.Sql.DataManager.oSqlSpExecute oSql = null;                                                                                             
        ///     String sPath = Server.MapPath(".");                                                                                                   
        ///     oSqlSpExecute.logLevel ologLevel;                                                                                                     
        ///     try                                                                                                                                   
        ///     {                                                                                                                                     
        ///         oSql = new oSqlSpExecute();                                                                                                       
        ///         oSql.appServiceFileName = sPath + Path.DirectorySeparatorChar + "parametria\\services.xml";                                                                   
        ///         oSql.appLogFileName = System.Configuration.ConfigurationManager.AppSettings["PathLog"] + Path.DirectorySeparatorChar + "wsDSQ" + getDataFormatDate() + ".log";
        ///                                                                                                                                           
        ///                                                                                                                                           
        ///         //Determino el nivel de log                                                                                                       
        ///         switch (System.Configuration.ConfigurationManager.AppSettings["logLevel"])                                                        
        ///         {                                                                                                                                 
        ///             case "0":                                                                                                                     
        ///                 ologLevel = oSqlSpExecute.logLevel.none;                                                                                  
        ///                 break;                                                                                                                    
        ///             case "1":                                                                                                                     
        ///                 ologLevel = oSqlSpExecute.logLevel.low;                                                                                   
        ///                 break;                                                                                                                    
        ///             case "2":                                                                                                                     
        ///                 ologLevel = oSqlSpExecute.logLevel.medium;                                                                                
        ///                 break;                                                                                                                    
        ///             case "3":                                                                                                                     
        ///                 ologLevel = oSqlSpExecute.logLevel.high;                                                                                  
        ///                 break;                                                                                                                    
        ///             default:                                                                                                                      
        ///                 ologLevel = oSqlSpExecute.logLevel.high;                                                                                  
        ///                 break;                                                                                                                    
        ///         }                                                                                                                                 
        ///                                                                                                                                           
        ///         oSql.appLogLevel =  ologLevel;                                                                                                    
        ///         oSql.conectar();                                                                                                                  
        ///         return(oSql.getXMLQuery(ref sXmlMessage));                                                                                        
        ///                                                                                                                                           
        ///     }                                                                                                                                     
        ///     catch (Exception ex)                                                                                                                  
        ///     {                                                                                                                                     
        ///         throw ex;                                                                                                                         
        ///         //throw ownSoapException(ex.Message, ex.Data.ToString()   ,"http://www.mauricioreyna.com.ar/");                                   
        ///     }                                                                                                                                     
        ///     finally                                                                                                                               
        ///     {                                                                                                                                     
        ///         if (oSql != null)                                                                                                                 
        ///             oSql.desconectar();                                                                                                           
        ///         oSql.Dispose();                                                                                                                   
        ///                                                                                                                                           
        ///     }                                                                                                                                     
        ///                                                                                                                                           
        /// }                                                                                                                                         
        /// </code>
        /// </example>
        /// </param>
        /// <exception cref=" Dabitis.Sql.DataManager.oSqlSpExecuteException">Por errores de conexión</exception>
        /// <exception cref="System.Exception">Por errores de negocio de mensajería</exception>
        /// <returns>
        /// <para>Respuesta De Datos</para>
        /// <para>Dabitis.Sql.DataManager realiza los mapeos según se realicen estos desde el procedimiento almacenado,  generando una estructura de tipo  "TABLE"  por cada uno  de estos dentro del mensaje de respuesta. Si existieran parámetros de salida dentro de dicho procedimiento se generará una estructura de tipo "TABLE" la cual contendrá dichos parámetros. La misma será fácilmente identificable ya que posee el atributo "TYPE" con valor igual a "OUTPUT".
        /// </para>		
        /// <para><![CDATA[<TABLES resultCode="0" service_name="EMPLOYEE_SALES_BY_COUNTRY">]]></para>
        /// <para><![CDATA[<TABLE TYPE="OUTPUT">                                           ]]></para>
        /// <para><![CDATA[	<RECORD>                                                    ]]></para>
        /// <para><![CDATA[		<DATEREPORT>24/02/2009</DATEREPORT>                     ]]></para>
        /// <para><![CDATA[	</RECORD>                                                   ]]></para>
        /// <para><![CDATA[</TABLE>                                                        ]]></para>
        /// <para><![CDATA[<TABLE>                                                         ]]></para>
        /// <para><![CDATA[	<RECORD>                                                    ]]></para>
        /// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
        /// <para><![CDATA[		<LASTNAME>Fuller</LASTNAME>                             ]]></para>
        /// <para><![CDATA[		<FIRSTNAME>Andrew</FIRSTNAME>                           ]]></para>
        /// <para><![CDATA[		<SHIPPEDDATE>1996-09-12T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
        /// <para><![CDATA[		<ORDERID>10280</ORDERID>                                ]]></para>
        /// <para><![CDATA[		<SALEAMOUNT>613.2</SALEAMOUNT>                          ]]></para>
        /// <para><![CDATA[	</RECORD>                                                   ]]></para>
        /// <para><![CDATA[	<RECORD>                                                    ]]></para>
        /// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
        /// <para><![CDATA[		<LASTNAME>Callahan</LASTNAME>                           ]]></para>
        /// <para><![CDATA[		<FIRSTNAME>Laura</FIRSTNAME>                            ]]></para>
        /// <para><![CDATA[		<SHIPPEDDATE>1996-08-30T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
        /// <para><![CDATA[		<ORDERID>10286</ORDERID>                                ]]></para>
        /// <para><![CDATA[		<SALEAMOUNT>3016</SALEAMOUNT>                           ]]></para>
        /// <para><![CDATA[	</RECORD>                                                   ]]></para>
        /// <para><![CDATA[	<RECORD>                                                    ]]></para>
        /// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
        /// <para><![CDATA[		<LASTNAME>Callahan</LASTNAME>                           ]]></para>
        /// <para><![CDATA[		<FIRSTNAME>Laura</FIRSTNAME>                            ]]></para>
        /// <para><![CDATA[		<SHIPPEDDATE>1996-08-28T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
        /// <para><![CDATA[		<ORDERID>10287</ORDERID>                                ]]></para>
        /// <para><![CDATA[		<SALEAMOUNT>819</SALEAMOUNT>                            ]]></para>
        /// <para><![CDATA[	</RECORD>                                                   ]]></para>
        /// <para><![CDATA[</TABLE>                                                        ]]></para>
        /// <para><![CDATA[</TABLES>                                                       ]]></para>
        /// </returns>
        public string getXMLQuery(ref string strXML)
        {
            string pRootName = "";
            return getXMLQuery(ref strXML, ref pRootName);
        }
        public string getXMLQuery(ref oSqlMessage SqlMessage)
        {
            string strXML = SqlMessage.getServiceDocument().OuterXml;
            string pRootName = "";
            return getXMLQuery(ref strXML, ref pRootName);
        }
        /// <summary>
        /// Recibe un mensaje XML de tipo Dabitis.Sql.DataManager y ejecuta su instrucción.
        /// </summary>
        /// <param name="strXML">Mensaje Dabitis.Sql.DataManager
        /// <example>Mensaje tipo Dabitis.Sql.DataManager
        /// <code>
		/// <![CDATA[
		/// <?xml version="1.0" encoding="iso-8859-1"?>                   
		/// <ROOT>                                                        
		///	<RECORD>                                                  
		///		<SERVICE_NAME>EMPLOYEE_SALES_BY_COUNTRY</SERVICE_NAME>
		///		<REPORT_TEMPLATE>service_tabla.xsl</REPORT_TEMPLATE>  
		///		<FECHA_DE_COMIENZO>28/08/1996</FECHA_DE_COMIENZO>     
		///		<FECHA_DE_FIN>16/04/1998</FECHA_DE_FIN>               
		///		<DATEREPORT/>                                         
		///	</RECORD>                                                 
		/// </ROOT>                                                       
		/// ]]>
        /// </code>
        /// </example>
		/// <example>
        /// <para>En el siguiente ejemplo de código se produce un ciclo completo de conexión y obtención de datos. Utiliza los siguientes métodos y propiedades:
		///			<list  type="bullet">
		///				<item><term>oSqlSpExecute.Dispose()</term></item>
		///				<item><term>oSqlSpExecute.appLogLevel   </term></item>
		///				<item><term>oSqlSpExecute.appServiceFileName </term></item>
		///				<item><term>oSqlSpExecute.conectar()    </term></item>
		///				<item><term>oSqlSpExecute.desconectar()    </term></item>
		///				<item><term>oSqlSpExecute.getXMLQuery()</term></item>
		///				<item><term>oSqlSpExecute.oSqlSpExecute()</term></item>
		///			</list>		
		/// </para>
		/// <code>
		/// public string getDataMessage(string sXmlMessage)                                                                                          
		/// {                                                                                                                                         
		///     Dabitis.Sql.DataManager.oSqlSpExecute oSql = null;                                                                                             
		///     String sPath = Server.MapPath(".");                                                                                                   
		///     oSqlSpExecute.logLevel ologLevel;                                                                                                     
		///     try                                                                                                                                   
		///     {                                                                                                                                     
		///         oSql = new oSqlSpExecute();                                                                                                       
		///         oSql.appServiceFileName = sPath + Path.DirectorySeparatorChar + "parametria\\services.xml";                                                                   
		///         oSql.appLogFileName = System.Configuration.ConfigurationManager.AppSettings["PathLog"] + Path.DirectorySeparatorChar + "wsDSQ" + getDataFormatDate() + ".log";
		///                                                                                                                                           
		///                                                                                                                                           
		///         //Determino el nivel de log                                                                                                       
		///         switch (System.Configuration.ConfigurationManager.AppSettings["logLevel"])                                                        
		///         {                                                                                                                                 
		///             case "0":                                                                                                                     
		///                 ologLevel = oSqlSpExecute.logLevel.none;                                                                                  
		///                 break;                                                                                                                    
		///             case "1":                                                                                                                     
		///                 ologLevel = oSqlSpExecute.logLevel.low;                                                                                   
		///                 break;                                                                                                                    
		///             case "2":                                                                                                                     
		///                 ologLevel = oSqlSpExecute.logLevel.medium;                                                                                
		///                 break;                                                                                                                    
		///             case "3":                                                                                                                     
		///                 ologLevel = oSqlSpExecute.logLevel.high;                                                                                  
		///                 break;                                                                                                                    
		///             default:                                                                                                                      
		///                 ologLevel = oSqlSpExecute.logLevel.high;                                                                                  
		///                 break;                                                                                                                    
		///         }                                                                                                                                 
		///                                                                                                                                           
		///         oSql.appLogLevel =  ologLevel;                                                                                                    
		///         oSql.conectar();                                                                                                                  
		///         return(oSql.getXMLQuery(ref sXmlMessage));                                                                                        
		///                                                                                                                                           
		///     }                                                                                                                                     
		///     catch (Exception ex)                                                                                                                  
		///     {                                                                                                                                     
		///         throw ex;                                                                                                                         
		///         //throw ownSoapException(ex.Message, ex.Data.ToString()   ,"http://www.mauricioreyna.com.ar/");                                   
		///     }                                                                                                                                     
		///     finally                                                                                                                               
		///     {                                                                                                                                     
		///         if (oSql != null)                                                                                                                 
		///             oSql.desconectar();                                                                                                           
		///         oSql.Dispose();                                                                                                                   
		///                                                                                                                                           
		///     }                                                                                                                                     
		///                                                                                                                                           
		/// }                                                                                                                                         
        /// </code>
        /// </example>
        /// </param>
		/// <param name="pRootName">Indica la raíz del parámetro de retorno.</param>
        /// <exception cref=" Dabitis.Sql.DataManager.oSqlSpExecuteException">Por errores de conexión</exception>
        /// <exception cref="System.Exception">Por errores de negocio de mensajería</exception>
        /// <returns>
        /// <para>Respuesta De Datos</para>
		/// <para>Dabitis.Sql.DataManager realiza los mapeos según se realicen estos desde el procedimiento almacenado,  generando una estructura de tipo  "TABLE"  por cada uno  de estos dentro del mensaje de respuesta. Si existieran parámetros de salida dentro de dicho procedimiento se generará una estructura de tipo "TABLE" la cual contendrá dichos parámetros. La misma será fácilmente identificable ya que posee el atributo "TYPE" con valor igual a "OUTPUT".
		/// </para>		
		/// <para><![CDATA[<TABLES resultCode="0" service_name="EMPLOYEE_SALES_BY_COUNTRY">]]></para>
		/// <para><![CDATA[<TABLE TYPE="OUTPUT">                                           ]]></para>
		/// <para><![CDATA[	<RECORD>                                                    ]]></para>
		/// <para><![CDATA[		<DATEREPORT>24/02/2009</DATEREPORT>                     ]]></para>
		/// <para><![CDATA[	</RECORD>                                                   ]]></para>
		/// <para><![CDATA[</TABLE>                                                        ]]></para>
		/// <para><![CDATA[<TABLE>                                                         ]]></para>
		/// <para><![CDATA[	<RECORD>                                                    ]]></para>
		/// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
		/// <para><![CDATA[		<LASTNAME>Fuller</LASTNAME>                             ]]></para>
		/// <para><![CDATA[		<FIRSTNAME>Andrew</FIRSTNAME>                           ]]></para>
		/// <para><![CDATA[		<SHIPPEDDATE>1996-09-12T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
		/// <para><![CDATA[		<ORDERID>10280</ORDERID>                                ]]></para>
		/// <para><![CDATA[		<SALEAMOUNT>613.2</SALEAMOUNT>                          ]]></para>
		/// <para><![CDATA[	</RECORD>                                                   ]]></para>
		/// <para><![CDATA[	<RECORD>                                                    ]]></para>
		/// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
		/// <para><![CDATA[		<LASTNAME>Callahan</LASTNAME>                           ]]></para>
		/// <para><![CDATA[		<FIRSTNAME>Laura</FIRSTNAME>                            ]]></para>
		/// <para><![CDATA[		<SHIPPEDDATE>1996-08-30T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
		/// <para><![CDATA[		<ORDERID>10286</ORDERID>                                ]]></para>
		/// <para><![CDATA[		<SALEAMOUNT>3016</SALEAMOUNT>                           ]]></para>
		/// <para><![CDATA[	</RECORD>                                                   ]]></para>
		/// <para><![CDATA[	<RECORD>                                                    ]]></para>
		/// <para><![CDATA[		<COUNTRY>USA</COUNTRY>                                  ]]></para>
		/// <para><![CDATA[		<LASTNAME>Callahan</LASTNAME>                           ]]></para>
		/// <para><![CDATA[		<FIRSTNAME>Laura</FIRSTNAME>                            ]]></para>
		/// <para><![CDATA[		<SHIPPEDDATE>1996-08-28T00:00:00-03:00</SHIPPEDDATE>    ]]></para>
		/// <para><![CDATA[		<ORDERID>10287</ORDERID>                                ]]></para>
		/// <para><![CDATA[		<SALEAMOUNT>819</SALEAMOUNT>                            ]]></para>
		/// <para><![CDATA[	</RECORD>                                                   ]]></para>
		/// <para><![CDATA[</TABLE>                                                        ]]></para>
		/// <para><![CDATA[</TABLES>                                                       ]]></para>
        /// </returns>
        public string getXMLQuery(ref string strXML, ref string pRootName) //pRootName no implementado
        {
            XmlDocument oXmlStructureMessage = null;
            XmlNode oXmlNodeMessage = null;
            XslCompiledTransform oXslCompiledTransform = null;
            bool bHasInputTemplate = false;
            bool bHasOutputTemplate = false;
            bool bHasXmlDeclarationRoot = false;
            string sFileTemplateName = "";
            string sReturn = "";
            string sRootName = "";
            string sStep = "";
            try
            {
                if (!this.isConnected)
                {
                    this._oException = new oException(false);
                    throw new oSqlSpExecuteException("No existe una conexión abierta. No es posible escribir el log");
                }
                if (this._llevel > logLevel.medium)
                    this._IOLog.recordLogLine("Mensaje: " + Regex.Replace(strXML, @"\r\n?|\n", "").ToString());

                sStep = this.sStep;
                this._oException.addMsg("No se pudo cargar la estructura del mensaje", sStep);
                oXmlStructureMessage = new XmlDocument();
                oXmlStructureMessage.LoadXml(strXML);

                /*if (this._llevel > logLevel.medium)
                    this._IOLog.recordLogLine("Mensaje JSON:" + JsonConvert.SerializeXmlNode(oXmlStructureMessage));
                */
                //Determino si existe una instrucción de plantilla de transformaciónd de entrada
                bHasInputTemplate = (oXmlStructureMessage.SelectSingleNode("*/@input-template-name") != null
                    && oXmlStructureMessage.SelectSingleNode("*/@input-template-name").InnerText.Trim().Length > 0
                    );
                //Determino si existe una instrucción de plantilla de transformaciónd de salida
                bHasOutputTemplate = (oXmlStructureMessage.SelectSingleNode("*/@output-template-name") != null
                    && oXmlStructureMessage.SelectSingleNode("*/@output-template-name").InnerText.Trim().Length > 0
                    );

                //Determino si existe una instrucción de cambio de raíz
                bHasXmlDeclarationRoot = (oXmlStructureMessage.SelectSingleNode("*/@xml-declaration-root") != null
                    && oXmlStructureMessage.SelectSingleNode("*/@xml-declaration-root").InnerText.Trim().Length > 0
                    );

                if (bHasInputTemplate || bHasOutputTemplate)
                    oXslCompiledTransform = new XslCompiledTransform();
                //Proceso la plantilla de entrada.
                if (bHasInputTemplate)
                {
                    sStep = this.sStep;
                    this._oException.addMsg("Error al cargar la plantilla de entrada", sStep);
                    sFileTemplateName = this.appDirectoryTemplate + Path.DirectorySeparatorChar + ""
                        + oXmlStructureMessage.SelectSingleNode("*/@input-template-name").InnerText
                        + ".xsl";
                    if (this._llevel > logLevel.medium)
                        this._IOLog.recordLogLine("Aplicando plantilla de entrada:" + sFileTemplateName);
                    oXslCompiledTransform.Load(sFileTemplateName);
                    //Transformo y recargo el DOM
                    oXmlStructureMessage.LoadXml(XmlTransformInMemory(ref oXslCompiledTransform, ref strXML));
                }
                //procesamiento de mensajes
                if (oXmlStructureMessage.SelectNodes("ROOT/RECORD").Count == 1)//es un mensaje simple
                {
                    if (bHasXmlDeclarationRoot)
                        sRootName = Regex.Replace(oXmlStructureMessage.SelectSingleNode("ROOT").Attributes["xml-declaration-root"].InnerText, "[^A-Za-z]", "_");//Me aseguro que no haya basura 
                    oXmlNodeMessage = oXmlStructureMessage.SelectSingleNode("//RECORD");
                    sReturn = "<ROOT>" + createExecuteInstruction(ref oXmlNodeMessage) + "</ROOT>";
                }
                else//Mensaje múltiple.
                {
                    sReturn = createMultipleExecuteInstruction(ref oXmlStructureMessage);
                }

                if (this._llevel > logLevel.medium)
                {
                    sStep = this.sStep;
                    this._oException.addMsg("No se pudo serializar mensaje de respuesta", sStep);
                    oXmlStructureMessage.LoadXml(sReturn);

                    //this._IOLog.recordLogLine("Respuesta:" + Regex.Replace(sReturn, @"\r\n?|\n", "").ToString());
                    //this._IOLog.recordLogLine("Respuesta JSON: " + JsonConvert.SerializeXmlNode(oXmlStructureMessage));


                }


                return sReturn;
            }
            catch (oSqlSpExecuteException oEx)
            {
                throw oEx;
            }
            catch (Exception oEx)
            {

                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
            finally
            {
            }
        }
        private string createMultipleExecuteInstruction(ref XmlDocument oXmlStructureMessage)
        {
            string sRootName = "ROOT";
            XmlNode oNodeAux = null;
            StringBuilder sbToReturn = null;
            try
            {
                //Parametrización optativa de raíz de retorno
                if (
                oXmlStructureMessage.SelectSingleNode("//ROOT").Attributes["xml-declaration-root"] != null
                &&
                oXmlStructureMessage.SelectSingleNode("//ROOT").Attributes["xml-declaration-root"].InnerText.Trim().Length > 0
                )
                    sRootName += Regex.Replace(oXmlStructureMessage.SelectSingleNode("//ROOT").Attributes["xml-declaration-root"].InnerText, "[^A-Za-z]", "_");
                sbToReturn = new StringBuilder();
                sbToReturn.Append("<" + sRootName + ">");
                foreach (XmlNode oNode in oXmlStructureMessage.SelectNodes("//RECORD"))
                {
                    try
                    {
                        oNodeAux = oNode;
                        sbToReturn.Append(createExecuteInstruction(ref oNodeAux));
                    }
                    catch (Exception oEx)
                    {
                        sbToReturn.Append("<TABLE resultCode=\"-1\" ");
                        sbToReturn.Append("resultString=\"" + Regex.Replace(oEx.Message, "[^A-Za-z0-9ÁÉÍÓÚáéíóúñÑ()_]", " ") + "\"");
                        if (oNode.SelectSingleNode("SERVICE_NAME") != null)
                            sbToReturn.Append(" service_name = \"" + oNode.SelectSingleNode("SERVICE_NAME").InnerXml + "\"");
                        sbToReturn.Append("/>");
                    }
                }
                sbToReturn.Append("</" + sRootName + ">");
                return sbToReturn.ToString();
            }
            catch (oSqlSpExecuteException oEx)
            {
                throw oEx;
            }
            catch (Exception oEx)
            {

                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
            finally
            {
            }
        }
        private string createExecuteInstruction(ref XmlNode oXmlNodeMessage)
        {
            DataSet _DataSet = null;
            bool bHasOutputParameters = false;
            string sDataType = "";
            string sFieldName = "";
            string sFieldsSeparator = " ";
            string sFieldValue = "";
            string sNodeValue = "";
            string sOutPutDeclareSeparator = "declare @return int ";
            string sOutPutResponseSeparator = "select returnValue= @return";
            string sPrecision = "";
            string sServiceName = "";
            string sSpName = "";
            string sStep = "";
            string sTypeMode = "0";
            string sXmlAux = "";
            StringBuilder sbExecute = new StringBuilder();
            StringBuilder sbResponse = new StringBuilder();
            StringBuilder sbResponseDeclare = new StringBuilder();
            StringBuilder sbResponseOut = new StringBuilder();
            XmlAttribute oNewValueAttribute = null;
            XmlNode oNodeService = null;
            XmlNode XmlNodeCommandTimeOut = null;
            XslCompiledTransform oXslCompiledTransform = null;
            try
            {
                this._LastSQLStatement = "";
                //Determino el nombre del servicio
                sStep = this.sStep;
                this._oException.addMsg("Se esperaba el nodo SERVICE_NAME", sStep);
                sServiceName = oXmlNodeMessage.SelectSingleNode("SERVICE_NAME").InnerText;
                //Escribo Log
                if (this._llevel > logLevel.low)
                    this._IOLog.recordLogLine("Servicio determinado: " + sServiceName);

                sStep = this.sStep;
                this._oException.addMsg("Servicio no definido('" + sServiceName + ") en " + this.appServiceFileName, sStep);
                oNodeService = this._XmlServiceDocument.SelectSingleNode("//" + sServiceName);
                //Determino el nombre del procedure
                sStep = this.sStep;
                this._oException.addMsg("No se encontró el atributo SP_NAME en el servicio(" + sServiceName + ")", sStep);
                sSpName = oNodeService.SelectSingleNode("//" + sServiceName + "/@SP_NAME").InnerText;
                if (this._llevel > logLevel.medium)
                    this._IOLog.recordLogLine("Procedimiento almacenado determinado: " + sSpName);
                //Defino la primera y última estructura del sql
                sbResponseDeclare.Append(sOutPutDeclareSeparator);
                sbResponseOut.Append(sOutPutResponseSeparator);

                sbResponse.Append("execute @return=" + sSpName); //comienzo a armar la cadena de comando

                //Comienzo con el seteo de parametros
                foreach (XmlNode oNode in oNodeService.SelectNodes("PARAMETERS_LIST/PARAMETER"))
                {
                    //Obtener el Nodo
                    sStep = this.sStep;
                    this._oException.addMsg("Error en la lectura de parámetros del mensaje, no se encontró el nodo '" + oNode.InnerText + "' en el mensaje.", sStep);
                    //Escribo Log
                    if (this._llevel > logLevel.medium)
                        this._IOLog.recordLogLine("Relacionando " + oNode.InnerText);

                    //Evaluo los atributos básicos************************************************
                    //Verifico el tipo definido en el archivo
                    sStep = this.sStep;
                    this._oException.addMsg("El nodo '" + oNode.InnerText + "' no posee un atributo TYPE", sStep);
                    sDataType = oNode.Attributes.GetNamedItem("TYPE").InnerText;
                    //Verifico el campo asociado
                    sStep = this.sStep;
                    this._oException.addMsg("El nodo '" + oNode.InnerText + "' no posee un atributo FIELD", sStep);
                    sFieldName = "@" + oNode.Attributes.GetNamedItem("FIELD").InnerText;
                    //Determino el tipo de parámetros (IN/OUT)
                    sStep = this.sStep;
                    this._oException.addMsg("El atributo TYPEMODE no contiene un valor válido(" + oNode.InnerText + ")", sStep);
                    if (oNode.Attributes.GetNamedItem("TYPEMODE") != null)
                    {
                        sTypeMode = oNode.Attributes.GetNamedItem("TYPEMODE").InnerText;
                        if (sTypeMode != "0" && sTypeMode != "1")
                            throw new Exception("Valor no apto para atributo TYPEMODE. Se esperaba 1 o 0");

                    }
                    //Tratamiento solo para parámetros de IN
                    if (sTypeMode == "0")
                    {
                        //Obtener el Nodo
                        sStep = this.sStep;
                        this._oException.addMsg("Error en la lectura de parámetros del mensaje, no se encontró el nodo '" + oNode.InnerText + "' en el mensaje.", sStep);

                        //Si el nodo no tiene default en el mensaje mapeo directamente en nodo
                        if (oNode.Attributes.GetNamedItem("DEFAULT") == null)
                            sNodeValue = oXmlNodeMessage.SelectSingleNode(oNode.InnerText).InnerText.Replace("'", "''");
                        else
                        {
                            //Si tiene default evaluo si vino un valor en el nodo o mapeo el default
                            if (oXmlNodeMessage.SelectSingleNode(oNode.InnerText) == null)
                            {
                                //mapeo el default
                                if (this._llevel > logLevel.medium)
                                    this._IOLog.recordLogLine("Obteniendo default");
                                sNodeValue = oNode.Attributes.GetNamedItem("DEFAULT").InnerText;
                                //Si el valor es null se asume a que es equivalente al tipo null y no se mapea el valor
                                if (sNodeValue.ToLower() == "null")//Ojo esto, queda a observación final++++++++++++++++++++++++
                                    continue;
                                //el valor esta definido por otro nodo
                                if (oNode.Attributes.GetNamedItem("ISXPATH") != null
                                    && oNode.Attributes.GetNamedItem("ISXPATH").InnerText == "1")
                                {
                                    if (this._llevel > logLevel.medium)
                                        this._IOLog.recordLogLine("Obteniendo XPATH");
                                    sStep = this.sStep;
                                    this._oException.addMsg("El nodo establecido como remapeo no es parte del mensaje (" + sNodeValue + ").", sStep);
                                    sNodeValue = oXmlNodeMessage.SelectSingleNode(sNodeValue).InnerText.Replace("'", "''");
                                }
                            }
                            else
                            {
                                //mapeo el valor del mensaje
                                sNodeValue = oXmlNodeMessage.SelectSingleNode(oNode.InnerText).InnerText.Replace("'", "''");
                            }

                        }

                        //Verifico que el dato sea válido.
                        sStep = this.sStep;
                        this._oException.addMsg("Valor no apto para el tipo " + sDataType + " (" + oNode.InnerText + "=" + sNodeValue + ") o el tipo de dato no es correcto.", sStep);
                        sFieldValue = formatValue(ref sDataType, ref sNodeValue);
                        //Escribo Log
                        if (this._llevel > logLevel.medium)
                            this._IOLog.recordLogLine(sFieldName + "=" + sFieldValue);
                        //Verifico si el campo puede venir vacío
                        if (
                            (
                                (
                                oNode.Attributes.GetNamedItem("NOTNULLABLE") != null
                                && oNode.Attributes.GetNamedItem("NOTNULLABLE").InnerText == "1"
                                )
                                ||
                                (
                                oNode.Attributes.GetNamedItem("NOTNULL") != null
                                && oNode.Attributes.GetNamedItem("NOTNULL").InnerText == "1"
                                )
                            )
                            && sFieldValue.Length == 0
                            )
                            throw new oSqlSpExecuteException("Se esperaba un valor con largo superior a 0 en el nodo " + oNode.InnerText);

                    }//if (sTypeMode=="0")
                    //Agrego el parámetro 


                    sbResponse.Append(sFieldsSeparator + (sTypeMode == "1" ? sFieldName + "=" + sFieldName + " output" : sFieldName + "=" + sFieldValue));
                    sFieldsSeparator = ", ";


                    sPrecision = (oNode.Attributes.GetNamedItem("PRESICION") != null ?
                        "(" + oNode.Attributes.GetNamedItem("PRESICION").InnerText + ")" : "");

                    if (sTypeMode == "1")
                    {
                        bHasOutputParameters = true;
                        sOutPutDeclareSeparator = ", ";
                        sbResponseDeclare.Append(sOutPutDeclareSeparator + sFieldName + " " + this._oDataTypeDictionary[sDataType] + sPrecision);
                        sOutPutResponseSeparator = ", ";
                        sbResponseOut.Append(sOutPutResponseSeparator + sFieldName.Replace("@", "") + "=" + sFieldName);

                    }



                }//End foreach
                /*if (this._llevel > logLevel.medium)
                    this._IOLog.recordLogLine(sbResponse.ToString());*/
                sbExecute.Append(sbResponseDeclare);
                sbExecute.Append(";");
                sbExecute.Append(sbResponse);
                sbExecute.Append(";");
                sbExecute.Append(sbResponseOut);

                this._LastSQLStatement = sbExecute.ToString();

                _DataSet = new DataSet();

                sStep = this.sStep;
                this._oException.addMsg("Error al ejecutar instrucción en base de datos", sStep);
                //sFieldValue = formatValue(ref sDataType, ref sNodeValue);


                //CommandTimeout
                //Tiempo (en segundos) que se debe esperar para que se ejecute el comando. El valor predeterminado es 30 segundos. 
                //Un valor de 0 indica que no hay límite y se debe evitar en CommandTimeout, porque el intento de ejecución de un comando esperaría indefinidamente.
                if ((XmlNodeCommandTimeOut = this._XmlServiceDocument.SelectSingleNode("//@commandTimeOut")) != null)
                    this._SqlCommand.CommandTimeout = Convert.ToInt16(XmlNodeCommandTimeOut.SelectSingleNode("//@commandTimeOut").InnerText);
                this._SqlCommand.CommandText = sbExecute.ToString();



                if (this._llevel > logLevel.medium)
                    this._IOLog.recordLogLine("CommandText:" + this._SqlCommand.CommandText);

                sStep = this.sStep;
                this._oException.addMsg("Error al establecer transacción a la conexión", sStep);

                /*if (this.stateTransactionDb == stateTransaction.open)
                    this._SqlCommand.Transaction = this._OleDbTransaction;
                */
                this._SqlDataAdapter.SelectCommand = this._SqlCommand;
                sStep = this.sStep;
                this._oException.addMsg("Error al ejecutar la instrucción", sStep);

                this._SqlDataAdapter.Fill(_DataSet, "oData");

                //Comienzo el proceso mapeos y transformación
                sStep = this.sStep;
                this._oException.addMsg("Error al cargar respuesta de la base de datos", sStep);

                this._XmlDocument.LoadXml(_DataSet.GetXml());
                oNewValueAttribute = this._XmlDocument.CreateAttribute("servicename");
                oNewValueAttribute.InnerText = sServiceName;
                this._XmlDocument.SelectSingleNode("//NewDataSet").Attributes.Append(oNewValueAttribute);

                sStep = this.sStep;
                this._oException.addMsg("Error aplicar la transformación de la respuesta", sStep);
                oXslCompiledTransform = new XslCompiledTransform();
                oXslCompiledTransform.Load(this._appDirectoryTemplate + Path.DirectorySeparatorChar + "generic-dataset-struct.xslt");
                sXmlAux = this._XmlDocument.InnerXml;
                this._XmlDocument.LoadXml(XmlTransformInMemory(ref oXslCompiledTransform, ref sXmlAux));




                if (bHasOutputParameters)
                {
                    oNewValueAttribute = this._XmlDocument.CreateAttribute("TYPE");
                    oNewValueAttribute.InnerText = "OUTPUT";
                    this._XmlDocument.SelectSingleNode("//TABLES").LastChild.Attributes.Append(oNewValueAttribute);
                }

                //Devuelvo la instrucción SQL ejecutada
                if (this._XmlServiceDocument.SelectSingleNode("//@showLastSQlStatement") != null &&
                    this._XmlServiceDocument.SelectSingleNode("//@showLastSQlStatement").InnerText == "1"
                    )
                {
                    oNewValueAttribute = this._XmlDocument.CreateAttribute("lastSQlStatement");
                    oNewValueAttribute.InnerText = this._LastSQLStatement;
                    this._XmlDocument.SelectSingleNode("//TABLES ").Attributes.Append(oNewValueAttribute);


                }



                return this._XmlDocument.InnerXml;
            }
            catch (oSqlSpExecuteException oEx)
            {
                throw oEx;
            }
            catch (Exception oEx)
            {
                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }

            finally
            {
                if (_DataSet != null)
                    _DataSet.Dispose();
                this._SqlCommand.CommandText = "";
                this._SqlDataAdapter.SelectCommand = null;
            }


        }
        #endregion //getData
        #region Conectar
        /// <summary>
        /// Libera la conexión y destruye todos lo objetos internos inherentes a la comunicación
        /// </summary>
        public void desconectar()
        {
            if (this.isConnected)
            {
                /*if (this._OleDbTransaction != null)
                {
                    this._OleDbTransaction.Rollback();
                    this._OleDbTransaction.Dispose();
                }*/
                if (this._SqlConnection != null)
                    this._SqlConnection.Close();
                if (this._SqlConnection != null)
                    this._SqlConnection.Dispose();
                if (this._SqlDataAdapter != null)
                    this._SqlDataAdapter.Dispose();
                if (this._SqlCommand != null)
                    this._SqlCommand.Dispose();
                if (this._SqlDataAdapter != null)
                    this._SqlDataAdapter.Dispose();
                if (_XmlDocument != null)
                    this._XmlDocument = null;



            }
        }
        /// <summary>
        /// <para>Conecta el servicio al motor de base de datos según la cadena de conexión especificada en el nodo STRING_CONNECTION. Además posee un nodo denominado PATH_TEMPLATE. Este nodo es optativo y define donde la librería buscara sus plantillas XSLT para aplicar a los mensajes en forma directa tanto de entrada como de salida, así como también su plantilla principal denominada "generic-dataset-struct.xsl".</para>
        /// <para>Si no se asigna un valor a dicho nodo la aplición buscará sus plantillas en la carpeta "xsl" que deberá estar definida dentro de la ruta donde se encuentre el archivo de servicios.</para>
        /// <para>La opción de asignar el parámetro STRING_CONNECTION\@isEncrypt en 1, permite utilizar el valor del nodo en forma encriptada a través del aplicativo waGenConn.exe(TripleDES)</para>
        /// </summary>
		/// <example>Conexión MSSQL
        /// <code>
        /// <![CDATA[<STRING_CONNECTION isEncrypt="0">Provider=SQLOLEDB.1;Password=miClave;Persist Security Info=True;User ID=miUsuario;Data Source=miServidor</STRING_CONNECTION>]]>
        /// </code>
        /// </example>
        /// <example>Establecer la conexión
        /// <code>
        /// Dabitis.Sql.DataManager.oSqlSpExecute oSql = null;
        /// oSql = new oSqlSpExecute();
        /// oSql.appServiceFileName = "miArchivoDeConfiguracion.xml";
        /// oSql.conectar();
        /// </code>
        /// </example>        

        public void conectar()
        {
            string stringConnection = "";
            string sStep = "";
            StringBuilder sbFirstWriteLog = null;
            bool bFirstWriteLog = false;
            string auxVar = "";
            encrypData.EncrypData ed = null;
            try
            {
                if (this.isConnected)
                    throw new oSqlSpExecuteException("La conexión ya se encuentra abierta");

                if (!File.Exists(this.appLogFileName))
                {

                    sbFirstWriteLog = new StringBuilder();
                    sbFirstWriteLog.AppendLine("*******************************************************");
                    sbFirstWriteLog.AppendLine("*SQL Data Manager Version " + GetAppVersion() + "                     *");
                    sbFirstWriteLog.AppendLine("*Developed by MJR                                     *");
                    sbFirstWriteLog.AppendLine("*Copyright 2001-2019© - MauricioReyna.com.ar®         *");
                    sbFirstWriteLog.AppendLine("*Optimized for MSQL                                   *");
                    sbFirstWriteLog.AppendLine("*Dependencies: HandlerException.dll Version 3.0.0.0   *");
                    sbFirstWriteLog.AppendLine("*All Rights Reserved                                  *");
                    sbFirstWriteLog.AppendLine("*******************************************************");
                    bFirstWriteLog = true;
                }

                this._IOLog = new IOLog(appLogFileName);

                //Si es la primera escritura escribo el copyrigth




                if (bFirstWriteLog)
                    this._IOLog.recordLogLine(sbFirstWriteLog.ToString());

                this._IOLog.idProcess = Regex.Replace(Guid.NewGuid().ToString(), "[^0-9]", "");
                this._oException = new oException(this._appLogFileName);
                this._oException.idLog = this._IOLog.idProcess;

                validateLicense();

                sStep = this.sStep;
                this._oException.addMsg("No se pudo cargar el archivo de conexión", sStep);
                this._XmlServiceDocument = new XmlDocument();
                this._XmlServiceDocument.Load(this._appServiceFileName);
                if (this._llevel > logLevel.none)
                    this._IOLog.recordLogLine("Archivo de conexión: " + this._appServiceFileName);

                sStep = this.sStep;
                this._oException.addMsg("No se pudo obtener el nodo de conexión[STRING_CONNECTION]", sStep);
                //evalúo la cadena y sim está encriptada
                if (this._XmlServiceDocument.SelectSingleNode("//STRING_CONNECTION/@isEncrypt") != null
                    && this._XmlServiceDocument.SelectSingleNode("//STRING_CONNECTION/@isEncrypt").InnerText == "1")
                {
                    ed = new encrypData.EncrypData();
                    stringConnection = this._XmlServiceDocument.SelectSingleNode("//STRING_CONNECTION").InnerText;
                    stringConnection = ed.DecryptPacket(ref stringConnection);
                    ed = null;
                }
                else
                    stringConnection = this._XmlServiceDocument.SelectSingleNode("//STRING_CONNECTION").InnerText;

                sStep = this.sStep;
                if (this._appDirectoryTemplate.Length == 0)
                {
                    this._oException.addMsg("No se pudo obtener el nodo de plantillas[PATH_TEMPLATE]", sStep);
                    this.appDirectoryTemplate = this._XmlServiceDocument.SelectSingleNode("//PATH_TEMPLATE").InnerText;
                }
                auxVar = this.appDirectoryTemplate;//Si no escribo o asigno no setea el default
                if (this._llevel > logLevel.none)
                    this._IOLog.recordLogLine("Directorio de plantillas: " + auxVar);//Si no escribo o asigno no setea el default


                sStep = this.sStep;
                this._oException.addMsg("Error al establecer la conexión a la base de datos", sStep);
                this._SqlConnection = new SqlConnection(stringConnection);
                this._SqlConnection.Open();
                if (this._llevel > logLevel.none)
                    this._IOLog.recordLogLine("Conexión establecida[Server: " + this._SqlConnection.DataSource.ToString() + "]");
                sStep = this.sStep;
                this._oException.addMsg("No se pudo asignar la conexión al objeto de comando", sStep);
                this._SqlCommand = new SqlCommand();
                this._SqlCommand.CommandType = CommandType.Text;
                this._SqlCommand.Connection = this._SqlConnection;
                this._SqlDataAdapter = new SqlDataAdapter();
                this._XmlDocument = new XmlDocument();

            }
            catch (oSqlSpExecuteException oEx)
            {
                throw oEx;
            }
            catch (Exception oEx)
            {

                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
            finally
            {

            }

        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #endregion//Conectar
        #region miscelaneas
        private void validateLicense()
        {
            DateTime expireDate;
            XmlDocument oXmlLicense = null;
            encrypData.EncrypData ed = null;
            string sExpireDate = "";
            string sLicenseFile = appDir + Path.DirectorySeparatorChar + "DSQLicense.key";
            string sStep = "";
            try
            {
                if (!File.Exists(sLicenseFile))
                    sLicenseFile = AssemblyDirectory + Path.DirectorySeparatorChar + "DSQLicense.key";

                sStep = this.sStep;
                this._oException.addMsg("Error al obtener la licencia.", sStep);
                oXmlLicense = new XmlDocument();
                oXmlLicense.Load(sLicenseFile);
                ed = new encrypData.EncrypData();
                sStep = this.sStep;
                this._oException.addMsg("Error al obtener la fecha de expiración", sStep);
                System.Globalization.CultureInfo format = new System.Globalization.CultureInfo("fr-FR", true);
                sExpireDate = oXmlLicense.SelectSingleNode("//expireDate").InnerText;
                sExpireDate = ed.DecryptPacket(ref sExpireDate);
                expireDate = System.DateTime.Parse(sExpireDate, format, System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                sStep = this.sStep;
                this._oException.addMsg("Licencia expirada", sStep);
                if (System.DateTime.Now.Date > expireDate)
                {
                    throw new Dabitis.Sql.DataManager.oSqlSpExecuteException("Fecha de licencia[" + expireDate.ToShortDateString() + "]");
                }

            }
            catch (Exception oEx)
            {
                throw new Dabitis.Sql.DataManager.oSqlSpExecuteException(this._oException.HandleException(sStep, oEx).Message);
            }
            finally
            {

            }


        }
        /// <summary>
        /// Returns version of the current Assembly
        /// </summary>
        /// <returns>Version string in format x.x.x.x</returns>
        private string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Returns last SQl Statement executed o constructed
        /// </summary>
        /// <returns>Version string in format x.x.x.x</returns>
        private string getLastSQLStatement()
        {
            return this._LastSQLStatement;
        }



        private string formatValue(ref string sDataType, ref string sValue)
        {

            string strToReturn = "";
            string sDatePatternInput = "";
            string sDatePatternDB = "";
            DateTime oDateTime;

            switch (sDataType.ToUpper())
            {
                case "INT":
                case "INT1":
                case "INT2":
                case "INT4":
                    strToReturn = Convert.ToString(Convert.ToInt64(sValue));
                    break;
                case "NUMERIC":
                case "MONEY":
                case "FLT8":
                    System.Globalization.NumberFormatInfo ni = new System.Globalization.NumberFormatInfo();
                    ni.NumberDecimalSeparator = ".";
                    strToReturn = Convert.ToString(Double.Parse(sValue, ni)).ToString().Replace(",", ".");
                    break;
                case "VARCHAR":
                case "CHAR":
                    strToReturn = "'" + sValue + "'";
                    break;
                case "DATE":
                case "DATETIME":
                    //08/03/2015 Nuevo manejo de patrón de fecha
                    if (this._XmlServiceDocument.SelectSingleNode("//@datePatternInput") != null)
                        sDatePatternInput = this._XmlServiceDocument.SelectSingleNode("//@datePatternInput").InnerText;
                    else
                        sDatePatternInput = (sDataType.ToUpper() == "DATETIME") ? "dd/MM/yyyy HH:mm:ss:fff" : "dd/MM/yyyy"; //Patrón por defecto

                    //08/03/2015 Nuevo manejo de patrón de fecha para la base de datos
                    if (this._XmlServiceDocument.SelectSingleNode("//@datePatternDB") != null)
                        sDatePatternDB = this._XmlServiceDocument.SelectSingleNode("//@datePatternDB").InnerText;
                    else
                        sDatePatternDB = "yyyyMMdd HH:mm:ss:fff"; //Patrón por defecto
                    /*
                     There are two datetime string formats that are interpreted correctly with with any language setting.
                      With the first format, only the YYYYMMDD part is required, but with the second format (ISO8601), you must supply everything except for the milliseconds.
                    dd/MM/yyyy HH:mm:ss:fff
                    20100127 15:33:13.343
                    2010-01-27T15:33:13.343
                    * 
                     */
                    oDateTime = System.DateTime.ParseExact(sValue, sDatePatternInput, System.Globalization.CultureInfo.InvariantCulture);
                    strToReturn = "'" + oDateTime.ToString(sDatePatternDB) + "'";
                    break;
                default:
                    throw new oSqlSpExecuteException("El tipo " + sDataType + " no es un tipo de dato válido");
                    //break;
            }
            return strToReturn;
        }
        /// <summary>Función utilizada para concatenar al nombre de archivo de log según la fecha actual</summary>
        /// <returns>
        /// Una cadena con el formato aaaammdd 
        /// <example>
        /// <code>
        /// public string appLogFileName
        /// {
        ///     get
        ///     {
        ///         if (this._appLogFileName == "")
        ///             this._appLogFileName = appDir + Path.DirectorySeparatorChar + "sdm" + getDataFormatDate() + ".log";
        ///         return this._appLogFileName;
        ///     }
        ///     set
        ///     {
        ///         this._appLogFileName = value;
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </returns>
        public string getDataFormatDate()
        {
            string sReturn = "";
            sReturn += DateTime.Now.ToString("yyyyyMMdd");
            return sReturn;
        }
        private string XmlTransformInMemory(ref XslCompiledTransform transform, ref string input)
        {
            //El problema con caracteres fuera de encondig haxadecimal se resuelve con objecto.CheckCharacters = false
            //13/06/2009 10:24 p.m.
            StringBuilder sb = new StringBuilder();
            XmlReaderSettings xrSet = new XmlReaderSettings();
            xrSet.CheckCharacters = false;
            XmlReader xReader = XmlReader.Create(new StringReader(input), xrSet);
            XmlWriterSettings xwSet = new XmlWriterSettings();
            xwSet.OmitXmlDeclaration = true;
            xwSet.CheckCharacters = false;
            XmlWriter xWriter = XmlWriter.Create(sb, xwSet);
            transform.Transform(xReader, xWriter);
            return sb.ToString();
        }
        private void loadDataType()
        {
            this._oDataTypeDictionary.Add("DATE", "smalldatetime");
            this._oDataTypeDictionary.Add("DATETIME", "datetime");
            this._oDataTypeDictionary.Add("FLT8", "float");
            this._oDataTypeDictionary.Add("INT", "int");
            this._oDataTypeDictionary.Add("INT1", "tinyint");
            this._oDataTypeDictionary.Add("INT2", "smallint");
            this._oDataTypeDictionary.Add("INT4", "bigint");
            this._oDataTypeDictionary.Add("MONEY", "money");
            this._oDataTypeDictionary.Add("NUMERIC", "numeric");
            this._oDataTypeDictionary.Add("VARCHAR", "varchar");
            this._oDataTypeDictionary.Add("CHAR", "char");

        }
        /// <summary>A partir del parámetro recibido lo procesa para crear string xml tipo document</summary>
        /// <param name="strToTransform"><![CDATA[Conjunto de elementos y valores a crear separados por "|" y "=" tipo  "Nombre&valor|Nombre1=valor1...|Nombren=valorn"]]></param>
        /// <param name="iWithRoot">Indica la raíz del parámetro de retorno.
        /// <list type="table">Valores posibles
        /// <item>
        ///     <term>1</term>
        ///     <description>Verdadero</description>
        /// </item>
        /// <item>
        ///     <term>0</term>
        ///     <description>Falso</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>
        /// <example>Estructura tipo de retorno.
        /// <code><![CDATA[
		/// string strSql="";                                        
		/// strSql += "SERVICE_NAME=INS_MESSAGE";                    
		/// strSql += "|APELLIDO=txtApellido";                       
		/// strSql += "|NOMBRE=txtNombre";                           
		/// strSql += "|MAIL=txtMail";                               
		/// strSql += "|MENSAJE=txtMensaje.Value";    
		/// Dabitis.Sql.DataManager.oSqlSpExecute dato = new oSqlSpExecute() ;
		/// strSql = dato.sMakeSimpleXML(strSql, 1); 
		///                                                         
		///                                                         
		/// //Retorna                                                
		/// //<ROOT>                                                 
		/// //<RECORD>                                               
		/// //	<SERVICE_NAME>INS_MESSAGE</SERVICE_NAME>         
		/// //	<APELLIDO>txtApellido</APELLIDO>                 
		/// //	<NOMBRE>txtNombre</NOMBRE>                       
		/// //	<MAIL>txtMail</MAIL>                             
		/// //	<MENSAJE>txtMensaje.Value</MENSAJE>              
		/// //</RECORD>                                              
		/// //</ROOT>                                                
		/// ]]>
        /// </code>
        /// </example>
        /// </returns>
        public string sMakeSimpleXML(String strToTransform, int iWithRoot)
        {
            string strToReturn;
            string[] wvarArrayParameters;
            string[] wvarAuxrecord;
            int wvarCounter;
            try
            {
                strToReturn = ((iWithRoot != 1) ? "" : "<ROOT>") + "<RECORD>"; //raíz
                wvarArrayParameters = strToTransform.Split('|');
                for (wvarCounter = 0; wvarCounter < wvarArrayParameters.Length; wvarCounter++)
                {
                    wvarAuxrecord = Convert.ToString(wvarArrayParameters[wvarCounter]).Split('=');

                    strToReturn += "<" + wvarAuxrecord[0] + "><![CDATA[" + Convert.ToString(wvarAuxrecord[1]) + "]]></" + wvarAuxrecord[0] + ">";
                    wvarAuxrecord = null;

                }

                strToReturn = strToReturn + "</RECORD>" + ((iWithRoot != 1) ? "" : "</ROOT>"); //cierro raíz

            }
            catch (Exception oExc)
            {
                throw (new Exception("Error: Verifique la sintaxis. Ejemplo: var1=valor2|var2=valor2"));
            }
            finally { }

            return strToReturn;
        }
        #endregion//miscelaneas
        #region Procesar
        #endregion //Procesar
        #region Transaction
        /// <summary>Inicia una transacciónd de base de datos.</summary>
        /// <exception cref="System.Exception">Se eleva cuando existe un error en la conexión base</exception>
        /// <exception cref="Dabitis.Sql.DataManager.oSqlSpExecuteException ">Se eleva cuando existe un error de negocio en la conexión</exception>
        /// <seealso cref="rollbackTransaction"/>
        /// <seealso cref="commitTransaction"/>
        /// <seealso cref="beginTransaction"/>
        /*public void beginTransaction()
        {
            string sStep="";
            try
            {
                sStep = this.sStep;
                if (this._oException!=null)
                    this._oException.addMsg("beginTransaction()", sStep);
                if (!this.isConnected)
                    throw new oSqlSpExecuteException("No existe una conexión abierta");
                if (this._stateTransaction == stateTransaction.open)
                    throw new oSqlSpExecuteException("La transacción ya se encuentra abierta");
                else
                {
                    this._OleDbTransaction = this._SqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
                    this._stateTransaction = stateTransaction.open;
                }
            }
            catch (Exception oEx)
            {
                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
        }*/
        /// <summary>
        /// Deshace una transacción desde un estado pendiente.
        /// </summary>
        /// <exception cref="System.Exception">Se eleva cuando existe un error en la conexión base</exception>
        /// <exception cref="Dabitis.Sql.DataManager.oSqlSpExecuteException ">Se eleva cuando existe un error de negocio en la conexión</exception>
        /// <seealso cref="rollbackTransaction"/>
        /// <seealso cref="commitTransaction"/>
        /// <seealso cref="beginTransaction"/>
        /*public void rollbackTransaction()
        {

            string sStep = "";
            try
            {
                sStep = this.sStep;
                if (this._oException != null)
                    this._oException.addMsg("rollbackTransaction()", sStep);
                if (!this.isConnected)
                    throw new oSqlSpExecuteException("No existe una conexión abierta");
                if (this._stateTransaction != stateTransaction.open)
                    throw new oSqlSpExecuteException("No existe una transacción abierta");
                else
                {
                    this._OleDbTransaction.Rollback();
                    this._OleDbTransaction.Dispose();
                    this._OleDbTransaction = null;
                    this._stateTransaction = stateTransaction.none;
                }
            }
            catch (Exception oEx)
            {
                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
        }*/
        /// <summary>
        /// Confirma la transacción de base de datos.
        /// </summary>
        /// <exception cref="System.Exception">Se eleva cuando existe un error en la conexión base</exception>
        /// <exception cref="Dabitis.Sql.DataManager.oSqlSpExecuteException ">Se eleva cuando existe un error de negocio en la conexión</exception>
        /// <seealso cref="rollbackTransaction"/>
        /// <seealso cref="commitTransaction"/>
        /// <seealso cref="beginTransaction"/>
        /*public void commitTransaction()
        {
            string sStep = "";
            try
            {
                sStep = this.sStep;
                if (this._oException != null)
                    this._oException.addMsg("commitTransaction()", sStep);
                if (!this.isConnected)
                    throw new oSqlSpExecuteException("No existe una conexión abierta");
                if (this._stateTransaction != stateTransaction.open)
                    throw new oSqlSpExecuteException("No existe una transacción abierta");
                else
                {
                    this._OleDbTransaction.Commit(); ;
                    this._OleDbTransaction.Dispose();
                    this._OleDbTransaction = null;
                    this._stateTransaction = stateTransaction.none;
                }
            }
            catch (Exception oEx)
            {
                if (this._oException != null)
                    throw this._oException.HandleException(sStep, oEx);
                else
                    throw oEx;
            }
        }*/
        #endregion//Transaction

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

    /// <summary>
    /// Clase propietaria para manejar excepciones de la clase oSqlSpExecute
    /// </summary>
    public class oSqlSpExecuteException : Exception
    {
        #region constructors
        /// <summary>
        /// Objeto de excepción de la clase oSqlSpExecute. Hereda de Exception Class. 
        /// </summary>
        public oSqlSpExecuteException() :
            base("Excepción sin mensaje definido")
        { }
        //public oSqlSpExecuteException() : base()
        //{
        //}
        /// <summary>
        /// Objeto de excepción de la clase oSqlSpExecute. Hereda de Exception Class. 
        /// </summary>
        /// <param name="message">Mensaje a cargar en la exceptión elevada</param>
        public oSqlSpExecuteException(string message) : base(message)
        {

        }
        #endregion //constructors
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    /// Clase propietaria para construir un mensaje mediante un objeto
    /// </summary>

    public class oSqlMessage : IDisposable
    {
        #region property and members
        private bool Disposed = false;
        private XmlDocument _XmlDocumentMessage = new XmlDocument();
        #endregion //property and members
        #region constructors
        /// <summary>
        /// Clase que representa mensaje a agregar en una petición DSQ
        /// </summary>
        /// <param name="sServiceName">Indica el valor del servicio a invocar</param>
        public oSqlMessage()
        {
            _XmlDocumentMessage.LoadXml("<ROOT/>");
        }
        #endregion//constructors
        #region methods
        /// <summary>
        /// Método para agregar un parametro al mensaje de la petición
        /// </summary>
        /// <param name="name">Indica el nombre del parámetro a agregar</param>
        /// <param name="value">Indica el valor del parámetro a agregar</param>
        public void addService(oSqlMessageService oSqlMessageServiceParam)
        {

            XmlNode XmlDocumentService = _XmlDocumentMessage.ImportNode(
                                                                        oSqlMessageServiceParam.getServiceDocument().FirstChild, true
                                                                        );//Importo un nodo de otro documento al documento actual.
            _XmlDocumentMessage.FirstChild.AppendChild(XmlDocumentService);
        }
        /// <summary>
        /// Retorna el documento XML Interno de la mensajería
        /// </summary>
        public XmlDocument getServiceDocument()
        {
            return _XmlDocumentMessage;
        }
        #endregion //methods
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #region dispose
        /// <summary>
        /// Liberar todos los recursos de su propiedad. También libera todos los recursos que posean sus tipos base; para ello, llama al método Dispose de su tipo primario.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    _XmlDocumentMessage = null;
                    //free any managed resources
                }
            }
            Disposed = true;
        }
        /// <summary>
        /// Destructor de la instancia
        /// </summary>
        ~oSqlMessage()
        {
            Dispose(false);
        }
        #endregion//dispose
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// Clase propietaria para construir un servicio mediante un objeto. Este objeto debe ser agregado a un objeto de tipo oSqlMessage.
        /// </summary>
        public class oSqlMessageService : IDisposable
        {

            #region property and members
            private bool Disposed = false;
            private XmlDocument _XmlDocumentMessage = new XmlDocument();
            #endregion //property and members
            #region constructors
            /// <summary>
            /// Clase que representa mensaje a agregar en una petición DSQ
            /// </summary>
            /// <param name="sServiceName">Indica el valor del servicio a invocar</param>
            public oSqlMessageService(string sServiceName)
            {
                string sRootNode = "";
                sRootNode += "<RECORD>";
                sRootNode += "<SERVICE_NAME>";
                sRootNode += sServiceName;
                sRootNode += "</SERVICE_NAME>";
                sRootNode += "</RECORD>";
                _XmlDocumentMessage.LoadXml(sRootNode);
            }
            #endregion//constructors
            #region property and members
            /// <summary>
            /// Método para agregar un parametro al mensaje de la petición
            /// </summary>
            /// <param name="name">Indica el nombre del parámetro a agregar</param>
            /// <param name="value">Indica el valor del parámetro a agregar</param>
            public void addParameters(string name, string value)
            {
                XmlNode oNewValueElement;
                if (_XmlDocumentMessage.FirstChild.SelectSingleNode(name) != null)//Existe el nodo
                {
                    throw new Exception("El parametro " + name + " ya existe");
                }
                else
                {
                    oNewValueElement = _XmlDocumentMessage.CreateNode(XmlNodeType.Element, name, "");
                    oNewValueElement.InnerText = value;
                    _XmlDocumentMessage.SelectSingleNode("RECORD").AppendChild(oNewValueElement);
                }
            }
            /// <summary>
            /// Retorna el documento interno del mensaje
            /// </summary>
            /// <returns></returns>
            public XmlDocument getServiceDocument()
            {
                return _XmlDocumentMessage;
            }
            #endregion //property and members
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            #region dispose
            /// <summary>
            /// Liberar todos los recursos de su propiedad. También libera todos los recursos que posean sus tipos base; para ello, llama al método Dispose de su tipo primario.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            private void Dispose(bool disposing)
            {
                if (!this.Disposed)
                {
                    if (disposing)
                    {
                        _XmlDocumentMessage = null;
                        //free any managed resources
                    }
                }
                Disposed = true;
            }
            /// <summary>
            /// Destructor de la instancia
            /// </summary>
            ~oSqlMessageService()
            {
                Dispose(false);
            }
            #endregion//dispose
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        }

    }
}
