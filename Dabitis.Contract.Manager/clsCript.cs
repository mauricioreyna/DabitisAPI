using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace encrypData
{
    public class EncrypData
    {

        #region "atributos"
        //ENCRIPTACION DE CONTRASEÑAS
        byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        byte[] iv = { 48, 47, 51, 36, 87, 145, 12, 247 };
        //ENCRIPTACION DE PAQUETES
        private DESCryptoServiceProvider DesCSP;
        //byte[] _abIV = {23, 196, 36, 156, 242, 181, 172, 95};
        byte[] _abIV = { 0, 0, 0, 0, 0, 0, 0, 0 };
        //byte[] _abKey = {238, 154, 47, 44, 96, 196, 7, 48};
        byte[] _abKey = { 0, 0, 0, 0, 0, 0, 0, 0 };
        #endregion

        #region "propiedades"
        public byte[] ClaveInicializador
        {
            set { iv = value; }
        }

        public byte[] Clave
        {
            set { key = value; }
        }
        #endregion

        #region "metodos"
        public EncrypData()
        {
            string strKeys;
            strKeys = "23, 196, 36, 156, 242, 181, 172, 95";
            string[] aKeys = strKeys.Split(',');

            //Carga de la primer clave
            for (int i = 0; i < aKeys.Length; i++)
                this._abIV[i] = byte.Parse(aKeys[i]);

            //Carga de la segunda clave
            strKeys = "238, 154, 47, 44, 96, 196, 7, 48";
            aKeys = strKeys.Split(',');
            for (int i = 0; i < aKeys.Length; i++)
                this._abKey[i] = byte.Parse(aKeys[i]);
        }

        public byte[] Encrypt(string plainText)
        {
            UTF8Encoding utf8encoder = new UTF8Encoding();
            byte[] inputInBytes = utf8encoder.GetBytes(plainText);
            TripleDESCryptoServiceProvider tdesProvider = new TripleDESCryptoServiceProvider();
            ICryptoTransform cryptoTransform = tdesProvider.CreateEncryptor(this.key, this.iv);
            MemoryStream encryptedStream = new MemoryStream();
            CryptoStream cryptStream = new CryptoStream(encryptedStream, cryptoTransform, CryptoStreamMode.Write);
            cryptStream.Write(inputInBytes, 0, inputInBytes.Length);
            cryptStream.FlushFinalBlock();
            encryptedStream.Position = 0;
            byte[] result = new byte[(encryptedStream.Length - 1 + 1)];
            encryptedStream.Read(result, 0, (int)encryptedStream.Length);
            cryptStream.Close();
            return result;
        }

        public string EncryptPacket(ref string sPaquete)
        {
            string sRetorna = "";
            byte[] abMensaje = System.Text.Encoding.UTF8.GetBytes(sPaquete);
            DesCSP = new DESCryptoServiceProvider();
            DesCSP.Mode = CipherMode.CBC;
            DesCSP.Padding = PaddingMode.PKCS7;
            ICryptoTransform DesEncrypt = DesCSP.CreateEncryptor(_abKey, _abIV);
            MemoryStream mstmMensaje;
            CryptoStream CrStMensaje;
            try
            {
                mstmMensaje = new MemoryStream();
                CrStMensaje = new CryptoStream(mstmMensaje, DesEncrypt, CryptoStreamMode.Write);
                CrStMensaje.Write(abMensaje, 0, abMensaje.Length);
                CrStMensaje.FlushFinalBlock();
                mstmMensaje.Position = 0;
                byte[] abResultado = new byte[(mstmMensaje.Length - 1 + 1)];
                mstmMensaje.Read(abResultado, 0, (int)mstmMensaje.Length);
                CrStMensaje.Close();
                sRetorna = btEncryptToStr(ref abResultado);
                abResultado = null;
            }
            catch (Exception Ex)
            {
                sRetorna = "ERROR:" + Ex.Message.ToString();
            }
            finally
            {
                abMensaje = null;
                DesCSP = null;
                DesEncrypt = null;
                CrStMensaje = null;
                mstmMensaje = null;
            }
            return sRetorna;

        }

        public string Decrypt(byte[] inputInBytes)
        {
            UTF8Encoding utf8encoder = new UTF8Encoding();
            TripleDESCryptoServiceProvider tdesProvider = new TripleDESCryptoServiceProvider();
            ICryptoTransform cryptoTransform = tdesProvider.CreateDecryptor(this.key, this.iv);
            MemoryStream decryptedStream = new MemoryStream();
            CryptoStream cryptStream = new CryptoStream(decryptedStream, cryptoTransform, CryptoStreamMode.Write);
            UTF8Encoding utf8 = new UTF8Encoding();
            if ((inputInBytes) != null)
            {
                cryptStream.Write(inputInBytes, 0, inputInBytes.Length);
                cryptStream.FlushFinalBlock();
                decryptedStream.Position = 0;
                byte[] result = new byte[(decryptedStream.Length - 1 + 1)];
                decryptedStream.Read(result, 0, (int)decryptedStream.Length);
                cryptStream.Close();
                return utf8.GetString(result);
            }
            else
            {
                return "";
            }
        }

        public string DecryptPacket(ref string sPaquete)
        {
            string sRetorna = "";
            byte[] abMensaje = strEncryptToBytes(ref sPaquete);
            if ((abMensaje) != null)
            {
                DesCSP = new DESCryptoServiceProvider();
                DesCSP.Mode = CipherMode.CBC;
                DesCSP.Padding = PaddingMode.PKCS7;
                ICryptoTransform DesDecrypt = DesCSP.CreateDecryptor(_abKey, _abIV);
                MemoryStream mstmMensaje;
                CryptoStream CrStMensaje;
                try
                {
                    mstmMensaje = new MemoryStream();
                    CrStMensaje = new CryptoStream(mstmMensaje, DesDecrypt, CryptoStreamMode.Write);
                    CrStMensaje.Write(abMensaje, 0, abMensaje.Length);
                    CrStMensaje.FlushFinalBlock();
                    mstmMensaje.Position = 0;
                    byte[] abResultado = new byte[(mstmMensaje.Length - 1 + 1)];
                    mstmMensaje.Read(abResultado, 0, (int)mstmMensaje.Length);
                    CrStMensaje.Close();
                    sRetorna = System.Text.Encoding.UTF8.GetString(abResultado);
                    abResultado = null;
                }
                catch (Exception Ex)
                {
                    sRetorna = "ERROR: " + Ex.Message;
                }
                finally
                {
                    DesCSP = null;
                    DesDecrypt = null;
                    mstmMensaje = null;
                    CrStMensaje = null;
                }
                abMensaje = null;
            }
            return sRetorna;
        }

        public string btEncryptToStr(ref byte[] bt)
        {
            int arrayLen = bt.GetUpperBound(0);
            string[] strEnc = new String[arrayLen + 1];
            for (int i = 0; i <= arrayLen; i++)
                strEnc[i] = (bt[i].ToString());
            return String.Join("-", strEnc);
        }

        public byte[] strEncryptToBytes(ref string Strbt)
        {
            System.Collections.ArrayList alClave = new System.Collections.ArrayList();
            byte[] clave = null;
            string[] aData;
            try
            {
                if (Strbt != "")
                {
                    //LO CONVIERTO A UN ARRAY DE BYTES
                    aData = Strbt.Split(new Char[] { '-' });
                    foreach (string s in aData)
                        alClave.Add(byte.Parse(s));

                }
                clave = new byte[alClave.Count];
                Array.Copy(alClave.ToArray(), clave, clave.Length);
            }
            catch (Exception e)
            {
                return null;
            }
            return clave;
        }
        #endregion
    }
}