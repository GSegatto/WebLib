using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories;

public class NewsletterRepository : INewsletterRepository
{
    private readonly BibliotecaContext _context;

    public NewsletterRepository(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Newsletters
            .AnyAsync(n => n.Email == email);
    }

    public async Task AdicionarAsync(Newsletter newsletter)
    {
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Newsletter>> ObterTodosAsync()
    {
        return await _context.Newsletters
            .OrderByDescending(n => n.DataInscricao)
            .ToListAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var newsletter = await _context.Newsletters.FindAsync(id);

        if (newsletter != null)
        {
            _context.Newsletters.Remove(newsletter);
            await _context.SaveChangesAsync();
        }
    }
}