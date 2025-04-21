using Microsoft.AspNetCore.Mvc;
using FerioBackend.Data;
using FerioBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FerioBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FerioDbContext _context;

        public UsuariosController(FerioDbContext context)
        {
            _context = context;
        }
        //obtiene todos los usuarios
        [HttpGet]
        [Authorize(Roles = "Organizador")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/usuarios/ (Obtiene un usuario por ID)
        [HttpGet("{id}")]
        
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new Usuario
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Empresa = u.Empresa,
                    Direccion = u.Direccion,
                    FotoPerfil = u.FotoPerfil,
                    LastLoginDate = u.LastLoginDate,
                    StandId = u.StandId,
                    StandsFavoritos = u.StandsFavoritos,
                    TipoUsuario = u.TipoUsuario,
                    Token = u.Token,
                    TokenExpiration = u.TokenExpiration
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(usuario);
        }


        
        // PUT: api/usuarios/ (Actualiza un usuario existente, sólo ciertos campos)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("El ID en la URL y en el cuerpo deben coincidir.");
            }


            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            
            var usuarioLogueadoId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var esOrganizador = User.IsInRole("Organizador");

           
            if (usuarioLogueadoId != id && !esOrganizador)
            {
                return Unauthorized("No tienes permisos para modificar este perfil.");
            }

            usuarioExistente.Nombre = usuarioExistente.Nombre;
            usuarioExistente.Email = usuarioExistente.Email;
            usuarioExistente.Telefono = usuario.Telefono;
            usuarioExistente.Empresa = usuario.Empresa;
            usuarioExistente.Direccion = usuario.Direccion;
            usuarioExistente.FotoPerfil = usuario.FotoPerfil;
            usuarioExistente.Contrasena = usuarioExistente.Contrasena;
            usuarioExistente.StandId = usuario.StandId;



            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/usuarios/5 (Elimina un usuario), solo lo puede hacer un organizador
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizador")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
