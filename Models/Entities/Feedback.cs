using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? UserId { get; set; }

    public int? FacilityId { get; set; }

    public virtual Facility? Facility { get; set; }

    public virtual User? User { get; set; }
}
