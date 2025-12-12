using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly FptBookingContext _context;

        public NotificationRepository(FptBookingContext context)
        {
            _context = context;
        }

        public Notification Add(Notification noti)
        {
            _context.Notifications.Add(noti);
            _context.SaveChanges();
            return noti;
        }
     public Notification? GetById(int id)
        {
            return _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Booking)
                    .ThenInclude(b => b.Facility)
                .FirstOrDefault(n => n.NotificationId == id);
        }

        public List<Notification> GetByUserId(int userId)
        {
            return _context.Notifications
                .Include(n => n.Booking)
                    .ThenInclude(b => b.Facility)
                .Include(n => n.User)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Date)
                .ToList();
        }

        public List<Notification> GetUnreadByUserId(int userId)
        {
            return _context.Notifications
                .Include(n => n.Booking)
                    .ThenInclude(b => b.Facility)
                .Include(n => n.User)
                .Where(n => n.UserId == userId && n.Status == "Unread")
                .OrderByDescending(n => n.Date)
                .ToList();
        }

        public bool MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification == null) return false;

            notification.Status = "Read";
            _context.SaveChanges();
            return true;
        }

        public bool MarkAllAsRead(int userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId && n.Status == "Unread")
                .ToList();

            if (!notifications.Any()) return false;

            foreach (var notification in notifications)
            {
                notification.Status = "Read";
            }

            _context.SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            _context.SaveChanges();
            return true;
        }
    }
}