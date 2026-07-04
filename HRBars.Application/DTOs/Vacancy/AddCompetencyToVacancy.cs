using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Vacancy
{
    public class AddCompetencyToVacancy
    {
        [Required(ErrorMessage = "Название компетенции обязательно")]
        [MaxLength(150, ErrorMessage = "Название не может превышать 150 символов")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Категория не может превышать 100 символов")]
        public string? Category { get; set; }

        [MaxLength(300, ErrorMessage = "Описание не может превышать 300 символов")]
        public string? Description { get; set; }

        public int Weight { get; set; } = 1;
        public int MaxScore { get; set; } = 10;
    }
}
