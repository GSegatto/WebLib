namespace Biblioteca.Models;

public class TipoUsuario
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
