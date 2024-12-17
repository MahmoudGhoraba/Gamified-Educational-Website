using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Leaderboard
{
    public int BoardId { get; set; }

    public int? Season { get; set; }

    public virtual ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();
}
