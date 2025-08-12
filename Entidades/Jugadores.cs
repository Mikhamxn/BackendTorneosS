using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackendTorneosS.Entidades
{
    public class Jugadores
    {
        [Key]
        public int intJugador { get; set; }
        public string strNombre { get; set; }
        public int intEdad { get; set; }
        public string strPosicion { get; set; }
        public int intEquipo { get; set; }
        [ForeignKey(nameof(intEquipo))]
        [JsonIgnore]
        public Equipos? Equipo { get; set; }

        public double dblAltura { get; set; }
        public double dblPeso { get; set; }
        public bool bitCapitan { get; set; }
        public int? intGoles { get; set; }
        public int? intNumero { get; set; }
        public int? intTarjetaAmarilla { get; set; }
        public int? intTarjetaRoja { get; set; }
        public int? intAsistencias { get; set; }
    }
}
