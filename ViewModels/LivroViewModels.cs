using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.ViewModels;

public class LivroListaViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }
    public string? Capa { get; set; }
    public bool Favoritado { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public string Autores { get; set; } = string.Empty;
    public int TotalExemplares { get; set; }
    public int ExemplaresDisponiveis { get; set; }
}

public class LivroFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Informe o ISBN.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O ISBN deve ter entre 10 e 20 caracteres.")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o ano de publicação.")]
    [Range(1000, 2100, ErrorMessage = "Informe um ano entre 1000 e 2100.")]
    [Display(Name = "Ano de publicação")]
    public int? AnoPublicacao { get; set; }

    public bool Destaque { get; set; }

    public IFormFile? Capa { get; set; }

    public string? CapaAtual { get; set; }

    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "Selecione uma editora.")]
    [Display(Name = "Editora")]
    public int? EditoraId { get; set; }

    [Display(Name = "Autores")]
    public List<int> AutoresIds { get; set; } = new();

    public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Editoras { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Autores { get; set; } = new List<SelectListItem>();
}

public class ExemplarItemViewModel
{
    public int Id { get; set; }

    public int LivroId { get; set; }

    public string NumeroExemplar { get; set; } = string.Empty;

    public string Situacao { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;
    public string? Capa { get; set; }

    public bool Disponivel { get; set; }
}

public class LivroDetalhesViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string Isbn { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }
    public string? Capa { get; set; }
    public bool Favoritado { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public string Autores { get; set; } = string.Empty;
    public List<ExemplarItemViewModel> Exemplares { get; set; } = new();
}

public class ExemplarFormViewModel
{
    public int LivroId { get; set; }

    [Required(ErrorMessage = "Informe o número do exemplar (tombo).")]
    [StringLength(20, ErrorMessage = "O número do exemplar deve ter no máximo 20 caracteres.")]
    public string NumeroExemplar { get; set; } = string.Empty;
}

public class LivrosIndexViewModel
{
    [Display(Name = "Buscar")]
    public string? Q { get; set; }

    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }

    public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();

    public List<LivroListaViewModel> Livros { get; set; } = new();
}