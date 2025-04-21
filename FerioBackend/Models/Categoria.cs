namespace FerioBackend.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } 

        public ICollection<StandCategoria> StandCategoria { get; set; }
    }

}
