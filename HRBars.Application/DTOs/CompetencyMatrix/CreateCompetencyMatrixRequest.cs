using System.ComponentModel.DataAnnotations;
using HRBars.Application.DTOs.Competency;

namespace HRBars.Application.DTOs.CompetencyMatrix;

public class CreateCompetencyMatrixRequest
{
    [Required(ErrorMessage = "Название матрицы обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Компетенции обязательны")]
    public List<CompetencyResponse> CompetencyResponses { get; set; } = new List<CompetencyResponse>();

    public Guid VacancyId { get; set; }
}