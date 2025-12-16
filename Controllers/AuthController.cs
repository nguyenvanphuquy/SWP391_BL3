using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Services.Interfaces;

namespace SWP391_BL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var response = _userService.Login(request);

            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(response);
        }
        [HttpPost("google-login")]
        public IActionResult GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.IdToken))
            {
                return Unauthorized(new { message = "IdToken không hợp lệ" });
            }

            var result = _userService.GoogleLogin(request.IdToken);

            if (result == null)
            {
                return Unauthorized(new { message = "Đăng nhập thất bại hoặc tài khoản bị khóa" });
            }

            return Ok(result);
        }

    }
}
