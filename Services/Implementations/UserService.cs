using BCrypt.Net;
using Google.Apis.Auth;
using SWP391_BL3.Configurations;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
namespace SWP391_BL3.Services.Implementations
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
                .OrderByDescending(user => user.CreateAt) // Thêm dòng này - sắp xếp theo ngày tạo mới nhất
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
    /*
     * Q28: Google OAuth có validate đúng cách không?
     * A28: CÓ.
     * - Validate token với Google server (GoogleJsonWebSignature.ValidateAsync)
     * - Validate Audience (ClientId) để đảm bảo token từ app của mình
     * - Validate signature để chống token giả mạo
     * - Lưu ý: .Result có thể gây deadlock, nên dùng await async
     * 
     * Q29: Có xử lý trường hợp token giả mạo không?
     * A29: CÓ. ValidateAsync() sẽ throw exception nếu token không hợp lệ
     * - Invalid signature → exception
     * - Expired token → exception
     * - Wrong audience → exception
     */
    public LoginResponse? GoogleLogin(string idToken)
        {
            try
            {
                // 1. Xác thực token với Google
                // Validate token với Google server để đảm bảo token hợp lệ
                var clientId = _configuration["Authentication:Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId } // Chỉ chấp nhận token từ app của mình
                };

                // TODO: Nên dùng await thay vì .Result để tránh deadlock
                var payload = GoogleJsonWebSignature.ValidateAsync(idToken, settings).Result;

                if (payload == null)
                {
                    return null;
                }

                // 2. Kiểm tra user đã tồn tại chưa (theo GoogleId)
                var user = _userRepository.GetByGoogleId(payload.Subject);

                if (user == null)
                {
                    // Kiểm tra email đã tồn tại chưa
                    user = _userRepository.GetbyEmail(payload.Email);

                    if (user != null)
                    {
                        // Nếu email đã tồn tại nhưng chưa có GoogleId
                        // => Liên kết tài khoản với Google
                        user.GoogleId = payload.Subject;
                        user.UpdateAt = DateTime.UtcNow;
                        _userRepository.Update(user);
                    }
                    else
                    {
                        // Tạo user mới với role mặc định (Customer)
                        var customerRole = _roleRepository.GetByName("Student");
                        if (customerRole == null)
                        {
                            throw new Exception("Không tìm thấy role Customer");
                        }

                        user = new User
                        {
                            FullName = payload.Name,
                            Email = payload.Email,
                            GoogleId = payload.Subject,
                            PasswordHash = null, // Không cần password khi login bằng Google
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
                    // User đã tồn tại, cập nhật thời gian đăng nhập
                    user.UpdateAt = DateTime.UtcNow;
                    _userRepository.Update(user);
                }

                // 3. Kiểm tra trạng thái tài khoản
                if (user.Status != "Active")
                {
                    return null; // Tài khoản bị khóa
                }

                // 4. Tạo JWT token
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
                 * Q30: Có logging đầy đủ không?
                 * A30: CHƯA đầy đủ.
                 * - Hiện tại: Chỉ dùng Console.WriteLine (không phù hợp production)
                 * - Nên: Dùng ILogger<T> để log structured logging
                 * - Nên log: Exception details, User info (không log sensitive data)
                 * - Production: Dùng Serilog, NLog, hoặc Application Insights
                 */
                // Log error nếu cần
                // TODO: Thay bằng ILogger
                Console.WriteLine($"Google Login Error: {ex.Message}");
                return null;
            }
        }
    }
}