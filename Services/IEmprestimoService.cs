using Biblioteca.Models;

namespace Biblioteca.Services;

public interface IEmprestimoService
{
    // Usuário solicita um exemplar pelo site.
    Task<Emprestimo> SolicitarAsync(int usuarioId, int exemplarId);

    // Administrador aprova a solicitação.
    Task AprovarSolicitacaoAsync(int emprestimoId);

    // Administrador registra a retirada física.
    Task RegistrarRetiradaFisicaAsync(int emprestimoId);

    // Administrador recusa a solicitação.
    Task RecusarSolicitacaoAsync(int emprestimoId);

    // Usuário cancela uma solicitação ainda não retirada.
    Task CancelarSolicitacaoAsync(int emprestimoId, int usuarioId);

    // Administrador registra a devolução física.
    Task RegistrarDevolucaoAsync(int emprestimoId);
}