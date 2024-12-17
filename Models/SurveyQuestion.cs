using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class SurveyQuestion
{
    public int SurveyId { get; set; }

    public string Question { get; set; } = null!;

    public virtual ICollection<FilledSurvey> FilledSurveys { get; set; } = new List<FilledSurvey>();

    public virtual Survey Survey { get; set; } = null!;
}
