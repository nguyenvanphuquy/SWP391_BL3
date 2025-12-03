using SWP391_BL3.Models.DTOs.Request;
using SWP391_BL3.Models.DTOs.Response;
namespace SWP391_BL3.Services.Interfaces
{
    public interface IUserService
    {
        LoginResponse? Login(LoginRequest request);
    }
}
