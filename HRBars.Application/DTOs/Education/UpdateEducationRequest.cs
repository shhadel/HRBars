using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Education;

public class UpdateEducationRequest
{
    [Required(ErrorMessage = "Название учебного заведения обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Institution { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Название факультета не должно превышать 100 символов")]
    public string? Faculty { get; set; }

    [StringLength(50, ErrorMessage = "Степень не должна превышать 50 символов")]
    public string? Degree { get; set; }

    [Range(1900, 2026, ErrorMessage = "Год начала должен быть между 1900 и 2026")]
    public short? StartYear { get; set; }

    [Range(1900, 2026, ErrorMessage = "Год окончания должен быть между 1900 и 2026")]
    public short? EndYear { get; set; }
}