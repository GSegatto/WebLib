using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.ViewModels;

public class EmprestimoFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecione o usuário.")]
    [Display(Name = "Usuário")]
    public int? UsuarioId { get; set; }

    [Required(ErrorMessage = "Selecione o livro/exemplar.")]
    [Display(Name = "Livro (exemplar)")]
    public int? ExemplarId { get; set; }

    [Required(ErrorMessage = "Informe a data de retirada.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de retirada")]
    public DateTime? DataRetirada { get; set; }

    [Required(ErrorMessage = "Informe a data prevista de devolução.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data prevista de devolução")]
    public DateTime? DataPrevistaDevolucao { get; set; }

    public IEnumerable<SelectListItem> Usuarios { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Exemplares { get; set; } = new List<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataRetirada.HasValue && DataPrevistaDevolucao.HasValue && DataPrevistaDevolucao.Value.Date < DataRetirada.Value.Date)
            yield return new ValidationResult(
                "A data prevista de devolução não pode ser anterior à data de retirada.",
                new[] { nameof(DataPrevistaDevolucao) });
    }
}

public class EmprestimoListaViewModel
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string NumeroExemplar { get; set; } = string.Empty;

    public DateTime? DataRetirada { get; set; }
    public DateTime? DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }

    public StatusEmprestimo Status { get; set; }
    public bool EmAberto { get; set; }
    public bool Atrasado { get; set; }
}

public class AcervoViewModel
{
    public List<ExemplarItemViewModel> Disponiveis { get; set; } = new();

    public List<EmprestimoListaViewModel> SolicitacoesPendentes { get; set; } = new();

    public List<EmprestimoListaViewModel> AguardandoRetirada { get; set; } = new();

    public List<EmprestimoListaViewModel> Emprestados { get; set; } = new();
}

public class MeusEmprestimosViewModel
{
    public int Limite { get; set; }

    public List<EmprestimoListaViewModel> Solicitacoes { get; set; } = new();

    public List<EmprestimoListaViewModel> EmAberto { get; set; } = new();

    public List<EmprestimoListaViewModel> Devolvidos { get; set; } = new();

    public List<EmprestimoListaViewModel> Encerrados { get; set; } = new();
}