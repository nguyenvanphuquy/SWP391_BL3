using BCrypt.Net;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
using SWP391_BL3.Configurations;
namespace SWP391_BL3.Services.Implementations
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public readonly JwtTokenGenerator _jwtTokenGenerator;
        public UserService(IUserRepository userRepository, JwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;

            _jwtTokenGenerator = jwtTokenGenerator;
        }
        public LoginResponse? Login(LoginRequest request)
        {
            var user = _userRepository.GetbyEmail(request.Email);
            if (user == null) return null;
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid) return null;
            var token = _jwtTokenGenerator.GenerateToken(user);
            return new LoginResponse
            {
                Token = token,
                RefreshToken = "",
                Expires = DateTime.UtcNow.AddHours(1),
                UserId = user.UserId,
                Email = user.Email,
                RoleId = user.RoleId,

            };
        }
    }
}
