using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Collaborative
{
    public int QuestId { get; set; }

    public DateTime Deadline { get; set; }

    public int? MaxNumParticipants { get; set; }

    public virtual ICollection<LearnersCollaboration> LearnersCollaborations { get; set; } = new List<LearnersCollaboration>();

    public virtual Quest Quest { get; set; } = null!;
}
