namespace Biblioteca.Models;

public class Exemplar
{
    public int Id { get; set; }
    public int LivroId { get; set; }
    public string NumeroExemplar { get; set; } = string.Empty; // número de tombo
    public SituacaoExemplar Situacao { get; set; } = SituacaoExemplar.Disponivel;

    public Livro Livro { get; set; } = null!;
    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();

    public string SituacaoTexto => Situacao switch
    {
        SituacaoExemplar.Disponivel => "Disponível",
        SituacaoExemplar.Reservado => "Reservado",
        SituacaoExemplar.Emprestado => "Emprestado",
        _ => "Em manutenção"
    };
}
