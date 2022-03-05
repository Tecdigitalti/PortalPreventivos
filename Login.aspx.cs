using Portalpreventivos.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Portalpreventivos
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnIngreso_Click(object sender, EventArgs e)
        {
            Bitacora _bit = new Bitacora();
            DataBase conOB = new DataBase();
            string usuario = "", nombreUsuario = "", msg = "", email="";
            bool respCon = false, login = false;


            if(txtAgente.Text.Trim().Equals("") || txtPassword.Text.Trim().Equals(""))
            {
                lblDetalle.Text = "Usuario y/o Contraseña No ingresado";
            }
            else
            {
                respCon = conOB.NewOra(DataBase.DBType.SimmpapelODBC);

                if (respCon)
                {
                    //Validacion del usuario con el DA
                    conOB.Autenticacion_UsuarioLogin(txtAgente.Text.Trim(), txtPassword.Text.Trim(), ref login, ref msg);

                    if (login)
                    {
                        //Recuperacion de la info del Usuario
                        using (DataTable info = conOB.LogginUsuario(txtAgente.Text.Trim(), txtPassword.Text.Trim()))
                        {
                            conOB.Close();
                            if (info.Rows.Count > 0)
                            {
                                usuario = info.Rows[0]["usuario"].ToString().Trim();
                                nombreUsuario = info.Rows[0]["nombre"].ToString().Trim().ToUpper();
                                email = info.Rows[0]["email"].ToString().Trim();
                            }
                        }

                        if (!usuario.Equals(""))
                        {
                           Session["Usuario"] = usuario;
                           Session["Nombre"] = nombreUsuario;
                           Response.Redirect("~/Default.aspx");
                        }
                        else
                        {
                            lblDetalle.Text = "Usuario / Password incorrecto, favor de validar";
                        }
                    }
                    else
                    {
                        lblDetalle.Text = "Usuario / Password No aparece en el Directorio Activo, favor de validar";
                    }

                }
                else
                {
                    lblDetalle.Text = "Favor de comunicarse con el administrador de sistemas, ocurrio un problema de comunicació con la BD";
                }
            }
            
        }
    }
}