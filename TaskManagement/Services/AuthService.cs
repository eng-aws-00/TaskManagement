using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.DbContexts;
using TaskManagement.Dtos.Auth;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface IAuthService
    {
        long Signup(SignupDto signupDto);
        LoginResponseDto Login(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly TaskManagementContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(TaskManagementContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public long Signup(SignupDto signupDto)
        {
            var existingEmail = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == signupDto.Email.ToLower());
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email already registered.");
            }

            var existingUsername = _dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == signupDto.Username.ToLower());
            if (existingUsername != null)
            {
                throw new InvalidOperationException("Username already taken.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(signupDto.Password);

            var user = new User
            {
                Username = signupDto.Username,
                Email = signupDto.Email,
                HashedPassword = hashedPassword
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user.Id;
        }

        public LoginResponseDto Login(LoginDto loginDto)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == loginDto.Email.ToLower());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            string token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenSettings = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenSettings);
        }
    }
}
