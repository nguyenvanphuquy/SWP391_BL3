namespace SWP391_BL3.Models.DTOs.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public int? RoleId { get; set; }
    }
}
