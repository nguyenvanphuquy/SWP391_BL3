using SWP391_BL3.Models.Entities;

namespace SWP391_BL3.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Notification Add(Notification noti);
        Notification? GetById(int id);
        List<Notification> GetByUserId(int userId);
        List<Notification> GetUnreadByUserId(int userId);
        bool MarkAsRead(int notificationId);
        bool MarkAllAsRead(int userId);
        bool Delete(int id);
    }
}
