using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly BibliotecaContext _context;

    public FavoritoRepository(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<bool> EhFavoritoAsync(int usuarioId, int livroId)
    {
        return await _context.Favoritos
            .AnyAsync(f => f.UsuarioId == usuarioId && f.LivroId == livroId);
    }

    public async Task AdicionarAsync(Favorito favorito)
    {
        _context.Favoritos.Add(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int usuarioId, int livroId)
    {
        var favorito = await _context.Favoritos
            .FirstOrDefaultAsync(f =>
                f.UsuarioId == usuarioId &&
                f.LivroId == livroId);

        if (favorito == null)
            return;

        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Livro>> ObterFavoritosAsync(int usuarioId)
    {
        return await _context.Favoritos
            .Where(f => f.UsuarioId == usuarioId)

            .Include(f => f.Livro)
                .ThenInclude(l => l.Categoria)

            .Include(f => f.Livro)
                .ThenInclude(l => l.Editora)

            .Include(f => f.Livro)
                .ThenInclude(l => l.Autores)

            .Include(f => f.Livro)
                .ThenInclude(l => l.Exemplares)

            .Select(f => f.Livro)
            .OrderBy(l => l.Titulo)
            .ToListAsync();
    }
}