using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.User
{
    public class UpdateUser
    {
        [MaxLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
        public string? LastName { get; set; }

        [MaxLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
        public string? MiddleName { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [MaxLength(255, ErrorMessage = "Email не может превышать 255 символов")]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [MaxLength(100, ErrorMessage = "Пароль не может превышать 100 символов")]
        public string? Password { get; set; }

        public string? RoleName { get; set; }

        public bool? IsActive { get; set; }

        public List<string>? Permissions { get; set; }
    }
}