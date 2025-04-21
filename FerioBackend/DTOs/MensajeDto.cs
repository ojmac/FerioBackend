using FerioBackend.Models;

namespace FerioBackend.DTOs
{
    public class MensajeDto
    {
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; }
        public bool Leido { get; set; }
        public int UsuarioId { get; set; }
        public int? StandId { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
        public string? Empresa { get; set; }
    }

}
