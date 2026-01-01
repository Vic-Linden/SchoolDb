using System;
using System.Collections.Generic;

namespace MyDatabase.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public int Points { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
