namespace Biblioteca.Models;

public class Livro
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public int EditoraId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }

    public bool Destaque { get; set; }
    public string? Capa { get; set; }

    public Categoria Categoria { get; set; } = null!;
    public Editora Editora { get; set; } = null!;

    // N:N com Autor -> tabela associativa Livro_Autor (configurada no DbContext)
    public ICollection<Autor> Autores { get; set; } = new List<Autor>();
    public ICollection<Exemplar> Exemplares { get; set; } = new List<Exemplar>();
}