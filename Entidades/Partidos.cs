// Entidades/Partidos.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackendTorneosS.Entidades
{
    public class Partidos
    {
        [Key]
        public int intPartido { get; set; }

        [ForeignKey("EquipoLocal")]
        public int intEquipoLocal { get; set; }

        [ForeignKey("EquipoVisitante")]
        public int intEquipoVisitante { get; set; }

        public int? intGolesLocal { get; set; }
        public int? intGolesVisitante { get; set; }
        public DateTime datFecha { get; set; }
        public TimeSpan timHora { get; set; }
        public string strLugar { get; set; }
        public string strEstado { get; set; } // "scheduled", "in_progress", "finished"
        public int intTorneo { get; set; }

        // Estadísticas de equipo
        public int intTarjetasAmarillasLocal { get; set; }
        public int intTarjetasRojasLocal { get; set; }
        public int intTarjetasAmarillasVisitante { get; set; }
        public int intTarjetasRojasVisitante { get; set; }

        // Relaciones
        [JsonIgnore]
        public virtual Equipos? EquipoLocal { get; set; }

        [JsonIgnore]
        public virtual Equipos? EquipoVisitante { get; set; }

        [JsonIgnore]
        public virtual Torneos? Torneo { get; set; }

        [JsonIgnore]
        public virtual List<EventoPartido>? Eventos { get; set; }
    }
}