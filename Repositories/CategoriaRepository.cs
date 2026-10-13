using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<bool> PossuiLivrosAsync(int id);

    Task<List<Categoria>> ObterPrincipaisAsync();
}

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(BibliotecaContext ctx) : base(ctx) { }

    public override async Task<List<Categoria>> ObterTodosAsync()
        => await _set.OrderBy(x => x.Nome).ToListAsync();

    public async Task<bool> PossuiLivrosAsync(int id)
        => await _set.AnyAsync(x => x.Id == id && x.Livros.Any());

    public async Task<List<Categoria>> ObterPrincipaisAsync()
    => await _set
        .Where(c => c.EhPrincipal)
        .OrderBy(c => c.Nome)
        .Take(6)
        .ToListAsync();
}
