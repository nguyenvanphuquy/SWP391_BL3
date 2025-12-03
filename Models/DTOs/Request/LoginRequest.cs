using System.ComponentModel.DataAnnotations;

namespace SWP391_BL3.Models.DTOs.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
