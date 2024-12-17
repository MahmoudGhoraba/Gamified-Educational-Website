using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Instructor
{
    public string? ProfilePicture { get; set; }

    public int InstructorId { get; set; }

    public string? Name { get; set; }

    public string? LatestQualification { get; set; }

    public string? ExpertiseArea { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<EmotionalfeedbackReview> EmotionalfeedbackReviews { get; set; } = new List<EmotionalfeedbackReview>();

    public virtual ICollection<InstructorDiscrussion> InstructorDiscrussions { get; set; } = new List<InstructorDiscrussion>();

    public virtual ICollection<PathReview> PathReviews { get; set; } = new List<PathReview>();

    public virtual ICollection<SignupPage> SignupPages { get; set; } = new List<SignupPage>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
