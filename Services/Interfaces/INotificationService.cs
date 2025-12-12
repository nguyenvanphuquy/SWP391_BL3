using SWP391_BL3.Models.DTOs.Response;

namespace SWP391_BL3.Services.Interfaces
{
    public interface INotificationService
    {
        List<NotificationResponse> GetUserNotifications(int userId);
        List<NotificationResponse> GetUnreadNotifications(int userId);
        NotificationResponse? GetNotificationById(int id);
        bool MarkAsRead(int notificationId);
        bool MarkAllAsRead(int userId);
        int GetUnreadCount(int userId);
        bool DeleteNotification(int id);
    }
}
