using System;
using System.Collections.Generic;

namespace MyDatabase.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string Personnumber { get; set; } = null!;

    public int ClassId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
