using System;
using System.Collections.Generic;

namespace Test.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Project> ProjectCompaniesCutomers { get; set; } = new List<Project>();

    public virtual ICollection<Project> ProjectImplementingCutomers { get; set; } = new List<Project>();
}
