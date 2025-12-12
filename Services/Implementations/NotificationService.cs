using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Repositories.Interfaces;
using SWP391_BL3.Services.Interfaces;
namespace SWP391_BL3.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public List<NotificationResponse> GetUserNotifications(int userId)
        {
            var notifications = _notificationRepository.GetByUserId(userId);

            return notifications.Select(n => new NotificationResponse
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Status = n.Status,
                Date = n.Date,
                UserId = n.UserId,
                BookingId = n.BookingId,
                BookingCode = n.Booking?.BookingCode,
                UserFullName = n.User?.FullName
            }).ToList();
        }

        public List<NotificationResponse> GetUnreadNotifications(int userId)
        {
            var notifications = _notificationRepository.GetUnreadByUserId(userId);

            return notifications.Select(n => new NotificationResponse
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Status = n.Status,
                Date = n.Date,
                UserId = n.UserId,
                BookingId = n.BookingId,
                BookingCode = n.Booking?.BookingCode,
                UserFullName = n.User?.FullName
            }).ToList();
        }

        public NotificationResponse? GetNotificationById(int id)
        {
            var notification = _notificationRepository.GetById(id);
            if (notification == null) return null;

            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Status = notification.Status,
                Date = notification.Date,
                UserId = notification.UserId,
                BookingId = notification.BookingId,
                BookingCode = notification.Booking?.BookingCode,
                UserFullName = notification.User?.FullName
            };
        }

        public bool MarkAsRead(int notificationId)
        {
            return _notificationRepository.MarkAsRead(notificationId);
        }

        public bool MarkAllAsRead(int userId)
        {
            return _notificationRepository.MarkAllAsRead(userId);
        }

        public int GetUnreadCount(int userId)
        {
            return _notificationRepository.GetUnreadByUserId(userId).Count;
        }

        public bool DeleteNotification(int id)
        {
            return _notificationRepository.Delete(id);
        }
    }
}