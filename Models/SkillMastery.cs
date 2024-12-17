using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class SkillMastery
{
    public int QuestId { get; set; }

    public virtual ICollection<LearnersMastery> LearnersMasteries { get; set; } = new List<LearnersMastery>();

    public virtual Quest Quest { get; set; } = null!;
}
