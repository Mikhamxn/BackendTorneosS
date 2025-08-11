using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendTorneosS.Entidades
{
    public class Usuarios
    {
        [Key]
        public int intUsuario { get; set; }

        [Column("strNombre")]
        public string strNombre { get; set; }

        [Column("strApellidoPaterno")]
        public string strApellidoPaterno { get; set; }

        [Column("strApellidoMaterno")]
        public string strApellidoMaterno { get; set; }

        [Column("strCorreo")]
        public string strCorreo { get; set; }

        [Column("strPass")]
        public string strPass { get; set; }

        [Column("strMoneda")]
        public string strMoneda { get; set; }

        [Column("datFechaRegistro")]
        public DateTime? datFechaRegistro { get; set; }

        [Column("datFechaActualizacion")]
        public DateTime? datFechaActualizacion { get; set; }

        [Column("datFechaUltimoLogin")]
        public DateTime? datFechaUltimoLogin { get; set; }

        [Column("bitEstatus")]
        public bool bitEstatus { get; set; } = true;
        [Column("datFechaToken")]
        public DateTime? datFechaToken { get; set; }
        [Column("strTokenRecuperacion")]
        public string? strTokenRecuperacion { get; set; }
        [Column("isAdmin")]
        public bool? isAdmin { get; set; }
    }
}

