using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Booking
{
    public int BookingId { get; set; }

    public string? BookingCode { get; set; }

    public DateOnly? BookingDate { get; set; }

    public string? Purpose { get; set; }

    public int? NumberOfMenber { get; set; }

    public string? Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? UserId { get; set; }

    public int? FacilityId { get; set; }

    public int? SlotId { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual Facility? Facility { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Slot? Slot { get; set; }

    public virtual User? User { get; set; }
}
