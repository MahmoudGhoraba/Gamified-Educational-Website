using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class LearningPrefernce
{
    public int LearnerId { get; set; }

    public string LearningPreference { get; set; } = null!;

    public virtual Learner Learner { get; set; } = null!;
}
