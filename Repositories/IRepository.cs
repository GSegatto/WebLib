namespace Biblioteca.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> ObterTodosAsync();
    Task<T?> ObterPorIdAsync(int id);
    Task AdicionarAsync(T entidade);
    /// <summary>Persiste alterações feitas em uma entidade já carregada (rastreada) pelo repositório.</summary>
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(T entidade);
}
