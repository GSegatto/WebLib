
using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public static class DbSeeder
{
    public const string AdminEmail = "admin@biblioteca.com";
    public const string DemoEmail = "usuario@biblioteca.com";

    public static async Task SeedAsync(
        IServiceProvider sp,
        bool incluirDemo)
    {
        var ctx = sp.GetRequiredService<BibliotecaContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher<Usuario>>();
        var config = sp.GetRequiredService<IConfiguration>();

        var tipoAdmin = await ctx.TiposUsuario
            .FirstAsync(t => t.Descricao == Perfis.Admin);

        var tipoComum = await ctx.TiposUsuario
            .FirstAsync(t => t.Descricao == Perfis.Comum);

        // Cria o administrador caso ainda não exista.
        if (!await ctx.Usuarios.AnyAsync(
            u => u.TipoUsuarioId == tipoAdmin.Id))
        {
            var adminSenha = config["Seed:AdminSenha"];

            if (string.IsNullOrWhiteSpace(adminSenha))
            {
                throw new InvalidOperationException(
                    "Configure a senha inicial do administrador " +
                    "na variável de ambiente Seed__AdminSenha.");
            }

            var admin = new Usuario
            {
                Nome = "Administrador",
                Email = AdminEmail,
                TipoUsuarioId = tipoAdmin.Id
            };

            admin.SenhaHash = hasher.HashPassword(
                admin,
                adminSenha);

            ctx.Usuarios.Add(admin);
            await ctx.SaveChangesAsync();
        }

        // Dados de exemplo somente quando autorizados
        // e quando o acervo estiver vazio.
        if (!incluirDemo || await ctx.Livros.AnyAsync())
            return;

        var demoSenha = config["Seed:DemoSenha"];

        if (string.IsNullOrWhiteSpace(demoSenha))
        {
            throw new InvalidOperationException(
                "Configure a senha de demonstração " +
                "na variável de ambiente Seed__DemoSenha.");
        }

        var demo = new Usuario
        {
            Nome = "Maria Leitora",
            Email = DemoEmail,
            TipoUsuarioId = tipoComum.Id
        };

        demo.SenhaHash = hasher.HashPassword(demo, demoSenha);

        ctx.Usuarios.Add(demo);

        var ficcao = new Categoria { Nome = "Ficção" };
        var tecnico = new Categoria { Nome = "Técnico" };

        var editora = new Editora
        {
            Nome = "Editora Exemplo"
        };

        var machado = new Autor
        {
            Nome = "Machado de Assis"
        };

        var martin = new Autor
        {
            Nome = "Robert C. Martin"
        };

        var l1 = new Livro
        {
            Titulo = "Dom Casmurro",
            Isbn = "978-8535902778",
            AnoPublicacao = 1899,
            Categoria = ficcao,
            Editora = editora,
            Autores = { machado }
        };

        var l2 = new Livro
        {
            Titulo = "Código Limpo",
            Isbn = "978-8576082675",
            AnoPublicacao = 2009,
            Categoria = tecnico,
            Editora = editora,
            Autores = { martin }
        };

        l1.Exemplares.Add(
            new Exemplar { NumeroExemplar = "T-0001" });

        l1.Exemplares.Add(
            new Exemplar { NumeroExemplar = "T-0002" });

        l2.Exemplares.Add(
            new Exemplar { NumeroExemplar = "T-0003" });

        ctx.Livros.AddRange(l1, l2);

        await ctx.SaveChangesAsync();
    }
}