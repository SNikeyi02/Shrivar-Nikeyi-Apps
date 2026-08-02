using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Prog_POE.Data;
using Prog_POE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

// Microsoft Learn 2023. Eager Loading of Related Data - EF Core. 
// Online 
// Available at:
// <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
// [Accessed 20 November 2025].

namespace Prog_POE.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // LOGIN (GET)
        public IActionResult Index()
        {
            return View(new LoginView());
        }

        // LOGIN (POST)
        [HttpPost]
        public async Task<IActionResult> Index(LoginView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await AuthenticateUser(model.Username, model.Password);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password. Please try again.");
                return View(model);
            }

            // Store session data
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Redirect based on role
            return user.Role switch
            {
                UserRole.Lecturer => RedirectToAction("Home", "Lecturer"),
                UserRole.Coordinator => RedirectToAction("Home", "Coordinator"),
                UserRole.Manager => RedirectToAction("Home", "Manager"),
                UserRole.HR => RedirectToAction("Dashboard", "HR"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // FORGOT PASSWORD (GET)
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordView());
        }

        // FORGOT PASSWORD (POST)
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var foundUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.Equals(model.Username));

            if (foundUser != null)
            {
                model.Message = "A query has been raised to the admin for password reset.";
            }
            else
            {
                model.Message = "User not found in the system.";
            }

            return View(model);
        }

        private async Task<User> AuthenticateUser(string username, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username.Equals(username) &&
                    u.Password == password && 
                    u.IsActive);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Login");
        }
    }
}
