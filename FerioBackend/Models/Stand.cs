using System.Collections.Generic;

namespace FerioBackend.Models
{
    public class Stand
    {
        public int Id { get; set; } 
        public string Nombre { get; set; } 
        public string Descripcion { get; set; } 
        public string Logo { get; set; } 
        public string Ubicacion { get; set; } 
        public string EnlaceWeb { get; set; } 
        public string Contacto { get; set; }

        public int PosX { get; set; }  // Coordenada X en el plano
        public int PosY { get; set; }  // Coordenada Y en el plano
        public int Width { get; set; } = 2; 
        public int Height { get; set; } = 2;


        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        
        public string HorarioAtencion { get; set; }

        
        public ICollection<StandCategoria> StandCategoria { get; set; }
    }
}
