using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class PersonalizationProfile
{
    public int LearnerId { get; set; }

    public int ProfileId { get; set; }

    public string? PreferedContentType { get; set; }

    public string? EmotionalState { get; set; }

    public string? PersonalityType { get; set; }

    public virtual ICollection<HealthCondition> HealthConditions { get; set; } = new List<HealthCondition>();

    public virtual Learner Learner { get; set; } = null!;

    public virtual ICollection<LearningPath> LearningPaths { get; set; } = new List<LearningPath>();
}
