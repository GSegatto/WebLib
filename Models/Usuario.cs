namespace Biblioteca.Models;

public class Usuario
{
    public int Id { get; set; }
    public int TipoUsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;

    public TipoUsuario TipoUsuario { get; set; } = null!;
    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
}
