using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Test.Models;

public partial class Employee : IdentityUser<int>
{
    //public int Id { get; set; }

    public string? FirstName { get; set; } = null!;

    public string? LastName { get; set; } = null!;

    public string? Patronymic { get; set; }

    //public string Email { get; set; } = null!;

    public virtual ICollection<EmployeeToProject> EmployeeToProjects { get; set; } = new List<EmployeeToProject>(); 

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<Task> TasksAuthor { get; set; } = new List<Task>();
    public virtual ICollection<Task> TasksExecutor { get; set; } = new List<Task>();
}
