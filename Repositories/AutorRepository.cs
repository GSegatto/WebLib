using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface IAutorRepository : IRepository<Autor>
{
    Task<bool> PossuiLivrosAsync(int id);
    Task<List<Autor>> ObterPorIdsAsync(IEnumerable<int> ids);
}

public class AutorRepository : Repository<Autor>, IAutorRepository
{
    public AutorRepository(BibliotecaContext ctx) : base(ctx) { }

    public override async Task<List<Autor>> ObterTodosAsync()
        => await _set.OrderBy(x => x.Nome).ToListAsync();

    public async Task<bool> PossuiLivrosAsync(int id)
        => await _set.AnyAsync(x => x.Id == id && x.Livros.Any());

    public async Task<List<Autor>> ObterPorIdsAsync(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        return await _set.Where(a => lista.Contains(a.Id)).ToListAsync();
    }
}
