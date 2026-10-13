using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface IEditoraRepository : IRepository<Editora>
{
    Task<bool> PossuiLivrosAsync(int id);
}

public class EditoraRepository : Repository<Editora>, IEditoraRepository
{
    public EditoraRepository(BibliotecaContext ctx) : base(ctx) { }

    public override async Task<List<Editora>> ObterTodosAsync()
        => await _set.OrderBy(x => x.Nome).ToListAsync();

    public async Task<bool> PossuiLivrosAsync(int id)
        => await _set.AnyAsync(x => x.Id == id && x.Livros.Any());
}
