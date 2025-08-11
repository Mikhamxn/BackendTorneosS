// Entidades/EventoPartido.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackendTorneosS.Entidades
{
    public class EventoPartido
    {
        [Key]
        public int intEvento { get; set; }

        [ForeignKey("Partido")]
        public int intPartido { get; set; }

        [ForeignKey("Jugador")]
        public int intJugador { get; set; }

        public string strTipoEvento { get; set; } // "Gol", "Asistencia", "TarjetaAmarilla", "TarjetaRoja"
        public int intMinuto { get; set; }

        // Relaciones
        [JsonIgnore]
        public virtual Partidos? Partido { get; set; }

        [JsonIgnore]
        public virtual Jugadores? Jugador { get; set; }
    }
}