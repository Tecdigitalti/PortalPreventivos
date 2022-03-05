using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Portalpreventivos.Data
{
    public class Bitacora
    {

        public void agregaBitacora(string mensaje)
        {
            try
            {
                string rutaTemp = ConfigurationManager.AppSettings["Ruta_Log"];// ConfigurationManager.AppSettings["Ruta_Log"];
                string strRutaBitacora2 = System.Web.HttpContext.Current.Server.MapPath("~/");
                TextWriter tw2 = new StreamWriter(rutaTemp + "\\Bitacora_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", true);
                tw2.WriteLine(DateTime.Now.ToString() + " - " + mensaje);
                tw2.Close();
            }
            catch (Exception)
            { }
        }

        public void agregaError(string mensaje)
        {
            try
            {
                string rutaTemp = ConfigurationManager.AppSettings["Ruta_Log"];
                string strRutaBitacora2 = System.Web.HttpContext.Current.Server.MapPath("~/");
                TextWriter tw2 = new StreamWriter(rutaTemp + "\\Errores_" + DateTime.Now.ToString("ddMMyyyy") + ".txt", true);
                tw2.WriteLine(DateTime.Now.ToString() + " - " + mensaje);
                tw2.Close();
            }
            catch (Exception)
            {

            }
        }


    }
}