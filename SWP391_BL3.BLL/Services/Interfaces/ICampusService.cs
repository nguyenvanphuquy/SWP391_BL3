using SWP391_BL3.DAL.Models.DTOs.Response;

namespace SWP391_BL3.BLL.Services.Interfaces
{
    public interface ICampusService
    {
        IEnumerable<CampusResponse> GetAll();
    }
}
