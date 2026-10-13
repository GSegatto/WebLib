using Biblioteca.Models;

namespace Biblioteca.Repositories;

public interface IFavoritoRepository
{
    Task<bool> EhFavoritoAsync(int usuarioId, int livroId);

    Task AdicionarAsync(Favorito favorito);

    Task RemoverAsync(int usuarioId, int livroId);

    Task<List<Livro>> ObterFavoritosAsync(int usuarioId);
}