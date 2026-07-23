using ERP.Data;
using ERP.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Code attribution
//Asynchronous Programming
//Available at: https://learn.microsoft.com/en-us/ef/core/miscellaneous/async
//[Accessed: 1 June 2026]

namespace ERP.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // This method attempts to sign in a user using their username and password.
        public async Task<bool> PasswordLoginAsync(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);
            return result.Succeeded;
        }

        // This method determines the appropriate dashboard to redirect a user to based on their assigned roles.
        public async Task<string> GetRedirectDashboardAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal)
                       ?? throw new UnauthorizedAccessException("Session expired or user profile not found.");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return "AdminDashboard";

            if (roles.Contains("Client"))
                return "Index";

            throw new UnauthorizedAccessException("Access denied: User has no assigned workflow roles.");
        }

        // This method retrieves all users from the database and returns them as a list.
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        // This method creates an initial admin user with the specified email, password, and name.
        // It also ensures that the "Admin" role exists and assigns it to the newly created user.
        public async Task CreateInitialAdminAsync(string email, string password, string name)
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                    throw new Exception($"Failed to create Admin role: {roleResult.Errors.First().Description}");
            }

            var user = new ApplicationUser { UserName = email, Email = email, Name = name, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"User creation failed: {result.Errors.First().Description}");
            }

            await _userManager.AddToRoleAsync(user, "Admin");
        }
    }
}