using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Slot
{
    public int SlotId { get; set; }

    public int? SlotNumber { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
