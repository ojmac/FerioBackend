using System;
using System.Collections.Generic;

namespace FerioBackend.Models
{
    public class Usuario
    {
        public int Id { get; set; } 
        public required string Nombre { get; set; } 
        public required string Email { get; set; } 
        public string? Contrasena { get; set; } 
        public TipoUsuario TipoUsuario { get; set; } 
        public string? FotoPerfil { get; set; } 
        public string? Telefono { get; set; } 
        public string? Direccion { get; set; } 
        public string? Empresa { get; set; } 
        public DateTime? LastLoginDate { get; set; } 
        public string? Token { get; set; } 
        public DateTime? TokenExpiration { get; set; } 

        // Relación uno a uno (si el usuario es Expositor)
        public int? StandId { get; set; } // Clave foránea explícita
        public Stand? Stand { get; set; } 

        // Relación muchos a muchos con los stands favoritos
        public List<UsuarioStandFavorito> StandsFavoritos { get; set; } = new List<UsuarioStandFavorito>();
    }


    public enum TipoUsuario
    {
        Visitante = 0,
        Expositor = 1,
        Organizador = 2
    }
}
