using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Admin
{
    public string? ProfilePicture { get; set; }

    public int AdminId { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<SignupPage> SignupPages { get; set; } = new List<SignupPage>();
}
