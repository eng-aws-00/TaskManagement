using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.DbContexts;
using TaskManagement.Dtos.Auth;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskManagementContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthController(TaskManagementContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDto signupDto)
        {
            try
            {
                // Check if email already exists
                var existingEmail = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == signupDto.Email.ToLower());
                if (existingEmail != null)
                {
                    return BadRequest("Email already registered.");
                }

                // Check if username already exists
                var existingUsername = _dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == signupDto.Username.ToLower());
                if (existingUsername != null)
                {
                    return BadRequest("Username already taken.");
                }

                // Hash password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(signupDto.Password);

                // Create user
                var user = new User
                {
                    Username = signupDto.Username,
                    Email = signupDto.Email,
                    HashedPassword = hashedPassword
                };

                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();

                return StatusCode(201, user.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == loginDto.Email.ToLower());
                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword);
                if (!isPasswordValid)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Generate JWT token
                string token = GenerateJwtToken(user);

                // Return response
                var response = new LoginResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
            var token = tokenHandler.WriteToken(tokenSettings);

            return token;
        }
    }
}
