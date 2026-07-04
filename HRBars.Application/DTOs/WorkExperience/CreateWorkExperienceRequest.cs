using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.WorkExperience;

public class CreateWorkExperienceRequest
{
    [Required(ErrorMessage = "Название компании обязательно")]
    [StringLength(200, ErrorMessage = "Название компании не должно превышать 200 символов")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Должность обязательна")]
    [StringLength(100, ErrorMessage = "Должность не должна превышать 100 символов")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата начала работы обязательна")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
    public string? Description { get; set; }
}