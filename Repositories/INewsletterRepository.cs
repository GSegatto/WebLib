using Biblioteca.Models;

namespace Biblioteca.Repositories;

public interface INewsletterRepository
{
    Task<bool> EmailExisteAsync(string email);
    Task AdicionarAsync(Newsletter newsletter);
    Task<List<Newsletter>> ObterTodosAsync();
    Task RemoverAsync(int id);
}