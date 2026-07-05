namespace WebApplication2.Models.DTO
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
        public string? Message { get; set; }
    }
}
