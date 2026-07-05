using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsDeleted { get; set; } = false;
    }
}
