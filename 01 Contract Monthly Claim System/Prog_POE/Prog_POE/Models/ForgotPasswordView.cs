using System.ComponentModel.DataAnnotations;

namespace Prog_POE.Models
{
    public class ForgotPasswordView
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        public string? Message { get; set; }
    }
}