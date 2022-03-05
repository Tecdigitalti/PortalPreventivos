using Portalpreventivos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Portalpreventivos
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.RawUrl.Contains("exit"))
                {
                    //Session["objUser"] = null;
                    //UnityOB objInt = (UnityOB)Session["SesionOB"];

                    //if (objInt != null)
                    //{
                    //    objInt.DisconnectionOB();
                    //}
                    Response.Redirect("~/Login.aspx");
                }

                //SE ESTABLECE QUE NO ALMACENE LOS DATOS EN CACHE
                Response.ExpiresAbsolute = DateTime.Now.AddDays(-1);
                Response.AddHeader("pragma", "no-cache");
                Response.AddHeader("cache-control", "private");
                Response.AddHeader("Cache-Control", "must-revalidate");
                Response.AddHeader("Cache-Control", "no-cache");
                Response.CacheControl = "no-cache";
                foreach (System.Collections.DictionaryEntry entry in HttpContext.Current.Cache)
                {
                    HttpContext.Current.Cache.Remove((string)entry.Key);
                }

                if (Session["Login"] == null)
                {
                    lblNombreUsuario.InnerText = "";
                    lblMenuUsuario.InnerHtml = "";

                }
                //else
                //{
                //    DataTable dt = Session["Login"] as DataTable;
                //    string st_NombreUsuario = dt.Rows[0]["Nombre"].ToString();
                //    lblNombreUsuario.InnerHtml = st_NombreUsuario;
                //    lblMenuUsuario.InnerHtml = "<a href=\"Inicio.aspx\" style=\"color: #005568; font-size:medium\">Inicio</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"Dashboard.aspx\" style=\"color: #005568; font-size:medium\">Calificación</a>";
                //}
            }
            catch (Exception) { }
        }
    }
}