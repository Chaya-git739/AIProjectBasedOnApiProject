using AuthenticationService.Models;
using AuthenticationService.Models.DTO;

namespace AuthenticationService.Data
{
    public interface IUserDal
    {
        Task Add(UserDto userDto);
        Task<List<UserDto>> GetAll();
        Task Delete(int id);
        Task<UserModel> GetFullUserByEmailAsync(string email);
    }
}
