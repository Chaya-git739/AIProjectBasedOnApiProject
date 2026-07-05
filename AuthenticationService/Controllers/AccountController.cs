using Microsoft.AspNetCore.Mvc;
using AuthenticationService.Models.DTO;
using AuthenticationService.Services;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            _logger.LogInformation("Login attempt for email: {Email}", login.Email);

            try
            {
                var response = await _authService.LoginAsync(login);

                if (response != null)
                {
                    return Ok(response);
                }

                return Unauthorized("Invalid username or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", login.Email);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", userDto?.Email);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for registration");
                    return BadRequest(ModelState);
                }

                var response = await _authService.RegisterAsync(userDto);
                _logger.LogInformation("User registered successfully: {Email}", userDto.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for email: {Email}", userDto?.Email);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
