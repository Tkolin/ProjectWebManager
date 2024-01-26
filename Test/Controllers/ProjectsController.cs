using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Test.Models;
using Test.Services;
using NuGet.Protocol;
using Newtonsoft.Json;
using Microsoft.Build.Framework;


namespace Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectsController : Controller
    {
        private readonly ProjectDbContext _context;
        private readonly ProjectService _projectService;
        private UserManager<Employee> _userManager;
        public ProjectsController(ProjectDbContext context, ProjectService projectService, UserManager<Employee> UserManager)
        {
            _context = context;
            _projectService = projectService;
            _userManager = UserManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(DateTime? dateStartSearch, DateTime? dateEndSearch,string? nameSearch, string? sortOrder)
        {
            List<Project> projects = new List<Project>();
            if (_context.Projects != null)
            {
                projects = await _context.Projects
                .Include(p => p.CompaniesCutomer)
                .Include(p => p.ImplementingCutomer)
                .Include(p => p.ProjectDirector)
                .Include(p => p.ProjectTask)
                     .ThenInclude(pt => pt.StatusTask)
                .Include(p => p.EmployeeToProjects)
                    .ThenInclude(etp => etp.Employee)
                 .Include(p => p.DocumentsToProjects)
                    .ThenInclude(dtp => dtp.Document)
                .ToListAsync();
            }
            else
                return Problem("Entity set 'ProjectDbContext.Projects' is null.");
            var user = await _userManager.GetUserAsync(User);

            if (user.Id != 0)
            {
                bool isEmployee = await _userManager.IsInRoleAsync(user, "employee");
                bool isManager = await _userManager.IsInRoleAsync(user, "projectmanager");
                TempData["isEmployee"] = isEmployee;
                TempData["isManager"] = isManager;

                ViewData["UserId"] = user.Id;

                if(isManager)
                    projects = projects.Where(p=>p.ProjectDirectorId == user.Id).ToList();
                else if(isEmployee)
                    projects = projects.Where(p => p.EmployeeToProjects.Any(etp => etp.EmployeeId == user.Id)).ToList();
            }

            return View(FilteredTable(projects,dateStartSearch,dateEndSearch,sortOrder, nameSearch));
        }
        [HttpGet]
        [Route("Details")]
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.CompaniesCutomer)
                .Include(p => p.ImplementingCutomer)
                .Include(p => p.ProjectDirector)
                .Include(p => p.ProjectTask)
                     .ThenInclude(pt => pt.StatusTask)
                .Include(p => p.EmployeeToProjects)
                    .ThenInclude(etp => etp.Employee)
                 .Include(p=>p.DocumentsToProjects)
                    .ThenInclude(dtp=>dtp.Document)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            FillViewDataSelectList();
            return View();
        }
        [HttpPost]
        [Route("UploadFiles")]
        public async Task<IActionResult> UploadFiles()
        {
            // TODO: Add file validation logic if needed
            return Ok("Project and files uploaded successfully.");
        }

        [HttpPost]
        [Route("SaveProject")]
        public async Task<IActionResult> SaveProject([FromForm] string projectData, List<IFormFile> files)//
        {
            try
            {
                Project project = JsonConvert.DeserializeObject<Project>(projectData);
                List<Document> documents = new List<Document>();
                foreach (var file in files)
                {
                    documents.Add(new Document
                    {
                        Name = file.FileName,
                        Data = ReadFully(file.OpenReadStream())
                    });
                }

                _projectService.SaveDocument(documents);
                _projectService.SaveProject(project);
                _projectService.BindingDocumentToProject(project.Id, documents.Select(s => s.Id).ToList());
                return Ok("Project and files uploaded successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error uploading project and files: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("SearchCompanies")]
        public IActionResult SearchCompanies(string query)
        {
            var companies = _projectService.GetCompanies();
            companies = companies.Where(c => c.Name.ToLower().Contains(query.ToLower())).ToList();
            return Json(companies);
        }

        [HttpGet]
        [Route("SearchEmployees")]
        public IActionResult SearchEmployees(string query)
        {
            var employees = _projectService.GetEmployees();
            var filteredEmployees = employees
                    .Where(c => c.FirstName.ToLower().Contains(query.ToLower()) || c.LastName.ToLower().Contains(query.ToLower()))
                    .Select(c => new { Id = c.Id, FullName = $"{c.FirstName} {c.LastName}" })
                    .ToList();

            return Json(filteredEmployees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CompaniesCutomerId,ImplementingCutomerId,DateStart,DateEnd,Priority,ProjectDirectorId")] Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            FillViewDataSelectList(project);
            return View(project);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int? id, [FromForm] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            FillViewDataSelectList(project);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.CompaniesCutomer)
                .Include(p => p.ImplementingCutomer)
                .Include(p => p.ProjectDirector)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        [Route("DetailsEmployeeToProjects")]
        public async Task<IActionResult> DetailsEmployeeToProjects(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            var selectedProject = _context.Projects.Where(p=>p.Id == id).FirstOrDefault();
            
            if(selectedProject == null) { return NotFound(); }

            var projectDbContext = _context.EmployeeToProjects.Include(e => e.Employee).Where(etp => etp.ProjectId == id);
            var employeesToProject = projectDbContext.Select(e => e.Employee);
            
            ViewBag.ProjectName = selectedProject.Name;
                             
            var availableEmployees = _context.Employees.Except(employeesToProject).ToList();
            ViewData["AvailableEmployees"] = new SelectList(availableEmployees, "Id", "LastName", employeesToProject);

            TempData.Clear();
            TempData.Add("OpenProjectId", id);

            return View(await employeesToProject.ToListAsync());
        }

        [HttpPost]
        [Route("AddEmployeeToProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployeeToProject([FromForm] int selectedEmploy)
        {        
            if(selectedEmploy == 0)
                return BadRequest("Выберите сотрудника"); 
            int selectedProjectId = Convert.ToInt32(TempData["OpenProjectId"]);
            _context.EmployeeToProjects.Add(new EmployeeToProject(0, selectedProjectId, selectedEmploy));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DetailsEmployeeToProjects), new { id = selectedProjectId } );
        }

        [HttpPost("DeleteEmployeeToProject")]
        [Route("DeleteEmployeeToProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployeeToProject(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            int selectedProjectId = Convert.ToInt32(TempData["OpenProjectId"]);
            var employeeToProject = await _context.EmployeeToProjects.Where(etp=> 
                                                            etp.EmployeeId == id &&
                                                            etp.ProjectId == selectedProjectId).FirstAsync();
            if (employeeToProject != null)
            {
                _context.EmployeeToProjects.Remove(employeeToProject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DetailsEmployeeToProjects), new { id = selectedProjectId });
        }


        [HttpGet]
        [Route("DetailsTasksToProjects")]
        public async Task<IActionResult> DetailsTasksToProjects(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            var selectedProject = _context.Projects.Where(p => p.Id == id).FirstOrDefault();

            if (selectedProject == null) { return NotFound(); }


            var tasks = _context.Task.Where(p => p.ProjectId == id);

            ViewBag.ProjectName = selectedProject.Name;

            TempData.Clear();
            TempData.Add("OpenProjectId", id);

            return View(await tasks.ToListAsync());
        }

        [HttpPost, ActionName("AddTasksToProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTasksToProject([Bind("Id,Name,CompaniesCutomerId,ImplementingCutomerId,DateStart,DateEnd,Priority,ProjectDirectorId")] Models.Task task)
        {
            int selectedProjectId = Convert.ToInt32(TempData["OpenProjectId"]);
            _context.Task.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DetailsEmployeeToProjects), new { id = selectedProjectId });
        }

        [HttpPost, ActionName("DeleteTasksToProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTasksToProject(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int selectedProjectId = Convert.ToInt32(TempData["OpenProjectId"]);
            var task = await _context.Task.Where(t => t.Id == id).FirstAsync();
            if (task != null)
            {
                _context.Task.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DetailsEmployeeToProjects), new { id = selectedProjectId });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ProjectDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }

            await _context.SaveChangesAsync();
            return View();
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        /// <summary>
        /// Filter and search data
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="dateStartSearch"></param>
        /// <param name="dateEndSearch"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public List<Project> FilteredTable(List<Project> projects, DateTime? dateStartSearch, DateTime? dateEndSearch, string? sortOrder,string? nameSearch)
        {
           

            DateTime MinDate = new DateTime(2000, 1, 1);
            if (dateStartSearch >= MinDate)
            {
                projects = projects.Where(s => s.DateStart >= dateStartSearch).ToList();
            }
            if (dateEndSearch >= MinDate)
            {
                projects = projects.Where(s => s.DateStart <= dateEndSearch).ToList();
            }
            if (!string.IsNullOrEmpty(nameSearch))
            {
                projects = projects.Where(s => s.Name.ToLower().Contains(nameSearch.ToLower())).ToList();
            }

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateStartSortParm = sortOrder == "dateStart_desc" ? "dateStart_asc" : "dateStart_desc";
            ViewBag.DateEndSortParm = sortOrder == "dateEnd_desc" ? "dateEnd_asc" : "dateEnd_desc";
            ViewBag.PrioritySortParm = sortOrder == "priority_desc" ? "priority_asc" : "priority_desc";
            ViewBag.CompaniesCutomerSortParm = sortOrder == "companiesCutomer_desc" ? "companiesCutomer_asc" : "companiesCutomer_desc";
            ViewBag.ImplementingCutomerSortParm = sortOrder == "implementingCutomer_desc" ? "implementingCutomer_asc" : "implementingCutomer_desc";
            ViewBag.ProjectDirectorSortParm = sortOrder == "projectDirector_desc" ? "projectDirector_asc" : "projectDirector_desc";

            projects = sortOrder switch
            {
                "name_desc" => projects.OrderByDescending(s => s.Name).ToList(),
                "dateStart_desc" => projects.OrderByDescending(s => s.DateStart).ToList(),
                "dateStart_asc" => projects.OrderBy(s => s.DateStart).ToList(),
                "dateEnd_desc" => projects.OrderByDescending(s => s.DateEnd).ToList(),
                "dateEnd_asc" => projects.OrderBy(s => s.DateEnd).ToList(),
                "priority_desc" => projects.OrderByDescending(s => s.Priority).ToList(),
                "priority_asc" => projects.OrderBy(s => s.Priority).ToList(),
                "companiesCutomer_desc" => projects.OrderByDescending(s => s.CompaniesCutomer).ToList(),
                "companiesCutomer_asc" => projects.OrderBy(s => s.CompaniesCutomer).ToList(),
                "implementingCutomer_desc" => projects.OrderByDescending(s => s.ImplementingCutomer).ToList(),
                "implementingCutomer_asc" => projects.OrderBy(s => s.ImplementingCutomer).ToList(),
                "projectDirector_desc" => projects.OrderByDescending(s => s.ProjectDirector).ToList(),
                "projectDirector_asc" => projects.OrderBy(s => s.ProjectDirector).ToList(),
                _ => projects.OrderBy(s => s.Name).ToList()
            };


            return projects;
        }
        /// <summary>
        /// Fill SelectList for Views with task-specific data
        /// </summary>
        public void FillViewDataSelectList()
        {
            var employeeListView =
                  _context.Employees
                    .Select(s => new
                    {
                        Id = s.Id,
                        FullName = $"{s.LastName} {s.FirstName}"
                    })
                    .ToList();

            ViewData["CompaniesCutomerId"] = new SelectList(_context.Companies, "Id", "Name");
            ViewData["ImplementingCutomerId"] = new SelectList(_context.Companies, "Id", "Name");
            ViewData["ProjectDirectorId"] = new SelectList(employeeListView, "Id", "FullName");
        }
        /// <summary>
        /// Fill SelectList for Views with task-specific data
        /// </summary>
        /// <param name="project"></param>
        public void FillViewDataSelectList(Project project)
        {
            var employeeListView =
                  _context.Employees
                    .Select(s => new
                    {
                        Id = s.Id,
                        FullName = $"{s.LastName} {s.FirstName}"
                    })
                    .ToList();

            ViewData["CompaniesCutomerId"] = new SelectList(_context.Companies, "Id", "Name", project.CompaniesCutomerId);
            ViewData["ImplementingCutomerId"] = new SelectList(_context.Companies, "Id", "Name", project.ImplementingCutomerId);
            ViewData["ProjectDirectorId"] = new SelectList(employeeListView, "Id", "FullName", project.ProjectDirectorId);
        }
        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
