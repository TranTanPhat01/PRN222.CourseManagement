using System;
using System.Collections.Generic;

namespace CourseManagement.Models;

public enum CourseStatus
{
    Active = 0,
    Inactive = 1,
    Archived = 2
}

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Credits { get; set; }

    public int DepartmentId { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Active;

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
