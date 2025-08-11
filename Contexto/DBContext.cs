using BackendTorneosS.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BackendTorneosS.Contexto
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<UsuarioSesion> UsuarioSesion { get; set; }
        public DbSet<Equipos> Equipos { get; set; }
        public DbSet<Jugadores> Jugadores { get; set; }
        public DbSet<Torneos> Torneos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelo)
        {
            modelo.HasDefaultSchema("Torneos");

            modelo.Entity<Usuarios>().ToTable("tblUsuarios");
            modelo.Entity<Torneos>().ToTable("tblTorneos");
            modelo.Entity<Equipos>().ToTable("tblEquipos");
            modelo.Entity<Jugadores>().ToTable("tblJugadores");
            modelo.Entity<UsuarioSesion>().ToTable("tblUsuarioSesion");

            modelo.Entity<Torneos>().Property(x => x.intTorneo).ValueGeneratedOnAdd();
            modelo.Entity<Jugadores>().Property(x => x.intJugador).ValueGeneratedOnAdd();
            modelo.Entity<Equipos>().Property(x => x.intEquipo).ValueGeneratedOnAdd();
            modelo.Entity<Usuarios>().Property(x => x.intUsuario).ValueGeneratedOnAdd();
            modelo.Entity<UsuarioSesion>().Property(x => x.intUsuarioSesion).ValueGeneratedOnAdd();
        }
    }
}
