using System;
using System.Collections.Generic;

namespace Test.Models;

public partial class EmployeeToProject
{
    public EmployeeToProject () {}
    public EmployeeToProject(int id, int projectId, int employeeId)
    {
        Id = id;
        ProjectId = projectId;
        EmployeeId = employeeId;
    }
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
