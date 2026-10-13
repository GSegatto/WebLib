using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface ILivroRepository : IRepository<Livro>
{
    Task<bool> IsbnExisteAsync(string isbn, int? ignorarId = null);
    Task<List<Livro>> BuscarAsync(string? termo, int? categoriaId);
    Task<List<Livro>> ObterDestaquesAsync();
}

public class LivroRepository : Repository<Livro>, ILivroRepository
{
    public LivroRepository(BibliotecaContext ctx) : base(ctx) { }

    private IQueryable<Livro> ComDetalhes() => _set
        .Include(l => l.Categoria)
        .Include(l => l.Editora)
        .Include(l => l.Autores)
        .Include(l => l.Exemplares);

    public override async Task<List<Livro>> ObterTodosAsync()
        => await ComDetalhes()
            .OrderBy(l => l.Titulo)
            .ToListAsync();

    public override async Task<Livro?> ObterPorIdAsync(int id)
        => await ComDetalhes()
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<List<Livro>> ObterDestaquesAsync()
        => await ComDetalhes()
            .Where(l => l.Destaque)
            .OrderBy(l => l.Titulo)
            .Take(4)
            .ToListAsync();

    public async Task<bool> IsbnExisteAsync(string isbn, int? ignorarId = null)
        => await _set.AnyAsync(l =>
            l.Isbn == isbn &&
            (ignorarId == null || l.Id != ignorarId));

    public async Task<List<Livro>> BuscarAsync(string? termo, int? categoriaId)
    {
        var q = ComDetalhes();

        if (categoriaId.HasValue)
            q = q.Where(l => l.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();

            q = q.Where(l =>
                l.Titulo.Contains(t) ||
                l.Isbn.Contains(t) ||
                l.Editora.Nome.Contains(t) ||
                l.Autores.Any(a => a.Nome.Contains(t)));
        }

        return await q
            .OrderBy(l => l.Titulo)
            .ToListAsync();
    }
}