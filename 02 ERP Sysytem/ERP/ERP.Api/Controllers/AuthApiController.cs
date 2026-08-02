using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

//Code attribution
//Create web APIs with ASP.NET Core
//Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0
//[Accessed: 6 June 2026]

namespace ERP.Api.Controllers
{
    // This API controller is responsible for handling user registration and login operations,
    // including dynamic role assignment based on email domain parsing.
    // It leverages ASP.NET Core Identity for user management and JWT for secure token generation.
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // POST: api/authapi/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Invalid account configuration payload context.");
            }

            // 1. DOMAIN DETERMINATION LOGIC
            string targetRole = "Client"; // Default fallback role
            string emailLower = model.Email.ToLower().Trim();

            if (emailLower.EndsWith("@admin.com"))
            {
                targetRole = "Admin";
            }
            else if (emailLower.EndsWith("@client.com"))
            {
                targetRole = "Client";
            }
            else
            {
                return BadRequest("Registration Error: Email must end with '@admin.com' or '@client.com'.");
            }

            // 2. CHECK FOR EXISTING PROFILE
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("Registration Error: An account with that email address is already provisioned.");
            }

            // 3. BUILD THE USER DEFINITION
            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                EmailConfirmed = true,
                NormalizedUserName = _userManager.NormalizeName(model.Email),
                NormalizedEmail = _userManager.NormalizeEmail(model.Email)
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                string errorList = string.Join(" ", System.Linq.Enumerable.Select(result.Errors, e => e.Description));
                return BadRequest($"Identity Policy Blocked Registration: {errorList}");
            }

            // 4. GUARANTEE IDENTITY ROLE GENERATION
            if (!await _roleManager.RoleExistsAsync(targetRole))
            {
                var role = new IdentityRole(targetRole);
                role.NormalizedName = _roleManager.NormalizeKey(targetRole);
                await _roleManager.CreateAsync(role);
            }

            // 5. ASSIGN THE PARSED DYNAMIC ROLE
            await _userManager.AddToRoleAsync(newUser, targetRole);

            return Ok(new { Message = $"User successfully registered into the {targetRole} framework profile." });
        }

        // POST: api/authapi/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Email parameter payload is null.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials or user profile not found.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid credentials or incorrect security password entry.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            string userRole = roles.Count > 0 ? roles[0] : "Client";

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:Secret"] ?? "YourFallbackSuperSecretSystemKeyString2026";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.Email!),
                    new Claim(ClaimTypes.Role, userRole)
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token), Role = userRole });
        }
    }

    // These request models are designed to capture the necessary data for user registration and login operations.
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // The LoginRequest model captures the essential credentials required for user authentication.
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}