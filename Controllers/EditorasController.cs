using Microsoft.AspNetCore.Authorization;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class EditorasController : Controller
{
    private readonly IEditoraRepository _repo;

    public EditorasController(IEditoraRepository repo) => _repo = repo;

    // GET /Editoras
    public async Task<IActionResult> Index()
    {
        var itens = await _repo.ObterTodosAsync();
        return View(itens.Select(x => new NomeViewModel { Id = x.Id, Nome = x.Nome }).ToList());
    }

    // GET /Editoras/Create
    public IActionResult Create() => View("Form", new NomeViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NomeViewModel vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);

        await _repo.AdicionarAsync(new Editora { Nome = vm.Nome.Trim() });
        TempData["Sucesso"] = "Editora cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Editoras/Edit/5
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
        TempData["Sucesso"] = "Editora atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item == null) return NotFound();

        if (await _repo.PossuiLivrosAsync(id))
        {
            TempData["Erro"] = "Não é possível excluir a editora \"" + item.Nome + "\" porque existem livros vinculados.";
            return RedirectToAction(nameof(Index));
        }

        await _repo.RemoverAsync(item);
        TempData["Sucesso"] = "Editora excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
