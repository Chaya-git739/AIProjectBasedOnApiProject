using WebApplication2.DAL; // Importing DAL
using WebApplication2.Models.DTO; // Importing User DTOs
using System; // Importing for Exception handling
using System.Linq; // Importing FirstOrDefault and LINQ
using System.Text.RegularExpressions; // Required for Regex validation
using System.Threading.Tasks; // Add this if not already present

namespace WebApplication2.BLL // BLL namespace
{
    public class UserServiceBLL : IUserBll // Implementing the BLL interface
    {
        private readonly IUserDal _userDal; // Dependency on DAL
        private readonly IOrderDal _orderDal; // Dependency on Order DAL for validation

        public UserServiceBLL(IUserDal userDal, IOrderDal orderDal = null) // Constructor receiving DAL
        {
            _userDal = userDal; // Storing DAL in field
            _orderDal = orderDal; // Storing Order DAL for validation
        }

        public async Task AddUser(UserDto userDto) // Make method async
        {
            if (string.IsNullOrEmpty(userDto.Role)) // Check if role is not defined
            {
                userDto.Role = "Customer"; // Default role
            }

            // Validate email format
            if (!IsValidEmail(userDto.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }

            // Validate password strength
            if (!IsValidPassword(userDto.Password))
            {
                throw new ArgumentException("Password must be at least 6 characters long and contain a number.");
            }

            // Validate name contains only letters
            if (!IsValidName(userDto.Name))
            {
                throw new ArgumentException("Name must contain only letters.");
            }

            // Await the GetAll() call and then use FirstOrDefault
            var users = await _userDal.GetAll();
            var existingUser = users.FirstOrDefault(u => u.Email == userDto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Email is already registered.");
            }

            // Hash the password before saving
            userDto.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            await _userDal.Add(userDto); // Await Add as well
        }

        public async Task<UserDto> ValidateUser(string email, string password)
        {
            // שליפת המשתמש המלא כולל הסיסמה
            var user = await _userDal.GetFullUserByEmailAsync(email);

            // בדיקה שהמשתמש קיים והסיסמה תואמת באמצעות BCrypt
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // מחזירים DTO ללא הסיסמה לצורך יצירת ה-Token
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
            // Simple regex for email validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool IsValidPassword(string password)
        {
            // Validates password to be at least 6 characters long and include a digit
            return password.Length >= 6 && password.Any(char.IsDigit);
        }

        private bool IsValidName(string name)
        {
            // מאפשר אותיות בכל שפה, רווחים ותווים מיוחדים נפוצים
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-');
        }

        public async Task DeleteUserAsync(int userId)
        {
            // בדיקה: האם למשתמש זה יש הזמנות מאושרות?
            if (_orderDal != null)
            {
                bool hasConfirmedOrders = await _orderDal.HasConfirmedOrdersForUserAsync(userId);
                if (hasConfirmedOrders)
                {
                    throw new BusinessException("לא ניתן למחוק את המשתמש כיוון שיש לו הזמנות מאושרות בהיסטוריה.");
                }
            }

            // מחיקה רכה (Soft Delete)
            await _userDal.Delete(userId);
        }
    }
}