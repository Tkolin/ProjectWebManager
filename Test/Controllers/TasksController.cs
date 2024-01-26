using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Controllers
{
    public class TasksController : Controller
    {
        private readonly ProjectDbContext _context;
        private UserManager<Employee> _userManager;

        public TasksController(ProjectDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tasks
        public async Task<IActionResult> Index(int? projectId, string nameSearch, int? typeSearch, string sortOrder)
        {
            List<Models.Task> tasks;

            // Check if Task entity set is not null
            if (_context.Task != null)
            {
                tasks = await _context.Task
                    .Include(t => t.Author)
                    .Include(t => t.Executor)
                    .Include(t => t.Project)
                        .ThenInclude(p => p.ProjectDirector)
                    .Include(t => t.StatusTask)
                    .ToListAsync();

                TempData.Remove("ProjectIdForCreate");

                if (projectId != null && projectId != 0)
                {
                    tasks = tasks.Where(t => t.ProjectId == projectId).ToList();
                    TempData["ProjectIdForCreate"] = projectId;
                    ViewBag.ProjectId = projectId;
                    TempData.Keep("ProjectIdForCreate");
                    ViewBag.ProjectNameLink = _context.Projects.Where(p => p.Id == projectId).FirstOrDefault()?.Name;
                    
                }

                var user = await _userManager.GetUserAsync(User);
                if (user.Id != 0)
                {
                    TempData["userId"] = user.Id;
                    bool isEmployee = await _userManager.IsInRoleAsync(user, "employee");
                    bool isManager = await _userManager.IsInRoleAsync(user, "projectmanager");
                    TempData["isEmployee"] = isEmployee;
                    TempData["isManager"] = isManager;

                    if (isEmployee)
                        tasks = tasks.Where(p => p.ExecutorId == user.Id).ToList();
                    else if (isManager)
                        tasks = tasks.Where(t => t.Project.ProjectDirectorId == user.Id).ToList();

                    var emp = _context.Employees.Where(p => p.Id == user.Id).FirstOrDefault();
                    ViewBag.EmployeeNameLink = emp?.LastName + " " + emp?.FirstName;
                }
            }
            else
            {
                return Problem("Entity set 'ProjectDbContext.Task' is null.");
            }

            ViewBag.TaskTypes = new SelectList(_context.StatusTask.ToList(), "Id", "Name");

            return View(FilteredTable(tasks, sortOrder, nameSearch, typeSearch));
        }

        // GET: Tasks/Create
        public async Task<IActionResult> Create(int? id)
        {        
            var user = await _userManager.GetUserAsync(User);
            ViewBag.userId = user.Id;
            ViewBag.isManager = await _userManager.IsInRoleAsync(user, "projectmanager");
            ViewBag.isSupervisor = await _userManager.IsInRoleAsync(user, "supervisor");
            ViewBag.isEmployee = await _userManager.IsInRoleAsync(user, "employee");


            FillViewDataSelectList();
            var task = new Models.Task();
            if (TempData["userId"] != null && !(bool)TempData["isEmployee"])
            {
                task.AuthorId = (int)TempData["userId"];
                TempData.Keep("userId");
                TempData.Keep("isEmployee");
            }
            if (id != null && id != 0)
            {
                task.ProjectId = (int)id;
                ViewData["ProjectName"] = _context.Projects.Where(p => p.Id == (int)id).Select(p => p.Name).FirstOrDefault();
            }
            return View(task);
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,AuthorId,ExecutorId,Description,Priority,StatusId")] Models.Task task)
        {
            task.Id = 0;
            _context.Add(task);
            _context.SaveChanges();
            return BackSupported();
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Task == null)
            {
                return NotFound();
            }

            var task = await _context.Task.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            FillViewDataSelectList(task);

            if (TempData["ProjectIdForCreate"] != null)
            {
                task.ProjectId = (int)TempData["ProjectIdForCreate"];
                TempData.Keep("ProjectIdForCreate");
            }

            if (TempData["userId"] != null && !(bool)TempData["isEmployee"])
            {
                task.AuthorId = (int)TempData["userId"];
                TempData.Keep("userId");
            }

            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,AuthorId,ExecutorId,Description,Priority,StatusId")] Models.Task task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            try
            {
                if (TempData["userId"] != null && !(bool)TempData["isEmployee"])
                {
                    task.AuthorId = (int)TempData["userId"];
                }

                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(task.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            FillViewDataSelectList(task);
            return BackSupported();
        }

        // POST: Tasks/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Task == null)
            {
                return Problem("Entity set 'ProjectDbContext.Task' is null.");
            }

            var task = await _context.Task.FindAsync(id);
            if (task != null)
            {
                _context.Task.Remove(task);
            }

            await _context.SaveChangesAsync();
            return BackSupported();
        }

        private bool TaskExists(int id)
        {
            return (_context.Task?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Fill SelectList for Views
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
            ViewData["AuthorId"] = new SelectList(employeeListView, "Id", "FullName");
            ViewData["ExecutorId"] = new SelectList(employeeListView, "Id", "FullName");

            var projectListView = _context.Projects.ToList();
            if (TempData["userId"] != null && (int)TempData["userId"] != 0)
            {
                var user = _userManager.GetUserAsync(User);
                projectListView =              projectListView.Where(p => p.ProjectDirectorId == (int)TempData["userId"]).ToList();
                ViewData["AuthorId"] = new SelectList(employeeListView, "Id", "FullName", user.Id);
            }
            ViewData["ProjectId"] = new SelectList(projectListView, "Id", "Name");

            ViewData["StatusId"] = new SelectList(_context.StatusTask, "Id", "Name");
        }

        /// <summary>
        /// Fill SelectList for Views with task-specific data
        /// </summary>
        /// <param name="task"></param>
        public async void FillViewDataSelectList(Models.Task task)
        {
            var employeeListView =
                _context.Employees
                    .Select(s => new
                    {
                        Id = s.Id,
                        FullName = $"{s.LastName} {s.FirstName}"
                    })
                    .ToList();

            ViewData["AuthorId"] = new SelectList(employeeListView, "Id", "FullName", task.AuthorId);
            ViewData["ExecutorId"] = new SelectList(employeeListView, "Id", "FullName", task.ExecutorId);

            var projectListView = _context.Projects.ToList();
            if (TempData["userId"] != null && (int)TempData["userId"] != 0)
            {
                var user = await _userManager.GetUserAsync(User);
                projectListView =
                    projectListView.Where(p => p.ProjectDirectorId == (int)TempData["userId"]).ToList();
                ViewData["AuthorId"] = new SelectList(employeeListView, "Id", "FullName", user.Id);
            }
            ViewData["ProjectId"] = new SelectList(projectListView, "Id", "Name", task.ProjectId);

            ViewData["StatusId"] = new SelectList(_context.StatusTask, "Id", "Name", task.StatusId);
        }

        /// <summary>
        /// Filter and search data
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public List<Models.Task> FilteredTable(List<Models.Task> tasks, string sortOrder, string nameSearch, int? typeSearch)
        {
            if (!string.IsNullOrEmpty(nameSearch))
            {
                tasks = tasks.Where(t => t.Project.Name.ToLower().Contains(nameSearch.ToLower())).ToList();
            }
            if (typeSearch.HasValue)
            {
                tasks = tasks.Where(t => t.StatusId == typeSearch).ToList();
            }

            ViewBag.DescriptionSortParm = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";
            ViewBag.PrioritySortParm = sortOrder == "priority_asc" ? "priority_desc" : "priority_asc";
            ViewBag.ProjectSortParm = sortOrder == "project_asc" ? "project_desc" : "project_asc";
            ViewBag.ExecutorSortParm = sortOrder == "executor_asc" ? "executor_desc" : "executor_asc";
            ViewBag.AuthorSortParm = sortOrder == "author_asc" ? "author_desc" : "author_asc";
            ViewBag.StatusTaskSortParm = sortOrder == "statusTask_asc" ? "statusTask_desc" : "statusTask_asc";

            switch (sortOrder)
            {
                case "description_desc":
                    tasks = tasks.OrderByDescending(s => s.Description).ToList();
                    break;
                case "priority_asc":
                    tasks = tasks.OrderBy(s => s.Priority).ToList();
                    break;
                case "priority_desc":
                    tasks = tasks.OrderByDescending(s => s.Priority).ToList();
                    break;
                case "project_asc":
                    tasks = tasks.OrderBy(s => s.Project.Name).ToList();
                    break;
                case "project_desc":
                    tasks = tasks.OrderByDescending(s => s.Project.Name).ToList();
                    break;
                case "executor_asc":
                    tasks = tasks.OrderBy(s => s.Executor.LastName + s.Executor.FirstName).ToList();
                    break;
                case "executor_desc":
                    tasks = tasks.OrderByDescending(s => s.Executor.LastName + s.Executor.FirstName).ToList();
                    break;
                case "author_asc":
                    tasks = tasks.OrderBy(s => s.Author.LastName + s.Author.FirstName).ToList();
                    break;
                case "author_desc":
                    tasks = tasks.OrderByDescending(s => s.Author.LastName + s.Author.FirstName).ToList();
                    break;
                case "statusTask_asc":
                    tasks = tasks.OrderBy(s => s.StatusTask.Name).ToList();
                    break;
                case "statusTask_desc":
                    tasks = tasks.OrderByDescending(s => s.StatusTask.Name).ToList();
                    break;
                default:
                    tasks = tasks.OrderBy(s => s.Description).ToList();
                    break;
            }

            return tasks;
        }

        public IActionResult BackSupported()
        {
            if (TempData["ProjectIdForCreate"] != null)
            {
                int id = (int)TempData["ProjectIdForCreate"];
                TempData.Remove("ProjectIdForCreate");
                return RedirectToAction("Index", new { projectId = id });
            }

            if (TempData["userId"] != null)
                TempData.Remove("ProjectIdForCreate");

            return RedirectToAction(nameof(Index));
        }
    }
}
