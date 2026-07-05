using System.Text.RegularExpressions;
using AuthenticationService.Data;
using AuthenticationService.Models.DTO;

namespace AuthenticationService.Services
{
    public class UserServiceBll : IUserBll
    {
        private readonly IUserDal _userDal;

        public UserServiceBll(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public async Task AddUser(UserDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.Role))
            {
                userDto.Role = "Customer";
            }

            if (!IsValidEmail(userDto.Email))
                throw new ArgumentException("Invalid email format.");

            if (!IsValidPassword(userDto.Password))
                throw new ArgumentException("Password must be at least 6 characters long and contain a number.");

            if (!IsValidName(userDto.Name))
                throw new ArgumentException("Name must contain only letters.");

            var users = await _userDal.GetAll();
            var existingUser = users.FirstOrDefault(u => u.Email == userDto.Email);
            if (existingUser != null)
                throw new ArgumentException("Email is already registered.");

            userDto.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            await _userDal.Add(userDto);
        }

        public async Task<UserDto> ValidateUser(string email, string password)
        {
            var user = await _userDal.GetFullUserByEmailAsync(email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Name = user.Name
                };
            }

            return null;
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 && password.Any(char.IsDigit);
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-');
        }
    }
}
