using System;
using System.Collections.Generic;

namespace Spaghetti.Models;

public partial class Survey
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();
}
