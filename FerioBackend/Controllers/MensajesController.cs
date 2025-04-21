using Microsoft.AspNetCore.Mvc;
using FerioBackend.Data;
using FerioBackend.Models;
using FerioBackend.DTOs;    
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace FerioBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensajesController : ControllerBase
    {
        private readonly FerioDbContext _context;

        public MensajesController(FerioDbContext context)
        {
            _context = context;
        }

        // GET: api/mensajes (Obtiene todos los mensajes, seugún el tipo de usuario)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MensajeDto>>> GetMensajes([FromQuery] TipoUsuario? tipoUsuario = null)
        {
            var mensajes = tipoUsuario switch
            {
                TipoUsuario.Expositor => await _context.Mensajes
                    .Where(m => m.TipoUsuario == TipoUsuario.Expositor || m.TipoUsuario == TipoUsuario.Organizador)
                    .Select(m => new MensajeDto
                    {
                        Titulo = m.Titulo,
                        Contenido = m.Contenido,
                        FechaEnvio = m.FechaEnvio,
                        Leido = m.Leido,
                        UsuarioId = m.UsuarioId,
                        StandId = m.StandId,
                        TipoUsuario = m.TipoUsuario,
                        Empresa = m.Stand.Nombre
                    })
                    .ToListAsync(),

                TipoUsuario.Visitante => await _context.Mensajes
                    .Where(m => m.TipoUsuario == TipoUsuario.Visitante || m.TipoUsuario == TipoUsuario.Organizador)
                    .Select(m => new MensajeDto
                    {
                        Titulo = m.Titulo,
                        Contenido = m.Contenido,
                        FechaEnvio = m.FechaEnvio,
                        Leido = m.Leido,
                        UsuarioId = m.UsuarioId,
                        StandId = m.StandId,
                        TipoUsuario = m.TipoUsuario,
                        Empresa = m.Stand.Nombre
                    })
                    .ToListAsync(),

                _ => await _context.Mensajes
                    .Select(m => new MensajeDto
                    {
                        Titulo = m.Titulo,
                        Contenido = m.Contenido,
                        FechaEnvio = m.FechaEnvio,
                        Leido = m.Leido,
                        UsuarioId = m.UsuarioId,
                        StandId = m.StandId,
                        TipoUsuario = m.TipoUsuario,
                        Empresa = m.Stand.Nombre
                    })
                    .ToListAsync()
            };

            return Ok(mensajes);
        }


        // GET: api/mensajes/5 (Obtiene un mensaje por su ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Mensaje>> GetMensaje(int id)
        {
            var mensaje = await _context.Mensajes.FindAsync(id);

            if (mensaje == null)
            {
                return NotFound();
            }

            return mensaje;
        }

        // POST: api/mensajes (Crea un nuevo mensaje)
        [HttpPost]
        [Authorize(Roles = "Organizador, Expositor")] 
        public async Task<ActionResult<Mensaje>> PostMensaje(MensajeDto mensaje)
        {
            
            var usuario = await _context.Usuarios.FindAsync(mensaje.UsuarioId);
            if (usuario == null)
                return BadRequest("Usuario no válido.");

            Stand? stand = null;
            if (mensaje.StandId.HasValue)
            {
                stand = await _context.Stands.FindAsync(mensaje.StandId.Value);
                if (stand == null)
                    return BadRequest("Stand no válido.");
            }
            var mensajeR = new Mensaje
            {
                Titulo = mensaje.Titulo,
                Contenido = mensaje.Contenido,
                FechaEnvio = mensaje.FechaEnvio,
                Leido = mensaje.Leido,
                Usuario = usuario,
                Stand = stand,  
                UsuarioId = mensaje.UsuarioId,
                StandId = mensaje.StandId,
                TipoUsuario = mensaje.TipoUsuario 
            };

            _context.Mensajes.Add(mensajeR);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMensaje), new { id = mensajeR.Id }, mensaje);
        }

        // PUT: api/mensajes/5 (Actualiza un mensaje existente)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organizador")] // Solo el organizador puede actualizar los mensajes, no implementado
        public async Task<IActionResult> PutMensaje(int id, Mensaje mensaje)
        {
            if (id != mensaje.Id)
            {
                return BadRequest();
            }

            _context.Entry(mensaje).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/mensajes/5 (Elimina un mensaje), Solo el organizador puede eliminar mensajes 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizador")] 
        public async Task<IActionResult> DeleteMensaje(int id)
        {
            var mensaje = await _context.Mensajes.FindAsync(id);
            if (mensaje == null)
            {
                return NotFound();
            }

            _context.Mensajes.Remove(mensaje);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
