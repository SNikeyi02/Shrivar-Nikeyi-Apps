using System.ComponentModel.DataAnnotations;

namespace Prog_POE.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
