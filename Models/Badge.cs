using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Badge
{
    public int BadgeId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Criteria { get; set; }

    public int? Points { get; set; }

    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
