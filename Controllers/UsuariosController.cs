using Biblioteca.Extensions;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.Services;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class UsuariosController : Controller
{
    private readonly IUsuarioRepository _repo;
    private readonly IAutenticacaoService _auth;

    public UsuariosController(IUsuarioRepository repo, IAutenticacaoService auth)
    {
        _repo = repo;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _repo.ObterTodosAsync();
        return View(usuarios.Select(u => new UsuarioListaViewModel
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Tipo = u.TipoUsuario.Descricao
        }).ToList());
    }

    public async Task<IActionResult> Create()
    {
        var vm = new UsuarioFormViewModel();
        await PreencherTiposAsync(vm);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormViewModel vm)
    {
        await ValidarAsync(vm, senhaObrigatoria: true);
        if (!ModelState.IsValid)
        {
            await PreencherTiposAsync(vm);
            return View("Form", vm);
        }

        var u = new Usuario
        {
            Nome = vm.Nome.Trim(),
            Email = vm.Email.Trim().ToLowerInvariant(),
            TipoUsuarioId = vm.TipoUsuarioId!.Value
        };
        u.SenhaHash = _auth.GerarHash(u, vm.Senha!);
        await _repo.AdicionarAsync(u);

        TempData["Sucesso"] = "Usuário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var u = await _repo.ObterPorIdAsync(id);
        if (u == null) return NotFound();

        var vm = new UsuarioFormViewModel { Id = u.Id, Nome = u.Nome, Email = u.Email, TipoUsuarioId = u.TipoUsuarioId };
        await PreencherTiposAsync(vm);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        await ValidarAsync(vm, senhaObrigatoria: false);
        if (!ModelState.IsValid)
        {
            await PreencherTiposAsync(vm);
            return View("Form", vm);
        }

        var u = await _repo.ObterPorIdAsync(id);
        if (u == null) return NotFound();

        u.Nome = vm.Nome.Trim();
        u.Email = vm.Email.Trim().ToLowerInvariant();
        u.TipoUsuarioId = vm.TipoUsuarioId!.Value;
        if (!string.IsNullOrWhiteSpace(vm.Senha)) // em branco = mantém a senha atual
            u.SenhaHash = _auth.GerarHash(u, vm.Senha);
        await _repo.AtualizarAsync(u);

        TempData["Sucesso"] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _repo.ObterPorIdAsync(id);
        if (u == null) return NotFound();

        if (u.Id == User.UsuarioId())
        {
            TempData["Erro"] = "Você não pode excluir a sua própria conta.";
            return RedirectToAction(nameof(Index));
        }

        if (await _repo.PossuiEmprestimosAsync(id))
        {
            TempData["Erro"] = $"Não é possível excluir {u.Nome}: o usuário possui empréstimos registrados.";
            return RedirectToAction(nameof(Index));
        }

        await _repo.RemoverAsync(u);
        TempData["Sucesso"] = "Usuário excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(UsuarioFormViewModel vm, bool senhaObrigatoria)
    {
        if (!string.IsNullOrWhiteSpace(vm.Email) &&
            await _repo.EmailExisteAsync(vm.Email.Trim().ToLowerInvariant(), vm.Id == 0 ? null : vm.Id))
            ModelState.AddModelError(nameof(vm.Email), "Já existe um usuário com este e-mail.");

        if (senhaObrigatoria && string.IsNullOrWhiteSpace(vm.Senha))
            ModelState.AddModelError(nameof(vm.Senha), "Informe a senha.");
    }

    private async Task PreencherTiposAsync(UsuarioFormViewModel vm)
        => vm.Tipos = (await _repo.ObterTiposAsync()).Select(t => new SelectListItem(t.Descricao, t.Id.ToString()));
}
