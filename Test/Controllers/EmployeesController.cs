using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ProjectDbContext _context;

        public EmployeesController(ProjectDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int? Id, string? sortOrder, string? nameSearch)
        {
            var employees = new List<Employee>();

            if (_context.Employees != null)
            {
                employees = await _context.Employees.ToListAsync();
            }
            else
                return Problem("Entity set 'ProjectDbContext.Task' is null.");

            return View(FilteredTable(employees, sortOrder, nameSearch));


        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Patronymic,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,Patronymic,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'ProjectDbContext.Employees'  is null.");
            }
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
          return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        /// <summary>
        /// Filter and search data
        /// </summary>
        /// <param name="employees"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public List<Employee> FilteredTable(List<Employee> employees, string? sortOrder, string? nameSearch)
        {
            if (!string.IsNullOrEmpty(nameSearch))
            {
                employees = employees.Where(t => t.FirstName.ToLower().Contains(nameSearch.ToLower()) ||
                                                t.LastName.ToLower().Contains(nameSearch.ToLower())).ToList();
            }

            ViewBag.FirstNameParm = String.IsNullOrEmpty(sortOrder) ? "firstName_desc" : "";
            ViewBag.LastNameParm = sortOrder == "lastName_asc" ? "lastName_desc" : "lastName_asc";
            ViewBag.PatronymicParm = sortOrder == "patronymic_asc" ? "patronymic_desc" : "patronymic_asc";
            ViewBag.EmailParm = sortOrder == "email_asc" ? "email_desc" : "email_asc";

            switch (sortOrder)
            {
                case "firstName_desc":
                    employees = employees.OrderByDescending(s => s.FirstName).ToList();
                    break;
                case "lastName_asc":
                    employees = employees.OrderBy(s => s.LastName).ToList();
                    break;
                case "lastName_desc":
                    employees = employees.OrderByDescending(s => s.LastName).ToList();
                    break;
                case "patronymic_asc":
                    employees = employees.OrderBy(s => s.Patronymic).ToList();
                    break;
                case "patronymic_desc":
                    employees = employees.OrderByDescending(s => s.Patronymic).ToList();
                    break;
                case "email_asc":
                    employees = employees.OrderBy(s => s.Email).ToList();
                    break;
                case "email_desc":
                    employees = employees.OrderByDescending(s => s.Email).ToList();
                    break;
                default:
                    employees = employees.OrderBy(s => s.FirstName).ToList();
                    break;
            }


            return employees;
        }
    }
}
