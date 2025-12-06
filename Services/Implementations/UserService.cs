using BCrypt.Net;
using BE_SWP391.Repositories.Interfaces;
using SWP391_BL3.Configurations;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Implementations;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
namespace SWP391_BL3.Services.Implementations
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IRoleRepository  _roleRepository;
        public UserService(IUserRepository userRepository, JwtTokenGenerator jwtTokenGenerator, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;

            _jwtTokenGenerator = jwtTokenGenerator;
            _roleRepository = roleRepository;
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
        public IEnumerable<UserResponse> GetAll()
        {
            var user = _userRepository.GetAll();
            return user.Select(u => new UserResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Status = u.Status,
                RoleId = u.RoleId,
                CreateAt = u.CreateAt,
                UpdateAt = u.UpdateAt,

            }
            );
        }
        public IEnumerable<UserInforResponse> GetAllInfor()
        {
            var users = _userRepository.GetAllInfor();
            var userInforResponses = users.Select(user => new UserInforResponse
            {
                id = user.UserId,
                name = user.FullName ?? string.Empty,
                email = user.Email,
                roleName = user.Role != null ? user.Role.RoleName : "N/A",
                booking = user.BookingUsers != null ? user.BookingUsers.Count : 0,
                status = user.Status
            });
            return userInforResponses;
        }
        public UserResponse? GetById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return null;
            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                RoleId = user.RoleId,
                CreateAt = user.CreateAt,
                UpdateAt = user.UpdateAt

            };

        }
        public UserResponse Create(UserRequest request)
        {
            if (_userRepository.GetbyEmail(request.Email) != null)
            {
                throw new Exception("Email already exists");
            }
            var role = _roleRepository.GetByName(request.RoleName);
            if (role == null)
            {
                throw new Exception("Vai trò không hợp lệ");
            }
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                Status = "Active",
                RoleId = role.RoleId,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            _userRepository.Create(user);
            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                RoleId = user.RoleId,
                CreateAt = user.CreateAt,
                UpdateAt = user.UpdateAt
            };
        }
        public bool Delete(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return false;
            _userRepository.UpdateStatus(id, "Inactive");
            return true;
        }
    }
}
