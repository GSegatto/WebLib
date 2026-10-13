using Microsoft.AspNetCore.Authorization;
using Biblioteca.Models;
using Biblioteca.Repositories;
using Biblioteca.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Controllers;

[Authorize]
public class LivrosController : Controller
{
    private readonly ILivroRepository _livros;
    private readonly ICategoriaRepository _categorias;
    private readonly IEditoraRepository _editoras;
    private readonly IAutorRepository _autores;
    private readonly IWebHostEnvironment _env;
    private readonly IFavoritoRepository _favoritos;

    public LivrosController(
        ILivroRepository livros,
        ICategoriaRepository categorias,
        IEditoraRepository editoras,
        IAutorRepository autores,
        IWebHostEnvironment env,
        IFavoritoRepository favoritos)
    {
        _livros = livros;
        _categorias = categorias;
        _editoras = editoras;
        _autores = autores;
        _env = env;
        _favoritos = favoritos;
    }

    // =========================
    // CATÁLOGO
    // =========================

    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? q,
        int? categoriaId)
    {
        var livros = await _livros.BuscarAsync(q, categoriaId);
        var categorias = await _categorias.ObterTodosAsync();

        var favoritos = new HashSet<int>();

        if (User.Identity?.IsAuthenticated == true)
        {
            var usuarioId = int.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var livrosFavoritos =
                await _favoritos.ObterFavoritosAsync(usuarioId);

            favoritos = livrosFavoritos
                .Select(l => l.Id)
                .ToHashSet();
        }

        return View(new LivrosIndexViewModel
        {
            Q = q,

            CategoriaId = categoriaId,

            Categorias = categorias.Select(
                c => new SelectListItem(
                    c.Nome,
                    c.Id.ToString())),

            Livros = livros.Select(l => new LivroListaViewModel
            {
                Id = l.Id,

                Titulo = l.Titulo,

                Isbn = l.Isbn,

                AnoPublicacao = l.AnoPublicacao,

                Capa = l.Capa,

                Favoritado = favoritos.Contains(l.Id),

                Categoria = l.Categoria.Nome,

                Editora = l.Editora.Nome,

                Autores = string.Join(
                    ", ",
                    l.Autores
                        .Select(a => a.Nome)
                        .OrderBy(n => n)),

                TotalExemplares =
                    l.Exemplares.Count,

                ExemplaresDisponiveis =
                    l.Exemplares.Count(
                        e => e.Situacao ==
                             SituacaoExemplar.Disponivel)

            }).ToList()
        });
    }

    // =========================
    // DETALHES
    // =========================

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var l = await _livros.ObterPorIdAsync(id);

        if (l == null)
            return NotFound();

        var favoritado = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var usuarioId = int.Parse(
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            favoritado = await _favoritos.EhFavoritoAsync(
                usuarioId,
                id);
        }

        return View(new LivroDetalhesViewModel
        {
            Id = l.Id,

            Titulo = l.Titulo,

            Descricao = l.Descricao,

            Isbn = l.Isbn,

            AnoPublicacao = l.AnoPublicacao,

            Capa = l.Capa,

            Favoritado = favoritado,

            Categoria = l.Categoria.Nome,

            Editora = l.Editora.Nome,

            Autores = string.Join(
                ", ",
                l.Autores
                    .Select(a => a.Nome)
                    .OrderBy(n => n)),

            Exemplares = l.Exemplares
                .OrderBy(e => e.NumeroExemplar)
                .Select(e => new ExemplarItemViewModel
                {
                    Id = e.Id,

                    NumeroExemplar =
                        e.NumeroExemplar,

                    Situacao =
                        e.SituacaoTexto,

                    Disponivel =
                        e.Situacao ==
                        SituacaoExemplar.Disponivel,

                    Titulo = l.Titulo

                })
                .ToList()
        });
    }

    // =========================
    // CADASTRO DE LIVRO
    // =========================

    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Create()
    {
        var vm = new LivroFormViewModel();

        await PreencherListasAsync(vm);

        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Create(
        LivroFormViewModel vm)
    {
        await ValidarRegrasAsync(vm);

        if (!ModelState.IsValid)
        {
            await PreencherListasAsync(vm);

            return View("Form", vm);
        }

        var caminhoCapa =
            await SalvarCapaAsync(vm.Capa);

        var livro = new Livro
        {
            Titulo = vm.Titulo.Trim(),

            Descricao = vm.Descricao,

            Isbn = vm.Isbn.Trim(),

            AnoPublicacao =
                vm.AnoPublicacao!.Value,

            Destaque = vm.Destaque,

            Capa = caminhoCapa,

            CategoriaId =
                vm.CategoriaId!.Value,

            EditoraId =
                vm.EditoraId!.Value,

            Autores =
                await _autores.ObterPorIdsAsync(
                    vm.AutoresIds)
        };

        await _livros.AdicionarAsync(livro);

        TempData["Sucesso"] =
            "Livro cadastrado com sucesso. Agora cadastre os exemplares físicos abaixo.";

        return RedirectToAction(
            nameof(Details),
            new { id = livro.Id });
    }

    // =========================
    // EDIÇÃO DE LIVRO
    // =========================

    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var l = await _livros.ObterPorIdAsync(id);

        if (l == null)
            return NotFound();

        var vm = new LivroFormViewModel
        {
            Id = l.Id,

            Titulo = l.Titulo,

            Descricao = l.Descricao,

            Isbn = l.Isbn,

            AnoPublicacao =
                l.AnoPublicacao,

            Destaque =
                l.Destaque,

            CapaAtual =
                l.Capa,

            CategoriaId =
                l.CategoriaId,

            EditoraId =
                l.EditoraId,

            AutoresIds =
                l.Autores
                    .Select(a => a.Id)
                    .ToList()
        };

        await PreencherListasAsync(vm);

        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Edit(
        int id,
        LivroFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        await ValidarRegrasAsync(vm);

        if (!ModelState.IsValid)
        {
            await PreencherListasAsync(vm);

            return View("Form", vm);
        }

        var livro =
            await _livros.ObterPorIdAsync(id);

        if (livro == null)
            return NotFound();

        if (vm.Capa != null &&
            vm.Capa.Length > 0)
        {
            var novaCapa =
                await SalvarCapaAsync(vm.Capa);

            ExcluirCapa(livro.Capa);

            livro.Capa = novaCapa;
        }

        livro.Titulo =
            vm.Titulo.Trim();

        livro.Descricao =
            vm.Descricao;

        livro.Isbn =
            vm.Isbn.Trim();

        livro.AnoPublicacao =
            vm.AnoPublicacao!.Value;

        livro.Destaque =
            vm.Destaque;

        livro.CategoriaId =
            vm.CategoriaId!.Value;

        livro.EditoraId =
            vm.EditoraId!.Value;

        livro.Autores.Clear();

        foreach (
            var autor in
            await _autores.ObterPorIdsAsync(
                vm.AutoresIds))
        {
            livro.Autores.Add(autor);
        }

        await _livros.AtualizarAsync(livro);

        TempData["Sucesso"] =
            "Livro atualizado com sucesso.";

        return RedirectToAction(
            nameof(Index));
    }

    // =========================
    // EXCLUSÃO DE LIVRO
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var livro =
            await _livros.ObterPorIdAsync(id);

        if (livro == null)
            return NotFound();

        if (livro.Exemplares.Any())
        {
            TempData["Erro"] =
                $"Não é possível excluir \"{livro.Titulo}\": remova antes os exemplares cadastrados.";

            return RedirectToAction(
                nameof(Index));
        }

        var capa =
            livro.Capa;

        livro.Autores.Clear();

        await _livros.RemoverAsync(livro);

        ExcluirCapa(capa);

        TempData["Sucesso"] =
            "Livro excluído com sucesso.";

        return RedirectToAction(
            nameof(Index));
    }

    // =========================
    // FAVORITOS
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Favoritar(int id)
    {
        var usuarioId = int.Parse(
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var jaFavoritado =
            await _favoritos.EhFavoritoAsync(
                usuarioId,
                id);

        if (!jaFavoritado)
        {
            await _favoritos.AdicionarAsync(
                new Favorito
                {
                    UsuarioId = usuarioId,
                    LivroId = id
                });
        }

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Desfavoritar(int id)
    {
        var usuarioId = int.Parse(
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        await _favoritos.RemoverAsync(
            usuarioId,
            id);

        return RedirectToAction(
            nameof(Index));
    }

    // =========================
    // HELPERS
    // =========================

    private async Task<string?> SalvarCapaAsync(
        IFormFile? arquivo)
    {
        if (arquivo == null ||
            arquivo.Length == 0)
            return null;

        var extensoesPermitidas =
            new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        var extensao =
            Path.GetExtension(
                arquivo.FileName)
                .ToLowerInvariant();

        if (!extensoesPermitidas.Contains(
                extensao))
        {
            throw new InvalidOperationException(
                "Formato de imagem não permitido.");
        }

        if (arquivo.Length >
            5 * 1024 * 1024)
        {
            throw new InvalidOperationException(
                "A imagem deve ter no máximo 5 MB.");
        }

        var pasta =
            Path.Combine(
                _env.WebRootPath,
                "img",
                "capas");

        Directory.CreateDirectory(pasta);

        var nomeArquivo =
            $"{Guid.NewGuid()}{extensao}";

        var caminho =
            Path.Combine(
                pasta,
                nomeArquivo);

        await using var stream =
            new FileStream(
                caminho,
                FileMode.Create);

        await arquivo.CopyToAsync(
            stream);

        return $"/img/capas/{nomeArquivo}";
    }

    private void ExcluirCapa(
        string? caminhoCapa)
    {
        if (string.IsNullOrWhiteSpace(
                caminhoCapa))
            return;

        if (!caminhoCapa.StartsWith(
                "/img/capas/"))
            return;

        var nomeArquivo =
            Path.GetFileName(
                caminhoCapa);

        var caminho =
            Path.Combine(
                _env.WebRootPath,
                "img",
                "capas",
                nomeArquivo);

        if (System.IO.File.Exists(
                caminho))
        {
            System.IO.File.Delete(
                caminho);
        }
    }

    private async Task ValidarRegrasAsync(
        LivroFormViewModel vm)
    {
        if (vm.AutoresIds.Count == 0)
        {
            ModelState.AddModelError(
                nameof(vm.AutoresIds),
                "Selecione pelo menos um autor.");
        }

        if (!string.IsNullOrWhiteSpace(
                vm.Isbn) &&
            await _livros.IsbnExisteAsync(
                vm.Isbn.Trim(),
                vm.Id == 0
                    ? null
                    : vm.Id))
        {
            ModelState.AddModelError(
                nameof(vm.Isbn),
                "Já existe um livro cadastrado com este ISBN.");
        }
    }

    private async Task PreencherListasAsync(
        LivroFormViewModel vm)
    {
        vm.Categorias =
            (await _categorias.ObterTodosAsync())
            .Select(
                c => new SelectListItem(
                    c.Nome,
                    c.Id.ToString()));

        vm.Editoras =
            (await _editoras.ObterTodosAsync())
            .Select(
                e => new SelectListItem(
                    e.Nome,
                    e.Id.ToString()));

        vm.Autores =
            (await _autores.ObterTodosAsync())
            .Select(
                a => new SelectListItem(
                    a.Nome,
                    a.Id.ToString()));
    }

    [Authorize]
    public async Task<IActionResult> MeusFavoritos()
    {
        var usuarioId = int.Parse(
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var livros = await _favoritos.ObterFavoritosAsync(usuarioId);

        var vm = new MeusFavoritosViewModel
        {
            Livros = livros.Select(l => new LivroListaViewModel
            {
                Id = l.Id,
                Titulo = l.Titulo,
                Isbn = l.Isbn,
                AnoPublicacao = l.AnoPublicacao,
                Capa = l.Capa,
                Favoritado = true,
                Categoria = l.Categoria.Nome,
                Editora = l.Editora.Nome,
                Autores = string.Join(
                    ", ",
                    l.Autores.Select(a => a.Nome).OrderBy(n => n)),
                TotalExemplares = l.Exemplares.Count,
                ExemplaresDisponiveis = l.Exemplares.Count(
                    e => e.Situacao == SituacaoExemplar.Disponivel)
            }).ToList()
        };

        return View(vm);
    }
}