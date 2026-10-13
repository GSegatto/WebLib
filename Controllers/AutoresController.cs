using Microsoft.AspNetCore.Authorization;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class AutoresController : Controller
{
    private readonly IAutorRepository _repo;

    public AutoresController(IAutorRepository repo) => _repo = repo;

    // GET /Autores
    public async Task<IActionResult> Index()
    {
        var itens = await _repo.ObterTodosAsync();
        return View(itens.Select(x => new NomeViewModel { Id = x.Id, Nome = x.Nome }).ToList());
    }

    // GET /Autores/Create
    public IActionResult Create() => View("Form", new NomeViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NomeViewModel vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        await _repo.AdicionarAsync(new Autor { Nome = vm.Nome.Trim() });
        TempData["Sucesso"] = "Autor cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Autores/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item == null) return NotFound();
        return View("Form", new NomeViewModel { Id = item.Id, Nome = item.Nome });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NomeViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", vm);

        var item = await _repo.ObterPorIdAsync(id);
        if (item == null) return NotFound();

        item.Nome = vm.Nome.Trim();
        await _repo.AtualizarAsync(item);
        TempData["Sucesso"] = "Autor atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item == null) return NotFound();

        if (await _repo.PossuiLivrosAsync(id))
        {
            TempData["Erro"] = "Não é possível excluir o autor \"" + item.Nome + "\" porque existem livros vinculados.";
            return RedirectToAction(nameof(Index));
        }

        await _repo.RemoverAsync(item);
        TempData["Sucesso"] = "Autor excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
