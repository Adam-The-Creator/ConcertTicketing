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
            if (await _context.Customers.AnyAsync(c => c.Email == signupRequest.Email))
            {
                return Conflict("E-mail already exists.");
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(signupRequest.Password, salt);

            var password = new Password
            {
                Id = Guid.NewGuid(),
                Password1 = hashedPassword,
                Salt = salt
            };

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = signupRequest.Name,
                Email = signupRequest.Email,
                Created = DateTime.UtcNow,
                PasswordId = password.Id,
                Password = password
            };

            _context.Passwords.Add(password);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { customer.Id, customer.Email });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] SigninRequest signinRequest)
        {
            var customer = await _context.Customers.Include(c => c.Password).FirstOrDefaultAsync(c => c.Email == signinRequest.Email);

            if (customer == null || customer.Password == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var hashedInput = HashPassword(signinRequest.Password, customer.Password.Salt!);

            if (hashedInput != customer.Password.Password1)
            {
                return Unauthorized("Invalid credentials.");
            }

            customer.SignedIn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { customer.Id, customer.Name });
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
    }

    public class SignupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SigninRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
