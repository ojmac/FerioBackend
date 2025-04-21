using FerioBackend.Models;
namespace FerioBackend.Models
{
    public class UsuarioStandFavorito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int StandId { get; set; }
        public Stand Stand { get; set; }
    }
}