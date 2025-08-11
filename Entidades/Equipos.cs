using System.ComponentModel.DataAnnotations;

namespace BackendTorneosS.Entidades
{
    public class Equipos
    {
        [Key]
        public int intEquipo { get; set; }
        public string strNombre { get; set; }
        public DateTime datFundacion { get; set; }
        public int? intEntrenador { get; set; }
        public string strCiudad { get; set; }

        // Estadísticas generales (nullable)
        public int? intEmpates { get; set; }
        public int? intDerrotas { get; set; }
        public int? intVictorias { get; set; }
        public int intJugadores { get; set; }
        public int intTorneo { get; set; }
        public int? intUsuarioCapitan { get; set; }

        // Nuevas propiedades para estadísticas (nullable)
        public int? intPartidosJugados { get; set; }
        public int? intPartidosGanados { get; set; }
        public int? intPartidosEmpatados { get; set; }
        public int? intPartidosPerdidos { get; set; }
        public int? intPuntos { get; set; }
        public int? intGolesFavor { get; set; }
        public int? intGolesContra { get; set; }

        // Estadísticas como local/visitante (nullable)
        public int? intPartidosLocal { get; set; }
        public int? intPartidosVisitante { get; set; }

        // Relación con jugadores
        public List<Jugadores>? jugadores { get; set; }
    }
}