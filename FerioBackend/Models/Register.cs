namespace FerioBackend.Models
{
    public class Register
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public TipoUsuario TipoUsuario { get; set; }  
        //public bool Confirmado { get; set; } = false;
        //public string? TokenConfirmacion { get; set; }
    }
}
