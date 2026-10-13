using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface IExemplarRepository : IRepository<Exemplar>
{
    Task<List<Exemplar>> ObterDisponiveisAsync();
    Task<bool> NumeroExisteAsync(string numero);
    Task<bool> PossuiEmprestimosAsync(int id);
}

public class ExemplarRepository : Repository<Exemplar>, IExemplarRepository
{
    public ExemplarRepository(BibliotecaContext ctx) : base(ctx) { }

    public override async Task<List<Exemplar>> ObterTodosAsync()
        => await _set
            .Include(x => x.Livro)
            .OrderBy(x => x.Livro.Titulo)
            .ThenBy(x => x.NumeroExemplar)
            .ToListAsync();

    public override async Task<Exemplar?> ObterPorIdAsync(int id)
        => await _set
            .Include(x => x.Livro)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Exemplar>> ObterDisponiveisAsync()
        => await _set
            .Include(x => x.Livro)
            .Where(x =>
                x.Situacao == SituacaoExemplar.Disponivel &&
                !_ctx.Emprestimos.Any(e =>
                    e.ExemplarId == x.Id &&
                    (e.Status == StatusEmprestimo.Pendente ||
                     e.Status == StatusEmprestimo.AguardandoRetirada ||
                     e.Status == StatusEmprestimo.EmAndamento)))
            .OrderBy(x => x.Livro.Titulo)
            .ThenBy(x => x.NumeroExemplar)
            .ToListAsync();

    public async Task<bool> NumeroExisteAsync(string numero)
        => await _set.AnyAsync(x => x.NumeroExemplar == numero);

    public async Task<bool> PossuiEmprestimosAsync(int id)
        => await _ctx.Emprestimos.AnyAsync(e => e.ExemplarId == id);
}