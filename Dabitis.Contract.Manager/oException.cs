using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HandlerException
{
    public class oException : IDisposable
    {

        #region Members
        Dictionary<string, string> iExceptCol = new Dictionary<string, string>();
        private IOLog oLog = null;
        private bool writeLog = false;
        private string _idLog = "";
        private bool disposed = false;
        private int _step = 1000; //se reservan los índices superiores a 1000 para el manejo interno.
        #endregion Members
        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.writeLog)
                        this.oLog.Dispose();

                }
            }

            disposed = true;
        }
        #endregion Dispose
        #region types_struct_enum
        public struct itemException
        {
            public string sMessage;
            public string strToExe;
            public itemException(string sMessage, string strToExe)
            {
                this.sMessage = sMessage;
                this.strToExe = strToExe;
            }
        }
        #endregion types_struct_enum
        #region Constructors
        public oException()
        {
            this.oLog = new IOLog();
        }
        public oException(string sParNameFileLog)
        {
            this.oLog = new IOLog(sParNameFileLog);
            this.writeLog = true;
        }
        public oException(string sParNameFileLog, bool bWriteLog)
        {
            this.oLog = new IOLog(sParNameFileLog);
            this.writeLog = bWriteLog;
        }
        public oException(bool bWriteLog)
        {
            if (bWriteLog)
            {
                this.oLog = new IOLog();
                this.writeLog = true;
            }
        }
        #endregion Constructors
        #region set_and_get
        public string idLog
        {
            get
            {
                return _idLog;
            }
            set
            {
                this._idLog = value;
                this.oLog.idProcess = this._idLog;
            }

        }
        private string sStep
        {
            get
            {
                return (this._step++).ToString();
            }
        }
        #endregion set_and_get
        #region handler_error
        public string addMsg(string strMsg, string index)
        {
            if (this.iExceptCol.ContainsKey(index))//Si existe elimino y regenero.
                this.iExceptCol.Remove(index);
            this.iExceptCol.Add(index, strMsg);
            return index;

        }
        public string addMsg(string strMsg)
        {
            return this.addMsg(strMsg, this.sStep);
        }
        public oHandlerItemException HandleException()
        {
            //determino la última key
            if (this.iExceptCol.Count == 0)
                throw new ArgumentNullException("No existen descripciones asocidas en el objeto oException");
            string index = this.iExceptCol.Last().Key;
            return HandleException(index, null);
        }
        public oHandlerItemException HandleException(Exception oException)
        {
            //determino la última key
            if (this.iExceptCol.Count == 0)
                throw new ArgumentNullException("No existen descripciones asocidas en el objeto oException");
            string index = this.iExceptCol.Last().Key;
            return HandleException(index, oException);
        }
        public oHandlerItemException HandleException(string index)
        {
            return this.HandleException(index, null);
        }
        public oHandlerItemException HandleException(string index, Exception oException)
        {
            Exception oExAux = null;
            string sMsgException;
            try
            {
                //Si no existe el índice o el mismo está vacío.
                if (index.Trim().Length == 0 || !this.iExceptCol.ContainsKey(index))
                    throw new IndexOutOfRangeException("No se puede determinar un mensaje asociado a ese índice[index:" + index + "]");


                sMsgException = this.iExceptCol[index].ToString();

                if (oException != null)
                {
                    //Primer mensaje
                    sMsgException += "-->" + oException.Message + "{" + oException.GetType().FullName + "}"; ;
                    //mensajes internos
                    oExAux = oException.InnerException;
                    while (oExAux != null)
                    {
                        sMsgException += "-->" + oExAux.Message + "{" + oExAux.GetType().FullName + "}";
                        oExAux = oExAux.InnerException;
                    }
                }


                if (this.writeLog)
                {
                    this.oLog.recordLogLine(DateTime.Now.ToString("yyyyMMdd hhmmss ") + sMsgException);
                }
                return new oHandlerItemException(sMsgException, null);
            }
            catch (IndexOutOfRangeException oEx)
            {
                throw oEx;
            }
            catch (Exception oEx)
            {
                return new oHandlerItemException(oEx.Message, oEx.InnerException);
            }
        }
        #endregion handler_error
    }
    public class oHandlerItemException : Exception
    {
        public oHandlerItemException() : base() { }
        public oHandlerItemException(string message) : base(message) { }
        public oHandlerItemException(string message, System.Exception inner) : base(message, inner) { }
        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected oHandlerItemException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        { }
    }
    public class IOLog : IDisposable
    {
        #region Members
        private string _iDProcess = "";
        private FileStream _fStream = null;
        private string _sFileNameException = "";
        private string currentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        private bool disposed = false;
        #endregion
        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this._fStream != null)
                        this._fStream.Close();
                }
            }
            disposed = true;
        }
        #endregion Dispose
        #region Constructors
        public IOLog()
        {
            this.setNewFileName();
            this.setFileStream();
        }
        public IOLog(string sNameFile)
        {
            if (sNameFile.Trim().Length == 0)
                this.setNewFileName();
            else
                this.FileName = sNameFile;

            this.setFileStream();
        }
        #endregion Constructors
        #region set_and_get"
        public string FileName
        {
            get
            {
                return this._sFileNameException;
            }
            set
            {
                this._sFileNameException = value;
            }
        }
        public void setFileStream()
        {
            this._fStream = new FileStream(this.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

        }
        public string idProcess
        {
            get
            {
                return _iDProcess;
            }
            set
            {
                this._iDProcess = value;
            }
        }
        public void setNewFileName()
        {
            if (!Directory.Exists(currentDirectory + "\\Log")) Directory.CreateDirectory(currentDirectory + "\\Log");
            this._sFileNameException += currentDirectory + "\\log\\oException";
            this._sFileNameException += DateTime.Now.ToString("yyyyMMdd") + ".log";
        }
        #endregion
        #region Record Log
        public void recordLogLine(string sMessage)
        {
            sMessage = (this._iDProcess + (this._iDProcess.Length > 0 ? " " : "") + sMessage);
            Byte[] writeArray = System.Text.Encoding.Default.GetBytes(sMessage + '\n');

            this._fStream.Position = this._fStream.Length;
            this._fStream.Write(writeArray, 0, writeArray.Length);
            this._fStream.Flush();
        }
        #endregion Record Log

    }
}
