using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.CompetencyMatrix;
public class UpdateCompetencyMatrixRequest
{
    [Required(ErrorMessage = "Название матрицы обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }

    public Guid? VacancyId { get; set; }

    public bool IsArchived { get; set; }
}