using System;
using System.Collections.Generic;

namespace MyDatabase.Models;

public partial class Staff
{
    public int PersonId { get; set; }

    public string Role { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? DepartmentId { get; set; }

    public DateOnly? HireDate { get; set; }

    public decimal? MonthlySalary { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
