using System.ComponentModel.DataAnnotations;

namespace BackendTorneosS.Entidades
{
    public class Torneos
    {
        [Key]
        public int intTorneo { get; set; }
        public string strNombre { get; set; }
        public string strDescripcion { get; set; }
        public DateTime datFechaInicio { get; set; }
        public DateTime datFechaFin { get; set; }
        public string strUbicacion { get; set; }
        public string bitEstatus { get; set; }
        public int intEquiposActivos { get; set; }
        public int intEquiposMaximos { get; set; }
        public string strTipo { get; set; }
        public double dblPremio { get; set; }
    }
}
