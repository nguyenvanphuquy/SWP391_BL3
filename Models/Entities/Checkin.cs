using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Checkin
{
    public int CheckinId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }
}
