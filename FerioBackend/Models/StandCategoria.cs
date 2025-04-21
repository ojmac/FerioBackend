using FerioBackend.Models;

public class StandCategoria
{
    public int StandId { get; set; }
    public Stand Stand { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; }
}
