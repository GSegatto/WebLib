namespace Biblioteca.Models;

public class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    // Caminho da imagem utilizada na Home.
    public string? Imagem { get; set; }

    // Indica se a categoria aparece entre as principais.
    public bool EhPrincipal { get; set; } = false;

    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}