using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class SignupPage
{
    public string Email { get; set; } = null!;

    public int? LearnerId { get; set; }

    public int? InstructorId { get; set; }

    public int? AdminId { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Instructor? Instructor { get; set; }

    public virtual Learner? Learner { get; set; }
}
