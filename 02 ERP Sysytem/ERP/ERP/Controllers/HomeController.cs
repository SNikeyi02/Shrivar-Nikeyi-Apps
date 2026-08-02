using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

//Code attribution
//AllowAnonymousAttribute Class
//Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.allowanonymousattribute?view=aspnetcore-10.0
//[Accessed: 7 June 2026]

//Code attribution
//AuthenticationProperties Class
//Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationproperties?view=aspnetcore-10.0
//[Accessed: 5 June 2026]

namespace ERP.Controllers
{
    // This controller serves as the entry point for unauthenticated users,
    // providing both login and registration capabilities.
    // It is decorated with [AllowAnonymous] to permit access without authentication,
    // while still enforcing role-based redirection logic after successful login.
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(roleClaim))
                {
                    return RedirectBasedOnRole(roleClaim);
                }
            }

            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password fields are required.");
                return View(nameof(Index));
            }

            var client = _clientFactory.CreateClient("ErpApiClient");
            var loginPayload = new { Email = username, Password = password };

            try
            {
                var response = await client.PostAsJsonAsync("api/authapi/login", loginPayload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResult>();

                    if (result != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Role, result.Role),
                            new Claim("JwtToken", result.Token)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                        };

                        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                        return RedirectBasedOnRole(result.Role);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"API connection dropped: {ex.Message}");
                return View(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials or unauthorized user account access.");
            return View(nameof(Index));
        }

        // NEW POST: /Home/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string fullName, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The security passwords entered do not match.");
                return View(nameof(Index));
            }

            var client = _clientFactory.CreateClient("ErpApiClient");
            var registerPayload = new
            {
                Email = email,
                Name = fullName,
                Password = password,
                Role = "Admin" // Explicitly locked to Admin creation capability
            };

            try
            {
                var response = await client.PostAsJsonAsync("api/authapi/register", registerPayload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Admin account registered successfully! You can now log in below.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMsg) ? errorMsg : "Registration failed identity verification checks.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"API validation gateway unreachable: {ex.Message}");
            }

            return View(nameof(Index));
        }

        // POST: /Home/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectBasedOnRole(string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }

            return RedirectToAction("Index", "Client");
        }
    }

    // This is a simple DTO class to capture the login result from the API, including the JWT token and user role.
    public class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}