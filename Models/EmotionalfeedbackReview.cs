using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class EmotionalfeedbackReview
{
    public int FeedbackId { get; set; }

    public int InstructorId { get; set; }

    public string? Review { get; set; }

    public virtual EmotionalFeedback Feedback { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;
}
