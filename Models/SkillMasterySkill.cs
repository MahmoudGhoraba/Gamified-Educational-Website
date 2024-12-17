using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class SkillMasterySkill
{
    public int QuestId { get; set; }

    public string Skill { get; set; } = null!;

    public virtual Quest Quest { get; set; } = null!;
}
