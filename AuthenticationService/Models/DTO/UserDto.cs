using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.DTO
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "הטלפון חייב להכיל ספרות בלבד")]
        public string Phone { get; set; }

        public string Password { get; set; }
        public string Role { get; set; } = "Customer";
    }
}
