using Microsoft.AspNetCore.Authorization;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class ExemplaresController : Controller
{
    private readonly IExemplarRepository _exemplares;
    private readonly ILivroRepository _livros;

    public ExemplaresController(IExemplarRepository exemplares, ILivroRepository livros)
    {
        _exemplares = exemplares;
        _livros = livros;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExemplarFormViewModel vm)
    {
        var voltar = RedirectToAction("Details", "Livros", new { id = vm.LivroId });

        if (await _livros.ObterPorIdAsync(vm.LivroId) == null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Erro"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            return voltar;
        }

        var numero = vm.NumeroExemplar.Trim();
        if (await _exemplares.NumeroExisteAsync(numero))
        {
            TempData["Erro"] = $"Já existe um exemplar com o número {numero}.";
            return voltar;
        }

        await _exemplares.AdicionarAsync(new Exemplar { LivroId = vm.LivroId, NumeroExemplar = numero });
        TempData["Sucesso"] = $"Exemplar {numero} cadastrado com sucesso.";
        return voltar;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var exemplar = await _exemplares.ObterPorIdAsync(id);
        if (exemplar == null) return NotFound();

        var voltar = RedirectToAction("Details", "Livros", new { id = exemplar.LivroId });

        if (await _exemplares.PossuiEmprestimosAsync(id))
        {
            TempData["Erro"] = "Não é possível excluir um exemplar que possui histórico de empréstimos.";
            return voltar;
        }

        await _exemplares.RemoverAsync(exemplar);
        TempData["Sucesso"] = "Exemplar excluído com sucesso.";
        return voltar;
    }
}
