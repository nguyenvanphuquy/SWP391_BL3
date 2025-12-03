using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class Campus
{
    public int CampusId { get; set; }

    public string? CampusName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
