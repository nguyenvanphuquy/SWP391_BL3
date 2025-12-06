namespace SWP391_BL3.Models.DTOs.Response
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; } = null;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

    }
}
