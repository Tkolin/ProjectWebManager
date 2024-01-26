using System;
using System.Collections.Generic;

namespace Test.Models;

public partial class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CompaniesCutomerId { get; set; }

    public int ImplementingCutomerId { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public int Priority { get; set; }

    public int ProjectDirectorId { get; set; }

    public virtual Company? CompaniesCutomer { get; set; } = null!;

    public virtual ICollection<EmployeeToProject>? EmployeeToProjects { get; set; } = new List<EmployeeToProject>();
    public virtual ICollection<DocumentsToProject>? DocumentsToProjects { get; set; } = new List<DocumentsToProject>();

    public virtual Company? ImplementingCutomer { get; set; } = null!;

    public virtual Employee? ProjectDirector { get; set; }
    public virtual ICollection<Task>? ProjectTask { get; set; } = new List<Task>();
}
