using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Notification
{
    public int Id { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Message { get; set; }

    public string? UrgencyLevel { get; set; }

    public bool? ReadStatus { get; set; }

    public virtual ICollection<Learner> Learners { get; set; } = new List<Learner>();
}
