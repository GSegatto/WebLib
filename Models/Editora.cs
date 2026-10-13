namespace Biblioteca.Models;

public class Editora
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
