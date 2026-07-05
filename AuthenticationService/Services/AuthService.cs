using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationService.Models.DTO;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserBll _userBll;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserBll userBll, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userBll = userBll;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDTO login)
        {
            _logger.LogInformation("Login attempt for email: {Email}", login.Email);

            var user = await _userBll.ValidateUser(login.Email, login.Password);
            if (user == null)
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", login.Email);
                return null;
            }

            var token = CreateToken(user);
            _logger.LogInformation("Login successful for email: {Email}", login.Email);

            return new AuthResponseDto
            {
                Token = token,
                User = user
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(UserDto userDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", userDto?.Email);

            if (userDto.Email == "admin@admin.com" || userDto.Role == "Manager")
            {
                userDto.Role = "Manager";
                _logger.LogInformation("Role set to Manager for email: {Email}", userDto?.Email);
            }

            await _userBll.AddUser(userDto);
            _logger.LogInformation("User registered successfully: {Email}", userDto.Email);

            var user = await _userBll.ValidateUser(userDto.Email, userDto.Password);
            if (user != null)
            {
                return new AuthResponseDto
                {
                    Token = CreateToken(user),
                    User = user,
                    Message = "User registered successfully"
                };
            }

            return new AuthResponseDto
            {
                Message = "User registered successfully. You can now log in."
            };
        }

        private string CreateToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyHere1234567890!";
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
