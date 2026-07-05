using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using AuthenticationService.Models;
using AuthenticationService.Models.DTO;
using System.Linq.Expressions;

namespace AuthenticationService.Data
{
    public class UserDal : IUserDal
    {
        private readonly IMapper _mapper;
        private readonly AuthDbContext _context;
        private readonly ILogger _logger;

        public UserDal(AuthDbContext context, IMapper mapper, ILogger<UserDal> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Add(UserDto userDto)
        {
            try
            {
                if (!Enum.TryParse<UserRole>(userDto.Role, true, out _))
                    throw new ArgumentException("Invalid role");

                var userModel = _mapper.Map<UserModel>(userDto);
                _context.Users.Add(userModel);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error during Add operation:{ex.Message}");
                throw;
            }
        }

        public async Task<List<UserDto>> GetAll()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserDal.GetAll");
                throw;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.IsDeleted = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                _logger.LogError($"Error in UserDal.Delete for id: {id}");
                throw;
            }
        }

        public async Task<UserModel> GetFullUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UserDal.GetFullUserByEmailAsync for email: {email}");
                throw;
            }
        }
    }
}
