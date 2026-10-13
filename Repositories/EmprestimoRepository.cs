using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public interface IEmprestimoRepository : IRepository<Emprestimo>
{
    Task<List<Emprestimo>> ObterEmAbertoAsync();
    Task<List<Emprestimo>> ObterEmAbertoPorUsuarioAsync(int usuarioId);
    Task<List<Emprestimo>> ObterPorUsuarioAsync(int usuarioId);
    Task<Emprestimo?> ObterAbertoPorExemplarAsync(int exemplarId);

    Task<List<Emprestimo>> ObterSolicitacoesPendentesAsync();
    Task<List<Emprestimo>> ObterAguardandoRetiradaAsync();
}

public class EmprestimoRepository : Repository<Emprestimo>, IEmprestimoRepository
{
    public EmprestimoRepository(BibliotecaContext ctx) : base(ctx) { }

    private IQueryable<Emprestimo> ComDetalhes() => _set
        .Include(e => e.Usuario)
        .Include(e => e.Exemplar)
            .ThenInclude(x => x.Livro);

    public override async Task<List<Emprestimo>> ObterTodosAsync()
        => await ComDetalhes()
            .OrderByDescending(e => e.Id)
            .ToListAsync();

    public override async Task<Emprestimo?> ObterPorIdAsync(int id)
        => await ComDetalhes()
            .FirstOrDefaultAsync(e => e.Id == id);

    // Empréstimos cuja retirada física já foi registrada.
    public async Task<List<Emprestimo>> ObterEmAbertoAsync()
        => await ComDetalhes()
            .Where(e => e.Status == StatusEmprestimo.EmAndamento)
            .OrderBy(e => e.DataPrevistaDevolucao)
            .ToListAsync();

    // Empréstimos efetivamente ativos do usuário.
    public async Task<List<Emprestimo>> ObterEmAbertoPorUsuarioAsync(int usuarioId)
        => await ComDetalhes()
            .Where(e => e.UsuarioId == usuarioId
                     && e.Status == StatusEmprestimo.EmAndamento)
            .ToListAsync();

    public async Task<List<Emprestimo>> ObterPorUsuarioAsync(int usuarioId)
        => await ComDetalhes()
            .Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Id)
            .ToListAsync();

    // Verifica se o exemplar possui solicitação, reserva ou empréstimo ativo.
    public async Task<Emprestimo?> ObterAbertoPorExemplarAsync(int exemplarId)
        => await ComDetalhes()
            .FirstOrDefaultAsync(e =>
                e.ExemplarId == exemplarId &&
                (e.Status == StatusEmprestimo.Pendente
                 || e.Status == StatusEmprestimo.AguardandoRetirada
                 || e.Status == StatusEmprestimo.EmAndamento));

    public async Task<List<Emprestimo>> ObterSolicitacoesPendentesAsync()
        => await ComDetalhes()
            .Where(e => e.Status == StatusEmprestimo.Pendente)
            .OrderBy(e => e.Id)
            .ToListAsync();

    public async Task<List<Emprestimo>> ObterAguardandoRetiradaAsync()
        => await ComDetalhes()
            .Where(e => e.Status == StatusEmprestimo.AguardandoRetirada)
            .OrderBy(e => e.Id)
            .ToListAsync();
}