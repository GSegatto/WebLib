using Biblioteca.Extensions;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.Services;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Controllers;

[Authorize]
public class EmprestimosController : Controller
{
    private const int PrazoPadraoDias = 14;

    private readonly IEmprestimoService _service;
    private readonly IEmprestimoRepository _emprestimos;
    private readonly IExemplarRepository _exemplares;
    private readonly IUsuarioRepository _usuarios;

    public EmprestimosController(
        IEmprestimoService service,
        IEmprestimoRepository emprestimos,
        IExemplarRepository exemplares,
        IUsuarioRepository usuarios)
    {
        _service = service;
        _emprestimos = emprestimos;
        _exemplares = exemplares;
        _usuarios = usuarios;
    }

    // Solicitação feita pelo usuário logado
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitar(int exemplarId, int livroId)
    {
        try
        {
            await _service.SolicitarAsync(User.UsuarioId(), exemplarId);

            TempData["Sucesso"] =
                "Solicitação enviada! Aguarde a aprovação da biblioteca.";

            return RedirectToAction("MeusEmprestimos", "Conta");
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;

            return RedirectToAction(
                "Details",
                "Livros",
                new { id = livroId });
        }
    }

    // Painel administrativo
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Index()
    {
        var disponiveis = await _exemplares.ObterDisponiveisAsync();
        var pendentes = await _emprestimos.ObterSolicitacoesPendentesAsync();
        var aguardando = await _emprestimos.ObterAguardandoRetiradaAsync();
        var emAndamento = await _emprestimos.ObterEmAbertoAsync();

        return View(new AcervoViewModel
        {
            Disponiveis = disponiveis.Select(e => new ExemplarItemViewModel
            {
                Id = e.Id,
                LivroId = e.Livro.Id,
                Titulo = e.Livro.Titulo,
                Isbn = e.Livro.Isbn,
                Capa = e.Livro.Capa,
                NumeroExemplar = e.NumeroExemplar,
                Situacao = e.SituacaoTexto,
                Disponivel = true
            }).ToList(),

            SolicitacoesPendentes = pendentes
                .Select(e => e.ParaLista())
                .ToList(),

            AguardandoRetirada = aguardando
                .Select(e => e.ParaLista())
                .ToList(),

            Emprestados = emAndamento
                .Select(e => e.ParaLista())
                .ToList()
        });
    }

    // Aprovar solicitação
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Aprovar(int id)
    {
        try
        {
            await _service.AprovarSolicitacaoAsync(id);

            TempData["Sucesso"] =
                "Solicitação aprovada. O livro está reservado e aguarda retirada.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Recusar solicitação
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Recusar(int id)
    {
        try
        {
            await _service.RecusarSolicitacaoAsync(id);

            TempData["Sucesso"] =
                "Solicitação recusada. O exemplar voltou a ficar disponível.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Registrar a retirada física do livro
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> RegistrarRetirada(int id)
    {
        try
        {
            await _service.RegistrarRetiradaFisicaAsync(id);

            TempData["Sucesso"] =
                $"Retirada registrada. O prazo de {PrazoPadraoDias} dias começou a contar.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Histórico completo
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Historico()
    {
        var lista = await _emprestimos.ObterTodosAsync();

        return View(lista.Select(e => e.ParaLista()).ToList());
    }

    // Formulário administrativo para criar e aprovar uma solicitação
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Novo(int? exemplarId)
    {
        var vm = new EmprestimoFormViewModel
        {
            ExemplarId = exemplarId,
            DataRetirada = DateTime.Today,
            DataPrevistaDevolucao = DateTime.Today.AddDays(PrazoPadraoDias)
        };

        await PreencherListasAsync(vm);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Novo(EmprestimoFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var emprestimo = await _service.SolicitarAsync(
                    vm.UsuarioId!.Value,
                    vm.ExemplarId!.Value);

                await _service.AprovarSolicitacaoAsync(emprestimo.Id);

                TempData["Sucesso"] =
                    "Solicitação registrada e aprovada. Registre a retirada quando o usuário buscar o livro.";

                return RedirectToAction(nameof(Index));
            }
            catch (RegraNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await PreencherListasAsync(vm);

        return View(vm);
    }

    // Registrar a devolução
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Devolver(int id)
    {
        try
        {
            await _service.RegistrarDevolucaoAsync(id);

            TempData["Sucesso"] =
                "Devolução registrada. O exemplar está disponível para novo empréstimo.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PreencherListasAsync(EmprestimoFormViewModel vm)
    {
        vm.Usuarios = (await _usuarios.ObterTodosAsync())
            .Select(u => new SelectListItem(u.Nome, u.Id.ToString()));

        vm.Exemplares = (await _exemplares.ObterTodosAsync())
            .Select(x => new SelectListItem(
                $"{x.Livro.Titulo} — exemplar {x.NumeroExemplar}" +
                (x.Situacao == SituacaoExemplar.Disponivel
                    ? ""
                    : $" [{x.SituacaoTexto}]"),
                x.Id.ToString()));
    }
}