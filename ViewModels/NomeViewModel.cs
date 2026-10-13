using System.ComponentModel.DataAnnotations;

namespace Biblioteca.ViewModels;

/// <summary>Usado por Categorias, Editoras e Autores (só têm Id + Nome).</summary>
public class NomeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}
