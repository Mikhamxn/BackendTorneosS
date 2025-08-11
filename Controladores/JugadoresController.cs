
using BackendTorneosS.Contexto;
using BackendTorneosS.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendTorneosS.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class JugadoresController : ControllerBase
    {
        private readonly DBContext _context;

        public JugadoresController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Jugadores>>> GetJugadores()
        {
            return await _context.Jugadores.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Jugadores>> GetJugador(int id)
        {
            // Usamos FindAsync para buscar por PK
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
                return NotFound();      // 404 si no existe

            return jugador;            // 200 + objeto JSON
        }

        [HttpPost]
        public async Task<ActionResult<Jugadores>> PostEquipos(Jugadores Jugadores)
        {
            _context.Jugadores.Add(Jugadores);
            await _context.SaveChangesAsync();

            return Created($"{Jugadores.intJugador}", Jugadores);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTorneos(int id, Jugadores Jugadores)
        {
            if (id != Jugadores.intJugador)
                return BadRequest();

            _context.Entry(Jugadores).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Jugadores.Any(e => e.intJugador == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }
    }
}

