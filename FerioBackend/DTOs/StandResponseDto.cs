using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerioBackend.DTOs
{
    public class StandResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Logo { get; set; }
        public string Ubicacion { get; set; }
        public string HorarioAtencion { get; set; }
        public string EnlaceWeb { get; set; }
        public string Contacto { get; set; }
        public int UsuarioId { get; set; }
        public List<int> CategoriaIds { get; set; }
    }
}