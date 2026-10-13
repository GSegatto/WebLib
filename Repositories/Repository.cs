using Biblioteca.Data;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BibliotecaContext _ctx;
    protected readonly DbSet<T> _set;

    public Repository(BibliotecaContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public virtual async Task<List<T>> ObterTodosAsync() => await _set.ToListAsync();

    public virtual async Task<T?> ObterPorIdAsync(int id) => await _set.FindAsync(id);

    public async Task AdicionarAsync(T entidade)
    {
        _set.Add(entidade);
        await _ctx.SaveChangesAsync();
    }

    public async Task AtualizarAsync(T entidade) => await _ctx.SaveChangesAsync();

    public async Task RemoverAsync(T entidade)
    {
        _set.Remove(entidade);
        await _ctx.SaveChangesAsync();
    }
}
