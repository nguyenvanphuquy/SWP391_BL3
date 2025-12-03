using System;
using System.Collections.Generic;

namespace SWP391_BL3.Models.Entities;

public partial class FacilityType
{
    public int TypeId { get; set; }

    public string? TypeName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
