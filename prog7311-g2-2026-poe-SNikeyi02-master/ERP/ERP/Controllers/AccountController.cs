using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

//Code attribution
//IHttpClientFactory with .NET
//Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
//[Accessed: 5 June 2026]

//Code attribution
//Use cookie authentication without ASP.NET Core Identity
//Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
//[Accessed: 5 June 2026]

namespace ERP.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // POST: /Account/QuickLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickLogin(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email target is required.";
                return RedirectToAction("Index", "Home");
            }

            var client = _clientFactory.CreateClient("ErpApiClient");

            try
            {
                var response = await client.PostAsJsonAsync("api/authapi/login", new { Email = email, Password = "YourDefaultOrFormPassword Here" });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AccountLoginResult>();

                    if (result != null)
                    {
                        var claims = new System.Collections.Generic.List<Claim>
                        {
                            new Claim(ClaimTypes.Name, email),
                            new Claim(ClaimTypes.Role, result.Role),
                            new Claim("JwtToken", result.Token)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                        if (string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("AdminDashboard", "Admin");
                        }

                        return RedirectToAction("Index", "Client");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"API validation gateway unreachable: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Login failed. Please confirm your credentials match your assigned corporate email domain configuration.";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }

    // This is a simple DTO class to capture the login result from the API, including the JWT token and user role.
    public class AccountLoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}