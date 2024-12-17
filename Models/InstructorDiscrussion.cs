using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class InstructorDiscrussion
{
    public int ForumId { get; set; }

    public int InstructorId { get; set; }

    public TimeOnly? Time { get; set; }

    public string Post { get; set; } = null!;

    public virtual DiscussionForum Forum { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;
}
