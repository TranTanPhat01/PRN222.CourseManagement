using System;
using System.Collections.Generic;

namespace CourseManagement.Models;

public partial class Enrollment
{
    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    public decimal? Grade { get; set; }

    public bool IsGradeFinalized { get; set; } = false;

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
