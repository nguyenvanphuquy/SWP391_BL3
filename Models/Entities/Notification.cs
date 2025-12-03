using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime? Date { get; set; }

    public int? UserId { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User? User { get; set; }
}
