namespace Biblioteca.Models;

public class Emprestimo
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public int ExemplarId { get; set; }

    public DateTime? DataRetirada { get; set; }
    public DateTime? DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Exemplar Exemplar { get; set; } = null!;

    public StatusEmprestimo Status { get; set; } = StatusEmprestimo.Pendente;

    public bool EmAberto =>
        Status == StatusEmprestimo.EmAndamento;

    public bool Atrasado =>
        EmAberto &&
        DataPrevistaDevolucao.HasValue &&
        DataPrevistaDevolucao.Value.Date < DateTime.Today;
}