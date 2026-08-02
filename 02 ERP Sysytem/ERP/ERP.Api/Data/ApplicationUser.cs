using Microsoft.AspNetCore.Identity;

namespace ERP.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}