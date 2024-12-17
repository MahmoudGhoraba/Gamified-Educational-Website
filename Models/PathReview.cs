using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class PathReview
{
    public int InstructorId { get; set; }

    public int PathId { get; set; }

    public string? Review { get; set; }

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual LearningPath Path { get; set; } = null!;
}
