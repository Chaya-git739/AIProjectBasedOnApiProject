using AuthenticationService.Models.DTO;

namespace AuthenticationService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDTO login);
        Task<AuthResponseDto> RegisterAsync(UserDto userDto);
    }
}
