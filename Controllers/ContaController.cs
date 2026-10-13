using System.Security.Claims;
using Biblioteca.Extensions;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.Services;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize]
public class ContaController : Controller
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IEmprestimoRepository _emprestimos;
    private readonly IAutenticacaoService _auth;

    public ContaController(IUsuarioRepository usuarios, IEmprestimoRepository emprestimos, IAutenticacaoService auth)
    {
        _usuarios = usuarios;
        _emprestimos = emprestimos;
        _auth = auth;
    }

    // ---------- Login / Cadastro / Logout (públicos) ----------
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var usuario = await _auth.AutenticarAsync(vm.Email, vm.Senha);
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(vm);
        }

        await EntrarAsync(usuario, vm.Lembrar);
        
        return !string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)
            ? LocalRedirect(vm.ReturnUrl)
            : RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Registrar() => View(new RegistroViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel vm)
    {
        var email = vm.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!string.IsNullOrEmpty(email) && await _usuarios.EmailExisteAsync(email))
            ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
        if (!ModelState.IsValid) return View(vm);

        var tipo = await _usuarios.ObterTipoPorDescricaoAsync(Perfis.Comum);
        var usuario = new Usuario { Nome = vm.Nome.Trim(), Email = email, TipoUsuarioId = tipo!.Id };
        usuario.SenhaHash = _auth.GerarHash(usuario, vm.Senha);
        await _usuarios.AdicionarAsync(usuario);

        usuario.TipoUsuario = tipo;
        await EntrarAsync(usuario, lembrar: false);
        TempData["Sucesso"] = "Conta criada com sucesso!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Sucesso"] = "Você saiu da sua conta.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AcessoNegado() => View();

    // ---------- Área do usuário logado ----------
    public async Task<IActionResult> Perfil()
    {
        var u = await _usuarios.ObterPorIdAsync(User.UsuarioId());
        if (u == null) return await SairPorContaInexistenteAsync();
        return View(new PerfilViewModel { Nome = u.Nome, Email = u.Email, Tipo = u.TipoUsuario.Descricao });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilViewModel vm)
    {
        var u = await _usuarios.ObterPorIdAsync(User.UsuarioId());
        if (u == null) return await SairPorContaInexistenteAsync();
        vm.Tipo = u.TipoUsuario.Descricao;

        var email = vm.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!string.IsNullOrEmpty(email) && await _usuarios.EmailExisteAsync(email, u.Id))
            ModelState.AddModelError(nameof(vm.Email), "Já existe uma conta com este e-mail.");
        if (!ModelState.IsValid) return View(vm);

        u.Nome = vm.Nome.Trim();
        u.Email = email;
        await _usuarios.AtualizarAsync(u);

        await EntrarAsync(u, lembrar: true); // renova o cookie com o nome/e-mail atualizados
        TempData["Sucesso"] = "Perfil atualizado com sucesso.";
        return RedirectToAction(nameof(Perfil));
    }

    public IActionResult Senha() => View(new AlterarSenhaViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Senha(AlterarSenhaViewModel vm)
    {
        var u = await _usuarios.ObterPorIdAsync(User.UsuarioId());
        if (u == null) return await SairPorContaInexistenteAsync();

        if (!string.IsNullOrEmpty(vm.SenhaAtual) && !_auth.VerificarSenha(u, vm.SenhaAtual))
            ModelState.AddModelError(nameof(vm.SenhaAtual), "A senha atual está incorreta.");
        if (!ModelState.IsValid) return View(vm);

        u.SenhaHash = _auth.GerarHash(u, vm.NovaSenha);
        await _usuarios.AtualizarAsync(u);

        TempData["Sucesso"] = "Senha alterada com sucesso.";
        return RedirectToAction(nameof(Senha));
    }

    public async Task<IActionResult> MeusEmprestimos()
    {
        var lista = await _emprestimos.ObterPorUsuarioAsync(User.UsuarioId());

        return View(new MeusEmprestimosViewModel
        {
            Limite = EmprestimoService.LimiteEmprestimosSimultaneos,

            Solicitacoes = lista
                .Where(e =>
                    e.Status == StatusEmprestimo.Pendente ||
                    e.Status == StatusEmprestimo.AguardandoRetirada)
                .Select(e => e.ParaLista())
                .ToList(),

            EmAberto = lista
                .Where(e => e.Status == StatusEmprestimo.EmAndamento)
                .Select(e => e.ParaLista())
                .ToList(),

            Devolvidos = lista
                .Where(e => e.Status == StatusEmprestimo.Concluido)
                .Select(e => e.ParaLista())
                .ToList(),

            Encerrados = lista
                .Where(e =>
                    e.Status == StatusEmprestimo.Recusado ||
                    e.Status == StatusEmprestimo.Cancelado)
                .Select(e => e.ParaLista())
                .ToList()
        });
    }

    public IActionResult Preferencias() => View();

    // ---------- helpers ----------
    private async Task EntrarAsync(Usuario u, bool lembrar)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Nome),
            new(ClaimTypes.Email, u.Email),
            new(ClaimTypes.Role, u.TipoUsuario.Descricao) // "Administrador" ou "Comum"
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = lembrar });
    }

    private async Task<IActionResult> SairPorContaInexistenteAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
