using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using SWP391_BL3.BLL.Configurations;
using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;
using SWP391_BL3.DAL.Models.Entities;
using SWP391_BL3.DAL.Repositories.Interfaces;
using SWP391_BL3.BLL.Services.Interfaces;
namespace SWP391_BL3.BLL.Services.Implementations
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;

        public readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IRoleRepository  _roleRepository;
        private readonly IConfiguration _configuration; 
        public UserService(IUserRepository userRepository, JwtTokenGenerator jwtTokenGenerator, IRoleRepository roleRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleRepository = roleRepository;
            _configuration = configuration;
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
            var userInforResponses = users
                .OrderByDescending(user => user.CreateAt) // Thêm dòng này - s?p x?p theo ngày t?o m?i nh?t
                .Select(user => new UserInforResponse
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
                throw new Exception("Vai trò không h?p l?");
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
    /*
     * Q28: Google OAuth có validate dúng cách không?
     * A28: CÓ.
     * - Validate token v?i Google server (GoogleJsonWebSignature.ValidateAsync)
     * - Validate Audience (ClientId) d? d?m b?o token t? app c?a mình
     * - Validate signature d? ch?ng token gi? m?o
     * - Luu ý: .Result có th? gây deadlock, nên dùng await async
     * 
     * Q29: Có x? lý tru?ng h?p token gi? m?o không?
     * A29: CÓ. ValidateAsync() s? throw exception n?u token không h?p l?
     * - Invalid signature ? exception
     * - Expired token ? exception
     * - Wrong audience ? exception
     */
    public LoginResponse? GoogleLogin(string idToken)
        {
            try
            {
                // 1. Xác th?c token v?i Google
                // Validate token v?i Google server d? d?m b?o token h?p l?
                var clientId = _configuration["Authentication:Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId } // Ch? ch?p nh?n token t? app c?a mình
                };

                // TODO: Nên dùng await thay vì .Result d? tránh deadlock
                var payload = GoogleJsonWebSignature.ValidateAsync(idToken, settings).Result;

                if (payload == null)
                {
                    return null;
                }

                // 2. Ki?m tra user dã t?n t?i chua (theo GoogleId)
                var user = _userRepository.GetByGoogleId(payload.Subject);

                if (user == null)
                {
                    // Ki?m tra email dã t?n t?i chua
                    user = _userRepository.GetbyEmail(payload.Email);

                    if (user != null)
                    {
                        // N?u email dã t?n t?i nhung chua có GoogleId
                        // => Liên k?t tài kho?n v?i Google
                        user.GoogleId = payload.Subject;
                        user.UpdateAt = DateTime.UtcNow;
                        _userRepository.Update(user);
                    }
                    else
                    {
                        // T?o user m?i v?i role m?c d?nh (Customer)
                        var customerRole = _roleRepository.GetByName("Student");
                        if (customerRole == null)
                        {
                            throw new Exception("Không tìm th?y role Customer");
                        }

                        user = new User
                        {
                            FullName = payload.Name,
                            Email = payload.Email,
                            GoogleId = payload.Subject,
                            PasswordHash = null, // Không c?n password khi login b?ng Google
                            Phone = null,
                            Status = "Active",
                            RoleId = customerRole.RoleId,
                            CreateAt = DateTime.UtcNow,
                            UpdateAt = DateTime.UtcNow
                        };

                        _userRepository.Create(user);
                    }
                }
                else
                {
                    // User dã t?n t?i, c?p nh?t th?i gian dang nh?p
                    user.UpdateAt = DateTime.UtcNow;
                    _userRepository.Update(user);
                }

                // 3. Ki?m tra tr?ng thái tài kho?n
                if (user.Status != "Active")
                {
                    return null; // Tài kho?n b? khóa
                }

                // 4. T?o JWT token
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
            catch (Exception ex)
            {
                /*
                 * Q30: Có logging d?y d? không?
                 * A30: CHUA d?y d?.
                 * - Hi?n t?i: Ch? dùng Console.WriteLine (không phù h?p production)
                 * - Nên: Dùng ILogger<T> d? log structured logging
                 * - Nên log: Exception details, User info (không log sensitive data)
                 * - Production: Dùng Serilog, NLog, ho?c Application Insights
                 */
                // Log error n?u c?n
                // TODO: Thay b?ng ILogger
                Console.WriteLine($"Google Login Error: {ex.Message}");
                return null;
            }
        }
    }
}