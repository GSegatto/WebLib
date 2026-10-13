using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<List<TipoUsuario>> ObterTiposAsync();
    Task<TipoUsuario?> ObterTipoPorDescricaoAsync(string descricao);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email, int? ignorarId = null);
    Task<bool> PossuiEmprestimosAsync(int id);
}

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(BibliotecaContext ctx) : base(ctx) { }

    public override async Task<List<Usuario>> ObterTodosAsync()
        => await _set.Include(u => u.TipoUsuario).OrderBy(u => u.Nome).ToListAsync();

    public override async Task<Usuario?> ObterPorIdAsync(int id)
        => await _set.Include(u => u.TipoUsuario).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> ObterPorEmailAsync(string email)
        => await _set.Include(u => u.TipoUsuario).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<List<TipoUsuario>> ObterTiposAsync()
        => await _ctx.TiposUsuario.OrderBy(t => t.Descricao).ToListAsync();

    public async Task<TipoUsuario?> ObterTipoPorDescricaoAsync(string descricao)
        => await _ctx.TiposUsuario.FirstOrDefaultAsync(t => t.Descricao == descricao);

    public async Task<bool> EmailExisteAsync(string email, int? ignorarId = null)
        => await _set.AnyAsync(u => u.Email == email && (ignorarId == null || u.Id != ignorarId));

    public async Task<bool> PossuiEmprestimosAsync(int id)
        => await _ctx.Emprestimos.AnyAsync(e => e.UsuarioId == id);
}
