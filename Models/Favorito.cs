namespace Biblioteca.Models;

public class Favorito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int LivroId { get; set; }

    public Livro Livro { get; set; } = null!;

    public DateTime DataFavoritado { get; set; } = DateTime.Now;
}