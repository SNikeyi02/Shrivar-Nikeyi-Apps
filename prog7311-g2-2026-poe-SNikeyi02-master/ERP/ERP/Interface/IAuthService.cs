using ERP.Data;

namespace ERP.Interface
{
    public interface IAuthService
    {
        Task<bool> PasswordLoginAsync(string username, string password);
        Task<string> GetRedirectDashboardAsync(System.Security.Claims.ClaimsPrincipal userPrincipal);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task CreateInitialAdminAsync(string email, string password, string name);
    }
}
