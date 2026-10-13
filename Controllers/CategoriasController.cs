using Microsoft.AspNetCore.Authorization;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

[Authorize(Roles = Perfis.Admin)]
public class CategoriasController : Controller
{
    private readonly ICategoriaRepository _repo;
    private readonly IWebHostEnvironment _env;

    private const int LimitePrincipais = 6;
    private const long TamanhoMaximoImagem = 5 * 1024 * 1024;

    private static readonly string[] ExtensoesPermitidas =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public CategoriasController(
        ICategoriaRepository repo,
        IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var itens = await _repo.ObterTodosAsync();

        var vm = itens.Select(x => new CategoriaListaViewModel
        {
            Id = x.Id,
            Nome = x.Nome,
            EhPrincipal = x.EhPrincipal
        }).ToList();

        return View(vm);
    }

    public IActionResult Create()
        => View("Form", new CategoriaFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View("Form", vm);

        var imagem = await SalvarImagemAsync(vm.ArquivoImagem);

        if (imagem == null && vm.ArquivoImagem != null)
        {
            ModelState.AddModelError(
                nameof(vm.ArquivoImagem),
                "A imagem é inválida ou ultrapassa o limite de 5 MB.");

            return View("Form", vm);
        }

        await _repo.AdicionarAsync(new Categoria
        {
            Nome = vm.Nome.Trim(),
            Imagem = imagem,
            EhPrincipal = false
        });

        TempData["Sucesso"] = "Categoria cadastrada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);

        if (item == null)
            return NotFound();

        return View("Form", new CategoriaFormViewModel
        {
            Id = item.Id,
            Nome = item.Nome,
            ImagemAtual = item.Imagem
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CategoriaFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        var item = await _repo.ObterPorIdAsync(id);

        if (item == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            vm.ImagemAtual = item.Imagem;
            return View("Form", vm);
        }

        string? novaImagem = null;

        if (vm.ArquivoImagem != null)
        {
            novaImagem = await SalvarImagemAsync(vm.ArquivoImagem);

            if (novaImagem == null)
            {
                ModelState.AddModelError(
                    nameof(vm.ArquivoImagem),
                    "A imagem é inválida ou ultrapassa o limite de 5 MB.");

                vm.ImagemAtual = item.Imagem;
                return View("Form", vm);
            }
        }

        var imagemAntiga = item.Imagem;

        item.Nome = vm.Nome.Trim();

        if (novaImagem != null)
            item.Imagem = novaImagem;

        await _repo.AtualizarAsync(item);

        if (novaImagem != null)
            ExcluirArquivoImagem(imagemAntiga);

        TempData["Sucesso"] = "Categoria atualizada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarPrincipal(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);

        if (item == null)
            return NotFound();

        if (!item.EhPrincipal)
        {
            var categorias = await _repo.ObterTodosAsync();

            if (categorias.Count(c => c.EhPrincipal) >= LimitePrincipais)
            {
                TempData["Erro"] =
                    "Você já selecionou as seis categorias principais. " +
                    "Desmarque uma delas antes de selecionar outra.";

                return RedirectToAction(nameof(Index));
            }

            item.EhPrincipal = true;
        }
        else
        {
            item.EhPrincipal = false;
        }

        await _repo.AtualizarAsync(item);

        TempData["Sucesso"] = item.EhPrincipal
            ? "Categoria adicionada às principais."
            : "Categoria removida das principais.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);

        if (item == null)
            return NotFound();

        if (await _repo.PossuiLivrosAsync(id))
        {
            TempData["Erro"] =
                $"Não é possível excluir a categoria \"{item.Nome}\" " +
                "porque existem livros vinculados.";

            return RedirectToAction(nameof(Index));
        }

        var imagem = item.Imagem;

        await _repo.RemoverAsync(item);
        ExcluirArquivoImagem(imagem);

        TempData["Sucesso"] = "Categoria excluída com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SalvarImagemAsync(
        IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return null;

        if (arquivo.Length > TamanhoMaximoImagem)
            return null;

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        if (!ExtensoesPermitidas.Contains(extensao))
            return null;

        var pasta = Path.Combine(
            _env.WebRootPath,
            "images",
            "categorias");

        Directory.CreateDirectory(pasta);

        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using (var stream = new FileStream(caminho, FileMode.CreateNew))
        {
            await arquivo.CopyToAsync(stream);
        }

        return $"/images/categorias/{nomeArquivo}";
    }

    private void ExcluirArquivoImagem(string? caminhoRelativo)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
            return;

        const string prefixo = "/images/categorias/";

        if (!caminhoRelativo.StartsWith(
                prefixo, StringComparison.OrdinalIgnoreCase))
            return;

        var nomeArquivo = Path.GetFileName(caminhoRelativo);

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return;

        var caminho = Path.Combine(
            _env.WebRootPath,
            "images",
            "categorias",
            nomeArquivo);

        if (System.IO.File.Exists(caminho))
            System.IO.File.Delete(caminho);
    }
}