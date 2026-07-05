using AuthenticationService.Models.DTO;

namespace AuthenticationService.Services
{
    public interface IUserBll
    {
        Task AddUser(UserDto userDto);
        Task<UserDto> ValidateUser(string email, string password);
    }
}
