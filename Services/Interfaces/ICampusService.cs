using SWP391_BL3.Models.DTOs.Response;

namespace SWP391_BL3.Services.Interfaces
{
    public interface ICampusService
    {
        IEnumerable<CampusResponse> GetAll();
    }
}
