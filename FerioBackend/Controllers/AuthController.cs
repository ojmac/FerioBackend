using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FerioBackend.Data;  
using FerioBackend.Models; 
using Microsoft.EntityFrameworkCore;
using FerioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
//using FerioBackend.Interfaces; // doble autenticacion con email, no implementado


namespace FerioBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly FerioDbContext _context;
        private readonly IConfiguration _configuration;
        //private readonly IEmailService _emailService;

        public AuthController(FerioDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            //_emailService = emailService ?? throw new ArgumentNullException(nameof(emailService)); // Verifica si es null
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            Debug.WriteLine("Inicio del método Login");

            // Buscar al usuario por su correo
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == login.Email);
            Debug.WriteLine(user == null ? "Usuario no encontrado" : $"Usuario encontrado: {user.Email}");

            if (user == null || !AuthService.VerifyPasswordHash(login.Password, user.Contrasena))
            {
                Debug.WriteLine("Contraseña incorrecta o usuario no encontrado");
                return Unauthorized("Correo o contraseña incorrectos.");
            }

            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                Debug.WriteLine("Clave secreta JWT no configurada");
                return BadRequest("La clave secreta del JWT no está configurada.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Generar el JWT
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Nombre),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.TipoUsuario.ToString())
    };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Debug.WriteLine($"Token generado: {tokenString}");

            user.Token = tokenString;
            user.TokenExpiration = token.ValidTo;

            try
            {
                _context.Usuarios.Update(user);
                await _context.SaveChangesAsync();
                Debug.WriteLine("Token y expiración guardados en la base de datos");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al guardar token en BD: {ex.Message}");
                return StatusCode(500, "Error interno al guardar el token.");
            }

            var response = new
            {
                token = tokenString,
                expiration = token.ValidTo
            };

            // Actualiza la fecha del último login
            user.LastLoginDate = DateTime.Now;

            try
            {
                _context.Usuarios.Update(user);
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Última fecha de login actualizada: {user.LastLoginDate}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar LastLoginDate: {ex.Message}");
            }

            Debug.WriteLine("Login completado correctamente");
            return Ok(response);
        }

        //// POST api/auth/login
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] Login login)
        //{
        //    // Buscar al usuario por su correo
        //    var user = await _context.Usuarios
        //        .FirstOrDefaultAsync(u => u.Email == login.Email);

        //    if (user == null || !AuthService.VerifyPasswordHash(login.Password, user.Contrasena))  // Usar el servicio para verificar
        //    {
        //        return Unauthorized("Correo o contraseña incorrectos.");
        //    }

        //    var secretKey = _configuration["Jwt:SecretKey"];
        //    if (string.IsNullOrEmpty(secretKey))
        //    {
        //        return BadRequest("La clave secreta del JWT no está configurada.");
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        //    // Generar el JWT
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, user.Nombre),
        //        new Claim(ClaimTypes.Email, user.Email),
        //        new Claim("UserId", user.Id.ToString()),
        //        new Claim(ClaimTypes.Role, user.TipoUsuario.ToString())
        //    };

        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(

        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddHours(8), // Expira en 8 horas
        //        signingCredentials: creds
        //    );

        //    // Guarda el token en el usuario
        //    user.Token = new JwtSecurityTokenHandler().WriteToken(token);
        //    user.TokenExpiration = token.ValidTo; // Guardar la fecha de expiración del token
        //    _context.Usuarios.Update(user);
        //    await _context.SaveChangesAsync();

        //    // Devuelve el token JWT y su fecha de expiración
        //    var response = new
        //    {
        //        token = new JwtSecurityTokenHandler().WriteToken(token),
        //        expiration = token.ValidTo
        //    };

        //    // Actualiza la fecha del último login del usuario 
        //    user.LastLoginDate = DateTime.Now;
        //    _context.Usuarios.Update(user);
        //    await _context.SaveChangesAsync();

        //    // Devuelve el token en la respuesta
        //    return Ok(response);
        //}

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] Register register)
        //{
        //    // Verifica si el usuario ya existe
        //    var existingUser = await _context.Usuarios
        //        .FirstOrDefaultAsync(u => u.Email == register.Email);
        //    if (existingUser != null)
        //    {
        //        return BadRequest("El correo electrónico ya está registrado.");
        //    }

        //    // Validación: solo los Visitantes pueden registrarse por sí mismos
        //    if (register.TipoUsuario != TipoUsuario.Visitante)
        //    {
        //        return BadRequest("Solo los Visitantes pueden registrarse por sí mismos.");
        //    }

        //    // Hash de la contraseña antes de almacenarla
        //    var hashedPassword = AuthService.HashPassword(register.Password);
        //    var token = Guid.NewGuid().ToString(); // Token de confirmación único

        //    // Crear un nuevo objeto Usuario
        //    var user = new Usuario
        //    {
        //        Nombre = register.Nombre,
        //        Email = register.Email,
        //        Contrasena = hashedPassword,  // Guardamos la contraseña cifrada
        //        TipoUsuario = register.TipoUsuario, // El tipo de usuario es Visitante
        //        //Confirmado = false,
        //        //TokenConfirmacion = token
        //    };

        //    // Agrega el nuevo usuario a la base de datos
        //    //_context.Usuarios.Add(user);
        //    //await _context.SaveChangesAsync();

        //    //// Envia email de confirmación
        //    //var confirmLink = $"{_configuration["AppUrl"]}/api/auth/confirm?token={token}";
        //    //var emailBody = $"<h2>Bienvenido a Ferio</h2><p>Haz clic en el siguiente enlace para activar tu cuenta:</p><a href='{confirmLink}'>Activar Cuenta</a>";
        //    //_emailService.SendEmailAsync(user.Email, "Confirma tu cuenta", emailBody);

        //    //return Ok("Registro exitoso. Revisa tu correo para activar tu cuenta.");
        //}

        //[HttpGet("confirm")]
        //public async Task<IActionResult> ConfirmarCuenta([FromQuery] string token)
        //{
        //    // Busca al usuario con el token de confirmación
        //    var user = await _context.Usuarios
        //        .FirstOrDefaultAsync(u => u.TokenConfirmacion == token);

        //    
        //    if (user == null)
        //    {
        //        return BadRequest("Token de confirmación inválido.");
        //    }

        //    // Confirma la cuenta y limpia el token
        //    user.Confirmado = true;
        //    user.TokenConfirmacion = null;  // Limpiar el token después de confirmar

        //    // Actualiza el usuario en la base de datos
        //    _context.Usuarios.Update(user);
        //    await _context.SaveChangesAsync();

        //    return Ok("Cuenta confirmada exitosamente.");
        //}



        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous] // Permite acceso sin autenticación
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == register.Email);
            if (existingUser != null)
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            if (register.TipoUsuario != TipoUsuario.Visitante)
            {
                return BadRequest("Solo los Visitantes pueden registrarse por sí mismos.");
            }

            var hashedPassword = AuthService.HashPassword(register.Password);

            var user = new Usuario
            {
                Nombre = register.Nombre,
                Email = register.Email,
                Contrasena = hashedPassword, 
                TipoUsuario = register.TipoUsuario
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }


        // POST api/auth/register (Solo Organizador puede registrar Expositores y otros Organizadores, se hará por la api)

        [HttpPost("registerOrganizador")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterOrganizador([FromBody] Register register)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == register.Email);
            if (existingUser != null)
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            if (register.TipoUsuario == TipoUsuario.Visitante)
            {
                return BadRequest("Los Visitantes deben registrarse por sí mismos.");
            }

            var hashedPassword = AuthService.HashPassword(register.Password);

            var user = new Usuario
            {
                Nombre = register.Nombre,
                Email = register.Email,
                Contrasena = hashedPassword, 
                TipoUsuario = register.TipoUsuario 
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }

    }
}
