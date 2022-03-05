<%@ Page Title="Preventivos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Portalpreventivos._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        function mensaje() {
            alert("El Archivo tiene un espacio en blanco en uno de los campos favor de revisar el archivo");
        }
        function mensaje2() {
            alert("El Archivo tiene un campo de mas favor de revisar el archivo");
        }
    </script>
    <div class="panel panel-info">
      <div class="panel-footer">
          <form class="form-group" runat="server">
                 <div class="input-group">
                     <asp:FileUpload class="form-control" ID="idFile" runat="server" />
                     <asp:Button  class="btn btn-primary btn-sm" ID="btnReadfile" runat="server" Text="Enviar" OnClick="btnReadfile_Click1" />
                 </div>
                 <asp:HiddenField ID="claveUsuario" runat="server" />
                 <br />
              <asp:TextBox ID="txtsalida" runat="server"  Height="34px" Width="974px" ForeColor="Red"></asp:TextBox>
          </form>
      </div>
    </div>

</asp:Content>
