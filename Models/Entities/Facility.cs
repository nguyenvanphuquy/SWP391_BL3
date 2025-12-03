using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string? FacilityCode { get; set; }

    public int? Capacity { get; set; }

    public int? Floor { get; set; }

    public string? Equipment { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? CampusId { get; set; }

    public int? TypeId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Campus? Campus { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual FacilityType? Type { get; set; }

    public virtual ICollection<Slot> Slots { get; set; } = new List<Slot>();
}
