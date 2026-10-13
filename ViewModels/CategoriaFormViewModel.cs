using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Biblioteca.ViewModels;

public class CategoriaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public string? ImagemAtual { get; set; }

    public IFormFile? ArquivoImagem { get; set; }
}