

using System.ComponentModel.DataAnnotations;

namespace BackendTorneosS.Entidades
{
    public class UsuarioSesion
    {
        [Key]
        public int intUsuarioSesion { get; set; }
        public int intUsuario { get; set; }
        public string strTokenHash { get; set; }
        public string strDeviceInfo { get; set; }
        public string strIPAddress { get; set; }
        public DateTime datFechaCreado { get; set; }
        public string strJti { get; set; }
        public DateTime datExpiracion { get; set; }
        public bool bitRevoked { get; set; }
    }
}
