using Biblioteca.Extensions;
using Biblioteca.Repositories;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class NewsletterController : Controller
{
    private readonly INewsletterRepository _newsletter;

    public NewsletterController(INewsletterRepository newsletter)
    {
        _newsletter = newsletter;
    }

    public async Task<IActionResult> Index()
    {
        var inscritos = await _newsletter.ObterTodosAsync();

        return View(inscritos);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _newsletter.RemoverAsync(id);

        TempData["Sucesso"] = "Inscrição removida com sucesso.";

        return RedirectToAction(nameof(Index));
    }
}