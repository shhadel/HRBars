using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Competency;

public class CreateCompetencyRequest
{
    [Required(ErrorMessage = "Название компетенции обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Категория не должна превышать 50 символов")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
}