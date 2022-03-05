using System;
using System.Collections.Generic;
using System.Text;
using Hyland.Unity;
using Hyland.Unity.UnityForm;
using System.IO;
using System.Data.Odbc;
using System.Globalization;

namespace Portalpreventivos.Data
{
    public class UnityOB
    {
        private Hyland.Unity.Application g_app;
        string i_SessionID;
        public string stMensajeError = string.Empty;
        Bitacora Bitacora = new Bitacora();

        public bool ConnectionOB()
        {
            bool bReturn = false;
            string App_Server = System.Configuration.ConfigurationManager.AppSettings["App_Server"];
            string Usuario = System.Configuration.ConfigurationManager.AppSettings["Usuario"];
            string Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
            string DataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"];

            try
            {
                AuthenticationProperties props = Hyland.Unity.Application.CreateOnBaseAuthenticationProperties(App_Server, Usuario, Password, DataSource);
                props.LicenseType = LicenseType.Default;

                g_app = Hyland.Unity.Application.Connect(props);

                i_SessionID = g_app.CurrentUser.ID.ToString();
                g_app.Diagnostics.Write("Conectado InterfazOnBase Id Sesion: " + g_app.SessionID.ToString());
                bReturn = true;
            }
            catch (UnityAPIException unityEx)
            {
                Bitacora.agregaError(unityEx.ToString());
                stMensajeError = unityEx.ToString();
            }
            catch (Exception ex)
            {
                Bitacora.agregaError(ex.ToString());
                stMensajeError = ex.ToString();
            }
            return bReturn;
        }

        public bool IsConnected()
        {
            return g_app == null ? false : g_app.IsConnected;
        }

        public string ConnectionOB(string sessionID)
        {
            string loggedUser = "";
            string App_Server = System.Configuration.ConfigurationManager.AppSettings["App_Server"];

            try
            {
                AuthenticationProperties props = Hyland.Unity.Application.CreateSessionIDAuthenticationProperties(App_Server, sessionID, true);

                g_app = Hyland.Unity.Application.Connect(props);

                loggedUser = g_app.CurrentUser.Name;
                i_SessionID = g_app.CurrentUser.ID.ToString();

                g_app.Diagnostics.Level = Hyland.Unity.Diagnostics.DiagnosticsLevel.Info;
                g_app.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, "Conectado ok usando la sesión: " + sessionID);
            }
            catch (UnityAPIException unityEx)
            {
                Bitacora.agregaError(unityEx.ToString());
            }
            catch (Exception ex)
            {
                Bitacora.agregaError(ex.ToString());
            }
            return loggedUser;
        }

        public void ConnectionOB_AD(string usuario, string password, ref bool login, ref string msg)
        {
            string id_ob = "";
            string st_msj = "OK";
            i_SessionID = string.Empty;

            string App_Server = System.Configuration.ConfigurationManager.AppSettings["App_Server"];
            string DataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"];
            string Domain = System.Configuration.ConfigurationManager.AppSettings["Domain"];

            try
            {
                DomainAuthenticationProperties props = Hyland.Unity.Application.CreateDomainAuthenticationProperties(App_Server, DataSource);
                props.LicenseType = LicenseType.Default;
                props.Domain = Domain;
                props.Username = usuario; //logIn.user.Trim();
                props.Password = password;//logIn.password.Trim();
                g_app = Hyland.Unity.Application.Connect(props);

                i_SessionID = g_app.CurrentUser.ID.ToString();
                login = true;
                msg = i_SessionID;

                //return (true, i_SessionID);
            }
            catch (UnityAPIException unityEx)
            {
                if (unityEx.ServerErrorCode != null)
                {
                    if (unityEx.ServerErrorCode.ToString().Trim().ToUpper() == "MAX_LICENSES")
                        st_msj = "<strong> Se alcanzo el máximo de licencias concurrentes. </strong><br><br> Por favor regrese y reintente.";
                    else
                        st_msj = "<strong> Error al conectarse a onbase: </strong> " + unityEx.ToString();
                }
                else
                    st_msj = "<strong> Error al conectarse a onbase: </strong> " + unityEx.ToString();

                login = false;
                msg = st_msj;
            }
            catch (Exception ex)
            {
                st_msj = "<strong> Error al conectarse a onbase: </strong> " + ex.ToString();
                login = false;
                msg = st_msj;
            }

            //return (false, st_msj);
        }

        public void DisconnectionOB()
        {
            try
            {
                if ((g_app != null) && (g_app.IsConnected))
                {
                    g_app.Disconnect();
                    if (!string.IsNullOrEmpty(i_SessionID))
                        DeleteLoggedUserInative();
                }
            }
            catch (UnityAPIException unityEx)
            {
                Bitacora.agregaError("Error al desconectar a onbase" + unityEx);
            }
            catch (Exception ex)
            {
                Bitacora.agregaError("Error al desconectar a onbase" + ex);
            }
        }

        private void DeleteLoggedUserInative()
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append("DELETE FROM HSI.LOGGEDUSER WHERE USERNUM = " + i_SessionID + " AND CHECKIN = 0");

            try
            {
                DataBase DB = new DataBase();
                OdbcDataReader dr = null;

                DB.NewOra(DataBase.DBType.OnBase);

                dr = DB.GetDataReader(sb.ToString());

                dr.Read();

                dr.Close();
                DB.Close();
            }
            catch (Exception ex)
            {
                Bitacora.agregaError(ex.ToString());
            }
        }

        public long guardaNuevaUnityForm(string nombreTemplate, Dictionary<string, string> keywords, List<Dictionary<string, string>> keywordRecords, string keywordRecordName)
        {
            long newDocID = -1;
            stMensajeError = "";

            if (g_app != null && g_app.IsConnected)
            {
                try
                {
                    //Find the Unity Form Template
                    FormTemplate template = g_app.Core.UnityFormTemplates.Find(nombreTemplate);
                    if (template == null)
                        throw new Exception("FormTemplate is null");
                    //Create StoreNewUnityFormProperties object
                    StoreNewUnityFormProperties newUnityProps = g_app.Core.Storage.CreateStoreNewUnityFormProperties(template);

                    foreach (KeyValuePair<string, string> kvp in keywords)
                    {
                        Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                        newUnityProps.AddKeyword(keyword);
                    }

                    foreach (Dictionary<string, string> keywordRecord in keywordRecords)
                    {
                        EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName);
                        newUnityProps.AddKeywordRecord(editableKeywordRecord);
                    }

                    //Store the new document
                    Document doc = g_app.Core.Storage.StoreNewUnityForm(newUnityProps);
                    //Set the new document's to newDocID
                    newDocID = doc.ID;
                }
                catch (SessionNotFoundException ex)
                {
                    g_app.Diagnostics.Write("The Unity API session could not be found, please reconnect. " + ex);
                    stMensajeError = "The Unity API session could not be found, please reconnect. " + ex.Message;
                }
                catch (UnityAPIException ex)
                {
                    g_app.Diagnostics.Write("There was a Unity API exception. " + ex);
                    stMensajeError = "There was a Unity API exception. " + ex.Message;
                }
                catch (Exception ex)
                {
                    g_app.Diagnostics.Write("There was an unknown exception. " + ex);
                    stMensajeError = "There was an unknown exception. " + ex.Message;
                }
            }
            else
            {
                g_app.Diagnostics.Write("Aplicación no conectada.");
                stMensajeError = "Aplicación no conectada.";
            }

            return newDocID;

        }

        public long guardaNuevaUnityForm(string nombreTemplate, Dictionary<string, string> keywords, List<Dictionary<string, string>> keywordRecords, string keywordRecordName, List<Dictionary<string, string>> keywordRecords2, string keywordRecordName2, List<Dictionary<string, string>> keywordRecords3, string keywordRecordName3)
        {
            long newDocID = -1;
            stMensajeError = "";

            if (g_app != null && g_app.IsConnected)
            {
                try
                {
                    //Find the Unity Form Template
                    FormTemplate template = g_app.Core.UnityFormTemplates.Find(nombreTemplate);
                    if (template == null)
                        throw new Exception("FormTemplate is null");
                    //Create StoreNewUnityFormProperties object
                    StoreNewUnityFormProperties newUnityProps = g_app.Core.Storage.CreateStoreNewUnityFormProperties(template);

                    foreach (KeyValuePair<string, string> kvp in keywords)
                    {
                        Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                        newUnityProps.AddKeyword(keyword);
                    }

                    foreach (Dictionary<string, string> keywordRecord in keywordRecords)
                    {
                        EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName);
                        newUnityProps.AddKeywordRecord(editableKeywordRecord);
                    }

                    if (keywordRecords2.Count > 0)
                    {
                        foreach (Dictionary<string, string> keywordRecord in keywordRecords2)
                        {
                            EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName2);
                            newUnityProps.AddKeywordRecord(editableKeywordRecord);
                        }
                    }

                    if (keywordRecords3.Count > 0)
                    {
                        foreach (Dictionary<string, string> keywordRecord in keywordRecords3)
                        {
                            EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName3);
                            newUnityProps.AddKeywordRecord(editableKeywordRecord);
                        }
                    }

                    //Store the new document
                    Document doc = g_app.Core.Storage.StoreNewUnityForm(newUnityProps);
                    //Set the new document's to newDocID
                    newDocID = doc.ID;
                }
                catch (SessionNotFoundException ex)
                {
                    g_app.Diagnostics.Write("The Unity API session could not be found, please reconnect. " + ex);
                    stMensajeError = "The Unity API session could not be found, please reconnect. " + ex.Message;
                }
                catch (UnityAPIException ex)
                {
                    g_app.Diagnostics.Write("There was a Unity API exception. " + ex);
                    stMensajeError = "There was a Unity API exception. " + ex.Message;
                }
                catch (Exception ex)
                {
                    g_app.Diagnostics.Write("There was an unknown exception. " + ex);
                    stMensajeError = "There was an unknown exception. " + ex.Message;
                }
            }
            else
            {
                g_app.Diagnostics.Write("Aplicación no conectada.");
                stMensajeError = "Aplicación no conectada.";
            }

            return newDocID;

        }

        internal Document getDocumentByID(long onbaseID)
        {
            //Document document = g_app.Core.GetDocumentByID(Convert.ToInt64(onbaseID));
            Document document = g_app.Core.GetDocumentByID(onbaseID);

            if (document == null)
                throw new Exception("El documento es nulo.");

            return document;
        }

        public bool FnBl_ObtenValorTipoKW(Document Documento, List<string> KeywordList, ref List<string> ReturnValues)
        {
            string St_MsjeError = "";

            try
            {
                //Creamos un objeto KeywordType con el tipo de keyword que queremos leer
                var KWList = g_app.Core.KeywordTypes.FindAll(X => KeywordList.Contains(X.Name));

                foreach (var KWT in KWList)
                {
                    int indexOfKW = KeywordList.IndexOf(KWT.Name);

                    var KWR = Documento.KeywordRecords.Find(KWT);
                    if (KWR != null)
                    {
                        var KW = KWR.Keywords.Find(KWT);

                        if (KW != null && !KW.IsBlank)
                        {
                            ReturnValues[indexOfKW] = KW.Value.ToString();
                        }
                    }
                }
            }
            //Si hay algún error lo guardamos en St_MsjeError y lo escribimos en el diagnostics
            catch (Exception ex)
            {
                St_MsjeError = "Error al obtener KW Multiples , " + ex;
                g_app.Diagnostics.Write(St_MsjeError);
                return false;
            }

            return true;
        }

        public bool FnBl_ObtenValorTipoKW(Document Documento, string St_TipoKW, ref string St_ValorKW, bool bl_obligatorio)
        {
            string St_MsjeError = "";
            St_ValorKW = "";

            try
            {
                //Creamos un objeto KeywordType con el tipo de keyword que queremos leer
                KeywordType KT_TipoKW = g_app.Core.KeywordTypes.Find(St_TipoKW);
                if (KT_TipoKW == null)
                    throw new Exception(string.Format("El Tipo de Keyword {0} no existe o no esta configurado en OnBase", St_TipoKW));

                //Barremos los KeywordRecord con el tipo de keyword que queremos leer
                foreach (KeywordRecord keyRecord in Documento.KeywordRecords.FindAll(KT_TipoKW))
                {
                    //Barremos los keywrods que queremos leer
                    foreach (Keyword keyword in keyRecord.Keywords.FindAll(KT_TipoKW))
                    {
                        //Asignamos el valor del keyword
                        if (keyword.KeywordType.DataType == KeywordDataType.Date || keyword.KeywordType.DataType == KeywordDataType.DateTime)
                            St_ValorKW = keyword.DateTimeValue.ToString("yyyy-MM-dd");
                        else
                            St_ValorKW = keyword.Value.ToString();
                    }
                }
            }
            //Si hay algún error lo guardamos en St_MsjeError y lo escribimos en el diagnostics
            catch (Exception ex)
            {
                St_MsjeError = "Error al obtener el KW:" + St_TipoKW + " , " + ex;
                g_app.Diagnostics.Write(St_MsjeError);
            }

            if (St_ValorKW == "" && bl_obligatorio == true)
            {
                g_app.Diagnostics.Write("ERROR, No se encontro valor para keyword: " + St_TipoKW);
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Método de ayuda para crear un nuevo Keyword
        /// </summary>
        /// <param name="keywordTypeName">Nombre del Keyword</param>
        /// <param name="Value">Valor del nuevo Keyword</param>
        /// <returns>Objeto tipo Keyword</returns>
        public Keyword CreateKeywordHelper(string keywordTypeName, string Value)
        {
            KeywordType keyType = g_app.Core.KeywordTypes.Find(keywordTypeName);
            if (keyType == null)
            {
                throw new Exception("Keyword type " + keywordTypeName + " could not be found.");
            }
            return CreateKeywordHelper(keyType, Value);
        }

        private Keyword CreateKeywordHelper(KeywordType Keytype, string Value)
        {
            Keyword key = null;
            switch (Keytype.DataType)
            {
                case KeywordDataType.Currency:
                case KeywordDataType.Numeric20:
                    decimal decVal = 0;
                    decimal.TryParse(Value, out decVal);
                    key = Keytype.CreateKeyword(decVal);
                    break;
                case KeywordDataType.Date:
                case KeywordDataType.DateTime:
                    DateTime dateVal = DateTime.ParseExact(Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    key = Keytype.CreateKeyword(dateVal);
                    break;
                case KeywordDataType.FloatingPoint:
                    double dblVal = 0;
                    double.TryParse(Value, out dblVal);
                    key = Keytype.CreateKeyword(dblVal);
                    break;
                case KeywordDataType.Numeric9:
                    long lngVal = 0;
                    long.TryParse(Value, out lngVal);
                    key = Keytype.CreateKeyword(lngVal);
                    break;
                default:
                    //Por testear
                    //Value = Value.Length > Keytype.DataLength ? Value.Substring(0, (int)Keytype.DataLength) : Value;
                    key = Keytype.CreateKeyword(Value);
                    break;
            }
            return key;
        }

        /// <summary>
        /// Metodo de ayuda para guardar un nuevo documento en OnBase
        /// </summary>
        /// <param name="tipoDocumento">Nombre del Tipo de Documento</param>
        /// <param name="keywords">Arreglo de Keywords Nombre,Valor</param>
        /// <param name="stream">Stream de memoria que representa al documento</param>
        /// <param name="extension">Extension del Documento</param>
        /// <returns>ID del nuevo documento</returns>
        public long guardaNuevoDoc(string tipoDocumento, Dictionary<string, string> keywords, string filePath)
        {
            long newDocID = -1;
            stMensajeError = "";

            try
            {
                //Find the DocumentType for "Loan Application"
                DocumentType docType = g_app.Core.DocumentTypes.Find(tipoDocumento);
                if (docType == null)
                {
                    throw new Exception("El tipo de documento es nulo.");
                }
                //Find FileType for "Image File Format"
                FileType fileType = g_app.Core.FileTypes.Find(FileType(Path.GetExtension(filePath)));
                if (fileType == null)
                    throw new Exception("The FileType is null.");
                //Create StoreNewDocumentProperties
                StoreNewDocumentProperties newDocProperties = g_app.Core.Storage.CreateStoreNewDocumentProperties(docType, fileType);

                foreach (KeyValuePair<string, string> kvp in keywords)
                {
                    Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                    newDocProperties.AddKeyword(keyword);
                }

                //Create a PageData based on the FilePath (make sure to dispose it!)
                using (PageData pageData = g_app.Core.Storage.CreatePageData(filePath))
                {
                    //Store the new Document
                    //Set the Document ID to newDocID
                    Document document = g_app.Core.Storage.StoreNewDocument(pageData, newDocProperties);
                    newDocID = document.ID;
                }
            }
            catch (UnityAPIException ex)
            {
                g_app.Diagnostics.Write(ex);
                stMensajeError = ex.Message;
            }
            catch (Exception ex)
            {
                g_app.Diagnostics.Write(ex);
                stMensajeError = ex.Message;
            }

            return newDocID;
        }

        public QueryResult obtieneListaDeDocumentos(List<string> nombreTiposDeDocumento, Dictionary<string, string> keywords, ref long ID_NumeroDoc)
        {
            string stMaxDoc = System.Configuration.ConfigurationManager.AppSettings["maxDoc"];

            DocumentQuery docQuery = g_app.Core.CreateDocumentQuery();

            foreach (KeyValuePair<string, string> kvp in keywords)
            {
                Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                docQuery.AddKeyword(keyword, KeywordOperator.Equal, KeywordRelation.And);
            }


            foreach (string stTipoDoc in nombreTiposDeDocumento)
            {
                DocumentType docType = g_app.Core.DocumentTypes.Find(stTipoDoc);
                ID_NumeroDoc = docType.ID;

                if (docType == null)
                    throw new Exception("Tipo de documento " + stTipoDoc + " no existe en OnBase.");
                docQuery.AddDocumentType(docType);
            }

            docQuery.AddDisplayColumn(DisplayColumnType.DocumentID);
            docQuery.AddDisplayColumn(DisplayColumnType.DocumentName);

            //Se agrega retrievaloptions, se comenta sorts
            //docQuery.AddSort(DocumentQuery.SortAttribute.DocumentName, true);
            //docQuery.AddSort(DocumentQuery.SortAttribute.ArchivalDate, false);
            docQuery.RetrievalOptions = DocumentRetrievalOptions.LoadKeywords;
            QueryResult results = docQuery.ExecuteQueryResults(Convert.ToInt32(stMaxDoc));

            return results;
        }

        public QueryResult obtieneListaDeDocumentos(List<string> nombreTiposDeDocumento, Dictionary<string, string> keywords, List<string> keywordsDisplay, ref long ID_NumeroDoc)
        {
            string stMaxDoc = System.Configuration.ConfigurationManager.AppSettings["maxDoc"];

            DocumentQuery docQuery = g_app.Core.CreateDocumentQuery();

            foreach (KeyValuePair<string, string> kvp in keywords)
            {
                Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                docQuery.AddKeyword(keyword, KeywordOperator.Equal, KeywordRelation.And);
            }

            foreach (string stTipoDoc in nombreTiposDeDocumento)
            {
                DocumentType docType = g_app.Core.DocumentTypes.Find(stTipoDoc);
                ID_NumeroDoc = docType.ID;

                if (docType == null)
                    throw new Exception("Tipo de documento " + stTipoDoc + " no existe en OnBase.");
                docQuery.AddDocumentType(docType);
            }

            foreach (string displayCol in keywordsDisplay)
            {
                KeywordType keyType = g_app.Core.KeywordTypes.Find(displayCol);
                if (keyType == null)
                    throw new Exception("KeywordType " + displayCol + " es nulo");
                docQuery.AddDisplayColumn(keyType);
            }

            docQuery.AddSort(DocumentQuery.SortAttribute.DocumentTypeName, false);
            docQuery.AddSort(DocumentQuery.SortAttribute.ArchivalDate, true);

            QueryResult results = docQuery.ExecuteQueryResults(Convert.ToInt32(stMaxDoc));

            return results;
        }

        //internal DocumentList obtieneListaDeDocumentos(List<string> nombreTiposDeDocumento, Dictionary<string, string> keywords, string cola, DateTime dtFechaInicial, DateTime dtFechaFinal, ref long ID_NumeroDoc)
        //{
        //    DocumentList docList = null;
        //    string stMaxDoc = System.Configuration.ConfigurationManager.AppSettings["maxDoc"];

        //    if (keywords.Count <= 0 && string.IsNullOrEmpty(cola))
        //        throw new Exception("Debe proporcionar Estatus, Numero Acreedor o Cola");

        //    DocumentQuery docQuery = g_app.Core.CreateDocumentQuery();

        //    foreach (KeyValuePair<string, string> kvp in keywords)
        //    {
        //        Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
        //        docQuery.AddKeyword(keyword, KeywordOperator.Equal, KeywordRelation.And);
        //    }

        //    foreach (string stTipoDoc in nombreTiposDeDocumento)
        //    {
        //        DocumentType docType = g_app.Core.DocumentTypes.Find(stTipoDoc);
        //        ID_NumeroDoc = docType.ID;

        //        if (docType == null)
        //            throw new Exception("Tipo de documento " + stTipoDoc + " no existe en OnBase.");
        //        docQuery.AddDocumentType(docType);
        //    }

        //    docQuery.AddDateRange(dtFechaInicial, dtFechaFinal);
        //    docQuery.AddSort(DocumentQuery.SortAttribute.ArchivalDate, true);

        //    if (!string.IsNullOrEmpty(cola))
        //    {
        //        Queue queue = g_app.Workflow.Queues.Find(cola);
        //        if (queue == null)
        //            throw new Exception("Queue is null.");

        //        QueueQueryOptions queueQueryOpt = queue.CreateQueueQueryOptions();
        //        queueQueryOpt.ListType = QueueQueryListType.AllWorkItems;
        //        queueQueryOpt.Filter = docQuery;

        //        // Metodo que realiza la busqueda, regresa una lista de objetos tipo Document
        //        docList = queue.GetDocumentList(queueQueryOpt, Convert.ToInt32(stMaxDoc), DocumentRetrievalOptions.LoadKeywords);
        //    }
        //    else
        //    {
        //        docList = docQuery.Execute(Convert.ToInt32(stMaxDoc));
        //    }

        //    return docList;
        //}

        internal List<Document> obtieneListaDeDocumentos(Int64 NoRegistros, string nombreTipoDeDocumento, Dictionary<string, string> keywords, DateTime? de = null, DateTime? hasta = null)
        {

            Core core = g_app.Core;
            DocumentQuery docQuery = core.CreateDocumentQuery();
            List<Document> Lista = new List<Document>();

            if (de != null)
            {
                if (hasta != null)
                {
                    docQuery.AddDateRange(Convert.ToDateTime(de), Convert.ToDateTime(hasta));
                }
                else
                {
                    docQuery.AddDateRange(Convert.ToDateTime(de), Convert.ToDateTime(de));
                }
            }

            foreach (KeyValuePair<string, string> kvp in keywords)
            {
                Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                docQuery.AddKeyword(keyword, KeywordOperator.Equal, KeywordRelation.And);
            }

            DocumentList docList = docQuery.Execute(NoRegistros);
            foreach (Document documento in docList)
            {
                //Sort all the documents and add them in a list or array    
                //if (documento.Name.Equals(nombreTipoDeDocumento))
                //{
                Lista.Add(documento);
                //}

            }

            return Lista;
        }

        private EditableKeywordRecord CreateKeywordRecord(Dictionary<string, string> keywordRecord, string keywordRecordName)
        {
            EditableKeywordRecord editableKeyRecord = null;
            //Find keyword record type
            KeywordRecordType keyRecordType = g_app.Core.KeywordRecordTypes.Find(keywordRecordName);
            if (keyRecordType == null)
                throw new Exception("KeywordRecordType is null.");
            //Create editable keyword record
            editableKeyRecord = keyRecordType.CreateEditableKeywordRecord();
            //foreach key value pair in SignerInfo, add the keyword to the editable keyword record
            foreach (KeyValuePair<string, string> kvp in keywordRecord)
            {
                string keywordTypeName = kvp.Key;
                string keywordValue = kvp.Value;

                editableKeyRecord.AddKeyword(CreateKeywordHelper(keywordTypeName, keywordValue));
            }

            return editableKeyRecord;
        }

        private string FileType(string st_extension)
        {
            //string st_extension = System.IO.Path.GetExtension(st_File).Trim().ToUpper();
            string st_FileTypeStr = "";

            switch (st_extension.ToUpper().Trim())
            {
                case ".TXT":
                    st_FileTypeStr = "Text Report Format";
                    break;
                case ".CTX":
                    st_FileTypeStr = "Text Report Format";
                    break;
                case ".CSX":
                    st_FileTypeStr = "Text Report Format";
                    break;
                case ".JPG":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".GIF":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".TIFF":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".TIF":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".PNG":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".BMP":
                    st_FileTypeStr = "Image File Format";
                    break;
                case ".DOC":
                    st_FileTypeStr = "MS Word Document";
                    break;
                case ".DOCX":
                    st_FileTypeStr = "MS Word Document";
                    break;
                case ".XLS":
                    st_FileTypeStr = "MS Excel Spreadsheet";
                    break;
                case ".XLSX":
                    st_FileTypeStr = "MS Excel Spreadsheet";
                    break;
                case ".PPT":
                    st_FileTypeStr = "MS Power Point";
                    break;
                case ".PPTX":
                    st_FileTypeStr = "MS Power Point";
                    break;
                case ".RTF":
                    st_FileTypeStr = "Rich Text Format";
                    break;
                case ".PDF":
                    st_FileTypeStr = "PDF";
                    break;
                case ".HTM":
                    st_FileTypeStr = "HTML";
                    break;
                case ".HTML":
                    st_FileTypeStr = "HTML";
                    break;
                case ".AVI":
                    st_FileTypeStr = "AVI Movie";
                    break;
                case ".MOV":
                    st_FileTypeStr = "Quick Time Movie";
                    break;
                case ".WAV":
                    st_FileTypeStr = "WAV Audio File";
                    break;
                case ".PLC":
                    st_FileTypeStr = "PCL Filter";
                    break;
                case ".XML":
                    st_FileTypeStr = "XML";
                    break;
                case ".MSG":
                    st_FileTypeStr = "MS Outlook Message";
                    break;
                case ".DXL":
                    st_FileTypeStr = "Lotus Notes Document";
                    break;
                case ".LIC":
                    st_FileTypeStr = "Internal XML";
                    break;
                case ".XDP":
                    st_FileTypeStr = "Adobe XDP";
                    break;
                case ".XPS":
                    st_FileTypeStr = "XPS";
                    break;
                case ".RAR":
                    st_FileTypeStr = "RAR Archive";
                    break;
                case ".VSD":
                    st_FileTypeStr = "MS Visio";
                    break;
                case ".VSDX":
                    st_FileTypeStr = "MS Visio";
                    break;
                default:
                    st_FileTypeStr = "Image File Format";
                    break;
            }
            return st_FileTypeStr;
        }

        internal bool eliminaEntradaMIKG(Document document, string itemnum, string keywordRecordName, ref string stMensaje)
        {
            bool bReturn = false;
            KeywordModifier keyModifier = document.CreateKeywordModifier();
            KeywordRecord keyRecordToDelete = null;

            foreach (KeywordRecord keyRecord in document.KeywordRecords)
            {
                if (keyRecord.KeywordRecordType.Name == keywordRecordName)
                {
                    foreach (Keyword keyword in keyRecord.Keywords)
                    {
                        if (keyword.KeywordType.Name == "GdKComp-ID del Documento")
                        {
                            if (!keyword.IsBlank)
                            {
                                if (keyword.Value.ToString() == itemnum)
                                {
                                    keyRecordToDelete = keyRecord;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (keyRecordToDelete != null)
            {
                keyModifier.RemoveKeywordRecord(keyRecordToDelete);
                keyModifier.ApplyChanges();
                bReturn = true;
            }
            else
            {
                stMensaje = "No se encontró registo en el grupo de kw's de la solicitud principal para el comprobante que se quiere borrar";
            }

            return bReturn;
        }

        public void borraDocumento(Document document)
        {
            g_app.Core.Storage.DeleteDocument(document);
        }

        internal void borraKWs(Document document)
        {
            KeywordModifier keyModifier = document.CreateKeywordModifier();

            foreach (KeywordRecord keyRecord in document.KeywordRecords)
            {
                if (keyRecord.KeywordRecordType.RecordType == RecordType.StandAlone)
                {
                    foreach (Keyword keyword in keyRecord.Keywords)
                    {
                        keyModifier.RemoveKeyword(keyword);
                    }
                }
                else
                {
                    keyModifier.RemoveKeywordRecord(keyRecord);
                }
            }

            keyModifier.ApplyChanges();
        }

        internal void actualizaKWs(Document document, Dictionary<string, string> keywords, List<Dictionary<string, string>> keywordRecords, string keywordRecordName, List<Dictionary<string, string>> keywordRecords2, string keywordRecordName2, List<Dictionary<string, string>> keywordRecords3, string keywordRecordName3)
        {
            KeywordModifier keyModifier = document.CreateKeywordModifier();

            foreach (KeyValuePair<string, string> kvp in keywords)
            {
                Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                keyModifier.AddKeyword(keyword);
            }

            foreach (Dictionary<string, string> keywordRecord in keywordRecords)
            {
                EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName);
                keyModifier.AddKeywordRecord(editableKeywordRecord);
            }

            foreach (Dictionary<string, string> keywordRecord in keywordRecords2)
            {
                EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName2);
                keyModifier.AddKeywordRecord(editableKeywordRecord);
            }

            foreach (Dictionary<string, string> keywordRecord in keywordRecords3)
            {
                EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName3);
                keyModifier.AddKeywordRecord(editableKeywordRecord);
            }

            keyModifier.ApplyChanges();
        }

        internal void actualizaKWs(Document document, Dictionary<string, string> keywords, List<Dictionary<string, string>> keywordRecords, string keywordRecordName)
        {
            KeywordModifier keyModifier = document.CreateKeywordModifier();

            foreach (KeyValuePair<string, string> kvp in keywords)
            {
                Keyword keyword = CreateKeywordHelper(kvp.Key, kvp.Value);
                keyModifier.AddKeyword(keyword);
            }

            foreach (Dictionary<string, string> keywordRecord in keywordRecords)
            {
                EditableKeywordRecord editableKeywordRecord = CreateKeywordRecord(keywordRecord, keywordRecordName);
                keyModifier.AddKeywordRecord(editableKeywordRecord);
            }

            keyModifier.ApplyChanges();
        }

        internal void Diag(string p)
        {
            if (g_app.IsConnected)
            {
                g_app.Diagnostics.Write(p);
            }
        }

        internal bool extraeDocumento(Document document, string providerName, ref string stPath, string st_NoPoliza, string strClave)
        {
            bool bReturn = true;
            stMensajeError = "";

            try
            {
                DefaultDataProvider defaultProvider = g_app.Core.Retrieval.Default;
                switch (providerName)
                {
                    case "Default":
                        using (PageData pageData = defaultProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                    case "Native":
                        NativeDataProvider nativeProvider = g_app.Core.Retrieval.Native;
                        using (PageData pageData = nativeProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                    case "Image":
                        ImageDataProvider imageProvider = g_app.Core.Retrieval.Image;
                        using (PageData pageData = imageProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                    case "PDF":
                        PDFDataProvider pdfProvider = g_app.Core.Retrieval.PDF;
                        using (PageData pageData = pdfProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                    case "Text":
                        TextDataProvider textProvider = g_app.Core.Retrieval.Text;
                        using (PageData pageData = textProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                    default:
                        using (PageData pageData = defaultProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
                        {
                            bReturn = Save(pageData, document, ref stPath, strClave, st_NoPoliza);
                        }
                        break;
                }
            }
            catch (SessionNotFoundException ex)
            {
                stMensajeError = "The Unity API session could not be found, please reconnect." + ex.Message;
                bReturn = false;
            }
            catch (UnityAPIException ex)
            {
                g_app.Diagnostics.Write(ex);
                stMensajeError = "There was a Unity API exception." + ex.Message;
                bReturn = false;
            }
            catch (Exception ex)
            {
                g_app.Diagnostics.Write(ex);
                stMensajeError = "There was an unknown exception." + ex;
                bReturn = false;
            }

            return bReturn;
        }

        internal bool Save(PageData pageData, Document document, ref string path, string strClave, string st_NoPoliza = "")
        {
            bool bReturn = true;
            string parentFolder = System.Configuration.ConfigurationManager.AppSettings["rutaArchivosTemp"] + strClave;

            try
            {
                if (!(Directory.Exists(parentFolder)))
                {
                    Directory.CreateDirectory(parentFolder);
                    Bitacora _bit = new Bitacora();
                    _bit.agregaBitacora("Se creo la carpeta " + parentFolder);
                }

                if (Directory.Exists(parentFolder))
                {
                    path = parentFolder + "/" + document.DocumentType.Name.ToString().Replace("/", "") + "_" + st_NoPoliza + "." + pageData.Extension;

                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);

                        Hyland.Unity.Utility.WriteStreamToFile(pageData.Stream, path);
                    }
                    catch (Exception ex)
                    {
                        bReturn = false;
                        stMensajeError = ex.Message;
                    }
                }

            }
            catch (Exception ex) { }



            return bReturn;
        }

        internal void actualizaKW(Document document, string nombreKW, string valorKW)
        {
            try
            {
                KeywordModifier keyModifier = document.CreateKeywordModifier();
                keyModifier.AddKeyword(CreateKeywordHelper(nombreKW, valorKW));
                keyModifier.ApplyChanges();
            }
            catch (Exception ex)
            {
                g_app.Diagnostics.Write("Error al actualizar el documento con ID: " + document.ID.ToString());
                g_app.Diagnostics.Write(ex.ToString());
            }
        }

    }
}