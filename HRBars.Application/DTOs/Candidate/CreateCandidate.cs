using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Candidate
{
    public class CreateCandidate
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [MaxLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
        public string? MiddleName { get; set; }

        [MaxLength(200)]
        public string? DesiredVacancy { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone]
        [MaxLength(30, ErrorMessage = "Номер телефона не может превышать 30 символов")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(255, ErrorMessage = "Почта не может превышать 255 символов")]
        public string? Email { get; set; }

        [MaxLength(100, ErrorMessage = "Город не может превышать 100 символов")]
        public string? City { get; set; }

        public string? Skills { get; set; }
    }
}