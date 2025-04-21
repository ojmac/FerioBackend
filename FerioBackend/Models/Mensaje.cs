namespace FerioBackend.Models
{
    public class Mensaje
    {
        public int Id { get; set; } 
        public string Titulo { get; set; } 
        public string Contenido { get; set; } 
        public DateTime FechaEnvio { get; set; } 
        public bool Leido { get; set; }   

      
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        
        public int? StandId { get; set; }
        public Stand? Stand { get; set; }

        
        public TipoUsuario TipoUsuario { get; set; }
    }
}

