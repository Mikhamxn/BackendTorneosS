using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendTorneosS.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BackendTorneosS.Services
{
    public class SimuladorService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        // Configuración de probabilidades (ajustables)
        private readonly Probabilidades _probabilidades = new Probabilidades
        {
            ProbabilidadGolLocalPorMinuto = 0.008,    // 0.8% por minuto (~1.44 goles/partido)
            ProbabilidadGolVisitantePorMinuto = 0.006, // 0.6% por minuto (~1.08 goles/partido)
            ProbabilidadAsistencia = 0.7,             // 70% de probabilidad de asistencia
            ProbabilidadTarjetaAmarillaPorMinuto = 0.005, // 0.5% por minuto (~2.25 tarjetas/partido)
            ProbabilidadTarjetaRojaPorMinuto = 0.001,     // 0.1% por minuto (~0.45 tarjetas/partido)
            VentajaLocal = 1.2,                       // 20% más probabilidades para el local
            ProbabilidadDelanteroAnotar = 50,        // 50% más probable que anote un delantero
            ProbabilidadCentrocampistaAsistir = 30,   // 30% más probable que asista un centrocampista
            ProbabilidadDefensaTarjeta = 40           // 40% más probable que sea amonestado un defensa
        };

        public SimuladorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public class Probabilidades
        {
            public double ProbabilidadGolLocalPorMinuto { get; set; }
            public double ProbabilidadGolVisitantePorMinuto { get; set; }
            public double ProbabilidadAsistencia { get; set; }
            public double ProbabilidadTarjetaAmarillaPorMinuto { get; set; }
            public double ProbabilidadTarjetaRojaPorMinuto { get; set; }
            public double VentajaLocal { get; set; }
            public int ProbabilidadDelanteroAnotar { get; set; }
            public int ProbabilidadCentrocampistaAsistir { get; set; }
            public int ProbabilidadDefensaTarjeta { get; set; }
        }

        public async Task<Partidos> SimularPartido(int partidoId)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.intPartido == partidoId);

            if (partido == null || partido.strEstado != "scheduled")
                return null;

            // Obtener jugadores
            var jugadoresLocal = await _context.Jugadores
                .Where(j => j.intEquipo == partido.intEquipoLocal)
                .ToListAsync();

            var jugadoresVisitante = await _context.Jugadores
                .Where(j => j.intEquipo == partido.intEquipoVisitante)
                .ToListAsync();

            // Limpiar eventos anteriores
            _context.EventosPartido.RemoveRange(
                _context.EventosPartido.Where(e => e.intPartido == partidoId));

            var eventos = new List<EventoPartido>();

            // Simulación minuto a minuto
            for (int minuto = 1; minuto <= 90; minuto++)
            {
                // Simular eventos para el equipo local
                SimularEventosEquipo(partidoId, jugadoresLocal, minuto, eventos, true);

                // Simular eventos para el equipo visitante
                SimularEventosEquipo(partidoId, jugadoresVisitante, minuto, eventos, false);
            }

            // Calcular estadísticas basadas en eventos
            CalcularEstadisticasPartido(partido, eventos, jugadoresLocal, jugadoresVisitante);

            // Guardar cambios
            await _context.EventosPartido.AddRangeAsync(eventos);
            await ActualizarEstadisticasEquipos(partido, eventos);

            partido.strEstado = "finished";
            await _context.SaveChangesAsync();

            return partido;
        }

        private void SimularEventosEquipo(int partidoId, List<Jugadores> jugadores, int minuto,
            List<EventoPartido> eventos, bool esLocal)
        {
            double probGol = esLocal ?
                _probabilidades.ProbabilidadGolLocalPorMinuto * _probabilidades.VentajaLocal :
                _probabilidades.ProbabilidadGolVisitantePorMinuto;

            // Simular gol
            if (_random.NextDouble() < probGol)
            {
                var anotador = ObtenerJugadorPorPosicion(jugadores, "Delantero", _probabilidades.ProbabilidadDelanteroAnotar);
                eventos.Add(CrearEvento(partidoId, anotador, "Gol", minuto));

                // Simular asistencia
                if (_random.NextDouble() < _probabilidades.ProbabilidadAsistencia)
                {
                    var asistente = ObtenerJugadorPorPosicion(jugadores, "Centrocampista", _probabilidades.ProbabilidadCentrocampistaAsistir);
                    if (asistente.intJugador != anotador.intJugador)
                    {
                        eventos.Add(CrearEvento(partidoId, asistente, "Asistencia", minuto));
                    }
                }
            }

            // Simular tarjetas amarillas
            if (_random.NextDouble() < _probabilidades.ProbabilidadTarjetaAmarillaPorMinuto)
            {
                var jugador = ObtenerJugadorPorPosicion(jugadores, "Defensa", _probabilidades.ProbabilidadDefensaTarjeta);
                eventos.Add(CrearEvento(partidoId, jugador, "TarjetaAmarilla", minuto));
            }

            // Simular tarjetas rojas (menos frecuentes)
            if (_random.NextDouble() < _probabilidades.ProbabilidadTarjetaRojaPorMinuto)
            {
                var jugador = ObtenerJugadorPorPosicion(jugadores, "Defensa", _probabilidades.ProbabilidadDefensaTarjeta);
                eventos.Add(CrearEvento(partidoId, jugador, "TarjetaRoja", minuto));
            }
        }

        private Jugadores ObtenerJugadorPorPosicion(List<Jugadores> jugadores, string posicion, int porcentajePreferencia)
        {
            // Si hay jugadores en la posición preferida y cumple el porcentaje
            if (_random.Next(100) < porcentajePreferencia)
            {
                var preferidos = jugadores.Where(j => j.strPosicion == posicion).ToList();
                if (preferidos.Any())
                    return preferidos[_random.Next(preferidos.Count)];
            }

            // Si no, jugador aleatorio
            return jugadores[_random.Next(jugadores.Count)];
        }

        private EventoPartido CrearEvento(int partidoId, Jugadores jugador, string tipoEvento, int minuto)
        {
            return new EventoPartido
            {
                intPartido = partidoId,
                intJugador = jugador.intJugador,
                strTipoEvento = tipoEvento,
                intMinuto = minuto
            };
        }

        private void CalcularEstadisticasPartido(Partidos partido, List<EventoPartido> eventos,
            List<Jugadores> jugadoresLocal, List<Jugadores> jugadoresVisitante)
        {
            // Contar goles
            partido.intGolesLocal = eventos.Count(e => e.strTipoEvento == "Gol" &&
                jugadoresLocal.Any(j => j.intJugador == e.intJugador));

            partido.intGolesVisitante = eventos.Count(e => e.strTipoEvento == "Gol" &&
                jugadoresVisitante.Any(j => j.intJugador == e.intJugador));

            // Contar tarjetas
            partido.intTarjetasAmarillasLocal = eventos.Count(e => e.strTipoEvento == "TarjetaAmarilla" &&
                jugadoresLocal.Any(j => j.intJugador == e.intJugador));

            partido.intTarjetasRojasLocal = eventos.Count(e => e.strTipoEvento == "TarjetaRoja" &&
                jugadoresLocal.Any(j => j.intJugador == e.intJugador));

            partido.intTarjetasAmarillasVisitante = eventos.Count(e => e.strTipoEvento == "TarjetaAmarilla" &&
                jugadoresVisitante.Any(j => j.intJugador == e.intJugador));

            partido.intTarjetasRojasVisitante = eventos.Count(e => e.strTipoEvento == "TarjetaRoja" &&
                jugadoresVisitante.Any(j => j.intJugador == e.intJugador));
        }

        private async Task ActualizarEstadisticasEquipos(Partidos partido, List<EventoPartido> eventos)
        {
            var local = await _context.Equipos.FindAsync(partido.intEquipoLocal);
            var visitante = await _context.Equipos.FindAsync(partido.intEquipoVisitante);

            // Inicializar valores si son null
            InicializarEstadisticasEquipo(local);
            InicializarEstadisticasEquipo(visitante);

            // Actualizar partidos jugados
            local.intPartidosJugados++;
            visitante.intPartidosJugados++;

            // Actualizar goles
            local.intGolesFavor += partido.intGolesLocal ?? 0;
            local.intGolesContra += partido.intGolesVisitante ?? 0;
            visitante.intGolesFavor += partido.intGolesVisitante ?? 0;
            visitante.intGolesContra += partido.intGolesLocal ?? 0;

            // Actualizar como local/visitante
            local.intPartidosLocal++;
            visitante.intPartidosVisitante++;

            // Determinar resultado
            if (partido.intGolesLocal > partido.intGolesVisitante)
            {
                local.intPartidosGanados++;
                local.intPuntos += 3;
                visitante.intPartidosPerdidos++;
            }
            else if (partido.intGolesLocal < partido.intGolesVisitante)
            {
                visitante.intPartidosGanados++;
                visitante.intPuntos += 3;
                local.intPartidosPerdidos++;
            }
            else
            {
                local.intPartidosEmpatados++;
                local.intPuntos += 1;
                visitante.intPartidosEmpatados++;
                visitante.intPuntos += 1;
            }
        }

        private void InicializarEstadisticasEquipo(Equipos equipo)
        {
            equipo.intPartidosJugados ??= 0;
            equipo.intPartidosGanados ??= 0;
            equipo.intPartidosEmpatados ??= 0;
            equipo.intPartidosPerdidos ??= 0;
            equipo.intPuntos ??= 0;
            equipo.intGolesFavor ??= 0;
            equipo.intGolesContra ??= 0;
            equipo.intPartidosLocal ??= 0;
            equipo.intPartidosVisitante ??= 0;
        }
    }
}