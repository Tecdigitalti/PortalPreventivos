using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portalpreventivos.Data
{
    public class Estructura
    {
        public string nombre { get; set; }
        public  string descripcion { get; set; }
        public string poliza { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public string causa { get; set; }
    }
}