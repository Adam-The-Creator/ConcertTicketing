using ConcertTicketing.Server.Data.Context;
using ConcertTicketing.Server.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public AuthController(ConcertTicketingDBContext context) => _context = context;

        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] SignupRequest signupRequest)
        {
            if (await _context.Users.AnyAsync(u => u.Email == signupRequest.Email))
            {
                return Conflict("E-mail already exists.");
            }

            var hashedPassword = HashPassword(signupRequest.Password);
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (userRole == null) return StatusCode(500, "User role 'Customer' not configured.");

            var password = new Password
            {
                Id = Guid.NewGuid(),
                HashedPassword = hashedPassword
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = signupRequest.Username,
                Email = signupRequest.Email,
                Created = DateTime.UtcNow,
                PasswordId = password.Id,
                Password = password,
                UserRoleId = userRole.Id
            };

            _context.Passwords.Add(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Username, user.Email, user.UserRole.RoleName });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] SigninRequest signinRequest)
        {
            var user = await _context.Users.Include(u => u.Password).Include(u => u.UserRole).FirstOrDefaultAsync(u => u.Email == signinRequest.Email);

            if (user == null || user.Password == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!VerifyPassword(signinRequest.Password, user.Password.HashedPassword.TrimEnd())) return Unauthorized("Invalid credentials.");

            user.SignedIn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Username, user.Email, user.UserRole.RoleName });
        }



        public static string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }

        public static string HashPassword(string password, string salt)
        {
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = SHA256.HashData(combined);
            return Convert.ToBase64String(hash);
        }
        
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string inputPassword, string hashedUserPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedUserPassword);
        }
    }

    public class SignupRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SigninRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}