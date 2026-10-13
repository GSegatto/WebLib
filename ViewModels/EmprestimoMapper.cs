using Biblioteca.Models;

namespace Biblioteca.ViewModels;

public static class EmprestimoMapper
{
    public static EmprestimoListaViewModel ParaLista(this Emprestimo e) => new()
    {
        Id = e.Id,
        Usuario = e.Usuario.Nome,
        Titulo = e.Exemplar.Livro.Titulo,
        NumeroExemplar = e.Exemplar.NumeroExemplar,

        DataRetirada = e.DataRetirada,
        DataPrevistaDevolucao = e.DataPrevistaDevolucao,
        DataDevolucao = e.DataDevolucao,

        Status = e.Status,
        EmAberto = e.EmAberto,
        Atrasado = e.Atrasado
    };
}