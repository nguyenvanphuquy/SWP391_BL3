
using SWP391_BL3.DAL.Models.DTOs.Request;
using SWP391_BL3.DAL.Models.DTOs.Response;
namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface IUserService
    {
        LoginResponse? Login(LoginRequest request);
        UserResponse? GetById(int id);
        IEnumerable<UserResponse> GetAll();
        IEnumerable<UserInforResponse> GetAllInfor();
        UserResponse Create(UserRequest request);
        bool Delete(int id);
        LoginResponse? GoogleLogin(string idToken);
    }
}
