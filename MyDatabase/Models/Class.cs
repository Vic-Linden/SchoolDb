using System;
using System.Collections.Generic;

namespace MyDatabase.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int MentorId { get; set; }

    public virtual Staff Mentor { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
