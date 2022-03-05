<%@ Page Title="Portal Preventivos" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Portalpreventivos.Login" %>
<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="head">
    <link rel="stylesheet" href="Styles/jquery-ui-1.8.16.custom.css" />
    <script type="text/javascript" src="Scripts/jquery-1.7.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.16.custom.js"></script>
    <script src="Scripts/jquery-3.4.1.min.js" type="text/javascript"></script> 

</asp:Content>

<asp:Content runat="server"  ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <script type="text/javascript">
            function ShowMessage(message, messagetype) {
                var cssclass;
                switch (messagetype) {
                    case 'Success':
                        cssclass = 'alert-success'
                        break;
                    case 'Error':
                        cssclass = 'alert-danger'
                        break;
                    case 'Warning':
                        cssclass = 'alert-warning'
                        break;
                    default:
                        cssclass = 'alert-info'
                }
                $('#<%=Mensajes.ClientID %>').append('<div id="alert_div" style="margin: 0 0.5%; -webkit-box-shadow: 3px 4px 6px #999;" class="alert fade in ' + cssclass + '"><a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><span>' + message + '</span></div>');

            setTimeout(function () {
                $("#alert_div").remove();
            }, 4000);
        }
        </script>
        <script>
            $(document).ready(function () {
                $("#navBar").hide();
            });
        </script>
    <div class="cssEncabezado">
            <div class="col-md-2 col-xs-2 cssImgEncabezado text-center">
                <asp:Image runat="server" ImageUrl="~/Imagenes/logo.png" Width="111px" Height="80px" />
            </div>
            <div class="col-md-10 col-xs-10 text-center">
                <label id="lblNombreUsuario" class="pull-right" style="color: white; padding-top: 5px; font-size: small" runat="server"></label>
            </div>
            <div class="col-xs-12 col-sm-12 col-md-10 col-xs-12">
                <br />
                <div class="cssTituloEncabezado">Portal de Preventivos</div>
                <label id="lblMenuUsuario" class="pull-left" style="color: #005568; padding-top: 18px; font-size: small" runat="server"></label>
            </div>
    </div>
    <div>
        <form runat="server">
            <asp:ScriptManager runat="server" EnablePageMethods="true"></asp:ScriptManager>
            <asp:UpdateProgress ID="updateProgress" runat="server" AssociatedUpdatePanelID="UdPnlLogin">
                <ProgressTemplate>
                    <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
                        <div class="cssModalDialog">
                            <span class="failureNotification" style="font-size: 17px">Cargando...</span>
                            <br />
                            <br />
                            Estamos trabajando. Permítenos
                            <br />
                            procesar tu información.
                            <br />
                        </div>
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>
            <div runat="server" id="Mensajes"></div>
            <asp:UpdatePanel ID="UdPnlLogin" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnIngreso" />
                </Triggers>
                <ContentTemplate>
                    <div>
                        <fieldset class="login">
                            <div class="row form-group">
                                <div class="col-xs-12 col-sm-12 col-lg-12">
                                    <div class="row form-group">
                                        <div class="text-center cssFontPage">
                                            <h4>INICIAR SESIÓN</h4>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-xs-4 col-sm-4 col-lg-4 table-col" style="margin-top: auto; margin-bottom: auto;">
                                            <asp:Image runat="server" ImageUrl="~/Imagenes/logo_nuevo_gmx.png" Width="100%" Height="100%" />
                                        </div>
                                        <div class="col-xs-7 col-sm-7 col-lg-7">
                                            <div class="row">
                                                <div class="row form-group">
                                                    <asp:Label ID="UserNameLabel" class="form-control-static control-label col-sm-5 " runat="server" Style="text-align: right;" AssociatedControlID="txtAgente">Usuario:</asp:Label>                                                    <div class="col-sm-6">
                                                        <asp:TextBox ID="txtAgente" runat="server" CssClass="form-control input-sm" MaxLength="120" autocomplete="off" Width="100%" AutoCompleteType="Disabled"></asp:TextBox>
                                                    </div>
                                                    <div class="col-sm-1">
                                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="txtAgente"
                                                            CssClass="failureNotification" ErrorMessage="Usuario requerido." ToolTip="La Clave del usuario es requerido."
                                                            ValidationGroup="IngresoLogin" Display="Dynamic">*</asp:RequiredFieldValidator>
                                                    </div>
                                                </div>
                                                <div class="row form-group">
                                                    <asp:Label ID="PasswordLabel" class="form-control-static control-label col-sm-5" runat="server" Style="text-align: right;" AssociatedControlID="txtPassword">Contraseña:</asp:Label>
                                                    <div class="col-sm-6">
                                                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control input-sm passwordEntry" TextMode="Password" Width="100%" autocomplete="off" AutoCompleteType="Disabled"></asp:TextBox>
                                                    </div>
                                                    <div class="col-sm-1">
                                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="txtPassword"
                                                            CssClass="failureNotification" ErrorMessage="Contraseña es requerida." ToolTip="Contraseña es requerida."
                                                            ValidationGroup="IngresoLogin" Display="Dynamic">*</asp:RequiredFieldValidator>
                                                    </div>
                                                </div>
                                                <div class="row form-group">
                                                    <div class="col-xs-offset-6 col-sm-offset-7 col-lg-offset-8" style="margin-top: 20px">
                                                        <asp:Button ID="btnIngreso" runat="server" Text="Iniciar sesión" ValidationGroup="IngresoLogin" CssClass="btn btn-xs buttonAjustable" OnClick="btnIngreso_Click" />
                                                    </div>
                                                </div>
                                                <div class="row form-group">
                                                    <span class="text-danger">
                                                        <asp:Label ID="lblDetalle" runat="server" Text=""></asp:Label></span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row form-group" runat="server" visible="false">
                                        <div class="col-sm-offset-2 col-sm-3"></div>
                                        <div class="col-sm-3">
                                            <asp:CheckBox ID="chk_GuardaSesion" runat="server" />&nbsp;&nbsp;&nbsp;
                                            <asp:Label ID="lblGuardaSesion" class="control-label " runat="server" AssociatedControlID="chk_GuardaSesion">No cerrar sesión</asp:Label>
                                        </div>
                                        <div class="col-sm-1">
                                        </div>
                                    </div>
                                    <div class="row form-group">
                                        <div class="col-sm-offset-3 col-sm-8">
                                            <asp:ValidationSummary ID="LoginUserValidationSummary" ValidationGroup="IngresoLogin" DisplayMode="BulletList" runat="server" CssClass="failureNotification" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </fieldset>

                        <div runat="server" class="loginSoporte">
                            <div class="col-sm-12 col-md-12 col-lg-12 row form-group text-center" style="margin-left: 5px;">
                                <div>
                                    <br />
                                    <br />
                                    <label class="control-label">
                                        <p>Para una mejor experiencia de navegación en el portal se recomienda usar los navegadores:</p>
                                    </label>
                                </div>
                                <div class="col-sm-12 col-md-12 col-lg-12 text-center">
                                    <div class="col-sm-12 col-md-12 col-lg-12 text-center">
                                        <div class="col-sm-3 col-md-3 col-lg-3 text-center">
                                            <asp:Image runat="server" ImageUrl="~/Imagenes/chrome.png" />
                                        </div>
                                        <div class="col-sm-3 col-md-3 col-lg-3 text-center">
                                            <asp:Image runat="server" ImageUrl="~/Imagenes/opera.png" />
                                        </div>
                                        <div class="col-sm-3 col-md-3 col-lg-3 text-center">
                                            <asp:Image runat="server" ImageUrl="~/Imagenes/firefox.png" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                </ContentTemplate>
            </asp:UpdatePanel>
       </form>
    </div>
</asp:Content>
