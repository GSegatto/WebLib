using Biblioteca.Models;
using Biblioteca.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services;

public class EmprestimoService : IEmprestimoService
{
    public const int LimiteEmprestimosSimultaneos = 3;
    private const int PrazoPadraoDias = 14;
    private const string IndiceEmprestimoAberto = "UX_Emprestimo_Exemplar_EmAberto";

    private readonly IEmprestimoRepository _emprestimos;
    private readonly IExemplarRepository _exemplares;
    private readonly IUsuarioRepository _usuarios;

    public EmprestimoService(
        IEmprestimoRepository emprestimos,
        IExemplarRepository exemplares,
        IUsuarioRepository usuarios)
    {
        _emprestimos = emprestimos;
        _exemplares = exemplares;
        _usuarios = usuarios;
    }

    // 1. Usuário solicita um exemplar pelo site.
    public async Task<Emprestimo> SolicitarAsync(int usuarioId, int exemplarId)
    {
        var usuario = await _usuarios.ObterPorIdAsync(usuarioId)
            ?? throw new RegraNegocioException("Usuário não encontrado.");

        var exemplar = await _exemplares.ObterPorIdAsync(exemplarId)
            ?? throw new RegraNegocioException("Exemplar não encontrado.");

        if (exemplar.Situacao != SituacaoExemplar.Disponivel)
            throw new RegraNegocioException("Este exemplar não está disponível para solicitação.");

        var existente = await _emprestimos.ObterAbertoPorExemplarAsync(exemplarId);

        if (existente != null)
            throw new RegraNegocioException("Este exemplar já possui uma solicitação, reserva ou empréstimo ativo.");

        var abertosDoUsuario = await _emprestimos.ObterEmAbertoPorUsuarioAsync(usuarioId);

        if (abertosDoUsuario.Any(e => e.Atrasado))
            throw new RegraNegocioException(
                "Você possui empréstimo em atraso. Devolva o livro antes de solicitar outro.");

        if (abertosDoUsuario.Count >= LimiteEmprestimosSimultaneos)
            throw new RegraNegocioException(
                $"Você já atingiu o limite de {LimiteEmprestimosSimultaneos} empréstimos em andamento.");

        var emprestimo = new Emprestimo
        {
            UsuarioId = usuario.Id,
            ExemplarId = exemplar.Id,
            Status = StatusEmprestimo.Pendente,
            DataRetirada = null,
            DataPrevistaDevolucao = null,
            DataDevolucao = null
        };

        // Reserva o exemplar para impedir solicitações duplicadas.
        exemplar.Situacao = SituacaoExemplar.Reservado;

        try
        {
            await _emprestimos.AdicionarAsync(emprestimo);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains(IndiceEmprestimoAberto) == true)
        {
            throw new RegraNegocioException(
                "Este exemplar acabou de ser solicitado por outra pessoa. Atualize a página.");
        }

        return emprestimo;
    }

    // 2. Administrador aprova a solicitação.
    public async Task AprovarSolicitacaoAsync(int emprestimoId)
    {
        var emprestimo = await _emprestimos.ObterPorIdAsync(emprestimoId)
            ?? throw new RegraNegocioException("Solicitação não encontrada.");

        if (emprestimo.Status != StatusEmprestimo.Pendente)
            throw new RegraNegocioException("Esta solicitação não está pendente.");

        emprestimo.Status = StatusEmprestimo.AguardandoRetirada;
        emprestimo.Exemplar.Situacao = SituacaoExemplar.Reservado;

        await _emprestimos.AtualizarAsync(emprestimo);
    }

    // 3. Administrador registra a retirada física.
    public async Task RegistrarRetiradaFisicaAsync(int emprestimoId)
    {
        var emprestimo = await _emprestimos.ObterPorIdAsync(emprestimoId)
            ?? throw new RegraNegocioException("Solicitação não encontrada.");

        if (emprestimo.Status != StatusEmprestimo.AguardandoRetirada)
            throw new RegraNegocioException(
                "Somente solicitações aprovadas podem ter a retirada registrada.");

        var abertosDoUsuario =
            await _emprestimos.ObterEmAbertoPorUsuarioAsync(emprestimo.UsuarioId);

        if (abertosDoUsuario.Any(e => e.Atrasado))
            throw new RegraNegocioException(
                "Este usuário possui empréstimo em atraso e não pode retirar outro livro.");

        if (abertosDoUsuario.Count >= LimiteEmprestimosSimultaneos)
            throw new RegraNegocioException(
                $"O usuário já atingiu o limite de {LimiteEmprestimosSimultaneos} empréstimos em andamento.");

        var hoje = DateTime.Today;

        emprestimo.DataRetirada = hoje;
        emprestimo.DataPrevistaDevolucao = hoje.AddDays(PrazoPadraoDias);
        emprestimo.Status = StatusEmprestimo.EmAndamento;
        emprestimo.Exemplar.Situacao = SituacaoExemplar.Emprestado;

        await _emprestimos.AtualizarAsync(emprestimo);
    }

    // 4. Administrador recusa a solicitação.
    public async Task RecusarSolicitacaoAsync(int emprestimoId)
    {
        var emprestimo = await _emprestimos.ObterPorIdAsync(emprestimoId)
            ?? throw new RegraNegocioException("Solicitação não encontrada.");

        if (emprestimo.Status != StatusEmprestimo.Pendente)
            throw new RegraNegocioException(
                "Somente solicitações pendentes podem ser recusadas.");

        emprestimo.Status = StatusEmprestimo.Recusado;
        emprestimo.Exemplar.Situacao = SituacaoExemplar.Disponivel;

        await _emprestimos.AtualizarAsync(emprestimo);
    }

    // 5. Usuário cancela a própria solicitação.
    public async Task CancelarSolicitacaoAsync(int emprestimoId, int usuarioId)
    {
        var emprestimo = await _emprestimos.ObterPorIdAsync(emprestimoId)
            ?? throw new RegraNegocioException("Solicitação não encontrada.");

        if (emprestimo.UsuarioId != usuarioId)
            throw new RegraNegocioException(
                "Você não tem permissão para cancelar esta solicitação.");

        if (emprestimo.Status != StatusEmprestimo.Pendente &&
            emprestimo.Status != StatusEmprestimo.AguardandoRetirada)
        {
            throw new RegraNegocioException(
                "Esta solicitação não pode mais ser cancelada.");
        }

        emprestimo.Status = StatusEmprestimo.Cancelado;
        emprestimo.Exemplar.Situacao = SituacaoExemplar.Disponivel;

        await _emprestimos.AtualizarAsync(emprestimo);
    }

    // 6. Administrador registra a devolução física.
    public async Task RegistrarDevolucaoAsync(int emprestimoId)
    {
        var emprestimo = await _emprestimos.ObterPorIdAsync(emprestimoId)
            ?? throw new RegraNegocioException("Empréstimo não encontrado.");

        if (emprestimo.Status != StatusEmprestimo.EmAndamento)
            throw new RegraNegocioException(
                "Somente empréstimos em andamento podem ser devolvidos.");

        emprestimo.DataDevolucao = DateTime.Today;
        emprestimo.Status = StatusEmprestimo.Concluido;
        emprestimo.Exemplar.Situacao = SituacaoExemplar.Disponivel;

        await _emprestimos.AtualizarAsync(emprestimo);
    }
}