using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "דוא״ל הוא חובה")]
        [EmailAddress(ErrorMessage = "דוא״ל אינו תקני")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "סיסמה היא חובה")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "סיסמה חייבת להיות לפחות 6 תווים")]
        public string Password { get; set; } = null!;
    }
}
