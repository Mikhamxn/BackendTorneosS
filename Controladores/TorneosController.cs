using BackendTorneosS.Contexto;
using BackendTorneosS.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendTorneosS.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class TorneosController : ControllerBase
    {
        private readonly DBContext _context;

        public TorneosController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Torneos>>> GetTorneos()
        {
            return await _context.Torneos.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Torneos>> PostEquipos(Torneos torneos)
        {
            var equipoExistente = await _context.Torneos.FirstOrDefaultAsync(e => e.strNombre == torneos.strNombre);

            if (equipoExistente != null)
            {
                return BadRequest("El equipo ya está registrado");
            }

            _context.Torneos.Add(torneos);
            await _context.SaveChangesAsync();

            return Created($"{torneos.intTorneo}", torneos);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTorneos(int id, Torneos torneos)
        {
            if (id != torneos.intTorneo)
                return BadRequest();

            _context.Entry(torneos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Torneos.Any(e => e.intTorneo == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }
    }
}
