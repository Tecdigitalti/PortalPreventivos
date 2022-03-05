using System;
using System.IO;
using System.Web.UI;
using System.Threading.Tasks;
using System.Text;
using System.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace Portalpreventivos
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            claveUsuario.Value = (string)Session["Usuario"];
        }

        protected void btnReadfile_Click1(object sender, EventArgs e)
        {
            string nombre_Archivo = idFile.FileName;
            string carpetaTemp = ConfigurationManager.AppSettings["Ruta_Temporal"];  //Server.MapPath("/Temporal/");
            string carpetaFinal = ConfigurationManager.AppSettings["Ruta_Proc"]; ;  //Server.MapPath("/Procesados/");
            string ruta = "", salida = "", mensaje = "", filaCorrecta = "", completa = "";
            bool procesar = false;

            //valores
            string cell_Nombre = "";
            string cell_Descripcion = "";
            string cell_Poliza = "";
            string cell_FechaInicio = "";
            string cell_FechaFin = "";
            string cell_Causa = "";
            string cell_Mas = "";

            if (idFile.HasFile && Path.GetExtension(idFile.FileName).Equals(".xlsx"))
            {

                if (!Directory.Exists(carpetaTemp))
                {
                    Directory.CreateDirectory(carpetaTemp);
                }

                ruta = carpetaTemp + Path.GetFileName(idFile.FileName);

                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                    idFile.SaveAs(ruta);
                }
                else
                {
                    idFile.SaveAs(ruta);
                }
                procesar = true;
            }
            else
            {
                txtsalida.Text = "El Documento seleccionado debe de ser un archivo de excel con terminacion '.xlsx' ";
            }

            bool error = false;
  
            try
            {
                IWorkbook workbook = null;
                FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read);
                if (idFile.FileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fs);
                }
                else
                {
                    if (idFile.FileName.IndexOf(".xls") > 0)
                    {
                        workbook = new HSSFWorkbook(fs);
                    }
                        
                }
                //First sheet
                ISheet sheet = workbook.GetSheetAt(0);

                // read the current row data
                XSSFRow headerRow = (XSSFRow)sheet.GetRow(0);
                // LastCellNum is the number of cells of current rows
                int cellCount = headerRow.LastCellNum;
                // LastRowNum is the number of rows of current table
                int rowCount = sheet.LastRowNum + 1;
                // Total de filas del documento
                int _totFilas = sheet.LastRowNum;

                if(cellCount >= 7)
                {
                    error = true;
                    mensaje = "El Documento tiene un VALOR de más favor de corregir y/o eliminar la columna completa";
                }
                else
                {
                    string valor = "";
                    for (int i = (sheet.FirstRowNum + 1); i < rowCount; i++)
                    {
                        int columna = 0;
                        XSSFRow row = (XSSFRow)sheet.GetRow(i);
                        try
                        {
                            for (int j = 0; j < row.LastCellNum; j++) //row.FirstCellNum
                            {
                                columna++;
                                if(columna >= 7 && !string.IsNullOrEmpty(row.GetCell(j).ToString()))
                                {
                                    cell_Mas = row.GetCell(j).ToString();
                                    break;
                                }
                                else
                                {
                                    valor = row.GetCell(j).ToString();
                                }
                                
                                if (!valor.Equals("")) 
                                {
                                  
                                    switch (columna)
                                    {
                                        case 1:
                                            cell_Nombre = valor;
                                            if(cell_Nombre.Length > 249)
                                            {
                                                cell_Nombre = cell_Nombre.Substring(0, 245);
                                            }
                                            break;
                                        case 2:
                                            cell_Descripcion = valor;
                                            if (cell_Descripcion.Length > 249)
                                            {
                                                cell_Descripcion = cell_Descripcion.Substring(0, 245);
                                            }
                                            break;
                                        case 3:
                                            cell_Poliza = valor;
                                            if (cell_Poliza.Length > 249)
                                            {
                                                cell_Poliza = cell_Poliza.Substring(0, 245);
                                            }
                                            break;
                                        case 4:
                                            cell_FechaInicio = valor;
                                            if (cell_FechaInicio.Length > 249)
                                            {
                                                cell_FechaInicio = cell_FechaInicio.Substring(0, 245);
                                            }
                                            break;
                                        case 5:
                                            cell_FechaFin = valor;
                                            if (cell_FechaFin.Length > 249)
                                            {
                                                cell_FechaFin = cell_FechaFin.Substring(0, 245);
                                            }
                                            break;
                                        case 6:
                                            cell_Causa = valor;
                                            if (cell_Causa.Length > 249)
                                            {
                                                cell_Causa = cell_Causa.Substring(0, 245);
                                            }
                                            break;
                                    }

                                }

                            }

                            if (!cell_Mas.Equals(""))
                            {
                                error = true;
                                mensaje = "La Fila " + i.ToString() + " tiene un valor de más ' " + cell_Mas + " ' favor de corregir el archivo";
                                break;
                            }
                            else
                            {
                                filaCorrecta = cell_Nombre + "|" + cell_Descripcion + "|" + cell_Poliza + "|" + cell_FechaInicio + "|" + cell_FechaFin + "|" + cell_Causa + "|";
                            }

                        }catch (Exception Ex){
                            if (columna == 1)
                            {
                                salida = salida + "NOMBRE";
                            }
                            else
                            {
                                if (columna == 2)
                                {
                                    salida = salida + " DESCRIPCION,";
                                }
                                else
                                {
                                    if (columna == 3)
                                    {
                                        salida = salida + " POLIZA,";
                                    }
                                    else
                                    {
                                        if (columna == 4)
                                        {
                                            salida = salida + " FECHA1,";
                                        }
                                        else
                                        {
                                            if (columna == 5)
                                            {
                                                salida = salida + " FECHA2,";
                                            }
                                            else
                                            {
                                                if (columna == 6)
                                                {
                                                    salida = salida + " CAUSA";
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            error = true;
                            mensaje = "La Fila " + (i+1).ToString() + " tiene valores vacios en la columnas [" + salida + "] favor de corregir el archivo";
                            break;
                        }

                        ////Cuando llegue a la ultima fila se agrega el usuario
                        //if (i == 1)
                        //{
                            completa = completa + filaCorrecta + claveUsuario.Value + "|" + "] ";
                        //}
                        //else
                        //{
                        //    completa = completa + filaCorrecta + "] ";
                        //}

                        filaCorrecta = "";
                    }

                }


                if (error)
                {
                    txtsalida.Text = mensaje;
                }
                else
                {
                    char[] separador = { ']' };
                    string[] linea = completa.Split(separador, StringSplitOptions.RemoveEmptyEntries);
                    string ruta2 = carpetaFinal + Path.GetFileNameWithoutExtension(idFile.FileName) + ".csv";

                    FileStream stream = null;
                    try
                    {
                        // Crea un nuevo archivo 
                        stream = new FileStream(ruta2, FileMode.OpenOrCreate);
                        // Escribir sobre el archivo generado
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            foreach (string resp in linea)
                            {
                                writer.WriteLine(resp.Trim(), "\n");
                            }
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Dispose();
                    }
                    File.Delete(ruta);
                    txtsalida.Text = "El Documento fue procesado correctamente!!!";
                }

            }
            catch (Exception ex)
            {
                mensaje = "Debe seleccionar un documento en excel para procesar con terminacion'.xlsx' ";
                txtsalida.Text = mensaje;
            }

            
        }
    }
}