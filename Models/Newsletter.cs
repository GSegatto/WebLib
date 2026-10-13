using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class Newsletter
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime DataInscricao { get; set; } = DateTime.Now;
}