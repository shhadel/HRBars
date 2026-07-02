using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.User
{
    public class CreateUser
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [MaxLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [MaxLength(255, ErrorMessage = "Email не может превышать 255 символов")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [MaxLength(100, ErrorMessage = "Пароль не может превышать 100 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Роль обязательна")]
        public string RoleName { get; set; } = string.Empty;

        public List<string>? Permissions { get; set; }
    }
}