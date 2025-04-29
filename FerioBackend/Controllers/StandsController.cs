using Microsoft.AspNetCore.Mvc;
using FerioBackend.Data;
using FerioBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using FerioBackend.DTOs;
using System.Diagnostics;

namespace FerioBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandsController : ControllerBase
    {
        private readonly FerioDbContext _context;

        public StandsController(FerioDbContext context)
        {
            _context = context;
        }

        // GET: Obtiene todos los stands 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stand>>> GetStands()
        {
            return await _context.Stands.ToListAsync();
        }

        // GET: Obtiene un stand por ID (Accesible para organizadores o para el Expositor dueño de su propio stand)
        [HttpGet("{id}")]
        [Authorize(Roles = "Organizador, Expositor")]
        public async Task<ActionResult<StandResponseDto>> GetStand(int id)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("No se pudo obtener el ID del usuario.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var userRole = User.FindFirst("Role")?.Value;

            var stand = await _context.Stands
                .Include(s => s.StandCategoria) 
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stand == null)
            {
                return NotFound();
            }

            if (userRole == "Organizador" || stand.UsuarioId == userId)
            {
               
                var standResponseDto = ConvertToStandResponseDto(stand);
                return Ok(standResponseDto);
            }

            return Unauthorized("No tienes permiso para acceder a este stand.");
        }

        private StandResponseDto ConvertToStandResponseDto(Stand stand)
        {
            return new StandResponseDto
            {
                Id = stand.Id,
                Nombre = stand.Nombre,
                Descripcion = stand.Descripcion,
                Logo = stand.Logo,
                Ubicacion = stand.Ubicacion,
                EnlaceWeb = stand.EnlaceWeb,
                Contacto = stand.Contacto,
                UsuarioId = stand.UsuarioId,
                HorarioAtencion = stand.HorarioAtencion,
                PosX = stand.PosX,
                PosY = stand.PosY,
                Width = stand.Width,
                Height = stand.Height,

                CategoriaIds = stand.StandCategoria?.Select(sc => sc.CategoriaId).ToList() ?? new List<int>()
            };
        }


        // POST: Crea un nuevo stand (Solo el organizador puede crear stands)
        [HttpPost]
        [Authorize(Roles = "Organizador")] 
        public async Task<ActionResult<StandResponseDto>> PostStand(StandCreateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
            {
                return BadRequest("El usuario indicado no existe.");
            }

            var stand = new Stand
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Logo = dto.Logo,
                Ubicacion = dto.Ubicacion,
                HorarioAtencion = dto.HorarioAtencion,
                EnlaceWeb = dto.EnlaceWeb,
                Contacto = dto.Contacto,
                UsuarioId = dto.UsuarioId,
                Usuario = usuario,
                PosX = dto.PosX,
                PosY = dto.PosY,
                Width = dto.Width,
                Height = dto.Height,
                StandCategoria = dto.CategoriaIds.Select(id => new StandCategoria
                {
                    CategoriaId = id
                }).ToList()
            };

            _context.Stands.Add(stand);
            await _context.SaveChangesAsync();

            usuario.StandId = stand.Id;
            await _context.SaveChangesAsync();

            var responseDto = new StandResponseDto
            {
                Id = stand.Id,
                Nombre = stand.Nombre,
                Descripcion = stand.Descripcion,
                Logo = stand.Logo,
                Ubicacion = stand.Ubicacion,
                HorarioAtencion = stand.HorarioAtencion,
                EnlaceWeb = stand.EnlaceWeb,
                Contacto = stand.Contacto,
                UsuarioId = stand.UsuarioId,
                PosX = stand.PosX,
                PosY = stand.PosY,
                Width = stand.Width,
                Height = stand.Height,
                CategoriaIds = stand.StandCategoria.Select(sc => sc.CategoriaId).ToList()
            };

            return CreatedAtAction(nameof(GetStand), new { id = stand.Id }, responseDto);
        }



        [HttpGet("categorias")] // Obtiene la lista de categorías
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var categorias = await _context.Categorias.ToListAsync();

            if (categorias == null || !categorias.Any())
            {
                return NotFound("No hay categorías registradas.");
            }

            return Ok(categorias);
        }

        // PUT: Actualiza un stand existente (Solo el organizador o el Expositor dueño del stand pueden modificarlo)
        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutStand(int id, StandCreateDto stand)
        {
           

            var userIdClaim = User.FindFirst("UserId");
           

            if (userIdClaim == null )
            {
                return Unauthorized("No se pudo obtener información del usuario autenticado.");
            }

            var userId = int.Parse(userIdClaim.Value);
            

            var existingStand = await _context.Stands
                .Include(s => s.StandCategoria)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStand == null)
            {
                return NotFound("No se encontró el stand.");
            }

            var esDueño = existingStand.UsuarioId == userId;
           

            if (!esDueño )
            {
                return Unauthorized("No tienes permiso para modificar este stand.");
            }

            var usuario = await _context.Usuarios.FindAsync(stand.UsuarioId);
            if (usuario == null)
            {
                return BadRequest("El usuario indicado no existe.");
            }
        
            existingStand.Nombre = stand.Nombre;
            existingStand.Descripcion = stand.Descripcion;
            existingStand.Logo = stand.Logo;
            existingStand.Ubicacion = stand.Ubicacion;
            existingStand.HorarioAtencion = stand.HorarioAtencion;
            existingStand.EnlaceWeb = stand.EnlaceWeb;
            existingStand.Contacto = stand.Contacto;
            existingStand.UsuarioId = stand.UsuarioId;
            existingStand.PosX = stand.PosX;
            existingStand.PosY = stand.PosY;
            existingStand.Width = stand.Width;
            existingStand.Height = stand.Height;

            var categoriasAnteriores = _context.StandCategoria
                .Where(sc => sc.StandId == existingStand.Id);
            _context.StandCategoria.RemoveRange(categoriasAnteriores);

          
            if (stand.CategoriaIds != null && stand.CategoriaIds.Any())
            {
                foreach (var catId in stand.CategoriaIds)
                {
                    _context.StandCategoria.Add(new StandCategoria
                    {
                        StandId = existingStand.Id,
                        CategoriaId = catId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: Elimina un stand (Sólo el organizador)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> DeleteStand(int id)
        {
            var stand = await _context.Stands.FindAsync(id);
            if (stand == null)
                return NotFound("El stand no existe.");

            
            var relaciones = _context.StandCategoria.Where(sc => sc.StandId == id);
            _context.StandCategoria.RemoveRange(relaciones);
            await _context.SaveChangesAsync();
           
            _context.Stands.Remove(stand);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
