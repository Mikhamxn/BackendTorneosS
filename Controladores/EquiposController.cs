using BackendTorneosS.Contexto;
using BackendTorneosS.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendTorneosS.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquiposController : ControllerBase
    {
        private readonly DBContext _context;

        public EquiposController(DBContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipos>>> GetEquipos()
        {
            return await _context.Equipos.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Equipos>> PostEquipos(Equipos equipos)
        {
            var equipoExistente = await _context.Equipos.FirstOrDefaultAsync(e => e.strNombre == equipos.strNombre);

            if (equipoExistente != null)
            {
                return BadRequest("El equipo ya está registrado");
            }

            _context.Equipos.Add(equipos);
            await _context.SaveChangesAsync();

            if (equipos.jugadores != null && equipos.jugadores.Any())
            {
                foreach (var jugador in equipos.jugadores)
                {
                    jugador.intJugador = 0;
                    jugador.intEquipo = equipos.intEquipo;   // asigna FK
                    _context.Jugadores.Add(jugador);
                }
                await _context.SaveChangesAsync();
            }

            var capitan = _context.Jugadores.FirstOrDefault(j => j.intEquipo == equipos.intEquipo && j.bitCapitan);

            if (capitan != null)
            {
                
                equipos.intUsuarioCapitan = capitan.intJugador;
                _context.Equipos.Update(equipos);
                await _context.SaveChangesAsync();
            }

            return Created($"{equipos.intEquipo}", equipos);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipos(int id, Equipos equipos)
        {
            if (id != equipos.intEquipo)
                return BadRequest();

            _context.Entry(equipos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Equipos.Any(e => e.intEquipo == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }
    }
}
