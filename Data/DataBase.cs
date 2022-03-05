using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;

namespace Portalpreventivos.Data
{
    public class DataBase
    {
        public enum DBType
        {
            OnBase,
            SII,
            Presupuesto,
            Simmpapel,
            SimmpapelODBC,
            SIIADO,
            SIIDevODBC,
            GMX_DESA
        }

        public OdbcConnection ConnDB { get; set; }
        //Bitacora logs = new Bitacora();
        SMTP smtp = new SMTP();

        public bool NewOra(DBType db_Type)
        {
            string strOutput = "";


            switch (db_Type)
            {
                case DBType.OnBase: //BD OnBase
                    strOutput = ConfigurationManager.ConnectionStrings["OnBase"].ConnectionString;
                    break;
                case DBType.Simmpapel: // BD Simmpapel
                    strOutput = ConfigurationManager.ConnectionStrings["Simmpapel"].ConnectionString;
                    break;
                case DBType.SimmpapelODBC: // BD Simmpapel (ODBC)
                    strOutput = ConfigurationManager.ConnectionStrings["SimmpapelODBC"].ConnectionString;
                    break;
                case DataBase.DBType.SIIDevODBC:
                    strOutput = ConfigurationManager.ConnectionStrings["SIIDevODBC"].ConnectionString;
                    break;
                case DataBase.DBType.GMX_DESA:
                    strOutput = ConfigurationManager.ConnectionStrings["GMX_DESA"].ConnectionString;
                    break;
            }
            Bitacora bita = new Bitacora();

            if (string.IsNullOrWhiteSpace(strOutput))
            {
                throw new Exception("No se encuentra la conexión en el archivo de configuración de la aplicación.");
                return false;
            }
            else
            {
                //int intConexion = 0;

                try
                {
                    ConnDB = new OdbcConnection(strOutput);
                    ConnDB.Open();
                    return true;
                }
                catch (TimeoutException ex)
                {

                    var msgError = "Error de Time Out - no hay respuesta de la BD " + ex.Message;
                    //smtp.Fn_EnviaEmailError(ex, msgError);
                    bita.agregaError(msgError + ex.GetType());
                    return false;
                }
                catch (Exception ex)
                {
                    //Bitacora bita = new Bitacora();
                    var msgError = "Error al intentar conectarse a la BD " + db_Type.ToString();
                    //smtp.Fn_EnviaEmailError(ex, msgError);
                    bita.agregaError(msgError + ex.StackTrace.Trim());
                    return false;

                }


            }
        }

        public int Execute(string SQL)
        {
            int lngRecords;
            try
            {
                OdbcCommand cmdQuery = new OdbcCommand
                {
                    Connection = ConnDB,
                    CommandText = SQL,
                    CommandType = System.Data.CommandType.Text
                };
                lngRecords = cmdQuery.ExecuteNonQuery();
                cmdQuery.Dispose();
            }
            catch (Exception ex)
            {
                lngRecords = 0;
            }

            return lngRecords;
        }

        public int ExecuteStoredProc(string SQL)
        {
            int lngRecords;
            OdbcCommand cmdQuery = new OdbcCommand
            {
                Connection = ConnDB,
                CommandText = SQL,
                CommandType = System.Data.CommandType.StoredProcedure
            };
            lngRecords = cmdQuery.ExecuteNonQuery();
            cmdQuery.Dispose();
            return lngRecords;
        }

        public OdbcDataReader GetDataReader(string SQL)
        {
            OdbcDataReader dr;
            using (OdbcCommand cmdQuery = new OdbcCommand())
            {

                cmdQuery.Connection = ConnDB;
                cmdQuery.CommandText = SQL;
                cmdQuery.CommandType = System.Data.CommandType.Text;
                dr = cmdQuery.ExecuteReader();
            }
            return dr;
        }

        public DataTable GetDataTable(string SQL)
        {
            DataTable tbl = new DataTable();
            Bitacora _bit = new Bitacora();

            OdbcCommand cmdQuery = new OdbcCommand
            {
                Connection = ConnDB,
                CommandText = SQL,
                CommandType = System.Data.CommandType.Text
            };

            OdbcDataAdapter da = new OdbcDataAdapter(cmdQuery);
            try
            {
                da.Fill(tbl);
            }
            catch (TimeoutException tex)
            {
                _bit.agregaError("Error TimeOUT:" + tex.Message);
            }
            catch (Exception ex)
            {
                _bit.agregaError("Error :" + ex.Message);
            }


            return tbl;
        }

        public DataTable GetDataTable2(string SQL)
        {
            DataTable tbl = new DataTable();

            OdbcCommand cmdQuery = new OdbcCommand
            {
                Connection = ConnDB,
                CommandText = SQL,
                CommandType = System.Data.CommandType.StoredProcedure
            };

            OdbcDataAdapter da = new OdbcDataAdapter(cmdQuery);
            da.Fill(tbl);

            return tbl;
        }

        public void Close()
        {
            if (ConnDB != null && ConnDB.State != ConnectionState.Closed)
                ConnDB.Close();

        }

        public void AutenticacionUsuario(string usuario, string password, ref bool login, ref string msg)
        {
            UnityOB unit_OB = new UnityOB();
            string msgAD = "";
            bool loginAD = false;

            //var conexion = unit_OB.ConnectionOB_AD(logIn, password, ref login, ref msg);
            unit_OB.ConnectionOB_AD(usuario, password, ref loginAD, ref msgAD);

            if (loginAD)
            {
                unit_OB.DisconnectionOB();
                login = loginAD;
                msg = msgAD;
                //return (true, conexion.msg);
            }
            else
            {
                login = false;
                msg = "Nombre de usuario o contraseña incorrectos. Favor de verificar.";
                //return (false, "Nombre de usuario o contraseña incorrectos. Favor de verificar.");
            }

        }

        public DataTable recuperaInfoUsuario(string usuario)
        {
            DataTable tblUser;
            var query = "SELECT  VU.[Nombre], "
                       + " [Estatus]"
                       + " FROM Vw_UsuariosAD VU"
                       + " WHERE UPPER(ClaveUsuario) = UPPER('" + usuario + "')";
            tblUser = GetDataTable(query);
            return tblUser;
        }

        public DataTable LogginUsuario(string usuario, string password)
        {
            DataTable tblGrupo;
            var query = "SELECT ua.username as usuario, ua.realname as nombre, ua.emailaddress as email FROM OnBase.hsi.useraccount ua WHERE ua.username = '" + usuario + "'";
                        
            tblGrupo = GetDataTable(query);

            return tblGrupo;
        }

        public void Autenticacion_UsuarioLogin(string usuario, string password, ref bool login, ref string msg)
        {
            UnityOB unit_OB = new UnityOB();
            string msgAD = "";
            bool loginAD = false;

            var bValidaFecha = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["ValidacionContraseniaIngreso"]);
            if (!bValidaFecha)
            {
                login = true;
                msgAD = "123";
                msg = msgAD;
                //return (true, "123"); ;
            }
            //var conexion = unit_OB.ConnectionOB_AD(logIn, password, ref login, ref msg);
            unit_OB.ConnectionOB_AD(usuario, password, ref loginAD, ref msgAD);

            if (loginAD)
            {
                unit_OB.DisconnectionOB();
                login = loginAD;
                msg = msgAD;
                //return (true, conexion.msg);
            }
            else
            {
                login = false;
                msg = "Nombre de usuario o contraseña incorrectos. Favor de verificar.";
                //return (false, "Nombre de usuario o contraseña incorrectos. Favor de verificar.");
            }

        }

    }
}