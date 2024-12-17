using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class LearningPath
{
    public int PathId { get; set; }

    public int? LearnerId { get; set; }

    public int? ProfileId { get; set; }

    public string? CompletionStatus { get; set; }

    public string? CustomContent { get; set; }

    public string? AdaptiveRules { get; set; }

    public virtual ICollection<PathReview> PathReviews { get; set; } = new List<PathReview>();

    public virtual PersonalizationProfile? PersonalizationProfile { get; set; }
}
