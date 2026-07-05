using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Application;

public class ChangeStatusRequest
{
    [Required(ErrorMessage = "Статус обязателен")]
    public ApplicationStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
    public string? Comment { get; set; }
}