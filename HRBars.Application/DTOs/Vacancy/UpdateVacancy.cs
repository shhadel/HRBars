using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Vacancy
{
    public class UpdateVacancy
    {
        [MaxLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string? Title { get; set; }

        [MaxLength(150, ErrorMessage = "Название отдела не может превышать 150 символов")]
        public string? Department { get; set; }

        public string? Description { get; set; }
    }
}
