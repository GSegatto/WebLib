using Biblioteca.Extensions;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class HomeController : Controller
{
    private readonly ILivroRepository _livros;
    private readonly IExemplarRepository _exemplares;
    private readonly IEmprestimoRepository _emprestimos;
    private readonly IUsuarioRepository _usuarios;
    private readonly INewsletterRepository _newsletter;
    private readonly ICategoriaRepository _categorias;

    public HomeController(
    ILivroRepository livros,
    IExemplarRepository exemplares,
    IEmprestimoRepository emprestimos,
    IUsuarioRepository usuarios,
    INewsletterRepository newsletter,
    ICategoriaRepository categorias)
    {
        _livros = livros;
        _exemplares = exemplares;
        _emprestimos = emprestimos;
        _usuarios = usuarios;
        _newsletter = newsletter;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel { Autenticado = User.Identity?.IsAuthenticated == true, Admin = User.EhAdmin() };

        var categoriasPrincipais = await _categorias.ObterPrincipaisAsync();

        vm.CategoriasPrincipais = categoriasPrincipais
        .Select(c => new CategoriaHomeViewModel
        {
            Id = c.Id,
            Nome = c.Nome,
            Imagem = c.Imagem
        })
            .ToList();

        var livrosDestaque = await _livros.ObterDestaquesAsync();

        vm.LivrosDestaque = livrosDestaque.Select(l => new LivroListaViewModel
        {
            Id = l.Id,
            Titulo = l.Titulo,
            Isbn = l.Isbn,
            AnoPublicacao = l.AnoPublicacao,
            Capa = l.Capa,
            Categoria = l.Categoria.Nome,
            Editora = l.Editora.Nome,
            Autores = string.Join(", ", l.Autores.Select(a => a.Nome).OrderBy(n => n)),
            TotalExemplares = l.Exemplares.Count,
            ExemplaresDisponiveis = l.Exemplares.Count(e => e.Situacao == SituacaoExemplar.Disponivel)
        }).ToList();

        if (vm.Admin)
        {
            var abertos = await _emprestimos.ObterEmAbertoAsync();
            vm.TotalLivros = (await _livros.ObterTodosAsync()).Count;
            vm.ExemplaresDisponiveis = (await _exemplares.ObterDisponiveisAsync()).Count;
            vm.EmprestimosAbertos = abertos.Count;
            vm.Atrasados = abertos.Count(e => e.Atrasado);
            vm.TotalUsuarios = (await _usuarios.ObterTodosAsync()).Count;
        }
        else if (vm.Autenticado)
        {
            var meus = await _emprestimos.ObterEmAbertoPorUsuarioAsync(User.UsuarioId());
            vm.MeusAbertos = meus.Count;
            vm.MeusAtrasados = meus.Count(e => e.Atrasado);
        }

        return View(vm);
    }

    public IActionResult Sobre()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Newsletter(Newsletter newsletter)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        if (await _newsletter.EmailExisteAsync(newsletter.Email))
        {
            TempData["NewsletterErro"] = "Este e-mail já está inscrito.";
            return RedirectToAction("Index");
        }

        await _newsletter.AdicionarAsync(newsletter);

        TempData["NewsletterSucesso"] = "E-mail cadastrado com sucesso!";

        return RedirectToAction("Index");
    }

    public IActionResult Erro() => View();
}
