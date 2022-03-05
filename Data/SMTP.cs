using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Portalpreventivos.Data
{
    public class SMTP
    {
        private string emailAdmon = ConfigurationManager.AppSettings["EmailAdmon"].ToString();

        public SmtpClient SMTPClient { get; set; }

        public MailMessage MailMessage { get; set; }

        public string Subject { get; set; }

        private string emailTo;

        public string EmailTo
        {
            get { return emailTo; }
            set
            {
                emailTo = value;
                To = new MailAddress(emailTo);
            }
        }

        public MailAddress To { get; set; }

        public string Body { get; set; }

        //Bitacora logs = new Bitacora();

        public SMTP()
        {
            SMTPClient = new SmtpClient();
            MailMessage = new MailMessage();
        }

        /// <summary>
        /// Constructor de objeto SMTP
        /// </summary>
        /// <param name="subject">string: Asunto del correo</param>
        /// <param name="emailTo">string: Direccion(es) de correo de destinatario(s)</param>
        /// <param name="body">string: Cuerpo del mensaje en formato</param>
        public SMTP(string subject, string emailTo, string body)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            EmailTo = emailTo ?? throw new ArgumentNullException(nameof(emailTo));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }


        /// <summary>
        /// Envía un correo electrónico
        /// </summary>
        /// <param name="st_URLAttachment">URL de archivo adjunto.</param>

        public void Fn_EnvíaEMail(string st_URLAttachment, bool algo)
        {
            SMTPClient = new SmtpClient();
            MailMessage = new MailMessage();

            #region Vista del mensjae en HTML

            StringBuilder msg_Body = new StringBuilder();

            msg_Body.Append("<div style='font-family: Tw Cen MT; '>");
            msg_Body.Append("<table cellspacing='0' cellpadding='40' align='center' style='font-family: Tw Cen MT; width: 80%; font-size: 11pt; color: #003a5d; margin-top: 2px; text-align:justify;'>");
            msg_Body.Append("<tr>");
            msg_Body.Append("<td style='width:70%; height:100%; border-style: solid; border-width: 2px;  border-color: #023c5b;' rowspan='3' align='center' valign='top'>");
            msg_Body.Append("<p style='font-size: large; text-align: left; color: #003a5d; font-family: Tw Cen MT;'>");
            msg_Body.Append("<br/>");
            msg_Body.Append("<span style='font-size: xx-large; font-weight: bold; text-align: left; font-family: Tw Cen aMT; color: #003a5d;'>GMX Seguros</span>");
            msg_Body.Append("<br/> Juntos el riesgo es menor<sup style='font-size:x-small;'>MR</sup>");
            msg_Body.Append("</p>");
            msg_Body.Append("<br>");
            msg_Body.Append("<br/><span align = \"left\"  style='font-size: 18; text-align:justify; color: #003a5d; font-family: Tw Cen MT;'> " + Regex.Replace(Body, @"\r\n?|\n", "<br/>") + " </span> <br/>");
            msg_Body.Append("</td><td style='width:30%; height:100%;' align='center' valign='top' rowspan='3' bgcolor='#023c5b'>");
            msg_Body.Append("<table style='width:100%; height:100%; text-align:center; font-family:Arial, Helvetica, sans-serif;'>");
            msg_Body.Append("<tr><td><img src='cid:logoGMX' width='140'/></td></tr>");
            msg_Body.Append("<tr><td style='margin-top: 40px; height: 100px;'>");
            msg_Body.Append("<br/></td></tr>");
            msg_Body.Append("</table>");
            msg_Body.Append("</td>");
            msg_Body.Append("</tr>");
            msg_Body.Append("</table>");
            msg_Body.Append("</div>");


            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(msg_Body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html);

            //Incrustamos logo en la vista html
            LinkedResource img = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~/Images/PVL_AJUSTADOR_V.png"), "image/png")
            {
                ContentId = "logoGMX"
            };
            htmlView.LinkedResources.Add(img);
            #endregion


            #region Vista del mensaje en texto plano
            var msg_BodyPlano = new StringBuilder();
            msg_BodyPlano.AppendLine("GMX Seguros:");
            msg_BodyPlano.AppendLine("Ocurrió un error en el portal de Administración de Catálogos - Compras");
            msg_BodyPlano.AppendLine("Error:" + Body);
            AlternateView textoPlano = AlternateView.CreateAlternateViewFromString(msg_BodyPlano.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain);
            #endregion

            MailMessage.To.Add(To);
            MailMessage.Subject = Subject;
            MailMessage.IsBodyHtml = true;
            //Se agregan vistas alternas
            MailMessage.AlternateViews.Add(textoPlano);
            MailMessage.AlternateViews.Add(htmlView);

            //Add an attachment
            if (!string.IsNullOrWhiteSpace(st_URLAttachment))
            {
                MailMessage.Attachments.Add(new Attachment(st_URLAttachment));
            }


            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            //Send the Email
            try
            {
                SMTPClient.Send(MailMessage);

            }
            catch (Exception ex)
            {
                //logs.agregaError("Ocurrió un error al enviar el correo electrónico" + ex.ToString());
                throw ex;

            }

        }

        public string Fn_EnvíaEMail(string st_URLAttachment = null)
        {
            string Error = "";

            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["SmtpClient"]);
                mail.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["MailAddress"]);
                mail.To.Add(To);
                mail.Subject = Subject;

                #region Vista del mensjae en HTML

                StringBuilder msg_Body = new StringBuilder();

                msg_Body.Append("<div style='font-family: Tw Cen MT; '>");
                msg_Body.Append("<table cellspacing='0' cellpadding='40' align='center' style='font-family: Tw Cen MT; width: 80%; font-size: 11pt; color: #003a5d; margin-top: 2px; text-align:justify;'>");
                msg_Body.Append("<tr>");
                msg_Body.Append("<td style='width:70%; height:100%; border-style: solid; border-width: 2px;  border-color: #023c5b;' rowspan='3' align='center' valign='top'>");
                msg_Body.Append("<p style='font-size: large; text-align: left; color: #003a5d; font-family: Tw Cen MT;'>");
                msg_Body.Append("<br/>");
                msg_Body.Append("<span style='font-size: xx-large; font-weight: bold; text-align: left; font-family: Tw Cen aMT; color: #003a5d;'>GMX Seguros</span>");
                msg_Body.Append("<br/> Juntos el riesgo es menor<sup style='font-size:x-small;'>MR</sup>");
                msg_Body.Append("</p>");
                msg_Body.Append("<br>");
                msg_Body.Append("<br/><span align = \"left\"  style='font-size: 18; text-align:justify; color: #003a5d; font-family: Tw Cen MT;'> " + Regex.Replace(Body, @"\r\n?|\n", "<br/>") + " </span> <br/>");
                msg_Body.Append("</td><td style='width:30%; height:100%;' align='center' valign='top' rowspan='3' bgcolor='#023c5b'>");
                msg_Body.Append("<table style='width:100%; height:100%; text-align:center; font-family:Arial, Helvetica, sans-serif;'>");
                msg_Body.Append("<tr><td><img src='cid:logoGMX' width='140'/></td></tr>");
                msg_Body.Append("<tr><td style='margin-top: 40px; height: 100px;'>");
                msg_Body.Append("<br/></td></tr>");
                msg_Body.Append("</table>");
                msg_Body.Append("</td>");
                msg_Body.Append("</tr>");
                msg_Body.Append("</table>");
                msg_Body.Append("</div>");


                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(msg_Body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html);

                //Incrustamos logo en la vista html
                LinkedResource img = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~/Images/logo.png"), "image/png")
                {
                    ContentId = "logoGMX"
                };
                htmlView.LinkedResources.Add(img);
                #endregion


                #region Vista del mensaje en texto plano
                var msg_BodyPlano = new StringBuilder();
                msg_BodyPlano.AppendLine("GMX Seguros:");
                msg_BodyPlano.AppendLine("Ocurrió un error en el portal de Caratula");
                msg_BodyPlano.AppendLine("Error:" + Body);
                AlternateView textoPlano = AlternateView.CreateAlternateViewFromString(msg_BodyPlano.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain);
                #endregion
                //oba.g_app.Diagnostics.Write("Body:" + mail.Body);
                //oba.DisconnectionOB();
                mail.IsBodyHtml = true;
                //Se agregan vistas alternas
                //mail.AlternateViews.Add(textoPlano);
                mail.AlternateViews.Add(htmlView);

                string Puerto = System.Configuration.ConfigurationManager.AppSettings["Port"];
                SmtpServer.Port = Convert.ToInt16(Puerto);
                SmtpServer.Credentials = new System.Net.NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["NetworkCredentialUsr"], System.Configuration.ConfigurationManager.AppSettings["NetworkCredentialPwd"]);
                SmtpServer.EnableSsl = false;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Error;
        }

        public void Fn_EnviaEmailError(Exception ex, string st_MsgError)
        {
            if (ex == null)
                ex = new Exception("No existe una excepción,");

            StringBuilder st_Body = new StringBuilder();
            st_Body.AppendLine("Error:           " + st_MsgError);
            st_Body.AppendLine("Mensaje:         " + ex.Message ?? string.Empty);
            st_Body.AppendLine("Source:          " + ex.Source ?? string.Empty);
            st_Body.AppendLine("Pila de eventos: " + ex.StackTrace ?? string.Empty);


            if (ex.InnerException != null)
            {
                st_Body.AppendLine(Environment.NewLine + "== Excepción Interna ==");
                st_Body.AppendLine("   Mensaje:         " + ex.InnerException.Message ?? string.Empty);
                st_Body.AppendLine("   Source:          " + ex.InnerException.Source ?? string.Empty);
                st_Body.AppendLine("   Pila de eventos: " + ex.InnerException.StackTrace ?? string.Empty);
            }

            var smtp = new SMTP("Ocurrió un error en el portal de Admón. de Catálogos", emailAdmon, st_Body.ToString());

            smtp.Fn_EnvíaEMail();
        }

    }
}